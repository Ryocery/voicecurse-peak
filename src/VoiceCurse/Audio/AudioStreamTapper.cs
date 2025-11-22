using System;
using UnityEngine;
using VoiceCurse.Core;

namespace VoiceCurse.Audio;

public class AudioStreamTapper : MonoBehaviour {
    private IVoiceRecognizer? _recognizer;
    private bool _isActive;
    private bool _muteOutput;
        
    public void Initialize(IVoiceRecognizer recognizer, bool muteOutput = false) {
        _recognizer = recognizer;
        _isActive = true;
        _muteOutput = muteOutput;
    }

    private void OnAudioFilterRead(float[] data, int channels) {
        if (!_isActive) {
            return;
        }

        int samples = data.Length / channels;
        short[] pcmBuffer = new short[samples];

        for (int i = 0; i < samples; i++) {
            float sample = 0;

            for (int c = 0; c < channels; c++) {
                sample += data[i * channels + c];
            }
            sample /= channels;

            switch (sample) {
                case > 1.0f:
                    sample = 1.0f;
                    break;
                case < -1.0f:
                    sample = -1.0f;
                    break;
            }

            pcmBuffer[i] = (short)(sample * 32767);
        }

        _recognizer?.FeedAudio(pcmBuffer, pcmBuffer.Length);
            
        if (_muteOutput) {
            Array.Clear(data, 0, data.Length);
        }
    }
}