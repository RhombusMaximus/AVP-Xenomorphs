using System;
using RRYautja.settings;

namespace RRYautja
{
    /// <summary>
    /// Centralized debug logging for AVP Xenomorphs.
    /// Only logs when DebugMode is enabled in mod settings.
    /// Rate-limited to prevent log spam.
    /// </summary>
    public static class AvPDebug
    {
        private static int lastLogTick = -9999;
        private const int MinLogInterval = 250; // Only log once per 250 ticks (about 4 seconds)
        private static System.Collections.Generic.HashSet<string> loggedOnce = new System.Collections.Generic.HashSet<string>();

        public static bool Enabled => SettingsHelper.latest?.DebugMode == true;

        /// <summary>
        /// Log a debug message, rate-limited to prevent spam.
        /// Use for per-tick events (mask drawing, job assignment, etc).
        /// </summary>
        public static void Log(string category, string message)
        {
            if (!Enabled) return;
            int tick = Verse.Find.TickManager?.TicksGame ?? 0;
            if (tick - lastLogTick < MinLogInterval) return;
            lastLogTick = tick;
            Verse.Log.Message("[AVP Xenomorphs] [" + category + "] " + message + " (tick " + tick + ")");
        }

        /// <summary>
        /// Log a debug message exactly once per unique key.
        /// Use for one-time events (faction creation, patches, def loading).
        /// </summary>
        public static void LogOnce(string key, string message)
        {
            if (!Enabled) return;
            if (loggedOnce.Contains(key)) return;
            loggedOnce.Add(key);
            Verse.Log.Message("[AVP Xenomorphs] " + message);
        }

        /// <summary>
        /// Log a warning, always logs (not rate-limited).
        /// </summary>
        public static void Warning(string message)
        {
            Verse.Log.Warning("[AVP Xenomorphs] " + message);
        }

        /// <summary>
        /// Log an error, always logs (not rate-limited).
        /// </summary>
        public static void Error(string message)
        {
            Verse.Log.Error("[AVP Xenomorphs] " + message);
        }
    }
}