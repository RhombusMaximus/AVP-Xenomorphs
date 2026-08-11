# Remove Yautja (Predator) Content from AVP-Rimworld

> **For Hermes:** Use subagent-driven-development skill to implement this plan task-by-task.

**Goal:** Remove ALL Yautja/Predator content from the AVP-Rimworld mod, keeping only Xenomorph and USCM content.

**Architecture:** The Yautja content spans 5 C# projects, ~40 XML def files, ~200 texture files, and deep cross-references in shared C# files. The approach is: (1) create a removal branch, (2) delete Yautja-only files, (3) surgically edit shared files to remove Yautja references, (4) remove the HunterMarkingSystem project entirely, (5) fix compilation, (6) verify.

**Tech Stack:** C# (.NET 4.7.2), RimWorld 1.6 modding, Harmony patches, XML defs

---

## Key Decisions

1. **HunterMarkingSystem** is entirely Yautja-themed (predator blood-marking culture). Remove the whole project + assembly + all its defs/patches. However, `XenomorphHostSystem.cs` has `using HunterMarkingSystem.*` lines that must be removed, and `AvPExtensions.cs` references `HMSUtility.GetMark()` in the Yautja-specific butchering code (lines 689-692, 817-820).

2. **Cloaking system** (`Cloakgen`, `Hediff_Cloak`, `Gizmo_CloakgenStatus`, `Graphic_Invisible`, `PawnGraphicSet_Invisible`) is Yautja-specific BUT `Graphic_Invisible` and `PawnGraphicSet_Invisible` are also used by Xenomorphs (hidden xenomorphs) and USCM Comp_Stealth. KEEP `Graphic_Invisible.cs` and `PawnGraphicSet_Invisible.cs`. REMOVE `Cloakgen.cs`, `Hediff_Cloak.cs`, `Gizmo_CloakgenStatus.cs`.

3. **`YautjaDefOf.cs`** contains both Yautja-specific defs AND defs used by other systems (e.g., `RRY_Hediff_Cloaked` is used by cloaking patches, `RRY_Rynath` is a Yautja animal). The entire file should be removed, but all references to it in non-Yautja code must be removed or replaced.

4. **`Returning_Projectile.cs`** is the Smart Disk returning projectile — Yautja only. Remove it. Also remove `Hediff_Bouncer.cs` and the `RRY_Hediff_BouncedProjectile` def.

5. **`Building_Turret_Shoulder.cs`** and `CompEquippableTurret.cs` are Yautja plasmacaster/shoulder cannon support. Remove them and their defs.

6. **USCM `Comp_Stealth.cs`** is a USCM stealth field — KEEP it. It uses `Graphic_Invisible` (which we keep) and `RRYautja.ExtensionMethods` (which we keep after removing Yautja-specific extension methods).

7. **Shared files to surgically edit** (not delete):
   - `Source/RRYautja/util/AvPExtensions.cs` — remove `isYautja()`, remove Yautja branches in butchering methods (lines 689-738, 817-849, 916-948, 1008-1014)
   - `Source/RRYautja/util/AvPSettings.cs` — remove `AllowYautjaFaction` setting, remove YautjaDefOf references
   - `Source/RRYautja/Harmony/Patches.cs` — remove YautjaDefOf reference (line 45), remove `using RRYautja.settings` if unused
   - `Source/RRYautja/Xenomorph/XenomorphHostSystem.cs` — remove `using HunterMarkingSystem.*` lines
   - `Source/RRYautja/Xenomorph/HediffComp_SpawnerXeno.cs` — remove YautjaDefOf reference
   - Multiple Harmony patches that reference `YautjaDefOf.RRY_Hediff_Cloaked` — remove the cloaked check or the whole patch if Yautja-only
   - `Source/TrapsRearmable/Traps/TrapsDefOf.cs` — remove "Yautja" comments (cosmetic only, no functional change)

8. **XML files** — delete all Yautja-named files, edit shared files to remove Yautja def references.

9. **Textures** — delete all Yautja texture directories.

10. **Sounds** — delete Yautja sound defs (check if any sound files are shared).

11. **Languages/NameBanks** — delete all RRYautja* name bank files.

12. **Patches** — delete `Yautja_Race_patch_Baby_and_Children.xml`, edit shared patches to remove Yautja references.

---

## Task List

### Task 1: Create removal branch

**Objective:** Create a clean branch for the Yautja removal work.

**Files:** None (git operation only)

