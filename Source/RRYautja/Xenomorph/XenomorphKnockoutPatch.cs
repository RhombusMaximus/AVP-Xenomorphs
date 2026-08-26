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
    /// Patches Verb_MeleeAttackDamage.ApplyDamage which is the actual
    /// method that applies melee damage in 1.6.
    /// </summary>
    [StaticConstructorOnStartup]
    static class XenomorphKnockoutPatch
    {
        static XenomorphKnockoutPatch()
        {
            try
            {
                var harmony = new Harmony("com.ogliss.rimworld.mod.rryatuja.knockout");

                // Try multiple method names — the internal damage method varies by RimWorld version
                string[] methodNames = { "ApplyDamage", "DoMeleeHit", "ApplyMeleeDamageToTarget" };
                bool patched = false;

                foreach (string name in methodNames)
                {
                    var method = AccessTools.Method(typeof(Verb_MeleeAttack), name);
                    if (method != null)
                    {
                        try
                        {
                            harmony.Patch(method, postfix: new HarmonyMethod(typeof(XenomorphKnockoutPatch), nameof(MeleeDamagePostfix)));
                            AvPDebug.LogOnce("KnockoutPatch", "[AVP Xenomorphs] Patched Verb_MeleeAttack." + name + " for knockout chance");
                            patched = true;
                            break;
                        }
                        catch (Exception)
                        {
                            // Try next method
                        }
                    }
                }

                if (!patched)
                {
                    // List all methods on Verb_MeleeAttack for debugging
                    var methods = typeof(Verb_MeleeAttack).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    foreach (var m in methods)
                    {
                        AvPDebug.LogOnce("VerbMethod_" + m.Name, "[AVP Xenomorphs] Verb_MeleeAttack method: " + m.Name + "(" + string.Join(", ", Array.ConvertAll(m.GetParameters(), p => p.ParameterType.Name + " " + p.Name)) + ")");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("[AVP Xenomorphs] Failed to init knockout patch: " + e.Message);
            }
        }

        public static void MeleeDamagePostfix(Verb_MeleeAttack __instance, Thing target)
        {
            try
            {
                Pawn attacker = __instance.CasterPawn;
                if (attacker == null) return;
                if (!attacker.isXenomorph()) return;
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