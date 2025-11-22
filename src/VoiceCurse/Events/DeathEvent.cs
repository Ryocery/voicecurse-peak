using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using VoiceCurse.Core;

namespace VoiceCurse.Events;

public class DeathEvent(VoiceCurseConfig config) : VoiceEventBase(config) {
    private readonly HashSet<string> _keywords = [
        "die", "death", "dead", "suicide", "kill", "deceased", "skeleton", 
        "skull", "calcium", "bones", "bone", "perish", "demise", "expire", 
        "expired", "fatal", "mortality", "mortal", "milk"
    ];

    private static Item? _cachedMilkItem;

    protected override IEnumerable<string> GetKeywords() => _keywords;

    protected override bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword) {
        if (player.data.dead) return false;

        if (fullSentence.Contains("milk")) {
            SpawnMilk(player);
        }
            
        player.photonView.RPC("RPCA_Die", RpcTarget.All, player.Center);
        return true;
    }

    private void SpawnMilk(Character player) {
        _cachedMilkItem ??= Resources.FindObjectsOfTypeAll<Item>().FirstOrDefault(i => i.name.Contains("Milk") || (i.UIData != null && i.UIData.itemName.Contains("Milk")));
        if (_cachedMilkItem is null) return;
        
        int itemCount = 0;
        if (player.player?.itemSlots != null) {
            itemCount += player.player.itemSlots.Count(slot => !slot.IsEmpty());
        }
        if (player.player?.backpackSlot is { hasBackpack: true }) {
            if (player.player.backpackSlot.data.TryGetDataEntry(DataEntryKey.BackpackData, out BackpackData backpackData)) {
                itemCount += backpackData.FilledSlotCount();
            }
        }
        
        if (itemCount == 0) {
            player.refs.items.SpawnItemInHand(_cachedMilkItem.name);
        } else {
            foreach (ItemSlot? slot in player.player!.itemSlots) slot.EmptyOut();
            if (player.player.backpackSlot is { hasBackpack: true } && 
                player.player.backpackSlot.data.TryGetDataEntry(DataEntryKey.BackpackData, out BackpackData bpData)) {
                foreach (ItemSlot? slot in bpData.itemSlots) slot.EmptyOut();
            }
            for (int i = 0; i < itemCount; i++) {
                player.refs.items.SpawnItemInHand(_cachedMilkItem.name);
            }
        }
    }
}