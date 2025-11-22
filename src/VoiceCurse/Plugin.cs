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
        private IVoiceRecognizer? _recognizer;
        private AudioStreamTapper? _tapper;
        private AudioSource? _micSource;
        private readonly ConcurrentQueue<Action> _mainThreadActions = new();

        private string _lastPartialText = "";

        private void Awake() {
            Log = Logger;
            Log.LogInfo($"Plugin {Name} is loading...");
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
                _recognizer.OnPhraseRecognized += OnPhraseRecognized;
                _recognizer.OnPartialResult += OnPartialResult;
                
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

            if (_micSource == null && _recognizer != null) {
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
        
        private void OnPhraseRecognized(string text) {
            _mainThreadActions.Enqueue(() => {
                Log.LogInfo($"[FINAL]: {text}");
                _lastPartialText = "";
            });
        }
        
        private void OnPartialResult(string text) {
            if (text == _lastPartialText || text.Length < 2) return;
            
            _lastPartialText = text;

            _mainThreadActions.Enqueue(() => {
                Log.LogInfo($"[PARTIAL]: {text}");
                if (text.Contains("open sesame")) {
                    Log.LogWarning("MAGIC CAST: OPEN SESAME!");
                }
            });
        }

        private void OnDestroy() {
            _recognizer?.Stop();
            _recognizer?.Dispose();
        }
    }
}