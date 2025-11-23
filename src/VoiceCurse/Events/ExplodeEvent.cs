using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoiceCurse.Events;

public class ExplodeEvent(Config config) : VoiceEventBase(config) {
    private readonly HashSet<string> _keywords = [
        "explosion", "dynamite", "grenade", "explodes", "explode", 
        "blowing", "blew", "blow", "boom", "nuke", "bomb", "bombs", 
        "nuclear", "detonate", "detonation", "explosive", "blast",
        "kaboom"
    ];
        
    private static GameObject? _cachedExplosionPrefab;
    private const float ExplosionRadius = 6.0f; 
    protected override IEnumerable<string> GetKeywords() => _keywords;

    protected override bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword) {
        if (player.data.dead) return false;
        return true;
    }
    
    public override void PlayEffects(Vector3 position) {
        if (!_cachedExplosionPrefab) FindExplosionPrefab();
        
        if (_cachedExplosionPrefab) {
            Object.Instantiate(_cachedExplosionPrefab, position, Quaternion.identity);
        }

        Character local = Character.localCharacter;

        if (!local || local.data.dead) return;
        float distance = Vector3.Distance(local.Center, position);

        if (!(distance <= ExplosionRadius)) return;
        if (local.refs.afflictions) {
            local.refs.afflictions.AddStatus(CharacterAfflictions.STATUSTYPE.Injury, 0.4f);
        }

        local.Fall(3f); 
        Vector3 launchDirection = (local.Center - position).normalized;
        launchDirection += Vector3.up * 0.6f;
        launchDirection.Normalize();
        float launchForce = Random.Range(2000f, 3000f); 
        local.AddForce(launchDirection * launchForce);
    }

    private static void FindExplosionPrefab() {
        Dynamite? dynamiteRef = Resources.FindObjectsOfTypeAll<Dynamite>().FirstOrDefault();
        if (dynamiteRef) {
            _cachedExplosionPrefab = dynamiteRef.explosionPrefab;
        }
    }
}