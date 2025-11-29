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
    public ConfigEntry<bool> TransmuteDeathEnabled { get; private set; }
    
    public ConfigEntry<bool> TransmuteMilkEnabled { get; private set; }
    public ConfigEntry<bool> TransmuteCactusEnabled { get; private set; }
    public ConfigEntry<bool> TransmuteCoconutEnabled { get; private set; }
    public ConfigEntry<bool> TransmuteAppleEnabled { get; private set; }
    public ConfigEntry<bool> TransmuteBananaEnabled { get; private set; }
    public ConfigEntry<bool> TransmuteEggEnabled { get; private set; }
    public ConfigEntry<bool> TransmuteFruitEnabled { get; private set; }
    public ConfigEntry<bool> TransmuteMushroomEnabled { get; private set; }
    public ConfigEntry<bool> TransmuteBallEnabled { get; private set; }
    
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
    
    // Event: Sacrifice
    public ConfigEntry<bool> SacrificeEnabled { get; private set; }
    public ConfigEntry<string> SacrificeKeywords { get; private set; }
    public ConfigEntry<float> SacrificeCooldown { get; private set; }
    
    // Event: Blind
    public ConfigEntry<bool> BlindEnabled { get; private set; }
    public ConfigEntry<string> BlindKeywords { get; private set; }
    public ConfigEntry<float> BlindDuration { get; private set; }

    public Config(ConfigFile config) {
        // Global
        EnableDebugLogs = config.Bind("Global", "EnableDebugLogs", true, "Enable debug logs for speech detection.");
        GlobalCooldown = config.Bind("Global", "GlobalCooldown", 2.0f, "The time in seconds that must pass before another voice event can be triggered.");

        // Event: Launch
        LaunchEnabled = config.Bind("Event.Launch", "Enabled", true, "Enable the Launch event.");
        LaunchKeywords = config.Bind("Event.Launch", "Keywords", "launch, fly, boost, ascend, lift, up, cannon, canon, rocket, soar, jump, spring, catapult, fling, hurl, propel, shoot, skyrocket, takeoff, left, right, forward, back, yeet, lob, pitch, chuck, heave, airborne, levitate, hover, elevate, rise, vault, leap, hop, eject, thrust, projectile, missile, space, orbit", "List of keywords that trigger the launch event, separated by commas.");
        LaunchForceLowerBound = config.Bind("Event.Launch", "ForceLowerBound", 1500f, "Lowest possible amount of force applied to the player upon launching..");
        LaunchForceHigherBound = config.Bind("Event.Launch", "ForceHigherBound", 3000f, "Highest possible amount of force applied to the player upon launching..");
        LaunchStunDuration = config.Bind("Event.Launch", "StunDuration", 3.0f, "Duration in seconds the player will be stunned/ragdolled after launching.");

        // Event: Affliction
        AfflictionEnabled = config.Bind("Event.Affliction", "Enabled", true, "Enable the Affliction event.");
        AfflictionMinPercent = config.Bind("Event.Affliction", "MinPercent", 0.2f, "The minimum percentage (0.0 to 1.0) of the status bar to fill when triggered.");
        AfflictionMaxPercent = config.Bind("Event.Affliction", "MaxPercent", 0.6f, "The maximum percentage (0.0 to 1.0) of the status bar to fill when triggered.");
        AfflictionTemperatureSwapEnabled = config.Bind("Event.Affliction", "TemperatureSwapEnabled", true, "If enabled, attempting to get rid of hot or cold by saying the opposite will just swap the existing effect to the new one and add on top of that to prevent cheesing it.");
        
        AfflictionKeywordsInjury = config.Bind("Event.Affliction", "KeywordsInjury", "damage, hurt, injury, injured, pain, harm, wound, hit, bleed, bruise, cut, slash, orange, ache, sore, trauma, gash, scrape, laceration, tear, torn, broken, fracture, sprain, puncture, stab, maim, cripple, batter", "Keywords that trigger physical injury.");
        AfflictionKeywordsHunger = config.Bind("Event.Affliction", "KeywordsHunger", "hunger, hungry, starving, starve, food, malnourishment, famished, eat, snack, meal, yellow, appetite, crave, craving, ravenous, peckish, feast, feed, sustenance, nourishment, nutrition, consume", "Keywords that trigger hunger.");
        AfflictionKeywordsCold = config.Bind("Event.Affliction", "KeywordsCold", "freezing, cool, cold, blizzard, shiver, ice, frozen, chill, frigid, winter, blue, frost, arctic, polar, glacier, icicle, hypothermia, numb, shivering, freeze", "Keywords that trigger cold status.");
        AfflictionKeywordsHot = config.Bind("Event.Affliction", "KeywordsHot", "hot, fire, melt, scorching, heat, burn, pyro, flame, summer, cook, hell, red, sizzle, sear, swelter, boil, roast, bake, baking, scald, inferno, blaze, blazing, ignite, combust, incinerate", "Keywords that trigger hot status.");
        AfflictionKeywordsPoison = config.Bind("Event.Affliction", "KeywordsPoison", "poison, sick, vomit, toxic, venom, contaminate, purple, nausea, nauseous, intoxicate, pollute, taint, corrupt, disease, ill, ailment, malady", "Keywords that trigger poison status.");
        AfflictionKeywordsSpores = config.Bind("Event.Affliction", "KeywordsSpores", "spore, pink", "Keywords that trigger spore status.");

        // Event: Transmute
        TransmuteEnabled = config.Bind("Event.Transmute", "Enabled", true, "Enable the Transmute event.");
        TransmuteDeathEnabled = config.Bind("Event.Transmute", "EnableDeath", true, "Enable instant death when transmute is triggered (by using your flesh as something to transmute). If disabled, you will just take damage instead (your flesh partially transmutes).");
        
        TransmuteMilkEnabled = config.Bind("Event.Transmute", "EnableMilk", true, "Enable 'milk/calcium' -> Fortified Milk");
        TransmuteCactusEnabled = config.Bind("Event.Transmute", "EnableCactus", true, "Enable 'cactus' -> Cactus");
        TransmuteCoconutEnabled = config.Bind("Event.Transmute", "EnableCoconut", true, "Enable 'coconut' -> Coconut");
        TransmuteAppleEnabled = config.Bind("Event.Transmute", "EnableApple", true, "Enable 'apple/berry' -> Crispberries");
        TransmuteBananaEnabled = config.Bind("Event.Transmute", "EnableBanana", true, "Enable 'banana' -> Peel");
        TransmuteEggEnabled = config.Bind("Event.Transmute", "EnableEgg", true, "Enable 'egg' -> Egg");
        TransmuteFruitEnabled = config.Bind("Event.Transmute", "EnableFruit", true, "Enable 'fruit' -> Random Fruit");
        TransmuteMushroomEnabled = config.Bind("Event.Transmute", "EnableMushroom", true, "Enable 'mushroom/fungus' -> Mushroom");
        TransmuteBallEnabled = config.Bind("Event.Transmute", "EnableBall", false, "Enable 'ball' -> Basketball");
        
        // Event: Death
        DeathEnabled = config.Bind("Event.Death", "Enabled", true, "Enable the Death event.");
        DeathKeywords = config.Bind("Event.Death", "Keywords", "die, death, dead, suicide, kill, deceased, skeleton, skull, bone, perish, demise, expire, fatal, mortal, slain, dying, corpse, cadaver, lifeless, cease, extinct, eliminate, terminate, execute, obliterate, annihilate, eradicate, end, finish, doom, grave, burial, coffin, casket, tomb, crypt, reaper, grim", "List of keywords that trigger the death event, separated by commas.");

        // Event: Zombify
        ZombifyEnabled = config.Bind("Event.Zombify", "Enabled", true, "Enable the Zombify event.");
        ZombifyKeywords = config.Bind("Event.Zombify", "Keywords", "zombie, zombify, zombified, walker, ghoul, bitten, bite, brain, rot, decay, infected, infection, plague, pandemic, virus, outbreak, cannibal, flesh, meat, undead, risen, horde, apocalypse, reanimate, lurker, creeper, crawler, groaning, groan, moan, growl, snarl", "List of keywords that trigger the zombify event, separated by commas.");

        // Event: Sleep
        SleepEnabled = config.Bind("Event.Sleep", "Enabled", true, "Enable the Sleep event.");
        SleepKeywords = config.Bind("Event.Sleep", "Keywords", "faint, sleep, exhausted, sleepy, tired, bed, nap, rest, slumber, doze, snooze, pass out, knockout, blackout, coma, narc, drowsy, unconscious, collapse, zonk, conk, yawn, fatigue, fatigued, weary, lethargic, sluggish, drained, wiped, beat, worn, spent, shut-eye, shuteye, siesta, catnap, dreamland, nodding off, drift off, lights out, out cold", "List of keywords that trigger the sleep event, separated by commas.");
        
        // Event: Drop
        DropEnabled = config.Bind("Event.Drop", "Enabled", true, "Enable the Drop event.");
        DropKeywords = config.Bind("Event.Drop", "Keywords", "drop, oops, whoops, butterfingers, fumble, release, discard, off, loss, lose, let go, slip away, misplace, clumsy, accident, unhand, relinquish, surrender, abandon, ditch, shed, cast, toss, throw away, get rid", "List of keywords that trigger the drop event, separated by commas.");

        // Event: Explode
        ExplodeEnabled = config.Bind("Event.Explode", "Enabled", true, "Enable the Explode event.");
        ExplodeKeywords = config.Bind("Event.Explode", "Keywords", "explosion, dynamite, grenade, explode, blow, blast, boom, nuke, bomb, nuclear, detonate, detonation, explosive, kaboom, burst", "List of keywords that trigger the explode event, separated by commas.");
        ExplodeRadius = config.Bind("Event.Explode", "Radius", 6.0f, "The radius of the explosion effect and damage.");
        ExplodeDamage = config.Bind("Event.Explode", "DamagePercent", 0.4f, "The percentage of injury (0.0 to 1.0) applied to the player.");
        ExplodeStunDuration = config.Bind("Event.Explode", "StunDuration", 3.0f, "Duration in seconds the player will be stunned/ragdolled after explosion.");
        ExplodeForceLowerBound = config.Bind("Event.Explode", "ForceLowerBound", 2000f, "Lowest possible amount of explosion force applied.");
        ExplodeForceHigherBound = config.Bind("Event.Explode", "ForceHigherBound", 3000f, "Highest possible amount of explosion force applied.");
        
        // Event: Slip
        SlipEnabled = config.Bind("Event.Slip", "Enabled", true, "Enable the Slip event.");
        SlipKeywords = config.Bind("Event.Slip", "Keywords", "fuck, asshole, bastard, bitch, fag, damn, crap, slip, slide, trip, fall, fell, stumble, tumble, topple, stagger, wobble, skid, slick, peel, unbalanced, unstable, tilt", "List of keywords that trigger the slip event, separated by commas.");
        SlipStunDuration = config.Bind("Event.Slip", "StunDuration", 2.0f, "Duration in seconds the player will be stunned/ragdolled after slipping.");
        
        // Event: Sacrifice
        SacrificeEnabled = config.Bind("Event.Sacrifice", "Enabled", true, "Enable the Sacrifice event.");
        SacrificeKeywords = config.Bind("Event.Sacrifice", "Keywords", "sacrifice, trade, revive, resurrect, exchange, soul, offer", "List of keywords that trigger the sacrifice event.");
        SacrificeCooldown = config.Bind("Event.Sacrifice", "Cooldown", 300f, "Cooldown in seconds for the sacrifice event.");
        
        // Event: Blind
        BlindEnabled = config.Bind("Event.Blind", "Enabled", true, "Enable the Blind event.");
        BlindKeywords = config.Bind("Event.Blind", "Keywords", "blind, flash, eyes, vision, cant see, darkness, my eyes", "List of keywords that trigger the blind event.");
        BlindDuration = config.Bind("Event.Blind", "Duration", 15.0f, "Duration in seconds the player will be blinded.");
    }
}