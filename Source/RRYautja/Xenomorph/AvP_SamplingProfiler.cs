using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using HarmonyLib;
using Verse;

namespace RRYautja
{
    /// <summary>
    /// Main-thread sampling profiler for lag diagnosis.
    ///
    /// Active ONLY while the mod's DebugMode setting is enabled (settings ->
    /// AVP Xenomorphs -> Debug). While active it:
    ///   - measures frame/tick time (postfix on TickManager.DoSingleTick)
    ///   - samples the main thread's stack ~40x per second
    ///   - every ~45 seconds, dumps a report: sample count, slow-tick count,
    ///     and the top aggregated main-thread stacks with hit percentages.
    ///
    /// The hot method during a lag episode will dominate the dump regardless
    /// of which mod it belongs to. StackTrace(Thread, bool) suspends the main
    /// thread for the snapshot; at 25 ms intervals the overhead is negligible.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class AvP_SamplingProfiler
    {
        private const int SampleIntervalMs = 25;
        private const int DumpEverySamples = 1800; // ~45 s of sampling

        private static Thread mainThread;
        private static Thread samplerThread;
        private static readonly Stopwatch tickWatch = new Stopwatch();
        private static readonly object statsLock = new object();
        private static readonly Dictionary<string, int> stackCounts = new Dictionary<string, int>();
        private static int totalSamples;
        private static int slowTicks;      // inter-tick gap > 40 ms
        private static long worstTickMs;
        private static long totalTickMs;
        private static int tickMeasurements;

        static AvP_SamplingProfiler()
        {
            try
            {
                var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.samplingprofiler");
                var method = AccessTools.Method(typeof(TickManager), "DoSingleTick");
                if (method != null)
                {
                    harmony.Patch(method, postfix: new HarmonyMethod(typeof(AvP_SamplingProfiler), nameof(TickPostfix)));
                    Log.Message("[AVP Xenomorphs] Sampling profiler installed (active while mod DebugMode is on; dumps a report every ~45s)");
                }
                InstallHarmonyTickProfiler(harmony);
            }
            catch (Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to init sampling profiler: " + e.Message);
            }
        }

        #region Harmony tick profiler (per-type ms attribution; works on Unity Mono where thread-stack sampling does not)

        private static bool tickProfilingInstalled;

        // Sampling: whole ticks are profiled 1-in-6. Prefix/postfix always push/pop in
        // pairs (sentinel -1 = not measured) so nesting stays balanced.
        private static volatile bool sampleThisTick;
        private static readonly System.Collections.Generic.Stack<long> timingStack = new System.Collections.Generic.Stack<long>(64);

        private static readonly Dictionary<string, double> typeMs = new Dictionary<string, double>();
        private static readonly Dictionary<string, int> typeCalls = new Dictionary<string, int>();

        private static void InstallHarmonyTickProfiler(Harmony harmony)
        {
            // Only install the (moderately) expensive instrumentation when the user opted in.
            if (RRYautja.settings.SettingsHelper.latest == null || !RRYautja.settings.SettingsHelper.latest.DebugMode) return;
            if (tickProfilingInstalled) return;

            int patched = 0;
            patched += PatchCategory(harmony, typeof(MapComponent), "MapComponentTick");
            patched += PatchCategory(harmony, typeof(ThingComp), "CompTick");
            patched += PatchCategory(harmony, typeof(HediffComp), "CompPostTick");
            tickProfilingInstalled = true;
            Log.Message($"[AVP Xenomorphs] [Perf] Harmony tick profiler active: {patched} tick methods instrumented (reports per-type ms every ~45s)");
        }

        private static int PatchCategory(Harmony harmony, Type baseType, string tickMethod)
        {
            int patched = 0;
            foreach (Type t in GenTypes.AllSubclassesNonAbstract(baseType))
            {
                try
                {
                    var m = t.GetMethod(tickMethod, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                    // Only the classes that actually override the method (declaring type == subclass)
                    if (m == null || m.DeclaringType != t) continue;
                    harmony.Patch(m,
                        prefix: new HarmonyMethod(typeof(AvP_SamplingProfiler), nameof(TimingPrefix)),
                        postfix: new HarmonyMethod(typeof(AvP_SamplingProfiler), nameof(TimingPostfix)));
                    patched++;
                }
                catch { }
            }
            return patched;
        }

        public static void TimingPrefix()
        {
            timingStack.Push(sampleThisTick ? swGlobal.ElapsedTicks : -1L);
        }

        public static void TimingPostfix(object __instance)
        {
            if (timingStack.Count == 0) return;
            long start = timingStack.Pop();
            if (start < 0) return;
            double ms = (swGlobal.ElapsedTicks - start) * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
            string key = __instance.GetType().FullName;
            typeMs.TryGetValue(key, out double ms0); typeMs[key] = ms0 + ms;
            typeCalls.TryGetValue(key, out int c); typeCalls[key] = c + 1;
        }

        private static readonly System.Diagnostics.Stopwatch swGlobal = System.Diagnostics.Stopwatch.StartNew();

        #endregion

        public static void TickPostfix()
        {
            try
            {
                if (mainThread == null)
                {
                    mainThread = Thread.CurrentThread;
                    tickWatch.Start();
                    return;
                }
                tickWatch.Stop();
                long ms = tickWatch.ElapsedMilliseconds;
                tickWatch.Restart();
                lock (statsLock)
                {
                    tickMeasurements++;
                    totalTickMs += ms;
                    if (ms > worstTickMs) worstTickMs = ms;
                    if (ms > 40) slowTicks++;
                }
                sampleThisTick = (Find.TickManager.TicksGame % 6 == 0);
                EnsureSamplerThread();
            }
            catch
            {
                // never throw into the tick loop
            }
        }

        private static void EnsureSamplerThread()
        {
            if (!AvPDebug.Enabled) return;
            if (samplerThread != null && samplerThread.IsAlive) return;
            samplerThread = new Thread(SamplerLoop) { IsBackground = true, Name = "AvP_SamplingProfiler" };
            samplerThread.Start();
        }

        private static void SamplerLoop()
        {
            while (AvPDebug.Enabled && Current.ProgramState == ProgramState.Playing)
            {
                int samplesAtDump;
                lock (statsLock)
                {
                    samplesAtDump = totalSamples + DumpEverySamples;
                }
                int windowStartTicks = slowTicks;
                long windowStartWorst = worstTickMs;
                while (true)
                {
                    Thread.Sleep(SampleIntervalMs);
                    lock (statsLock)
                    {
                        if (!AvPDebug.Enabled) return;
                        totalSamples++;
                        if (totalSamples >= samplesAtDump) break;
                    }
                    RecordSample();
                }
                DumpReport(windowStartTicks, windowStartWorst);
            }
        }

        private static void RecordSample()
        {
#if NETFRAMEWORK
            // StackTrace(Thread, bool) only exists on .NET Framework — the 1.6 build.
            // On net10 (1.7/Waylith) thread-stack sampling is unavailable and we
            // fall back to tick/frame timing only.
            try
            {
                if (mainThread == null) return;
                var trace = new StackTrace(mainThread, false);
                var frames = trace.GetFrames();
                if (frames == null) return;
                // Key = the two topmost managed frames (enough to identify the hot method)
                string key = null;
                int taken = 0;
                var sb = new StringBuilder();
                for (int i = 0; i < frames.Length && taken < 2; i++)
                {
                    var m = frames[i].GetMethod();
                    if (m == null) continue;
                    var declaring = m.DeclaringType;
                    if (declaring == null) continue;
                    var name = declaring.FullName + "." + m.Name;
                    if (name.StartsWith("System.") || name.StartsWith("mscorlib")) continue;
                    sb.Append(taken == 0 ? name : (" <- " + name));
                    taken++;
                }
                if (taken == 0) return;
                key = sb.ToString();
                lock (statsLock)
                {
                    stackCounts.TryGetValue(key, out int c);
                    stackCounts[key] = c + 1;
                }
            }
            catch
            {
                // sample failed (e.g. thread in native transition) — ignore
            }
#endif
        }

        private static void DumpReport(int windowStartSlowTicks, long windowStartWorst)
        {
            try
            {
            List<KeyValuePair<string, int>> top;
            int samples;
            int slow, meas;
            long worst, avg;
            lock (statsLock)
            {
                samples = totalSamples;
                slow = slowTicks;
                meas = tickMeasurements;
                worst = worstTickMs;
                avg = meas > 0 ? totalTickMs / meas : 0;
                top = stackCounts.OrderByDescending(kv => kv.Value).Take(25).ToList();
                stackCounts.Clear();
                // reset per-window slow/worst baselines so each report covers its own window
                slowTicks = 0;
                worstTickMs = 0;
                tickMeasurements = 0;
                totalTickMs = 0;
            }
            if (samples == 0) return;
            var report = new StringBuilder();
            report.AppendLine($"[AVP Xenomorphs] [Perf] Sampling report: {samples} samples, tick/frame avg {avg} ms, worst {worst} ms, slow (>40ms) {slow}");
            foreach (var kv in top)
            {
                report.AppendLine($"[AVP Xenomorphs] [Perf]   {100 * kv.Value / (double)samples,5:0.0}%  {kv.Key}");
            }
            // Per-type tick attribution (Harmony timing, 1-in-6 sampled ticks)
            List<KeyValuePair<string, double>> typeTop;
            Dictionary<string, int> callsSnapshot;
            lock (statsLock)
            {
                typeTop = typeMs.OrderByDescending(kv => kv.Value).Take(15).ToList();
                callsSnapshot = new Dictionary<string, int>(typeCalls);
                typeMs.Clear();
                typeCalls.Clear();
            }
            if (typeTop.Count > 0)
            {
                report.AppendLine("[AVP Xenomorphs] [Perf] Top ticking components (sampled ms this window / calls):");
                foreach (var kv in typeTop)
                {
                    callsSnapshot.TryGetValue(kv.Key, out int calls);
                    report.AppendLine($"[AVP Xenomorphs] [Perf]   {kv.Value,9:0.0} ms  ({calls,5} calls)  {kv.Key}");
                }
            }
            Log.Message(report.ToString().TrimEndNewlines());
            }
            catch (Exception e)
            {
                // Never let the report kill the sampler thread — log and keep sampling.
                Verse.Log.Warning("[AVP Xenomorphs] [Perf] Sampling report failed: " + e.Message);
            }
        }
    }
}
