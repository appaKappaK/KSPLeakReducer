using HarmonyLib;

namespace NoMoreLeaks.Patches
{
    [HarmonyPatch(typeof(Part), "OnDestroy")]
    internal static class PartModuleLifecycleLeakPatch
    {
        private static void Prefix(Part __instance)
        {
            CleanupPartModules(__instance);
        }

        private static void Postfix(Part __instance)
        {
            CleanupPartModules(__instance);
        }

        internal static void CleanupPartModules(Part part)
        {
            if (part == null || part.Modules == null) return;

            for (int i = 0; i < part.Modules.Count; i++)
            {
                if (part.Modules[i] is ModuleInventoryPart inventoryPart)
                    ModuleInventoryPartLeakPatch.Cleanup(inventoryPart);
                else if (part.Modules[i] is ModuleControlSurface controlSurface)
                    ModuleControlSurfaceLeakPatch.Cleanup(controlSurface);
                else if (part.Modules[i] is ModuleGroundPart groundPart)
                    ModuleGroundPartLeakPatch.Cleanup(groundPart);
            }
        }
    }

    [HarmonyPatch(typeof(Part), "OnDelete")]
    internal static class PartOnDeleteLeakPatch
    {
        private static void Prefix(Part __instance)
        {
            PartModuleLifecycleLeakPatch.CleanupPartModules(__instance);
        }
    }

    [HarmonyPatch(typeof(Part), "RemoveModule")]
    internal static class PartRemoveModuleLeakPatch
    {
        private static void Prefix(PartModule module)
        {
            CleanupModule(module);
        }

        private static void Postfix(PartModule module)
        {
            CleanupModule(module);
        }

        private static void CleanupModule(PartModule module)
        {
            if (module is ModuleInventoryPart inventoryPart)
                ModuleInventoryPartLeakPatch.Cleanup(inventoryPart);
            else if (module is ModuleControlSurface controlSurface)
                ModuleControlSurfaceLeakPatch.Cleanup(controlSurface);
            else if (module is ModuleGroundPart groundPart)
                ModuleGroundPartLeakPatch.Cleanup(groundPart);
        }
    }

    [HarmonyPatch(typeof(Part), "RemoveModules")]
    internal static class PartRemoveModulesLeakPatch
    {
        private static void Prefix(Part __instance)
        {
            PartModuleLifecycleLeakPatch.CleanupPartModules(__instance);
        }

        private static void Postfix(Part __instance)
        {
            PartModuleLifecycleLeakPatch.CleanupPartModules(__instance);
        }
    }
}
