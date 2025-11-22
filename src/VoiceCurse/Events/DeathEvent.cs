using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using VoiceCurse.Core;
using Zorro.Core;

namespace VoiceCurse.Events;

public class DeathEvent(VoiceCurseConfig config) : IVoiceEvent {
    private readonly HashSet<string> _triggerWords = [
        "die", "death", "dead", "suicide", "kill", "deceased", "skeleton", 
        "skull", "calcium", "bones", "bone", "perish", "demise", "expire", 
        "fatal", "mortality", "mortal", "milk"];

    private static Item? _cachedMilkItem;

    public bool TryExecute(string spokenWord, string fullSentence) {
        bool match = _triggerWords.Any(fullSentence.Contains);
        if (!match) return false;
        
        Character localChar = Character.localCharacter;
        if (localChar is null || localChar.data.dead) return false;

        if (config.EnableDebugLogs.Value) {
            Debug.Log($"[VoiceCurse] Death triggered by phrase: '{fullSentence}'");
        }
        
        if (fullSentence.Contains("milk")) {
            SpawnMilk(localChar);
        }
            
        localChar.photonView.RPC("RPCA_Die", RpcTarget.All, localChar.Center);
        return true;
    }

    private void SpawnMilk(Character player) {
        _cachedMilkItem ??= Resources.FindObjectsOfTypeAll<Item>()
            .FirstOrDefault(i => i.name.Contains("Milk") || (i.UIData != null && i.UIData.itemName.Contains("Milk")));

        if (_cachedMilkItem is not null) {
            int itemCount = 0;
            
            if (player.player?.itemSlots != null) {
                itemCount += player.player.itemSlots.Count(slot => !slot.IsEmpty());
            }
            
            if (player.player?.backpackSlot != null && player.player.backpackSlot.hasBackpack) {
                if (player.player.backpackSlot.data.TryGetDataEntry(DataEntryKey.BackpackData, out BackpackData backpackData)) {
                    itemCount += backpackData.FilledSlotCount();
                }
            }
            
            if (itemCount == 0) {
                player.refs.items.SpawnItemInHand(_cachedMilkItem.name);
            } 
            else {
                foreach (ItemSlot? slot in player.player!.itemSlots) {
                    slot.EmptyOut();
                }

                if (player.player.backpackSlot.hasBackpack && 
                    player.player.backpackSlot.data.TryGetDataEntry(DataEntryKey.BackpackData, out BackpackData bpData)) {
                    
                    foreach (ItemSlot? slot in bpData.itemSlots) {
                        slot.EmptyOut();
                    }
                }
                
                for (int i = 0; i < itemCount; i++) {
                    player.refs.items.SpawnItemInHand(_cachedMilkItem.name);
                }
            }
        }
    }
}