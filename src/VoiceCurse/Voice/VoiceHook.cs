using System.Reflection;
using Photon.Voice;
using Photon.Voice.Unity;
using UnityEngine;
using VoiceCurse.Handlers;

namespace VoiceCurse.Voice;

public class VoiceHook : MonoBehaviour {
    private VoiceCurseManager? _manager;
    private Recorder? _recorder;
    private bool _hasInjected;

    public void Initialize(VoiceCurseManager manager, Recorder recorder) {
        _manager = manager;
        _recorder = recorder;
        CheckIfReady();
    }

    private void Update() {
        if (!_hasInjected) {
            CheckIfReady();
        }
    }

    private void CheckIfReady() {
        if (!_recorder || _manager == null) return;
        if (!_recorder.IsCurrentlyTransmitting) return;
        
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        FieldInfo? field = typeof(Recorder).GetField("voice", flags);

        if (field == null) return;
        LocalVoice? voice = field.GetValue(_recorder) as LocalVoice;
        if (voice == null) return;
        
        _manager.OnPhotonVoiceReady(_recorder, voice);
        _hasInjected = true;
    }
    
    private void PhotonVoiceCreated(PhotonVoiceCreatedParams p) {
        if (_hasInjected) return;
        if (_recorder == null) return;
        
        _manager?.OnPhotonVoiceReady(_recorder, p.Voice);
        _hasInjected = true;
    }
}