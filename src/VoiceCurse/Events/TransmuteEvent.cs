using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;

namespace VoiceCurse.Events;

public class TransmuteEvent(Config config) : VoiceEventBase(config) {
    private readonly Dictionary<string, string> _transmuteMap = new() {
        { "milk", "Fortified Milk" },
        { "calcium", "Fortified Milk" },
        { "cactus", "Cactus" },
        { "cacti", "Cactus" },
        { "coconut", "Coconut" },
        { "apple", "Red Crispberry" },
        { "banana", "Berrynana Peel Yellow" },
        { "egg", "Egg" },
    };

    private readonly Dictionary<string, Item?> _itemCache = new();

    protected override IEnumerable<string> GetKeywords() => _transmuteMap.Keys;

    protected override bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword) {
        if (player.data.dead) return false;
        
        string? targetItemName = null;
        
        if (_transmuteMap.TryGetValue(matchedKeyword, out string? search)) {
            targetItemName = search;
        } else {
            string? key = _transmuteMap.Keys.FirstOrDefault(fullSentence.Contains);
            if (key != null) targetItemName = _transmuteMap[key];
        }

        if (targetItemName == null) return false;
        TransmuteInventory(player, targetItemName);
            
        player.photonView.RPC("RPCA_Die", RpcTarget.All, player.Center);
        return true;

    }

    private void TransmuteInventory(Character player, string itemNameSearch) {
        if (!_itemCache.TryGetValue(itemNameSearch, out Item? targetItem)) {
            targetItem = Resources.FindObjectsOfTypeAll<Item>()
                .FirstOrDefault(i => i.name.Contains(itemNameSearch) || 
                                    (i.UIData != null && i.UIData.itemName.Contains(itemNameSearch)));
            _itemCache[itemNameSearch] = targetItem;
        }

        if (!targetItem) {
            if (Config.EnableDebugLogs.Value) Debug.LogWarning($"[VoiceCurse] Could not find item matching '{itemNameSearch}'");
            return;
        }

        string prefabPath = "0_Items/" + targetItem.name;
        int countToSpawn = 0;
        
        if (player.player?.itemSlots != null) {
            foreach (ItemSlot slot in player.player.itemSlots) {
                if (slot.IsEmpty()) continue;
                countToSpawn++;
                slot.EmptyOut();
            }
        }
        
        ItemSlot? backpackSlot = player.player?.GetItemSlot(3);
        if (backpackSlot != null && !backpackSlot.IsEmpty()) {
            if (backpackSlot.data.TryGetDataEntry(DataEntryKey.BackpackData, out BackpackData backpackData)) {
                countToSpawn += backpackData.FilledSlotCount();
            }
            countToSpawn++;
            backpackSlot.EmptyOut();
        }
        
        if (countToSpawn == 0) countToSpawn = 1;

        Vector3 spawnOrigin = player.Center;

        for (int i = 0; i < countToSpawn; i++) {
            Vector3 pos = spawnOrigin + Random.insideUnitSphere * 0.5f;
            pos.y = spawnOrigin.y + 0.5f; 
            GameObject obj = PhotonNetwork.Instantiate(prefabPath, pos, Quaternion.identity);
            
            if (obj.TryGetComponent(out PhotonView pv)) {
                pv.RPC("SetKinematicRPC", RpcTarget.All, false, pos, Quaternion.identity);
            }
        }
        
        player.refs.afflictions.UpdateWeight();
    }
}