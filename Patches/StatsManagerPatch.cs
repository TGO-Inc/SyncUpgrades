using HarmonyLib;
using SyncUpgrades.Core;

namespace SyncUpgrades.Patches;

[HarmonyPatch(typeof(StatsManager))]
internal class StatsManagerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("PlayerAdd", typeof(string), typeof(string))]
    private static void PlayerAdd(string _steamID, string _playerName)
    {
        // If not host OR single-player, return
        if (SemiFunc.IsNotMasterClient())
            return;
        
        // Sync the upgrades
        // Entry.LogSource.LogInfo("[PlayerAdd] Syncing upgrades for player: " + _steamID);
        SyncManager.SyncUpgrades(_steamID);
    }
}