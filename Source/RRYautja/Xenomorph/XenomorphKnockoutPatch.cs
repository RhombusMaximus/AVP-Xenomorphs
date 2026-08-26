using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using RRYautja.ExtensionMethods;

namespace RRYautja
{
    /// <summary>
    /// Adds 25% knockout chance to Drone and Warrior melee attacks.
    /// When a Drone or Warrior hits a target, there's a 25% chance
    /// to apply the Anesthetic hediff (knocking the target out).
    /// </summary>
    [StaticConstructorOnStartup]
    static class XenomorphKnockoutPatch
    {
        static XenomorphKnockoutPatch()
        {
            try
            {
                var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.knockout");
                var method = AccessTools.Method(typeof(Verb_MeleeAttack), "ApplyMeleeDamageToTarget");
                if (method != null)
                {
                    harmony.Patch(method, postfix: new HarmonyMethod(typeof(XenomorphKnockoutPatch), nameof(ApplyMeleeDamagePostfix)));
                    AvPDebug.LogOnce("KnockoutPatch", "[AVP Xenomorphs] Patched Verb_MeleeAttack.ApplyMeleeDamageToTarget for knockout chance");
                }
            }
            catch (Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to init knockout patch: " + e.Message);
            }
        }

        public static void ApplyMeleeDamagePostfix(Verb_MeleeAttack __instance, Thing target)
        {
            try
            {
                Pawn attacker = __instance.CasterPawn;
                if (attacker == null) return;
                if (!attacker.isXenomorph()) return;
                // Only Drones and Warriors get knockout chance
                string defName = attacker.kindDef?.defName;
                if (defName != "RRY_Xenomorph_Drone" && defName != "RRY_Xenomorph_Warrior") return;

                if (target is Pawn victim && !victim.Dead && !victim.Downed)
                {
                    if (Rand.Chance(0.25f))
                    {
                        victim.health.AddHediff(XenomorphDefOf.RRY_Hediff_Anesthetic);
                        AvPDebug.Log("Knockout", attacker.LabelShort + " knocked out " + victim.LabelShort);
                    }
                }
            }
            catch (Exception e)
            {
                AvPDebug.Error("Knockout patch error: " + e.Message);
            }
        }
    }
}