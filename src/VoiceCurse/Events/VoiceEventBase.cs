using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoiceCurse.Core;

namespace VoiceCurse.Events;

public abstract class VoiceEventBase(VoiceCurseConfig config) : IVoiceEvent {
    protected readonly VoiceCurseConfig Config = config;
    protected abstract IEnumerable<string> GetKeywords();
    protected abstract bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword);

    public bool TryExecute(string spokenWord, string fullSentence) {
        string? matchedKeyword = GetKeywords().FirstOrDefault(spokenWord.Contains);
        if (matchedKeyword == null) return false;
        
        Character localChar = Character.localCharacter;
        if (localChar is null) return false;
        NotifyPlayer(spokenWord, matchedKeyword);
        
        if (Config.EnableDebugLogs.Value) {
            Debug.Log($"[VoiceCurse] {GetType().Name} triggered by word '{spokenWord}' (matched '{matchedKeyword}')");
        }
        
        return OnExecute(localChar, spokenWord, fullSentence, matchedKeyword);
    }

    private void NotifyPlayer(string fullWord, string keyword) {
        UI_Notifications? uiNotifications = Object.FindFirstObjectByType<UI_Notifications>();
        if (uiNotifications is null) return;

        string displayString = fullWord;
        int index = fullWord.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase);
        
        if (index >= 0) {
            string prefix = fullWord[..index];
            string match = fullWord.Substring(index, keyword.Length);
            string suffix = fullWord[(index + keyword.Length)..];
            
            displayString = $"{prefix}<color=red><b>{match}</b></color>{suffix}";
        }

        uiNotifications.AddNotification($"Curse Trigger: {displayString}");
    }
}