using HarmonyLib;
using Photon.Pun;
using SyncUpgrades.Core;

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
        
        #if DEBUG
        Entry.LogSource.LogInfo($"[{nameof(UpdateHealthRightAway)}] Upgrade: " + playerName);
        #endif
        
        SyncBundle bundle = new(__instance, ___photonView, ___statsManager, playerName);
        SyncManager.PlayerConsumedUpgrade(bundle, SyncUtil.HealthId);
    }
    
    // UpgradePlayerEnergy
    [HarmonyPrefix]
    [HarmonyPatch("UpdateEnergyRightAway", typeof(string))]
    private static void UpdateEnergyRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID)
    {
        // If not host OR single-player, return
        if (SemiFunc.IsNotMasterClient())
            return;
        
        #if DEBUG
        Entry.LogSource.LogInfo($"[{nameof(UpdateEnergyRightAway)}] Upgrade: " + _steamID);
        #endif
        
        SyncBundle bundle = new(__instance, ___photonView, ___statsManager, _steamID);
        SyncManager.PlayerConsumedUpgrade(bundle, SyncUtil.StaminaId);
    }
    
    // UpgradePlayerTumbleLaunch
    [HarmonyPrefix]
    [HarmonyPatch("UpdateTumbleLaunchRightAway", typeof(string))]
    private static void UpdateTumbleLaunchRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID)
    {
        // If not host OR single-player, return
        if (SemiFunc.IsNotMasterClient())
            return;
        
        #if DEBUG
        Entry.LogSource.LogInfo($"[{nameof(UpdateTumbleLaunchRightAway)}] Upgrade: " + _steamID);
        #endif
        
        SyncBundle bundle = new(__instance, ___photonView, ___statsManager, _steamID);
        SyncManager.PlayerConsumedUpgrade(bundle, SyncUtil.TumbleLaunchId);
    }
    
    // UpgradePlayerSprintSpeed
    [HarmonyPrefix]
    [HarmonyPatch("UpdateSprintSpeedRightAway", typeof(string))]
    private static void UpdateSprintSpeedRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID)
    {
        // If not host OR single-player, return
        if (SemiFunc.IsNotMasterClient())
            return;
        
        #if DEBUG
        Entry.LogSource.LogInfo($"[{nameof(UpdateSprintSpeedRightAway)}] Upgrade: " + _steamID);
        #endif
        
        SyncBundle bundle = new(__instance, ___photonView, ___statsManager, _steamID);
        SyncManager.PlayerConsumedUpgrade(bundle, SyncUtil.SprintSpeedId);
    }
    
    // UpgradePlayerGrabStrength
    [HarmonyPrefix]
    [HarmonyPatch("UpdateGrabStrengthRightAway", typeof(string))]
    private static void UpdateGrabStrengthRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID)
    {
        // If not host OR single-player, return
        if (SemiFunc.IsNotMasterClient())
            return;
        
        #if DEBUG
        Entry.LogSource.LogInfo($"[{nameof(UpdateGrabStrengthRightAway)}] Upgrade: " + _steamID);
        #endif
        
        SyncBundle bundle = new(__instance, ___photonView, ___statsManager, _steamID);
        SyncManager.PlayerConsumedUpgrade(bundle, SyncUtil.GrabStrengthId);
    }
    
    // UpgradePlayerThrowStrength
    [HarmonyPrefix]
    [HarmonyPatch("UpdateThrowStrengthRightAway", typeof(string))]
    private static void UpdateThrowStrengthRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID)
    {
        // If not host OR single-player, return
        if (SemiFunc.IsNotMasterClient())
            return;
        
        #if DEBUG
        Entry.LogSource.LogInfo($"[{nameof(UpdateThrowStrengthRightAway)}] Upgrade: " + _steamID);
        #endif
        
        SyncBundle bundle = new(__instance, ___photonView, ___statsManager, _steamID);
        SyncManager.PlayerConsumedUpgrade(bundle, SyncUtil.GrabThrowId);
    }

    // UpgradePlayerGrabRange
    [HarmonyPrefix]
    [HarmonyPatch("UpdateGrabRangeRightAway", typeof(string))]
    private static void UpdateGrabRangeRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID)
    {
        // If not host OR single-player, return
        if (SemiFunc.IsNotMasterClient())
            return;
        
        #if DEBUG
        Entry.LogSource.LogInfo($"[{nameof(UpdateGrabRangeRightAway)}] Upgrade: " + _steamID);
        #endif
        
        SyncBundle bundle = new(__instance, ___photonView, ___statsManager, _steamID);
        SyncManager.PlayerConsumedUpgrade(bundle, SyncUtil.GrabRangeId);
    }
    
    // UpgradePlayerExtraJump
    [HarmonyPrefix]
    [HarmonyPatch("UpdateExtraJumpRightAway", typeof(string))]
    private static void UpdateExtraJumpRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID)
    {
        // If not host OR single-player, return
        if (SemiFunc.IsNotMasterClient())
            return;
        
        #if DEBUG
        Entry.LogSource.LogInfo($"[{nameof(UpdateExtraJumpRightAway)}] Upgrade: " + _steamID);
        #endif
        
        SyncBundle bundle = new(__instance, ___photonView, ___statsManager, _steamID);
        SyncManager.PlayerConsumedUpgrade(bundle, SyncUtil.ExtraJumpId);
    }
    
    // UpgradeMapPlayerCount
    [HarmonyPrefix]
    [HarmonyPatch("UpdateMapPlayerCountRightAway", typeof(string))]
    private static void UpdateMapPlayerCountRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID)
    {
        // If not host OR single-player, return
        if (SemiFunc.IsNotMasterClient())
            return;
        
        #if DEBUG
        Entry.LogSource.LogInfo($"[{nameof(UpdateMapPlayerCountRightAway)}] Upgrade: " + _steamID);
        #endif
        
        SyncBundle bundle = new(__instance, ___photonView, ___statsManager, _steamID);
        SyncManager.PlayerConsumedUpgrade(bundle, SyncUtil.MapPlayerCountId);
    }
}