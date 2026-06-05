# NoMoreLeaks

Small KSP/Harmony patch plugin for stock `Assembly-CSharp` event callback leaks reported by KSPCommunityFixes.

This mod does not replace KSPCommunityFixes. It tries to prevent selected callbacks from remaining subscribed after their owning Unity object is destroyed, so KSPCF has less cleanup work to do later.

## Versioning

Current project version: `1.6.0`

- `#.#.1` patch releases are small fixes, doc changes, and low-risk cleanup adjustments
- `#.1.#` minor releases are medium leak-coverage passes or workflow improvements
- `1.#.#` major releases mark larger architectural shifts in how the mod works

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
- `StageGroup | onDeltaVAppAtmosphereChanged`
- `StageGroup | onDeltaVAppInfoItemsChanged`
- `BuildingPickerItem | OnInViewChange`
- `BuildingPickerItem | OnClick`
- `ModuleProceduralFairing | onVariantApplied`
- `ModuleProceduralFairing | onVariantsAdded`
- `OverlayGenerator | onPlanetariumTargetChange`
- `OverlayGenerator | onGameStateLoad`
- `OverlayGenerator | MapView.OnEnterMapView`
- `OverlayGenerator | MapView.OnExitMapView`
- `TimingManager.Instance.(timing0..5/timingPre/timingFI) | destroyed stock delegate owners`
- `VesselAutopilotUI | OnGameSettingsApplied`
- `VesselAutopilotUI | onVesselChange`
- `VesselAutopilotUI | onKerbalLevelUp`
- `NavBallToggle | OnMapExited`
- `InternalNavBall | onVesselChange`
- `PartSet | onPartResourceFlowStateChange`
- `PartSet | onPartResourceFlowModeChange`
- `CommNetVessel | CommNet.OnNetworkInitialized`
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

The plugin uses Harmony patches on known stock teardown paths such as module `OnDestroy()`, `Vessel.Unload()`, `Part.OnDestroy`, `Part.OnDelete`, `Part.RemoveModule`, and `Part.RemoveModules`.

It also runs a persistent sweeper from a `DontDestroyOnLoad` KSP addon. The broad stock sweep runs on scene load and on a timed interval, a stronger scene-unload sweep cleans generic stock `GameEvents` and delegate leftovers, and low-cost hot-path cleanup for `ModuleInventoryPart` and `VesselAutopilotUI` runs every frame so scene transitions like `EDITOR -> FLIGHT` have fewer chances to strand subscriptions.

### Editor → Flight transition

The `EDITOR -> FLIGHT` transition requires special handling because `ModuleInventoryPart` instances are still alive when the old scene begins tearing down, causing them to slip past the standard `RemoveDestroyedOwners` sweep. The mod hooks `EditorLogic.exitEditor` to sweep live instances proactively before any teardown begins. See the `1.6.0` changelog entry for the full root cause analysis.

This conclusion came from the June 4-5 memory-leak exports under `/home/matt/Desktop/ksp-memleaks/`. In those sessions, NML debug summaries showed broad cleanup already working for cargo, map/UI, fairing, building-picker, and destroyed stock callback owners, while KSPCF still removed the same small `ModuleInventoryPart` editor callback family after scene transitions. That pattern pointed to a timing window rather than a missing callback name: the old editor inventory modules were still live when NML's destroyed-owner sweeps ran, then were destroyed later and caught by KSPCF.

### Other lifecycle details

Some fixes are direct lifecycle patches. For example, `ModuleRobotArmScanner` hides `ModuleDeployablePart.OnDestroy()`, so the base `onVesselChange` unsubscribe can be skipped unless patched directly.

Tracking-station cleanup also sweeps vessel `OrbitRenderer.onVesselIconClicked` callbacks, because `SpaceTracking` registers per-vessel icon callbacks outside the central `GameEvents` list.

Map-view cleanup removes destroyed owners from static `MapView.OnEnterMapView` / `MapView.OnExitMapView` delegates. The scene-unload pass scans `TimingManager.Instance` and the stock timing buckets (`timing0` through `timing5`, `timingPre`, and `timingFI`) for destroyed stock delegate targets.

## Validation

Use KSPCF memory leak logging and compare the exported summaries before and after a run. The helper script under `memleaks/export-ksp-memleaks.sh` writes timestamped files by default so repeated exports do not overwrite each other:

- `KSPCF-memory-leaks-raw.txt`
- `KSPCF-memory-leaks-summary.txt`
- `KSPCF-memory-leaks-warnings.txt`
- `NoMoreLeaks-debug-raw.txt`
- `NoMoreLeaks-debug-summary.txt`

The expected result is that covered callback owners either disappear from the KSPCF summary or drop substantially. If a covered owner still appears, check `KSP.log` for:

```text
[NoMoreLeaks] Removed
```

If that line never appears during gameplay, the sweeper did not catch any destroyed owners before KSPCF did. If the debug files are empty, confirm `GameData/NoMoreLeaks/NoMoreLeaks.cfg` is present in the live install.

