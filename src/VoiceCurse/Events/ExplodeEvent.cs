using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoiceCurse.Core;

namespace VoiceCurse.Events {
    public class ExplodeEvent(VoiceCurseConfig config) : IVoiceEvent {
        private readonly HashSet<string> _keywords = ["explosion", "explode", "blowing", "blew", "blow", "boom"];
        private static GameObject? _cachedExplosionPrefab;

        public bool TryExecute(string spokenWord, string fullSentence) {
            if (!_keywords.Contains(spokenWord)) return false;

            Character localChar = Character.localCharacter;
            if (localChar is null || localChar.data.dead) return false;

            if (config.EnableDebugLogs.Value) {
                Debug.Log($"[VoiceCurse] Explosion triggered by '{spokenWord}'");
            }
            
            if (_cachedExplosionPrefab is null) {
                FindExplosionPrefab();
            }

            if (_cachedExplosionPrefab is null) return true;
            Object.Instantiate(_cachedExplosionPrefab, localChar.Center, Quaternion.identity);
            localChar.refs.afflictions.AddStatus(CharacterAfflictions.STATUSTYPE.Injury, 0.7f);
            localChar.AddForce(Vector3.up * 10f + Random.insideUnitSphere * 40f);
            return true;
        }

        private void FindExplosionPrefab() {
            Dynamite? dynamiteRef = Resources.FindObjectsOfTypeAll<Dynamite>().FirstOrDefault();
            if (dynamiteRef is null) return;
            _cachedExplosionPrefab = dynamiteRef.explosionPrefab;
        }
    }
}