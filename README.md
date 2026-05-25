# NoMoreLeaks

Small KSP/Harmony patch plugin for stock `Assembly-CSharp` event callback leaks reported by KSPCommunityFixes.

This mod does not replace KSPCommunityFixes. It tries to prevent selected callbacks from remaining subscribed after their owning Unity object is destroyed, so KSPCF has less cleanup work to do later.

## Install

Copy the packaged folder into KSP `GameData`:

```text
GameData/NoMoreLeaks/
  NoMoreLeaks.cfg
  NoMoreLeaks.version
  Plugins/NoMoreLeaks.dll
```

Harmony 2 is required:

```text
GameData/000_Harmony/0Harmony.dll
```

On startup, the log should contain:

```text
[NoMoreLeaks] Harmony patches applied
[NoMoreLeaks] VerboseDebugLogging=True
```

When the runtime sweeper removes stale callbacks, the log should contain:

```text
[NoMoreLeaks] Removed N destroyed callback owners
```

When verbose debug logging is enabled, the log will also contain lines like:

```text
[NoMoreLeaks:Debug] Removed 1 callback(s) from onPartActionUICreate owned by Assembly-CSharp:ModuleInventoryPart via RemoveOwner
```

## Current Patch Targets

Stock KSP / DLC callback owners:

- `ModuleCargoPart | OnEVAConstructionMode`
- `ModuleInventoryPart | onPartActionUICreate`
- `ModuleInventoryPart | onModuleInventoryChanged`
- `ModuleInventoryPart | onEditorPartEvent`
- `ModuleInventoryPart | OnPartPurchased`
- `ModuleInventoryPart | OnInventoryPartOnMouseChanged`
- `UIPartActionInventorySlot | OnEVACargoMode`
- `UIPartActionInventorySlot | onEditorPartDeleted`
- `ModuleDeployableSolarPanel | onVesselChange`
- `ModuleControlSurface | onEditorPartEvent`
- `ModuleControlSurface | onVesselReferenceTransformSwitch`
- `ModuleRobotArmScanner | onVesselChange`
- `ModuleGroundPart | onPartActionUIShown`
- `ModuleGroundPart | onPartActionUIDismiss`
- `ModuleGroundPart | onVesselChange`
- `ModuleGroundPart | onPartWillDie`
- `ModuleGroundPart | onLevelConfirmExit`
- `ModuleGroundPart | OnEVAConstructionMode`
- `ModuleGroundSciencePart | onGroundScienceDeregisterCluster`
- `ModuleGroundSciencePart | onGroundScienceClusterUpdated`
- `ModuleGroundSciencePart | onGroundScienceClusterPowerStateChanged`
- `ModuleGroundExperiment | onGroundScienceGenerated`
- `ModuleGroundExperiment | onGroundScienceTransmitted`
- `ModuleGroundExpControl | onGroundSciencePartDeployed`
- `ModuleGroundExpControl | onGroundSciencePartEnabledStateChanged`
- `ModuleGroundExpControl | onGroundSciencePartRemoved`
- `KerbalEVA | OnROCExperimentStored`
- `KerbalEVA | OnROCExperimentReset`
- `EVAConstructionModeEditor | OnClick`
- `RunwayCollisionHandler | OnDestructibleLoaded`
- `BuildingPickerItem | OnInViewChange`
- `BuildingPickerItem | OnClick`
- `OverlayGenerator | onPlanetariumTargetChange`
- `OverlayGenerator | onGameStateLoad`
- `OverlayGenerator | MapView.OnEnterMapView`
- `OverlayGenerator | MapView.OnExitMapView`
- `MapView | TimingManager.Instance.timing5.onLateUpdate`
- `VesselAutopilotUI | OnGameSettingsApplied`
- `NavBallToggle | OnMapExited`
- `InternalNavBall | onVesselChange`
- `CommNetVessel | onPlanetariumTargetChange`
- `SpaceTracking | OnVesselIconClicked`
- `SpaceTracking | OnMapViewFiltersModified`
- `SpaceTracking | onInputLocksModified`
- `SpaceTracking | onGUIRecoveryDialogSpawn`
- `SpaceTracking | onGUIRecoveryDialogDespawn`
- `SpaceTracking | onPlanetariumTargetChange`

