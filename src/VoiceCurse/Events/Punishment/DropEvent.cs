using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

namespace VoiceCurse.Events.Punishment;

public class DropEvent(Config config) : VoiceEventBase(config) {
    private readonly HashSet<string> _keywords = ParseKeywords(config.DropKeywords.Value);

    private static HashSet<string> ParseKeywords(string configLine) {
        return configLine
            .Split([','], StringSplitOptions.RemoveEmptyEntries)
            .Select(k => k.Trim().ToLowerInvariant())
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .ToHashSet();
    }
    
    protected override IEnumerable<string> GetKeywords() {
        return Config.DropEnabled.Value ? _keywords : Enumerable.Empty<string>();
    }

    protected override bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword) {
        return Config.DropEnabled.Value && !player.data.dead;
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
            if (origin.player.GetItemSlot(i).IsEmpty()) continue;
            origin.refs.items.photonView.RPC("DropItemFromSlotRPC", RpcTarget.All, i, dropPos);
            dropPos += Vector3.up * 0.3f;
        }

        origin.refs.items.photonView.RPC("EquipSlotRpc", RpcTarget.All, -1, -1);
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