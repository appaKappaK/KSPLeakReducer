using System.IO;
using UnityEngine;

namespace KSPLeakReducer
{
    internal static class KSPLeakReducerSettings
    {
        private const string ConfigRelativePath = "GameData/KSPLeakReducer/KSPLeakReducer.cfg";
        private const string LegacyConfigRelativePath = "GameData/NoMoreLeaks/NoMoreLeaks.cfg";
        private const string ConfigNodeName = "KSPLEAKREDUCER";
        private const string LegacyConfigNodeName = "NOMORELEAKS";

        internal static bool VerboseDebugLogging { get; private set; }

        internal static void Load()
        {
            VerboseDebugLogging = false;

            string configPath = ResolveConfigPath();
            if (!File.Exists(configPath))
            {
                Debug.Log("[KSPLeakReducer] Config not found at " + configPath + ", using defaults");
                return;
            }

            ConfigNode config = ConfigNode.Load(configPath);
            if (config == null)
            {
                Debug.LogWarning("[KSPLeakReducer] Failed to load config at " + configPath);
                return;
            }

            ConfigNode node = config.GetNode(ConfigNodeName)
                ?? config.GetNode(LegacyConfigNodeName)
                ?? config;
            bool verboseDebugLogging = VerboseDebugLogging;
            node.TryGetValue("VerboseDebugLogging", ref verboseDebugLogging);
            VerboseDebugLogging = verboseDebugLogging;

            Debug.Log("[KSPLeakReducer] VerboseDebugLogging=" + VerboseDebugLogging);
        }

        internal static void LogDebug(string message)
        {
            if (!VerboseDebugLogging || string.IsNullOrEmpty(message)) return;
            Debug.Log("[KSPLeakReducer:Debug] " + message);
        }

        private static string ResolveConfigPath()
        {
            string configPath = Path.Combine(KSPUtil.ApplicationRootPath, ConfigRelativePath);
            if (File.Exists(configPath)) return configPath;

            string legacyConfigPath = Path.Combine(KSPUtil.ApplicationRootPath, LegacyConfigRelativePath);
            if (File.Exists(legacyConfigPath))
            {
                Debug.Log("[KSPLeakReducer] Using legacy config at " + legacyConfigPath);
                return legacyConfigPath;
            }

            return configPath;
        }
    }
}
