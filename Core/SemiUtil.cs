using System;
using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using Photon.Pun;

namespace SyncUpgrades.Core;

[PublicAPI]
public static class SemiUtil
{
    public static readonly AccessTools.FieldRef<PlayerAvatar, string> GetSteamID 
        = AccessTools.FieldRefAccess<PlayerAvatar, string>("steamID");
    
    private const RpcTarget Others = RpcTarget.Others;
    public static string HostSteamId => GetSteamID(Local);
    public static PlayerAvatar Local => SemiFunc.PlayerAvatarLocal();

    public static string TrimKey(string? key)
    {
        if (string.IsNullOrEmpty(key))
            return string.Empty;
        if (key.StartsWith("appliedPlayerUpgrade"))
            return key[20..];
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

    public static void IncrementUpdateDictAndSync(PunBundle bundle, string steamId, UpgradeId key, int amount)
    {
        bundle.Stats.dictionaryOfDictionaries[key.RawName][steamId] += amount;
        bundle.Manager.SyncAllDictionaries();
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