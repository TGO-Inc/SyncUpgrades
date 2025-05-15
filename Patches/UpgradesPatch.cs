using ExitGames.Client.Photon;
using HarmonyLib;
using REPOLib.Modules;
using SyncUpgrades.Core;

namespace SyncUpgrades.Patches;

[HarmonyPatch(typeof(Upgrades))]
public class UpgradesPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HandleUpgradeEvent))]
    private static void HandleUpgradeEvent(EventData eventData)
    {
        // If not host OR single-player, return
        if (SemiFunc.IsNotMasterClient())
            return;
        
        #if DEBUG
        Entry.LogSource.LogInfo($"[{nameof(HandleUpgradeEvent)}] [{eventData.Code}] [{eventData.CustomData}]");
        #endif
        
        if (eventData.CustomData is not Hashtable customData)
            return;
        
        var upgradeId = (string) customData["UpgradeId"];
        var steamId = (string) customData["SteamId"];
        var level = (int) customData["Level"];
        
        if (!Upgrades.TryGetUpgrade(upgradeId, out _))
            return;

        string realId = SyncUtil.FixKey(upgradeId);
        SyncManager.PlayerConsumedUpgrade(steamId, UpgradeId.New(realId), level);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(nameof(RaiseUpgradeEvent))]
    private static void RaiseUpgradeEvent(string upgradeId, string steamId, int level)
    {
        // If not host OR single-player, return
        if (SemiFunc.IsNotMasterClient())
            return;
        
        // If not the host, return
        if (steamId != SyncUtil.HostSteamId)
            return;
        
        #if DEBUG
        Entry.LogSource.LogInfo($"[{nameof(RaiseUpgradeEvent)}] [{steamId}]");
        #endif

        string realId = SyncUtil.FixKey(upgradeId);
        SyncManager.PlayerConsumedUpgrade(steamId, UpgradeId.New(realId), level);
    }
}