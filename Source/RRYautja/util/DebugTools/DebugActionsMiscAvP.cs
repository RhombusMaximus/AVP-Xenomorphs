// TODO: DebugAction system was reworked in RimWorld 1.6 - needs migration
#if false
using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse.AI.Group;
using Verse.Profile;
using Verse.Sound;

namespace Verse
{
	// Token: 0x0200053D RID: 1341
	public static class DebugActionsMiscAvP
	{
		// Token: 0x06002209 RID: 8713 RVA: 0x000FDFC4 File Offset: 0x000FC1C4
		[DebugAction("General", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void DestroyAllStrangeFungus()
		{
			foreach (Thing thing in Find.CurrentMap.listerThings.AllThings.ToList<Thing>())
			{
				if (thing is Plant && thing.def.defName.Contains(XenomorphDefOf.RRY_Plant_Neomorph_Fungus.defName))
				{
					thing.Destroy(DestroyMode.Vanish);
				}
			}
		}

	}
}

#endif
