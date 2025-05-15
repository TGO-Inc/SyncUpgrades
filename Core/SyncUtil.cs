using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using REPOLib.Modules;

namespace SyncUpgrades.Core;

[PublicAPI]
public static class SyncUtil
{
    private const string PlayerUpgrade = "playerUpgrade";
    private const string AppliedPlayerUpgrade = "appliedPlayerUpgrade";
    private const RpcTarget Others = RpcTarget.Others;
    public static string HostSteamId => Local.SteamId();
    public static PlayerAvatar Local => SemiFunc.PlayerAvatarLocal();

    public static string TrimKey(string? key)
    {
        if (string.IsNullOrEmpty(key)) return string.Empty;
        if (key.StartsWith(AppliedPlayerUpgrade)) return key[20..];
        return key.StartsWith(PlayerUpgrade) ? key[13..] : key;
    }

    public static string FixKey(string? key)
    {
        if (string.IsNullOrEmpty(key)) return string.Empty;
        if (!key.StartsWith(PlayerUpgrade)) return PlayerUpgrade+key;
        return key;
    }
    
    public static PlayerAvatar GetPlayer(string targetSteamId) => SemiFunc.PlayerAvatarGetFromSteamID(targetSteamId);
    
    public static IEnumerable<UpgradeId> GetUpgradeTypes(SyncBundle bundle) 
        => bundle.Stats.dictionaryOfDictionaries.Where(kvp => kvp.Key.StartsWith(PlayerUpgrade) || kvp.Key.StartsWith(AppliedPlayerUpgrade)).Select(UpgradeId.New);

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
        "Throw" => UpgradeType.GrabThrow,
        // Assume
        _ => UpgradeType.Modded
    };

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
        UpgradeType.GrabThrow => "Throw",
        UpgradeType.Modded => "Modded: Unknown",
        _ => throw new ArgumentException($"Invalid UpgradeType for {nameof(GetUpgradeName)}")
    };

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
        UpgradeType.GrabThrow => stats.playerUpgradeThrow,
        UpgradeType.Modded => stats.dictionaryOfDictionaries[id.RawName],
        _ => throw new ArgumentException($"Invalid UpgradeType for {nameof(GetUpgrades)}")
    };

    public static string GetRPCFunctionName(UpgradeType key) => key switch
    {
        UpgradeType.Health => "UpgradePlayerHealthRPC",
        UpgradeType.Stamina => "UpgradePlayerEnergyRPC",
        UpgradeType.ExtraJump => "UpgradePlayerExtraJumpRPC",
        UpgradeType.TumbleLaunch => "UpgradePlayerTumbleLaunchRPC",
        UpgradeType.MapPlayerCount => "UpgradeMapPlayerCountRPC",
        UpgradeType.SprintSpeed => "UpgradePlayerSprintSpeedRPC",
        UpgradeType.GrabStrength => "UpgradePlayerGrabStrengthRPC",
        UpgradeType.GrabRange => "UpgradePlayerGrabRangeRPC",
        UpgradeType.GrabThrow => "UpgradePlayerThrowStrengthRPC",
        _ or UpgradeType.Modded => throw new ArgumentException($"Invalid UpgradeType for {nameof(GetRPCFunctionName)}")
    };

    public static void CallRPCOnePlayer(SyncBundle bundle, PlayerAvatar workingPlayer, UpgradeId key)
        => CallRPCOnePlayer(bundle, workingPlayer.SteamId(), key, workingPlayer.photonView.Owner);

    public static void CallRPCOnePlayer(SyncBundle bundle, string steamId, UpgradeId key, Player player)
        => bundle.View.LogRPC(GetRPCFunctionName(key.Type), player, steamId, ++GetUpgrades(bundle.Stats, key)[steamId]);

    public static void CallRPC(SyncBundle bundle, string steamId, UpgradeId key)
        => bundle.View.LogRPC(GetRPCFunctionName(key.Type), Others, steamId, ++GetUpgrades(bundle.Stats, key)[steamId]);

    public static void SyncStatsDictionaryToAll(SyncBundle bundle)
        => SyncQueue.Enqueue(SyncRequest.New(bundle));

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
        UpgradeType.GrabThrow => bundle.Manager.UpgradePlayerThrowStrength(steamId),
        UpgradeType.Modded => UpgradeModded(bundle, SemiFunc.PlayerAvatarGetFromSteamID(steamId), key, 1),
        _ => throw new ArgumentException($"Invalid UpgradeType for {nameof(CallUpdateFunction)}")
    };
    
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
    
    public static void AddToStatsDictionaryAndSync(SyncBundle bundle, string steamId, UpgradeId key, int amount)
    {
        AddToStatsDictionary(bundle, steamId, key, amount);
        SyncStatsDictionaryToAll(bundle);
    }
    
    public static int AddToStatsDictionary(SyncBundle bundle, string steamId, UpgradeId key, int amount)
    {
        #if DEBUG
        Entry.LogSource.LogInfo($"[{nameof(AddToStatsDictionary)}] [{bundle}] {key} {steamId} {amount}");
        #endif
        return bundle.Stats.dictionaryOfDictionaries[key.RawName][steamId] += amount;
    }

    public static readonly UpgradeId HealthId = new(UpgradeType.Health);
    public static readonly UpgradeId StaminaId = new(UpgradeType.Stamina);
    public static readonly UpgradeId ExtraJumpId = new(UpgradeType.ExtraJump);
    public static readonly UpgradeId TumbleLaunchId = new(UpgradeType.TumbleLaunch);
    public static readonly UpgradeId MapPlayerCountId = new(UpgradeType.MapPlayerCount);
    public static readonly UpgradeId SprintSpeedId = new(UpgradeType.SprintSpeed);
    public static readonly UpgradeId GrabStrengthId = new(UpgradeType.GrabStrength);
    public static readonly UpgradeId GrabRangeId = new(UpgradeType.GrabRange);
    public static readonly UpgradeId GrabThrowId = new(UpgradeType.GrabThrow);

    private static readonly ConcurrentQueue<ISyncRequest> SyncQueue = [];
    public static void QueueRPC(PhotonView view, string methodName, object target, object[] parameters)
        => SyncQueue.Enqueue(PunRequest.New(view, methodName, target, parameters));

    public static void RunOneRPC()
    {
        if (SyncQueue.TryDequeue(out ISyncRequest request))
            request.Run();
    }
}