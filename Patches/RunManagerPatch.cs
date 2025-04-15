using HarmonyLib;
using SyncUpgrades.Core;

namespace SyncUpgrades.Patches;

[HarmonyPatch(typeof(RunManager))]
internal class RunManagerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("ChangeLevel")]
    private static void ChangeLevel()
    {
        // If not host OR single-player, return
        if (SemiFunc.IsNotMasterClient())
            return;
        
        // Sync the upgrades
        var defaultBundle = new PunBundle(PunManager.instance, PunManager.instance.GetView(), StatsManager.instance, SemiUtil.HostSteamId);
        SyncManager.SyncAll(defaultBundle);
    }
}