using Photon.Pun;

namespace SyncUpgrades.Core;

public record SyncBundle(PunManager Manager, PhotonView View, StatsManager Stats, string SteamId)
{
    public static SyncBundle Bundle(string steamId) => new(PunManager.instance, StatsManager.instance, steamId);
    public SyncBundle(PunManager mgr, StatsManager sts, string sId) : this (mgr, mgr.GetView(), sts, sId) { }
    public override string ToString() => this.SteamId;
    public static SyncBundle Default(string steamId) => new(PunManager.instance, StatsManager.instance, steamId);
}