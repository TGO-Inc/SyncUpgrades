using System.Collections.Generic;
using JetBrains.Annotations;
using SyncUpgrades.Core.Internal;

namespace SyncUpgrades.Core;

/// <summary>
/// <see cref="UpgradeId"/> is a simple container for the upgrade name and type.
/// </summary>
/// <param name="RawName"></param>
[PublicAPI]
public record UpgradeId(string RawName)
{
    public UpgradeId(UpgradeType upgradeType) 
        : this(SyncUtil.GetUpgradeName(upgradeType))
        => this.Type = upgradeType;

    public UpgradeType Type { get; } = SyncUtil.GetUpgradeType(RawName);

    #region Util
    public override string ToString() => $"{{ {nameof(this.Type)} = \"{this.Type.ToName()}\", {nameof(this.RawName)} = \"{this.RawName}\" }}";
    public static UpgradeId New(string rawName) => new(rawName);
    public static UpgradeId New<T>(KeyValuePair<string, T> item) => new(item.Key);
    #endregion
}