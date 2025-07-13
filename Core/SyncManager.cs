using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using JetBrains.Annotations;
using REPOLib.Modules;

namespace SyncUpgrades.Core;

[PublicAPI]
public static class SyncManager
{
    private const string Section = "Default Upgrades";
    private const string ModdedSection = "Modded Upgrades";
    private static ConfigEntry<bool> _syncHealth         = Entry.BepConfig.Bind(Section, "Health", true, "Sync Max Health");
    private static ConfigEntry<bool> _syncStamina        = Entry.BepConfig.Bind(Section, "Stamina", true, "Sync Max Stamina");
    private static ConfigEntry<bool> _syncExtraJump      = Entry.BepConfig.Bind(Section, "Extra Jump", true, "Sync Extra Jump");
    private static ConfigEntry<bool> _syncMapPlayerCount = Entry.BepConfig.Bind(Section, "Map Player Count", true, "Sync Map Player Count");
    private static ConfigEntry<bool> _syncGrabRange      = Entry.BepConfig.Bind(Section, "Grab Range", true, "Sync Grab Range");
    private static ConfigEntry<bool> _syncGrabStrength   = Entry.BepConfig.Bind(Section, "Grab Strength", true, "Sync Grab Strength");
    private static ConfigEntry<bool> _syncThrowStrength  = Entry.BepConfig.Bind(Section, "Throw Strength", true, "Sync Throw Strength");
    private static ConfigEntry<bool> _syncSprintSpeed    = Entry.BepConfig.Bind(Section, "Sprint Speed", true, "Sync Sprint Speed");
    private static ConfigEntry<bool> _syncTumbleLaunch   = Entry.BepConfig.Bind(Section, "Tumble Launch", true, "Sync Tumble Launch");
    private static ConfigEntry<bool> _syncTumbleWings    = Entry.BepConfig.Bind(Section, "Tumble Wings", true, "Sync Tumble Wings");
    private static ConfigEntry<bool> _syncCrouchRest     = Entry.BepConfig.Bind(Section, "Crouch Rest", true, "Sync Crouch Rest");
    internal static ConcurrentDictionary<UpgradeId, ConfigEntry<bool>> registeredModdedUpgrades = [];

    internal static void Init()
    {
        foreach (PlayerUpgrade? upgrade in Upgrades.PlayerUpgrades)
            RegisterModdedUpgrade(upgrade);
    }
    
    /// <summary>
    /// Registers a modded upgrade in the configuration for user customization.
    /// </summary>
    /// <param name="upgrade"></param>
    public static void RegisterModdedUpgrade(PlayerUpgrade upgrade)
    {
        string rawName = SyncUtil.FixKey(upgrade.UpgradeId);
        var upgradeId = UpgradeId.New(rawName);
        
        // Skip if the upgrade is already registered
        if (registeredModdedUpgrades.ContainsKey(upgradeId))
            return;
        
        #if DEBUG
        Entry.LogSource.LogInfo($"[{nameof(RegisterModdedUpgrade)}] {upgradeId.RawName}");
        #endif
        
        registeredModdedUpgrades.TryAdd(upgradeId, 
            Entry.BepConfig.Bind(ModdedSection, SyncUtil.TrimKey(upgradeId.RawName), true, $"Sync {upgradeId.RawName}"));
    }

    /// <summary>
    /// Informs the <see cref="SyncManager"/> that an upgrade has been consumed by a player.
    /// </summary>
    /// <param name="steamId"></param>
    /// <param name="upgrade"></param>
    /// <param name="newLevel"></param>
    public static void PlayerConsumedUpgrade(string steamId, UpgradeId upgrade, int newLevel) 
        => PlayerConsumedUpgrade(SyncBundle.Default(steamId), upgrade, newLevel);
    
    /// <summary>
    /// <inheritdoc cref="PlayerConsumedUpgrade(string, UpgradeId, int)"/>
    /// </summary>
    /// <param name="bundle"></param>
    /// <param name="upgrade"></param>
    /// <param name="newLevel"></param>
    public static void PlayerConsumedUpgrade(SyncBundle bundle, UpgradeId upgrade, int newLevel = 0)
    {
        // If synchronization is enabled
        if (!ShouldSync(upgrade))
            return;
        
        // Upgrade host if not host and return
        if (bundle.SteamId != SyncUtil.HostSteamId) 
        {
            #if DEBUG
            Entry.LogSource.LogInfo($"[{nameof(PlayerConsumedUpgrade)}] [{bundle.SteamId}] {upgrade} != {nameof(SyncHostToAll)}()");
            #endif
            
            if (newLevel > 0)
            {
                // load the upgrade dictionary
                Dictionary<string, int> upgradeDictionary = SyncUtil.GetUpgrades(bundle.Stats, upgrade);
            
                // get level difference
                int targetLevel = upgradeDictionary.GetValueOrDefault(SyncUtil.HostSteamId, 0);
                int difference = newLevel - targetLevel;
                
                // Upgrade the host
                for (var i = 0; i < difference; i++)
                    SyncUtil.CallUpdateFunction(bundle, SyncUtil.HostSteamId, upgrade);

                return;
            }
            
            SyncUtil.CallUpdateFunction(bundle, SyncUtil.HostSteamId, upgrade);
            return;
        }
        
        #if DEBUG
        Entry.LogSource.LogInfo($"[{nameof(PlayerConsumedUpgrade)}] [{bundle.SteamId}] {upgrade} => {nameof(SyncHostToAll)}()");
        #endif
        
        // Sync the upgrade to all clients
        // Dont do this because it doesnt work. Maybe register something with REPOLib?
        // TODO: Register upgrade sync with REPOLib
        // SyncHostToAll(bundle);
    }
    
