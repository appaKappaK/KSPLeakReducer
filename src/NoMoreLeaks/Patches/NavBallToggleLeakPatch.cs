using HarmonyLib;
using KSP.UI.Screens.Flight;

namespace NoMoreLeaks.Patches
{
    [HarmonyPatch(typeof(NavBallToggle), "OnDestroy")]
    internal static class NavBallToggleLeakPatch
    {
        private static void Prefix(NavBallToggle __instance)
        {
            Cleanup(__instance);
        }

        private static void Postfix(NavBallToggle __instance)
        {
            Cleanup(__instance);
        }

        private static void Cleanup(NavBallToggle instance)
        {
            EventCleanup.RemoveOwner(GameEvents.OnMapExited, instance);
            EventCleanup.RemoveGameEvent(GameEvents.OnMapExited, instance, "OnMapExited");
        }
    }
}
