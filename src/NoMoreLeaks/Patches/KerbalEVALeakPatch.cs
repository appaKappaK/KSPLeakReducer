using HarmonyLib;

namespace NoMoreLeaks.Patches
{
    [HarmonyPatch(typeof(KerbalEVA), "OnDestroy")]
    internal static class KerbalEVALeakPatch
    {
        private static void Prefix(KerbalEVA __instance)
        {
            Cleanup(__instance);
        }

        private static void Postfix(KerbalEVA __instance)
        {
            Cleanup(__instance);
        }

        private static void Cleanup(KerbalEVA instance)
        {
            EventCleanup.RemoveGameEvent(GameEvents.OnROCExperimentStored, instance, "OnROCExperimentFinished");
            EventCleanup.RemoveGameEvent(GameEvents.OnROCExperimentReset, instance, "OnROCExperimentReset");
        }
    }
}
