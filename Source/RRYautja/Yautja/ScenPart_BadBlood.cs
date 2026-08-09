using System;
using RimWorld;
using RRYautja;
using Verse;

namespace RimWorld
{
    // Token: 0x0200065F RID: 1631
    public class ScenPart_BadBlood : ScenPart
    {
        // Token: 0x060021DF RID: 8671 RVA: 0x001003C0 File Offset: 0x000FE7C0
        public override void PostGameStart()
        {
            foreach (var f in Find.FactionManager.AllFactionsListForReading.FindAll(x=> !x.IsPlayer && x.def!=YautjaDefOf.RRY_Yautja_BadBloodFaction))
            {
                f.TryAffectGoodwillWith(Find.FactionManager.OfPlayer, -100, false, true, null, null);
            }
        }
        
    }
}
