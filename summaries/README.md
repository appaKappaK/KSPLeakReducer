# Development And Validation Notes

This directory is reserved for NoMoreLeaks development notes and local
KSPCommunityFixes memory-leak exports. Generated export files are ignored by
git; this README and the public export helper are tracked.

## Versioning

Current project version: `1.7.0`

- `#.#.1` patch releases are small fixes, documentation changes, and low-risk
  cleanup adjustments.
- `#.1.#` minor releases are medium leak-coverage passes or workflow
  improvements.
- `1.#.#` major releases mark larger architectural shifts in how the mod
  works.

## Current Patch Targets

Stock KSP and DLC callback owners currently covered include:

- `ModuleCargoPart`, `ModuleInventoryPart`, `UIPartActionInventorySlot`, and
  `UIPartActionControllerInventory`
- `ModuleDeployableSolarPanel`, `ModuleControlSurface`, and
  `ModuleRobotArmScanner`
- Ground-part, ground-science, and `KerbalEVA` callback families
- `EVAConstructionModeEditor`, `RunwayCollisionHandler`, and `BuildingPickerItem`
- `StageGroup`, `ModuleProceduralFairing`, and vessel resource `PartSet`
- `OverlayGenerator`, `MapView`, `NavBallToggle`, and `InternalNavBall`
- `VesselAutopilotUI`, `CommNetVessel`, and `SpaceTracking`
- Destroyed stock delegate owners in `TimingManager.Instance`

Optional third-party cleanup currently covered:

- Kerbal Planetary Base Systems:
  `PlanetarySurfaceStructures.ModuleKPBSCorridorNodes | onEditorShipModified`

This targets the `PlanetarySurfaceStructures` assembly and is unrelated to the
separate `PlanetsideExplorationTechnologies` assembly. `RealAntennas` callbacks
are intentionally not patched here.

## Implementation Notes

The plugin uses Harmony patches on known stock teardown paths such as module
`OnDestroy()`, `Vessel.Unload()`, `Part.OnDestroy`, `Part.OnDelete`,
`Part.RemoveModule`, `Part.RemoveModules`, and
`ModuleInventoryPart.DeletePartObject`.

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

### Inventory Part Creation And Deletion

Inventory-created parts can strand callbacks when they are deleted before the
normal part lifecycle finishes. NoMoreLeaks cleans the selected part and its
child-part hierarchy before `ModuleInventoryPart.DeletePartObject` destroys it.
It also sweeps inventory callbacks after both
`UIPartActionControllerInventory.CreatePartFromInventory` overloads.

### Other Lifecycle Details

- Patch helper methods avoid Harmony's reserved `Cleanup` name. Harmony treats
  that name as a patch-cleanup method and can invoke it during `PatchAll()`
  instead of during normal gameplay teardown.
- `Part.OnDelete` cleanup traverses child-part hierarchies so callback-owning
  modules below the root part are also cleaned. `Part.OnDestroy` cleans only the
  part being destroyed so surviving child parts retain their callbacks.
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
and after a test run. Run the public `exp-memleaks` helper with a KSP log:

```bash
./summaries/exp-memleaks /path/to/KSP.log
```

It writes timestamped exports into this directory by default. Pass an existing
output directory as the second argument to write them elsewhere. The helper
refuses to overwrite an export with the same timestamp. Generated exports are
ignored by git, but raw exports can contain mod names, object identifiers, and
matching log lines, so review them before sharing publicly.

Important generated files include:

- `KSPCF-memory-leaks-summary-*.txt`
- `KSPCF-memory-leaks-unhandled-summary-*.txt`
- `KSPCF-memory-leaks-scenes-summary-*.txt`
- `KSPCF-memory-leaks-warnings-summary-*.txt`
- `NoMoreLeaks-debug-summary-*.txt`
- `NoMoreLeaks-debug-markers-*.txt`

The warnings export includes every exception header plus targeted warnings and
errors related to NoMoreLeaks, KSPCommunityFixes, science, and communications.
It is intentionally not a complete export of every ordinary KSP warning or
error.

