using System.Collections.Generic;

namespace VoiceCurse.Events;

public class SleepEvent(Config config) : VoiceEventBase(config) {
    private readonly HashSet<string> _keywords = [
        "faint", "sleep", "exhausted", "sleepy", "tired", "bed", 
        "nap", "rest", "slumber", "doze", "snooze", "pass out", 
        "knockout", "blackout", "coma", "narc", "drowsy"
    ];
    
    protected override IEnumerable<string> GetKeywords() => _keywords;

    protected override bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword) {
        if (player.data.passedOut || player.data.dead) return false;

        player.PassOutInstantly();
        return true;
    }
}