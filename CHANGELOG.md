# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project follows a simple `MAJOR.MINOR.PATCH` version scheme through
`GameData/NoMoreLeaks/NoMoreLeaks.version`.

## [Unreleased]

### Added

- Per-frame `VesselAutopilotUI` callback sweeping for `OnGameSettingsApplied`, `onVesselChange`, and `onKerbalLevelUp`.
- Broader `VesselAutopilotUI` destroy-time cleanup for all known stock subscriptions.
- Debug-export ignore rules for `NoMoreLeaks` summary files in `.gitignore`.

### Changed

- `CommNetVessel` and `RunwayCollisionHandler` cleanup now runs on both `OnDestroy` prefix and postfix, matching the safer pattern already used elsewhere in the mod.
- `README.md` now reflects the current leak profile, exporter behavior, and validation workflow.

## [0.1.0] - 2026-05-25

### Added

- Initial public project structure for the `NoMoreLeaks` Harmony plugin.
- Stock leak cleanup for inventory, cargo, ground science, editor, map-view, tracking-station, and several stock flight UI callback owners reported by KSPCommunityFixes.
- Persistent runtime sweeper with optional verbose debug logging.
- Helper export script workflow for KSPCF summaries and `NoMoreLeaks` debug output.
- Leak-history notes documenting the stock callback trends observed during May 2026 testing.
