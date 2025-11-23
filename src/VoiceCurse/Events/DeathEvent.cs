using System.Collections.Generic;
using Photon.Pun;

namespace VoiceCurse.Events;

public class DeathEvent(Config config) : VoiceEventBase(config) {
    private readonly HashSet<string> _deathKeywords = [
        "die", "death", "dead", "suicide", "kill", "deceased", "skeleton", 
        "skull", "bones", "bone", "perish", "demise", "expire", "expired", 
        "fatal", "mortality", "mortal", "died", "slain", "dying"
    ];

    protected override IEnumerable<string> GetKeywords() => _deathKeywords;

    protected override bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword) {
        if (player.data.dead) return false;
        player.photonView.RPC("RPCA_Die", RpcTarget.All, player.Center);
        return true;
    }
}