using CommNet;
using HarmonyLib;

namespace KSPLeakReducer.Patches
{
    [HarmonyPatch(typeof(CommNetVessel), "OnDestroy")]
    internal static class CommNetVesselLeakPatch
    {
        private static void Prefix(CommNetVessel __instance)
        {
            CleanupCallbacks(__instance);
        }

        private static void Postfix(CommNetVessel __instance)
        {
            CleanupCallbacks(__instance);
        }

        private static void CleanupCallbacks(CommNetVessel instance)
        {
            EventCleanup.RemoveOwner(GameEvents.CommNet.OnNetworkInitialized, instance);
            EventCleanup.RemoveGameEvent(GameEvents.CommNet.OnNetworkInitialized, instance, "OnNetworkInitialized");
            EventCleanup.RemoveOwner(GameEvents.onPlanetariumTargetChanged, instance);
            EventCleanup.RemoveGameEvent(GameEvents.onPlanetariumTargetChanged, instance, "OnMapFocusChange");
        }
    }
}
