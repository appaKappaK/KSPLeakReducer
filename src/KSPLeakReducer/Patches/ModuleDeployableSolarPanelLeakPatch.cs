using HarmonyLib;

namespace KSPLeakReducer.Patches
{
    [HarmonyPatch(typeof(ModuleDeployableSolarPanel), "OnDestroy")]
    internal static class ModuleDeployableSolarPanelLeakPatch
    {
        private static void Prefix(ModuleDeployableSolarPanel __instance)
        {
            CleanupCallbacks(__instance);
        }

        private static void Postfix(ModuleDeployableSolarPanel __instance)
        {
            CleanupCallbacks(__instance);
        }

        private static void CleanupCallbacks(ModuleDeployableSolarPanel instance)
        {
            EventCleanup.RemoveGameEvent(GameEvents.onVesselChange, instance, "onVesselFocusChange");
        }
    }
}
