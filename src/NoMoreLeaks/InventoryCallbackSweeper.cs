using UnityEngine;

namespace NoMoreLeaks
{
    internal static class InventoryCallbackSweeper
    {
        internal static void Sweep()
        {
            int removed = 0;
            removed += EventCleanup.RemoveDestroyedOwners(GameEvents.onPartActionUICreate, typeof(ModuleInventoryPart));
            removed += EventCleanup.RemoveDestroyedOwners(GameEvents.onModuleInventoryChanged, typeof(ModuleInventoryPart));
            removed += EventCleanup.RemoveDestroyedOwners(GameEvents.onEditorPartEvent, typeof(ModuleInventoryPart));
            removed += EventCleanup.RemoveDestroyedOwners(GameEvents.OnPartPurchased, typeof(ModuleInventoryPart));
            removed += EventCleanup.RemoveDestroyedOwners(GameEvents.OnInventoryPartOnMouseChanged, typeof(ModuleInventoryPart));
            removed += EventCleanup.RemoveDestroyedOwners(GameEvents.OnEVACargoMode, typeof(UIPartActionInventorySlot));
            removed += EventCleanup.RemoveDestroyedOwners(GameEvents.onEditorPartDeleted, typeof(UIPartActionInventorySlot));

            if (removed > 0)
                Debug.Log("[NoMoreLeaks] Removed " + removed + " destroyed inventory callbacks");
        }
    }
}
