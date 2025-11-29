using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;

namespace VoiceCurse.Events;

public class DeathEvent(Config config) : VoiceEventBase(config) {
    private readonly HashSet<string> _deathKeywords = ParseKeywords(config.DeathKeywords.Value);

    protected override IEnumerable<string> GetKeywords() {
        return Config.DeathEnabled.Value ? _deathKeywords : Enumerable.Empty<string>();
    }

    protected override bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword) {
        if (!Config.DeathEnabled.Value) return false;
        if (player.data.dead) return false;
        
        player.photonView.RPC("RPCA_Die", RpcTarget.All, player.Center);
        return true;
    }
}