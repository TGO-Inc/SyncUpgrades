using System.Runtime.CompilerServices;
using HarmonyLib;
using SyncUpgrades.Core;
using UnityEngine;

namespace SyncUpgrades.Patches;

[HarmonyPatch(typeof(ItemUpgrade))]
internal class ItemUpgradePatch
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [HarmonyPatch(nameof(Start))]
    // ReSharper disable once InconsistentNaming
    private static void Start(ItemUpgrade __instance)
    {
        var upgradeClass = (MonoBehaviour)__instance.upgradeEvent.GetPersistentTarget(0);
        var itemToggle = upgradeClass.GetComponent<ItemToggle>();

        UpgradeId upgrade = upgradeClass switch
        {
            ItemUpgradeMapPlayerCount => SyncUtil.MapPlayerCountId,
            ItemUpgradePlayerCrouchRest => SyncUtil.CrouchRestId,
            ItemUpgradePlayerEnergy => SyncUtil.StaminaId,
            ItemUpgradePlayerExtraJump => SyncUtil.ExtraJumpId,
            ItemUpgradePlayerGrabRange => SyncUtil.GrabRangeId,
            ItemUpgradePlayerGrabStrength => SyncUtil.GrabStrengthId,
            ItemUpgradePlayerGrabThrow => SyncUtil.ThrowStrengthId,
            ItemUpgradePlayerHealth => SyncUtil.HealthId,
            ItemUpgradePlayerSprintSpeed => SyncUtil.SprintSpeedId,
            ItemUpgradePlayerTumbleLaunch => SyncUtil.TumbleLaunchId,
            ItemUpgradePlayerTumbleWings => SyncUtil.TumbleWingsId,
            var _ => UpgradeId.New("UnknownUpgrade")
        };

        __instance.upgradeEvent.AddListener(UpgradeEvent);
        return;

        void UpgradeEvent() => UpgradeWrapper(itemToggle, upgrade);
    }

    private static readonly AccessTools.FieldRef<ItemToggle, int> ItemTogglePhotonID = AccessTools.FieldRefAccess<ItemToggle, int>("playerTogglePhotonID");
    private static void UpgradeWrapper(ItemToggle itemToggle, UpgradeId upgrade, [CallerMemberName] string methodName = "Unknown Caller Method")
    {
        // If not host OR single-player, return
        if (SemiFunc.IsNotMasterClient())
            return;
        
        string? steamID = SemiFunc.PlayerGetSteamID(SemiFunc.PlayerAvatarGetFromPhotonID(ItemTogglePhotonID(itemToggle)));
        
        #if DEBUG
        Entry.LogSource.LogInfo($"[{methodName}] Upgrade: " + steamID);
        #endif
        
        // SyncBundle bundle = new(itemToggle, steamID);
        // SyncManager.PlayerConsumedUpgrade(bundle, upgrade);
    }
}

