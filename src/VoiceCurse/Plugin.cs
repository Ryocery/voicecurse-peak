using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using Photon.Pun;
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
    private AudioStreamTapper? _tapper;
    private AudioSource? _micSource;
    private VoiceCurseNetworker? _networker;
        
    private readonly ConcurrentQueue<Action> _mainThreadActions = new();
    private volatile string _lastPartialText = "";

    private void Awake() {
        Log = Logger;
        Log.LogInfo($"Plugin {Name} is loading...");
        string? pluginDir = Path.GetDirectoryName(Info.Location);
        
        if (Directory.Exists(pluginDir)) {
            SetDllDirectory(pluginDir);
            Log.LogInfo($"Added DLL search directory: {pluginDir}");
        } else {
            Log.LogError($"Could not find plugin directory: {pluginDir}");
        }

        _config = new VoiceCurseConfig(Config);
        
        if (_config != null) {
            _eventHandler = new VoiceEventHandler(_config);
        }
        
        _networker = new VoiceCurseNetworker();
        PhotonNetwork.AddCallbackTarget(_networker);

        SetupVoiceRecognition();
    }

    private void SetupVoiceRecognition() {
        string modelPath = Path.Combine(Paths.PluginPath, "VoiceCurse", "model-en-us");

        if (!Directory.Exists(modelPath)) {
            Log.LogError($"Vosk model not found! Please create folder: {modelPath}");
            return;
        }

        try {
            _recognizer = new VoiceRecognizer(modelPath);
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
            Log.LogInfo("Voice Recognizer started successfully.");
        } catch (Exception ex) {
            Log.LogError($"Failed to start Voice Recognizer: {ex.Message}");
        }
    }

    private void Update() {
        while (_mainThreadActions.TryDequeue(out Action action)) {
            action.Invoke();
        }

        if (_micSource is null && _recognizer != null) {
            SetupMicrophone();
        }
    }

    private void SetupMicrophone() {
        GameObject micObj = new("VoiceCurse_Mic");
        DontDestroyOnLoad(micObj);
            
        _micSource = micObj.AddComponent<AudioSource>();
        _tapper = micObj.AddComponent<AudioStreamTapper>();
            
        if (_recognizer != null) {
            _tapper.Initialize(_recognizer, muteOutput: true);
        }
        
        string? deviceName = null; // Uses system default

        Log.LogInfo("Starting Microphone Capture on: System Default");
            
        _micSource.clip = Microphone.Start(deviceName, true, 10, 48000);
        _micSource.loop = true;
            
        while (!(Microphone.GetPosition(deviceName) > 0)) { }
            
        _micSource.Play();
    }

    private void OnDestroy() {
        if (_networker != null) {
            PhotonNetwork.RemoveCallbackTarget(_networker);
        }
        _recognizer?.Stop();
        _recognizer?.Dispose();
    }
}