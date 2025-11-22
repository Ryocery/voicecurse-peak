using System.Collections.Generic;
using VoiceCurse.Core;

namespace VoiceCurse.Events;

public class SleepEvent(VoiceCurseConfig config) : IVoiceEvent {
    private readonly HashSet<string> _triggerWords = ["faint", "sleep", "exhausted", "sleepy", "tired", "bed"];
}