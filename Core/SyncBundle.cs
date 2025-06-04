using JetBrains.Annotations;
using Photon.Pun;

namespace SyncUpgrades.Core;

/// <summary>
/// <see cref="SyncBundle"/> is a simple container for various components needed by different <see cref="SyncManager"/> methods.
/// </summary>
/// <param name="Manager"></param>
/// <param name="View"></param>
/// <param name="Stats"></param>
/// <param name="SteamId"></param>
[PublicAPI]
public record SyncBundle(PunManagerWrapper Manager, PhotonView View, StatsManager Stats, string SteamId)
{
    private SyncBundle(PunManagerWrapper mgr, StatsManager sts, string sId) : this (mgr, mgr.GetView(), sts, sId) { }
    public override string ToString() => this.SteamId;
    public static SyncBundle Default(string steamId) => new(PunManagerWrapper.instance, StatsManager.instance, steamId);
}