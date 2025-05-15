using Photon.Pun;
using Photon.Realtime;

namespace SyncUpgrades.Core;

public static class Extensions
{
    public static string SteamId(this PlayerAvatar avatar) 
        => SemiFunc.PlayerGetSteamID(avatar);
    
    public static PhotonView GetView(this PunManager instance) 
        => instance.GetComponent<PhotonView>();
    
    public static string ToName(this UpgradeType key) 
        => SyncUtil.GetUpgradeName(key);
    
    public static void LogRPC(this PhotonView view, string methodName, Player target, params object[] parameters)
        => SyncUtil.QueueRPC(view, methodName, target, parameters);
    
    public static void LogRPC(this PhotonView view, string methodName, RpcTarget target, params object[] parameters)
        => SyncUtil.QueueRPC(view, methodName, target, parameters);
}