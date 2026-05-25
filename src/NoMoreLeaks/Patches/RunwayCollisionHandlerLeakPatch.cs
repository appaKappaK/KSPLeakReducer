using HarmonyLib;

namespace NoMoreLeaks.Patches
{
    [HarmonyPatch(typeof(RunwayCollisionHandler), "OnDestroy")]
    internal static class RunwayCollisionHandlerLeakPatch
    {
        private static void Prefix(RunwayCollisionHandler __instance)
        {
            Cleanup(__instance);
        }

        private static void Postfix(RunwayCollisionHandler __instance)
        {
            Cleanup(__instance);
        }

        private static void Cleanup(RunwayCollisionHandler instance)
        {
            EventCleanup.RemoveOwner(DestructibleBuilding.OnLoaded, instance);
            EventCleanup.RemoveGameEvent(DestructibleBuilding.OnLoaded, instance, "OnSectionLoaded");
        }
    }
}
