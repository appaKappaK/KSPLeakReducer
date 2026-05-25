# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

This project uses a simple retrospective `MAJOR.MINOR.PATCH` scheme:

- patch (`#.#.1`) for small fixes, doc updates, and low-risk cleanup changes
- minor (`#.1.#`) for medium leak-coverage passes or workflow improvements
- major (`1.#.#`) for large structural changes in how the mod works

Older entries below were reconstructed from commit history and testing notes so the version history matches the work that already happened.

## [Unreleased]

## [1.5.0] - 2026-05-25

### Added

- Scene-unload sweeping for `NoMoreLeaks`, including broad stock callback cleanup at scene teardown.
- Full `TimingManager.Instance` bucket scanning for destroyed stock delegate owners across `timing0` through `timing5`, `timingPre`, and `timingFI`.
- Stock cleanup patches for `StageGroup`, `ModuleProceduralFairing`, and vessel resource-flow `PartSet` callback owners.

### Changed

- `CommNetVessel` cleanup now also removes `CommNet.OnNetworkInitialized` subscriptions.
- `README.md` now documents the scene-unload sweep, broader timing-manager coverage, and the current validation caveat for this pass.

## [1.4.2] - 2026-05-25

### Changed

- Established a retrospective version history from the existing commit log and testing timeline.
- Aligned project docs and the shipped `.version` metadata with the current release numbering.

## [1.4.1] - 2026-05-25

### Added

- `CHANGELOG.md` in `Keep a Changelog` format.

### Changed

- Polished the README leak-history wording so it reads as project documentation instead of session notes.

## [1.4.0] - 2026-05-25

### Added

- Per-frame `VesselAutopilotUI` callback sweeping for `OnGameSettingsApplied`, `onVesselChange`, and `onKerbalLevelUp`.
- Broader `VesselAutopilotUI` destroy-time cleanup for all known stock subscriptions.

### Changed

- `CommNetVessel` and `RunwayCollisionHandler` cleanup now runs on both `OnDestroy` prefix and postfix, matching the safer pattern already used elsewhere in the mod.
- `README.md` was refreshed to match the current leak profile, exporter behavior, and validation workflow.

## [1.3.0] - 2026-05-25

### Added

- Dedicated long-session map/UI cleanup for `OverlayGenerator`, `MapView`, `NavBallToggle`, `InternalNavBall`, `SpaceTracking`, `BuildingPickerItem`, and `EVAConstructionModeEditor`.
- Verbose debug logging configuration and exported debug summaries for `NoMoreLeaks`.

### Changed

- Inventory cleanup timing was tightened around editor and scene-transition churn after the `ModuleInventoryPart` regressions seen on `2026-05-24` and `2026-05-25`.

## [1.2.0] - 2026-05-24

### Added

- Broader stock cleanup coverage for callback owners beyond the original inventory and ground-part targets.
- Editor-specific `ModuleInventoryPart` lifecycle cleanup, including pre-subscribe sweeping and stronger editor teardown handling.

### Changed

- Leak exports and README validation notes were expanded as the test workflow became more repeatable.

## [1.1.0] - 2026-05-19

### Fixed

- Tracking-station vessel icon callback cleanup by sweeping `OrbitRenderer.onVesselIconClicked`.
- `ModuleRobotArmScanner` cleanup for `onVesselChange`, covering the case where the stock base unsubscribe is skipped.

## [1.0.1] - 2026-05-18

### Changed

- Stopped tracking generated plugin binaries and tightened local ignore rules for build artifacts.
- Reinforced ground-part cleanup after the first round of testing.

## [1.0.0] - 2026-05-18

### Added

- Persistent `DontDestroyOnLoad` sweeper architecture instead of relying only on per-type teardown patches.
- Inventory callback sweeping for `ModuleInventoryPart` and `UIPartActionInventorySlot`.
- First README-based install and validation guidance.

## [0.3.0] - 2026-05-17

### Added

- Ground-part, ground-science, and `KerbalEVA` callback cleanup.

## [0.2.0] - 2026-05-17

### Added

- Inventory, cargo, and control-surface leak patches for the first stock callback families under active cleanup.

## [0.1.0] - 2026-05-17

### Added

- Initial `NoMoreLeaks` plugin scaffold with Harmony patch loading and packaged `GameData` layout.

## [0.0.1] - 2026-05-17

### Added

- Initial repository import.
