using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KSPLeakReducer
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public sealed class Startup : MonoBehaviour
    {
        private const string HarmonyId = "kspleakreducer.ksp";
        private const float SweepInterval = 0.5f;
        private float nextSweep;

        private void Awake()
        {
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            KSPLeakReducerSettings.Load();
            new Harmony(HarmonyId).PatchAll();
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            Debug.Log("[KSPLeakReducer] Harmony patches applied");
            InventoryCallbackSweeper.Sweep();
        }

        private void OnDestroy()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void OnLevelWasLoaded(int level)
        {
            InventoryCallbackSweeper.Sweep();
            nextSweep = Time.realtimeSinceStartup + SweepInterval;
        }

        private void Update()
        {
            InventoryCallbackSweeper.SweepInventoryCallbacks();
            InventoryCallbackSweeper.SweepAutopilotCallbacks();

            if (Time.realtimeSinceStartup < nextSweep) return;

            nextSweep = Time.realtimeSinceStartup + SweepInterval;
            InventoryCallbackSweeper.Sweep();
        }

        private void OnSceneUnloaded(Scene scene)
        {
            InventoryCallbackSweeper.SweepSceneUnload();
        }
    }
}
