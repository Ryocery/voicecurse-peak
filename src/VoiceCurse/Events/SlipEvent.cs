using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace VoiceCurse.Events;

public class SlipEvent(Config config) : VoiceEventBase(config) {
    private readonly HashSet<string> _keywords = [
        "fuck", "shit", "asshole", "bastard", "bitch", "fag",
        "damn", "crap", "oops", "slip", "slide", "trip", "fall", 
        "fell"
    ];

    private static AudioClip? _cachedTripSound;
    protected override IEnumerable<string> GetKeywords() => _keywords;

    protected override bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword) {
        if (player.data.dead || player.data.fullyPassedOut) return false;
        
        player.Fall(2f); 
        Vector3 lookDir = player.data.lookDirection_Flat;
        ApplyForceToPart(player, BodypartType.Foot_R, (lookDir + Vector3.up) * 200f);
        ApplyForceToPart(player, BodypartType.Foot_L, (lookDir + Vector3.up) * 200f);
        ApplyForceToPart(player, BodypartType.Hip, Vector3.up * 1500f);
        ApplyForceToPart(player, BodypartType.Head, lookDir * -300f);

        return true;
    }

    private static void ApplyForceToPart(Character player, BodypartType partType, Vector3 force) {
        Rigidbody rb = player.GetBodypartRig(partType);
        if (rb) {
            rb.AddForce(force, ForceMode.Impulse);
        }
    }

    public override void PlayEffects(Vector3 position) {
        AudioClip? clip = GetTripSound();
        if (clip) {
            AudioSource.PlayClipAtPoint(clip, position);
        }
    }

    private static AudioClip? GetTripSound() {
        if (!_cachedTripSound) {
            _cachedTripSound = Resources.FindObjectsOfTypeAll<AudioClip>()
                .FirstOrDefault(c => c.name == "Au_Slip1");
        }
        return _cachedTripSound;
    }
}