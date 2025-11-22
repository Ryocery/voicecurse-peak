using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoiceCurse.Core;

namespace VoiceCurse.Events;

public class LaunchEvent(VoiceCurseConfig config) : VoiceEventBase(config) {
    private readonly HashSet<string> _keywords = [
        "launch", "fly", "blast", "boost", "ascend", "lift", "up"
    ];
    
    private static GameObject? _cachedLaunchSFX;

    protected override IEnumerable<string> GetKeywords() => _keywords;

    protected override bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword) {
        if (player.data.dead || player.data.fullyPassedOut) return false;

        if (_cachedLaunchSFX is null) FindLaunchSFX();

        player.Fall(3f); 
        Vector3 launchDirection = fullSentence.Contains("up") ? Vector3.up : Random.onUnitSphere;
        launchDirection.y = Mathf.Abs(launchDirection.y);
        if (launchDirection.y < 0.5f) launchDirection.y = 0.5f; 
        launchDirection.Normalize();

        float launchForce = Random.Range(1500f, 3000f); 
        Vector3 finalForce = launchDirection * launchForce;
        player.AddForce(finalForce);

        return true;
    }

    public override void PlayEffects(Vector3 position) {
        if (_cachedLaunchSFX is null) FindLaunchSFX();

        if (_cachedLaunchSFX is null) return;
        GameObject sfx = Object.Instantiate(_cachedLaunchSFX, position, Quaternion.identity);
        sfx.SetActive(true);
        Object.Destroy(sfx, 5f);
    }

    private void FindLaunchSFX() {
        ScoutCannon? cannon = Resources.FindObjectsOfTypeAll<ScoutCannon>().FirstOrDefault();
        if (cannon is null) return;
        _cachedLaunchSFX = cannon.fireSFX;
    }
}