Optional third-party cleanup currently covered:

- `PlanetarySurfaceStructures.ModuleKPBSCorridorNodes | onEditorShipModified`

`RealAntennas` callbacks are intentionally not patched here.

## How It Works

The plugin uses Harmony patches on known stock teardown paths such as module `OnDestroy()`, `Part.OnDestroy`, `Part.OnDelete`, `Part.RemoveModule`, and `Part.RemoveModules`.

It also runs a persistent sweeper from a `DontDestroyOnLoad` KSP addon. The broad stock sweep runs on scene load and on a timed interval, while inventory callback cleanup now runs every frame so scene transitions like `EDITOR -> FLIGHT` have fewer chances to strand `ModuleInventoryPart` subscriptions.

Some fixes are direct lifecycle patches. For example, `ModuleRobotArmScanner` hides `ModuleDeployablePart.OnDestroy()`, so the base `onVesselChange` unsubscribe can be skipped unless patched directly.

Tracking-station cleanup also sweeps vessel `OrbitRenderer.onVesselIconClicked` callbacks, because `SpaceTracking` registers per-vessel icon callbacks outside the central `GameEvents` list.

Map-view cleanup also removes destroyed owners from static `MapView.OnEnterMapView` / `MapView.OnExitMapView` delegates and `TimingManager.Instance.timing5.onLateUpdate`, because long flight sessions were repeatedly accumulating `OverlayGenerator` and `MapView` UI objects.

## Validation

Use KSPCF memory leak logging and compare the exported summary before and after a run. The helper script under `memleaks/export-ksp-memleaks.sh` now exports:

- `KSPCF-memory-leaks-raw.txt`
- `KSPCF-memory-leaks-summary.txt`
- `KSPCF-memory-leaks-warnings.txt`
- `NoMoreLeaks-debug-raw.txt`
- `NoMoreLeaks-debug-summary.txt`

Expected improvement is that covered callback owners either disappear from the KSPCF summary or drop significantly. If a covered owner still appears, check `KSP.log` for:

```text
[NoMoreLeaks] Removed
```

If that line never appears during gameplay, the sweeper did not catch any destroyed owners before KSPCF did. If the debug files are empty, confirm `GameData/NoMoreLeaks/NoMoreLeaks.cfg` is present in the live install.

## Observed Leak History

Based on archived KSPCF summaries from `~/.local/share/Trash/files`, the repo `leak-sums/` folder, and the `memleaks/nomoreleaks-off` control run, the stock leak trend has been consistent enough to justify this mod.

Recurring stock leak classes from the older trash reports:

- `ModuleInventoryPart` inventory callbacks were the most persistent remaining stock issue: `866` removals each for `onPartActionUICreate` and `onModuleInventoryChanged` across `17` archived summary files.
- `ModuleCargoPart | OnEVAConstructionMode` was the worst early stock leak: `4482` removals across `5` archived summary files.
- `ModuleDeployableSolarPanel | onVesselChange` appeared repeatedly: `232` removals across `10` archived summary files.
- `OverlayGenerator`, `MapView`, `RunwayCollisionHandler`, `VesselAutopilotUI`, and `NavBallToggle` showed up in many early and mid-May runs, but usually at much lower counts than the inventory leaks.
- `CommNetVessel | onPlanetariumTargetChange` stayed near `1` per run and `RealAntennas` entries were intentionally left out of this mod's scope.

Short timeline:

