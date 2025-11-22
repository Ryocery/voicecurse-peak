using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using VoiceCurse.Core;

namespace VoiceCurse.Events;

public class SleepEvent(VoiceCurseConfig config) : IVoiceEvent {
    private readonly HashSet<string> _triggerWords = ["faint", "sleep", "exhausted", "sleepy", "tired", "bed"];
    
    public bool TryExecute(string spokenWord, string fullSentence) {
        bool match = _triggerWords.Any(fullSentence.Contains);
        if (!match) return false;
        Character localChar = Character.localCharacter;
        if (localChar is null || localChar.data.passedOut || localChar.data.dead) return false;

        if (config.EnableDebugLogs.Value) {
            Debug.Log($"[VoiceCurse] Fainting triggered by phrase: '{fullSentence}'");
        }
            
        localChar.photonView.RPC("RPCA_PassOut", RpcTarget.All, localChar.Center);
        return true;
    }
}