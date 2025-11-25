using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace VoiceCurse.Events;

public class ExplodeEvent(Config config) : VoiceEventBase(config) {
    private readonly HashSet<string> _keywords = ParseKeywords(config.ExplodeKeywords.Value);
    private static GameObject? _cachedExplosionPrefab;

    private static HashSet<string> ParseKeywords(string configLine) {
        return configLine
            .Split([','], StringSplitOptions.RemoveEmptyEntries)
            .Select(k => k.Trim().ToLowerInvariant())
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .ToHashSet();
    }
        
    protected override IEnumerable<string> GetKeywords() {
        return Config.ExplodeEnabled.Value ? _keywords : Enumerable.Empty<string>();
    }

    protected override bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword) {
        if (!Config.ExplodeEnabled.Value) return false;
        return !player.data.dead;
    }
    
    public override void PlayEffects(Vector3 position) {
        if (!_cachedExplosionPrefab) FindExplosionPrefab();
        
        if (_cachedExplosionPrefab) {
            Object.Instantiate(_cachedExplosionPrefab, position, Quaternion.identity);
        }

        Character local = Character.localCharacter;

        if (!local || local.data.dead) return;
        float distance = Vector3.Distance(local.Center, position);
        if (!(distance <= Config.ExplodeRadius.Value)) return;
        
        if (local.refs.afflictions) {
            local.refs.afflictions.AddStatus(CharacterAfflictions.STATUSTYPE.Injury, Config.ExplodeDamage.Value);
        }

        local.Fall(Config.ExplodeStunDuration.Value); 
        
        Vector3 launchDirection = (local.Center - position).normalized;
        launchDirection += Vector3.up * 0.6f;
        launchDirection.Normalize();
        
        float launchForce = Random.Range(Config.ExplodeForceLowerBound.Value, Config.ExplodeForceHigherBound.Value); 
        local.AddForce(launchDirection * launchForce);
    }

    private static void FindExplosionPrefab() {
        Dynamite? dynamiteRef = Resources.FindObjectsOfTypeAll<Dynamite>().FirstOrDefault();
        if (dynamiteRef) {
            _cachedExplosionPrefab = dynamiteRef.explosionPrefab;
        }
    }
}