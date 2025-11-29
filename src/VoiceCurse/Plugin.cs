using System.IO;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using VoiceCurse.Handlers;

namespace VoiceCurse;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin {
    private static ManualLogSource Log { get; set; } = null!;
    private Config? _config;
    private VoiceHandler? _voiceHandler;
    private Harmony? _harmony;

    private void Awake() {
        Log = Logger;
        Log.LogInfo($"Plugin {Name} is loading...");
        _config = new Config(Config);
        
        _harmony = new Harmony(Id);
        _harmony.PatchAll();
        Log.LogInfo("Harmony Patches applied.");

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
        _harmony?.UnpatchSelf();
    }
}