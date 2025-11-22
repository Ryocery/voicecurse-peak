using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
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
using VoiceCurse.Networking;

namespace VoiceCurse;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin {
    private static ManualLogSource Log { get; set; } = null!;
    
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool SetDllDirectory(string lpPathName);

    private VoiceCurseConfig? _config;
    private IVoiceRecognizer? _recognizer;
    private VoiceEventHandler? _eventHandler;
    private VoiceCurseNetworker? _networker;
    
    private Model? _voskModel;
    
    private int _currentSampleRate;
    private bool _isHooked;
        
    private readonly ConcurrentQueue<Action> _mainThreadActions = new();
    private volatile string _lastPartialText = "";

    private void Awake() {
        Log = Logger;
        Log.LogInfo($"Plugin {Name} is loading...");
        
        string? pluginDir = Path.GetDirectoryName(Info.Location);
        if (Directory.Exists(pluginDir)) {
            SetDllDirectory(pluginDir);
        }

        _config = new VoiceCurseConfig(Config);
        if (_config != null) {
            _eventHandler = new VoiceEventHandler(_config);
        }

        _networker = new VoiceCurseNetworker();
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

        if (!_isHooked && Character.localCharacter is not null) {
            TryHookIntoPhotonVoice();
        }
    }

    private void TryHookIntoPhotonVoice() {
        PhotonVoiceView voiceView = Character.localCharacter.GetComponent<PhotonVoiceView>();
        
        if (voiceView?.RecorderInUse is null) return;
        Recorder recorder = voiceView.RecorderInUse;
            
        VoiceCurseHook hook = recorder.gameObject.AddComponent<VoiceCurseHook>();
        hook.Initialize(this, recorder);
            
        _isHooked = true;
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
            if (_recognizer != null) floatVoice.AddPostProcessor(new VoiceCurseAudioProcessor(_recognizer));
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

public class VoiceCurseHook : MonoBehaviour {
    private Plugin? _plugin;
    private Recorder? _recorder;
    private bool _hasInjected;

    public void Initialize(Plugin plugin, Recorder recorder) {
        _plugin = plugin;
        _recorder = recorder;
        CheckIfAlreadyReady();
    }

    private void CheckIfAlreadyReady() {
        if (_recorder is null || !_recorder.IsCurrentlyTransmitting) return;
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        FieldInfo? field = typeof(Recorder).GetField("voice", flags);

        if (field == null) return;
        LocalVoice? voice = field.GetValue(_recorder) as LocalVoice;
        if (voice == null) return;
        _plugin?.OnPhotonVoiceReady(_recorder, voice);
        _hasInjected = true;
    }

    private void PhotonVoiceCreated(PhotonVoiceCreatedParams p) {
        if (_hasInjected) return;
        if (_recorder is not null) _plugin?.OnPhotonVoiceReady(_recorder, p.Voice);
        _hasInjected = true;
    }
}