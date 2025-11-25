using System.Collections;
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
        
        ScatterBackpackContents(player);
        player.refs.items.DropAllItems(includeBackpack: true);
        
        return true;
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

    public override void PlayEffects(Vector3 position) {
        if (PhotonNetwork.IsMasterClient && Character.localCharacter) {
            Character.localCharacter.StartCoroutine(ClearDroppedBackpackRoutine(position));
        }
    }

    private static IEnumerator ClearDroppedBackpackRoutine(Vector3 position) {
        yield return new WaitForSeconds(0.25f);
        
        Collider[] hits = Physics.OverlapSphere(position, 2.0f);
        foreach (Collider hit in hits) {
            Backpack bag = hit.GetComponentInParent<Backpack>();
            if (!bag) continue;

            if (bag.data == null || !bag.data.TryGetDataEntry(DataEntryKey.BackpackData, out BackpackData bd)) continue;
            bool modified = false;
                
            foreach (ItemSlot slot in bd.itemSlots) {
                if (slot.IsEmpty()) continue;
                slot.EmptyOut();
                modified = true;
            }

            if (!modified) continue;
            bag.photonView.RPC("SetItemInstanceDataRPC", RpcTarget.All, bag.data);
                    
            if (bag.TryGetComponent(out BackpackVisuals visuals)) {
                visuals.RefreshVisuals();
            }
        }
    }
}