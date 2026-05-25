using HarmonyLib;

namespace NoMoreLeaks.Patches
{
    [HarmonyPatch(typeof(MapView), "OnDestroy")]
    internal static class MapViewLeakPatch
    {
        private static void Prefix(MapView __instance)
        {
            Cleanup(__instance);
        }

        private static void Postfix(MapView __instance)
        {
            Cleanup(__instance);
        }

        private static void Cleanup(MapView instance)
        {
            object timingManager = EventCleanup.GetStaticMember(typeof(TimingManager), "Instance");
            object timing5 = EventCleanup.GetInstanceField(timingManager, "timing5");
            EventCleanup.RemoveDelegatesOwnedBy(timing5, "onLateUpdate", instance);
        }
    }
}
