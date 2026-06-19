using HarmonyLib;

namespace KSPLeakReducer.Patches
{
    [HarmonyPatch(typeof(ModuleInventoryPart), "DeletePartObject")]
    internal static class ModuleInventoryDeletePartObjectLeakPatch
    {
        private static void Prefix(ModuleInventoryPart __instance)
        {
            Part selectedPart = EventCleanup.GetInstanceField(__instance, "selectedPart") as Part;
            if (selectedPart == null) return;

            KSPLeakReducerSettings.LogDebug("Inventory delete-part-object cleanup for " + DescribePart(selectedPart));
            PartModuleLifecycleLeakPatch.CleanupPartHierarchy(selectedPart, "ModuleInventoryPart.DeletePartObject.Prefix");
        }

        private static void Postfix()
        {
            if (!HighLogic.LoadedSceneIsEditor && !HighLogic.LoadedSceneIsFlight) return;
            InventoryCallbackSweeper.SweepInventoryCallbacks();
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
