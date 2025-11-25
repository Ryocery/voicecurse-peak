using System;
using System.Collections.Generic;
using System.Linq;

namespace VoiceCurse.Events;

public class SleepEvent(Config config) : VoiceEventBase(config) {
    private readonly HashSet<string> _sleepKeywords = ParseKeywords(config.SleepKeywords.Value);

    private static HashSet<string> ParseKeywords(string configLine) {
        return configLine
            .Split([','], StringSplitOptions.RemoveEmptyEntries)
            .Select(k => k.Trim().ToLowerInvariant())
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .ToHashSet();
    }
    
    protected override IEnumerable<string> GetKeywords() {
        return Config.SleepEnabled.Value ? _sleepKeywords : Enumerable.Empty<string>();
    }

    protected override bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword) {
        if (!Config.SleepEnabled.Value) return false;
        if (player.data.passedOut || player.data.dead) return false;

        player.PassOutInstantly();
        return true;
    }
}