using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoiceCurse.Core;

namespace VoiceCurse.Events;

public class ExplodeEvent(VoiceCurseConfig config) : VoiceEventBase(config) {
    private readonly HashSet<string> _keywords = [
        "explosion", "dynamite", "grenade", "explodes", "explode", 
        "blowing", "blew", "blow", "boom", "nuke", "bomb", "bombs", 
        "nuclear", "detonate", "detonation"
    ];
        
    private static GameObject? _cachedExplosionPrefab;

    protected override IEnumerable<string> GetKeywords() => _keywords;

    protected override bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword) {
        if (player.data.dead) return false;

        if (_cachedExplosionPrefab is null) {
            FindExplosionPrefab();
        }

        if (_cachedExplosionPrefab is null) return true;
        
        Object.Instantiate(_cachedExplosionPrefab, player.Center, Quaternion.identity);
        player.refs.afflictions.AddStatus(CharacterAfflictions.STATUSTYPE.Injury, 0.7f);
            
        player.Fall(3f); 
        Vector3 launchDirection = Random.onUnitSphere;
        launchDirection.y = Mathf.Abs(launchDirection.y);
        if (launchDirection.y < 0.5f) launchDirection.y = 0.5f; 
        launchDirection.Normalize();

        float launchForce = Random.Range(1500f, 3000f); 
        Vector3 finalForce = launchDirection * launchForce;
        player.AddForce(finalForce);

        return true;
    }

    private void FindExplosionPrefab() {
        Dynamite? dynamiteRef = Resources.FindObjectsOfTypeAll<Dynamite>().FirstOrDefault();
        if (dynamiteRef is null) return;
        _cachedExplosionPrefab = dynamiteRef.explosionPrefab;
    }
}