using HarmonyLib;

namespace NoMoreLeaks.Patches
{
    [HarmonyPatch(typeof(InternalNavBall), "OnDestroy")]
    internal static class InternalNavBallLeakPatch
    {
        private static void Prefix(InternalNavBall __instance)
        {
            CleanupCallbacks(__instance);
        }

        private static void Postfix(InternalNavBall __instance)
        {
            CleanupCallbacks(__instance);
        }

        private static void CleanupCallbacks(InternalNavBall instance)
        {
            EventCleanup.RemoveOwner(GameEvents.onVesselChange, instance);
        }
    }
}
