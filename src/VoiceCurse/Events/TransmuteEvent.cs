using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Photon.Pun;
using Random = UnityEngine.Random;

namespace VoiceCurse.Events;

public class TransmuteEvent : VoiceEventBase {
    private readonly List<(string Name, string[] Triggers, string[] Targets, Func<bool> IsEnabled)> _definitions;
    private readonly Dictionary<string, (string Name, string[] Targets, Func<bool> IsEnabled)> _transmuteLookup = new();
    private readonly Dictionary<string, Item?> _itemCache = new();
    
    private static readonly Regex NameCleaner = new(@"\s*\((\d+|Clone)\)", RegexOptions.Compiled);

    public TransmuteEvent(Config config) : base(config) {
        _definitions = [
            ("Milk",     ["milk", "calcium"], ["Fortified Milk"], () => config.TransmuteMilkEnabled.Value),
            ("Cactus",   ["cactus", "cacti"], ["Cactus"], () => config.TransmuteCactusEnabled.Value),
            ("Coconut",  ["coconut"], ["Coconut"], () => config.TransmuteCoconutEnabled.Value),
            ("Apple",    ["apple", "berry"], ["Red Crispberry", "Yellow Crispberry", "Green Crispberry"], () => config.TransmuteAppleEnabled.Value),
            ("Banana",   ["banana"], ["Berrynana Peel Yellow"], () => config.TransmuteBananaEnabled.Value),
            ("Egg",      ["egg"], ["Egg"], () => config.TransmuteEggEnabled.Value),
            ("Fruit",    ["fruit"], ["Red Crispberry", "Yellow Crispberry", "Green Crispberry", "Kingberry Purple", "Kingberry Yellow", "Kingberry Green", "Berrynana Brown", "Berrynana Yellow", "Berrynana Pink", "Berrynana Blue"], () => config.TransmuteFruitEnabled.Value),
            ("Mushroom", ["fungus", "mushroom", "fungi", "funghi", "shroom"], ["Mushroom Normie"], () => config.TransmuteMushroomEnabled.Value)
        ];
        
        foreach ((string Name, string[] Triggers, string[] Targets, Func<bool> IsEnabled) def in _definitions) {
            foreach (string trigger in def.Triggers) {
                _transmuteLookup[trigger] = (def.Name, def.Targets, def.IsEnabled);
            }
        }
    }

    protected override IEnumerable<string> GetKeywords() {
        return Config.TransmuteEnabled.Value ? _definitions.Where(d => d.IsEnabled()).SelectMany(d => d.Triggers) : [];
    }

