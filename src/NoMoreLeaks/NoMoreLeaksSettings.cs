using System.IO;
using UnityEngine;

namespace NoMoreLeaks
{
    internal static class NoMoreLeaksSettings
    {
        private const string ConfigRelativePath = "GameData/NoMoreLeaks/NoMoreLeaks.cfg";

        internal static bool VerboseDebugLogging { get; private set; }

        internal static void Load()
        {
            VerboseDebugLogging = false;

            string configPath = Path.Combine(KSPUtil.ApplicationRootPath, ConfigRelativePath);
            if (!File.Exists(configPath))
            {
                Debug.Log("[NoMoreLeaks] Config not found at " + configPath + ", using defaults");
                return;
            }

            ConfigNode config = ConfigNode.Load(configPath);
            if (config == null)
            {
                Debug.LogWarning("[NoMoreLeaks] Failed to load config at " + configPath);
                return;
            }

            ConfigNode node = config.GetNode("NOMORELEAKS") ?? config;
            bool verboseDebugLogging = VerboseDebugLogging;
            node.TryGetValue("VerboseDebugLogging", ref verboseDebugLogging);
            VerboseDebugLogging = verboseDebugLogging;

            Debug.Log("[NoMoreLeaks] VerboseDebugLogging=" + VerboseDebugLogging);
        }

        internal static void LogDebug(string message)
        {
            if (!VerboseDebugLogging || string.IsNullOrEmpty(message)) return;
            Debug.Log("[NoMoreLeaks:Debug] " + message);
        }
    }
}
