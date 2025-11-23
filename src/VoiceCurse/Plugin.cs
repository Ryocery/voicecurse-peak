using System.IO;
using BepInEx;
using BepInEx.Logging;
using VoiceCurse.Handlers;

namespace VoiceCurse;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin {
    private static ManualLogSource Log { get; set; } = null!;
    private Config? _config;
    private VoiceHandler? _voiceHandler;

    private void Awake() {
        Log = Logger;
        Log.LogInfo($"Plugin {Name} is loading...");
        _config = new Config(Config);
        
        string? pluginDir = Path.GetDirectoryName(Info.Location);
        if (string.IsNullOrEmpty(pluginDir)) pluginDir = Paths.PluginPath;
        _voiceHandler = new VoiceHandler(Log, _config, pluginDir);
        
        Log.LogInfo($"Plugin {Name} loaded successfully.");
    }

    private void Update() {
        _voiceHandler?.Update();
    }

    private void OnDestroy() {
        _voiceHandler?.Dispose();
    }
}