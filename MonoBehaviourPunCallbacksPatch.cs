using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;

namespace SyncUpgrades;

[HarmonyPatch(typeof(MonoBehaviourPunCallbacks))]
internal class MonoBehaviourPunCallbacksPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("OnPlayerEnteredRoom")]
    private static void OnPlayerEnteredRoom(Player newPlayer)
    {
        // If not host OR single-player, return
        if (SemiFunc.IsNotMasterClient())
            return;
        
        // Sync the upgrades
        Entry.Instance?.SyncUpgrades(newPlayer);
    }
}