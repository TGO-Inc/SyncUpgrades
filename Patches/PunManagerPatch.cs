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
    [IgnoreMethodPatchException]
    [HarmonyPatch(nameof(UpdateHealthRightAway), typeof(string), typeof(int))]
    private static void UpdateHealthRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID, int value)
        => UpgradeWrapper(__instance, ___photonView, ___statsManager, _steamID, value, SyncUtil.HealthId);
    
    // UpgradePlayerEnergy
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [HarmonyPatch(nameof(UpdateEnergyRightAway), typeof(string), typeof(int))]
    private static void UpdateEnergyRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID, int value)
        => UpgradeWrapper(__instance, ___photonView, ___statsManager, _steamID, value, SyncUtil.StaminaId);
    
    // UpgradePlayerTumbleLaunch
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [HarmonyPatch(nameof(UpdateTumbleLaunchRightAway), typeof(string), typeof(int))]
    private static void UpdateTumbleLaunchRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID, int value)
        => UpgradeWrapper(__instance, ___photonView, ___statsManager, _steamID, value, SyncUtil.TumbleLaunchId);
    
    // UpgradePlayerSprintSpeed
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [HarmonyPatch(nameof(UpdateSprintSpeedRightAway), typeof(string), typeof(int))]
    private static void UpdateSprintSpeedRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID, int value)
        => UpgradeWrapper(__instance, ___photonView, ___statsManager, _steamID, value, SyncUtil.SprintSpeedId);
    
    // UpgradePlayerGrabStrength
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [HarmonyPatch(nameof(UpdateGrabStrengthRightAway), typeof(string), typeof(int))]
    private static void UpdateGrabStrengthRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID, int value)
        => UpgradeWrapper(__instance, ___photonView, ___statsManager, _steamID, value, SyncUtil.GrabStrengthId);
    
    // UpgradePlayerThrowStrength
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [HarmonyPatch(nameof(UpdateThrowStrengthRightAway), typeof(string), typeof(int))]
    private static void UpdateThrowStrengthRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID, int value)
        => UpgradeWrapper(__instance, ___photonView, ___statsManager, _steamID, value, SyncUtil.ThrowStrengthId);

    // UpgradePlayerGrabRange
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [HarmonyPatch(nameof(UpdateGrabRangeRightAway), typeof(string), typeof(int))]
    private static void UpdateGrabRangeRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID, int value)
        => UpgradeWrapper(__instance, ___photonView, ___statsManager, _steamID, value, SyncUtil.GrabRangeId);
    
    // UpgradePlayerExtraJump
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [HarmonyPatch(nameof(UpdateExtraJumpRightAway), typeof(string), typeof(int))]
    private static void UpdateExtraJumpRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID, int value)
        => UpgradeWrapper(__instance, ___photonView, ___statsManager, _steamID, value, SyncUtil.ExtraJumpId);
    
    // UpgradeMapPlayerCount
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [HarmonyPatch(nameof(UpdateMapPlayerCountRightAway), typeof(string), typeof(int))]
    private static void UpdateMapPlayerCountRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID, int value)
        => UpgradeWrapper(__instance, ___photonView, ___statsManager, _steamID, value, SyncUtil.MapPlayerCountId);
    
    // UpgradePlayerTumbleWings
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [IgnoreMethodPatchException]
    [HarmonyPatch(nameof(UpdateTumbleWingsRightAway), typeof(string), typeof(int))]
    private static void UpdateTumbleWingsRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID, int value)
        => UpgradeWrapper(__instance, ___photonView, ___statsManager, _steamID, value, SyncUtil.TumbleWingsId);
    
    // UpgradeCrouchRest
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [IgnoreMethodPatchException]
    [HarmonyPatch(nameof(UpdateCrouchRestRightAway), typeof(string), typeof(int))]
    private static void UpdateCrouchRestRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID, int value)
        => UpgradeWrapper(__instance, ___photonView, ___statsManager, _steamID, value, SyncUtil.CrouchRestId);

    // UpgradeTumbleClimb
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [IgnoreMethodPatchException]
    [HarmonyPatch(nameof(UpdateTumbleClimbRightAway), typeof(string), typeof(int))]
    private static void UpdateTumbleClimbRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID, int value)
        => UpgradeWrapper(__instance, ___photonView, ___statsManager, _steamID, value, SyncUtil.TumbleClimbId);

    // UpgradeDeathHeadBattery
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [IgnoreMethodPatchException]
    [HarmonyPatch(nameof(UpdateDeathHeadBatteryRightAway), typeof(string), typeof(int))]
    private static void UpdateDeathHeadBatteryRightAway(PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, string _steamID, int value)
        => UpgradeWrapper(__instance, ___photonView, ___statsManager, _steamID, value, SyncUtil.TumbleClimbId);

    private static void UpgradeWrapper(
        PunManager __instance, PhotonView ___photonView, StatsManager ___statsManager, 
        string _steamID, int value, UpgradeId upgrade, [CallerMemberName] string methodName = "Unknown Caller Method")
    {
        // If not host OR single-player, return
        if (SemiFunc.IsNotMasterClient())
            return;
        
        #if DEBUG
        Entry.LogSource.LogInfo($"[{methodName}] Upgrade: " + _steamID + ", Value: " + value);
        #endif
        
        SyncBundle bundle = new(new PunManagerWrapper(__instance), ___photonView, ___statsManager, _steamID);
        SyncManager.PlayerConsumedUpgrade(bundle, upgrade, value);
    }
}