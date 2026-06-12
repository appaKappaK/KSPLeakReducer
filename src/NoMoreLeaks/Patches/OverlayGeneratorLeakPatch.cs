using HarmonyLib;

namespace NoMoreLeaks.Patches
{
    [HarmonyPatch(typeof(OverlayGenerator), "OnDestroy")]
    internal static class OverlayGeneratorLeakPatch
    {
        private static void Prefix(OverlayGenerator __instance)
        {
            CleanupCallbacks(__instance);
        }

        private static void Postfix(OverlayGenerator __instance)
        {
            CleanupCallbacks(__instance);
        }

        private static void CleanupCallbacks(OverlayGenerator instance)
        {
            EventCleanup.RemoveOwner(GameEvents.onGameStateLoad, instance);
            EventCleanup.RemoveOwner(GameEvents.onPlanetariumTargetChanged, instance);
            EventCleanup.RemoveGameEvent(GameEvents.onGameStateLoad, instance, "OnGameLoaded");
            EventCleanup.RemoveGameEvent(GameEvents.onPlanetariumTargetChanged, instance, "OnMapFocusChange");
            EventCleanup.RemoveStaticDelegateField(typeof(MapView), "OnEnterMapView", instance, "OnEnterMapView");
            EventCleanup.RemoveStaticDelegateField(typeof(MapView), "OnExitMapView", instance, "OnExitMapView");
        }
    }
}
