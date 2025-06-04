using System.Runtime.CompilerServices;
using HarmonyLib;
using Photon.Pun;
using SyncUpgrades.Core;
using SyncUpgrades.Core.Internal;

namespace SyncUpgrades.Patches;

[HarmonyPatch(typeof(PunManager))]
internal class PunManagerPatch
{
    // UpgradePlayerHealth
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [HarmonyPatch(nameof(UpdateHealthRightAway), typeof(string))]
    private static void UpdateHealthRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string playerName)
        => UpgradeWrapper(__instance, ___photonView, ___statsManager, playerName, SyncUtil.HealthId);
    
    // UpgradePlayerEnergy
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [HarmonyPatch(nameof(UpdateEnergyRightAway), typeof(string))]
    private static void UpdateEnergyRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID)
        => UpgradeWrapper(__instance, ___photonView, ___statsManager, _steamID, SyncUtil.StaminaId);
    
    // UpgradePlayerTumbleLaunch
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [HarmonyPatch(nameof(UpdateTumbleLaunchRightAway), typeof(string))]
    private static void UpdateTumbleLaunchRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID)
        => UpgradeWrapper(__instance, ___photonView, ___statsManager, _steamID, SyncUtil.TumbleLaunchId);
    
    // UpgradePlayerSprintSpeed
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [HarmonyPatch(nameof(UpdateSprintSpeedRightAway), typeof(string))]
    private static void UpdateSprintSpeedRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID)
        => UpgradeWrapper(__instance, ___photonView, ___statsManager, _steamID, SyncUtil.SprintSpeedId);
    
    // UpgradePlayerGrabStrength
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [HarmonyPatch(nameof(UpdateGrabStrengthRightAway), typeof(string))]
    private static void UpdateGrabStrengthRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID)
        => UpgradeWrapper(__instance, ___photonView, ___statsManager, _steamID, SyncUtil.GrabStrengthId);
    
    // UpgradePlayerThrowStrength
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [HarmonyPatch(nameof(UpdateThrowStrengthRightAway), typeof(string))]
    private static void UpdateThrowStrengthRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID)
        => UpgradeWrapper(__instance, ___photonView, ___statsManager, _steamID, SyncUtil.GrabThrowId);

    // UpgradePlayerGrabRange
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [HarmonyPatch(nameof(UpdateGrabRangeRightAway), typeof(string))]
    private static void UpdateGrabRangeRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID)
        => UpgradeWrapper(__instance, ___photonView, ___statsManager, _steamID, SyncUtil.GrabRangeId);
    
    // UpgradePlayerExtraJump
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [HarmonyPatch(nameof(UpdateExtraJumpRightAway), typeof(string))]
    private static void UpdateExtraJumpRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID)
        => UpgradeWrapper(__instance, ___photonView, ___statsManager, _steamID, SyncUtil.ExtraJumpId);
    
    // UpgradeMapPlayerCount
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [HarmonyPatch(nameof(UpdateMapPlayerCountRightAway), typeof(string))]
    private static void UpdateMapPlayerCountRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID)
        => UpgradeWrapper(__instance, ___photonView, ___statsManager, _steamID, SyncUtil.MapPlayerCountId);
    
    // UpgradePlayerTumbleWings
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [IgnoreMethodNotFoundPatchException]
    [HarmonyPatch(nameof(UpdateTumbleWingsRightAway), typeof(string))]
    private static void UpdateTumbleWingsRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID)
        => UpgradeWrapper(__instance, ___photonView, ___statsManager, _steamID, SyncUtil.TumbleWingsId);
    
    // UpgradeCrouchRest
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [IgnoreMethodNotFoundPatchException]
    [HarmonyPatch(nameof(UpdateCrouchRestRightAway), typeof(string))]
    private static void UpdateCrouchRestRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID)
        => UpgradeWrapper(__instance, ___photonView, ___statsManager, _steamID, SyncUtil.CrouchRestId);

    private static void UpgradeWrapper(
        PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, 
        string _steamID, UpgradeId upgrade, [CallerMemberName] string methodName = "Unknown Caller Method")
    {
        // If not host OR single-player, return
        if (SemiFunc.IsNotMasterClient())
            return;
        
        #if DEBUG
        Entry.LogSource.LogInfo($"[{methodName}] Upgrade: " + _steamID);
        #endif
        
        SyncBundle bundle = new(new PunManagerWrapper(__instance), ___photonView, ___statsManager, _steamID);
        SyncManager.PlayerConsumedUpgrade(bundle, upgrade);
    }
}