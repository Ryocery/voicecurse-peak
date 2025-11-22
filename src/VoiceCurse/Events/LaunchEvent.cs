using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoiceCurse.Core;

namespace VoiceCurse.Events;

public class LaunchEvent(VoiceCurseConfig config) : IVoiceEvent {
    private readonly HashSet<string> _keywords = [
        "launch", "fly", "blast", "boost", "ascend", "lift", "up"
    ];

    private static GameObject? _cachedLaunchSFX;

    public bool TryExecute(string spokenWord, string fullSentence) {
        if (!_keywords.Contains(spokenWord)) return false;

        Character localChar = Character.localCharacter;
        if (localChar is null || localChar.data.dead || localChar.data.fullyPassedOut) return false;

        if (config.EnableDebugLogs.Value) {
            Debug.Log($"[VoiceCurse] Launch triggered by '{spokenWord}'");
        }
        
        if (_cachedLaunchSFX is null) {
            FindLaunchSFX();
        }

        localChar.Fall(3f); 
        Vector3 launchDirection = fullSentence.Contains("up") ? Vector3.up : Random.onUnitSphere;
        launchDirection.y = Mathf.Abs(launchDirection.y);
        if (launchDirection.y < 0.5f) launchDirection.y = 0.5f; 
        launchDirection.Normalize();

        float launchForce = Random.Range(1500f, 3000f); 
        Vector3 finalForce = launchDirection * launchForce;
        localChar.AddForce(finalForce);
        
        if (_cachedLaunchSFX is not null) {
            GameObject sfx = Object.Instantiate(_cachedLaunchSFX, localChar.Center, Quaternion.identity);
            sfx.SetActive(true);
            
            Object.Destroy(sfx, 5f);
        }

        return true;
    }

    private void FindLaunchSFX() {
        ScoutCannon? cannon = Resources.FindObjectsOfTypeAll<ScoutCannon>().FirstOrDefault();
        
        if (cannon is null) {
            if (config.EnableDebugLogs.Value) Debug.LogWarning("[VoiceCurse] Could not find ScoutCannon to steal SFX from!");
            return;
        }
        
        _cachedLaunchSFX = cannon.fireSFX;
        
        if (_cachedLaunchSFX is not null && config.EnableDebugLogs.Value) {
             Debug.Log("[VoiceCurse] Successfully stole ScoutCannon fire SFX");
        }
    }
}