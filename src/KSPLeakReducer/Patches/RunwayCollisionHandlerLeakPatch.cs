using HarmonyLib;

namespace KSPLeakReducer.Patches
{
    [HarmonyPatch(typeof(RunwayCollisionHandler), "OnDestroy")]
    internal static class RunwayCollisionHandlerLeakPatch
    {
        private static void Prefix(RunwayCollisionHandler __instance)
        {
            CleanupCallbacks(__instance);
        }

        private static void Postfix(RunwayCollisionHandler __instance)
        {
            CleanupCallbacks(__instance);
        }

        private static void CleanupCallbacks(RunwayCollisionHandler instance)
        {
            EventCleanup.RemoveOwner(DestructibleBuilding.OnLoaded, instance);
            EventCleanup.RemoveGameEvent(DestructibleBuilding.OnLoaded, instance, "OnSectionLoaded");
        }
    }
}
