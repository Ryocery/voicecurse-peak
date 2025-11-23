using System.IO;
using BepInEx;
using BepInEx.Logging;
using VoiceCurse.Core;

namespace VoiceCurse;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin {
    private static ManualLogSource Log { get; set; } = null!;
    private Config? _config;
    private VoiceCurseManager? _manager;

    private void Awake() {
        Log = Logger;
        Log.LogInfo($"Plugin {Name} is loading...");
        _config = new Config(Config);
        
        string? pluginDir = Path.GetDirectoryName(Info.Location);
        if (string.IsNullOrEmpty(pluginDir)) pluginDir = Paths.PluginPath;
        
        _manager = new VoiceCurseManager(Log, _config, pluginDir);
    }

    private void Update() {
        _manager?.Update();
    }

    private void OnDestroy() {
        _manager?.Dispose();
    }
}