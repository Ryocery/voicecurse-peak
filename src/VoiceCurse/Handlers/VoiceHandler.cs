using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using BepInEx;
using BepInEx.Logging;
using Photon.Pun;
using Photon.Voice;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using UnityEngine;
using VoiceCurse.Interfaces;
using VoiceCurse.Voice;
using Vosk;

namespace VoiceCurse.Handlers;

public class VoiceHandler : IDisposable {
    private readonly ManualLogSource _log;
    private readonly Config _config;

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool SetDllDirectory(string lpPathName);

    private IVoiceRecognizer? _recognizer;
    private EventHandler? _eventHandler;
    private NetworkHandler? _networker;
    private Model? _voskModel;
    
    private int _currentSampleRate;
    
    private VoiceHook? _activeHook;
        
    private readonly ConcurrentQueue<Action> _mainThreadActions = new();
    private volatile string _lastPartialText = "";

    public VoiceHandler(ManualLogSource logger, Config config, string pluginDir) {
        _log = logger;
        _config = config;

        Initialize(pluginDir);
    }

    private void Initialize(string pluginDir) {
        _log.LogInfo("Initializing VoiceCurse Manager...");
        
        if (Directory.Exists(pluginDir)) {
            SetDllDirectory(pluginDir);
        }

        _eventHandler = new EventHandler(_config);
        _networker = new NetworkHandler();
        PhotonNetwork.AddCallbackTarget(_networker);
        
        string modelPath = Path.Combine(Paths.PluginPath, "VoiceCurse", "model-en-us");
        if (Directory.Exists(modelPath)) {
            try {
                _voskModel = new Model(modelPath);
                _log.LogInfo("Vosk Model loaded successfully.");
            } catch (Exception e) {
                _log.LogError($"Failed to load Vosk Model: {e.Message}");
            }
        } else {
            _log.LogError($"Vosk model not found at: {modelPath}");
        }

        if (_voskModel != null) {
            SetupVoiceRecognition(AudioSettings.outputSampleRate);
        }
    }

    public void Update() {
        while (_mainThreadActions.TryDequeue(out Action action)) {
            action.Invoke();
        }

        if (!_activeHook && Character.localCharacter) {
            TryHookIntoPhotonVoice();
        }
    }

    private void TryHookIntoPhotonVoice() {
        PhotonVoiceView voiceView = Character.localCharacter.GetComponent<PhotonVoiceView>();
        
        if (!voiceView || !voiceView.RecorderInUse) return;
        Recorder recorder = voiceView.RecorderInUse;

        VoiceHook hook = recorder.gameObject.AddComponent<VoiceHook>();
        hook.Initialize(this, recorder);
            
        _activeHook = hook;
        _log.LogInfo($"[VoiceCurse] Hooked into Recorder on: {recorder.gameObject.name}");
    }

    public void OnPhotonVoiceReady(Recorder recorder, LocalVoice voice) {
        int photonRate = 48000; 
        if (voice.Info.SamplingRate > 0) {
            photonRate = voice.Info.SamplingRate;
        }

        _log.LogInfo($"[VoiceCurse] Photon Voice Ready. Sampling Rate: {photonRate} Hz");

        SetupVoiceRecognition(photonRate);

        if (voice is LocalVoiceAudio<float> floatVoice) {
            if (_recognizer != null) floatVoice.AddPostProcessor(new VoiceProcessor(_recognizer));
            _log.LogInfo("[VoiceCurse] Audio Processor Injected successfully!");
        }
        else {
            _log.LogWarning($"[VoiceCurse] LocalVoice type mismatch: {voice.GetType().Name}");
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
                    _log.LogInfo($"[Recognized]: {text}");
                    _eventHandler?.HandleSpeech(text, true);
                });
            };
            
            _recognizer.OnPartialResult += (text) => {
                if (string.IsNullOrWhiteSpace(text) || text == _lastPartialText || text.Length < 2) return;
                _lastPartialText = text;
                string captured = text;
                _mainThreadActions.Enqueue(() => {
                    _log.LogInfo($"[Partial]: {captured}");
                    _eventHandler?.HandleSpeech(captured, false);
                });
            };

            _recognizer.Start();
        } catch (Exception ex) {
            _log.LogError($"Failed to start Voice Recognizer: {ex.Message}");
        }
    }

    public void Dispose() {
        if (_networker != null) PhotonNetwork.RemoveCallbackTarget(_networker);
        _recognizer?.Stop();
        _recognizer?.Dispose();
        _voskModel?.Dispose();
        _log.LogInfo("VoiceCurse Manager disposed.");
    }
}