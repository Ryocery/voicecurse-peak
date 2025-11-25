using UnityEngine;

namespace VoiceCurse.Interfaces;

public interface IVoiceEvent {
    bool TryExecute(string spokenWord, string fullSentence);
    void PlayEffects(Vector3 position);
    void PlayEffects(Character origin, Vector3 position);
}