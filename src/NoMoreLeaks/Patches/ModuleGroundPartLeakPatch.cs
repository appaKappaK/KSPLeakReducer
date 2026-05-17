using HarmonyLib;

namespace NoMoreLeaks.Patches
{
    [HarmonyPatch(typeof(ModuleGroundPart))]
    internal static class ModuleGroundPartLeakPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnDestroy")]
        private static void OnDestroyPrefix(ModuleGroundPart __instance)
        {
            Cleanup(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnDestroy")]
        private static void OnDestroyPostfix(ModuleGroundPart __instance)
        {
            Cleanup(__instance);
        }

        internal static void Cleanup(ModuleGroundPart instance)
        {
            if (instance == null) return;

            EventCleanup.RemoveGameEvent(GameEvents.onPartActionUIShown, instance, "OnPartActionUIShown");
            EventCleanup.RemoveGameEvent(GameEvents.onPartActionUIDismiss, instance, "OnPartActionUIDismiss");
            EventCleanup.RemoveGameEvent(GameEvents.onVesselChange, instance, "OnVesselChange");
            EventCleanup.RemoveGameEvent(GameEvents.onPartWillDie, instance, "OnPartWillDie");
            EventCleanup.RemoveGameEvent(GameEvents.onSceneConfirmExit, instance, "OnLeavingScene");
            EventCleanup.RemoveGameEvent(GameEvents.OnAnimationGroupRetractComplete, instance, "OnRetractCompleted");
            EventCleanup.RemoveGameEvent(GameEvents.OnEVAConstructionMode, instance, "OnEVAConstructionMode");

            if (instance is ModuleGroundSciencePart sciencePart)
                CleanupSciencePart(sciencePart);

            if (instance is ModuleGroundExperiment experiment)
                CleanupExperiment(experiment);

            if (instance is ModuleGroundExpControl control)
                CleanupControl(control);
        }

        private static void CleanupSciencePart(ModuleGroundSciencePart instance)
        {
            EventCleanup.RemoveGameEvent(GameEvents.onGroundScienceDeregisterCluster, instance, "OnGroundScienceDeregisterCluster");
            EventCleanup.RemoveGameEvent(GameEvents.onGroundScienceClusterUpdated, instance, "OnGroundScienceClusterUpdated");
            EventCleanup.RemoveGameEvent(GameEvents.onGroundScienceClusterPowerStateChanged, instance, "OnGroundScienceClusterPowerStateChanged");
        }

        private static void CleanupExperiment(ModuleGroundExperiment instance)
        {
            EventCleanup.RemoveGameEvent(GameEvents.onGroundScienceGenerated, instance, "OnGroundScienceGenerated");
            EventCleanup.RemoveGameEvent(GameEvents.onGroundScienceTransmitted, instance, "OnGroundScienceTransmitted");
        }

        private static void CleanupControl(ModuleGroundExpControl instance)
        {
            EventCleanup.RemoveGameEvent(GameEvents.onGroundSciencePartDeployed, instance, "OnGroundSciencePartDeployed");
            EventCleanup.RemoveGameEvent(GameEvents.onPartActionUIShown, instance, "OnPartActionUIOpened");
            EventCleanup.RemoveGameEvent(GameEvents.onPartActionUIDismiss, instance, "OnPartActionUIDismiss");
            EventCleanup.RemoveGameEvent(GameEvents.onGroundScienceClusterUpdated, instance, "OnGroundScienceClusterUpdated");
            EventCleanup.RemoveGameEvent(GameEvents.onGroundSciencePartEnabledStateChanged, instance, "OnGroundSciencePartEnabledStateChanged");
            EventCleanup.RemoveGameEvent(GameEvents.onGroundSciencePartRemoved, instance, "OnGroundScienceModuleRemoved");
            EventCleanup.RemoveGameEvent(GameEvents.onGroundScienceClusterPowerStateChanged, instance, "OnGroundScienceClusterPowerStateChanged");
        }
    }
}
