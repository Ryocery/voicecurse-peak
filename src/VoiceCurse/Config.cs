using BepInEx.Configuration;

namespace VoiceCurse;

public class Config {
    // Global
    public ConfigEntry<bool> EnableDebugLogs { get; private set; }
    public ConfigEntry<float> GlobalCooldown { get; private set; }

    // Launch
    public ConfigEntry<bool> LaunchEnabled { get; private set; }
    public ConfigEntry<string> LaunchKeywords { get; private set; }
    public ConfigEntry<float> LaunchForceMultiplier { get; private set; }
    public ConfigEntry<float> LaunchStunDuration { get; private set; }

    // TODO: Move these to AfflictionEvent specific config later
    // Keeping them here temporarily to prevent compilation errors in AfflictionEvent.cs
    public ConfigEntry<float> MinAfflictionPercent { get; private set; }
    public ConfigEntry<float> MaxAfflictionPercent { get; private set; }

    public Config(ConfigFile configFile) {
        // Global
        EnableDebugLogs = configFile.Bind("Global", "EnableDebugLogs", true, "Enable debug logs for speech detection.");
        GlobalCooldown = configFile.Bind("Global", "GlobalCooldown", 2.0f, "The time in seconds that must pass before another voice event can be triggered.");

        // Event: Launch
        LaunchEnabled = configFile.Bind("Event.Launch", "Enabled", true, "Enable the Launch event.");
        LaunchKeywords = configFile.Bind("Event.Launch", "Keywords", 
            "launch, fly, blast, boost, ascend, lift, up, cannon, canon, rocket, soar, jump, spring, catapult, fling, hurl, propel, shoot, skyrocket, takeoff, left, right, forward, forwards, backward, backwards, back, yeet, lob, pitch, toss, chuck, heave, airborne, levitate, hover, elevate, rise, vault, leap, bound, hop, eject, thrust, projectile, missile, space, orbit", 
            "List of keywords that trigger the launch event, separated by commas.");
        LaunchForceMultiplier = configFile.Bind("Event.Launch", "ForceMultiplier", 1.0f, "Multiplier applied to the launch force.");
        LaunchStunDuration = configFile.Bind("Event.Launch", "StunDuration", 3.0f, "Duration in seconds the player will be stunned/ragdolled after launching.");

        // Legacy / Temporary
        MinAfflictionPercent = configFile.Bind("General", "MinAfflictionPercent", 0.2f, "The minimum percentage (0.0 to 1.0) of the status bar to fill when a curse is triggered.");
        MaxAfflictionPercent = configFile.Bind("General", "MaxAfflictionPercent", 0.6f, "The maximum percentage (0.0 to 1.0) of the status bar to fill when a curse is triggered.");
    }
}