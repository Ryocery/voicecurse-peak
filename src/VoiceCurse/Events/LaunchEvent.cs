using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace VoiceCurse.Events;

public class LaunchEvent(Config config) : VoiceEventBase(config) {
    private readonly HashSet<string> _keywords = ParseKeywords(config.LaunchKeywords.Value);
    private static GameObject? _cachedLaunchSFX;

    private static HashSet<string> ParseKeywords(string configLine) {
        return configLine
            .Split([','], System.StringSplitOptions.RemoveEmptyEntries)
            .Select(k => k.Trim().ToLowerInvariant())
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .ToHashSet();
    }

    protected override IEnumerable<string> GetKeywords() {
        return Config.LaunchEnabled.Value ? _keywords : Enumerable.Empty<string>();
    }

    protected override bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword) {
        if (!Config.LaunchEnabled.Value) return false;
        if (player.data.dead || player.data.fullyPassedOut) return false;
        
        if (_cachedLaunchSFX is null) FindLaunchSFX();
        player.Fall(Config.LaunchStunDuration.Value);
        
        Vector3 forwardDir = player.data.lookDirection_Flat.normalized;
        Vector3 rightDir = Vector3.Cross(Vector3.up, forwardDir);
        Vector3 launchDirection;
        string directionName;
        
        if (fullSentence.Contains("left")) {
            directionName = "Left";
            launchDirection = -rightDir + Vector3.up * 0.2f;
        } 
        else if (fullSentence.Contains("right")) {
            directionName = "Right";
            launchDirection = rightDir + Vector3.up * 0.2f;
        }
        else if (fullSentence.Contains("backward") || fullSentence.Contains("backwards") || fullSentence.Contains("back")) {
            directionName = "Back";
            launchDirection = -forwardDir + Vector3.up * 0.2f;
        }
        else if (fullSentence.Contains("forward") || fullSentence.Contains("forwards")) {
            directionName = "Forward";
            launchDirection = forwardDir + Vector3.up * 0.2f;
        }
        else if (fullSentence.Contains("up")) {
            directionName = "Up";
            launchDirection = Vector3.up;
        }
        else {
            directionName = "Random";
            launchDirection = GetRandomUpwardDirection();
        }
        
        ExecutionDetail = directionName;

        launchDirection.Normalize();
        float launchForce = Random.Range(Config.LaunchForceLowerBound.Value, Config.LaunchForceHigherBound.Value);
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