using HarmonyLib;
using UnityEngine;

namespace NoMoreLeaks.Patches
{
    // Fires when the player clicks Launch/Revert in the editor, before any
    // scene teardown begins. All ModuleInventoryPart instances are still alive
    // here, so we can remove their subscriptions while they're still valid
    // Unity objects — before RemoveDestroyedOwners would skip them.
    [HarmonyPatch(typeof(EditorLogic), "exitEditor")]
    internal static class EditorExitInventorySweepPatch
    {
        private static void Prefix()
        {
            NoMoreLeaksSettings.LogDebug("Proactive sweep via EditorLogic.exitEditor.Prefix");
            EditorExitSweep.SweepLiveInventoryModules();
        }
    }

    internal static class EditorExitSweep
    {
        internal static void SweepLiveInventoryModules()
        {
            // Find all live ModuleInventoryPart instances and remove their
            // subscriptions directly — no destroyed-object check needed here
            // because we're calling this while they're still alive.
            ModuleInventoryPart[] modules = Object.FindObjectsOfType<ModuleInventoryPart>();

            int cleaned = 0;
            for (int i = 0; i < modules.Length; i++)
            {
                if (modules[i] == null) continue;
                ModuleInventoryPartLeakPatch.Cleanup(modules[i]);
                cleaned++;
            }

            if (cleaned > 0)
                NoMoreLeaksSettings.LogDebug(
                    "EditorExit proactive sweep cleaned " + cleaned + " live ModuleInventoryPart instance(s)");
        }
    }
}
