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

    // Event: Transmute
    public ConfigEntry<bool> TransmuteEnabled { get; private set; }
    
    // Individual Transmute Toggles
    public ConfigEntry<bool> TransmuteMilkEnabled { get; private set; }
    public ConfigEntry<bool> TransmuteCactusEnabled { get; private set; }
    public ConfigEntry<bool> TransmuteCoconutEnabled { get; private set; }
    public ConfigEntry<bool> TransmuteAppleEnabled { get; private set; }
    public ConfigEntry<bool> TransmuteBananaEnabled { get; private set; }
    public ConfigEntry<bool> TransmuteEggEnabled { get; private set; }
    public ConfigEntry<bool> TransmuteFruitEnabled { get; private set; }
    public ConfigEntry<bool> TransmuteMushroomEnabled { get; private set; }
    
    // Event: Death
    public ConfigEntry<bool> DeathEnabled { get; private set; }
    public ConfigEntry<string> DeathKeywords { get; private set; }

    // Event: Zombify
    public ConfigEntry<bool> ZombifyEnabled { get; private set; }
    public ConfigEntry<string> ZombifyKeywords { get; private set; }

    // Event: Sleep
    public ConfigEntry<bool> SleepEnabled { get; private set; }
    public ConfigEntry<string> SleepKeywords { get; private set; }
    
    // Event: Drop
    public ConfigEntry<bool> DropEnabled { get; private set; }
    public ConfigEntry<string> DropKeywords { get; private set; }

    // Event: Explode
    public ConfigEntry<bool> ExplodeEnabled { get; private set; }
    public ConfigEntry<string> ExplodeKeywords { get; private set; }
    public ConfigEntry<float> ExplodeRadius { get; private set; }
    public ConfigEntry<float> ExplodeDamage { get; private set; }
    public ConfigEntry<float> ExplodeStunDuration { get; private set; }
    public ConfigEntry<float> ExplodeForceLowerBound { get; private set; }
    public ConfigEntry<float> ExplodeForceHigherBound { get; private set; }
    
    // Event: Slip
    public ConfigEntry<bool> SlipEnabled { get; private set; }
    public ConfigEntry<string> SlipKeywords { get; private set; }
    public ConfigEntry<float> SlipStunDuration { get; private set; }

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

        // Event: Transmute
        TransmuteEnabled = configFile.Bind("Event.Transmute", "Enabled", true, "Enable the Transmute event.");
        
        TransmuteMilkEnabled = configFile.Bind("Event.Transmute", "EnableMilk", true, "Enable 'milk/calcium' -> Fortified Milk");
        TransmuteCactusEnabled = configFile.Bind("Event.Transmute", "EnableCactus", true, "Enable 'cactus' -> Cactus");
        TransmuteCoconutEnabled = configFile.Bind("Event.Transmute", "EnableCoconut", true, "Enable 'coconut' -> Coconut");
        TransmuteAppleEnabled = configFile.Bind("Event.Transmute", "EnableApple", true, "Enable 'apple/berry' -> Crispberries");
        TransmuteBananaEnabled = configFile.Bind("Event.Transmute", "EnableBanana", true, "Enable 'banana' -> Peel");
        TransmuteEggEnabled = configFile.Bind("Event.Transmute", "EnableEgg", true, "Enable 'egg' -> Egg");
        TransmuteFruitEnabled = configFile.Bind("Event.Transmute", "EnableFruit", true, "Enable 'fruit' -> Random Fruit");
        TransmuteMushroomEnabled = configFile.Bind("Event.Transmute", "EnableMushroom", true, "Enable 'mushroom/fungus' -> Mushroom");
        
        // Event: Death
        DeathEnabled = configFile.Bind("Event.Death", "Enabled", true, "Enable the Death event.");
        DeathKeywords = configFile.Bind("Event.Death", "Keywords", "die, death, dead, suicide, kill, deceased, skeleton, skull, bone, perish, demise, expire, fatal, mortal, slain, dying, corpse, cadaver, lifeless, cease, extinct, eliminate, terminate, execute, obliterate, annihilate, eradicate, end, finish, doom, grave, burial, coffin, casket, tomb, crypt, reaper, grim", "List of keywords that trigger the death event, separated by commas.");

        // Event: Zombify
        ZombifyEnabled = configFile.Bind("Event.Zombify", "Enabled", true, "Enable the Zombify event.");
        ZombifyKeywords = configFile.Bind("Event.Zombify", "Keywords", "zombie, zombify, zombified, walker, ghoul, bitten, bite, brain, rot, decay, infected, infection, plague, pandemic, virus, outbreak, cannibal, flesh, meat, undead, risen, horde, apocalypse, reanimate, reanimated, lurker, creeper, crawler, groaning, groan, moan, moaning, growl, snarl", "List of keywords that trigger the zombify event, separated by commas.");

        // Event: Sleep
        SleepEnabled = configFile.Bind("Event.Sleep", "Enabled", true, "Enable the Sleep event.");
        SleepKeywords = configFile.Bind("Event.Sleep", "Keywords", "faint, sleep, exhausted, sleepy, tired, bed, nap, rest, slumber, doze, snooze, pass out, knockout, blackout, coma, narc, drowsy, unconscious, collapse, zonk, conk, yawn, fatigue, fatigued, weary, lethargic, sluggish, drained, wiped, beat, worn, spent, shut-eye, shuteye, siesta, catnap, dreamland, nodding off, drift off, lights out, out cold", "List of keywords that trigger the sleep event, separated by commas.");
        
        // Event: Drop
        DropEnabled = configFile.Bind("Event.Drop", "Enabled", true, "Enable the Drop event.");
        DropKeywords = configFile.Bind("Event.Drop", "Keywords", "drop, oops, whoops, butterfingers, fumble, release, discard, off, loss, lose, let go, slip away, misplace, clumsy, accident, unhand, relinquish, surrender, abandon, ditched, ditch, shed, cast, toss, throw away, get rid", "List of keywords that trigger the drop event, separated by commas.");

        // Event: Explode
        ExplodeEnabled = configFile.Bind("Event.Explode", "Enabled", true, "Enable the Explode event.");
        ExplodeKeywords = configFile.Bind("Event.Explode", "Keywords", "explosion, dynamite, grenade, explodes, explode, blowing, blew, blow, boom, nuke, bomb, bombs, nuclear, detonate, detonation, explosive, blast, kaboom, burst", "List of keywords that trigger the explode event, separated by commas.");
        ExplodeRadius = configFile.Bind("Event.Explode", "Radius", 6.0f, "The radius of the explosion effect and damage.");
        ExplodeDamage = configFile.Bind("Event.Explode", "DamagePercent", 0.4f, "The percentage of injury (0.0 to 1.0) applied to the player.");
        ExplodeStunDuration = configFile.Bind("Event.Explode", "StunDuration", 3.0f, "Duration in seconds the player will be stunned/ragdolled after explosion.");
        ExplodeForceLowerBound = configFile.Bind("Event.Explode", "ForceLowerBound", 2000f, "Lowest possible amount of explosion force applied.");
        ExplodeForceHigherBound = configFile.Bind("Event.Explode", "ForceHigherBound", 3000f, "Highest possible amount of explosion force applied.");
        
        // Event: Slip
        SlipEnabled = configFile.Bind("Event.Slip", "Enabled", true, "Enable the Slip event.");
        SlipKeywords = configFile.Bind("Event.Slip", "Keywords", "fuck, asshole, bastard, bitch, fag, damn, crap, slip, slide, trip, fall, fell, stumble, tumble, topple, stagger, wobble, skid, slippery, slick, peel, unbalanced, unstable, tilting, tilt", "List of keywords that trigger the slip event, separated by commas.");
        SlipStunDuration = configFile.Bind("Event.Slip", "StunDuration", 2.0f, "Duration in seconds the player will be stunned/ragdolled after slipping.");
    }
}