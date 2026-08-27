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
    /// Patches Pawn.TakeDamage — the lowest level damage entry point.
    /// Checks if the attacker is a Drone/Warrior and applies Anesthetic 25% of the time.
    /// </summary>
    [StaticConstructorOnStartup]
    static class XenomorphKnockoutPatch
    {
        static XenomorphKnockoutPatch()
        {
            try
            {
                var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.knockout");

                // Patch Pawn.TakeDamage — guaranteed to have a body in all RimWorld versions
                var method = AccessTools.Method(typeof(Pawn), "TakeDamage");
                if (method != null)
                {
                    harmony.Patch(method, postfix: new HarmonyMethod(typeof(XenomorphKnockoutPatch), nameof(TakeDamagePostfix)));
                    AvPDebug.LogOnce("KnockoutPatch", "[AVP Xenomorphs] Patched Pawn.TakeDamage for knockout chance");
                }
                else
                {
                    Log.Error("[AVP Xenomorphs] Could not find Pawn.TakeDamage method");
                }
            }
            catch (Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to init knockout patch: " + e.Message);
            }
        }

        /// <summary>
        /// Postfix on Pawn.TakeDamage — after damage is applied,
        /// check if the attacker is a Drone/Warrior and apply knockout 25% of the time.
        /// </summary>
        public static void TakeDamagePostfix(Pawn __instance, ref DamageInfo dinfo)
        {
            try
            {
                // Only process melee damage
                if (dinfo.Def == null) return;

                Pawn attacker = dinfo.Instigator as Pawn;
                if (attacker == null) return;
                if (!attacker.isXenomorph()) return;

                // Only Drones and Warriors get knockout chance
                string defName = attacker.kindDef?.defName;
                if (defName != "RRY_Xenomorph_Drone" && defName != "RRY_Xenomorph_Warrior") return;

                // Only on living, non-downed targets
                if (__instance.Dead || __instance.Downed) return;

                // 25% chance to knock out
                if (Rand.Chance(0.25f))
                {
                    __instance.health.AddHediff(XenomorphDefOf.RRY_Hediff_Anesthetic);
                    AvPDebug.Log("Knockout", attacker.LabelShort + " knocked out " + __instance.LabelShort);
                }
            }
            catch (Exception e)
            {
                AvPDebug.Error("Knockout patch error: " + e.Message);
            }
        }
    }
}