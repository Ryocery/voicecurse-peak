using System.Reflection;
using Photon.Voice;
using Photon.Voice.Unity;
using UnityEngine;

namespace VoiceCurse;

public class VoiceCurseHook : MonoBehaviour {
    private Plugin? _plugin;
    private Recorder? _recorder;
    private bool _hasInjected;

    public void Initialize(Plugin plugin, Recorder recorder) {
        _plugin = plugin;
        _recorder = recorder;
        CheckIfReady();
    }

    private void Update() {
        if (!_hasInjected) {
            CheckIfReady();
        }
    }

    private void CheckIfReady() {
        if (_recorder == null || _plugin == null) return;
        if (!_recorder.IsCurrentlyTransmitting) return;
        
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        FieldInfo? field = typeof(Recorder).GetField("voice", flags);

        if (field == null) return;
        LocalVoice? voice = field.GetValue(_recorder) as LocalVoice;
        if (voice == null) return;
        
        _plugin.OnPhotonVoiceReady(_recorder, voice);
        _hasInjected = true;
    }
    
    private void PhotonVoiceCreated(PhotonVoiceCreatedParams p) {
        if (_hasInjected) return;
        if (_recorder == null) return;
        
        _plugin?.OnPhotonVoiceReady(_recorder, p.Voice);
        _hasInjected = true;
    }
}