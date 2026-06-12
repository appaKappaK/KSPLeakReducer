using HarmonyLib;
using System.Collections.Generic;

namespace NoMoreLeaks.Patches
{
    [HarmonyPatch(typeof(Part), "OnDestroy")]
    internal static class PartModuleLifecycleLeakPatch
    {
        private static void Prefix(Part __instance)
        {
            CleanupPartModules(__instance, "Part.OnDestroy.Prefix");
        }

        private static void Postfix(Part __instance)
        {
            CleanupPartModules(__instance, "Part.OnDestroy.Postfix");
        }

        internal static void CleanupPartModules(Part part, string reason = null)
        {
            if (part == null || part.Modules == null) return;

            int cleanedModules = 0;
            for (int i = 0; i < part.Modules.Count; i++)
            {
                if (part.Modules[i] is ModuleInventoryPart inventoryPart)
                {
                    ModuleInventoryPartLeakPatch.CleanupCallbacks(inventoryPart);
                    cleanedModules++;
                }
                else if (part.Modules[i] is ModuleControlSurface controlSurface)
                {
                    ModuleControlSurfaceLeakPatch.CleanupCallbacks(controlSurface);
                    cleanedModules++;
                }
                else if (part.Modules[i] is ModuleGroundPart groundPart)
                {
                    ModuleGroundPartLeakPatch.CleanupCallbacks(groundPart);
                    cleanedModules++;
                }
            }

            if (cleanedModules > 0)
                LogLifecycleCleanup(part, cleanedModules, reason);
        }

        internal static void CleanupPartHierarchy(Part part, string reason = null)
        {
            if (part == null) return;

            HashSet<Part> visited = new HashSet<Part>();
            CleanupPartHierarchy(part, reason, visited);
        }

        private static void CleanupPartHierarchy(Part part, string reason, HashSet<Part> visited)
        {
            if (part == null || !visited.Add(part)) return;

            CleanupPartModules(part, reason);

            if (part.children == null) return;
            for (int i = 0; i < part.children.Count; i++)
                CleanupPartHierarchy(part.children[i], reason, visited);
        }

        private static void LogLifecycleCleanup(Part part, int cleanedModules, string reason)
        {
            string partName = !string.IsNullOrEmpty(part.partInfo?.title)
                ? part.partInfo.title
                : !string.IsNullOrEmpty(part.partName)
                    ? part.partName
                    : part.name;
            int childCount = part.children != null ? part.children.Count : 0;
            bool hasParent = part.parent != null;
            NoMoreLeaksSettings.LogDebug("Lifecycle cleanup on " + partName
                + " via " + (string.IsNullOrEmpty(reason) ? "<unknown>" : reason)
                + " cleaned " + cleanedModules + " module(s); children=" + childCount
                + ", hasParent=" + hasParent);
        }
    }

    [HarmonyPatch(typeof(Part), "OnDelete")]
    internal static class PartOnDeleteLeakPatch
    {
        private static void Prefix(Part __instance)
        {
            PartModuleLifecycleLeakPatch.CleanupPartHierarchy(__instance, "Part.OnDelete.Prefix");
        }
    }

    [HarmonyPatch(typeof(Part), "RemoveModule")]
    internal static class PartRemoveModuleLeakPatch
    {
        private static void Prefix(PartModule module)
        {
            CleanupModule(module, "Part.RemoveModule.Prefix");
        }

        private static void Postfix(PartModule module)
        {
            CleanupModule(module, "Part.RemoveModule.Postfix");
        }

        private static void CleanupModule(PartModule module, string reason)
        {
            if (module is ModuleInventoryPart inventoryPart)
            {
                ModuleInventoryPartLeakPatch.CleanupCallbacks(inventoryPart);
                LogModuleCleanup(module, reason);
            }
            else if (module is ModuleControlSurface controlSurface)
            {
                ModuleControlSurfaceLeakPatch.CleanupCallbacks(controlSurface);
                LogModuleCleanup(module, reason);
            }
            else if (module is ModuleGroundPart groundPart)
            {
                ModuleGroundPartLeakPatch.CleanupCallbacks(groundPart);
                LogModuleCleanup(module, reason);
            }
        }

        private static void LogModuleCleanup(PartModule module, string reason)
        {
            string moduleName = module.GetType().FullName;
            string partName = module.part != null
                ? (!string.IsNullOrEmpty(module.part.partInfo?.title) ? module.part.partInfo.title : module.part.name)
                : "<no part>";
            NoMoreLeaksSettings.LogDebug("Module cleanup on " + moduleName
                + " for " + partName
                + " via " + (string.IsNullOrEmpty(reason) ? "<unknown>" : reason));
        }
    }

    [HarmonyPatch(typeof(Part), "RemoveModules")]
    internal static class PartRemoveModulesLeakPatch
    {
        private static void Prefix(Part __instance)
        {
            PartModuleLifecycleLeakPatch.CleanupPartModules(__instance, "Part.RemoveModules.Prefix");
        }

        private static void Postfix(Part __instance)
        {
            PartModuleLifecycleLeakPatch.CleanupPartModules(__instance, "Part.RemoveModules.Postfix");
        }
    }
}
