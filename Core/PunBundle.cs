using Photon.Pun;

namespace SyncUpgrades.Core;

public class PunBundle(PunManager mgr, PhotonView pv, StatsManager sts, string sId)
{
    public PunBundle(PunManager mgr, StatsManager sts, string sId) : this (mgr, mgr.GetView(), sts, sId) { }
    public PunManager Manager { get; } = mgr;
    public PhotonView View { get; } = pv;
    public StatsManager Stats { get; } = sts;
    public string SteamId { get; } = sId;
    public override string ToString() => this.SteamId;
}