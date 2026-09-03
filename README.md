# AVP Xenomorphs — RimWorld 1.6

Xenomorphs and Neomorphs from the Alien film franchise, ported from the original Alien vs Predator mod to RimWorld 1.6 as a Xenomorphs-only mod (no Yautja/USCM dependencies, no HAR required).

> *"Perfect organism. Its structural perfection is matched only by its hostility."*

The Xenomorphs join RimWorld as a new threat to your colony. Experience the full life cycle of the Xenomorphs. Learn to fear their infestations. Be wary of those spaceship parts that fall down — you never know what they may contain.

## Credits

### Original AVP Mod Developers
- **Ogliss** — Coding, XML, mechanics, Harmony patches, original vision
- **acide_bob** — XML & balancing
- **Rebelrot96** — Art, textures, concepts
- **Deon** — Contributions

### 1.6 Port & Xenomorphs-Only Fork
- **RhombusMaximus** — 1.6 migration, Yautja removal, bug fixes, performance optimization, new features, ongoing maintenance

### Special Thanks
- The RimWorld modding community for 1.5/1.6 API documentation
- Ludeon Studios for an incredible game
- Everyone who tested and reported bugs during development

## Pawns

**Xenomorphs:**
- **Queen** — The hive ruler. Lays eggs, commands the hive, devastating in combat
- **Drone** — Worker caste. Mines, builds hive structures, spits acid, collects hosts. Dark and Gold texture variants
- **Warrior** — Soldier caste. Tough, fast, deadly in melee. Explodes in acid blood on death
- **Runner** — Scout caste. Fast and agile, spawned from animal hosts. Can leap at targets
- **Facehugger** — Impregnates hosts by attaching to their face. Visual mask shown on infected pawns. Royal facehuggers can produce Queens
- **Chestburster** — Newborn form. Ravenous, grows rapidly through life stages
- **Predalien** — Dormant (no Yautja hosts yet). Kept for future content

**Neomorphs:**
- **Neomorph** — Spore-born cousin of the Xenomorph. Separate infection vector via Neomorph spores

**Special:**
- **Thrumbomorph** — Rare Xenomorph variant spawned from Thrumbos

## Features

### Life Cycle
- **Egg → Facehugger → Chestburster → Adult** — Full Xenomorph life cycle
- Eggs hatch when potential hosts walk within range (7 tiles)
- Hatch probability scales with host bodySize (0.5 minimum, larger hosts trigger easier)
- Facehuggers prioritize downed targets (5x score) and animals (3x score)
- Spawn type is data-driven via XML (`XenomorphSpawnDef`):
  - Humanlike hosts → 50% Drone / 50% Warrior
  - Small animals (bodySize < 0.9) → Runner
  - Thrumbo hosts → Thrumbomorph
  - Royal facehugger → Queen
  - Large animals → weighted random (Drone/Warrior/Runner)
  - Predalien impregnation → Runner/Drone (dormant, no Yautja hosts)

### Hive System
- Xenomorph tunnels spawn in caves and infestations
- Hives expand by creating child hives and resin walls
- Drones mine outward from the hive to expand territory
- Drones return to tunnel after mining; dig through walls if blocked
- Drones don't trap themselves behind their own resin walls
- Queen lays eggs near the hive
- Hive inspect panel shows total Xenomorph count by type
- Hives activate when discovered (unfogged)
- Hives go dormant without hosts nearby

### Combat
- **Acidic Blood** — Xenomorph blood damages everything in the same tile for a short period after being dropped by a LIVING Xenomorph
- **Acid Spit** — Drones can spit acid at range
- **Leap Ability** — Facehuggers and Runners can leap at targets (2-8 tile range, cooldown)
- **EMP Stun** — Xenomorphs can be stunned by EMP weapons
- **Cold Stasis** — Xenomorphs slow down in extreme cold (~-32F / Hypothermic Slowdown)
- **Self-Preservation** — Adult Xenomorphs flee when health drops below 30%
- **Kidnapping** — Xenomorphs prioritize kidnapping downed hosts during raids (first priority over combat)
- **Night Raids** — Xenomorph raids only happen at night
- **No Building Destruction** — Xenomorphs don't attack colony buildings during normal raids (Power Cut event exempt)
- **Buffed Stats** — Xenomorphs are strong enough to compete with vanilla insect hives

