using HarmonyLib;

namespace KSPLeakReducer.Patches
{
    [HarmonyPatch(typeof(EditorLogic), "SetBackup")]
    internal static class EditorSetBackupInventorySweepPatch
    {
        private static void Postfix()
        {
            if (!HighLogic.LoadedSceneIsEditor) return;
            KSPLeakReducerSettings.LogDebug("Editor sweep via EditorLogic.SetBackup.Postfix");
            InventoryCallbackSweeper.Sweep();
        }
    }

    [HarmonyPatch(typeof(EditorLogic), "DeletePart")]
    internal static class EditorDeletePartInventorySweepPatch
    {
        private static void Prefix(Part part)
        {
            if (part == null) return;

            KSPLeakReducerSettings.LogDebug("Editor delete requested for " + DescribePart(part));
            PartModuleLifecycleLeakPatch.CleanupPartModules(part, "EditorLogic.DeletePart.Prefix");
        }

        private static void Postfix()
        {
            if (!HighLogic.LoadedSceneIsEditor) return;
            KSPLeakReducerSettings.LogDebug("Editor sweep via EditorLogic.DeletePart.Postfix");
            InventoryCallbackSweeper.Sweep();
        }

        private static string DescribePart(Part part)
        {
            if (part == null) return "<null>";

            string partName = !string.IsNullOrEmpty(part.partInfo?.title)
                ? part.partInfo.title
                : !string.IsNullOrEmpty(part.partName)
                    ? part.partName
                    : part.name;
            int childCount = part.children != null ? part.children.Count : 0;
            return partName + " (children=" + childCount + ", hasParent=" + (part.parent != null) + ")";
        }
    }

    [HarmonyPatch(typeof(EditorLogic), "DestroySelectedPart")]
    internal static class EditorDestroySelectedPartInventorySweepPatch
    {
        private static void Prefix()
        {
            if (EditorLogic.SelectedPart == null) return;

            Part selectedPart = EditorLogic.SelectedPart;
            KSPLeakReducerSettings.LogDebug("Editor destroy-selected requested for " + DescribePart(selectedPart));
            PartModuleLifecycleLeakPatch.CleanupPartModules(selectedPart, "EditorLogic.DestroySelectedPart.Prefix");
        }

        private static void Postfix()
        {
            if (!HighLogic.LoadedSceneIsEditor) return;
            KSPLeakReducerSettings.LogDebug("Editor sweep via EditorLogic.DestroySelectedPart.Postfix");
            InventoryCallbackSweeper.Sweep();
        }

        private static string DescribePart(Part part)
        {
            if (part == null) return "<null>";

            string partName = !string.IsNullOrEmpty(part.partInfo?.title)
                ? part.partInfo.title
                : !string.IsNullOrEmpty(part.partName)
                    ? part.partName
                    : part.name;
            int childCount = part.children != null ? part.children.Count : 0;
            return partName + " (children=" + childCount + ", hasParent=" + (part.parent != null) + ")";
        }
    }
}
