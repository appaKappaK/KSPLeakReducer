using Expansions.Serenity;
using HarmonyLib;

namespace NoMoreLeaks.Patches
{
    [HarmonyPatch(typeof(ModuleRobotArmScanner), "OnDestroy")]
    internal static class ModuleRobotArmScannerLeakPatch
    {
        private static void Prefix(ModuleRobotArmScanner __instance)
        {
            CleanupCallbacks(__instance);
        }

        private static void Postfix(ModuleRobotArmScanner __instance)
        {
            CleanupCallbacks(__instance);
        }

        private static void CleanupCallbacks(ModuleRobotArmScanner instance)
        {
            EventCleanup.RemoveOwner(GameEvents.onVesselChange, instance);
        }
    }
}
