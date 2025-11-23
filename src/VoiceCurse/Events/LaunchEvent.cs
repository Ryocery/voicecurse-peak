using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoiceCurse.Core;

namespace VoiceCurse.Events;

public class LaunchEvent(Config config) : VoiceEventBase(config) {
    private readonly HashSet<string> _keywords = [
        "launch", "fly", "blast", "boost", "ascend", "lift", "up", 
        "cannon", "canon", "rocket", "soar", "jump", "spring", "catapult",
        "fling", "hurl", "propel", "shoot", "skyrocket", "takeoff",
        "left", "right", "forward", "forwards", "backward", "backwards", "back"
    ];
    
    private static GameObject? _cachedLaunchSFX;

    protected override IEnumerable<string> GetKeywords() => _keywords;

    protected override bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword) {
        if (player.data.dead || player.data.fullyPassedOut) return false;
        if (_cachedLaunchSFX is null) FindLaunchSFX();
        
        player.Fall(3f);

        Vector3 launchDirection = fullSentence switch {
            _ when fullSentence.Contains("left") => -player.transform.right + Vector3.up * 0.2f,
            _ when fullSentence.Contains("right") => player.transform.right + Vector3.up * 0.2f,
            _ when fullSentence.Contains("backward") || fullSentence.Contains("backwards") || fullSentence.Contains("back") => -player.transform.forward + Vector3.up * 0.2f,
            _ when fullSentence.Contains("forward") || fullSentence.Contains("forwards") => player.transform.forward + Vector3.up * 0.2f,
            _ when fullSentence.Contains("up") => Vector3.up,
            _ => GetRandomUpwardDirection()
        };

        launchDirection.Normalize();
        float launchForce = Random.Range(1500f, 3000f); 
        Vector3 finalForce = launchDirection * launchForce;
        player.AddForce(finalForce);

        return true;
    }
    
    private static Vector3 GetRandomUpwardDirection() {
        Vector3 direction = Random.onUnitSphere;
        direction.y = Mathf.Abs(direction.y);
        if (direction.y < 0.5f) direction.y = 0.5f;
        return direction;
    }

    public override void PlayEffects(Vector3 position) {
        if (_cachedLaunchSFX is null) FindLaunchSFX();
        if (_cachedLaunchSFX is null) return;
        GameObject sfx = Object.Instantiate(_cachedLaunchSFX, position, Quaternion.identity);
        sfx.SetActive(true);
        Object.Destroy(sfx, 5f);
    }

    private static void FindLaunchSFX() {
        ScoutCannon? cannon = Resources.FindObjectsOfTypeAll<ScoutCannon>().FirstOrDefault();
        if (cannon is null) return;
        _cachedLaunchSFX = cannon.fireSFX;
    }
}