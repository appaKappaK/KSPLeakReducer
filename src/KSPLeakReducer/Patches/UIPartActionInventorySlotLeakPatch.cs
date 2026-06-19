using HarmonyLib;

namespace KSPLeakReducer.Patches
{
    [HarmonyPatch(typeof(UIPartActionInventorySlot), "OnDestroy")]
    internal static class UIPartActionInventorySlotLeakPatch
    {
        private static void Prefix(UIPartActionInventorySlot __instance)
        {
            CleanupCallbacks(__instance);
        }

        private static void Postfix(UIPartActionInventorySlot __instance)
        {
            CleanupCallbacks(__instance);
        }

        private static void CleanupCallbacks(UIPartActionInventorySlot instance)
        {
            EventCleanup.RemoveOwner(GameEvents.OnEVACargoMode, instance);
            EventCleanup.RemoveOwner(GameEvents.onEditorPartDeleted, instance);
        }
    }
}