    protected override bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword) {
        if (!Config.TransmuteEnabled.Value) return false;
        if (player.data.dead) return false;
        
        string[]? targets = null;
        string? ruleName = null;
        
        if (_transmuteLookup.TryGetValue(matchedKeyword, out (string Name, string[] Targets, Func<bool> IsEnabled) foundData)) {
            if (foundData.IsEnabled()) {
                ruleName = foundData.Name;
                targets = foundData.Targets;
            }
        } 

        if (targets == null) {
            string? validKey = _transmuteLookup.Keys.FirstOrDefault(k => 
                fullSentence.Contains(k) && _transmuteLookup[k].IsEnabled());
            
            if (validKey != null) {
                (string Name, string[] Targets, Func<bool> IsEnabled) data = _transmuteLookup[validKey];
                ruleName = data.Name;
                targets = data.Targets;
            }
        }

        if (targets == null || targets.Length == 0) return false;
        
        ExecutionDetail = ruleName;

        bool deathEnabled = Config.TransmuteDeathEnabled.Value;
        
        if (deathEnabled) {
            TransmuteInventoryDeath(player, targets);
            player.DieInstantly();
        } else {
            TransmuteInventoryAlive(player, targets);

            if (!player.refs.afflictions) return true;
            float damage = Random.Range(Config.AfflictionMinPercent.Value, Config.AfflictionMaxPercent.Value);
            player.refs.afflictions.AddStatus(CharacterAfflictions.STATUSTYPE.Injury, damage);
        }

        return true;
    }

    private void TransmuteInventoryDeath(Character player, string[] possibleTargets) {
        int countToSpawn = 1;
        Vector3 voidPosition = new(0, -5000, 0);
        
        for (byte i = 0; i < 3; i++) {
            ItemSlot slot = player.player.GetItemSlot(i);
            if (slot == null || slot.IsEmpty()) continue;
            
            countToSpawn++;
            player.refs.items.photonView.RPC("DropItemFromSlotRPC", RpcTarget.All, i, voidPosition);
        }

        ItemSlot backpackSlot = player.player.GetItemSlot(3);
        if (backpackSlot != null && !backpackSlot.IsEmpty()) {
            if (backpackSlot.data.TryGetDataEntry(DataEntryKey.BackpackData, out BackpackData backpackData)) {
                countToSpawn += backpackData.FilledSlotCount();
            }
            
            countToSpawn++;
            player.refs.items.photonView.RPC("DropItemFromSlotRPC", RpcTarget.All, (byte)3, voidPosition);
        }

        SpawnTransmutedItems(player.Center, countToSpawn, possibleTargets);
        player.refs.afflictions.UpdateWeight();
    }

    private void TransmuteInventoryAlive(Character player, string[] possibleTargets) {
        Vector3 voidPosition = new(0, -5000, 0);
        Vector3 spawnOrigin = player.Center;

        for (byte i = 0; i < 3; i++) {
            ItemSlot slot = player.player.GetItemSlot(i);
            if (slot == null || slot.IsEmpty()) continue;
            
            player.refs.items.photonView.RPC("DropItemFromSlotRPC", RpcTarget.All, i, voidPosition);
            SpawnAndPickupItem(player, possibleTargets, spawnOrigin);
        }

        ItemSlot backpackSlot = player.player.GetItemSlot(3);
        if (backpackSlot != null && !backpackSlot.IsEmpty()) {
            int countToSpawn = 1;
            
            if (backpackSlot.data.TryGetDataEntry(DataEntryKey.BackpackData, out BackpackData backpackData)) {
                countToSpawn += backpackData.FilledSlotCount();
            }
            
            player.refs.items.photonView.RPC("DropItemFromSlotRPC", RpcTarget.All, (byte)3, voidPosition);
            SpawnTransmutedItems(spawnOrigin, countToSpawn, possibleTargets);
        }
        
        player.refs.afflictions.UpdateWeight();
    }
    
    private void SpawnTransmutedItems(Vector3 origin, int count, string[] targets) {
        for (int i = 0; i < count; i++) {
            SpawnItem(origin, targets, false, null);
        }
    }

    private void SpawnAndPickupItem(Character player, string[] targets, Vector3 origin) {
        SpawnItem(origin, targets, true, player);
    }

    private void SpawnItem(Vector3 origin, string[] targets, bool autoPickup, Character? picker) {
        string selectedTargetName = targets[Random.Range(0, targets.Length)];
        Item? targetItem = GetOrFindItem(selectedTargetName);
        if (!targetItem) return;

        Vector3 pos = origin + Random.insideUnitSphere * 0.5f;
        pos.y = origin.y + 0.5f; 
        
        string cleanName = NameCleaner.Replace(targetItem.name, "").Trim();
        string prefabPath = "0_Items/" + cleanName;
        
        GameObject obj = PhotonNetwork.Instantiate(prefabPath, pos, Quaternion.identity);
        if (!obj || !obj.TryGetComponent(out PhotonView pv)) return;
        pv.RPC("SetKinematicRPC", RpcTarget.All, false, pos, Quaternion.identity);

        if (!autoPickup || !picker || !obj.TryGetComponent(out Item item)) return;
        if (picker.refs != null && picker.refs.items != null) {
            picker.refs.items.lastEquippedSlotTime = 0f;
        }
        item.Interact(picker);
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