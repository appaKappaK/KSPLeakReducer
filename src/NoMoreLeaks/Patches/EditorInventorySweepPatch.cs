using HarmonyLib;

namespace NoMoreLeaks.Patches
{
    [HarmonyPatch(typeof(EditorLogic), "SetBackup")]
    internal static class EditorSetBackupInventorySweepPatch
    {
        private static void Postfix()
        {
            if (!HighLogic.LoadedSceneIsEditor) return;
            InventoryCallbackSweeper.Sweep();
        }
    }

    [HarmonyPatch(typeof(EditorLogic), "DeletePart")]
    internal static class EditorDeletePartInventorySweepPatch
    {
        private static void Prefix(Part part)
        {
            if (part == null) return;

            PartModuleLifecycleLeakPatch.CleanupPartModules(part);

            if (part.children == null) return;
            for (int i = 0; i < part.children.Count; i++)
                PartModuleLifecycleLeakPatch.CleanupPartModules(part.children[i]);
        }

        private static void Postfix()
        {
            if (!HighLogic.LoadedSceneIsEditor) return;
            InventoryCallbackSweeper.Sweep();
        }
    }

    [HarmonyPatch(typeof(EditorLogic), "DestroySelectedPart")]
    internal static class EditorDestroySelectedPartInventorySweepPatch
    {
        private static void Prefix()
        {
            if (EditorLogic.SelectedPart == null) return;

            Part selectedPart = EditorLogic.SelectedPart;
            PartModuleLifecycleLeakPatch.CleanupPartModules(selectedPart);

            if (selectedPart.children == null) return;
            for (int i = 0; i < selectedPart.children.Count; i++)
                PartModuleLifecycleLeakPatch.CleanupPartModules(selectedPart.children[i]);
        }

        private static void Postfix()
        {
            if (!HighLogic.LoadedSceneIsEditor) return;
            InventoryCallbackSweeper.Sweep();
        }
    }
}