**Steps:**
1. `cd ~/AVP-Rimworld`
2. `git checkout migration/1.1-to-1.6` (ensure we're on the migrated branch)
3. `git pull`
4. `git checkout -b removal/remove-yautja`
5. `git commit --allow-empty -m "chore: start Yautja content removal"`

---

### Task 2: Delete Yautja-only C# files

**Objective:** Remove all C# files that are exclusively Yautja-related.

**Files to delete:**
- `Source/RRYautja/Yautja/` (entire directory — Comp_Yautja.cs, DamageWorker_CombiPin.cs, GetReturningProjectile.cs, Hediff_Bouncer.cs, HediffComp_PinnedByWeapon.cs, IncidentWorker_WandererJoin.cs, Recipe_Remove_Gauntlet.cs, ScenPart_BadBlood.cs, Stuffable_Bullet.cs, Stuffable_Projectile.cs, ThoughtWorker_HonourableVsBadBlood.cs, Verb_Launch_Stuffable_Projectile.cs, Verb_Shoot_Stuffable.cs, YautjaBloodedUtility.cs, YautjaDefOf.cs, YautjaThoughtDef.cs, HealthShard/*)
- `Source/RRYautja/util/Cloakgen.cs`
- `Source/RRYautja/util/Hediff_Cloak.cs`
- `Source/RRYautja/util/Gizmo_CloakgenStatus.cs`
- `Source/RRYautja/util/Gizmo_ShardStatus.cs` (Yautja health shard gizmo)
- `Source/RRYautja/util/Returning_Projectile.cs`
- `Source/RRYautja/util/Building_Turret_Shoulder.cs`
- `Source/RRYautja/util/CompEquippableTurret.cs`
- `Source/RRYautja/util/MarkOffsetDefExtension.cs` (HunterMarkingSystem extension)
- `Source/RRYautja/Harmony/Patches/AvP_ApparelGraphicRecordGetter_TryGetGraphicApparel_YautjaSpecificHat_Patch.cs`
- `Source/RRYautja/Harmony/Patches/AvP_IncidentWorker_RaidEnemy_Yautja_TryExecute_Patch.cs`
- `Source/RRYautja/Harmony/Patches/AvP_PawnGenerator_GenerateBodyType_Yautja_Patch.cs`
- `Source/RRYautja/Harmony/Patches/AvP_PawnGenerator_GeneratePawn_Yautja_Patch.cs`
- `Source/RRYautja/Harmony/Patches/AvP_ShieldBelt_AllowVerbCast_YautjaWeapons_Patch.cs`
- `Source/RRYautja/Harmony/Patches/AvP_Pawn_Strip_Patch.cs` (wristblade strip prevention)
- `Source/RRYautja/Harmony/Patches/AvP_Building_Turret_Shoulder_ThreatDisabled_Patch.cs`
- `Source/RRYautja/Harmony/Patches/AvP_Building_Door_CanOpen_Cloaked_Patch.cs` (cloak-only door patch)
- `Source/RRYautja/Harmony/Patches/AvP_PawnRenderer_DrawEquipment_Cloak_Patch.cs` (cloak equipment render)
- `Source/RRYautja/Harmony/Patches/AvP_PawnUtility_IsInvisible_Patch.cs` (references Cloakgen)
- `Source/RRYautja/Harmony/Patches/AvP_PawnWoundDrawer_RenderOverBody_Patch.cs` (references RRY_Hediff_Cloaked)
- `Source/RRYautja/Harmony/Patches/AvP_PawnUIOverlay_DrawPawnGUIOverlay_Stealth_Patch.cs` (references RRY_Hediff_Cloaked)
- `Source/RRYautja/Harmony/Patches/AvP_Pawn_ThreatDisabled_Patch.cs` (references RRY_Hediff_Cloaked)
- `Source/RRYautja/Harmony/Patches/AvP_ThingSelectionUtility_SelectableByMapClick_HostileStealth_Patch.cs` (references RRY_Hediff_Cloaked)
- `Source/RRYautja/Harmony/Patches/AvP_AlienBodyAddon_CanDrawAddon_Patch.cs` (references RRY_Hediff_Cloaked)
- `Source/RRYautja/Harmony/Patches/AvP_PawnObserver_ObserveSurroundingThings_Patch.cs` (references YautjaDefOf)
- `Source/RRYautja/Harmony/Patches/AvP_Pawn_PathFollower_CostToMoveIntoCell_Patch.cs` (references YautjaDefOf)
- `Source/RRYautja/Harmony/Patches/AvP_StatWorker_GetExplanationUnfinalized_Patch.cs` (references YautjaDefOf)
- `Source/RRYautja/Harmony/Patches/AvP_StatWorker_GetValueUnfinalized_Patch.cs` (references YautjaDefOf)
- `Source/RRYautja/Harmony/Patches/AvP_IncidentWorker_RaidEnemyPatch_GetLetterText_Patch.cs` (references YautjaDefOf)
- `Source/RRYautja/Harmony/Patches/AvP_PawnGenerator_GeneratePawn_Misc_Patch.cs` (references YautjaDefOf)

**KEEP** (shared with Xenomorph/USCM):
- `Source/RRYautja/util/Graphic_Invisible.cs` — used by Xenomorphs and USCM Comp_Stealth
- `Source/RRYautja/util/PawnGraphicSet_Invisible.cs` — used by Xenomorphs
- `Source/RRYautja/Harmony/Patches/AvP_PawnGraphicSet_HeadMatAt_Invis_Patch.cs` — used for Xenomorph invisibility
- `Source/RRYautja/Harmony/Patches/AvP_PawnGraphicSet_HairMatAt_Invis_Patch.cs` — used for Xenomorph invisibility
- `Source/RRYautja/Harmony/Patches/AvP_PawnRenderer_OverrideMaterialIfNeeded_Xenomorph_Patch.cs` — Xenomorph patch (already #if false'd, remove the isCloaked comment block inside)

**Steps:**
1. Run `rm -rf Source/RRYautja/Yautja/`
2. Delete each individual file listed above
3. Commit: `git add -A && git commit -m "refactor: delete Yautja-only C# source files"`

---

### Task 3: Delete HunterMarkingSystem project entirely

**Objective:** Remove the entire HunterMarkingSystem C# project and assembly.

**Files to delete:**
- `Source/HunterMarkingSystem/` (entire directory)
- `Assemblies/HunterMarkingSystem.dll`

**Also edit:**
- `Source/RRYautja/RRYautja.csproj` — remove the `<ProjectReference Include="..\HunterMarkingSystem\HunterMarkingSystem.csproj">` element (line 68)

**Steps:**
1. `rm -rf Source/HunterMarkingSystem/`
2. `rm Assemblies/HunterMarkingSystem.dll`
3. Edit `Source/RRYautja/RRYautja.csproj` to remove the ProjectReference
4. Commit: `git add -A && git commit -m "refactor: remove HunterMarkingSystem project and assembly"`

---

### Task 4: Edit shared C# files to remove Yautja references

**Objective:** Surgically remove all Yautja references from shared C# files that must remain.

**Files to edit:**

#### 4a. `Source/RRYautja/util/AvPExtensions.cs`
- Remove `using HunterMarkingSystem;` (line 1)
- Remove `isYautja()` method (lines 40-42)
- In the butchering methods (around lines 689-738, 817-849, 916-948): remove the `bool yautja = ...` lines and all `if (yautja) { ... }` blocks
- Remove the static field declarations at bottom (lines 1008-1014) that reference YautjaDefOf and Yautja backstories
- Remove `isBlooded()`, `isBloodUnmarked()`, `isBloodMarked()` if they reference YautjaDefOf (check — they use defName.Contains which is string-based, so they may be safe to keep, but they're Yautja culture features — remove them)
- Remove `MarkStatus` enum if present (line 1022-1023)

#### 4b. `Source/RRYautja/util/AvPSettings.cs`
- Remove `using HunterMarkingSystem;` (line 6)
- Remove `AllowYautjaFaction` field (line 31)
- Remove `AllowYautjaFaction` Scribe_Values.Look (line 43)
- Remove `AllowYautjaFaction` checkbox UI (line 85)
- Remove YautjaDefOf references in race whitelist (lines 147-148) — replace with just `td == ThingDefOf.Human`

#### 4c. `Source/RRYautja/Harmony/Patches.cs`
- Remove `using RRYautja.settings;` if it becomes unused after removing YautjaDefOf reference
- Remove line 45: `AlienRace.RaceRestrictionSettings.apparelWhiteDict[key: def].Add(item: ((AlienRace.ThingDef_AlienRace)YautjaDefOf.RRY_Alien_Yautja));`
- Check if the entire method/block around line 45 is Yautja-only and remove if so

#### 4d. `Source/RRYautja/Xenomorph/XenomorphHostSystem.cs`
- Remove `using HunterMarkingSystem.Settings;` (line 7)
- Remove `using HunterMarkingSystem.ExtensionMethods;` (line 8)

#### 4e. `Source/RRYautja/Xenomorph/HediffComp_SpawnerXeno.cs`
- Remove any `YautjaDefOf` references — check what it references and remove/replace

#### 4f. `Source/RRYautja/Harmony/Patches/AvP_PawnRenderer_OverrideMaterialIfNeeded_Xenomorph_Patch.cs`
- Remove the commented-out `isCloaked()` block (lines 67-72 inside the `#if false` block) — cosmetic cleanup

#### 4g. `Source/RRYautja/Harmony/Patches/AvP_PawnGraphicSet_HeadMatAt_Invis_Patch.cs`
- Check if it references `YautjaDefOf.RRY_Hediff_Cloaked` — if so, remove that check, keep the Xenomorph hidden check

#### 4h. `Source/RRYautja/Harmony/Patches/AvP_PawnGraphicSet_HairMatAt_Invis_Patch.cs`
- Same as 4g — remove cloak references, keep Xenomorph hidden references

#### 4i. `Source/RRYautja/Harmony/Patches/AvP_FoodUtility_BestPawnToHuntForPredator_Patch.cs`
- Check for YautjaDefOf references and remove

#### 4j. `Source/RRYautja/Harmony/Patches/AvP_FoodUtility_TryFindBestFoodSourceFor_Patch.cs`
- Check for YautjaDefOf references and remove

#### 4k. `Source/RRYautja/Harmony/Patches/AvP_FoodUtility_AddFoodPoisoningHediff_Patch.cs`
- Check for YautjaDefOf references and remove

#### 4l. `Source/RRYautja/Harmony/Patches/AvP_RimWorld_Apparel_GetWornGizmos_Patch.cs`
- Check for Cloakgen/YautjaDefOf references and remove

#### 4m. `Source/RRYautja/Harmony/Patches/AvP_RimWorld_Cloakgen_GetWornGizmos_Patch.cs`
- This file is about Cloakgen gizmos — delete it entirely (it's Yautja-only)

#### 4n. `Source/RRYautja/Harmony/Patches/AvP_Pawn_ApparelTracker_Notify_ApparelAddedRemoved_CompAbilityItem_Patch.cs`
- Check for YautjaDefOf references and remove

#### 4o. `Source/RRYautja/Harmony/Patches/AvP_Pawn_EquipmentTracker_Notify_EquipmentAddedRemoved_CompAbilityItem_Patch.cs`
- Check for YautjaDefOf references and remove

#### 4p. `Source/RRYautja/util/Apparel_Comps.cs`
- Check for YautjaDefOf reference and remove

#### 4q. `Source/RRYautja/util/IntergrationUtils.cs`
- Check for YautjaDefOf reference and remove

#### 4r. `Source/RRYautja/util/DebugTools/DebugToolsPawnAvP.cs`
- Check for YautjaDefOf references and remove Yautja debug spawning

#### 4s. `Source/RRYautja/util/DebugTools/DebugToolsSpawningAvP.cs`
- Check for YautjaDefOf references and remove Yautja debug spawning

#### 4t. `Source/TrapsRearmable/Traps/TrapsDefOf.cs`
- Remove "Yautja" comments (cosmetic, lines 16, 19, 22)

**Steps:**
1. For each file, read it, identify the exact Yautja references, and edit them out
2. Commit after each logical group: `git commit -m "refactor: remove Yautja references from AvPExtensions.cs"` etc.

---

### Task 5: Delete Yautja-only XML def files

**Objective:** Remove all XML def files that are exclusively Yautja-related.

**Files to delete (Defs/):**
- `Defs/BackstoryDefs/Backstories_Yautja.xml`
- `Defs/BackstoryDefs/BackstoriesYautja.xml.bak`
- `Defs/Bodies/Bodies_Yautja.xml`
- `Defs/Bodies/BodyPartGroups_Yautja.xml`
- `Defs/Bodies/BodyParts_Yautja.xml`
- `Defs/DamageDefs/Damages_Yautja_RangedDamageTypes.xml`
- `Defs/FactionDefs/Factions_Yautja_Player.xml`
- `Defs/FactionDefs/Faction_Yautja_Badblood.xml`
- `Defs/FactionDefs/Faction_Yautja_Base.xml`
- `Defs/FactionDefs/Faction_Yautja.xml`
- `Defs/HunterMarkingSystem/HunterCultrue_Yautja.xml`
- `Defs/HunterMarkingSystem/Concepts_HunterMarkingSystem.xml`
- `Defs/HunterMarkingSystem/Hediffs_Special_Effects_HunterMarkingSystem.xml`
- `Defs/HunterMarkingSystem/Jobs_HunterMarkingSystem.xml`
- `Defs/HunterMarkingSystem/Thoughts_Situation_Social_HunterMarkingSystem.xml`
- `Defs/HunterMarkingSystem/Thoughts_Situation_Special_HunterMarkingSystem.xml`
- `Defs/JobDefs/Jobs_Yautja.xml`
- `Defs/Misc/LifeStageDefs/Yautja_LifeStages.xml`
- `Defs/PawnKindDefs/PawnKinds_NPC_Yautja.xml`
- `Defs/PawnKindDefs/PawnKinds_Player_Yautja.xml`
- `Defs/RulePackDefs/RulePacks_NameMakers_RRYautjaFaction.xml`
- `Defs/Scenarios/Yautja_Bobs_Blooding.xml`
- `Defs/Scenarios/YautjaNormalStarts.xml`
- `Defs/SoundDefs/Yautja_Sounds_Weapons.xml`
- `Defs/Storyteller/Incidents_World_Quests_Yautja.xml`
- `Defs/ThingDefs_Buildings/Yautja_Building_ApparelTurret.xml`
- `Defs/ThingDefs_Buildings/Yautja_Building_Structure.xml`
- `Defs/ThingDefs/Yautja_Apparel_Armor.xml`
- `Defs/ThingDefs/Yautja_Apparel_Clothing.xml`
- `Defs/ThingDefs/Yautja_Apparel_Equipment.xml`
- `Defs/ThingDefs/Yautja_BaseAbstracts_Apparel.xml`
- `Defs/ThingDefs/Yautja_BaseAbstracts_Weapons.xml`
- `Defs/ThingDefs/Yautja_Races_Animal_Giant.xml`
- `Defs/ThingDefs/Yautja_Race.xml`
- `Defs/ThingDefs/Yautja_Resource_Manufactured.xml`
- `Defs/ThingDefs/Yautja_Resource_Stuff.xml`
- `Defs/ThingDefs/Yautja_Weapons_Melee_Combistaff.xml`
- `Defs/ThingDefs/Yautja_Weapons_Melee.xml`
- `Defs/ThingDefs/Yautja_Weapons_Ranged.xml`
- `Defs/ThoughtDefs/Thoughts_YautjaCannibal.xml`
- `Defs/TraderKindDefs/TraderKinds_Base_Yautja.xml`
- `Defs/TraderKindDefs/TraderKinds_Caravan_Yautja.xml`
- `Defs/TraderKindDefs/TraderKinds_Orbital_Yautja.xml`
- `Defs/TraderKindDefs/TraderKinds_Visitor_Yautja.xml`
- `Defs/TutorDefs/Concepts_Yautja.xml`

**Also delete:**
- `Defs/ThingDefs/Yautja_Hound_Race.xml.bak`
- `Patches/FacialStuffYautja.xml.bak`
- `Patches/Yautja_Race_patch_Baby_and_Children.xml`

**Steps:**
1. Delete all files listed above
2. Commit: `git add -A && git commit -m "refactor: delete Yautja-only XML def files"`

---

### Task 6: Edit shared XML files to remove Yautja references

**Objective:** Remove Yautja-specific defs and references from shared XML files.

**Files to edit (the ones that grep found Yautja references in non-Yautja-named files):**

Each of these ~56 files needs to be inspected and Yautja-specific XML elements removed. Key examples:

- `Defs/AbilityDefs/Abilities_Base.xml` — remove Yautja abilities
- `Defs/AbilityDefs/Abilities_MeleeWeaponRanged.xml` — remove Yautja melee weapon abilities
- `Defs/AbilityDefs/Abilities_RangedAttack.xml` — remove Yautja ranged abilities
- `Defs/AbilityDefs/RRY_Abilities_Grenades.xml` — remove Yautja grenade abilities
- `Defs/BackstoryDefs/Backstories_Humanoid.xml` — remove Yautja backstories
- `Defs/HediffDefs/Hediffs_Special_Effects.xml` — remove `RRY_Hediff_Cloaked` and `RRY_Hediff_BouncedProjectile` defs
- `Defs/HediffDefs/Hediffs_Local_Infections.xml` — remove Yautja hediffs
- `Defs/HediffDefs/Hediffs_Local_Injuries.xml` — remove Yautja hediffs
- `Defs/HediffDefs/Hediffs_Xenomorph_Special_Effects.xml` — remove Yautja hediff references
- `Defs/HediffDefs/Hediffs_Artificials.xml` — remove Yautja hediff references
- `Defs/HediffGivers/HediffGiverSets.xml` — remove Yautja hediff giver sets
- `Defs/Misc/BodyTypeDefs/BodyTypes.xml` — remove RRYYautjaFemale/Male body types
- `Defs/Misc/Filth_Blood.xml` — remove Yautja blood filth defs
- `Defs/Misc/FleshTypeDefs/FleshType.xml` — remove Yautja flesh type
- `Defs/Misc/HairDefs/Hairdefs.xml` — remove Yautja hair defs
- `Defs/Misc/Items_Resource_Stuff_Leather.xml` — remove RRY_Leather_Rynath
- `Defs/Misc/PawnsArrivalModeDefs/PawnsArrivalModes.xml` — remove EdgeWalkInGroups if Yautja-only
- `Defs/PawnKindDefs/PawnKinds_NPC_Special.xml` — remove Yautja special pawn kinds
- `Defs/RecipeDefs/Recipe_Surgery.xml` — remove Yautja surgery recipes
- `Defs/ResearchProjectDefs/RRYResearch.xml` — remove Yautja research projects
- `Defs/ResearchProjectDefs/RRYResearchTabs.xml` — remove Yautja research tab
- `Defs/Scenarios/ScenParts_Various.xml` — remove Yautja scenario parts
- `Defs/Storyteller/Incidents_Map_CrashedShipParts.xml` — remove Yautja incident references
- `Defs/Storyteller/Incidents_Map_Misc.xml` — remove Yautja incident references
- `Defs/Storyteller/Incidents_Map_Special.xml` — remove Yautja incident references
- `Defs/Storyteller/Incidents_Map_Threats.xml` — remove Yautja incident references
- `Defs/ThingCategoryDefs/RRY-ThingCategories.xml` — remove Yautja thing categories
- `Defs/ThingDefs_Buildings/Buildings_Furniture.xml` — remove Yautja furniture
- `Defs/ThingDefs_Buildings/Buildings_Misc.xml` — remove Yautja buildings
- `Defs/ThingDefs_Buildings/Buildings_Production.xml` — remove Yautja production buildings
- `Defs/ThingDefs_Buildings/Buildings_TrapsRearmable.xml` — check if Yautja-related
- `Defs/ThingDefs_Buildings/USCM_Buildings_Exotic.xml` — remove Yautja references
- `Defs/ThingDefs_Buildings/USCM_Ethereal_Skyfallers.xml` — remove Yautja references
- `Defs/ThingDefs_Misc/RRY_Ethereal_Various.xml` — remove Yautja ethereal defs
- `Defs/ThingDefs/Neomorph_Plants_Base.xml` — remove Yautja references
- `Defs/ThingDefs/Neomorph_Races.xml` — remove Yautja references
- `Defs/ThingDefs/Synth_Race.xml` — remove Yautja references
- `Defs/ThingDefs/USCM_Weapons_Ranged*.xml` — remove Yautja weapon references
- `Defs/ThingDefs/Xenomorph_Egg.xml` — remove Yautja references
- `Defs/ThingDefs/Xenomorph_Races.xml` — remove Yautja references
- `Defs/ThoughtDefs/Thoughts_Situation_Social.xml` — remove Yautja thoughts
- `Defs/TraitDefs/Traits_Singular.xml` — remove Yautja traits
- `Defs/TraitDefs/Traits_Spectrum.xml` — remove Yautja traits
- `Defs/TutorDefs/Concepts_Xenomorph.xml` — remove Yautja concept references
- `Defs/WorkGiverDefs/WorkGivers.xml` — remove Yautja work givers
- `Defs/DutyDefs/Xenomorph_Duties_Misc.xml` — remove Yautja duty references
- `Defs/Maneuvers/Maneuvers.xml` — remove Yautja maneuver references
- `Defs/DamageDefs/Damages_USCM_Ranged.xml` — remove Yautja damage references
- `Defs/Stats/Stats_Apparel.xml` — remove Yautja stat references
- `Defs/Stats/Stats_Basics_General.xml` — remove Yautja stat references
- `Defs/Stats/Stats_Pawns_General.xml` — remove Yautja stat references
- `Defs/Stats/Stats_Stuff.xml` — remove Yautja stat references

**Patch files to edit:**
- `Patches/AlienFaces.xml` — remove Yautja face patches
- `Patches/Appearance_Clothes_Patch.xml` — remove Yautja apparel patches
- `Patches/FacehuggerDrawData_Patch.xml` — remove Yautja references
- `Patches/HediffGiverPatch.xml` — remove Yautja hediff giver patches
- `Patches/MarkDrawData_Patch.xml` — remove HunterMarkingSystem patches (Yautja-only)
- `Patches/StrangerinBlack_Patch.xml` — remove Yautja references

**Steps:**
1. For each file, read it, find all Yautja-related XML elements, and remove them
2. Be careful to only remove Yautja-specific elements, not shared/USCM/Xenomorph elements
3. Commit in logical groups (e.g., "remove Yautja defs from HediffDefs", "remove Yautja defs from ThingDefs")

---

### Task 7: Delete Yautja textures

**Objective:** Remove all Yautja texture directories and files.

**Directories to delete:**
- `Textures/Things/Apparel/Yautja_Armour/`
- `Textures/Things/Apparel/Yautja_Armour_Helmet/`
- `Textures/Things/Apparel/Yautja_Armour_Helmet_Falconer/`
- `Textures/Things/Apparel/Yautja_Cloak/`
- `Textures/Things/Apparel/Yautja_Elite_Armour/`
- `Textures/Things/Apparel/Yautja_Elite_Armour_Helmet/`
- `Textures/Things/Apparel/Yautja_Gauntlet/`
- `Textures/Things/Apparel/Yautja_Heavy_Armour/`
- `Textures/Things/Apparel/Yautja_Leader_Armour/`
- `Textures/Things/Apparel/Yautja_Leader_Armour_Helmet/`
- `Textures/Things/Apparel/Yautja_Light_Armour/`
- `Textures/Things/Apparel/Yautja_Light_Armour_Helmet/`
- `Textures/Things/Apparel/Yautja_Tribalwear/`
- `Textures/Things/Apparel/Yautja_Undersuit/`
- `Textures/Things/Apparel/Yautja_Vest/`
- `Textures/Things/Buildings/Yautja/`
- `Textures/Things/Pawn/Yautja/`
- `Textures/Things/Addons/Blood_Marks/Yautja_Kills/`

**Individual files to delete:**
- `Textures/Things/Equipment/Melee/Yautja_Combistaff_New.png`
- `Textures/Things/Equipment/Melee/Yautja_Combistaff.png`
- `Textures/Things/Equipment/Melee/Yautja_HandBlade.png`
- `Textures/Things/Equipment/Melee/Yautja_MetalBladedMaul.png`
- `Textures/Things/Equipment/Ranged/Yautja_Compound_Bow_m.png`
- `Textures/Things/Equipment/Ranged/Yautja_Compound_Bowm.png`
- `Textures/Things/Equipment/Ranged/Yautja_Compound_Bow.png`
- `Textures/Things/Equipment/Ranged/Yautja_Hunting_Bow_m.png`
- `Textures/Things/Equipment/Ranged/Yautja_Hunting_Bowm.png`
- `Textures/Things/Equipment/Ranged/Yautja_Hunting_Bow.png`
- `Textures/Things/Equipment/Ranged/Yautja_Sniper_Spear_Rifle.png`
- `Textures/Things/Equipment/Yautja_Combistaff_Old.png`
- `Textures/Things/Equipment/Yautja_ShoulderCannon*.png` (all variants)
- `Textures/Things/Projectile/Yautja_Combistaff_New.png`
- `Textures/Things/Projectile/Yautja_Combistaff.png`
- `Textures/Things/Pawn/Humanlike/Apparel/Pants/Pants_RRYYautja*.png` (all)
- `Textures/Things/Pawn/Humanlike/Apparel/TribalA/TribalA_RRYYautja*.png` (all)
- `Textures/Ui/Icons/Icon_YautjaB.png`
- `Textures/Ui/Icons/Icon_Yautja_JungleClan.png`
- `Textures/Ui/Icons/Icon_Yautja.png`
- `Textures/Yautja_Sarcophagus.zip`

**Steps:**
1. Delete all directories and files listed above
2. Commit: `git add -A && git commit -m "refactor: delete Yautja textures"`

---

### Task 8: Delete Yautja language/namebank files

**Objective:** Remove Yautja name bank files.

**Files to delete:**
- `Languages/English/Strings/NameBanks/RRYautjaBaseNames.txt`
- `Languages/English/Strings/NameBanks/RRYautjaClanJungle.txt`
- `Languages/English/Strings/NameBanks/RRYautjaClans.txt`
- `Languages/English/Strings/NameBanks/RRYautjaFemaleA.txt`
- `Languages/English/Strings/NameBanks/RRYautjaFemaleB.txt`
- `Languages/English/Strings/NameBanks/RRYautjaFemale.txt`
- `Languages/English/Strings/NameBanks/RRYautjaMaleA.txt`
- `Languages/English/Strings/NameBanks/RRYautjaMaleB.txt`
- `Languages/English/Strings/NameBanks/RRYautjaMale.txt`
- `Languages/English/Strings/NameBanks/RRYautjaNickFemale.txt`
- `Languages/English/Strings/NameBanks/RRYautjaNick.txt`

**Also edit:**
- `Languages/English/Keyed/RRY_Keys.xml` — remove Yautja-specific translation keys
- `Languages/English/Keyed/HMS_Keys.xml` — remove entirely (HunterMarkingSystem keys)

**Steps:**
1. Delete all name bank files
2. Edit RRY_Keys.xml to remove Yautja keys
3. Delete HMS_Keys.xml
4. Commit: `git add -A && git commit -m "refactor: delete Yautja language and namebank files"`

---

### Task 9: Edit keyed language files and remove HMS keys

**Objective:** Clean up language files.

This is covered in Task 8 above.

---

### Task 10: Remove Yautja references from .csproj files

**Objective:** Ensure the .csproj files don't reference deleted files.

**Files:**
- `Source/RRYautja/RRYautja.csproj` — remove HunterMarkingSystem ProjectReference (done in Task 3), check for any explicit Compile items pointing to deleted files
- Check if any .csproj has explicit `<Compile Include>` for Yautja files (the csproj uses SDK-style with globbing, so likely no explicit includes needed)

**Steps:**
1. Read RRYautja.csproj and check for any Yautja file references
2. Remove any found
3. Commit: `git add -A && git commit -m "refactor: clean up csproj references"`

---

### Task 11: Attempt compilation and fix errors iteratively

**Objective:** Build the solution and fix any remaining compilation errors.

**Steps:**
1. `cd ~/AVP-Rimworld/Source/RRYautja && dotnet build -c Release`
2. For each error, identify the file, determine if it's a Yautja reference that was missed, and fix it
3. Repeat until clean build
4. Also build `Source/PawnShields/`, `Source/TrapsRearmable/`, `Source/ResourceBoxes/` to ensure they still compile
5. Commit: `git add -A && git commit -m "fix: resolve compilation errors after Yautja removal"`

---

### Task 12: Copy built DLLs to Assemblies/

**Objective:** Ensure the Assemblies folder has the latest built DLLs.

**Steps:**
1. Copy `Source/RRYautja/bin/Release/RRYautja.dll` to `Assemblies/RRYautja.dll`
2. Ensure `Assemblies/HunterMarkingSystem.dll` is already deleted
3. Commit: `git add -A && git commit -m "chore: update built assemblies after Yautja removal"`

---

### Task 13: Final audit — grep for remaining Yautja references

**Objective:** Verify no Yautja references remain.

**Steps:**
1. `grep -rn "Yautja\|yautja\|RRYYautja\|HunterMarkingSystem\|HMSUtility\|HMSDefOf\|YautjaDefOf\|Cloakgen\|Hediff_Cloak\|RRY_Hediff_Cloaked\|SmartDisk\|Combistaff\|ShoulderCannon\|Plasmacaster\|HunterGauntlet\|Wristblade\|HealthShard\|BloodedM\|Unblooded\|BadBlood\|RRY_Rynath\|RRY_Leather_Rynath" Defs/ Patches/ Source/ Textures/ Languages/ --include="*.xml" --include="*.cs" --include="*.txt" 2>/dev/null | grep -v "obj/" | grep -v ".git/" | grep -v ".vs/"`
2. For each remaining hit, determine if it's a false positive or a missed reference
3. Fix any missed references
4. Commit: `git add -A && git commit -m "fix: final cleanup of remaining Yautja references"`

---

### Task 14: Push branch

**Objective:** Push the removal branch to remote.

**Steps:**
1. `git push -u origin removal/remove-yautja`
2. Report the branch name to the user

---

## Risks and Tradeoffs

1. **Xenomorph hidden/invisibility system** shares `Graphic_Invisible` and `PawnGraphicSet_Invisible` with Yautja cloaking. We're keeping these utility classes, so Xenomorph invisibility should still work.

2. **USCM Comp_Stealth** uses `Graphic_Invisible` and `RRYautja.ExtensionMethods`. After removing Yautja-specific extension methods from AvPExtensions.cs, verify Comp_Stealth still compiles.

3. **Shared Harmony patches** that check both `RRY_Hediff_Cloaked` (Yautja) and `RRY_Hediff_Xenomorph_Hidden` (Xenomorph) need to have only the cloak check removed, keeping the Xenomorph hidden check.

4. **The mod may lose some functionality** that was unexpectedly shared — e.g., if some USCM weapons had Yautja-specific ammo types or abilities that were defined in Yautja XML files.

5. **Save game compatibility** — existing saves with Yautja pawns/items will have broken references. This is expected for a content removal mod split.

6. **About.xml** may need updating to reflect the removed content and potentially rename the mod.

---

## Verification

After all tasks are complete:
1. Solution compiles with 0 errors
2. `grep -rn "Yautja" . | grep -v ".git/"` returns no hits (or only documentation comments)
3. The mod loads in RimWorld 1.6 without errors (user will need to test on Windows)
4. Xenomorph and USCM content functions normally