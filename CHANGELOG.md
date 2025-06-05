# 2.1.5
- For modded upgrades registered with REPOLib:
  - Added dynamic config options to disable modded upgrade synchronization
  - Fixed a small sync bug with modded upgrades

# 2.1.2
- Added support for TumbleWings and CrouchRest (Beta)
- Ensures backwards compatibility with previous versions of REPO (non Beta)
- Fixed naming and description of config entries

# 2.0.7
- [x] Fixed modded upgrade synchronization (now works 100% for REPOLib registered upgrades)
- [x] [Fixed client disconnect bug when duplicate networking operations are sent (removed duplicate networking)](#2)
- Rewrote SyncManager & removed ALL instances of duplicate networking
- Added ISyncRequest queue and queue processing for network operations
  - Operations are now execute per FixedUpdate instead of per iteration
- Changed BepInEx version to 5.4.21
- Migrated project settings to use NuGet packages for some dependencies
- Added tons of logging to the debug builds (on GitHub)

# 1.9.2
- Fixed accidental DOS on clients. Some reason the game calls "ChangeLevel" 17 trillion times when the game restarts after the arena

# 1.8.6
- Updated dependencies and readme

# 1.8.4
- Changed icon (made by actibytes)
- Changed networking behavior to increase efficiency (any slander from Lordfirespeed is outdated)
- Somehow forgot and have now added `MapPlayerCount` and `ExtraJump` upgrades

# 1.5.6
- Initial release