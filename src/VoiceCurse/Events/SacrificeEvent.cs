using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace VoiceCurse.Events;

public class SacrificeEvent(Config config) : VoiceEventBase(config) {
    private readonly HashSet<string> _sacrificeKeywords = ParseKeywords(config.SacrificeKeywords.Value);

    protected override IEnumerable<string> GetKeywords() {
        return Config.SacrificeEnabled.Value ? _sacrificeKeywords : Enumerable.Empty<string>();
    }

    protected override bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword) {
        if (!Config.SacrificeEnabled.Value) return false;
        if (player.data.dead) return false;
        
        Character? closestDeadPlayer = DeathTracker.GetClosestDeadPlayer(player.Center, out Vector3 deathPos);
        if (!closestDeadPlayer) return false; 
        
        Vector3 revivePosition = deathPos + Vector3.up * 1.0f;
        ExecutionDetail = $"Reviving {closestDeadPlayer.characterName}";
        closestDeadPlayer.view.RPC("RPCA_ReviveAtPosition", RpcTarget.All, revivePosition, true);
        DeathTracker.RemoveDeath(closestDeadPlayer);
        
        player.DieInstantly();
        return true;
    }
}

[HarmonyPatch(typeof(Character))]
public static class DeathTracker {
    private static readonly Dictionary<int, Vector3> DeathLocations = new();

    [HarmonyPatch("RPCA_Die")]
    [HarmonyPrefix]
    private static void OnDiePrefix(Character __instance) {
        if (__instance) {
            DeathLocations[__instance.photonView.ViewID] = __instance.Center;
        }
    }
    
    [HarmonyPatch("RPCA_Revive")]
    [HarmonyPostfix]
    private static void OnRevivePostfix(Character __instance) {
        RemoveDeath(__instance);
    }
    
    [HarmonyPatch("RPCA_ReviveAtPosition")]
    [HarmonyPostfix]
    private static void OnReviveAtPosPostfix(Character __instance) {
        RemoveDeath(__instance);
    }

    public static void RemoveDeath(Character c) {
        if (c && c.photonView) {
            DeathLocations.Remove(c.photonView.ViewID);
        }
    }

    public static Character? GetClosestDeadPlayer(Vector3 origin, out Vector3 deathPos) {
        Character? bestTarget = null;
        float closestDistance = float.MaxValue;
        deathPos = Vector3.zero;

        foreach (Character c in Character.AllCharacters.Where(c => c.data.dead)) {
            Vector3 targetPos;
            if (DeathLocations.TryGetValue(c.photonView.ViewID, out Vector3 recordedPos)) {
                targetPos = recordedPos;
            } else if (c.Ghost) {
                targetPos = c.Ghost.transform.position;
            } else {
                continue;
            }

            float dist = Vector3.Distance(origin, targetPos);
            if (!(dist < closestDistance)) continue;
            closestDistance = dist;
            bestTarget = c;
            deathPos = targetPos;
        }

        return bestTarget;
    }
}