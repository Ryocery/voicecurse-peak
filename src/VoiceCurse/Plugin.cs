using System;
using System.Collections.Concurrent;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using VoiceCurse.Core;
using VoiceCurse.Audio;

namespace VoiceCurse {
    [BepInAutoPlugin]
    public partial class Plugin : BaseUnityPlugin {
        internal static ManualLogSource Log { get; set; } = null!;
        
        private VoiceCurseConfig? _config;
        private IVoiceRecognizer? _recognizer;
        private VoiceEventHandler? _eventHandler;
        
        private AudioStreamTapper? _tapper;
        private AudioSource? _micSource;
        
        private readonly ConcurrentQueue<Action> _mainThreadActions = new();

        private void Awake() {
            Log = Logger;
            Log.LogInfo($"Plugin {Name} is loading...");
            
            _config = new VoiceCurseConfig(Config);
            
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
                _recognizer.Start();
                
                if (_config != null) {
                    _eventHandler = new VoiceEventHandler(_config, _recognizer);
                }

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
            GameObject micObj = new GameObject("VoiceCurse_Mic");
            DontDestroyOnLoad(micObj);
            
            _micSource = micObj.AddComponent<AudioSource>();
            _tapper = micObj.AddComponent<AudioStreamTapper>();
            
            if (_recognizer != null) {
                _tapper.Initialize(_recognizer, muteOutput: true);
            }

            Log.LogInfo("Starting Microphone Capture...");
            
            _micSource.clip = Microphone.Start(null, true, 10, 48000);
            _micSource.loop = true;
            
            while (!(Microphone.GetPosition(null) > 0)) { }
            
            _micSource.Play();
        }

        private void OnDestroy() {
            _eventHandler?.Dispose();
            _recognizer?.Stop();
            _recognizer?.Dispose();
        }
    }
}