namespace SyncUpgrades.Core;

public class UpgradeId(string rawName)
{
    public UpgradeId(UpgradeType upgradeType) 
        : this(SyncUtil.GetUpgradeName(upgradeType))
    {
        Type = upgradeType;
    }

    public UpgradeType Type { get; } = SyncUtil.GetUpgradeType(rawName);
    public string RawName { get; } = rawName;
}