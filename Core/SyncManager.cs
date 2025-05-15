using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using JetBrains.Annotations;
using Sirenix.Utilities;

namespace SyncUpgrades.Core;

[PublicAPI]
public static class SyncManager
{
    private const string Section = "Sync";
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
        _syncHealth = Entry.BepConfig.Bind(Section, "Health", true, "Sync Max Health");
        _syncStamina = Entry.BepConfig.Bind(Section, "Stamina", true, "Sync Max Stamina");
        _syncExtraJump = Entry.BepConfig.Bind(Section, "Extra Jump", true, "Sync Extra Jump Count");
        _syncTumbleLaunch = Entry.BepConfig.Bind(Section, "Tumble Launch", true, "Sync Tumble Launch Count");
        _syncMapPlayerCount = Entry.BepConfig.Bind(Section, "Map Player Count", true, "Sync Map Player Count");
        _syncSprintSpeed = Entry.BepConfig.Bind(Section, "Sprint Speed", false, "Sync Sprint Speed");
        _syncGrabStrength = Entry.BepConfig.Bind(Section, "Grab Strength", true, "Sync Grab Strength");
        _syncGrabRange = Entry.BepConfig.Bind(Section, "Grab Range", true, "Sync Grab Range");
        _syncGrabThrow = Entry.BepConfig.Bind(Section, "Grab Throw", true, "Sync Grab Throw");
        _moddedUpgrades = Entry.BepConfig.Bind(Section, "Modded Upgrades", true, "Sync Misc Modded Upgrades");
    }

    public static void PlayerConsumedUpgrade(string steamId, UpgradeId upgrade, int newLevel) 
        => PlayerConsumedUpgrade(SyncBundle.Default(steamId), upgrade, newLevel);
    
    public static void PlayerConsumedUpgrade(SyncBundle bundle, UpgradeId upgrade, int newLevel = 0)
    {
        // If synchronization is enabled
        if (!ShouldSync(upgrade))
            return;
        
        // Upgrade host if not host and return
        if (bundle.SteamId != SyncUtil.HostSteamId) 
        {
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
        
        // Sync the upgrade to all clients
        SyncHostToAll(bundle);
    }
    
    public static bool SyncHostToTarget(string targetSteamId)
    {
        SyncBundle bundle = SyncBundle.Default(targetSteamId);
        bool result = SyncHostToTarget(bundle, SyncUtil.GetPlayer(targetSteamId));
        SyncUtil.SyncStatsDictionaryToAll(bundle);
        return result;
    }

    public static void SyncHostToAll() 
        => SyncHostToAll(SyncBundle.Default(SyncUtil.HostSteamId));

    public static void SyncHostToAll(SyncBundle bundle)
    {
        if (SemiFunc.PlayerGetAll().Where(NotHost).Aggregate(false, (current, player) => current || SyncHostToTarget(bundle, player)))
            SyncUtil.SyncStatsDictionaryToAll(bundle);
    }

    private static bool SyncHostToTarget(SyncBundle bundle, PlayerAvatar target)
        => SyncFromSourceToTarget(bundle, SyncUtil.Local, target);
    
    private static bool SyncFromSourceToTarget(SyncBundle bundle, PlayerAvatar source, PlayerAvatar target)
    {
        string sourceId = source.SteamId();
        string targetId = target.SteamId();
        
        if (targetId == sourceId)
            return false;
        
        #if DEBUG
        Entry.LogSource.LogInfo($"[{nameof(SyncFromSourceToTarget)}] [{targetId}]");
        #endif
        
        var hasChanged = false;
        foreach (UpgradeId? upgradeId in SyncUtil.GetUpgradeTypes(bundle).Where(ShouldSync))
        {
            #if DEBUG
            Entry.LogSource.LogInfo($"[{nameof(SyncFromSourceToTarget)}] {upgradeId}");
            #endif
            
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
                for (var i = 0; i < diff; i++)
                    SyncUtil.CallRPCOnePlayer(bundle, target, upgradeId);
            else
                SyncUtil.UpgradeModded(bundle, target, upgradeId, diff);

            // Log the synchronization
            Entry.LogSource.LogInfo($"[{nameof(SyncFromSourceToTarget)}] Synchronized upgrade for player {targetId}: {upgradeId.RawName} ({upgradeId.Type}), from {targetLevel} to {sourceLevel}");
        }
        
        return hasChanged;
    }
    
    private static bool NotHost(PlayerAvatar avatar) => avatar.SteamId() != SyncUtil.HostSteamId;
    
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