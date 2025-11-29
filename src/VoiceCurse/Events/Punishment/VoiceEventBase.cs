using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoiceCurse.Handlers;
using VoiceCurse.Interfaces;

namespace VoiceCurse.Events.Punishment;

public abstract class VoiceEventBase(Config config) : IVoiceEvent {
    protected readonly Config Config = config;
    private float _lastExecutionTime = -999f;
    
    private float Cooldown => Config.GlobalCooldown.Value;

    protected string? ExecutionDetail { get; set; }
    protected abstract IEnumerable<string> GetKeywords();
    protected abstract bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword);

    public bool TryExecute(string spokenWord, string fullSentence) {
        if (Time.time < _lastExecutionTime + Cooldown) return false;
        
        string? matchedKeyword = GetKeywords().FirstOrDefault(keyword => 
            spokenWord.Contains((string)keyword) || 
            (fullSentence.Contains((string)keyword) && keyword.EndsWith(" " + spokenWord, System.StringComparison.OrdinalIgnoreCase))
        );
        
        if (matchedKeyword == null) return false;
        
        Character localChar = Character.localCharacter;
        if (!localChar || !localChar.gameObject.activeInHierarchy) return false;
        
        ExecutionDetail = null;
        bool success = false;
        
        try {
            success = OnExecute(localChar, spokenWord, fullSentence, matchedKeyword);
        } catch (System.Exception e) {
            if (Config.EnableDebugLogs.Value) {
                Debug.LogWarning($"[VoiceCurse] Failed to execute {GetType().Name}: {e.Message}");
            }
        }

        if (!success) return false;
        _lastExecutionTime = Time.time;
        
        if (Config.EnableDebugLogs.Value) {
            Debug.Log($"[VoiceCurse] {GetType().Name} executed locally. Broadcasting event...");
        }
        
        string eventName = GetType().Name.Replace("Event", "");
        string textToSend = fullSentence;
        int matchIndex = fullSentence.LastIndexOf(matchedKeyword, System.StringComparison.OrdinalIgnoreCase);

        if (matchIndex != -1) {
            int start = matchIndex;
            int end = matchIndex + matchedKeyword.Length;
            
            while (start > 0 && fullSentence[start - 1] != ' ') {
                start--;
            }
            
            while (end < fullSentence.Length && fullSentence[end] != ' ') {
                end++;
            }
            
            textToSend = fullSentence.Substring(start, end - start);
        }
        
        NetworkHandler.SendCurseEvent(textToSend, matchedKeyword, eventName, ExecutionDetail, localChar.Center);

        return true;
    }

    public virtual void PlayEffects(Character origin, Vector3 position) {
        PlayEffects(position);
    }

    public virtual void PlayEffects(Vector3 position) { }
}