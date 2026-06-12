using KSP.UI.Screens;
using HarmonyLib;

namespace NoMoreLeaks.Patches
{
    [HarmonyPatch(typeof(SpaceTracking), "OnDestroy")]
    internal static class SpaceTrackingLeakPatch
    {
        private static void Prefix(SpaceTracking __instance)
        {
            CleanupCallbacks(__instance);
        }

        private static void Postfix(SpaceTracking __instance)
        {
            CleanupCallbacks(__instance);
        }

        private static void CleanupCallbacks(SpaceTracking instance)
        {
            EventCleanup.RemoveOwner(GameEvents.OnMapViewFiltersModified, instance);
            EventCleanup.RemoveOwner(GameEvents.onInputLocksModified, instance);
            EventCleanup.RemoveOwner(GameEvents.onGUIRecoveryDialogSpawn, instance);
            EventCleanup.RemoveOwner(GameEvents.onGUIRecoveryDialogDespawn, instance);
            EventCleanup.RemoveOwner(GameEvents.onPlanetariumTargetChanged, instance);

            var vessels = FlightGlobals.fetch == null ? null : FlightGlobals.fetch.vessels;
            if (vessels == null) return;

            for (int i = 0; i < vessels.Count; i++)
            {
                Vessel vessel = vessels[i];
                if (vessel == null || vessel.orbitRenderer == null) continue;

                EventCleanup.RemoveDelegatesOwnedBy(vessel.orbitRenderer, "onVesselIconClicked", instance);
            }
        }
    }
}
