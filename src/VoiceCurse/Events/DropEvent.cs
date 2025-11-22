using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using VoiceCurse.Core;

namespace VoiceCurse.Events;

public class DropEvent(VoiceCurseConfig config) : VoiceEventBase(config) {
    private readonly HashSet<string> _keywords = [
        "drop", "oops", "whoops", "butterfingers", "fumble", "slip", 
        "release", "discard", "yeet", "off"
    ];

    protected override IEnumerable<string> GetKeywords() => _keywords;

    protected override bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword) {
        if (player.data.dead) return false;

        ScatterBackpackContents(player);
        player.refs.items.DropAllItems(includeBackpack: true);
        
        return true;
    }

    private void ScatterBackpackContents(Character player) {
        ItemSlot backpackSlot = player.player.GetItemSlot(3);
        
        if (backpackSlot.IsEmpty() || 
            !backpackSlot.data.TryGetDataEntry(DataEntryKey.BackpackData, out BackpackData backpackData)) {
            return;
        }

        Vector3 dropOrigin = player.Center;
        
        foreach (ItemSlot internalSlot in backpackData.itemSlots) {
            if (internalSlot.IsEmpty()) continue;

            string prefabName = internalSlot.GetPrefabName();
            if (string.IsNullOrEmpty(prefabName)) continue;
            
            Vector3 spawnPos = dropOrigin + (Random.insideUnitSphere * 0.5f);
            spawnPos.y = dropOrigin.y + 0.5f;
            
            GameObject droppedItem = PhotonNetwork.Instantiate(
                "0_Items/" + prefabName, 
                spawnPos, 
                Quaternion.identity
            );
            
            if (droppedItem.TryGetComponent(out PhotonView itemHeader)) {
                itemHeader.RPC("SetItemInstanceDataRPC", RpcTarget.All, internalSlot.data);
                itemHeader.RPC("SetKinematicRPC", RpcTarget.All, false, spawnPos, Quaternion.identity);
            }
            
            internalSlot.EmptyOut();
        }
    }
}