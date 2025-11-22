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

        if (_cachedLaunchSFX is null) {
            FindLaunchSFX();
        }

        player.Fall(3f); 

        Vector3 launchDirection = fullSentence.Contains("up") ? Vector3.up : Random.onUnitSphere;
        
        launchDirection.y = Mathf.Abs(launchDirection.y);
        if (launchDirection.y < 0.5f) launchDirection.y = 0.5f; 
        launchDirection.Normalize();

        float launchForce = Random.Range(1500f, 3000f); 
        Vector3 finalForce = launchDirection * launchForce;
        player.AddForce(finalForce);

        if (_cachedLaunchSFX != null) {
            GameObject sfx = Object.Instantiate(_cachedLaunchSFX, player.Center, Quaternion.identity);
            sfx.SetActive(true);
            Object.Destroy(sfx, 5f);
        }

        return true;
    }

    private void FindLaunchSFX() {
        ScoutCannon? cannon = Resources.FindObjectsOfTypeAll<ScoutCannon>().FirstOrDefault();
        
        if (cannon is null) {
            if (Config.EnableDebugLogs.Value) Debug.LogWarning("[VoiceCurse] Could not find ScoutCannon to steal SFX from!");
            return;
        }
        
        _cachedLaunchSFX = cannon.fireSFX;
        
        if (_cachedLaunchSFX is not null && Config.EnableDebugLogs.Value) {
             Debug.Log("[VoiceCurse] Successfully stole ScoutCannon fire SFX");
        }
    }
}