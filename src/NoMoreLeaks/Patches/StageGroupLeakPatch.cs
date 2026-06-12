using HarmonyLib;
using KSP.UI.Screens;

namespace NoMoreLeaks.Patches
{
    [HarmonyPatch(typeof(StageGroup), "OnDestroy")]
    internal static class StageGroupLeakPatch
    {
        private static void Prefix(StageGroup __instance)
        {
            CleanupCallbacks(__instance);
        }

        private static void Postfix(StageGroup __instance)
        {
            CleanupCallbacks(__instance);
        }

        private static void CleanupCallbacks(StageGroup instance)
        {
            EventCleanup.RemoveOwner(GameEvents.onDeltaVAppAtmosphereChanged, instance);
            EventCleanup.RemoveOwner(GameEvents.onDeltaVAppInfoItemsChanged, instance);
            EventCleanup.RemoveGameEvent(GameEvents.onDeltaVAppAtmosphereChanged, instance, "DeltaVAppAtmosphereChanged");
            EventCleanup.RemoveGameEvent(GameEvents.onDeltaVAppInfoItemsChanged, instance, "DeltaVCalcsCompleted");
        }
    }
}