Scene transitions may also log:

```text
[NoMoreLeaks] Scene-unload removed N destroyed callback owners
```

When the editor exit sweep fires successfully, verbose debug logging will show:

```text
[NoMoreLeaks:Debug] Proactive sweep via EditorLogic.exitEditor.Prefix
[NoMoreLeaks:Debug] EditorExit proactive sweep cleaned N live ModuleInventoryPart(s)
```

If `ModuleInventoryPart` entries still appear in the KSPCF summary after this line is present, the subscriptions are being re-added after the sweep fires, which points to a different timing issue.

## Observed Leak History

This section condenses the leak patterns seen across older KSPCF exports, the checked-in `leak-sums/` snapshots, and the `NoMoreLeaks` off control run. It is meant to show the recurring stock callback families that kept resurfacing across different sessions and patch revisions, not to preserve every individual test run.

Recurring stock leak classes in the older archived reports:

- `ModuleInventoryPart` inventory callbacks were the most persistent remaining stock issue, with `866` removals each for `onPartActionUICreate` and `onModuleInventoryChanged` across `17` archived summary files.
- `ModuleCargoPart | OnEVAConstructionMode` was the largest early stock leak, with `4482` removals across `5` archived summary files.
- `ModuleDeployableSolarPanel | onVesselChange` appeared repeatedly, with `232` removals across `10` archived summary files.
- `OverlayGenerator`, `MapView`, `RunwayCollisionHandler`, `VesselAutopilotUI`, and `NavBallToggle` appeared in many early and mid-May runs, though usually at much lower counts than the inventory leaks.
- `CommNetVessel | onPlanetariumTargetChange` stayed near `1` per run, and `RealAntennas` entries were intentionally left outside this mod's scope.

Condensed timeline:

| Date | Source | Main stock leaks seen | Summary |
| --- | --- | --- | --- |
| 2026-05-16 | archived trash baseline | `ModuleCargoPart` `2124`, `ModuleDeployableSolarPanel` `73`, `ModuleInventoryPart` `63/63`, plus overlay/map/autopilot/navball leaks | Broad stock leak baseline before the newer cleanup work |
| 2026-05-17 | archived trash runs | inventory leaks dominated many runs at `15` to `59`; some runs still showed ground science, overlay/map, control surface, and EVA editor leaks | Partial improvement, but not yet stable |
| 2026-05-18 | archived trash runs | inventory leaks spiked again up to `221/221`, with `OnPartPurchased` and `onEditorPartEvent` at `95` each; `UIPartActionInventorySlot` and `ModuleRobotArmScanner` also appeared | Clear regression around inventory teardown |
| 2026-05-20 | repo `leak-sums/5-20-26` | only `VesselAutopilotUI`, `CommNetVessel`, and `RealAntennas` remained | Best stock-focused result observed in the early runs |
| 2026-05-21 to 2026-05-23 | repo `leak-sums/` | `ModuleInventoryPart` returned strongly (`28` to `85`), `UIPartActionInventorySlot` reached `50/50`, `VesselAutopilotUI` stayed present, and `SpaceTracking` / `InternalNavBall` showed up in the worst run on `2026-05-23` | Inventory and stock UI leaks became the next main focus |
| 2026-05-24 | repo `memleaks/5-24-26*` | inventory-heavy editor runs ranged from `45/45` up to `78/78`; `UIPartActionInventorySlot`, `SpaceTracking`, and `InternalNavBall` were reduced by newer patches | Broader stock coverage improved, but inventory timing remained unstable |
| 2026-05-25 early | repo `memleaks/NEW LEAKS SUMMARY` long-flight run | `OverlayGenerator`, `MapView`, `NavBallToggle`, and `VesselAutopilotUI` became the dominant stock leaks during a long orbital session, while `ModuleInventoryPart` was low at `6/6` | Long-flight map/UI leak cluster identified |
| 2026-05-25 midday | repo `memleaks/NEW LEAKS SUMMARY` after map/UI fixes | `OverlayGenerator`, `MapView`, `NavBallToggle`, `SpaceTracking`, `InternalNavBall`, `BuildingPickerItem`, and `EVAConstructionModeEditor` disappeared from the KSPCF summary; `ModuleInventoryPart` regressed to `69/69` with `19/19` on purchased/editor events | Map/UI work paid off, but inventory remained the dominant stock leak |
| 2026-05-25 latest | repo `memleaks/NEW LEAKS SUMMARY` longer session after tighter inventory sweeps | `ModuleInventoryPart` improved again to `16/16` with `2/2` on purchased/editor events; `VesselAutopilotUI` rose to `20`; map/UI leak cluster stayed gone | Inventory timing improved again, leaving `VesselAutopilotUI` as the top remaining stock issue |
| 2026-06-04 to 2026-06-05 | three validation sessions after 1.5.0 | `ModuleInventoryPart` at `12/12` in worst session; `ModuleCargoPart` removed proactively by NML (not appearing in KSPCF); `VesselAutopilotUI` at `3` in worst session; `CommNetScenario` ERR present each session (RealAntennas/CommNet conflict, out of scope) | 1.5.0 showing improvement over May baseline; editor→flight transition identified as remaining gap |
| 2026-06-04 to 2026-06-05 | desktop exports in `/home/matt/Desktop/ksp-memleaks/` | first two exported KSPCF summaries were empty; later summaries repeatedly showed `ModuleInventoryPart` editor callbacks at `3/3/3/3`, with one longer run reaching `12` on `onPartActionUICreate` and `onModuleInventoryChanged` | The repeated small inventory-only residue, alongside active NML debug cleanup for other stock owners, supported the live-object editor-exit timing diagnosis |

