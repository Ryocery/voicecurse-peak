using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoiceCurse.Events;

public class AfflictionEvent(Config config) : VoiceEventBase(config) {
    private static readonly Dictionary<CharacterAfflictions.STATUSTYPE, string[]> WordGroups = new() {
        { CharacterAfflictions.STATUSTYPE.Injury, ["damage", "hurt", "injury", "injured", "pain", "harm", "wound", "hit", "bleed", "bruise", "cut", "slash", "slashed"] },
        { CharacterAfflictions.STATUSTYPE.Hunger, ["hunger", "hungry", "starving", "starve", "food", "malnourishment", "famished"] },
        { CharacterAfflictions.STATUSTYPE.Cold,   ["freezing", "cold", "blizzard", "shiver", "ice", "frozen", "chill", "frigid", "winter"] },
        { CharacterAfflictions.STATUSTYPE.Hot,    ["hot", "burning", "fire", "melt", "scorching", "heat", "burn", "pyro", "flame", "summer"] },
        { CharacterAfflictions.STATUSTYPE.Poison, ["poison", "sick", "vomit", "toxic", "venom", "contaminate"] },
        { CharacterAfflictions.STATUSTYPE.Spores, ["spores", "spore", "zombie", "fungus", "mushroom", "fungi", "funghi", "infected", "shroom"] },
    };
    
    private readonly Dictionary<string, CharacterAfflictions.STATUSTYPE> _wordToType = 
        WordGroups.SelectMany(g => g.Value.Select(w => (Word: w, Type: g.Key)))
            .ToDictionary(x => x.Word, x => x.Type);

    protected override IEnumerable<string> GetKeywords() => _wordToType.Keys;

    protected override bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword) {
        if (player.refs?.afflictions is null) return false;
        if (player.data.dead || player.data.fullyPassedOut) return false;

        if (!_wordToType.TryGetValue(matchedKeyword, out CharacterAfflictions.STATUSTYPE statusType)) {
            return false;
        }

        float amount = Random.Range(Config.MinAfflictionPercent.Value, Config.MaxAfflictionPercent.Value);
        
        if (Config.EnableDebugLogs.Value) {
            Debug.Log($"[VoiceCurse] Affliction Specifics: {statusType} ({amount:P0})");
        }

        player.refs.afflictions.AddStatus(statusType, amount);
        return true;
    }
}