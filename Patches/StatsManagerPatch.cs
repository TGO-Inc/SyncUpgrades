using HarmonyLib;
using SyncUpgrades.Core;

namespace SyncUpgrades.Patches;

// [HarmonyPatch(typeof(StatsManager))]
internal class StatsManagerPatch
{
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    [HarmonyPatch(nameof(PlayerAdd), typeof(string), typeof(string))]
    private static void PlayerAdd(string _steamID, string _playerName)
    {
        // If not host OR single-player, return
        if (SemiFunc.IsNotMasterClient() || SemiFunc.MenuLevel())
            return;
        
        #if DEBUG
        Entry.LogSource.LogInfo("[PlayerAdd] Syncing upgrades for player: " + _steamID);
        #endif
        
        // Sync the upgrades
        // SyncManager.SyncHostToTarget(_steamID);
    }
}