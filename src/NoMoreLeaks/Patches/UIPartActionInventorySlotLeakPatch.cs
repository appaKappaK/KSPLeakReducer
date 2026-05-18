using HarmonyLib;

namespace NoMoreLeaks.Patches
{
    [HarmonyPatch(typeof(UIPartActionInventorySlot), "OnDestroy")]
    internal static class UIPartActionInventorySlotLeakPatch
    {
        private static void Prefix(UIPartActionInventorySlot __instance)
        {
            Cleanup(__instance);
        }

        private static void Postfix(UIPartActionInventorySlot __instance)
        {
            Cleanup(__instance);
        }

        private static void Cleanup(UIPartActionInventorySlot instance)
        {
            EventCleanup.RemoveOwner(GameEvents.OnEVACargoMode, instance);
            EventCleanup.RemoveOwner(GameEvents.onEditorPartDeleted, instance);
        }
    }
}
