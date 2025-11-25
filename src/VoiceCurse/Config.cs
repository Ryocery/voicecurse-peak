using BepInEx.Configuration;

namespace VoiceCurse;

public class Config {
    // Global
    public ConfigEntry<bool> EnableDebugLogs { get; private set; }
    public ConfigEntry<float> GlobalCooldown { get; private set; }

    // Event: Launch
    public ConfigEntry<bool> LaunchEnabled { get; private set; }
    public ConfigEntry<string> LaunchKeywords { get; private set; }
    public ConfigEntry<float> LaunchStunDuration { get; private set; }
    public ConfigEntry<float> LaunchForceLowerBound { get; private set; }
    public ConfigEntry<float> LaunchForceHigherBound { get; private set; }

    // Event: Affliction
    public ConfigEntry<bool> AfflictionEnabled { get; private set; }
    public ConfigEntry<float> AfflictionMinPercent { get; private set; }
    public ConfigEntry<float> AfflictionMaxPercent { get; private set; }
    public ConfigEntry<bool> AfflictionTemperatureSwapEnabled { get; private set; }
    
    // Affliction Keywords
    public ConfigEntry<string> AfflictionKeywordsInjury { get; private set; }
    public ConfigEntry<string> AfflictionKeywordsHunger { get; private set; }
    public ConfigEntry<string> AfflictionKeywordsCold { get; private set; }
    public ConfigEntry<string> AfflictionKeywordsHot { get; private set; }
    public ConfigEntry<string> AfflictionKeywordsPoison { get; private set; }
    public ConfigEntry<string> AfflictionKeywordsSpores { get; private set; }

    public Config(ConfigFile configFile) {
        // Global
        EnableDebugLogs = configFile.Bind("Global", "EnableDebugLogs", true, "Enable debug logs for speech detection.");
        GlobalCooldown = configFile.Bind("Global", "GlobalCooldown", 2.0f, "The time in seconds that must pass before another voice event can be triggered.");

        // Event: Launch
        LaunchEnabled = configFile.Bind("Event.Launch", "Enabled", true, "Enable the Launch event.");
        LaunchKeywords = configFile.Bind("Event.Launch", "Keywords", "launch, fly, blast, boost, ascend, lift, up, cannon, canon, rocket, soar, jump, spring, catapult, fling, hurl, propel, shoot, skyrocket, takeoff, left, right, forward, forwards, backward, backwards, back, yeet, lob, pitch, toss, chuck, heave, airborne, levitate, hover, elevate, rise, vault, leap, bound, hop, eject, thrust, projectile, missile, space, orbit", "List of keywords that trigger the launch event, separated by commas.");
        LaunchForceLowerBound = configFile.Bind("Event.Launch", "ForceLowerBound", 1500f, "Lowest possible amount of force applied to the player upon launching..");
        LaunchForceHigherBound = configFile.Bind("Event.Launch", "ForceHigherBound", 3000f, "Highest possible amount of force applied to the player upon launching..");
        LaunchStunDuration = configFile.Bind("Event.Launch", "StunDuration", 3.0f, "Duration in seconds the player will be stunned/ragdolled after launching.");

        // Event: Affliction
        AfflictionEnabled = configFile.Bind("Event.Affliction", "Enabled", true, "Enable the Affliction event.");
        AfflictionMinPercent = configFile.Bind("Event.Affliction", "MinPercent", 0.2f, "The minimum percentage (0.0 to 1.0) of the status bar to fill when triggered.");
        AfflictionMaxPercent = configFile.Bind("Event.Affliction", "MaxPercent", 0.6f, "The maximum percentage (0.0 to 1.0) of the status bar to fill when triggered.");
        AfflictionTemperatureSwapEnabled = configFile.Bind("Event.Affliction", "TemperatureSwapEnabled", true, "If enabled, triggering Hot will cure Cold (and vice versa) instead of just adding status.");
        
        AfflictionKeywordsInjury = configFile.Bind("Event.Affliction", "KeywordsInjury", "damage, hurt, injury, injured, pain, harm, wound, hit, bleed, bruise, cut, slash, slashed, orange, ache, sore, trauma, gash, scrape, laceration, tear, torn, broken, fracture, sprain, puncture, stab, maim, maimed, cripple, batter", "Keywords that trigger physical injury.");
        AfflictionKeywordsHunger = configFile.Bind("Event.Affliction", "KeywordsHunger", "hunger, hungry, starving, starve, food, malnourishment, famished, eat, snack, meal, yellow, appetite, crave, craving, ravenous, peckish, feast, feed, sustenance, nourishment, nutrition, consume", "Keywords that trigger hunger.");
        AfflictionKeywordsCold = configFile.Bind("Event.Affliction", "KeywordsCold", "freezing, cold, blizzard, shiver, ice, frozen, chill, frigid, winter, blue, frost, arctic, polar, glacier, icicle, hypothermia, numb, shivering, freeze", "Keywords that trigger cold status.");
        AfflictionKeywordsHot = configFile.Bind("Event.Affliction", "KeywordsHot", "hot, burning, fire, melt, scorching, heat, burn, pyro, flame, summer, cook, hell, red, sizzle, sear, swelter, boil, roast, bake, baking, scald, inferno, blaze, blazing, ignite, combust, incinerate", "Keywords that trigger hot status.");
        AfflictionKeywordsPoison = configFile.Bind("Event.Affliction", "KeywordsPoison", "poison, sick, vomit, toxic, venom, contaminate, purple, nausea, nauseous, intoxicate, pollute, taint, corrupt, disease, ill, ailment, malady", "Keywords that trigger poison status.");
        AfflictionKeywordsSpores = configFile.Bind("Event.Affliction", "KeywordsSpores", "spore, pink", "Keywords that trigger spore status.");
    }
}