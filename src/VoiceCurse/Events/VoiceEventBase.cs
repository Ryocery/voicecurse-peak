using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoiceCurse.Handlers;
using VoiceCurse.Interfaces;

namespace VoiceCurse.Events;

public abstract class VoiceEventBase(Config config) : IVoiceEvent {
    protected readonly Config Config = config;
    private float _lastExecutionTime = -999f;
    private static float Cooldown => 2.0f;

    protected string? ExecutionDetail { get; set; }
    protected abstract IEnumerable<string> GetKeywords();
    protected abstract bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword);

    public bool TryExecute(string spokenWord, string fullSentence) {
        if (Time.time < _lastExecutionTime + Cooldown) return false;

        string? matchedKeyword = GetKeywords().FirstOrDefault(spokenWord.Contains);
        if (matchedKeyword == null) return false;
        
        Character localChar = Character.localCharacter;
        if (!localChar || !localChar.gameObject.activeInHierarchy) return false;
        
        _lastExecutionTime = Time.time;
        ExecutionDetail = null;
        bool success = false;
        
        try {
            success = OnExecute(localChar, spokenWord, fullSentence, matchedKeyword);
        } catch (System.Exception e) {
            if (Config.EnableDebugLogs.Value) {
                Debug.LogWarning($"[VoiceCurse] Failed to execute {GetType().Name}: {e.Message}");
            }
        }

        if (!success) return success;
        
        if (Config.EnableDebugLogs.Value) {
            Debug.Log($"[VoiceCurse] {GetType().Name} executed locally. Broadcasting event...");
        }
        
        string eventName = GetType().Name.Replace("Event", "");
        if (!string.IsNullOrEmpty(ExecutionDetail)) {
            eventName += $" ({ExecutionDetail})";
        }

        NetworkHandler.SendCurseEvent(spokenWord, matchedKeyword, eventName, localChar.Center);

        return success;
    }

    public virtual void PlayEffects(Vector3 position) { }
}