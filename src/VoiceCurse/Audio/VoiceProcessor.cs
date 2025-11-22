using Photon.Voice;
using VoiceCurse.Core;

namespace VoiceCurse.Audio;

public class VoiceProcessor(IVoiceRecognizer recognizer) : IProcessor<float> {
    public float[]? Process(float[]? buf) {
        if (buf == null || buf.Length == 0) return buf;
        
        short[] pcmBuffer = new short[buf.Length];
        for (int i = 0; i < buf.Length; i++) {
            float sample = buf[i];
            sample = sample switch {
                > 1.0f => 1.0f,
                < -1.0f => -1.0f,
                _ => sample
            };

            pcmBuffer[i] = (short)(sample * 32767);
        }
        
        recognizer.FeedAudio(pcmBuffer, pcmBuffer.Length);
        return buf;
    }

    public void Dispose() { }
}