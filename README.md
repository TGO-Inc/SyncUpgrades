# Sync Upgrades
- Only needed on the host!
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