using System;

namespace SilkenImpact {
    public static class PluginLogger {
        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogInfo(string message) {
            Plugin.Logger.LogInfo(message);
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogWarning(string message) {
            Plugin.Logger.LogWarning(message);
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogError(string message) {
            Plugin.Logger.LogError(message);
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogDebug(string message) {
            Plugin.Logger.LogDebug(message);
        }

        public static void LogFatal(string message) {
            Plugin.Logger.LogFatal(message);
        }

        [System.Diagnostics.Conditional("DETAIL")]
        public static void LogDetail(string message) {
            Plugin.Logger.LogInfo(message);
        }
    }
}