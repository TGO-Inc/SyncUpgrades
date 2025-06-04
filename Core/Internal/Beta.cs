using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace SyncUpgrades.Core.Internal;

internal static class Beta
{
    private static readonly Lazy<MethodInfo?> UpgradePlayerTumbleWingsMethodInfo = 
        new(() => AccessTools.Method(typeof(PunManager), nameof(UpgradePlayerTumbleWings), [typeof(string)]));
    
    private static readonly Lazy<MethodInfo?> UpgradePlayerCrouchRestMethodInfo = 
        new(() => AccessTools.Method(typeof(PunManager), nameof(UpgradePlayerCrouchRest), [typeof(string)]));
    
    public static Dictionary<string, int> GetPlayerUpgradeCrouchRest(this StatsManager manager)
        => manager.dictionaryOfDictionaries["playerUpgradeCrouchRest"];
    
    public static Dictionary<string, int> GetPlayerUpgradeTumbleWings(this StatsManager manager)
        => manager.dictionaryOfDictionaries["playerUpgradeTumbleWings"];
    
    public static int UpgradePlayerTumbleWings(PunManager manager, string steamId)
    {
        MethodInfo? function = UpgradePlayerTumbleWingsMethodInfo.Value;
        if (function is not null)
            return (int)function.Invoke(manager, [steamId]);
        
        Entry.LogSource.LogError($"Failed to find {nameof(UpgradePlayerTumbleWings)} method in {nameof(PunManager)}.");
        return 0;

    }
    
    public static int UpgradePlayerCrouchRest(PunManager manager, string steamId)
    {
        MethodInfo? function = UpgradePlayerCrouchRestMethodInfo.Value;
        if (function is not null)
            return (int)function.Invoke(manager, [steamId]);
        
        Entry.LogSource.LogError($"Failed to find {nameof(UpgradePlayerCrouchRest)} method in {nameof(PunManager)}.");
        return 0;
    }
}