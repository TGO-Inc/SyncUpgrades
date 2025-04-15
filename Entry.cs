using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using SyncUpgrades.Core;
using UnityEngine;

namespace SyncUpgrades;

[BepInPlugin(PluginId, PluginName, PluginVersion)]
public class Entry : BaseUnityPlugin
{
    private const string PluginName = "Sync Upgrades";
    private const string PluginVersion = "1.5.6";
    private const string PluginId = "TGO.SyncUpgrades";

    private static readonly Harmony Harmony = new(PluginId);
    internal static readonly ManualLogSource LogSource = BepInEx.Logging.Logger.CreateLogSource(PluginName);
    internal static ConfigFile BepConfig => _instance!.Config;
    private static Entry? _instance;
    
    private void Awake()
    {
        // Initialize the plugin
        _instance = this;
        
        // Initialize the SyncManager
        SyncManager.Init();
        
        // Apply Harmony patches
        Harmony.PatchAll();
        
        // Persist the game object
        gameObject.hideFlags = HideFlags.DontSaveInEditor;
        
        // Log startup
        LogSource.LogInfo("Sync Upgrades loaded!");
    }
}
