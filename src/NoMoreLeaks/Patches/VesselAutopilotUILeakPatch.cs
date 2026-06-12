using HarmonyLib;

namespace NoMoreLeaks.Patches
{
    [HarmonyPatch(typeof(VesselAutopilotUI))]
    internal static class VesselAutopilotUILeakPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Start")]
        private static void StartPrefix()
        {
            EventCleanup.RemoveDestroyedOwners(GameEvents.OnGameSettingsApplied, typeof(VesselAutopilotUI));
            EventCleanup.RemoveDestroyedOwners(GameEvents.onVesselChange, typeof(VesselAutopilotUI));
            EventCleanup.RemoveDestroyedOwners(GameEvents.onKerbalLevelUp, typeof(VesselAutopilotUI));
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnDestroy")]
        private static void OnDestroyPrefix(VesselAutopilotUI __instance)
        {
            CleanupCallbacks(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnDestroy")]
        private static void OnDestroyPostfix(VesselAutopilotUI __instance)
        {
            CleanupCallbacks(__instance);
        }

        private static void CleanupCallbacks(VesselAutopilotUI instance)
        {
            EventCleanup.RemoveOwner(GameEvents.OnGameSettingsApplied, instance);
            EventCleanup.RemoveOwner(GameEvents.onVesselChange, instance);
            EventCleanup.RemoveOwner(GameEvents.onKerbalLevelUp, instance);
            EventCleanup.RemoveGameEvent(GameEvents.OnGameSettingsApplied, instance, "onGameParametersChanged");
            EventCleanup.RemoveGameEvent(GameEvents.onVesselChange, instance, "OnVesselChange");
            EventCleanup.RemoveGameEvent(GameEvents.onKerbalLevelUp, instance, "OnKerbalLevelUp");
        }
    }
}
