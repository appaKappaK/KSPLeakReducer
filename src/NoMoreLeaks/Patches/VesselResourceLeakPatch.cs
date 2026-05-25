using HarmonyLib;

namespace NoMoreLeaks.Patches
{
    [HarmonyPatch(typeof(Vessel), "Unload")]
    internal static class VesselResourceLeakPatch
    {
        private static void Postfix(Vessel __instance)
        {
            Cleanup(__instance);
        }

        private static void Cleanup(Vessel vessel)
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
