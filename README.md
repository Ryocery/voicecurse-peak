# VoiceCurse

Mod that triggers gameplay events based on what players are saying.
It uses an offline voice recognition library to detect you saying specific words or phrases, which then activate various in-game effects.

## Features

The keywords themselves can all be configured to your own preference, although there is a lot of them. First, let's go over all possible events.

| Events     | Keyword Examples                    | Effect                                                                               |
|------------|-------------------------------------|--------------------------------------------------------------------------------------|
| Affliction | "hot", "cold", "ill", "sick"        | Gives you the affliction of the related keyword.                                     |
| Death      | "die", "end", "bones", "skeleton"   | Kills you instantly.                                                                 |
| Drop       | "oops", "release", "off", "lose"    | Drops all your items, including the items in your backpack.                          |
| Explode    | "blow", "explode", "blast", "nuke"  | Makes you explode instantly, this causes damage to your surroundings too.            |
| Launch     | "up", "left", "right", "cannon"     | Launches you in a specified direction if provided, otherwise randomly.               |
| Sleep      | "rest", "tired", "exhausted", "nap" | Makes you pass out instantly.                                                        |
| Slip       | "crap", "damn",  "trip", "fall"     | Causes you to trip like stepping on a banana.                                        |
| Transmute  | "milk", "fruit", "apple", "banana"  | Causes you and your items to tranform into related objects, this kills you instantly |
| Zombify    | "rot", "zombie", "ghoul", "bite"    | Turns you into a zombie.                                                             |

> ### Note
> Note that the events are triggered when it detects the keyword *anywhere* in the spoken sentence.
> This means that saying words like "gro**up**" will trigger the "up" launch event because "up" is contained within "group".
> This applies to every keyword and is not functionality that can be disabled.
> 
> This means that saying safe words like "n**ice**" can trigger the cold affliction event because "ice" is a keyword. There are many such examples and this is the central challenge of using this mod.

## Configuration

All keywords can be configured (except of Transmute, which operates a bit differently), all events can be enabled or disabled, and certain aspects of events can be customized.

You can modify the following settings for each event:

* **Enabled:** Toggle specific events on or off.
* **Keywords:** Custom comma-separated lists of trigger words for every event.
* **Forces & Durations:** Adjust launch forces, explosion radius, stun durations, and damage percentages.
* **Specific Toggles:** Enable or disable specific transmutation rules or mechanics.

The configuration applies only to yourself and what you want to happen to you when you say the keywords.

## Installation

1.  Ensure BepInEx is installed.
2.  Download the latest release of VoiceCurse.
3.  Extract the contents into your game's `BepInEx/plugins` folder.
4.  Run the game once to generate the configuration file.

## Everyone needs to have the mod installed for it to work properly!