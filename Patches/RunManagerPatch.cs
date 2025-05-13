using HarmonyLib;
using SyncUpgrades.Core;

namespace SyncUpgrades.Patches;

[HarmonyPatch(typeof(RunManager))]
internal class RunManagerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("ChangeLevel")]
    private static void ChangeLevel(bool _completedLevel, bool _levelFailed, bool ___restarting)
    {
        // If not host OR single-player, return
        if (SemiFunc.IsNotMasterClient() || _levelFailed)
            return;
        
        
        #if DEBUG
        Entry.LogSource.LogInfo($"[ChangeLevel] [{___restarting}] [{SemiFunc.MenuLevel()}] [{_completedLevel}] [{_levelFailed}] Syncing upgrades for all players");
        #endif
        
        // Sync the upgrades
        PunBundle defaultBundle = new(PunManager.instance, PunManager.instance.GetView(), StatsManager.instance, SyncUtil.HostSteamId);
        SyncManager.SyncAll(defaultBundle);
    }
}