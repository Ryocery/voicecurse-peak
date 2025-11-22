using System;
using System.Collections.Generic;
using VoiceCurse.Events;

namespace VoiceCurse.Core {
    public class VoiceEventHandler : IDisposable {
        private readonly IVoiceRecognizer _recognizer;
        private readonly List<IVoiceEvent> _events;

        public VoiceEventHandler(VoiceCurseConfig config, IVoiceRecognizer recognizer) {
            _recognizer = recognizer;
            _recognizer.OnPartialResult += HandleSpeech;
            _recognizer.OnPhraseRecognized += HandleSpeech;
            
            _events = [
                new InstantDeathEvent(config),
                new AfflictionEvent(config)
            ];
        }

        private void HandleSpeech(string text) {
            if (string.IsNullOrWhiteSpace(text)) return;

            string lowerText = text.ToLowerInvariant();
            
            foreach (IVoiceEvent? evt in _events) {
                evt.TryExecute(lowerText, lowerText);
            }
        }

        public void Dispose() {
            _recognizer.OnPartialResult -= HandleSpeech;
            _recognizer.OnPhraseRecognized -= HandleSpeech;
        }
    }
}