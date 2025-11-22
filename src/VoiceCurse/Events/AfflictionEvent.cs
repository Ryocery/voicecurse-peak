using System.Collections.Generic;
using UnityEngine;
using VoiceCurse.Core;

namespace VoiceCurse.Events {
    public class AfflictionEvent(VoiceCurseConfig config) : IVoiceEvent {
        private readonly Dictionary<CharacterAfflictions.STATUSTYPE, float> _cooldowns = new();
        private const float CooldownSeconds = 5.0f;

        public bool TryExecute(string spokenWord, string fullSentence) {
            if (!config.Keywords.TryGetValue(spokenWord, out CharacterAfflictions.STATUSTYPE statusType)) {
                return false;
            }

            if (_cooldowns.TryGetValue(statusType, out float lastTime)) {
                if (Time.time - lastTime < CooldownSeconds) {
                    return false;
                }
            }
            
            Character localChar = Character.localCharacter;
            if (localChar?.refs == null || localChar.refs.afflictions == null) {
                return false;
            }

            if (localChar.data.dead || localChar.data.fullyPassedOut) return false;

            float min = config.MinAfflictionPercent.Value;
            float max = config.MaxAfflictionPercent.Value;
            float amount = Random.Range(min, max);

            if (config.EnableDebugLogs.Value) {
                Debug.Log($"[VoiceCurse] Affliction: {statusType} ({amount:P0}) triggered by '{spokenWord}'");
            }

            localChar.refs.afflictions.AddStatus(statusType, amount);
            _cooldowns[statusType] = Time.time;

            return true;
        }
    }
}