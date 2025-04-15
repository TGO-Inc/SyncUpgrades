# Sync Upgrades
- Host only!
- Supports modded upgrades!
- If any player consumes an upgrade, it will be synced to all other players.
- If a player joins late, they will receive all the host upgrades.

### What makes this different?
- You may be familiar with [SharedUpgrades](https://thunderstore.io/c/repo/p/Traktool/SharedUpgrades/)
or [SyncHostUpgrades](https://thunderstore.io/c/repo/p/SharkLucas/SyncHostUpgrades/)

- This mod is a combination of both, and it is **not** buggy, **not** laggy, and **not** inefficient.

- The code behind this mod only runs when a player consumes an upgrade, a player joins the game, or a level is loaded.

- This is different behavior than the other mods, which run every frame to check for upgrades.
  - The before mentioned problem causes lag, fps drops, stutters, and other bugs.

- It was programmed and designed properly.

### What's the technical workflow?
1. When a player consumes an upgrade, the server receives the upgrade event.
2. If the player is the host, the server checks all clients and ensures they are on the same level as the host.
   - If they are not on the same level, the server sends the upgrade event for that player to all clients.
3. If the player is not the host, then the server upgrades the host first, which triggers the upgrade event once again (2).

## Modded Upgrades

### Any problems?
- Due to the non-standardized nature of consumable upgrades, there is no way to notify the appropriate handler when 
a player consumes a ***modded*** upgrade. When a player loads into the game, `LateStart` is called which fetches the
upgrade values and updates the underlying properties. This means, that once a player has loaded into the game, only 
NON-modded consumable upgrades will change the underlying properties and henceforth be applied to your player.

### What does all the mumbo jumbo mean?
- When a player consumes an upgrade, it changes the appropriate underlying properties of the player.
  - Here are some examples of the underlying properties: `PlayerAvatar.upgradeMapPlayerCount`, 
  `PhysGrabber.grabStrength`, `PhysGrabber.throwStrength`, `PhysGrabber.grabRange`

- Inside the `PunManager` there are functions for `UpdateThrowStrength`, `UpdateGrabStrength`, `UpdateGrabRange`, etc.

- When a player consumes an upgrade, it is possible to call `PunManager.UpdateThrowStrength` or similar, and these functions
will update the underlying properties of the player, in this case `PhysGrabber.throwStrength = newValue`.
- There are no such functions to call for modded upgrades
- The only way to notify the modded upgrade is if the modded upgrade required this mod `SyncUpgrades` as a dependency.
- The only time the underlying properties are updated are as follows
  - When a player consumes an upgrade (it makes an RPC call)
  - When a player is loaded into the level (`LateStart` is called which loads the underlying properties)
- There are no built-in RPC calls for modded upgrades

### How does this affect me?
- Modded upgrades will not update during the current session if they are consumed after the level has loaded and 
  players have spawned in.
- Modded upgrades do not send event calls when a player consumes a modded upgrade.

### What to do about it?
- Consume all modded upgrades during the `"Truck Lobby"` phase.

### HELP, when a non-host player consumes an upgrade, it doesn't sync to the host or anyone else!
- This is a problem with the mod not sending the upgrade event to the server.
- Developers please examine [Patches/PunManagerPatch.cs](Patches/PunManagerPatch.cs#L103-L148)