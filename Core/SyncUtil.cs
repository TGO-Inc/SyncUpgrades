using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System;
using REPOLib.Modules;

namespace SyncUpgrades.Core;

[PublicAPI]
public static class SyncUtil
{
    private const RpcTarget Others = RpcTarget.Others;
    public static string HostSteamId => Local.SteamId();
    public static PlayerAvatar Local => SemiFunc.PlayerAvatarLocal();

    public static string TrimKey(string? key)
    {
        if (string.IsNullOrEmpty(key)) return string.Empty;
        if (key.StartsWith("appliedPlayerUpgrade")) return key[20..];
        return key.StartsWith("playerUpgrade") ? key[13..] : key;
    }
    
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

    public static void CallRPCOnePlayer(PunBundle bundle, PlayerAvatar workingPlayer, UpgradeId key)
        => CallRPCOnePlayer(bundle, workingPlayer.SteamId(), key, workingPlayer.photonView.Owner);
    public static void CallRPCOnePlayer(PunBundle bundle, string steamId, UpgradeId key, Player player)
        => bundle.View.LogRPC(GetRPCFunctionName(key.Type), player, steamId, ++GetUpgrades(bundle.Stats, key)[steamId]);
    public static void SyncStatsDictionaryToAll(PunBundle bundle)
    {
        #if DEBUG
        Entry.LogSource.LogInfo($"[{nameof(SyncStatsDictionaryToAll)}] [{bundle}]");
        #endif
        bundle.Manager.SyncAllDictionaries();
    }
    public static void CallRPC(PunBundle bundle, string steamId, UpgradeId key)
        => bundle.View.LogRPC(GetRPCFunctionName(key.Type), Others, steamId, ++GetUpgrades(bundle.Stats, key)[steamId]);
    
    public static int CallUpdateFunction(PunManager instance, string steamId, UpgradeType key) => key switch
    {
        UpgradeType.Health => instance.UpgradePlayerHealth(steamId),
        UpgradeType.Stamina => instance.UpgradePlayerEnergy(steamId),
        UpgradeType.ExtraJump => instance.UpgradePlayerExtraJump(steamId),
        UpgradeType.TumbleLaunch => instance.UpgradePlayerTumbleLaunch(steamId),
        UpgradeType.MapPlayerCount => instance.UpgradeMapPlayerCount(steamId),
        UpgradeType.SprintSpeed => instance.UpgradePlayerSprintSpeed(steamId),
        UpgradeType.GrabStrength => instance.UpgradePlayerGrabStrength(steamId),
        UpgradeType.GrabRange => instance.UpgradePlayerGrabRange(steamId),
        UpgradeType.GrabThrow => instance.UpgradePlayerThrowStrength(steamId),
        _ or UpgradeType.Modded => throw new ArgumentException($"Invalid UpgradeType for {nameof(CallUpdateFunction)}")
    };
    
    public static void UpgradeModded(PunBundle bundle, PlayerAvatar workingPlayer, UpgradeId key, int amount)
    {
        string steamId = workingPlayer.SteamId();
        int newAmt = IncrementUpdateDict(bundle, steamId, key, amount);

        if (Upgrades.TryGetUpgrade(TrimKey(key.RawName), out PlayerUpgrade? upgrade))
        {
            #if DEBUG
            Entry.LogSource.LogInfo($"[{nameof(UpgradeModded)}] [{bundle}] {upgrade} {steamId} {newAmt}");
            #endif
            upgrade.SetLevel(workingPlayer, newAmt);
        }
        else
        {
            #if DEBUG
            Entry.LogSource.LogError($"[{nameof(UpgradeModded)}] [{bundle}] Upgrade not found in {nameof(REPOLib)}: {key.RawName}");
            #endif
        }
    }

    public static void IncrementUpdateDictAndSync(PunBundle bundle, string steamId, UpgradeId key, int amount)
    {
        IncrementUpdateDict(bundle, steamId, key, amount);
        SyncStatsDictionaryToAll(bundle);
    }
    
    public static int IncrementUpdateDict(PunBundle bundle, string steamId, UpgradeId key, int amount)
    {
        #if DEBUG
        Entry.LogSource.LogInfo($"[{nameof(IncrementUpdateDict)}] [{bundle}] {key} {steamId} {amount}");
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
}