Treat a run as a complete NoMoreLeaks validation only when the exported
`Harmony patches applied` marker count is non-zero, or the raw log contains
`[NoMoreLeaks] Harmony patches applied`. The exporter always prints the marker
label, even when its count is zero. The four archived runs with NoMoreLeaks
installed contain proactive-cleanup output but report a zero success-marker
count. They therefore represent partial patch installations from before the
reserved Harmony `Cleanup` method-name collision was fixed, not full `1.7.0`
validation runs.

KSPCommunityFixes' per-scene `cleaned N` total includes unhandled observations
and can repeatedly count persistent third-party callbacks. Use its callback-owner
summary, NoMoreLeaks markers, and allocation trend together instead of treating
that total as the number of callbacks actually removed.

Covered callback owners should disappear from the KSPCommunityFixes summary or
drop substantially. Useful runtime markers include:

```text
[NoMoreLeaks] Harmony patches applied
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
| 2026-06-10 | NoMoreLeaks-off control run grew from 9.228 GiB to 17.489 GiB across 43 scene exits; KSPCF cleaned 26,617 callbacks | Confirmed the underlying stock leaks remain severe without proactive cleanup; added inventory part-creation/deletion coverage and fixed Harmony reserved-name collisions |

The NoMoreLeaks-off control run confirmed that the underlying stock callback
leaks still occur without the mod. The dominant stock residue was
`ModuleCargoPart.OnEVAConstructionMode`, with 5,315 callbacks cleaned by
KSPCommunityFixes during the control session.

### Archived Export Review

All locally generated June 2026 exports through `2026-06-13_03-30-47` have
been reviewed. Allocation delta is measured from the first to final scene-exit
sample in each export.

| Export | NoMoreLeaks state | Scene exits | Final allocation | Allocation delta | Callbacks in KSPCF handled summary |
| --- | --- | ---: | ---: | ---: | ---: |
| `2026-06-08_03-03-56` | Partial install; zero success-marker count | 8 | 11.350 GiB | +1.618 GiB | 28 |
| `2026-06-09_05-57-49` | Partial install; zero success-marker count | 27 | 16.641 GiB | +6.880 GiB | 398 |
| `2026-06-09_20-15-48` | Partial install; zero success-marker count | 14 | 11.925 GiB | +1.766 GiB | 55 |
| `2026-06-10_02-54-33` | Partial install; zero success-marker count | 13 | 15.301 GiB | +4.971 GiB | 22 |
| `2026-06-10_06-07-40` | Not installed; control run | 43 | 17.489 GiB | +8.261 GiB | 5,857 |
| `2026-06-11_04-39-43` | Full patch install; startup sweep exception found | 15 | 11.375 GiB | +2.925 GiB | 1 |
| `2026-06-11_06-18-43` | Full patch install; vessel-list guard verified | 23 | 12.823 GiB | +2.724 GiB | 1 |
| `2026-06-11_22-40-51` | Full patch install; event-registry race found | 52 | 15.918 GiB | +5.945 GiB | 1 |
| `2026-06-12_02-38-13` | Full patch install; inventory deletion verified | 35 | 18.495 GiB | +7.152 GiB | 1 |
| `2026-06-12_05-20-57` | Full patch install; editor-heavy deployed-science follow-up | 32 | 16.450 GiB | +5.447 GiB | 1 |
| `2026-06-13_00-09-06` | Full patch install; short flight-focused follow-up | 7 | 12.270 GiB | +1.893 GiB | 1 |
| `2026-06-13_03-30-47` | Full patch install; long Mun rover flight and reload stress | 19 | 17.108 GiB | +6.168 GiB | 1 |

The older partial-install exports show that proactive cleanup was running and
substantially reduced covered stock residue, but they cannot establish the
behavior of the complete `1.7.0` patch set. Memory also continued to grow in
some sessions, so callback cleanup is not the only source of KSP process-memory
growth.

The June 11 run is the first export with a non-zero Harmony success marker. It
exercised the editor-exit sweep and the then-recursive child-part lifecycle
cleanup.
KSPCommunityFixes' handled summary contained only one
`RealAntennas.RACommNetVessel.onPlanetariumTargetChange` callback and no covered
stock callback owners. The run also exposed startup and scene-unload sweep
exceptions from accessing `FlightGlobals.Vessels` before flight globals
existed; the post-run singleton guard fixes those paths. Inventory
`DeletePartObject` cleanup was not exercised.

The following June 11 runs verified that the `FlightGlobals` singleton guard
prevents the earlier vessel-list exceptions. The longer heavy-rover build and
test session ran for more than seven hours across 52 scene exits and again left
only the intentionally unpatched RealAntennas callback in KSPCommunityFixes'
handled summary. It exposed one broad-sweep exception when KSP changed the
GameEvents registry during enumeration; the post-run registry snapshot and
deferred-retry behavior fixes that race. Inventory `DeletePartObject` cleanup
still was not exercised.

The June 12 mixed rover, base, station, and deployed-science session exercised
`ModuleInventoryPart.DeletePartObject` 29 times across control stations,
experiments, antennas, power parts, and other inventory-created objects. It
also exercised ground-science termination and recovery cleanup. No covered
stock callback owners remained in KSPCommunityFixes' handled summary, and no
NoMoreLeaks exceptions occurred across 35 scene exits. The session contained
no deployed-science missing-vessel warnings. All 29 inventory deletion objects
were standalone parts, so child-hierarchy cleanup through `DeletePartObject`
itself remains untested. The same run logged 296 child-bearing
`Part.OnDestroy` cleanups and showed that child parts receive their own
lifecycle cleanup, supporting the final decision to limit `Part.OnDestroy` to
the part actually being destroyed.

The later June 12 follow-up was more editor-heavy and repeatedly exercised
ground-science, cargo, and editor destroy-selected cleanup, but did not hit
`ModuleInventoryPart.DeletePartObject`. It again left only the intentionally
unpatched RealAntennas callback in KSPCommunityFixes' handled summary. KSPCF's
per-scene `cleaned N` totals were still large, especially in Editor and Flight,
which is consistent with repeated counts of unhandled third-party residue and
broader scene-cleanup accounting rather than a return of covered stock callback
owners. This was the first run after limiting `Part.OnDestroy` cleanup to the
part actually being destroyed; 32 scene exits, five proactive editor-exit
sweeps, and 19 destroy-selected cleanups completed without covered stock
residue. Its final GameEvents callback count remained close to the prior run
(`1,446` versus `1,480`). This run also reintroduced deployed-science
missing-vessel warnings, supporting the conclusion that they are intermittent
save-state noise rather than a NoMoreLeaks callback target.

The June 13 short follow-up covered only seven scene exits and appears to be
mostly flight-focused. It again left only the intentionally unpatched
RealAntennas callback in KSPCommunityFixes' handled summary, reported no
NoMoreLeaks warnings or exceptions, and its broad stock sweep cleaned destroyed
`AudioFX` pause and unpause callback owners. Its smaller total allocation
increase reflects the short run rather than demonstrated memory improvement:
normalized by scene exits, its increase was about `0.270 GiB` per exit,
compared with about `0.170 GiB` and `0.204 GiB` in the longer June 12 runs.
Managed memory fell from a `5.840 GiB` peak to `4.739 GiB` at the final exit
and unmanaged memory stayed relatively flat, but the run is too short to
establish a trend. Its final GameEvents callback count of `3,397` is elevated
and should be watched in a longer comparable flight run. It did not exercise
the editor-exit proactive sweep or inventory-deletion cleanup paths.

Those two post-release follow-ups add 39 scene exits with no NoMoreLeaks
exceptions and no covered stock callback residue. They provide a useful
no-regression check for the safer `Part.OnDestroy` behavior, but do not yet
demonstrate lower long-session memory growth.

The later June 13 run covered a roughly three-hour Mun rover trek involving
docking and resource transfer, repeated crashes, save/load recovery, and cheat
menu repositioning. Eleven of its 19 scene exits were from Flight, and it
logged 142 child-bearing lifecycle cleanups. It again left only the
intentionally unpatched RealAntennas callback in KSPCommunityFixes' handled
summary and produced no NoMoreLeaks exceptions. This provides a strong
flight-teardown and reload-stress no-regression check, but did not exercise
editor or inventory `DeletePartObject` paths.

Allocation increased from `10.940 GiB` at the first scene exit to
`17.108 GiB` at the final exit, about `0.325 GiB` per exit. Most of the increase
was managed memory; unmanaged memory peaked at `9.388 GiB` and ended at
`8.975 GiB`. GameEvents callbacks stayed between `1,460` and `1,676` across
most Flight exits before ending at `3,286`. Because KSPCommunityFixes found no
covered destroyed callback owners at that final exit, the elevated count does
not identify a new NoMoreLeaks target, but remains worth monitoring in another
comparable long-flight run. The session therefore reinforces cleanup stability
without demonstrating lower process-memory growth.

The full log also contains 201 `MissingFieldException` entries from
`SCANmechjeb` looking for `MuMech.MechJebCore.target`. This indicates a binary
compatibility mismatch between the installed SCANsat/SCANmechjeb and MechJeb
versions. It is outside NoMoreLeaks' scope, but the repeated exceptions can add
noise and stutter around vessel modification and vessel-change events.

### Known Unhandled Third-Party Residue

These callback owners recur in the unhandled summaries and remain outside the
current NoMoreLeaks stock-cleanup scope:

- `PlanetsideExplorationTechnologies.OnGameSettingsApplied`
- `NearFutureElectrical.ReactorUI.onGUIApplicationLauncherDestroyed`
- `NearFutureElectrical.DischargeCapacitorUI.onGUIApplicationLauncherDestroyed`
- `Scale.PartDB.19x.GameEventEditorVariantAppliedListener.onEditorVariantApplied`
- `PartInfo.PartInfoWindow.onEditorPartEvent`

The Planetside entry above is not the compatibility cleanup listed under Current
Patch Targets. NoMoreLeaks cleans the separate Kerbal Planetary Base Systems
`ModuleKPBSCorridorNodes.onEditorShipModified` callback, but does not currently
clean `PlanetsideExplorationTechnologies.OnGameSettingsApplied`.

Earlier exports also contain repeated deployed-science missing-vessel warnings.
They were absent in `2026-06-12_02-38-13` but reappeared in
`2026-06-12_05-20-57`, so treat them as intermittent save-state noise rather
than a NoMoreLeaks callback target.

## Current State

As of `1.7.0`:

- Long-flight `OverlayGenerator`, `MapView`, and `NavBallToggle` leaks are much
  less prominent.
- `SpaceTracking`, `InternalNavBall`, `BuildingPickerItem`, and
  `EVAConstructionModeEditor` have been substantially reduced.
- `ModuleCargoPart | OnEVAConstructionMode` is usually caught proactively.
- `ModuleInventoryPart` has fallen from large repeated callback sets to small
  residual counts in recent validation sessions. Version `1.7.0` adds cleanup
  around inventory-created part creation and deletion paths.
- Explicit subtree-deletion cleanup now covers child-part hierarchies.
- Harmony patch helpers no longer use the reserved `Cleanup` method name that
  could abort patch installation during startup.
- The June 11 full-install run left no covered stock callbacks in KSPCF's
  handled summary.
- The two later June 11 runs verified the `FlightGlobals` singleton guard
  prevents the vessel-list exception.
- The June 12 follow-up run completed 35 scene exits without another GameEvents
  registry-enumeration exception.
- The three later June 12 and June 13 follow-up runs added 58 scene exits with no
  covered stock callbacks in KSPCF's handled summary or NoMoreLeaks exceptions,
  validating the narrowed `Part.OnDestroy` behavior without showing a
  long-session memory-growth improvement yet.
- Inventory placement cancellation and deployed-part teardown now validate the
  new `DeletePartObject` cleanup path.

`CommNetScenario: Instance already exists!` can appear immediately after
RealAntennas rebuilds its CommNet homes. RealAntennas logs that this specific
stock error should be ignored. It is not evidence of a leaked callback and does
not require NoMoreLeaks cleanup.

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
