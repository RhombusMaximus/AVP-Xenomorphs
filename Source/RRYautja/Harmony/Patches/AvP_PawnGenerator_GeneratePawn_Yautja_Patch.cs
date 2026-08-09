using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using System;
using Verse.AI;
using System.Text;
using System.Linq;
using Verse.AI.Group;
using RimWorld.Planet;
using UnityEngine;
using RRYautja.settings;
using RRYautja.ExtensionMethods;

namespace RRYautja
{

    [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", new[] { typeof(PawnGenerationRequest) })]
    public static class AvP_PawnGenerator_GeneratePawn_Yautja_Patch
    {
        public static void Postfix(PawnGenerationRequest request, ref Pawn __result)
        {
            Comp_Yautja _Yautja = __result.TryGetComp<Comp_Yautja>();

            if (_Yautja != null)
            {
                Backstory pawnStoryC = __result.story.childhood;
                Backstory pawnStoryA = __result.story.Adulthood ?? null;

                AlienRace.BackstoryDef bsDefUnblooded = DefDatabase<AlienRace.BackstoryDef>.GetNamed("RRY_Yautja_YoungBlood");
                AlienRace.BackstoryDef bsDefBlooded = DefDatabase<AlienRace.BackstoryDef>.GetNamed("RRY_Yautja_Blooded");
                AlienRace.BackstoryDef bsDefBadbloodA = DefDatabase<AlienRace.BackstoryDef>.GetNamed("RRY_Yautja_BadBloodA");
                AlienRace.BackstoryDef bsDefBadblooBd = DefDatabase<AlienRace.BackstoryDef>.GetNamed("RRY_Yautja_BadBloodB");

                HediffDef unbloodedDef = YautjaDefOf.HMS_Hediff_Unblooded;
                HediffDef unmarkedDef = YautjaDefOf.HMS_Hediff_BloodedUM;
                HediffDef markedDef = YautjaDefOf.HMS_Hediff_BloodedM;

                bool hasunblooded = __result.health.hediffSet.HasHediff(unbloodedDef);
                bool hasbloodedUM = __result.health.hediffSet.HasHediff(unmarkedDef);
                bool hasbloodedM = __result.health.hediffSet.hediffs.Any<Hediff>(x => x.def.defName.StartsWith(markedDef.defName));

                if (!hasunblooded && !hasbloodedUM && !hasbloodedM)
                {
                    HediffDef hediffDef;
                    if (pawnStoryA != null)
                    {
                        if (pawnStoryA != bsDefUnblooded.backstory && __result.kindDef.race == YautjaDefOf.RRY_Alien_Yautja)
                        {
                            hediffDef = _Yautja.Props.bloodedDefs.RandomElement();

                            if (hediffDef != null)
                            {
                                /*
                                PawnKindDef pawnKindDef = YautjaBloodedUtility.RandomMarked(hediffDef);
                                if (_Yautja != null)
                                {
                                    _Yautja.MarkHedifflabel = pawnKindDef.LabelCap;
                                    _Yautja.MarkedhediffDef = hediffDef;
                                    _Yautja.predator = pawnKindDef.RaceProps.predator;
                                    _Yautja.BodySize = pawnKindDef.RaceProps.baseBodySize;
                                    _Yautja.combatPower = pawnKindDef.combatPower;
                                    //    Log.Message(string.Format("{0}: {1}", hediffDef.stages[0].label, pawnKindDef.LabelCap));
                                }
                                */
                            }

                        }
                        else
                        {
                            hediffDef = unbloodedDef;
                        }
                    }
                    else
                    {
                        hediffDef = unbloodedDef;
                    }
                    foreach (var item in __result.RaceProps.body.AllParts)
                    {
                        if (item.def == BodyPartDefOf.Head)
                        {

                            __result.health.AddHediff(hediffDef, item);
                        }
                    }
                }
                else
                {
                    //    Log.Message(string.Format("new pawn has hasunblooded:{0}, hasbloodedUM:{1}, hasbloodedM:{2}", hasunblooded, hasbloodedUM, hasbloodedM));
                }
            }
            if (__result.kindDef.race == YautjaDefOf.RRY_Alien_Yautja)
            {
                if (request.Faction.leader == null && request.Faction != Faction.OfPlayerSilentFail && request.KindDef.race == YautjaDefOf.RRY_Alien_Yautja)
                {
                    Rand.PushState();
                    bool upgradeWeapon = Rand.Chance(0.5f);
                    Rand.PopState();
                    if (__result.equipment.Primary != null && upgradeWeapon)
                    {
                        __result.equipment.Primary.TryGetQuality(out QualityCategory weaponQuality);
                        if (weaponQuality != QualityCategory.Legendary)
                        {
                            Thing Weapon = __result.equipment.Primary;
                            CompQuality Weapon_Quality = Weapon.TryGetComp<CompQuality>();
                            if (Weapon_Quality != null)
                            {
                                Weapon_Quality.SetQuality(QualityCategory.Legendary, ArtGenerationContext.Outsider);
                            }
                        }

                    }
                    else if (__result.apparel.WornApparelCount > 0 && !upgradeWeapon)
                    {
                        foreach (var item in __result.apparel.WornApparel)
                        {
                            item.TryGetQuality(out QualityCategory gearQuality);
                            float upgradeChance = 0.5f;
                            Rand.PushState();
                            bool upgradeGear = Rand.Chance(upgradeChance);
                            Rand.PopState();
                            if (gearQuality != QualityCategory.Legendary)
                            {
                                CompQuality Gear_Quality = item.TryGetComp<CompQuality>();
                                if (Gear_Quality != null)
                                {
                                    if (upgradeGear)
                                    {
                                        Gear_Quality.SetQuality(QualityCategory.Legendary, ArtGenerationContext.Outsider);
                                        break;
                                    }
                                }
                            }
                        }
                    }

                }

            }
            
        }
    }

}