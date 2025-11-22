using System.Collections.Generic;
using VoiceCurse.Events;

namespace VoiceCurse.Core;

public class VoiceEventHandler {
    public static readonly Dictionary<string, IVoiceEvent> Events = new();
    
    private readonly Dictionary<string, int> _previousWordCounts = new();

    public VoiceEventHandler(VoiceCurseConfig config) {
        List<IVoiceEvent> eventList = [
            new DeathEvent(config),
            new AfflictionEvent(config),
            new SleepEvent(config),
            new ExplodeEvent(config),
            new LaunchEvent(config),
            new DropEvent(config)
        ];

        Events.Clear();
        foreach (IVoiceEvent? evt in eventList) {
            string name = evt.GetType().Name.Replace("Event", "");
            Events[name] = evt;
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