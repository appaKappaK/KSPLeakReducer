using HarmonyLib;

namespace NoMoreLeaks.Patches
{
    [HarmonyPatch(typeof(KerbalEVA), "OnDestroy")]
    internal static class KerbalEVALeakPatch
    {
        private static void Prefix(KerbalEVA __instance)
        {
            CleanupCallbacks(__instance);
        }

        private static void Postfix(KerbalEVA __instance)
        {
            CleanupCallbacks(__instance);
        }

        private static void CleanupCallbacks(KerbalEVA instance)
        {
            EventCleanup.RemoveGameEvent(GameEvents.OnROCExperimentStored, instance, "OnROCExperimentFinished");
            EventCleanup.RemoveGameEvent(GameEvents.OnROCExperimentReset, instance, "OnROCExperimentReset");
        }
    }
}
