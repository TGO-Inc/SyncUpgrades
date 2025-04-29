using Photon.Pun;

namespace SyncUpgrades.Core;

public static class Extensions
{
    public static string SteamId(this PlayerAvatar avatar) => SemiFunc.PlayerGetSteamID(avatar);
    public static PhotonView GetView(this PunManager instance) => instance.GetComponent<PhotonView>();
}