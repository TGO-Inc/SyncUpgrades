using Photon.Pun;
using Photon.Realtime;

namespace SyncUpgrades.Core.Internal;

internal static class Extensions
{
    public static string SteamId(this PlayerAvatar avatar) 
        => SemiFunc.PlayerGetSteamID(avatar);
    
    public static PhotonView GetView(this PunManager instance) 
        => instance.GetComponent<PhotonView>();
    
    public static string ToName(this UpgradeType key) 
        => SyncUtil.GetUpgradeName(key);
}