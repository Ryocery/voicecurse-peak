using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VoiceCurse.Core;

namespace VoiceCurse.Events;

public abstract class VoiceEventBase(VoiceCurseConfig config) : IVoiceEvent {
    protected readonly VoiceCurseConfig Config = config;
    private static MonoBehaviour? _connectionLog;
    private static MethodInfo? _addMessageMethod;

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

        NotifyPlayer(localChar, spokenWord, matchedKeyword);

        if (Config.EnableDebugLogs.Value) {
            Debug.Log($"[VoiceCurse] {GetType().Name} triggered by word '{spokenWord}' (matched '{matchedKeyword}')");
        }

        return OnExecute(localChar, spokenWord, fullSentence, matchedKeyword);
    }

private void NotifyPlayer(Character player, string fullWord, string keyword) {
        _connectionLog ??=
            Object.FindFirstObjectByType(System.Type.GetType("PlayerConnectionLog, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")) as MonoBehaviour ??
            Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).FirstOrDefault(m => m.GetType().Name == "PlayerConnectionLog");

        if (_connectionLog is null) {
            if (Config.EnableDebugLogs.Value) Debug.LogWarning("[VoiceCurse] Could not find PlayerConnectionLog!");
            return;
        }
        
        if (_addMessageMethod == null) { 
            _addMessageMethod = _connectionLog.GetType().GetMethod("AddMessage", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        if (_addMessageMethod == null) return;
        
        Color playerColor = Color.white;
        if (player.refs?.customization is not null) {
            playerColor = player.refs.customization.PlayerColor;
        }
        string playerHex = "#" + ColorUtility.ToHtmlStringRGB(playerColor);
        
        string displayString = fullWord;
        int index = fullWord.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase);

        if (index >= 0) {
            string prefix = fullWord[..index];
            string match = fullWord.Substring(index, keyword.Length);
            string suffix = fullWord[(index + keyword.Length)..];
            
            displayString = $"{prefix}<color=#8B0000>{match}</color>{suffix}";
        }
        
        string eventName = GetType().Name.Replace("Event", "");
    
        string finalMessage = $"<color={playerHex}>{player.characterName} said \"{displayString}\" which triggered </color><color=#FFA500>{eventName}</color>";
        
        try {
            _addMessageMethod.Invoke(_connectionLog, [finalMessage]);
        } catch (System.Exception e) {
            if (Config.EnableDebugLogs.Value) {
                Debug.LogError($"[VoiceCurse] Reflection Invoke Failed: {e.Message}");
            }
        }
    }
}