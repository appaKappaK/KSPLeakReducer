using HarmonyLib;

namespace KSPLeakReducer.Patches
{
    [HarmonyPatch(typeof(Vessel), "Unload")]
    internal static class VesselResourceLeakPatch
    {
        private static void Postfix(Vessel __instance)
        {
            CleanupCallbacks(__instance);
        }

        private static void CleanupCallbacks(Vessel vessel)
        {
            EventCleanup.RemoveOwnersWhere(
                GameEvents.onPartResourceFlowStateChange,
                originator => IsOwnedByVesselPartSet(originator, vessel),
                typeof(PartSet).FullName);

            EventCleanup.RemoveOwnersWhere(
                GameEvents.onPartResourceFlowModeChange,
                originator => IsOwnedByVesselPartSet(originator, vessel),
                typeof(PartSet).FullName);
        }

        private static bool IsOwnedByVesselPartSet(object originator, Vessel vessel)
        {
            PartSet partSet = originator as PartSet;
            return partSet != null && ReferenceEquals(EventCleanup.GetInstanceField(partSet, "vessel"), vessel);
        }
    }
}
