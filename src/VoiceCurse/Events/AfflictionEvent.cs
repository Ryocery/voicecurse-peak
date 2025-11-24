using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoiceCurse.Events;

public class AfflictionEvent(Config config) : VoiceEventBase(config) {
    private static readonly Dictionary<CharacterAfflictions.STATUSTYPE, string[]> KeyWords = new() {
        { CharacterAfflictions.STATUSTYPE.Injury, ["damage", "hurt", "injury", "injured", "pain", "harm", "wound", "hit", "bleed", "bruise", "cut", "slash", "slashed", "orange", "ache", "sore", "trauma", "gash", "scrape", "laceration", "tear", "torn", "broken", "fracture", "sprain", "puncture", "stab", "maim", "maimed", "cripple", "batter"] },
        { CharacterAfflictions.STATUSTYPE.Hunger, ["hunger", "hungry", "starving", "starve", "food", "malnourishment", "famished", "eat", "snack", "meal", "yellow", "appetite", "crave", "craving", "ravenous", "peckish", "feast", "feed", "sustenance", "nourishment", "nutrition", "consume"] },
        { CharacterAfflictions.STATUSTYPE.Cold,   ["freezing", "cold", "blizzard", "shiver", "ice", "frozen", "chill", "frigid", "winter", "blue", "frost", "arctic", "polar", "glacier", "icicle", "hypothermia", "numb", "shivering", "freeze"] },
        { CharacterAfflictions.STATUSTYPE.Hot,    ["hot", "burning", "fire", "melt", "scorching", "heat", "burn", "pyro", "flame", "summer", "cook", "hell", "red", "sizzle", "sear", "swelter", "boil", "roast", "bake", "baking", "scald", "inferno", "blaze", "blazing", "ignite", "combust", "incinerate"] },
        { CharacterAfflictions.STATUSTYPE.Poison, ["poison", "sick", "vomit", "toxic", "venom", "contaminate", "purple", "nausea", "nauseous", "intoxicate", "pollute", "taint", "corrupt", "disease", "ill", "ailment", "malady"] },
        { CharacterAfflictions.STATUSTYPE.Spores, ["spore", "pink"] }
    };

    
    private readonly Dictionary<string, CharacterAfflictions.STATUSTYPE> _wordToType = 
        KeyWords.SelectMany(g => g.Value
            .Select(w => (Word: w, Type: g.Key)))
            .ToDictionary(x => x.Word, x => x.Type);

    protected override IEnumerable<string> GetKeywords() => _wordToType.Keys;

    protected override bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword) {
        if (player.refs?.afflictions is null) return false;
        if (player.data.dead || player.data.fullyPassedOut) return false;
        if (!_wordToType.TryGetValue(matchedKeyword, out CharacterAfflictions.STATUSTYPE statusType)) return false;
        
        ExecutionDetail = statusType.ToString();
    
        if (statusType is CharacterAfflictions.STATUSTYPE.Hot or CharacterAfflictions.STATUSTYPE.Cold) {
            HandleTemperatureExchange(player, statusType);
        }

        float amount = Random.Range(Config.MinAfflictionPercent.Value, Config.MaxAfflictionPercent.Value);
        if (Config.EnableDebugLogs.Value) {
            Debug.Log($"[VoiceCurse] Affliction Specifics: {statusType} ({amount:P0})");
        }
        
        player.refs.afflictions.AddStatus(statusType, amount);
        return true;
    }

    private void HandleTemperatureExchange(Character player, CharacterAfflictions.STATUSTYPE incomingType) {
        CharacterAfflictions.STATUSTYPE oppositeType = incomingType == CharacterAfflictions.STATUSTYPE.Hot ? CharacterAfflictions.STATUSTYPE.Cold : CharacterAfflictions.STATUSTYPE.Hot;
        float currentOppositeValue = player.refs.afflictions.GetCurrentStatus(oppositeType);
        if (currentOppositeValue == 0f) return;
        
        if (Config.EnableDebugLogs.Value) {
            Debug.Log($"[VoiceCurse] Swapping {oppositeType} ({currentOppositeValue:P0}) to {incomingType}");
        }
        
        player.refs.afflictions.SubtractStatus(oppositeType, currentOppositeValue);
        player.refs.afflictions.AddStatus(incomingType, currentOppositeValue);
    }
}