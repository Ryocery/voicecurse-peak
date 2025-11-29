using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoiceCurse.Events;

public class AfflictionEvent : VoiceEventBase {
    private readonly Dictionary<string, CharacterAfflictions.STATUSTYPE> _wordToType;

    public AfflictionEvent(Config config) : base(config) {
        _wordToType = new Dictionary<string, CharacterAfflictions.STATUSTYPE>();
        LoadKeywordsForType(config.AfflictionKeywordsInjury.Value, CharacterAfflictions.STATUSTYPE.Injury);
        LoadKeywordsForType(config.AfflictionKeywordsHunger.Value, CharacterAfflictions.STATUSTYPE.Hunger);
        LoadKeywordsForType(config.AfflictionKeywordsCold.Value, CharacterAfflictions.STATUSTYPE.Cold);
        LoadKeywordsForType(config.AfflictionKeywordsHot.Value, CharacterAfflictions.STATUSTYPE.Hot);
        LoadKeywordsForType(config.AfflictionKeywordsPoison.Value, CharacterAfflictions.STATUSTYPE.Poison);
        LoadKeywordsForType(config.AfflictionKeywordsSpores.Value, CharacterAfflictions.STATUSTYPE.Spores);
    }

    private void LoadKeywordsForType(string configLine, CharacterAfflictions.STATUSTYPE type) {
        IEnumerable<string> words = configLine
            .Split([','], StringSplitOptions.RemoveEmptyEntries)
            .Select(w => w.Trim().ToLowerInvariant())
            .Where(w => !string.IsNullOrWhiteSpace(w));

        foreach (string? word in words) {
            _wordToType[word] = type;
        }
    }

    protected override IEnumerable<string> GetKeywords() {
        return Config.AfflictionEnabled.Value ? _wordToType.Keys : Enumerable.Empty<string>();
    }

    protected override bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword) {
        if (!Config.AfflictionEnabled.Value) return false;
        if (player.refs?.afflictions is null) return false;
        if (player.data.dead || player.data.fullyPassedOut) return false;
        
        if (!_wordToType.TryGetValue(matchedKeyword, out CharacterAfflictions.STATUSTYPE statusType)) return false;
        ExecutionDetail = statusType.ToString();

        if (Config.AfflictionTemperatureSwapEnabled.Value && statusType is CharacterAfflictions.STATUSTYPE.Hot or CharacterAfflictions.STATUSTYPE.Cold) { 
            HandleTemperatureExchange(player, statusType);
        }

        float amount = UnityEngine.Random.Range(Config.AfflictionMinPercent.Value, Config.AfflictionMaxPercent.Value);
        
        if (Config.EnableDebugLogs.Value) {
            Debug.Log($"[VoiceCurse] Affliction Specifics: {statusType} ({amount:P0})");
        }
        
        player.refs.afflictions.AddStatus(statusType, amount);
        return true;
    }

    private void HandleTemperatureExchange(Character player, CharacterAfflictions.STATUSTYPE incomingType) {
        CharacterAfflictions.STATUSTYPE oppositeType = incomingType == CharacterAfflictions.STATUSTYPE.Hot ? CharacterAfflictions.STATUSTYPE.Cold : CharacterAfflictions.STATUSTYPE.Hot;
        float currentOppositeValue = player.refs.afflictions.GetCurrentStatus(oppositeType);
        
        if (currentOppositeValue <= 0.01f) return;
        
        if (Config.EnableDebugLogs.Value) {
            Debug.Log($"[VoiceCurse] Swapping {oppositeType} ({currentOppositeValue:P0}) to {incomingType}");
        }
        
        player.refs.afflictions.SubtractStatus(oppositeType, currentOppositeValue);
        player.refs.afflictions.AddStatus(incomingType, currentOppositeValue);
    }
}