Control comparison:

- With `NoMoreLeaks` off, the `memleaks/nomoreleaks-off` summary still showed broad stock leaks on `2026-05-24`, led by `ModuleGroundExperiment`, `ModuleInventoryPart`, `ModuleGroundPart`, `ModuleDeployableSolarPanel`, `OverlayGenerator`, and `VesselAutopilotUI`.
- That control run matters because it confirms the underlying stock problem still exists without the mod. The remaining work is about shrinking the leak list, not proving that the stock game leaks in the first place.

## Current State

As of `1.6.0`, the mod has materially improved several stock leak classes:

- Long-flight `OverlayGenerator` / `MapView` / `NavBallToggle` leaks were reduced out of the KSPCF summary after dedicated map/UI cleanup was added.
- `SpaceTracking`, `InternalNavBall`, `BuildingPickerItem`, and `EVAConstructionModeEditor` are now much less prominent than in the earlier worst runs.
- `ModuleCargoPart | OnEVAConstructionMode` is being caught proactively by NML and no longer appearing in KSPCF summaries.
- `ModuleInventoryPart` has come down substantially from its `69/69/19/19` regression; the `EDITOR -> FLIGHT` proactive sweep added in `1.6.0` targets the remaining window where still-alive instances were slipping past `RemoveDestroyedOwners`.

The main unresolved stock issue is `ModuleInventoryPart` during the editor exit window, which `1.6.0` directly addresses. `VesselAutopilotUI` counts are low but still present; those are more sensitive to UI lifecycle timing during long sessions and remain under observation.

`CommNetScenario: Instance already exists!` appears once per scene reload and is a known RealAntennas/CommNet conflict on scene transition. It is outside this mod's scope.

## Changelog

Project-level release notes now live in [CHANGELOG.md](CHANGELOG.md). The README keeps the higher-level leak history and current validation state, while the changelog is the better place for versioned patch notes.

## Build Notes

The project targets old .NET Framework for KSP compatibility. On Fedora/Linux, install Mono development tools and build the release DLL with `xbuild`:

```bash
sudo dnf install mono-devel
xbuild NoMoreLeaks.sln /p:Configuration=Release
```

The generated plugin lands in:

```text
GameData/NoMoreLeaks/Plugins/NoMoreLeaks.dll
```

The local Fedora Mono package supports C# `7.2` and the .NET Framework `4.7.1` API profile, so the project is pinned to those versions for reproducible local builds.

### GitHub release workflow

The `.github/workflows/release.yml` workflow builds the plugin, packages `GameData/NoMoreLeaks`, and creates or updates the GitHub release matching `GameData/NoMoreLeaks/NoMoreLeaks.version`. Release notes are pulled from the matching `CHANGELOG.md` section, so version `1.6.0` publishes the `1.6.0` changelog notes.

GitHub Actions does not include KSP's proprietary assemblies, so the workflow needs a repository secret named `KSP_REFERENCES_URL` that points to a private zip with this layout:

```text
GameData/000_Harmony/0Harmony.dll
KSP_x64_Data/Managed/Assembly-CSharp.dll
KSP_x64_Data/Managed/UnityEngine.dll
KSP_x64_Data/Managed/UnityEngine.CoreModule.dll
KSP_x64_Data/Managed/UnityEngine.UI.dll
KSP_x64_Data/Managed/UnityEngine.UIModule.dll
```

If the URL requires bearer authentication, also set `KSP_REFERENCES_TOKEN`.

One local way to prepare the reference zip is:

```bash
cd "/stm/SteamLibrary/steamapps/common/Kerbal Space Program"
zip -r ~/ksp-references.zip \
  GameData/000_Harmony/0Harmony.dll \
  KSP_x64_Data/Managed/Assembly-CSharp.dll \
  KSP_x64_Data/Managed/UnityEngine.dll \
  KSP_x64_Data/Managed/UnityEngine.CoreModule.dll \
  KSP_x64_Data/Managed/UnityEngine.UI.dll \
  KSP_x64_Data/Managed/UnityEngine.UIModule.dll
```

Generated binaries under `GameData/NoMoreLeaks/Plugins/` and build intermediates under `bin/` / `obj/` are ignored by git.
