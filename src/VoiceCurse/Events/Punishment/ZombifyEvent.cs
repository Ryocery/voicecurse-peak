using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;

namespace VoiceCurse.Events.Punishment;

public class ZombifyEvent(Config config) : VoiceEventBase(config) {
    private readonly HashSet<string> _zombifyKeywords = ParseKeywords(config.ZombifyKeywords.Value);

    private static HashSet<string> ParseKeywords(string configLine) {
        return configLine
            .Split([','], StringSplitOptions.RemoveEmptyEntries)
            .Select(k => k.Trim().ToLowerInvariant())
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .ToHashSet();
    }

    protected override IEnumerable<string> GetKeywords() {
        return Config.ZombifyEnabled.Value ? _zombifyKeywords : Enumerable.Empty<string>();
    }

    protected override bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword) {
        if (!Config.ZombifyEnabled.Value) return false;
        if (player.data.dead || player.data.zombified) return false;
        
        player.photonView.RPC("RPCA_Zombify", RpcTarget.All, player.Center);
        return true;
    }
}