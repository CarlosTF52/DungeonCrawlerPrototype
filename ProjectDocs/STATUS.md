# Project Status

## Current Phase

Prototype implementation.

## Current Focus

Milestone 1: One Good Run. The project now has scene travel, action combat, expedition state, banked/carried currency, roster-based character choice, HUD feedback, a first procedural/inspector-authored expedition map flow, and a first enemy archetype. The next focus is proving one complete authored expedition route end-to-end with a selected character, carried resources, slime threat, extraction, and hub return.

## Latest Decisions

- This task is the management hub for scope and overall status.
- Feature implementation should happen in separate feature-specific tasks.
- The project direction is a Darkest Dungeon-inspired fantasy survival-horror dungeon crawler with action 3D combat and a village hub.
- Prototype first, expand only after the loop is fun.
- First-person is currently being used for the playable prototype; third-person remains a possible later direction if animation readability or character presentation becomes more important.
- The GitHub default branch will remain `master`.
- For Milestone 1, prioritize expedition pressure before deep roster management, afflictions, crafting, or hub-building depth.
- Procedural generation starts as a Darkest Dungeon-style room graph/route, not full 3D procedural room placement.
- Keep Unity `Assets/_Recovery/` files ignored unless intentionally restoring from them.
- Possible 2-player co-op should be considered architecturally, but multiplayer implementation is not part of Milestone 1.
- Deeper combat is paused, but enemy archetype work is active enough to support the expedition loop.

## Repository Status

- Repository: `CarlosTF52/DungeonCrawlerPrototype`
- Branch: `master`
- Latest pushed commit before today's wrap-up: `93896db Add procedural expedition map`
- Today's village/enemy/doc changes are being prepared for commit and push.

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
- Currency now has two important layers: player-carried pouch and safe village bank. Extraction moves run loot into the carried pouch; the player must use a physical bank interaction to secure or withdraw money.
- Currency HUD supports TextMeshPro and can display current expedition loot, player pouch, or village bank totals.
- Village/tavern systems now include character roster assets, runtime character selection, starter Warden/Scout/Occultist characters, player stat application, and roster HUD/debug display.
- Procedural Expedition Map v1 is started and working: expeditions can use a generated/defined room route with room type, danger, enemy count, pickup count, trap count, objective flag, and extraction flag.
- Inspector-authored expedition routes now work and are preferred for near-term design control.
- `ExpeditionRunManager.Instance` was fixed to search for an existing scene manager before creating a blank runtime manager, preventing Inspector route data from being overwritten by initialization order.
- Current room content activation uses pre-placed scene objects, enabling/disabling room contents based on the active generated room. It does not spawn prefabs yet.
- Enemies now have their own feature slice. First archetype is a physics-based slime launcher with ScriptableObject attack profile, readable windup, lunge damage window, cancel-on-hit, visual mesh deformation, and safer post-hit recovery.

## Multiplayer Architecture Notes

- Prefer future names and APIs like participant, party, interacting player, and shared expedition state over single-player-only assumptions when it is cheap to do so.
- Do not add networking packages yet.
- Keep current Milestone 1 focused on proving the single-player expedition loop.

## Next Actions

- Play one complete authored route using a selected character: choose character, choose carried money, enter dungeon, face slime, collect objective/loot, extract, return to hub, bank or risk resources.
- Replace `VillageBankDebugSpend` with one concrete prototype upgrade, such as +max health, +max stamina, or +weapon damage.
- Decide whether failed expeditions should risk carried pouch money, and if so under what rules.
- Add simple hub feedback after extraction: show banked gold/relics, carried money, active character, and last run payout without relying only on debug HUD.
- Keep `ExpeditionGateway` for expedition-state transitions and plain scene-transition scripts only for non-run doors.
- Tune slime attack rhythm: range, windup, launch duration, recovery, interrupt window, and collision layers.

## Risks

- The loop has many working pieces, but the full authored route still needs one deliberate end-to-end playtest.
- Character roster systems currently provide selection/stat variation, but stress/injury consequences are placeholders.
- The money-risk rule is not final: carried pouch exists, but whether failure should lose carried money remains a design decision.
- Inspector-authored routes are good for control, but the system should avoid becoming too dependent on one reusable room shell forever.
- Debug HUDs and debug spend hooks are useful now but should not become permanent design foundations.
- Importing too many Asset Store packages too early may slow iteration.
- Mixing low-poly and realistic assets may create visual inconsistency.
- Large systems can pull the project away from the core loop before it is fun.
- First-person melee needs strong hit feedback and spacing to avoid feeling floaty or unfair.
- Sample/demo content from packages may clutter the repo if not separated from prototype-critical assets.
