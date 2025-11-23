using BepInEx.Configuration;

namespace VoiceCurse;

public class Config(ConfigFile configFile) {
    public ConfigEntry<float> MinAfflictionPercent { get; private set; } = 
        configFile.Bind("General", "MinAfflictionPercent", 0.2f, "The minimum percentage (0.0 to 1.0) of the status bar to fill when a curse is triggered.");
    public ConfigEntry<float> MaxAfflictionPercent { get; private set; } = 
        configFile.Bind("General", "MaxAfflictionPercent", 0.6f, "The maximum percentage (0.0 to 1.0) of the status bar to fill when a curse is triggered.");
    public ConfigEntry<bool> EnableDebugLogs { get; private set; } = 
        configFile.Bind("Debug", "EnableLogs", true, "Enable debug logs for speech detection.");
}