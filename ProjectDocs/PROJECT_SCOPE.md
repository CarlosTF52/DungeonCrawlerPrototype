# Dungeon Crawler Prototype - Project Scope

## North Star

A grim fantasy survival-horror dungeon crawler inspired by the expedition pressure of Darkest Dungeon, but played through action 3D combat. The player leaves a fragile village hub, enters dangerous cursed spaces, survives deliberate encounters, returns with loot or survivors, and uses those gains to rebuild, upgrade, and push deeper.

## Prototype Goal

Build one compelling playable loop before expanding systems or content.

The prototype should answer:

- Is it tense to leave the hub and enter the dungeon?
- Is combat methodical, readable, and satisfying?
- Is returning to the village with loot or a rescued survivor rewarding?
- Does one upgrade make the player want to do another run?


## Reference Direction

The project should draw inspiration from Darkest Dungeon's first-game structure and mood: dangerous expeditions, preparation pressure, attrition, survivor management, stress, dread, and a hub that matters between runs.

This is a reference for direction, not a clone target. The prototype should translate that kind of expedition pressure into 3D action combat, spatial exploration, horror atmosphere, and hands-on survival decisions.

For the first prototype, prioritize expedition pressure before deep roster management or complex affliction systems.

## Core Loop

1. Start in the village hub.
2. Prepare with basic gear and limited supplies.
3. Enter one dungeon route.
4. Explore a compact sequence of rooms.
5. Fight or avoid dangerous enemies.
6. Find loot, a relic, or one survivor.
7. Return to the village.
8. Decide what to deposit in the physical village bank and what to carry for dungeon merchants.
9. Spend banked loot on one meaningful upgrade.
10. Repeat with slightly higher danger.

## Initial Pillars

- The hub is hope.
- The dungeon is pressure.
- Combat is deliberate.
- Loot tells stories.
- Return matters.

## Prototype Includes

- One village hub.
- One dungeon.
- One player character/controller.
- One weapon type.
- Two regular enemy types.
- One high-threat enemy that encourages fear or avoidance.
- One rescued survivor.
- One upgrade station.
- Basic loot collection.
- Basic health, stamina, and healing.
- A return/extraction flow.

## Prototype Excludes

- Large open world.
- Full quest system.
- Complex dialogue trees.
- Full crafting tree.
- Multiple playable classes.
- Procedural world generation.
- Base-building depth.
- Large weapon roster.
- Companion AI.
- Full save/progression system beyond what is needed to test the loop.

## Target Perspective

Default recommendation: third-person.

Reason: third-person better showcases models, animations, armor, enemy silhouettes, and deliberate melee combat. First-person can be revisited if horror immersion becomes more important than animation readability.
## Multiplayer Direction

Possible 2-player co-op is an architectural consideration, not an active Milestone 1 feature.

Near-term systems should avoid hard-coding assumptions that make co-op expensive later, such as assuming there is only one player, one target, one inventory owner, or one body that can trigger expedition flow.

Networking, lobbies, replication, host/client authority, matchmaking, and full co-op gameplay are out of scope until the single-player expedition loop is fun.

