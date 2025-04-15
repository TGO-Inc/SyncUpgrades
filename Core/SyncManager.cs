using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using JetBrains.Annotations;

namespace SyncUpgrades.Core;

[PublicAPI]
public static class SyncManager
{
    private static ConfigEntry<bool>? _syncHealth;
    private static ConfigEntry<bool>? _syncStamina;
    private static ConfigEntry<bool>? _syncExtraJump;
    private static ConfigEntry<bool>? _syncMapPlayerCount;
    private static ConfigEntry<bool>? _syncGrabRange;
    private static ConfigEntry<bool>? _syncGrabStrength;
    private static ConfigEntry<bool>? _syncGrabThrow;
    private static ConfigEntry<bool>? _syncSprintSpeed;
    private static ConfigEntry<bool>? _syncTumbleLaunch;
    private static ConfigEntry<bool>? _moddedUpgrades;

    internal static void Init()
    {
        // Initialize configuration
        _syncHealth = Entry.BepConfig.Bind("Sync", "Health", true, "Sync Max Health");
        _syncStamina = Entry.BepConfig.Bind("Sync", "Stamina", true, "Sync Max Stamina");
        _syncExtraJump = Entry.BepConfig.Bind("Sync", "Extra Jump", true, "Sync Extra Jump Count");
        _syncTumbleLaunch = Entry.BepConfig.Bind("Sync", "Tumble Launch", true, "Sync Tumble Launch Count");
        _syncMapPlayerCount = Entry.BepConfig.Bind("Sync", "Map Player Count", true, "Sync Map Player Count");
        _syncSprintSpeed = Entry.BepConfig.Bind("Sync", "Sprint Speed", false, "Sync Sprint Speed");
        _syncGrabStrength = Entry.BepConfig.Bind("Sync", "Grab Strength", true, "Sync Grab Strength");
        _syncGrabRange = Entry.BepConfig.Bind("Sync", "Grab Range", true, "Sync Grab Range");
        _syncGrabThrow = Entry.BepConfig.Bind("Sync", "Grab Throw", true, "Sync Grab Throw");
        _moddedUpgrades = Entry.BepConfig.Bind("Sync", "Modded Upgrades", true, "Sync Misc Modded Upgrades");
    }
    
    public static void SyncUpgrades(string steamID) => SyncUpgrades(
        new PunBundle(PunManager.instance, PunManager.instance.GetView(), StatsManager.instance, steamID), 
        SemiFunc.PlayerAvatarGetFromSteamID(steamID));
    
    private static IEnumerable<UpgradeId> UpgradeTypes(PunBundle bundle) => bundle.Stats.dictionaryOfDictionaries
        .Where(kvp => kvp.Key.StartsWith("playerUpgrade") || kvp.Key.StartsWith("appliedPlayerUpgrade"))
        .Select(kvp => new UpgradeId(kvp.Key));
    
    private static void SyncUpgrades(PunBundle bundle, PlayerAvatar workingPlayer)
    {
        var hostSteamId = SemiUtil.HostSteamId;
        var steamId = workingPlayer.SteamId();
        
        if (steamId == SemiUtil.HostSteamId)
            return;
        
        // Retrieve the local player upgrades
        foreach (var upgradeId in UpgradeTypes(bundle).Where(ShouldSync))
        {
            // load the upgrade dictionary
            var upgradeDictionary = SemiUtil.GetUpgrades(bundle.Stats, upgradeId);
            
            // get player level
            var oldPlayerValue = upgradeDictionary.GetValueOrDefault(steamId, 0);

            // If synchronization is enabled and the host's upgrade level is higher than the player's
            if (!upgradeDictionary.TryGetValue(hostSteamId, out var hostLevel) || hostLevel <= oldPlayerValue)
                continue;
            
            // Calculate the difference
            var diff = hostLevel - oldPlayerValue;

            // Call the corresponding upgrade method based on the upgrade type
            if (upgradeId.Type != UpgradeType.Modded)
                for (var i = 0; i < diff; i++)
                    SemiUtil.CallRPC(bundle, steamId, upgradeId);
            else
                SemiUtil.IncrementUpdateDictAndSync(bundle, steamId, upgradeId, diff);

            // Log the synchronization
            Entry.LogSource.LogInfo($"Synchronized upgrade for player {steamId}: {upgradeId.RawName} ({upgradeId.Type}), from {oldPlayerValue} to {hostLevel}");
        }
    }
    
    public static void PlayerUpgradeStat(PunBundle bundle, UpgradeId upgradeId)
    {
        // If synchronization is enabled
        if (!ShouldSync(upgradeId))
            return;
        
        // Upgrade host if not host and return
        if (bundle.SteamId != SemiUtil.HostSteamId) 
        {
            SemiUtil.CallUpdateFunction(bundle.Manager, SemiUtil.HostSteamId, upgradeId.Type);
            return;
        }

        // Sync the upgrade to all clients
        SyncAll(bundle);
    }

    public static void SyncAll(PunBundle bundle)
    {
        // Skip sync with host, then sync the upgrades for all players
        foreach (var updatePlayer in SemiFunc.PlayerGetAll().Where(avatar => avatar.SteamId() != SemiUtil.HostSteamId))
            SyncUpgrades(bundle, updatePlayer);
    }
    
    private static bool ShouldSync(UpgradeId key) => key.Type switch
    {
        UpgradeType.Health => _syncHealth?.Value ?? false,
        UpgradeType.Stamina => _syncStamina?.Value ?? false,
        UpgradeType.ExtraJump => _syncExtraJump?.Value ?? false,
        UpgradeType.TumbleLaunch => _syncTumbleLaunch?.Value ?? false,
        UpgradeType.MapPlayerCount => _syncMapPlayerCount?.Value ?? false,
        UpgradeType.SprintSpeed => _syncSprintSpeed?.Value ?? false,
        UpgradeType.GrabStrength => _syncGrabStrength?.Value ?? false,
        UpgradeType.GrabRange => _syncGrabRange?.Value ?? false,
        UpgradeType.GrabThrow => _syncGrabThrow?.Value ?? false,
        _ or UpgradeType.Modded => _moddedUpgrades?.Value ?? false
    };
}