    /// <summary>
    /// Synchronizes the host's upgrades to the target player.
    /// </summary>
    /// <param name="targetSteamId"></param>
    /// <returns></returns>
    public static bool SyncHostToTarget(string targetSteamId)
    {
        SyncBundle bundle = SyncBundle.Default(targetSteamId);
        if (!SyncHostToTarget(bundle, SyncUtil.GetPlayer(targetSteamId)))
            return false;
        
        SyncUtil.SyncStatsDictionaryToAll(bundle);
        return true;
    }

    /// <summary>
    /// Synchronizes the host's upgrades to all players.
    /// </summary>
    public static void SyncHostToAll() 
        => SyncHostToAll(SyncBundle.Default(SyncUtil.HostSteamId));

    /// <summary>
    /// <inheritdoc cref="SyncHostToAll()"/>
    /// </summary>
    /// <param name="bundle"></param>
    public static void SyncHostToAll(SyncBundle bundle)
    {
        var players = SemiFunc.PlayerGetAll().Where(NotHost).ToArray();   
        
        #if DEBUG
        Entry.LogSource.LogInfo($"[{nameof(SyncHostToAll)}] Syncing upgrades for all players: [ {string.Join(", ", players.Select(p => p.SteamId()))} ]");
        #endif

        bool[] results = players.Select(player => SyncHostToTarget(bundle, player)).ToArray();
        if (results.Any()) SyncUtil.SyncStatsDictionaryToAll(bundle);
    }

    /// <summary>
    /// Synchronizes the host's upgrades to the target player.
    /// </summary>
    /// <param name="bundle"></param>
    /// <param name="target"></param>
    /// <returns>true if any changes were made. else false</returns>
    private static bool SyncHostToTarget(SyncBundle bundle, PlayerAvatar target)
        => SyncFromSourceToTarget(bundle, SyncUtil.Local, target);
    
    /// <summary>
    /// Synchronizes the source player's upgrades to the target player.
    /// </summary>
    /// <param name="bundle"></param>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    private static bool SyncFromSourceToTarget(SyncBundle bundle, PlayerAvatar source, PlayerAvatar target)
    {
        string sourceId = source.SteamId();
        string targetId = target.SteamId();
        
        #if DEBUG
        Entry.LogSource.LogInfo($"[{nameof(SyncFromSourceToTarget)}] [{sourceId}] [{targetId}]");
        #endif
        
        if (targetId == sourceId)
            return false;
        
        var hasChanged = false;
        foreach (UpgradeId? upgradeId in SyncUtil.GetUpgradeTypes(bundle).Where(ShouldSync))
        {
            // load the upgrade dictionary
            Dictionary<string, int> upgradeDictionary = SyncUtil.GetUpgrades(bundle.Stats, upgradeId);
            
            // get levels
            int targetLevel = upgradeDictionary.GetValueOrDefault(targetId, 0);
            int sourceLevel = upgradeDictionary.GetValueOrDefault(sourceId, 0);
            
            // If the source's level is higher than the target's
            if (sourceLevel <= targetLevel)
                continue;
            
            // Calculate the difference
            int diff = sourceLevel - targetLevel;
            if (!(hasChanged |= diff > 0))
                continue;

            // Call the corresponding upgrade method based on the upgrade type
            if (upgradeId.Type != UpgradeType.Modded)
                SyncUtil.AddToStatsDictionary(bundle, targetId, upgradeId, diff);
                // for (var i = 0; i < diff; i++)
                //     SyncUtil.CallRPCOnePlayer(bundle, target, upgradeId);
            else
                SyncUtil.UpgradeModded(bundle, target, upgradeId, diff);

            // Log the synchronization
            Entry.LogSource.LogInfo(
                $"[{nameof(SyncFromSourceToTarget)}] Synchronized upgrade for player {targetId}: {upgradeId.RawName} ({upgradeId.Type}), from {targetLevel} to {sourceLevel}");
        }
        
        return hasChanged;
    }
    
    /// <summary>
    /// Checks if the player is not the host.
    /// </summary>
    /// <param name="avatar"></param>
    /// <returns></returns>
    private static bool NotHost(PlayerAvatar avatar) => avatar.SteamId() != SyncUtil.HostSteamId;
    
    /// <summary>
    /// Checks if the upgrade should be synchronized based on its type.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private static bool ShouldSync(UpgradeId key) => key.Type switch
    {
        UpgradeType.Health => _syncHealth.Value,
        UpgradeType.Stamina => _syncStamina.Value,
        UpgradeType.ExtraJump => _syncExtraJump.Value,
        UpgradeType.TumbleLaunch => _syncTumbleLaunch.Value,
        UpgradeType.MapPlayerCount => _syncMapPlayerCount.Value,
        UpgradeType.SprintSpeed => _syncSprintSpeed.Value,
        UpgradeType.GrabStrength => _syncGrabStrength.Value,
        UpgradeType.GrabRange => _syncGrabRange.Value,
        UpgradeType.ThrowStrength => _syncThrowStrength.Value,
        UpgradeType.TumbleWings => _syncTumbleWings.Value,
        UpgradeType.CrouchRest => _syncCrouchRest.Value,
        UpgradeType.Modded => registeredModdedUpgrades.TryGetValue(key, out ConfigEntry<bool> value) && value.Value,
        var _ => false
    };
}