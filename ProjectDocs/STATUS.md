# Project Status

## Current Phase

Prototype implementation.

## Current Focus

Milestone 1: One Good Run. The project now has scene travel, action combat, expedition state, banked/carried currency, roster-based character choice, cursed age mechanics, HUD feedback, a first procedural/inspector-authored expedition map flow, and a first enemy archetype. The next focus is proving one complete authored expedition route end-to-end with a selected character, carried resources, slime threat, defeat/extraction consequences, hub return, banking, and upgrades.

## Latest Decisions

- This task is the management hub for scope and overall status.
- Feature implementation should happen in separate feature-specific tasks.
- Nice-to-have future features are tracked separately in `ProjectDocs/FUTURE_FEATURES.md` so they do not accidentally expand Milestone 1.
- The project direction is a Darkest Dungeon-inspired fantasy survival-horror dungeon crawler with action 3D combat and a village hub.
- Prototype first, expand only after the loop is fun.
- First-person is currently being used for the playable prototype; third-person remains a possible later direction if animation readability or character presentation becomes more important.
- The GitHub default branch will remain `master`.
- For Milestone 1, prioritize expedition pressure before deep roster management, afflictions, crafting, or hub-building depth.
- Procedural generation starts as a Darkest Dungeon-style room graph/route, not full 3D procedural room placement.
- Keep Unity `Assets/_Recovery/` files ignored unless intentionally restoring from them.
- Possible 2-player co-op should be considered architecturally, but multiplayer implementation is not part of Milestone 1.
- Deeper combat is paused, but enemy archetype work is active enough to support the expedition loop.
- Characters are people first, builds second: runtime roster state now supports generated names, starting age, age-modified stats, status, injury/stress placeholders, and village job placeholders.

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
- Currency now has two important layers: player-carried pouch and safe village bank. Dungeon pickups immediately update the carried pouch; the player must use a physical bank interaction to secure or withdraw money.
- Player UIX v1 has started: `PlayerVitalsHud` supports smoothed health/stamina/sanity bars, optional TMP number labels, and warning colors; `CurrencyHud` supports TextMeshPro and can display current expedition loot, player pouch, or village bank totals; `PlayerCharacterHud` shows active character identity/condition on the persistent player UI; `ExpeditionInfoHud` provides a Tab-held TMP information box for room, depth, objective, carried loot, and extraction state.
- Village/tavern systems now include character roster assets, runtime character selection, starter Warden/Scout/Occultist character definitions, generated runtime names, player stat application, age-modified effective stats, and roster HUD/debug display.
- Upgrade Systems v1 is in place: modular upgrade definitions, a persistent village upgrade manager, a blacksmith interactable, a default weapon damage upgrade, and player weapon damage scaling through active-character progression. Character health is now roster-owned, persists across tavern swaps, and resting characters partially recover after another character extracts. Tavern recovery can now be upgraded through the village-owned Better Beds upgrade, the Resurrection Ward exposes recoverable fallen roster status from a physical trigger HUD, and the Cemetery memorializes permanently dead characters from its own trigger HUD.
- Character defeat v1 is in place in code: `PlayerCharacterDefeatHandler` can age the active character by `10 + overkill damage`, add injury severity, mark them fallen/dead, clear carried pouch currency, and fail the expedition.
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

- Play one complete authored route using a selected character: choose character, choose carried money, enter dungeon, face slime, collect objective/loot into the pouch, extract, return to hub, bank or risk resources.
- Test the blacksmith and tavern loop in the hub: bank dungeon currency, buy Sharpened Weapons on one character, swap characters, confirm only the upgraded character deals increased damage, buy Better Beds, and confirm injured resting characters recover more after another character extracts.
- Add `PlayerCharacterDefeatHandler` to the player prefab/object and test defeat from a slime or debug damage source.
- Add simple hub feedback after extraction: show banked gold/relics, carried money, active character, and last run payout without relying only on debug HUD.
- Build the player canvas layout for UIX v1: wire `PlayerVitalsHud`, `CurrencyHud`, `PlayerCharacterHud`, and `ExpeditionInfoHud` to TextMeshPro/Image objects, then disable matching debug HUDs once the real panels are comfortable.
- Keep `ExpeditionGateway` for expedition-state transitions and plain scene-transition scripts only for non-run doors.
- Tune slime attack rhythm: range, windup, launch duration, recovery, interrupt window, and collision layers.

## Risks

- The loop has many working pieces, but the full authored route still needs one deliberate end-to-end playtest.
- Village jobs are still placeholders; stress values now start at 0 each session, default to a tunable max of 10, have roster-owned max/current state, use a player sanity/stress bar, and follow a first temporary damage rule where health lost also adds stress 1:1.
- The money-risk rule now has a first implementation for character defeat: carried pouch currency is lost. Abandoned expedition rules may still need separate tuning.
- Inspector-authored routes are good for control, but the system should avoid becoming too dependent on one reusable room shell forever.
- Debug HUDs and debug spend hooks are useful now but should not become permanent design foundations; blacksmith upgrades are the first real replacement for raw debug spending.
- Importing too many Asset Store packages too early may slow iteration.
- Mixing low-poly and realistic assets may create visual inconsistency.
- Large systems can pull the project away from the core loop before it is fun.
- First-person melee needs strong hit feedback and spacing to avoid feeling floaty or unfair.
- Sample/demo content from packages may clutter the repo if not separated from prototype-critical assets.
