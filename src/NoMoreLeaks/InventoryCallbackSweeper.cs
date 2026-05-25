using UnityEngine;

namespace NoMoreLeaks
{
    internal static class InventoryCallbackSweeper
    {
        internal static void Sweep()
        {
            int removed = 0;
            removed += EventCleanup.RemoveDestroyedStockGameEventOwners();
            removed += SweepInventoryCallbacks();
            removed += SweepMapUiCallbacks();
            removed += EventCleanup.RemoveDestroyedOwners(GameEvents.onVesselChange, typeof(Expansions.Serenity.ModuleRobotArmScanner));
            removed += EventCleanup.RemoveDestroyedOwnersByTypeName(GameEvents.onEditorShipModified, "PlanetarySurfaceStructures.ModuleKPBSCorridorNodes");
            removed += RemoveDestroyedSpaceTrackingCallbacks();

            if (removed > 0)
                Debug.Log("[NoMoreLeaks] Removed " + removed + " destroyed callback owners");
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

            object timingManager = EventCleanup.GetStaticMember(typeof(TimingManager), "Instance");
            object timing5 = EventCleanup.GetInstanceField(timingManager, "timing5");
            removed += EventCleanup.RemoveDestroyedDelegateMemberOwners(timing5, "onLateUpdate");

            return removed;
        }

        private static int RemoveDestroyedSpaceTrackingCallbacks()
        {
            if (FlightGlobals.Vessels == null) return 0;

            int removed = 0;
            for (int i = 0; i < FlightGlobals.Vessels.Count; i++)
            {
                Vessel vessel = FlightGlobals.Vessels[i];
                if (vessel == null || vessel.orbitRenderer == null) continue;

                removed += EventCleanup.RemoveDestroyedDelegateMemberOwners(vessel.orbitRenderer, "onVesselIconClicked");
            }

            return removed;
        }
    }
}
