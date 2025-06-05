using JetBrains.Annotations;

namespace SyncUpgrades.Core;

/// <summary>
/// Vanilla and modded upgrade types.
/// </summary>
[PublicAPI]
public enum UpgradeType
{
    Modded,
    Health,
    Stamina,
    ExtraJump,
    TumbleLaunch,
    MapPlayerCount,
    SprintSpeed,
    GrabStrength,
    GrabRange,
    ThrowStrength,
    TumbleWings,
    CrouchRest,
}