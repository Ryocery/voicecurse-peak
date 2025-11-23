using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;
using VoiceCurse.Interfaces;
using Vosk;

namespace VoiceCurse.Voice;

public class VoiceRecognizer : IVoiceRecognizer {
    private readonly VoskRecognizer _recognizer;
    private readonly ConcurrentQueue<short[]> _audioQueue = new();
    private Thread? _workerThread;
    private volatile bool _isRunning;

    public event Action<string>? OnPhraseRecognized;
    public event Action<string>? OnPartialResult;
    
    public VoiceRecognizer(Model model, float sampleRate) {
        try {
            _recognizer = new VoskRecognizer(model, sampleRate);
            _recognizer.SetMaxAlternatives(0);
            _recognizer.SetWords(true);
            
            Debug.Log($"[VoiceCurse] Initialized VoskRecognizer with Rate: {sampleRate} Hz");
        } catch (Exception e) {
            Debug.LogError("Failed to create VoskRecognizer: " + e.Message);
            throw;
        }
    }

    public void Start() {
        if (_isRunning) return;

        _isRunning = true;
        _workerThread = new Thread(ProcessAudioLoop) {
            IsBackground = true,
            Name = "VoiceCurse Worker"
        };
        _workerThread.Start();
    }

    public void Stop() {
        _isRunning = false;
        if (_workerThread is { IsAlive: true }) {
            _workerThread.Join(500);
        }
    }

    public void FeedAudio(short[] pcmData, int length) {
        if (_isRunning) {
            _audioQueue.Enqueue(pcmData);
        }
    }

    private void ProcessAudioLoop() {
        while (_isRunning) {
            if (_audioQueue.TryDequeue(out short[] data)) {
                if (_recognizer.AcceptWaveform(data, data.Length)) {
                    string jsonResult = _recognizer.Result();
                    ExtractAndFire(jsonResult, isPartial: false);
                } else {
                    string partialJson = _recognizer.PartialResult();
                    ExtractAndFire(partialJson, isPartial: true);
                }
            } else {
                Thread.Sleep(10);
            }
        }
    }

    private void ExtractAndFire(string json, bool isPartial) {
        if (string.IsNullOrEmpty(json)) return;

        string key = isPartial ? "\"partial\"" : "\"text\"";
        int textIndex = json.IndexOf(key + " :", StringComparison.Ordinal);
            
        if (textIndex == -1) {
            textIndex = json.IndexOf(key + ":", StringComparison.Ordinal);
        }
        
        if (textIndex == -1) return;
        
        int start = json.IndexOf("\"", textIndex + key.Length + 1, StringComparison.Ordinal) + 1;
        int end = json.LastIndexOf("\"", StringComparison.Ordinal);

        if (end <= start) return;
        
        string text = json.Substring(start, end - start);
        
        if (string.IsNullOrWhiteSpace(text)) return;
        
        if (isPartial) {
            OnPartialResult?.Invoke(text);
        } else {
            OnPhraseRecognized?.Invoke(text);
        }
    }

    public void Dispose() {
        Stop();
        _recognizer.Dispose();
    }
}