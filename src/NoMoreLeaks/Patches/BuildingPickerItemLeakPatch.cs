using HarmonyLib;
using KSP.UI.Screens.SpaceCenter;

namespace NoMoreLeaks.Patches
{
    [HarmonyPatch(typeof(BuildingPickerItem), "OnDestroy")]
    internal static class BuildingPickerItemLeakPatch
    {
        private static void Prefix(BuildingPickerItem __instance)
        {
            CleanupCallbacks(__instance);
        }

        private static void Postfix(BuildingPickerItem __instance)
        {
            CleanupCallbacks(__instance);
        }

        private static void CleanupCallbacks(BuildingPickerItem instance)
        {
            object building = EventCleanup.GetInstanceField(instance, "building");
            EventCleanup.RemoveInstanceEventField(building, "OnClick", instance, "OnBuildingClick");
            EventCleanup.RemoveInstanceEventField(building, "OnInViewChange", instance, "OnBuildingInView");
        }
    }
}
