using HarmonyLib;

namespace KSPLeakReducer.Patches
{
    [HarmonyPatch(typeof(UIPartActionControllerInventory))]
    internal static class UIPartActionControllerInventorySweepPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("CreatePartFromInventory", typeof(AvailablePart))]
        private static void CreatePartFromInventoryAvailablePostfix()
        {
            InventoryCallbackSweeper.SweepInventoryCallbacks();
        }

        [HarmonyPostfix]
        [HarmonyPatch("CreatePartFromInventory", typeof(ProtoPartSnapshot))]
        private static void CreatePartFromInventorySnapshotPostfix()
        {
            InventoryCallbackSweeper.SweepInventoryCallbacks();
        }
    }
}
