# Changelog

1.1.0 - Added new Sacrifice event
- Added the Sacrifice event that kills you instantly but revives the nearest player at their death position. This comes with a cooldown.
- Added configuration options for the Sacrifice event, including keywords and enabling/disabling the event.
- Added additional configuration for Transmute by disabling instant death and instead transmuting all items in your inventory while you are alive.
- Made the instant death effects from Transmute and Death use `Character.DieInstantly()` rather than `RPCA_Die` so that checkpoints now work properly.
- Added Basketball transmutation and config.
- Fixed issue where transmuted items were client-side only, they are now spawned by the Host and synced across all clients.

1.0.0 - Initial release
- First version of VoiceCurse mod with basic functionality and configuration options.