### Food System
- Xenomorphs have a very slow metabolism (baseHungerRate = 0.003)
- **Newborns** are ravenous (5x hunger) — hunt and eat immediately
- **Shedlings** very hungry (3x)
- **Young** hungry (2x)
- **Adolescents** slowing down (0.5x)
- **Adults** barely eat (0.1x) — months before starving
- Eat corpses and hunt animals (CarnivoreAnimalStrict + Corpse)

### Facehugger Mask
- Visible facehugger mask on infected pawns using native RimWorld 1.5+ render nodes
- Royal facehugger shows a different mask texture
- Automatically appears when infected, removes when facehugger detaches
- Scale, offset, and flip tuned for all body types and rotation directions

### Performance Optimizations
- Job giver tick throttle: all job givers run every 3rd tick (staggered by pawn hash)
- Hunting range limits: Hosthunter (40 tiles), Kidnap (20 tiles default, 12 for hive defense)
- Facehugger hunting disabled while attached to a host
- Null-map safety checks throughout the think tree
- WaitAutoAttack patch despawns broken facehuggers with null maps
- Mining jobs have expiry intervals (3000 ticks) to prevent getting stuck

### Hidden Infections
- Small chance any spawned pawn has a hidden impregnation (toggleable in mod settings)

## Incidents

- **Xenomorph Infestation** — Tunnels spawn in caves, expand into a hive. Don't wait too long to deal with them
- **Crashed Ship Part** — A spaceship part crashes containing Xenomorphs. Bring a strong squad
- **They Mostly Come at Night** — Xenomorph raids only fire at night (sky brightness < 0.3)
- **Neomorph Spores** — Neomorph spore plants that infect pawns. Hint: flamers may be required
- **Power Cut** — Xenomorphs assault the colony to cut power (event-specific, allows building attacks)

## Mod Settings

- Enable/Disable Xenomorph faction
- Allow/Disallow hidden impregnations
- Allow/Disallow Predalien impregnations
- Allow/Disallow non-humanlike hosts (animals)
- Allow cocooned pawns to convert to eggs if no Queen present
- Host kind counts (suitable/unsuitable hosts display)
- Debug mode for detailed logging

## Compatibility

- **No mod dependencies required** — works standalone with RimWorld 1.6
- HugsLib compatible (optional, for additional logging)
- **Melee Animation** compatible — includes WeaponTweakData for Xenomorph weapons
- **Replace Stuff** compatible — tunnel buildings have dummy textures for ghost rendering
- No HAR (Humanoid Alien Races) dependency — uses vanilla Pawn class
- Compatible with most race mods (Xenomorphs use vanilla rendering)
- Android/robot pawns are excluded as facehugger hosts (won't impregnate synthetics)

## Requirements

- RimWorld 1.6
- No other mods required

## Based On

Original mod: [Alien vs Predator](https://steamcommunity.com/sharedfiles/filedetails/?id=2034103876) by Ogliss, acide_bob, Rebelrot96, Deon

## Bug Report Template

When reporting bugs, please include:
1. RimWorld version
2. Full mod list (or at least relevant mods)
3. The error from the debug log (press ` to open console)
4. What was happening when the bug occurred
5. Save file if possible

## Known Limitations

- Predalien is dormant (no Yautja hosts yet — planned for future update)
- Faction is hidden (not selectable at game start — by design)
- Old saves from pre-1.6 versions may have compatibility issues
- `RRYautja` namespace is legacy (rename to `RRXenomorphs` planned for future)

## License

This is a fan mod based on the Alien franchise. All Xenomorph concepts are property of their respective owners. This mod is free and non-commercial.