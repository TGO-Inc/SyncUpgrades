using System.Collections.Generic;

namespace SyncUpgrades.Core;

public class UpgradeId(string rawName)
{
    public UpgradeId(UpgradeType upgradeType) 
        : this(SyncUtil.GetUpgradeName(upgradeType))
        => this.Type = upgradeType;

    public UpgradeType Type { get; } = SyncUtil.GetUpgradeType(rawName);
    public string RawName { get; } = rawName;
    public override string ToString() => $"{{ {nameof(this.Type)} = \"{this.Type.ToName()}\", {nameof(this.RawName)} = \"{this.RawName}\" }}";
    public static UpgradeId New(string rawName) => new(rawName);
    public static UpgradeId New<T>(KeyValuePair<string, T> item) => new(item.Key);
}