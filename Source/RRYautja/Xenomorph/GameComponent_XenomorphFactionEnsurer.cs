using System.Linq;
using RimWorld;
using Verse;

namespace RRYautja
{
    /// <summary>
    /// Ensures the Xenomorph faction exists when a new game starts.
    /// In RimWorld 1.6, hidden factions with requiredCountAtGameStart
    /// may not generate properly. This component creates the faction
    /// if it doesn't exist.
    /// </summary>
    public class GameComponent_XenomorphFactionEnsurer : GameComponent
    {
        public GameComponent_XenomorphFactionEnsurer(Game game) : base() {}

        public override void LoadedGame()
        {
            EnsureFaction();
        }

        public override void StartedNewGame()
        {
            EnsureFaction();
        }

        private void EnsureFaction()
        {
            FactionDef xenoDef = XenomorphDefOf.RRY_Xenomorph;
            if (xenoDef == null)
            {
                Log.Error("[AVP Xenomorphs] RRY_Xenomorph FactionDef is null!");
                return;
            }

            Faction existing = Find.FactionManager.FirstFactionOfDef(xenoDef);
            if (existing != null)
            {
                Log.Message("[AVP Xenomorphs] Xenomorph hive faction already exists.");
                return;
            }

            // Create the faction using RimWorld's faction manager
            Faction faction = FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms(xenoDef));
            Find.FactionManager.Add(faction);
            Log.Message("[AVP Xenomorphs] Created Xenomorph hive faction (was missing).");
        }
    }
}