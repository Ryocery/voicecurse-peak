using UnityEngine;

namespace VoiceCurse.Events;

public interface IVoiceEvent {
    bool TryExecute(string spokenWord, string fullSentence);
    void PlayEffects(Vector3 position); 
}