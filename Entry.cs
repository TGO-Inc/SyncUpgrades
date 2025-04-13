using System;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Photon.Realtime;
using UnityEngine;

namespace SyncUpgrades;

[BepInPlugin(PluginId, PluginName, PluginVersion)]
public class Entry : BaseUnityPlugin
{
    private const string PluginName = "Sync Upgrades";
    private const string PluginVersion = "1.1.0";
    private const string PluginId = "TGO.SyncUpgrades";
    
    public static Entry? Instance { get; private set; }

    private ConfigEntry<bool>? _syncHealth;
    private ConfigEntry<bool>? _syncStamina;
    private ConfigEntry<bool>? _syncExtraJump;
    private ConfigEntry<bool>? _syncMapPlayerCount;
    private ConfigEntry<bool>? _syncGrabRange;
    private ConfigEntry<bool>? _syncGrabStrength;
    private ConfigEntry<bool>? _syncGrabThrow;
    private ConfigEntry<bool>? _syncSprintSpeed;
    private ConfigEntry<bool>? _syncTumbleLaunch;
    
    private static readonly AccessTools.FieldRef<PlayerAvatar, string> SteamIDRef 
        = AccessTools.FieldRefAccess<PlayerAvatar, string>("steamID");

    private static readonly Harmony Harmony = new(PluginId);
    
    private void Awake()
    {
        Instance = this;

        // Initialize configuration
        _syncHealth = Config.Bind("Sync", "Health", true, "Sync Max Health");
        _syncStamina = Config.Bind("Sync", "Stamina", true, "Sync Max Stamina");
        _syncExtraJump = Config.Bind("Sync", "Extra Jump", true, "Sync Extra Jump Count");
        _syncTumbleLaunch = Config.Bind("Sync", "Tumble Launch", true, "Sync Tumble Launch Count");
        _syncMapPlayerCount = Config.Bind("Sync", "Map Player Count", true, "Sync Map Player Count");
        _syncSprintSpeed = Config.Bind("Sync", "Sprint Speed", false, "Sync Sprint Speed");
        _syncGrabStrength = Config.Bind("Sync", "Grab Strength", true, "Sync Grab Strength");
        _syncGrabRange = Config.Bind("Sync", "Grab Range", true, "Sync Grab Range");
        _syncGrabThrow = Config.Bind("Sync", "Grab Throw", true, "Sync Grab Throw");

        // Apply Harmony patches
        Harmony.PatchAll();
        
        // Persist the game object
        gameObject.hideFlags = HideFlags.DontSaveInEditor;
        
        Logger.LogInfo("SyncHostUpgrades loaded!");
    }

    public void SyncUpgrades(Player newPlayer)
    {
        // Get hashcode is the same as .ActorNumber which is the Photon ID
        var workingPlayer = SemiFunc.PlayerAvatarGetFromPhotonID(newPlayer.GetHashCode());
        
        // Retrieve the local player upgrades
        var hostUpgrades = StatsManager.instance.FetchPlayerUpgrades(SteamIDRef(SemiFunc.PlayerAvatarLocal()));

        var steamId = SteamIDRef(workingPlayer);
        var upgrades = StatsManager.instance.FetchPlayerUpgrades(steamId);

        foreach (var key in hostUpgrades.Keys.Where(ShouldSync))
        {
            // If synchronization is enabled and the host's upgrade level is higher than the player's
            if (!hostUpgrades.TryGetValue(key, out var hostLevel) || hostLevel <= upgrades[key])
                continue;
            
            // Calculate the difference
            var diff = hostLevel - upgrades[key];

            // Call the corresponding upgrade method based on the upgrade type
            for (var i = 0; i < diff; i++)
                GetUpdateFunction(key)(steamId);

            // Log the synchronization
            Logger.LogInfo($"Synchronized upgrade for player {newPlayer.NickName}: {key}, from {upgrades[key]} to {hostLevel}");
        }
    }
    
    private static Func<string, int> GetUpdateFunction(string? key) => key switch
    {
        "Health" => PunManager.instance.UpgradePlayerHealth,
        "Stamina" => PunManager.instance.UpgradePlayerEnergy,
        "Extra Jump" => PunManager.instance.UpgradePlayerExtraJump,
        "Launch" => PunManager.instance.UpgradePlayerTumbleLaunch,
        "Map Player Count" => PunManager.instance.UpgradeMapPlayerCount,
        "Speed" => PunManager.instance.UpgradePlayerSprintSpeed,
        "Strength" => PunManager.instance.UpgradePlayerGrabStrength,
        "Range" => PunManager.instance.UpgradePlayerGrabRange,
        "Throw" => PunManager.instance.UpgradePlayerThrowStrength,
        _ => _ => 0
    };
    
    private bool ShouldSync(string? key) => key switch
    {
        "Health" => _syncHealth?.Value ?? false,
        "Stamina" => _syncStamina?.Value ?? false,
        "Extra Jump" => _syncExtraJump?.Value ?? false,
        "Launch" => _syncTumbleLaunch?.Value ?? false,
        "Map Player Count" => _syncMapPlayerCount?.Value ?? false,
        "Speed" => _syncSprintSpeed?.Value ?? false,
        "Strength" => _syncGrabStrength?.Value ?? false,
        "Range" => _syncGrabRange?.Value ?? false,
        "Throw" => _syncGrabThrow?.Value ?? false,
        _ => false
    };
}
