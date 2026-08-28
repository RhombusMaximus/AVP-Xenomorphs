using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RRYautja
{
    /// <summary>
    /// Defines rules for which Xenomorph type spawns from a host.
    /// Evaluated in priority order (lowest priority number = checked first).
    /// First matching def determines the spawn type.
    /// 
    /// Adding a new Xenomorph type: create a new XenomorphSpawnDef in XML.
    /// No C# code changes needed.
    /// </summary>
    public class XenomorphSpawnDef : Def
    {
        /// <summary>Priority order. Lower = checked first.</summary>
        public int priority = 100;

        /// <summary>Host must be humanlike (true) or non-humanlike (false). null = either.</summary>
        public bool? hostHumanlike;

        /// <summary>Host body size range. Empty/invalid = any size.</summary>
        public FloatRange hostBodySizeRange = new FloatRange(0f, 999f);

        /// <summary>Specific host race defNames. Empty = any race.</summary>
        public List<string> hostRaceDefNames = new List<string>();

        /// <summary>Exclude these host race defNames.</summary>
        public List<string> excludeHostRaceDefNames = new List<string>();

        /// <summary>Requires royal facehugger impregnation.</summary>
        public bool requiresRoyal = false;

        /// <summary>Requires predalien impregnation.</summary>
        public bool requiresPredalien = false;

        /// <summary>If true, only spawn if no queen present on map.</summary>
        public bool onlyIfNoQueen = false;

        /// <summary>PawnKindDefs to choose from, with weights.</summary>
        public List<PawnKindDef> spawnKinds;
        public List<float> spawnWeights;

        /// <summary>Check if this def matches the given host pawn.</summary>
        public bool Matches(Pawn host, bool isRoyal, bool isPredalien, bool queenPresent)
        {
            if (requiresRoyal && !isRoyal) return false;
            if (requiresPredalien && !isPredalien) return false;
            if (onlyIfNoQueen && queenPresent) return false;

            if (hostHumanlike.HasValue && host.RaceProps.Humanlike != hostHumanlike.Value) return false;

            if (!hostBodySizeRange.Includes(host.BodySize)) return false;

            if (hostRaceDefNames.Count > 0 && !hostRaceDefNames.Contains(host.def.defName)) return false;

            if (excludeHostRaceDefNames.Contains(host.def.defName)) return false;

            return true;
        }

        /// <summary>Pick a PawnKindDef using weighted random selection.</summary>
        public PawnKindDef PickKind()
        {
            if (spawnKinds == null || spawnKinds.Count == 0) return null;

            if (spawnWeights == null || spawnWeights.Count != spawnKinds.Count)
            {
                return spawnKinds.RandomElement();
            }

            float total = spawnWeights.Sum();
            if (total <= 0f) return spawnKinds[0];

            float roll = Rand.Range(0f, total);
            float cumulative = 0f;
            for (int i = 0; i < spawnKinds.Count; i++)
            {
                cumulative += spawnWeights[i];
                if (roll <= cumulative) return spawnKinds[i];
            }
            return spawnKinds[spawnKinds.Count - 1];
        }
    }
}