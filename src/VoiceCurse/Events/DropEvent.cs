using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace VoiceCurse.Events;

public class DropEvent(Config config) : VoiceEventBase(config) {
    private readonly HashSet<string> _keywords = [
        "drop", "oops", "whoops", "butterfingers", "fumble", 
        "release", "discard", "off", "loss", "lose", "let go",
        "slip away", "misplace", "clumsy", "accident", "unhand",
        "relinquish", "surrender", "abandon", "ditched", "ditch",
        "shed", "cast", "toss", "throw away", "get rid"
    ];
    protected override IEnumerable<string> GetKeywords() => _keywords;

    protected override bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword) {
        if (player.data.dead) return false;
        string? backpackPrefab = ScatterBackpackContents(player);
        
        if (!string.IsNullOrEmpty(backpackPrefab)) {
            Vector3 dropPos = player.Center + player.transform.forward * 0.5f + Vector3.up * 0.2f;
            
            GameObject myBag = PhotonNetwork.Instantiate(
                "0_Items/" + backpackPrefab, 
                dropPos, 
                Quaternion.identity
            );
            
            if (myBag.TryGetComponent(out PhotonView pv)) {
                pv.RPC("SetKinematicRPC", RpcTarget.All, false, dropPos, Quaternion.identity);
            }
        }
        
        player.refs.items.DropAllItems(includeBackpack: false);
        if (!player.player.GetItemSlot(3).IsEmpty()) {
            player.refs.items.photonView.RPC("DropItemFromSlotRPC", RpcTarget.All, (byte)3, new Vector3(0, -5000, 0));
        }
        
        return true;
    }

    private static string? ScatterBackpackContents(Character player) {
        ItemSlot backpackSlot = player.player.GetItemSlot(3);
        
        if (backpackSlot.IsEmpty() || 
            !backpackSlot.data.TryGetDataEntry(DataEntryKey.BackpackData, out BackpackData backpackData)) {
            return null;
        }

        string backpackPrefabName = backpackSlot.GetPrefabName();
        Vector3 dropOrigin = player.Center;
        
        foreach (ItemSlot internalSlot in backpackData.itemSlots) {
            if (internalSlot.IsEmpty()) continue;

            string prefabName = internalSlot.GetPrefabName();
            if (string.IsNullOrEmpty(prefabName)) continue;
            
            Vector3 spawnPos = dropOrigin + Random.insideUnitSphere * 0.5f;
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

        return backpackPrefabName;
    }
}