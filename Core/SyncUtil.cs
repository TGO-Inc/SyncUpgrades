using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using System.Linq;
using REPOLib.Modules;
using SyncUpgrades.Core.Internal;

namespace SyncUpgrades.Core;

[PublicAPI]
public static class SyncUtil
{
    private const RpcTarget Others = RpcTarget.Others;
    private const string PlayerUpgrade = "playerUpgrade";
    private const string AppliedPlayerUpgrade = "appliedPlayerUpgrade";
    
    public static readonly UpgradeId HealthId         = new(UpgradeType.Health);
    public static readonly UpgradeId StaminaId        = new(UpgradeType.Stamina);
    public static readonly UpgradeId ExtraJumpId      = new(UpgradeType.ExtraJump);
    public static readonly UpgradeId TumbleLaunchId   = new(UpgradeType.TumbleLaunch);
    public static readonly UpgradeId MapPlayerCountId = new(UpgradeType.MapPlayerCount);
    public static readonly UpgradeId SprintSpeedId    = new(UpgradeType.SprintSpeed);
    public static readonly UpgradeId GrabStrengthId   = new(UpgradeType.GrabStrength);
    public static readonly UpgradeId GrabRangeId      = new(UpgradeType.GrabRange);
    public static readonly UpgradeId ThrowStrengthId  = new(UpgradeType.ThrowStrength);
    public static readonly UpgradeId TumbleWingsId    = new(UpgradeType.TumbleWings);
    public static readonly UpgradeId CrouchRestId     = new(UpgradeType.CrouchRest);
    
    private static readonly ConcurrentQueue<ISyncRequest> SyncQueue = [];
    
    /// <summary>
    /// Get the local player steam ID.
    /// </summary>
    public static string HostSteamId => Local.SteamId();
    
    /// <summary>
    /// Get the local player avatar.
    /// </summary>
    public static PlayerAvatar Local => SemiFunc.PlayerAvatarLocal();

    /// <summary>
    /// Trim the key to remove the prefix.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string TrimKey(string? key)
    {
        if (string.IsNullOrEmpty(key)) return string.Empty;
        if (key.StartsWith(AppliedPlayerUpgrade)) return key[20..];
        return key.StartsWith(PlayerUpgrade) ? key[13..] : key;
    }

    /// <summary>
    /// Fix the key to be a valid player upgrade key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string FixKey(string? key)
    {
        if (string.IsNullOrEmpty(key)) return string.Empty;
        if (!key.StartsWith(PlayerUpgrade)) return PlayerUpgrade+key;
        return key;
    }
    
    /// <summary>
    /// Get the PlayerAvatar from a Steam ID.
    /// </summary>
    /// <param name="targetSteamId"></param>
    /// <returns></returns>
    public static PlayerAvatar GetPlayer(string targetSteamId) => SemiFunc.PlayerAvatarGetFromSteamID(targetSteamId);
    
    /// <summary>
    /// Retrieves all the valid upgrade types.
    /// </summary>
    /// <param name="bundle"></param>
    /// <returns></returns>
    public static IEnumerable<UpgradeId> GetUpgradeTypes(SyncBundle bundle) 
        => bundle.Stats.dictionaryOfDictionaries
                 .Where(kvp => kvp.Key.StartsWith(PlayerUpgrade) || kvp.Key.StartsWith(AppliedPlayerUpgrade))
                 .Select(UpgradeId.New)
                 .Union(SyncManager.registeredModdedUpgrades.Keys);

    /// <summary>
    /// Converts a string key to an UpgradeType.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static UpgradeType GetUpgradeType(string? key) => TrimKey(key) switch
    {
        "Health" => UpgradeType.Health,
        "Stamina" => UpgradeType.Stamina,
        "ExtraJump" => UpgradeType.ExtraJump,
        "Launch" => UpgradeType.TumbleLaunch,
        "MapPlayerCount" => UpgradeType.MapPlayerCount,
        "Speed" => UpgradeType.SprintSpeed,
        "Strength" => UpgradeType.GrabStrength,
        "Range" => UpgradeType.GrabRange,
        "Throw" => UpgradeType.ThrowStrength,
        "TumbleWings" => UpgradeType.TumbleWings,
        "CrouchRest" => UpgradeType.CrouchRest,
        // Assume
        _ => UpgradeType.Modded
    };

