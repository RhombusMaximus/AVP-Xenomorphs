using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using System.Linq;

using RRYautja.ExtensionMethods;
using RRYautja.settings;

namespace RRYautja.Xenomorph
{
    [StaticConstructorOnStartup]
    public class XenomorphHostSystem
    {
        public static List<ThingDef> AllRaces = DefDatabase<ThingDef>.AllDefs.Where(x => x.race != null).ToList();

        static XenomorphHostSystem()
        {
            Log.Message(string.Format("Xenomorph Host System Loaded\n{0} Possible Hosts out of {1} Races detected: Race Setting init", AllRaces.Where(x => x.isPotentialHost()).Count(), AllRaces.Count));
            /*
            DefDatabase<ThingDef>.AllDefsListForReading.ForEach(action: td => 
            {
                if (td.race!=null && td.isPotentialHost())
                {
                    string text = string.Format("{0}'s possible Xenoforms", td.LabelCap);
                //    Log.Message(text);

                    foreach (var item in td.resultingXenomorph())
                    {
                        text = item.LabelCap;
                    //    Log.Message(text);
                    }
                }
            });
            */
        }
    }

}