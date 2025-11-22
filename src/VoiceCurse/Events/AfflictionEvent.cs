using System.Collections.Generic;
using UnityEngine;
using VoiceCurse.Core;

namespace VoiceCurse.Events {
    public class AfflictionEvent(VoiceCurseConfig config) : IVoiceEvent {
        private readonly Dictionary<string, CharacterAfflictions.STATUSTYPE> _keywords = new() {
            { "damage", CharacterAfflictions.STATUSTYPE.Injury },
            { "hurt", CharacterAfflictions.STATUSTYPE.Injury },
            { "injury", CharacterAfflictions.STATUSTYPE.Injury },
            { "injured", CharacterAfflictions.STATUSTYPE.Injury },
            { "pain", CharacterAfflictions.STATUSTYPE.Injury },
            { "ow", CharacterAfflictions.STATUSTYPE.Injury },
            
            { "hunger", CharacterAfflictions.STATUSTYPE.Hunger },
            { "hungry", CharacterAfflictions.STATUSTYPE.Hunger },
            { "starving", CharacterAfflictions.STATUSTYPE.Hunger },
            { "starve", CharacterAfflictions.STATUSTYPE.Hunger },
            { "food", CharacterAfflictions.STATUSTYPE.Hunger },
            
            { "freezing", CharacterAfflictions.STATUSTYPE.Cold },
            { "cold", CharacterAfflictions.STATUSTYPE.Cold },
            { "blizzard", CharacterAfflictions.STATUSTYPE.Cold },
            { "shiver", CharacterAfflictions.STATUSTYPE.Cold },
            { "ice", CharacterAfflictions.STATUSTYPE.Cold },
            
            { "hot", CharacterAfflictions.STATUSTYPE.Hot },
            { "burning", CharacterAfflictions.STATUSTYPE.Hot },
            { "fire", CharacterAfflictions.STATUSTYPE.Hot },
            { "melt", CharacterAfflictions.STATUSTYPE.Hot },
            
            { "poison", CharacterAfflictions.STATUSTYPE.Poison },
            { "sick", CharacterAfflictions.STATUSTYPE.Poison },
            { "vomit", CharacterAfflictions.STATUSTYPE.Poison },
            { "toxic", CharacterAfflictions.STATUSTYPE.Poison },
            
            { "spores", CharacterAfflictions.STATUSTYPE.Spores },
            { "fungus", CharacterAfflictions.STATUSTYPE.Spores },
            { "mushroom", CharacterAfflictions.STATUSTYPE.Spores },
            { "cough", CharacterAfflictions.STATUSTYPE.Spores },
            
            { "tired", CharacterAfflictions.STATUSTYPE.Drowsy },
            { "sleepy", CharacterAfflictions.STATUSTYPE.Drowsy },
            { "sleep", CharacterAfflictions.STATUSTYPE.Drowsy },
            { "yawn", CharacterAfflictions.STATUSTYPE.Drowsy }
        };

        public bool TryExecute(string spokenWord, string fullSentence) {
            if (!_keywords.TryGetValue(spokenWord, out CharacterAfflictions.STATUSTYPE statusType)) {
                return false;
            }
            
            Character localChar = Character.localCharacter;
            if (localChar?.refs?.afflictions is null) {
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
            return true;
        }
    }
}