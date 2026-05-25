using KSP.UI.Screens;
using HarmonyLib;

namespace NoMoreLeaks.Patches
{
    [HarmonyPatch(typeof(SpaceTracking), "OnDestroy")]
    internal static class SpaceTrackingLeakPatch
    {
        private static void Prefix(SpaceTracking __instance)
        {
            Cleanup(__instance);
        }

        private static void Postfix(SpaceTracking __instance)
        {
            Cleanup(__instance);
        }

        private static void Cleanup(SpaceTracking instance)
        {
            EventCleanup.RemoveOwner(GameEvents.OnMapViewFiltersModified, instance);
            EventCleanup.RemoveOwner(GameEvents.onInputLocksModified, instance);
            EventCleanup.RemoveOwner(GameEvents.onGUIRecoveryDialogSpawn, instance);
            EventCleanup.RemoveOwner(GameEvents.onGUIRecoveryDialogDespawn, instance);
            EventCleanup.RemoveOwner(GameEvents.onPlanetariumTargetChanged, instance);

            if (FlightGlobals.Vessels == null) return;

            for (int i = 0; i < FlightGlobals.Vessels.Count; i++)
            {
                Vessel vessel = FlightGlobals.Vessels[i];
                if (vessel == null || vessel.orbitRenderer == null) continue;

                EventCleanup.RemoveDelegatesOwnedBy(vessel.orbitRenderer, "onVesselIconClicked", instance);
            }
        }
    }
}
