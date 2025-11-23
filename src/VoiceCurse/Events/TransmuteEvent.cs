using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Photon.Pun;

namespace VoiceCurse.Events;

public class TransmuteEvent(Config config) : VoiceEventBase(config) {
    private static readonly List<(string Name, string[] Triggers, string[] Targets)> TransmuteDefinitions = [
        ("Milk",     ["milk", "calcium"], ["Fortified Milk"]),
        ("Cactus",   ["cactus", "cacti"], ["Cactus"]),
        ("Coconut",  ["coconut"], ["Coconut"]),
        ("Apple",    ["apple"], ["Red Crispberry", "Yellow Crispberry", "Green Crispberry"]),
        ("Banana",   ["banana"], ["Berrynana Peel Yellow"]),
        ("Egg",      ["egg"], ["Egg"]),
        ("Fruit",    ["fruit"], ["Red Crispberry", "Yellow Crispberry", "Green Crispberry", "Kingberry Purple", "Kingberry Yellow", "Kingberry Green", "Berrynana Brown", "Berrynana Yellow", "Berrynana Pink", "Berrynana Blue"]),
        ("Mushroom", ["fungus", "mushroom", "fungi", "funghi", "shroom"], ["Mushroom Normie", "Mushroom Normie Poison"])
    ];
    
    private readonly Dictionary<string, (string Name, string[] Targets)> _transmuteLookup = 
        TransmuteDefinitions
            .SelectMany(def => def.Triggers.Select(trigger => (Trigger: trigger, Data: (def.Name, def.Targets))))
            .ToDictionary(x => x.Trigger, x => x.Data);

    private readonly Dictionary<string, Item?> _itemCache = new();
    private static readonly Regex NameCleaner = new(@"\s*\((\d+|Clone)\)", RegexOptions.Compiled);

    protected override IEnumerable<string> GetKeywords() => _transmuteLookup.Keys;

    protected override bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword) {
        if (player.data.dead) return false;
        
        (string Name, string[] Targets)? match = null;
        
        if (_transmuteLookup.TryGetValue(matchedKeyword, out (string Name, string[] Targets) foundData)) {
            match = foundData;
        } else {
            string? key = _transmuteLookup.Keys.FirstOrDefault(fullSentence.Contains);
            if (key != null) match = _transmuteLookup[key];
        }

        if (match == null) return false;
        ExecutionDetail = match.Value.Name;
        TransmuteInventory(player, match.Value.Targets);
            
        player.photonView.RPC("RPCA_Die", RpcTarget.All, player.Center);
        return true;
    }

    private void TransmuteInventory(Character player, string[] possibleTargets) {
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
            string selectedTargetName = possibleTargets[Random.Range(0, possibleTargets.Length)];
            Item? targetItem = GetOrFindItem(selectedTargetName);
            if (!targetItem) continue;

            Vector3 pos = spawnOrigin + Random.insideUnitSphere * 0.5f;
            pos.y = spawnOrigin.y + 0.5f; 
            
            string cleanName = NameCleaner.Replace(targetItem.name, "").Trim();
            string prefabPath = "0_Items/" + cleanName;
            
            GameObject obj = PhotonNetwork.Instantiate(prefabPath, pos, Quaternion.identity);
            if (obj && obj.TryGetComponent(out PhotonView pv)) {
                pv.RPC("SetKinematicRPC", RpcTarget.All, false, pos, Quaternion.identity);
            }
        }
        
        player.refs.afflictions.UpdateWeight();
    }
    
    private Item? GetOrFindItem(string searchName) {
        if (_itemCache.TryGetValue(searchName, out Item? cachedItem)) {
            if (cachedItem) return cachedItem;
            _itemCache.Remove(searchName);
        }

        Item? foundItem = Resources.FindObjectsOfTypeAll<Item>().FirstOrDefault(i => i.name.Contains(searchName) || (i.UIData != null && i.UIData.itemName.Contains(searchName)));
        _itemCache[searchName] = foundItem;
        
        if (!foundItem && Config.EnableDebugLogs.Value) {
            Debug.LogWarning($"[VoiceCurse] Could not find item matching '{searchName}'");
        }
        
        return foundItem;
    }
}