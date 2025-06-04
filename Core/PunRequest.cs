using Photon.Pun;
using Photon.Realtime;

namespace SyncUpgrades.Core;

internal record PunRequest(PhotonView View, string MethodName, object Target, object[] Parameters) : ISyncRequest
{
    public static PunRequest New(PhotonView view, string methodName, object target, object[] parameters)
        => new(view,methodName,target, parameters);

    public void Run()
    {
        #if DEBUG
        if (!this.View)
            Entry.LogSource.LogError($"[NETWORKING] [{nameof(this.Run)}] PhotonView is null");
        else
            Entry.LogSource.LogInfo($"[NETWORKING] [{nameof(this.Run)}] [{nameof(this.View)}] [{this.MethodName}] To: [{this.Target}], {this.View.ViewID}, {this.View.OwnerActorNr}, {this.View.Owner}");
        #endif

        switch (this.Target)
        {
            case RpcTarget rpcTarget:
                this.View.RPC(this.MethodName, rpcTarget, this.Parameters);
                break;
            case Player player:
                this.View.RPC(this.MethodName, player, this.Parameters);
                break;
        }
    }
}