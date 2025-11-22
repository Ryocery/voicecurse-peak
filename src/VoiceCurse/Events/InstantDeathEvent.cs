using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using VoiceCurse.Core;

namespace VoiceCurse.Events {
    public class InstantDeathEvent(VoiceCurseConfig config) : IVoiceEvent {
        private readonly HashSet<string> _triggerWords = ["die", "death", "suicide", "kill me"];
        private float _lastTriggerTime;

        public bool TryExecute(string spokenWord, string fullSentence) {
            bool match = _triggerWords.Any(fullSentence.Contains);
            if (!match) return false;
            if (Time.time - _lastTriggerTime < 2.0f) return false;
            Character localChar = Character.localCharacter;
            if (localChar is null || localChar.data.dead) return false;

            if (config.EnableDebugLogs.Value) {
                Debug.Log($"[VoiceCurse] Instant Death triggered by phrase: '{fullSentence}'");
            }
            
            localChar.photonView.RPC("RPCA_Die", RpcTarget.All, localChar.Center);
            
            _lastTriggerTime = Time.time;
            return true;
        }
    }
}