namespace VoiceCurse.Events {
    public interface IVoiceEvent {
        bool TryExecute(string spokenWord, string fullSentence);
    }
}