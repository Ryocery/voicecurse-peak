using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VoiceCurse.Events.Punishment;
using VoiceCurse.Interfaces;

namespace VoiceCurse.Handlers;

public class EventHandler {
    public static readonly Dictionary<string, IVoiceEvent> Events = new();
    private readonly Dictionary<string, int> _previousWordCounts = new();

    public EventHandler(Config config) {
        Events.Clear();
        RegisterEvents(config);
    }
    
    private static void RegisterEvents(Config config) {
        IEnumerable<Type> eventTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(VoiceEventBase)
            .IsAssignableFrom(t) && !t.IsAbstract && t.IsClass);

        foreach (Type type in eventTypes) {
            try {
                if (Activator.CreateInstance(type, config) is not IVoiceEvent evt) continue;
                string name = type.Name.Replace("Event", "");
                Events[name] = evt;
                    
                if (config.EnableDebugLogs.Value) {
                    Debug.Log($"[VoiceCurse] Registered event: {name}");
                }
            } catch (Exception e) {
                Debug.LogError($"[VoiceCurse] Failed to register event {type.Name}: {e.Message}");
            }
        }
    }

    public void HandleSpeech(string text, bool isFinal) {
        if (string.IsNullOrWhiteSpace(text)) return;

        string lowerText = text.ToLowerInvariant();
        string[] words = lowerText.Split([' '], System.StringSplitOptions.RemoveEmptyEntries);
        Dictionary<string, int> currentCounts = new();
            
        foreach (string w in words) {
            currentCounts.TryAdd(w, 0);
            currentCounts[w]++;
        }

        foreach ((string? word, int count) in currentCounts) {
            int previousCount = 0;
            if (_previousWordCounts.TryGetValue(word, out int prev)) {
                previousCount = prev;
            }
                
            int diff = count - previousCount;

            if (diff <= 0) continue;
                
            for (int i = 0; i < diff; i++) {
                foreach (IVoiceEvent evt in Events.Values) {
                    evt.TryExecute(word, lowerText);
                }
            }
        }

        _previousWordCounts.Clear();
        foreach (KeyValuePair<string, int> kvp in currentCounts) {
            _previousWordCounts[kvp.Key] = kvp.Value;
        }

        if (isFinal) {
            _previousWordCounts.Clear();
        }
    }
}