using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using Photon.Pun;
using Photon.Voice;
using Photon.Voice.PUN; 
using Photon.Voice.Unity;
using Vosk;
using VoiceCurse.Core;
using VoiceCurse.Audio;
using VoiceCurse.External;
using VoiceCurse.Interfaces;
using EventHandler = VoiceCurse.Core.EventHandler;

namespace VoiceCurse;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin {
    private static ManualLogSource Log { get; set; } = null!;
    
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool SetDllDirectory(string lpPathName);

    private Config? _config;
    private IVoiceRecognizer? _recognizer;
    private EventHandler? _eventHandler;
    private EventNetworker? _networker;
    private Model? _voskModel;
    
    private int _currentSampleRate;
    
    private VoiceCurseHook? _activeHook;
        
    private readonly ConcurrentQueue<Action> _mainThreadActions = new();
    private volatile string _lastPartialText = "";

    private void Awake() {
        Log = Logger;
        Log.LogInfo($"Plugin {Name} is loading...");
        
        string? pluginDir = Path.GetDirectoryName(Info.Location);
        if (Directory.Exists(pluginDir)) {
            SetDllDirectory(pluginDir);
        }

        _config = new Config(Config);
        if (_config != null) {
            _eventHandler = new EventHandler(_config);
        }

        _networker = new EventNetworker();
        PhotonNetwork.AddCallbackTarget(_networker);

        string modelPath = Path.Combine(Paths.PluginPath, "VoiceCurse", "model-en-us");
        if (Directory.Exists(modelPath)) {
            try {
                _voskModel = new Model(modelPath);
                Log.LogInfo("Vosk Model loaded successfully.");
            } catch (Exception e) {
                Log.LogError($"Failed to load Vosk Model: {e.Message}");
            }
        } else {
            Log.LogError($"Vosk model not found at: {modelPath}");
        }

        if (_voskModel != null) {
            SetupVoiceRecognition(AudioSettings.outputSampleRate);
        }
    }

    private void Update() {
        while (_mainThreadActions.TryDequeue(out Action action)) {
            action.Invoke();
        }

        if (_activeHook == null && Character.localCharacter != null) {
            TryHookIntoPhotonVoice();
        }
    }

    private void TryHookIntoPhotonVoice() {
        PhotonVoiceView voiceView = Character.localCharacter.GetComponent<PhotonVoiceView>();
        
        if (voiceView == null || voiceView.RecorderInUse == null) return;
        Recorder recorder = voiceView.RecorderInUse;

        VoiceCurseHook hook = recorder.gameObject.AddComponent<VoiceCurseHook>();
        hook.Initialize(this, recorder);
            
        _activeHook = hook;
        Log.LogInfo($"[VoiceCurse] Hooked into Recorder on: {recorder.gameObject.name}");
    }

    public void OnPhotonVoiceReady(Recorder recorder, LocalVoice voice) {
        int photonRate = 48000; 
        if (voice.Info.SamplingRate > 0) {
            photonRate = voice.Info.SamplingRate;
        }

        Log.LogInfo($"[VoiceCurse] Photon Voice Ready. Sampling Rate: {photonRate} Hz");

        SetupVoiceRecognition(photonRate);

        if (voice is LocalVoiceAudio<float> floatVoice) {
            if (_recognizer != null) floatVoice.AddPostProcessor(new VoiceProcessor(_recognizer));
            Log.LogInfo("[VoiceCurse] Audio Processor Injected successfully!");
        }
        else {
            Log.LogWarning($"[VoiceCurse] LocalVoice type mismatch: {voice.GetType().Name}");
        }
    }

    private void SetupVoiceRecognition(int sampleRate) {
        if (_voskModel == null) return;
        
        if (_recognizer != null && _currentSampleRate == sampleRate) return;

        try {
            if (_recognizer != null) {
                _recognizer.Stop();
                _recognizer.Dispose();
            }

            _currentSampleRate = sampleRate;
            _recognizer = new VoiceRecognizer(_voskModel, sampleRate);
            
            _recognizer.OnPhraseRecognized += (text) => {
                _lastPartialText = "";
                _mainThreadActions.Enqueue(() => {
                    Log.LogInfo($"[Recognized]: {text}");
                    _eventHandler?.HandleSpeech(text, true);
                });
            };
            
            _recognizer.OnPartialResult += (text) => {
                if (string.IsNullOrWhiteSpace(text) || text == _lastPartialText || text.Length < 2) return;
                _lastPartialText = text;
                string captured = text;
                _mainThreadActions.Enqueue(() => {
                    Log.LogInfo($"[Partial]: {captured}");
                    _eventHandler?.HandleSpeech(captured, false);
                });
            };

            _recognizer.Start();
        } catch (Exception ex) {
            Log.LogError($"Failed to start Voice Recognizer: {ex.Message}");
        }
    }

    private void OnDestroy() {
        if (_networker != null) PhotonNetwork.RemoveCallbackTarget(_networker);
        _recognizer?.Stop();
        _recognizer?.Dispose();
        _voskModel?.Dispose();
    }
}