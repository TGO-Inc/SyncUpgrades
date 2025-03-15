using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using Photon.Pun;
using System;

namespace SyncHostUpgrades
{
    [BepInPlugin("SharkLucas.REPO.SyncHostUpgrades", "R.E.P.O. Sync Host Upgrades", "1.0.0")]
    public class SyncHostUpgrades : BaseUnityPlugin
    {
        // 单例实例
        public static SyncHostUpgrades? Instance { get; private set; }
        // 配置参数
        public ConfigEntry<bool>? SyncHealth;
        public ConfigEntry<bool>? SyncStamina;
        public ConfigEntry<bool>? SyncExtraJump;
        public ConfigEntry<bool>? SyncMapPlayerCount;
        public ConfigEntry<bool>? SyncGrabRange;
        public ConfigEntry<bool>? SyncGrabStrength;
        public ConfigEntry<bool>? SyncGrabThrow;
        public ConfigEntry<bool>? SyncSprintSpeed;
        public ConfigEntry<bool>? SyncTumbleLaunch;

        private static readonly AccessTools.FieldRef<PlayerAvatar, bool> isLocalRef = AccessTools.FieldRefAccess<PlayerAvatar, bool>("isLocal");
        private static readonly AccessTools.FieldRef<PlayerAvatar, string> playerNameRef = AccessTools.FieldRefAccess<PlayerAvatar, string>("playerName");
        private static readonly AccessTools.FieldRef<PlayerAvatar, string> steamIDRef = AccessTools.FieldRefAccess<PlayerAvatar, string>("steamID");

        // 用于缓存本地玩家的静态变量
        private static PlayerAvatar? cachedLocalPlayer = null;
        private static float lastCheckTime = 0f;

        /*
        [Info   :R.E.P.O. Sync Host Upgrades] Health,0
        [Info   :R.E.P.O. Sync Host Upgrades] Stamina,0
        [Info   :R.E.P.O. Sync Host Upgrades] Extra Jump,0
        [Info   :R.E.P.O. Sync Host Upgrades] Launch,0
        [Info   :R.E.P.O. Sync Host Upgrades] Map Player Count,0
        [Info   :R.E.P.O. Sync Host Upgrades] Speed,0
        [Info   :R.E.P.O. Sync Host Upgrades] Strength,0
        [Info   :R.E.P.O. Sync Host Upgrades] Range,0
        [Info   :R.E.P.O. Sync Host Upgrades] Throw,0
        */

        private void Awake()
        {
            Instance = this;

            // 初始化配置
            SyncHealth = Config.Bind("Sync", "Health", true, "Sync Max Health");
            SyncStamina = Config.Bind("Sync", "Stamina", true, "Sync Max Stamina");
            SyncExtraJump = Config.Bind("Sync", "Extra Jump", true, "Sync Extra Jump Count");
            SyncTumbleLaunch = Config.Bind("Sync", "Tumble Launch", true, "Sync Tumble Launch Count");
            SyncMapPlayerCount = Config.Bind("Sync", "Map Player Count", true, "Sync Map Player Count");
            SyncSprintSpeed = Config.Bind("Sync", "Sprint Speed", false, "Sync Sprint Speed");
            SyncGrabStrength = Config.Bind("Sync", "Grab Strength", true, "Sync Grab Strength");
            SyncGrabRange = Config.Bind("Sync", "Grab Range", true, "Sync Grab Range");
            SyncGrabThrow = Config.Bind("Sync", "Grab Throw", true, "Sync Grab Throw");

            // 应用Harmony补丁
            Harmony harmony = new Harmony("REPO.SyncHostUpgrades");
            harmony.PatchAll();

            // 使用不同的日志级别尝试
            Logger.LogInfo("SyncHostUpgrades loaded!");

            gameObject.hideFlags = HideFlags.DontSaveInEditor;
        }

