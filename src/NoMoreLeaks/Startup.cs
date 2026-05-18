using HarmonyLib;
using UnityEngine;

namespace NoMoreLeaks
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public sealed class Startup : MonoBehaviour
    {
        private const string HarmonyId = "matth.nomoreleaks";
        private const float InventorySweepInterval = 2f;
        private float nextInventorySweep;

        private void Awake()
        {
            new Harmony(HarmonyId).PatchAll();
            Debug.Log("[NoMoreLeaks] Harmony patches applied");
            InventoryCallbackSweeper.Sweep();
        }

        private void Update()
        {
            if (Time.realtimeSinceStartup < nextInventorySweep) return;

            nextInventorySweep = Time.realtimeSinceStartup + InventorySweepInterval;
            InventoryCallbackSweeper.Sweep();
        }
    }
}
