# Project Status

## Current Phase

Prototype implementation.

## Current Focus

Milestone 1: One Good Run. The project now has scene travel, action combat, expedition state, banked rewards, HUD feedback, and a first procedural/inspector-authored expedition map flow. The next focus is proving one complete authored expedition route end-to-end, then turning debug spending into one real upgrade.

## Latest Decisions

- This task is the management hub for scope and overall status.
- Feature implementation should happen in separate feature-specific tasks.
- The project direction is a Darkest Dungeon-inspired fantasy survival-horror dungeon crawler with action 3D combat and a village hub.
- Prototype first, expand only after the loop is fun.
- First-person is currently being used for the playable prototype; third-person remains a possible later direction if animation readability or character presentation becomes more important.
- The GitHub default branch will remain `master`.
- For Milestone 1, prioritize expedition pressure before deep roster management, afflictions, crafting, or hub-building depth.
- Procedural generation starts as a Darkest Dungeon-style room graph/route, not full 3D procedural room placement.
- Keep Unity `_Recovery` files out of commits unless intentionally restoring from them.
- Pause deeper combat work for now; the combat foundation is good enough to support expedition-loop testing.

## Repository Status

- Repository: `CarlosTF52/DungeonCrawlerPrototype`
- Branch: `master`
- Latest pushed commit previously recorded: `cb25e9c Add combat, HUD, and expedition systems`
- Today's procedural map and management-doc updates may still be local unless committed/pushed after this wrap-up.

## Active Milestone

Milestone 1 - One Good Run.

## Progress Notes

- Milestone 0 setup is effectively complete.
- Unity project exists and is connected to GitHub.
- ProjectDocs exists and is being used for management tracking.
- Scene Management v2 is functionally complete: persistent player, named spawn points, bidirectional travel, and interact-to-enter doors work.
- Combat v1 is functionally working: damage, enemy health/death, enemy chase, contact damage, invincibility frames, knockback/stagger, input actions, Animancer swing playback, timed weapon hitbox, stamina, and basic health/stamina HUD.
- Enemy contact can shove the player back through the Starter Assets `FirstPersonController` external velocity path, avoiding Rigidbody/CharacterController conflicts.
- Expedition Loop v1 is in place: run manager, gateways, loot pickups, objective triggers, outcome enum, debug HUD, village bank, bank debug HUD, and debug spend hook exist.
- Extracted gold/relics deposit into `VillageBank` on successful extraction.
- Currency HUD supports TextMeshPro and can display either current expedition loot or village bank totals.
- Procedural Expedition Map v1 is started and working: expeditions can use a generated/defined room route with room type, danger, enemy count, pickup count, trap count, objective flag, and extraction flag.
- Inspector-authored expedition routes now work and are preferred for near-term design control.
- `ExpeditionRunManager.Instance` was fixed to search for an existing scene manager before creating a blank runtime manager, preventing Inspector route data from being overwritten by initialization order.
- Current room content activation uses pre-placed scene objects, enabling/disabling room contents based on the active generated room. It does not spawn prefabs yet.

## Next Actions

- Commit/push today's procedural map and documentation updates if not already done.
- Play one complete authored route: Entrance -> Combat/Loot -> Objective -> Extraction -> return to hub with banked reward.
- Replace `VillageBankDebugSpend` with one concrete prototype upgrade, such as +max health, +max stamina, or +weapon damage.
- Add simple hub feedback after extraction: show banked gold/relics and last run payout without relying only on debug HUD.
- Keep `ExpeditionGateway` for expedition-state transitions and plain scene-transition scripts only for non-run doors.
- Consider a small repo cleanup pass for Unity recovery files and sample/demo content before the project gets larger.

## Risks

- The loop has many working pieces, but the full authored route still needs one deliberate end-to-end playtest.
- Inspector-authored routes are good for control, but the system should avoid becoming too dependent on one reusable room shell forever.
- Debug HUDs and debug spend hooks are useful now but should not become permanent design foundations.
- Importing too many Asset Store packages too early may slow iteration.
- Mixing low-poly and realistic assets may create visual inconsistency.
- Large systems can pull the project away from the core loop before it is fun.
- First-person melee needs strong hit feedback and spacing to avoid feeling floaty or unfair.
- Sample/demo content from packages may clutter the repo if not separated from prototype-critical assets.