        private void Update()
        {
            // 只在主机执行
            if (!PhotonNetwork.IsMasterClient)
            {
                return; // 非主机直接返回
            }

            // 获取本地玩家
            PlayerAvatar? localPlayer = GetLocalPlayer();
            if (localPlayer != null)
            {
                Dictionary<string, int> hostUpgrades = StatsManager.instance.FetchPlayerUpgrades(steamIDRef(localPlayer));

                List<PlayerAvatar> playerList = SemiFunc.PlayerGetAll();
                foreach (PlayerAvatar player in playerList)
                {
                    if (isLocalRef(player))
                    {
                        continue;
                    }

                    string steamId = steamIDRef(player);
                    Dictionary<string, int> upgrades = StatsManager.instance.FetchPlayerUpgrades(steamId);

                    foreach (string key in hostUpgrades.Keys)
                    {
                        // 检查配置是否启用了该升级类型的同步
                        bool shouldSync = false;

                        // 根据不同的升级类型检查配置
                        switch (key)
                        {
                            case "Health":
                                shouldSync = SyncHealth?.Value ?? false;
                                break;
                            case "Stamina":
                                shouldSync = SyncStamina?.Value ?? false;
                                break;
                            case "Extra Jump":
                                shouldSync = SyncExtraJump?.Value ?? false;
                                break;
                            case "Launch":
                                shouldSync = SyncTumbleLaunch?.Value ?? false;
                                break;
                            case "Map Player Count":
                                shouldSync = SyncMapPlayerCount?.Value ?? false;
                                break;
                            case "Speed":
                                shouldSync = SyncSprintSpeed?.Value ?? false;
                                break;
                            case "Strength":
                                shouldSync = SyncGrabStrength?.Value ?? false;
                                break;
                            case "Range":
                                shouldSync = SyncGrabRange?.Value ?? false;
                                break;
                            case "Throw":
                                shouldSync = SyncGrabThrow?.Value ?? false;
                                break;
                        }

                        // 如果启用了同步，且主机的升级等级高于玩家
                        if (shouldSync && hostUpgrades.TryGetValue(key, out int hostLevel) && hostLevel > upgrades[key])
                        {
                            // 计算差异
                            int diff = hostLevel - upgrades[key];

                            // 根据升级类型调用相应的升级方法
                            for (int i = 0; i < diff; i++)
                            {
                                switch (key)
                                {
                                    case "Health":
                                        if (PhotonNetwork.IsMasterClient)
                                            PunManager.instance.UpgradePlayerHealth(steamId);
                                        break;
                                    case "Stamina":
                                        if (PhotonNetwork.IsMasterClient)
                                            PunManager.instance.UpgradePlayerEnergy(steamId);
                                        break;
                                    case "Extra Jump":
                                        if (PhotonNetwork.IsMasterClient)
                                            PunManager.instance.UpgradePlayerExtraJump(steamId);
                                        break;
                                    case "Launch":
                                        if (PhotonNetwork.IsMasterClient)
                                            PunManager.instance.UpgradePlayerTumbleLaunch(steamId);
                                        break;
                                    case "Map Player Count":
                                        if (PhotonNetwork.IsMasterClient)
                                            PunManager.instance.UpgradeMapPlayerCount(steamId);
                                        break;
                                    case "Speed":
                                        if (PhotonNetwork.IsMasterClient)
                                            PunManager.instance.UpgradePlayerSprintSpeed(steamId);
                                        break;
                                    case "Strength":
                                        if (PhotonNetwork.IsMasterClient)
                                            PunManager.instance.UpgradePlayerGrabStrength(steamId);
                                        break;
                                    case "Range":
                                        if (PhotonNetwork.IsMasterClient)
                                            PunManager.instance.UpgradePlayerGrabRange(steamId);
                                        break;
                                    case "Throw":
                                        if (PhotonNetwork.IsMasterClient)
                                            PunManager.instance.UpgradePlayerThrowStrength(steamId);
                                        break;
                                }
                            }

                            Logger.LogInfo($"为玩家 {playerNameRef(player)} 同步升级: {key}, 从 {upgrades[key]} 到 {hostLevel}");
                        }
                    }
                }
            }
        }

        public static PlayerAvatar? GetLocalPlayer()
        {
            // 每秒最多检查一次，减少性能开销
            if (cachedLocalPlayer != null && Time.time - lastCheckTime < 1f)
            {
                return cachedLocalPlayer;
            }

            lastCheckTime = Time.time;

            List<PlayerAvatar> playerList = SemiFunc.PlayerGetAll();
            foreach (PlayerAvatar player in playerList)
            {
                if (isLocalRef(player))
                {
                    cachedLocalPlayer = player;
                    return player;
                }
            }
            return cachedLocalPlayer;
        }
    }
}
