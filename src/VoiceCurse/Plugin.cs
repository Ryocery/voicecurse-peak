using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using Photon.Pun;
using VoiceCurse.Core;
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
    
    private AudioClip? _micClip;
    private string? _deviceName;
    private int _lastSamplePos;
    private bool _isMicInitialized;
        
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
            int systemSampleRate = AudioSettings.outputSampleRate;
            Log.LogInfo($"Detected System Sample Rate: {systemSampleRate} Hz");

            _recognizer = new VoiceRecognizer(modelPath, systemSampleRate);
            
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

        if (!_isMicInitialized && _recognizer != null) {
            SetupMicrophone();
        }

        ProcessMicrophoneData();
    }

    private void SetupMicrophone() {
        _deviceName = null;
        Microphone.GetDeviceCaps(_deviceName, out int minFreq, out int maxFreq);

        int systemRate = AudioSettings.outputSampleRate;
        int targetFreq = systemRate;

        if (maxFreq > 0) {
            targetFreq = Mathf.Clamp(systemRate, minFreq, maxFreq);
        }

        Log.LogInfo($"Starting Direct Microphone Capture. Target Rate: {targetFreq} Hz");
        
        _micClip = Microphone.Start(_deviceName, true, 10, targetFreq);
        _isMicInitialized = true;
        _lastSamplePos = 0;
    }

    private void ProcessMicrophoneData() {
        if (!_isMicInitialized || _micClip is null || _recognizer == null) return;

        int currentPos = Microphone.GetPosition(_deviceName);
        
        if (currentPos == _lastSamplePos) return;
        
        int samplesToRead;
        if (currentPos > _lastSamplePos) {
            samplesToRead = currentPos - _lastSamplePos;
        } else {
            samplesToRead = (_micClip.samples - _lastSamplePos) + currentPos;
        }

        if (samplesToRead <= 0) return;
        
        float[] samples = new float[samplesToRead];
        
        if (currentPos > _lastSamplePos) {
            _micClip.GetData(samples, _lastSamplePos);
        } else {
            float[] endPart = new float[_micClip.samples - _lastSamplePos];
            _micClip.GetData(endPart, _lastSamplePos);
            
            float[] startPart = new float[currentPos];
            _micClip.GetData(startPart, 0);
            
            Array.Copy(endPart, 0, samples, 0, endPart.Length);
            Array.Copy(startPart, 0, samples, endPart.Length, startPart.Length);
        }
        
        short[] pcmBuffer = new short[samples.Length];
        for (int i = 0; i < samples.Length; i++) {
            pcmBuffer[i] = (short)(Mathf.Clamp(samples[i], -1f, 1f) * 32767);
        }
        
        _recognizer.FeedAudio(pcmBuffer, pcmBuffer.Length);
        
        _lastSamplePos = currentPos;
    }

    private void OnDestroy() {
        if (_networker != null) {
            PhotonNetwork.RemoveCallbackTarget(_networker);
        }
        _recognizer?.Stop();
        _recognizer?.Dispose();
    }
}