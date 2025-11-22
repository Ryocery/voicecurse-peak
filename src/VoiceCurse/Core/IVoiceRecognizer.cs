using System;

namespace VoiceCurse.Core {
    public interface IVoiceRecognizer : IDisposable {
        event Action<string> OnPhraseRecognized;
        event Action<string> OnPartialResult;
        
        void FeedAudio(short[] pcmData, int length);
        void Start();
        void Stop();
        void Update();
    }
}