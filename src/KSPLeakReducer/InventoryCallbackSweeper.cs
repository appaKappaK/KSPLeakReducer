using UnityEngine;

namespace KSPLeakReducer
{
    internal static class InventoryCallbackSweeper
    {
        internal static void Sweep()
        {
            int removed = 0;
            removed += SweepCommonCallbacks();
            LogSweep("Removed", removed);
        }

        internal static void SweepSceneUnload()
        {
            int removed = 0;
            removed += SweepCommonCallbacks();
            removed += SweepTimingManagerCallbacks();
            LogSweep("Scene-unload removed", removed);
        }

        internal static int SweepInventoryCallbacks()
        {
            int removed = 0;
            removed += EventCleanup.RemoveDestroyedOwners(GameEvents.onPartActionUICreate, typeof(ModuleInventoryPart));
            removed += EventCleanup.RemoveDestroyedOwners(GameEvents.onModuleInventoryChanged, typeof(ModuleInventoryPart));
            removed += EventCleanup.RemoveDestroyedOwners(GameEvents.onEditorPartEvent, typeof(ModuleInventoryPart));
            removed += EventCleanup.RemoveDestroyedOwners(GameEvents.OnPartPurchased, typeof(ModuleInventoryPart));
            removed += EventCleanup.RemoveDestroyedOwners(GameEvents.OnInventoryPartOnMouseChanged, typeof(ModuleInventoryPart));
            removed += EventCleanup.RemoveDestroyedOwners(GameEvents.OnEVACargoMode, typeof(UIPartActionInventorySlot));
            removed += EventCleanup.RemoveDestroyedOwners(GameEvents.onEditorPartDeleted, typeof(UIPartActionInventorySlot));
            return removed;
        }

        internal static int SweepEditorInventoryCallbacks()
        {
            return SweepInventoryCallbacks();
        }

        internal static int SweepMapUiCallbacks()
        {
            int removed = 0;
            removed += SweepAutopilotCallbacks();
            removed += EventCleanup.RemoveDestroyedOwners(GameEvents.onGameStateLoad, typeof(OverlayGenerator));
            removed += EventCleanup.RemoveDestroyedOwners(GameEvents.onPlanetariumTargetChanged, typeof(OverlayGenerator));
            removed += EventCleanup.RemoveDestroyedOwners(GameEvents.OnMapExited, typeof(KSP.UI.Screens.Flight.NavBallToggle));
            removed += EventCleanup.RemoveDestroyedOwners(GameEvents.onVesselChange, typeof(InternalNavBall));
            removed += EventCleanup.RemoveDestroyedOwners(GameEvents.OnMapViewFiltersModified, typeof(KSP.UI.Screens.SpaceTracking));
            removed += EventCleanup.RemoveDestroyedOwners(GameEvents.onInputLocksModified, typeof(KSP.UI.Screens.SpaceTracking));
            removed += EventCleanup.RemoveDestroyedOwners(GameEvents.onGUIRecoveryDialogSpawn, typeof(KSP.UI.Screens.SpaceTracking));
            removed += EventCleanup.RemoveDestroyedOwners(GameEvents.onGUIRecoveryDialogDespawn, typeof(KSP.UI.Screens.SpaceTracking));
            removed += EventCleanup.RemoveDestroyedOwners(GameEvents.onPlanetariumTargetChanged, typeof(KSP.UI.Screens.SpaceTracking));
            removed += EventCleanup.RemoveDestroyedStaticDelegateOwners(typeof(MapView), "OnEnterMapView", typeof(OverlayGenerator));
            removed += EventCleanup.RemoveDestroyedStaticDelegateOwners(typeof(MapView), "OnExitMapView", typeof(OverlayGenerator));

            return removed;
        }

        internal static int SweepAutopilotCallbacks()
        {
            int removed = 0;
            removed += EventCleanup.RemoveDestroyedOwners(GameEvents.OnGameSettingsApplied, typeof(VesselAutopilotUI));
            removed += EventCleanup.RemoveDestroyedOwners(GameEvents.onVesselChange, typeof(VesselAutopilotUI));
            removed += EventCleanup.RemoveDestroyedOwners(GameEvents.onKerbalLevelUp, typeof(VesselAutopilotUI));
            return removed;
        }

        private static int RemoveDestroyedSpaceTrackingCallbacks()
        {
            var vessels = FlightGlobals.fetch == null ? null : FlightGlobals.fetch.vessels;
            if (vessels == null) return 0;

            int removed = 0;
            for (int i = 0; i < vessels.Count; i++)
            {
                Vessel vessel = vessels[i];
                if (vessel == null || vessel.orbitRenderer == null) continue;

                removed += EventCleanup.RemoveDestroyedDelegateMemberOwners(vessel.orbitRenderer, "onVesselIconClicked");
            }

            return removed;
        }

        private static int SweepCommonCallbacks()
        {
            int removed = 0;
            removed += EventCleanup.RemoveDestroyedStockGameEventOwners();
            removed += SweepInventoryCallbacks();
            removed += SweepMapUiCallbacks();
            removed += EventCleanup.RemoveDestroyedOwners(GameEvents.onVesselChange, typeof(Expansions.Serenity.ModuleRobotArmScanner));
            removed += EventCleanup.RemoveDestroyedOwnersByTypeName(GameEvents.onEditorShipModified, "PlanetarySurfaceStructures.ModuleKPBSCorridorNodes");
            removed += RemoveDestroyedSpaceTrackingCallbacks();
            return removed;
        }

        private static int SweepTimingManagerCallbacks()
        {
            object timingManager = EventCleanup.GetStaticMember(typeof(TimingManager), "Instance");
            if (timingManager == null) return 0;

            int removed = 0;
            removed += EventCleanup.RemoveDestroyedDelegateMembers(timingManager, "TimingManager.Instance");
            removed += RemoveTimingBucketCallbacks(timingManager, "timing0");
            removed += RemoveTimingBucketCallbacks(timingManager, "timing1");
            removed += RemoveTimingBucketCallbacks(timingManager, "timing2");
            removed += RemoveTimingBucketCallbacks(timingManager, "timing3");
            removed += RemoveTimingBucketCallbacks(timingManager, "timing4");
            removed += RemoveTimingBucketCallbacks(timingManager, "timing5");
            removed += RemoveTimingBucketCallbacks(timingManager, "timingPre");
            removed += RemoveTimingBucketCallbacks(timingManager, "timingFI");
            return removed;
        }

        private static int RemoveTimingBucketCallbacks(object timingManager, string fieldName)
        {
            object timingBucket = EventCleanup.GetInstanceField(timingManager, fieldName);
            return timingBucket != null
                ? EventCleanup.RemoveDestroyedDelegateMembers(timingBucket, "TimingManager.Instance." + fieldName)
                : 0;
        }

        private static void LogSweep(string action, int removed)
        {
            if (removed > 0)
                Debug.Log("[KSPLeakReducer] " + action + " " + removed + " destroyed callback owners");
        }
    }
}
