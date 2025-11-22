using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using VoiceCurse.Core;

namespace VoiceCurse.Events;

public class DropEvent(VoiceCurseConfig config) : IVoiceEvent {
    private readonly HashSet<string> _keywords = [
        "drop", "dropping", "dropped", "oops", "whoops", 
        "butterfingers", "fumble", "fumbled", "slip", "slipping",
        "release", "discard", "off"
    ];

    public bool TryExecute(string spokenWord, string fullSentence) {
        if (!_keywords.Contains(spokenWord)) return false;

        Character localChar = Character.localCharacter;
        if (localChar is null || localChar.data.dead) return false;

        if (config.EnableDebugLogs.Value) {
            Debug.Log($"[VoiceCurse] Drop Items triggered by '{spokenWord}'");
        }
        
        ScatterBackpackContents(localChar);
        localChar.refs.items.DropAllItems(includeBackpack: true);
        
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