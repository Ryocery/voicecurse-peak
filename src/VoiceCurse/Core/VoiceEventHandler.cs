using System.Collections.Generic;
using VoiceCurse.Events;

namespace VoiceCurse.Core;

public class VoiceEventHandler(VoiceCurseConfig config) {
    private readonly List<IVoiceEvent> _events = new() {
        new DeathEvent(config),
        new AfflictionEvent(config)
    };
        
    private readonly Dictionary<string, int> _previousWordCounts = new();

    public void HandleSpeech(string text, bool isFinal) {
        if (string.IsNullOrWhiteSpace(text)) return;

        string lowerText = text.ToLowerInvariant();
        string[] words = lowerText.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        Dictionary<string, int> currentCounts = new();
            
        foreach (string w in words) {
            currentCounts.TryAdd(w, 0);
            currentCounts[w]++;
        }

        foreach (KeyValuePair<string, int> kvp in currentCounts) {
            string word = kvp.Key;
            int count = kvp.Value;
                
            int previousCount = 0;
            if (_previousWordCounts.TryGetValue(word, out int prev)) {
                previousCount = prev;
            }
                
            int diff = count - previousCount;

            if (diff <= 0) continue;
                
            for (int i = 0; i < diff; i++) {
                foreach (IVoiceEvent evt in _events) {
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