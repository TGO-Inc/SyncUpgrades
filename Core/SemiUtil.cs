using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System;

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
        _ or UpgradeType.Modded => throw new ArgumentException()
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
        _ => throw new ArgumentException()
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
        _ or UpgradeType.Modded => throw new ArgumentException()
    };

    public static void CallRPCOnePlayer(PunBundle bundle, PlayerAvatar workingPlayer, UpgradeId key)
        => CallRPCOnePlayer(bundle, workingPlayer.SteamId(), key, workingPlayer.photonView.Owner);
    public static void CallRPCOnePlayer(PunBundle bundle, string steamId, UpgradeId key, Player player)
        => bundle.View.RPC(GetRPCFunctionName(key.Type), player, steamId, ++GetUpgrades(bundle.Stats, key)[steamId]);
    public static void SyncStatsDictionaryToAll(PunBundle bundle) => bundle.Manager.SyncAllDictionaries();
    public static void CallRPC(PunBundle bundle, string steamId, UpgradeId key)
        => bundle.View.RPC(GetRPCFunctionName(key.Type), Others, steamId, ++GetUpgrades(bundle.Stats, key)[steamId]);
    
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
        _ or UpgradeType.Modded => throw new ArgumentException()
    };
    
    public static void UpgradeModded(PunBundle bundle, PlayerAvatar workingPlayer, UpgradeId key, int amount)
    {
        string steamId = workingPlayer.SteamId();
        int newAmt = bundle.Stats.dictionaryOfDictionaries[key.RawName][steamId] += amount;
        
        if (REPOLib.Modules.Upgrades.TryGetUpgrade(TrimKey(key.RawName), out REPOLib.Modules.PlayerUpgrade? upgrade))
            upgrade.SetLevel(workingPlayer, newAmt);
        else
            IncrementUpdateDict(bundle, steamId, key, amount);
    }

    public static void IncrementUpdateDictAndSync(PunBundle bundle, string steamId, UpgradeId key, int amount)
    {
        bundle.Stats.dictionaryOfDictionaries[key.RawName][steamId] += amount;
        bundle.Manager.SyncAllDictionaries();
    }
    
    public static void IncrementUpdateDict(PunBundle bundle, string steamId, UpgradeId key, int amount)
        => bundle.Stats.dictionaryOfDictionaries[key.RawName][steamId] += amount;
    
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