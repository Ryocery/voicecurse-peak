using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoiceCurse.Events;

public class BlindEvent(Config config) : VoiceEventBase(config) {
    private readonly HashSet<string> _keywords = ParseKeywords(config.BlindKeywords.Value);
    
    private float _blindEndTime;
    private Coroutine? _blindRoutine;

    protected override IEnumerable<string> GetKeywords() {
        return Config.BlindEnabled.Value ? _keywords : Enumerable.Empty<string>();
    }

    protected override bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword) {
        if (!Config.BlindEnabled.Value) return false;
        if (player.data.dead) return false;

        float duration = Config.BlindDuration.Value;
        _blindEndTime = Time.time + duration;
        
        if (_blindRoutine == null) {
            ToggleBlindness(player, true);
            _blindRoutine = player.StartCoroutine(BlindnessRoutine(player));
        }
        
        player.AddIllegalStatus("BLIND", duration); 
        return true;
    }

    private IEnumerator BlindnessRoutine(Character player) {
        while (Time.time < _blindEndTime) {
            if (!player || player.data.dead) break; 
            yield return null;
        }

        if (player) ToggleBlindness(player, false);
        _blindRoutine = null;
    }

    private static void ToggleBlindness(Character player, bool enable) {
        if (!player || player.refs == null || !player.refs.customization || !player.refs.customization.refs) return;
        Renderer? blindRenderer = player.refs.customization.refs.blindRenderer;
        if (blindRenderer) {
            blindRenderer.gameObject.SetActive(enable);
        }
    }
}