    /// <summary>
    /// Converts an UpgradeType to a string name.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static string GetUpgradeName(UpgradeType key) => key switch
    {
        UpgradeType.Health => "Health",
        UpgradeType.Stamina => "Stamina",
        UpgradeType.ExtraJump => "ExtraJump",
        UpgradeType.TumbleLaunch => "Launch",
        UpgradeType.MapPlayerCount => "MapPlayerCount",
        UpgradeType.SprintSpeed => "Speed",
        UpgradeType.GrabStrength => "Strength",
        UpgradeType.GrabRange => "Range",
        UpgradeType.ThrowStrength => "Throw",
        UpgradeType.TumbleWings => "TumbleWings",
        UpgradeType.CrouchRest => "CrouchRest",
        UpgradeType.Modded => "Modded: Unknown",
        _ => throw new ArgumentException($"Invalid UpgradeType for {nameof(GetUpgradeName)}")
    };

    /// <summary>
    /// Get the upgrade dictionary for a given UpgradeId.
    /// </summary>
    /// <param name="stats"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static Dictionary<string, int> GetUpgrades(StatsManager stats, UpgradeId id) => id.Type switch
    {
        UpgradeType.Health => stats.playerUpgradeHealth,
        UpgradeType.Stamina => stats.playerUpgradeStamina,
        UpgradeType.ExtraJump => stats.playerUpgradeExtraJump,
        UpgradeType.TumbleLaunch => stats.playerUpgradeLaunch,
        UpgradeType.MapPlayerCount => stats.playerUpgradeMapPlayerCount,
        UpgradeType.SprintSpeed => stats.playerUpgradeSpeed,
        UpgradeType.GrabStrength => stats.playerUpgradeStrength,
        UpgradeType.GrabRange => stats.playerUpgradeRange,
        UpgradeType.ThrowStrength => stats.playerUpgradeThrow,
        UpgradeType.TumbleWings => stats.playerUpgradeTumbleWings,
        UpgradeType.CrouchRest => stats.playerUpgradeCrouchRest,
        UpgradeType.Modded => stats.dictionaryOfDictionaries[id.RawName],
        _ => throw new ArgumentException($"Invalid UpgradeType for {nameof(GetUpgrades)}")
    };

    /// <summary>
    /// Get the RPC function name for a given UpgradeType.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    // public static string GetRPCFunctionName(UpgradeType key) => key switch
    // {
    //     UpgradeType.Health => "UpgradePlayerHealthRPC",
    //     UpgradeType.Stamina => "UpgradePlayerEnergyRPC",
    //     UpgradeType.ExtraJump => "UpgradePlayerExtraJumpRPC",
    //     UpgradeType.TumbleLaunch => "UpgradePlayerTumbleLaunchRPC",
    //     UpgradeType.MapPlayerCount => "UpgradeMapPlayerCountRPC",
    //     UpgradeType.SprintSpeed => "UpgradePlayerSprintSpeedRPC",
    //     UpgradeType.GrabStrength => "UpgradePlayerGrabStrengthRPC",
    //     UpgradeType.GrabRange => "UpgradePlayerGrabRangeRPC",
    //     UpgradeType.ThrowStrength => "UpgradePlayerThrowStrengthRPC",
    //     UpgradeType.TumbleWings => "UpgradePlayerTumbleWingsRPC",
    //     UpgradeType.CrouchRest => "UpgradePlayerCrouchRestRPC",
    //     _ or UpgradeType.Modded => throw new ArgumentException($"Invalid UpgradeType for {nameof(GetRPCFunctionName)}")
    // };

    /// <summary>
    /// Call the RPC for a specific player.
    /// </summary>
    /// <param name="bundle"></param>
    /// <param name="workingPlayer"></param>
    /// <param name="key"></param>
    // public static void CallRPCOnePlayer(SyncBundle bundle, PlayerAvatar workingPlayer, UpgradeId key)
    //     => CallRPCOnePlayer(bundle, workingPlayer.SteamId(), key, workingPlayer.photonView.Owner);

    /// <summary>
    /// Call the RPC for a specific player.
    /// </summary>
    /// <param name="bundle"></param>
    /// <param name="steamId"></param>
    /// <param name="key"></param>
    /// <param name="player"></param>
    // public static void CallRPCOnePlayer(SyncBundle bundle, string steamId, UpgradeId key, Player player)
    //     => bundle.View.LogRPC(GetRPCFunctionName(key.Type), player, steamId, ++GetUpgrades(bundle.Stats, key)[steamId]);

    /// <summary>
    /// Call the RPC for <see cref="Others"/>
    /// </summary>
    /// <param name="bundle"></param>
    /// <param name="steamId"></param>
    /// <param name="key"></param>
    // public static void CallRPC(SyncBundle bundle, string steamId, UpgradeId key)
    //     => bundle.View.LogRPC(GetRPCFunctionName(key.Type), Others, steamId, ++GetUpgrades(bundle.Stats, key)[steamId]);

