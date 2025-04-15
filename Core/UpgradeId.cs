namespace SyncUpgrades.Core;

public class UpgradeId(string rawName)
{
    public UpgradeId(UpgradeType upgradeType) 
        : this(SemiUtil.GetUpgradeName(upgradeType))
    {
        Type = upgradeType;
    }

    public UpgradeType Type { get; } = SemiUtil.GetUpgradeType(rawName);
    public string RawName { get; } = rawName;
}