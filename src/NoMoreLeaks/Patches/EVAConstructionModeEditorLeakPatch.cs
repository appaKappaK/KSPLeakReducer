using HarmonyLib;

namespace NoMoreLeaks.Patches
{
    [HarmonyPatch(typeof(EVAConstructionModeEditor), "OnDestroy")]
    internal static class EVAConstructionModeEditorLeakPatch
    {
        private static void Prefix(EVAConstructionModeEditor __instance)
        {
            CleanupCallbacks(__instance);
        }

        private static void Postfix(EVAConstructionModeEditor __instance)
        {
            CleanupCallbacks(__instance);
        }

        private static void CleanupCallbacks(EVAConstructionModeEditor instance)
        {
            object angleSnapButton = EventCleanup.GetInstanceField(instance, "angleSnapButton");
            EventCleanup.RemoveDelegatesOwnedBy(angleSnapButton, "onClick", instance);
            EventCleanup.RemoveInstanceEventField(angleSnapButton, "onClick", instance, "SnapButton");
        }
    }
}
