using HarmonyLib;

namespace NoMoreLeaks.Patches
{
    [HarmonyPatch(typeof(ModuleInventoryPart))]
    internal static class ModuleInventoryPartLeakPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnStart")]
        private static void OnStartPrefix(ModuleInventoryPart __instance)
        {
            if (HighLogic.LoadedSceneIsEditor)
                InventoryCallbackSweeper.SweepEditorInventoryCallbacks();

            CleanupCallbacks(__instance);
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnDestroy")]
        private static void OnDestroyPrefix(ModuleInventoryPart __instance)
        {
            CleanupCallbacks(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnDestroy")]
        private static void OnDestroyPostfix(ModuleInventoryPart __instance)
        {
            CleanupCallbacks(__instance);
        }

        internal static void CleanupCallbacks(ModuleInventoryPart instance)
        {
            EventCleanup.RemoveOwner(GameEvents.onPartActionUICreate, instance);
            EventCleanup.RemoveOwner(GameEvents.onModuleInventoryChanged, instance);
            EventCleanup.RemoveOwner(GameEvents.onEditorPartEvent, instance);
            EventCleanup.RemoveOwner(GameEvents.OnPartPurchased, instance);
            EventCleanup.RemoveOwner(GameEvents.OnInventoryPartOnMouseChanged, instance);

            EventCleanup.RemoveGameEvent(GameEvents.onPartActionUICreate, instance, "onPartActionUIOpened");
            EventCleanup.RemoveGameEvent(GameEvents.onModuleInventoryChanged, instance, "OnModuleInventoryChanged");
            EventCleanup.RemoveGameEvent(GameEvents.onEditorPartEvent, instance, "OnEditorPartEvent");
            EventCleanup.RemoveGameEvent(GameEvents.OnPartPurchased, instance, "OnPartPurchased");
            EventCleanup.RemoveGameEvent(GameEvents.OnInventoryPartOnMouseChanged, instance, "VesselEditorPartHighlighter");
        }
    }
}
