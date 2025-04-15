using System.Collections.Generic;
using Photon.Pun;

namespace SyncUpgrades.Core;

public class PunBundle(PunManager mgr, PhotonView pv, StatsManager sts, string sId)
{
    public PunManager Manager { get; } = mgr;
    public PhotonView View { get; } = pv;
    public StatsManager Stats { get; } = sts;
    public string SteamId { get; } = sId;
}