using HarmonyLib;

namespace NoMoreLeaks.Patches
{
    [HarmonyPatch(typeof(InternalNavBall), "OnDestroy")]
    internal static class InternalNavBallLeakPatch
    {
        private static void Prefix(InternalNavBall __instance)
        {
            Cleanup(__instance);
        }

        private static void Postfix(InternalNavBall __instance)
        {
            Cleanup(__instance);
        }

        private static void Cleanup(InternalNavBall instance)
        {
            EventCleanup.RemoveOwner(GameEvents.onVesselChange, instance);
        }
    }
}
