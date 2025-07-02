using Photon.Pun;
using SyncUpgrades.Core.Internal;

namespace SyncUpgrades.Core;

public class PunManagerWrapper(PunManager manager)
{
    // ReSharper disable once InconsistentNaming
    public static PunManagerWrapper instance => new(PunManager.instance);
    
    public PhotonView GetView()
        => manager.GetView();

    public int UpgradePlayerTumbleWings(string steamId)
        => manager.UpgradePlayerTumbleWings(steamId);
    
    public int UpgradePlayerCrouchRest(string steamId)
        => manager.UpgradePlayerCrouchRest(steamId);

    public void SyncAllDictionaries()
        => manager.SyncAllDictionaries();

    public int UpgradePlayerHealth(string steamId)
        => manager.UpgradePlayerHealth(steamId);
    
    public int UpgradePlayerEnergy(string steamId)
        => manager.UpgradePlayerEnergy(steamId);

    public int UpgradePlayerExtraJump(string steamId)
        => manager.UpgradePlayerExtraJump(steamId);
    
    public int UpgradePlayerTumbleLaunch(string steamId)
        => manager.UpgradePlayerTumbleLaunch(steamId);
    
    public int UpgradeMapPlayerCount(string steamId)
        => manager.UpgradeMapPlayerCount(steamId);
    
    public int UpgradePlayerSprintSpeed(string steamId)
        => manager.UpgradePlayerSprintSpeed(steamId);
    
    public int UpgradePlayerGrabStrength(string steamId)
        => manager.UpgradePlayerGrabStrength(steamId);
    
    public int UpgradePlayerGrabRange(string steamId)
        => manager.UpgradePlayerGrabRange(steamId);
    
    public int UpgradePlayerThrowStrength(string steamId)
        => manager.UpgradePlayerThrowStrength(steamId);
}