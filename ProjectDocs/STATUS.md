# Project Status

## Current Phase

Prototype implementation.

## Current Focus

Milestone 1: One Good Run. The immediate focus is proving a complete hub-to-dungeon-to-hub expedition loop with Darkest Dungeon-style pressure translated into action 3D horror combat.

## Latest Decisions

- This task is the management hub for scope and overall status.
- Feature implementation should happen in separate feature-specific tasks.
- The project direction is a Darkest Dungeon-inspired fantasy survival-horror dungeon crawler with action 3D combat and a village hub.
- Prototype first, expand only after the loop is fun.
- First-person is currently being used for the playable prototype; third-person remains a possible later direction if animation readability or character presentation becomes more important.
- The GitHub default branch will remain `master`.
- For Milestone 1, prioritize expedition pressure before deep roster management, afflictions, crafting, or hub-building depth.

## Repository Status

- Repository: `CarlosTF52/DungeonCrawlerPrototype`
- Branch: `master`
- Latest known synced commit: `648a51d Project with player and scene management`
- Current management-doc and expedition-loop updates are local/uncommitted unless pushed after this status update.

## Active Milestone

Milestone 1 - One Good Run.

## Progress Notes

- Milestone 0 setup is effectively complete.
- Unity project exists and is connected to GitHub.
- ProjectDocs exists and is being used for management tracking.
- Scene Management v2 is functionally complete: persistent player, named spawn points, bidirectional travel, and interact-to-enter doors work.
- Combat v1 is functionally working: damage, enemy health/death, enemy chase, contact damage, invincibility frames, knockback/stagger, input actions, Animancer swing playback, and timed weapon hitbox.
- Expedition Loop v1 has started: run manager, gateways, loot pickups, objective triggers, outcome enum, and debug HUD scripts exist.
- Expedition pickups/objectives were tested and fixed by ensuring the run starts through `ExpeditionGateway` instead of plain scene transition triggers.
- Extracted gold/relics are tracked for the current runtime session, but are not yet banked into persistent village inventory/progression.

## Next Actions

- Create `VillageInventory` or `HubProgressionManager` to bank extracted loot after successful extraction.
- Add one temporary hub feedback element that shows returned loot or expedition outcome.
- Verify the full One Good Run path in Unity: start in hub, begin expedition, collect loot/objective, extract, return to hub, see returned value.
- Keep expedition gateways separate from plain scene-transition doors: use `ExpeditionGateway` for start/descend/extract/fail/abandon.
- Decide whether to commit the current `ProjectDocs` and `Assets/Scripts/Expedition` changes.
- Watch imported sample/demo content so prototype iteration stays light.

## Risks

- Loot currently survives extraction only on the run manager until the next expedition reset; without banking, the hub progression loop is not real yet.
- Importing too many Asset Store packages too early may slow iteration.
- Mixing low-poly and realistic assets may create visual inconsistency.
- Large systems can pull the project away from the core loop before it is fun.
- First-person melee needs strong hit feedback and spacing to avoid feeling floaty or unfair.
- Sample/demo content from packages may clutter the repo if not separated from prototype-critical assets.
