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
        return !player.data.dead;
    }

    public override void PlayEffects(Character origin, Vector3 position) {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!origin || origin.data.dead) return;
        
        ScatterBackpackContents(origin);

        Transform hip = origin.GetBodypart(BodypartType.Hip).transform;
        Vector3 fwd = hip.forward;
        if (Vector3.Dot(fwd, Vector3.up) < 0f) fwd = -fwd;
        
        Vector3 dropPos = origin.Center + fwd * 0.6f + Vector3.up * 0.5f;
        
        for (byte i = 0; i < 4; i++) {
            origin.refs.items.DropItemFromSlotRPC(i, dropPos);
            dropPos += Vector3.up * 0.3f;
        }
    }

    private static void ScatterBackpackContents(Character player) {
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
    }
}