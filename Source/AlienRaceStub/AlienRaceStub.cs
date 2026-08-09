// Stub for AlienRace types used by AVP-Rimworld
// This is a compile-time stub only — the real AlienRace.dll must be present at runtime
using System.Collections.Generic;
using Verse;

namespace AlienRace
{
    public class ThingDef_AlienRace : ThingDef
    {
    }

    public class BackstoryDef : Def
    {
        public RimWorld.BackstoryDef backstory;
    }

    public class AlienPartGenerator
    {
        public class BodyAddon { }
    }

    public class RaceRestrictionSettings
    {
        public static Dictionary<ThingDef, List<ThingDef_AlienRace>> apparelWhiteDict = new Dictionary<ThingDef, List<ThingDef_AlienRace>>();
    }
}
