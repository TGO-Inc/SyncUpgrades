using System.Linq;
using HarmonyLib;
using Photon.Pun;
using SyncUpgrades.Core;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace SyncUpgrades.Patches;

[HarmonyPatch(typeof(PunManager))]
public class PunManagerPatch
{
    // UpgradePlayerHealth
    [HarmonyPrefix]
    [HarmonyPatch("UpdateHealthRightAway", typeof(string))]
    private static void UpdateHealthRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string playerName)
    {
        // If not host OR single-player, return
        if (SemiFunc.IsNotMasterClient())
            return;
        
        var bundle = new PunBundle(__instance, ___photonView, ___statsManager, playerName);
        SyncManager.PlayerUpgradeStat(bundle, SemiUtil.HealthId);
    }
    
    // UpgradePlayerEnergy
    [HarmonyPrefix]
    [HarmonyPatch("UpdateEnergyRightAway", typeof(string))]
    private static void UpdateEnergyRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID)
    {
        // If not host OR single-player, return
        if (SemiFunc.IsNotMasterClient())
            return;
        
        var bundle = new PunBundle(__instance, ___photonView, ___statsManager, _steamID);
        SyncManager.PlayerUpgradeStat(bundle, SemiUtil.StaminaId);
    }
    
    // UpgradePlayerTumbleLaunch
    [HarmonyPrefix]
    [HarmonyPatch("UpdateTumbleLaunchRightAway", typeof(string))]
    private static void UpdateTumbleLaunchRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID)
    {
        // If not host OR single-player, return
        if (SemiFunc.IsNotMasterClient())
            return;
        
        var bundle = new PunBundle(__instance, ___photonView, ___statsManager, _steamID);
        SyncManager.PlayerUpgradeStat(bundle, SemiUtil.TumbleLaunchId);
    }
    
    // UpgradePlayerSprintSpeed
    [HarmonyPrefix]
    [HarmonyPatch("UpdateSprintSpeedRightAway", typeof(string))]
    private static void UpdateSprintSpeedRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID)
    {
        // If not host OR single-player, return
        if (SemiFunc.IsNotMasterClient())
            return;
        
        var bundle = new PunBundle(__instance, ___photonView, ___statsManager, _steamID);
        SyncManager.PlayerUpgradeStat(bundle, SemiUtil.SprintSpeedId);
    }
    
    // UpgradePlayerGrabStrength
    [HarmonyPrefix]
    [HarmonyPatch("UpdateGrabStrengthRightAway", typeof(string))]
    private static void UpdateGrabStrengthRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID)
    {
        // If not host OR single-player, return
        if (SemiFunc.IsNotMasterClient())
            return;
        
        var bundle = new PunBundle(__instance, ___photonView, ___statsManager, _steamID);
        SyncManager.PlayerUpgradeStat(bundle, SemiUtil.GrabStrengthId);
    }
    
    // UpgradePlayerThrowStrength
    [HarmonyPrefix]
    [HarmonyPatch("UpdateThrowStrengthRightAway", typeof(string))]
    private static void UpdateThrowStrengthRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID)
    {
        // If not host OR single-player, return
        if (SemiFunc.IsNotMasterClient())
            return;
        
        var bundle = new PunBundle(__instance, ___photonView, ___statsManager, _steamID);
        SyncManager.PlayerUpgradeStat(bundle, SemiUtil.GrabThrowId);
    }

    // UpgradePlayerGrabRange
    [HarmonyPrefix]
    [HarmonyPatch("UpdateGrabRangeRightAway", typeof(string))]
    private static void UpdateGrabRangeRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID)
    {
        // If not host OR single-player, return
        if (SemiFunc.IsNotMasterClient())
            return;
        
        var bundle = new PunBundle(__instance, ___photonView, ___statsManager, _steamID);
        SyncManager.PlayerUpgradeStat(bundle, SemiUtil.GrabRangeId);
    }
    
    /// <summary>
    /// Please CALL THIS FUNCTION with photonView.RPC to notify the host of the upgrade!!
    /// <para>{"upgradeName" = { "steamId": 1 } }, true</para>
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="___photonView"></param>
    /// <param name="___statsManager"></param>
    /// <param name="data"></param>
    /// <param name="finalChunk">SET TO TRUE IF CALLING FROM MODDED RPC</param>
    /// <returns></returns>
    [HarmonyPrefix]
    [HarmonyPatch("ReceiveSyncData", typeof(Hashtable), typeof(bool))]
    private static bool ReceiveSyncData(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, Hashtable data, bool finalChunk)
    {
        // If not host OR single-player, return
        if (SemiFunc.IsNotMasterClient() || !finalChunk)
            return true;

        if (data.Count > 1)
            return true;

        var dataItem = data.First();
        if (dataItem.Key is not string upgradeName || dataItem.Value is not Hashtable hashtable)
            return true;
        
        var upgradeId = new UpgradeId(upgradeName);
        if (upgradeId.Type != UpgradeType.Modded)
            return true;
        
        if (hashtable.Count > 1)
            return true;
        
        var dataItem2 = hashtable.First();
        if (dataItem2.Key is not string steamId || dataItem2.Value is not int newValue)
            return true;

        var original = ___statsManager.dictionaryOfDictionaries[upgradeName][steamId];
        if (original >= newValue)
            return true;
        
        var bundle = new PunBundle(__instance, ___photonView, ___statsManager, steamId);
        for(var i = original; i <= newValue; i++)
            SyncManager.PlayerUpgradeStat(bundle, upgradeId);
        
        return false;
    }
}