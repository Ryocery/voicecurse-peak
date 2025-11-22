using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoiceCurse.Core;

namespace VoiceCurse.Events;

public class AfflictionEvent(VoiceCurseConfig config) : IVoiceEvent {
    private static readonly Dictionary<CharacterAfflictions.STATUSTYPE, string[]> WordGroups = new() {
        { CharacterAfflictions.STATUSTYPE.Injury, ["damage", "hurt", "injury", "injured", "pain", "ow"] },
        { CharacterAfflictions.STATUSTYPE.Hunger, ["hunger", "hungry", "starving", "starve", "food"] },
        { CharacterAfflictions.STATUSTYPE.Cold,   ["freezing", "cold", "blizzard", "shiver", "ice"] },
        { CharacterAfflictions.STATUSTYPE.Hot,    ["hot", "burning", "fire", "melt"] },
        { CharacterAfflictions.STATUSTYPE.Poison, ["poison", "sick", "vomit", "toxic"] },
        { CharacterAfflictions.STATUSTYPE.Spores, ["spores", "fungus", "mushroom"] },
    };
        
    private readonly Dictionary<string, CharacterAfflictions.STATUSTYPE> _keywords = 
        WordGroups.SelectMany(g => g.Value.Select(w => (Word: w, Type: g.Key))).ToDictionary(x => x.Word, x => x.Type); 
    // Thank you, LINQ, very cool.

    public bool TryExecute(string spokenWord, string fullSentence) {
        if (!_keywords.TryGetValue(spokenWord, out CharacterAfflictions.STATUSTYPE statusType)) {
            return false;
        }
            
        Character localChar = Character.localCharacter;
        if (localChar?.refs?.afflictions is null) return false;
        if (localChar.data.dead || localChar.data.fullyPassedOut) return false;
            
        float amount = Random.Range(config.MinAfflictionPercent.Value, config.MaxAfflictionPercent.Value);

        if (config.EnableDebugLogs.Value) {
            Debug.Log($"[VoiceCurse] Affliction: {statusType} ({amount:P0}) triggered by '{spokenWord}'");
        }
            
        localChar.refs.afflictions.AddStatus(statusType, amount);
        return true;
    }
}