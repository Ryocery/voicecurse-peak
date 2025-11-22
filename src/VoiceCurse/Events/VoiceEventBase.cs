using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VoiceCurse.Core;
using VoiceCurse.Networking;

namespace VoiceCurse.Events;

public abstract class VoiceEventBase(VoiceCurseConfig config) : IVoiceEvent {
    protected readonly VoiceCurseConfig Config = config;
    private float _lastExecutionTime = -999f;
    private static float Cooldown => 2.0f;

    protected abstract IEnumerable<string> GetKeywords();

    protected abstract bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword);

    public bool TryExecute(string spokenWord, string fullSentence) {
        if (Time.time < _lastExecutionTime + Cooldown) return false;

        string? matchedKeyword = GetKeywords().FirstOrDefault(spokenWord.Contains);
        if (matchedKeyword == null) return false;

        Character localChar = Character.localCharacter;
        if (localChar is null) return false;
        
        _lastExecutionTime = Time.time;
        
        bool success = OnExecute(localChar, spokenWord, fullSentence, matchedKeyword);
        if (!success) return success;
        
        if (Config.EnableDebugLogs.Value) {
            Debug.Log($"[VoiceCurse] {GetType().Name} executed locally. Sending Network Event...");
        }
            
        string eventName = GetType().Name.Replace("Event", "");
        VoiceCurseNetworker.SendCurseEvent(spokenWord, matchedKeyword, eventName, localChar.Center);

        return success;
    }
    
    public virtual void PlayEffects(Vector3 position) { }
}