# Development And Validation Notes

This directory is reserved for NoMoreLeaks development notes and local
KSPCommunityFixes memory-leak exports. Generated export files are ignored by
git; this README is the only tracked file in the directory.

## Versioning

Current project version: `1.6.0`

- `#.#.1` patch releases are small fixes, documentation changes, and low-risk
  cleanup adjustments.
- `#.1.#` minor releases are medium leak-coverage passes or workflow
  improvements.
- `1.#.#` major releases mark larger architectural shifts in how the mod
  works.

## Current Patch Targets

Stock KSP and DLC callback owners currently covered include:

- `ModuleCargoPart`, `ModuleInventoryPart`, and `UIPartActionInventorySlot`
- `ModuleDeployableSolarPanel`, `ModuleControlSurface`, and
  `ModuleRobotArmScanner`
- Ground-part, ground-science, and `KerbalEVA` callback families
- `EVAConstructionModeEditor`, `RunwayCollisionHandler`, and `BuildingPickerItem`
- `StageGroup`, `ModuleProceduralFairing`, and vessel resource `PartSet`
- `OverlayGenerator`, `MapView`, `NavBallToggle`, and `InternalNavBall`
- `VesselAutopilotUI`, `CommNetVessel`, and `SpaceTracking`
- Destroyed stock delegate owners in `TimingManager.Instance`

Optional third-party cleanup currently covered:

- `PlanetarySurfaceStructures.ModuleKPBSCorridorNodes | onEditorShipModified`

`RealAntennas` callbacks are intentionally not patched here.

## Implementation Notes

The plugin uses Harmony patches on known stock teardown paths such as module
`OnDestroy()`, `Vessel.Unload()`, `Part.OnDestroy`, `Part.OnDelete`,
`Part.RemoveModule`, and `Part.RemoveModules`.

It also runs a persistent sweeper from a `DontDestroyOnLoad` KSP addon. The
broad stock sweep runs on scene load and on a timed interval. A stronger
scene-unload sweep cleans generic stock `GameEvents` and delegate leftovers.
Low-cost cleanup for `ModuleInventoryPart` and `VesselAutopilotUI` runs every
frame so scene transitions have fewer chances to strand subscriptions.

### Editor To Flight Transition

The `EDITOR -> FLIGHT` transition requires special handling because
`ModuleInventoryPart` instances can still be alive when the old scene begins
tearing down. They therefore slip past cleanup that only removes destroyed
owners.

NoMoreLeaks hooks `EditorLogic.exitEditor` and proactively cleans live inventory
modules before teardown begins. The repeated small inventory-only residue in
June 2026 test runs, while broader stock cleanup was already working, identified
this timing window.

### Other Lifecycle Details

- `ModuleRobotArmScanner` hides `ModuleDeployablePart.OnDestroy()`, so its base
  `onVesselChange` unsubscribe can be skipped.
- Tracking-station cleanup sweeps vessel
  `OrbitRenderer.onVesselIconClicked` callbacks registered outside the central
  `GameEvents` list.
- Map-view cleanup removes destroyed owners from static
  `MapView.OnEnterMapView` and `MapView.OnExitMapView` delegates.
- Scene-unload cleanup scans `TimingManager.Instance` and its stock timing
  buckets for destroyed stock delegate targets.

## Validation Workflow

Use KSPCommunityFixes memory-leak logging and compare exported summaries before
and after a test run. The local `exp-memleaks.sh` helper writes timestamped
exports into this directory when the helper is present beside `summaries/`.
The helper and generated exports are intentionally ignored by git.

Important generated files include:

- `KSPCF-memory-leaks-summary-*.txt`
- `KSPCF-memory-leaks-unhandled-summary-*.txt`
- `KSPCF-memory-leaks-scenes-summary-*.txt`
- `KSPCF-memory-leaks-warnings-summary-*.txt`
- `NoMoreLeaks-debug-summary-*.txt`
- `NoMoreLeaks-debug-markers-*.txt`

Covered callback owners should disappear from the KSPCommunityFixes summary or
drop substantially. Useful runtime markers include:

```text
[NoMoreLeaks] Removed N destroyed callback owners
[NoMoreLeaks] Scene-unload removed N destroyed callback owners
[NoMoreLeaks:Debug] Proactive sweep via EditorLogic.exitEditor.Prefix
[NoMoreLeaks:Debug] EditorExit proactive sweep cleaned N live ModuleInventoryPart instance(s)
```

If inventory entries still appear after the editor-exit marker, subscriptions
were likely re-added after the proactive sweep or caught in a narrow teardown
ordering window.

## Observed Leak History

Recurring stock leak classes in older archived reports:

- `ModuleInventoryPart` inventory callbacks were the most persistent remaining
  stock issue.
- `ModuleCargoPart | OnEVAConstructionMode` was the largest early stock leak.
- `ModuleDeployableSolarPanel | onVesselChange` appeared repeatedly.
- `OverlayGenerator`, `MapView`, `RunwayCollisionHandler`,
  `VesselAutopilotUI`, and `NavBallToggle` appeared throughout early testing.
- `CommNetVessel | onPlanetariumTargetChange` generally stayed near one per
  run, while RealAntennas remained outside NoMoreLeaks' scope.

Condensed timeline:

| Date | Main stock leaks seen | Result |
| --- | --- | --- |
| 2026-05-16 to 2026-05-18 | Large cargo, inventory, solar-panel, map, and UI leak families | Broad stock baseline and inventory regression identified |
| 2026-05-20 to 2026-05-24 | Inventory and stock UI leaks became dominant | Broader stock coverage improved; inventory timing remained unstable |
| 2026-05-25 | Long-session map/UI leaks and recurring inventory callbacks | Dedicated map/UI cleanup worked; inventory remained the main target |
| 2026-06-04 to 2026-06-05 | Repeated small inventory callback sets after scene transitions | Editor-to-flight live-object timing window identified |
| 2026-06-07 to 2026-06-08 | Recent stock residue generally stayed in single digits | Inventory cleanup materially improved; `VesselAutopilotUI` remained the most consistent stock residue |

The NoMoreLeaks-off control run confirmed that the underlying stock callback
leaks still occur without the mod.

## Current State

As of `1.6.0`:

- Long-flight `OverlayGenerator`, `MapView`, and `NavBallToggle` leaks are much
  less prominent.
- `SpaceTracking`, `InternalNavBall`, `BuildingPickerItem`, and
  `EVAConstructionModeEditor` have been substantially reduced.
- `ModuleCargoPart | OnEVAConstructionMode` is usually caught proactively.
- `ModuleInventoryPart` has fallen from large repeated callback sets to small
  residual counts in recent validation sessions.
- `VesselAutopilotUI.OnGameSettingsApplied` is the most consistent remaining
  stock callback residue.

`CommNetScenario: Instance already exists!` is a RealAntennas/CommNet scene
transition conflict and remains outside this mod's scope.

## Build Notes

The project targets .NET Framework `4.7.1` and C# `7.2` for KSP compatibility.
On Linux with Mono:

```bash
xbuild NoMoreLeaks.sln /p:Configuration=Release
```

The generated plugin lands in:

```text
GameData/NoMoreLeaks/Plugins/NoMoreLeaks.dll
```

The GitHub release workflow retrieves private KSP, Unity, and Harmony reference
assemblies from the private `NoMoreLeaks-build-references` repository using the
read-only `KSP_REFERENCES_SSH_KEY` deploy key secret. Generated binaries and
build intermediates remain ignored by git.
