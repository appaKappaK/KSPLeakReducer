using HarmonyLib;

namespace NoMoreLeaks.Patches
{
    [HarmonyPatch(typeof(ModuleProceduralFairing), "OnDestroy")]
    internal static class ModuleProceduralFairingLeakPatch
    {
        private static void Prefix(ModuleProceduralFairing __instance)
        {
            Cleanup(__instance);
        }

        private static void Postfix(ModuleProceduralFairing __instance)
        {
            Cleanup(__instance);
        }

        private static void Cleanup(ModuleProceduralFairing instance)
        {
            EventCleanup.RemoveOwner(GameEvents.onVariantApplied, instance);
            EventCleanup.RemoveOwner(GameEvents.onVariantsAdded, instance);
            EventCleanup.RemoveGameEvent(GameEvents.onVariantApplied, instance, "onVariantApplied");
            EventCleanup.RemoveGameEvent(GameEvents.onVariantsAdded, instance, "onVariantsAdded");
        }
    }
}