    /// <summary>
    /// Synchronize the stats dictionary to all players.
    /// </summary>
    /// <param name="bundle"></param>
    public static void SyncStatsDictionaryToAll(SyncBundle bundle)
        => SyncQueue.Enqueue(SyncRequest.New(bundle));

    /// <summary>
    /// Call the update function for a specific upgrade type.
    /// </summary>
    /// <param name="bundle"></param>
    /// <param name="steamId"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static int CallUpdateFunction(SyncBundle bundle, string steamId, UpgradeId key) => key.Type switch
    {
        UpgradeType.Health => bundle.Manager.UpgradePlayerHealth(steamId),
        UpgradeType.Stamina => bundle.Manager.UpgradePlayerEnergy(steamId),
        UpgradeType.ExtraJump => bundle.Manager.UpgradePlayerExtraJump(steamId),
        UpgradeType.TumbleLaunch => bundle.Manager.UpgradePlayerTumbleLaunch(steamId),
        UpgradeType.MapPlayerCount => bundle.Manager.UpgradeMapPlayerCount(steamId),
        UpgradeType.SprintSpeed => bundle.Manager.UpgradePlayerSprintSpeed(steamId),
        UpgradeType.GrabStrength => bundle.Manager.UpgradePlayerGrabStrength(steamId),
        UpgradeType.GrabRange => bundle.Manager.UpgradePlayerGrabRange(steamId),
        UpgradeType.ThrowStrength => bundle.Manager.UpgradePlayerThrowStrength(steamId),
        UpgradeType.TumbleWings => bundle.Manager.UpgradePlayerTumbleWings(steamId),
        UpgradeType.CrouchRest => bundle.Manager.UpgradePlayerCrouchRest(steamId),
        UpgradeType.Modded => UpgradeModded(bundle, SemiFunc.PlayerAvatarGetFromSteamID(steamId), key, 1),
        _ => throw new ArgumentException($"Invalid UpgradeType for {nameof(CallUpdateFunction)}")
    };
    
    /// <summary>
    /// Upgrade a modded upgrade for a specific player.
    /// </summary>
    /// <param name="bundle"></param>
    /// <param name="workingPlayer"></param>
    /// <param name="key"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public static int UpgradeModded(SyncBundle bundle, PlayerAvatar workingPlayer, UpgradeId key, int amount)
    {
        string steamId = workingPlayer.SteamId();
        int newAmt = AddToStatsDictionary(bundle, steamId, key, amount);

        if (Upgrades.TryGetUpgrade(TrimKey(key.RawName), out PlayerUpgrade? upgrade))
        {
            #if DEBUG
            Entry.LogSource.LogInfo($"[NETWORKING] [{nameof(UpgradeModded)}] [{bundle}] {upgrade.UpgradeId} {steamId} {newAmt}");
            #endif
            upgrade.SetLevel(workingPlayer, newAmt);
        }
        else
        {
            #if DEBUG
            Entry.LogSource.LogError($"[{nameof(UpgradeModded)}] [{bundle}] Upgrade not found in {nameof(REPOLib)}: {key.RawName}");
            #endif
        }
        
        return newAmt;
    }
    
    /// <summary>
    /// Add <see cref="incrementAmount"/> of levels to the stats dictionary for a specific player.
    /// </summary>
    /// <param name="bundle"></param>
    /// <param name="steamId"></param>
    /// <param name="key"></param>
    /// <param name="incrementAmount"></param>
    /// <returns></returns>
    public static int AddToStatsDictionary(SyncBundle bundle, string steamId, UpgradeId key, int incrementAmount)
    {
        #if DEBUG
        Entry.LogSource.LogInfo($"[{nameof(AddToStatsDictionary)}] [{bundle}] {key} {steamId} {incrementAmount}");
        #endif
        return bundle.Stats.dictionaryOfDictionaries[key.RawName][steamId] += incrementAmount;
    }
    
    /// <summary>
    /// Queue an RPC call to be executed later.
    /// </summary>
    /// <param name="view"></param>
    /// <param name="methodName"></param>
    /// <param name="target"></param>
    /// <param name="parameters"></param>
    internal static void QueueRPC(PhotonView view, string methodName, object target, object[] parameters)
        => SyncQueue.Enqueue(PunRequest.New(view, methodName, target, parameters));

    /// <summary>
    /// Execute one RPC call from the queue.
    /// </summary>
    internal static void RunOneRPC()
    {
        if (SyncQueue.TryDequeue(out ISyncRequest request))
            request.Run();
    }
}