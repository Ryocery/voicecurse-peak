# Changelog

## [1.1.0] - Added Sacrifice Event

### Added
- **Sacrifice Event**: Instantly kills the player but revives the nearest player at their death position
    - Configurable cooldown
    - Additional configuration options
- **Blind Event**: Temporarily blinds the player (similar to Alpine hazard)
- **BananaBomb Slip**: New addition for the Slip event
    - Low initial chance (configurable) to launch bananas when triggered
- **Transmute Configuration**: Option to disable instant death and instead transmute all inventory items while alive.
  - This will still cause the transmuted items to drop. 
- **Basketball Transmutation**: New transmutation option with configuration

### Changed
- Instant death effects from Transmute and Death now use `Character.DieInstantly()` instead of `RPCA_Die`
    - Checkpoints now work properly

### Fixed
- Transmuted items are now spawned by the Host and synced across all clients (previously client-side only)

## [1.0.0] - Initial Release

### Added
- First version of VoiceCurse mod
- Basic functionality
- Configuration options