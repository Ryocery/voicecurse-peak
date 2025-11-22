using System.Collections.Generic;
using BepInEx.Configuration;

namespace VoiceCurse.Core {
    public class VoiceCurseConfig {
        public ConfigEntry<float> MinAfflictionPercent { get; private set; }
        public ConfigEntry<float> MaxAfflictionPercent { get; private set; }
        public ConfigEntry<bool> EnableDebugLogs { get; private set; }

        // Hardcoded for now, but designed to be expandable later
        public readonly Dictionary<string, CharacterAfflictions.STATUSTYPE> Keywords = new() {
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

        public VoiceCurseConfig(ConfigFile configFile) {
            MinAfflictionPercent = configFile.Bind(
                "General",
                "MinAfflictionPercent",
                0.2f,
                "The minimum percentage (0.0 to 1.0) of the status bar to fill when a curse is triggered."
            );

            MaxAfflictionPercent = configFile.Bind(
                "General",
                "MaxAfflictionPercent",
                0.6f,
                "The maximum percentage (0.0 to 1.0) of the status bar to fill when a curse is triggered."
            );
            
            EnableDebugLogs = configFile.Bind(
                "Debug",
                "EnableLogs",
                true,
                "Enable debug logs for speech detection."
            );
        }
    }
}