| Date | Source | Main stock leaks seen | Read |
| --- | --- | --- | --- |
| 2026-05-16 | archived trash baseline | `ModuleCargoPart` `2124`, `ModuleDeployableSolarPanel` `73`, `ModuleInventoryPart` `63/63`, plus overlay/map/autopilot/navball leaks | very noisy stock baseline |
| 2026-05-17 | archived trash runs | inventory leaks dominated many runs at `15` to `59`; some runs still showed ground science, overlay/map, control surface, and EVA editor leaks | partial improvement, not stable |
| 2026-05-18 | archived trash runs | inventory leaks spiked again up to `221/221`, with `OnPartPurchased` and `onEditorPartEvent` at `95` each; `UIPartActionInventorySlot` and `ModuleRobotArmScanner` also appeared | clear regression around inventory teardown |
| 2026-05-20 | repo `leak-sums/5-20-26` | only `VesselAutopilotUI`, `CommNetVessel`, and `RealAntennas` remained | best observed stock result so far |
| 2026-05-21 to 2026-05-23 | repo `leak-sums/` | `ModuleInventoryPart` returned strongly (`28` to `85`), `UIPartActionInventorySlot` reached `50/50`, `VesselAutopilotUI` stayed present, and `SpaceTracking` / `InternalNavBall` showed up in the worst run on `2026-05-23` | current focus area that drove the newer sweeper changes |
| 2026-05-24 | repo `memleaks/5-24-26*` | inventory-heavy editor runs ranged from `45/45` up to `78/78`; `UIPartActionInventorySlot`, `SpaceTracking`, and `InternalNavBall` were reduced by newer patches | inventory timing still unstable, but broader stock coverage improved |
| 2026-05-25 early | repo `memleaks/NEW LEAKS SUMMARY` long-flight run | `OverlayGenerator`, `MapView`, `NavBallToggle`, and `VesselAutopilotUI` became the dominant stock leaks during a long orbital session, while `ModuleInventoryPart` was low at `6/6` | long-flight map/UI leak cluster identified |
| 2026-05-25 latest | repo `memleaks/NEW LEAKS SUMMARY` after map/UI fixes | `OverlayGenerator`, `MapView`, `NavBallToggle`, `SpaceTracking`, `InternalNavBall`, `BuildingPickerItem`, and `EVAConstructionModeEditor` disappeared from the KSPCF summary; `ModuleInventoryPart` regressed to `69/69` with `19/19` on purchased/editor events | map/UI work paid off, inventory remains the main persistent stock leak |

Control comparison:

- With `NoMoreLeaks` off, the `memleaks/nomoreleaks-off` summary still showed broad stock leaks on `2026-05-24`, led by `ModuleGroundExperiment`, `ModuleInventoryPart`, `ModuleGroundPart`, `ModuleDeployableSolarPanel`, `OverlayGenerator`, and `VesselAutopilotUI`.
- That control run matters because it shows the goal of this mod is still valid: the stock game is retaining destroyed callback owners on its own, and the remaining work is about narrowing that list, not proving the problem exists.

## Current State

As of `2026-05-25`, the mod has materially improved several stock leak classes:

- long-flight `OverlayGenerator` / `MapView` / `NavBallToggle` leaks were reduced out of the KSPCF summary after dedicated map/UI cleanup was added
- `SpaceTracking`, `InternalNavBall`, `BuildingPickerItem`, and `EVAConstructionModeEditor` are now much less prominent than in the earlier worst runs
- the plugin is actively removing the targeted callbacks, confirmed by `NoMoreLeaks-debug-summary.txt`

The main unresolved stock issue is still `ModuleInventoryPart`:

- latest observed KSPCF summary: `69` `onPartActionUICreate`, `69` `onModuleInventoryChanged`, `19` `OnPartPurchased`, `19` `onEditorPartEvent`
- latest debug summary shows `NoMoreLeaks` is removing many `ModuleInventoryPart` callbacks itself, but KSPCF is still catching additional destroyed owners later
- current working theory is that inventory subscriptions are still slipping through during editor teardown and `EDITOR -> FLIGHT` transitions

## Build Notes

The project targets old .NET Framework for KSP compatibility. On Linux, the DLL can be built with Mono `mcs` against the local KSP assemblies.

Generated binaries under `GameData/NoMoreLeaks/Plugins/` and build intermediates under `bin/` / `obj/` are ignored by git.
