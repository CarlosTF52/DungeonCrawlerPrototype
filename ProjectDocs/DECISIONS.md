# Decision Log

## 2026-08-17 - Project Management Task

Decision: This Codex task is dedicated to project management only.

Reason: Keep high-level scope, status, and milestone decisions separate from feature implementation.

## 2026-08-17 - Prototype Before Full Game

Decision: Build a small prototype before expanding into a larger game.

Reason: Past projects drifted away from the original idea or consumed time before becoming fun.

## 2026-08-17 - Core Game Direction

Decision: Fantasy survival-horror dungeon crawler with a village hub.

Reason: This combines available humanoid/monster assets, melee systems, dungeon structure, and a progression loop that can be prototyped in a small scope.

## 2026-08-17 - Default Perspective

Decision: Start with third-person unless a strong reason appears to switch to first-person.

Reason: Third-person better supports animation readability, character gear visibility, enemy silhouettes, and methodical melee combat.

## 2026-08-17 - Documentation Location

Decision: Store project management docs in `ProjectDocs` inside the Unity project folder.

Reason: Keep project planning close to the Unity project while avoiding clutter in gameplay folders.

## 2026-08-17 - Git Default Branch

Decision: Keep the repository default branch as `master`.

Reason: The repository is already initialized this way, and the branch name does not affect the prototype workflow.

## 2026-08-17 - Milestone 1 Started

Decision: Treat the project as entering Milestone 1 after the 648a51d Project with player and scene management commit.

Reason: The repo now contains initial player, combat, scene, and transition foundations that move the project beyond setup and into playable-loop implementation.

## 2026-08-18 - Darkest Dungeon-Inspired Direction

Decision: Use Darkest Dungeon, especially the first game, as a major structural and tonal reference while keeping this project centered on action 3D combat and horror.

Reason: The reference clarifies the desired pressure: dangerous expeditions, hub recovery, attrition, dread, survivor value, and meaningful returns from dungeon runs.

## 2026-08-18 - First Playable Loop Priority

Decision: Prioritize completing Expedition Loop v1 before adding more combat polish or large hub systems.

Reason: Scene travel and combat already work well enough to support a small run. The prototype now needs proof that loot/objective extraction creates a reason to return to the hub and start another expedition.

## 2026-08-18 - Hub Banking Before Upgrades

Decision: Add a minimal village inventory or hub progression manager before building upgrade UI or deeper hub features.

Reason: Expedition loot currently exists as run-state data. Banking it after extraction is the smallest step that turns successful runs into persistent progress.

## 2026-08-18 - Commit Recovery Files Excluded

Decision: Exclude Unity _Recovery scene files from the cb25e9c Add combat, HUD, and expedition systems commit.

Reason: Recovery files are editor-generated safety artifacts and should only be committed if intentionally restored into real project scenes.

## 2026-08-18 - Village Bank Established

Decision: Keep VillageBank separate from ExpeditionRunManager.

Reason: The run manager owns carried expedition loot, while the village bank owns resources that safely made it home. This separation supports upgrades without confusing temporary run state with persistent hub progress.

## 2026-08-18 - Next Prototype Upgrade

Decision: The next feature after today's commit should be one concrete upgrade purchased from banked expedition loot.

Reason: The prototype has movement, combat, expedition state, extraction, banking, and HUD feedback. A real upgrade closes the motivation loop for repeating expeditions.

## 2026-08-19 - Procedural Route Before Procedural Geometry

Decision: Treat procedural generation as a Darkest Dungeon-style expedition route/room graph first, not full 3D procedural room placement.

Reason: The project needs expedition pacing and replay structure before costly dungeon-generation technology. Hand-authored or reusable room shells keep the prototype playable while the route system evolves.

## 2026-08-19 - Inspector-Authored Routes For Control

Decision: Support inspector-authored expedition routes and use them for near-term prototype testing.

Reason: Authoring the route directly gives better design control while tuning the first playable loop, while the procedural generator remains available as a fallback or later expansion.

## 2026-08-19 - Combat Depth Paused

Decision: Pause deeper combat development for now.

Reason: The current combat foundation is sufficient for expedition testing. Directional/Bannerlord-lite melee, blocking, and enemy attack animations should wait until the core expedition loop proves fun.

## 2026-08-19 - Preserve Configured Run Manager

Decision: ExpeditionRunManager.Instance should search for an existing configured scene manager before creating a blank runtime manager.

Reason: Inspector-authored route data must remain the source of truth. A blank auto-created singleton can erase effective setup by causing the configured manager to destroy itself as a duplicate.
## 2026-08-20 - Multiplayer-Aware Architecture

Decision: Treat possible 2-player co-op as an architectural consideration now, but defer actual multiplayer implementation.

Reason: Multiplayer would touch player spawning, scene transitions, expedition state, combat authority, enemy targeting, loot, death/failure, and hub progression. Planning for those seams now is cheap, while implementing networking before the single-player loop is fun would slow the prototype.

## 2026-08-20 - Ignore Unity Recovery Folder

Decision: Ignore Assets/_Recovery/ in git moving forward.

Reason: Unity auto-generates recovery scene files when the project opens. They are editor safety artifacts and create noisy untracked changes unless intentionally restored into real scenes.

## 2026-08-20 - Physical Bank And Carried Currency

Decision: Treat the bank as a physical village location. Successful extraction moves run loot into carried player currency, and the player must interact with the bank to deposit money into safe storage or withdraw money for dungeon use.

Reason: This creates a preparation choice before expeditions: keep money safe in the village, or carry money into the dungeon so merchants and future risk/reward systems can use it.

## 2026-08-20 - Roster Before Upgrade Depth

Decision: Add character roster and tavern selection before building deeper upgrade systems.

Reason: If stress, injuries, or recovery become part of the loop, the player needs multiple usable characters and a reason to swap between them. Character-specific stats and skills should be part of the village preparation layer.

## 2026-08-21 - Carried Money Before Safe Banking

Decision: Successful extraction should move run loot into the player currency pouch, while safe banking requires physical village bank interaction.

Reason: This creates a real risk/reward choice before expeditions and prepares the design for dungeon merchants that spend carried money.

## 2026-08-21 - Tavern Roster Before Deeper Upgrades

Decision: Add character roster selection before building deeper upgrade systems.

Reason: Character choice gives the village a clearer purpose and creates a foundation for future stress, injury, role, and skill systems.

## 2026-08-21 - First Enemy Archetype Is Slime Launcher

Decision: Use a physics-based slime lunge enemy as the first enemy-specific archetype.

Reason: The slime is readable, quick to prototype, and establishes reusable enemy patterns: ScriptableObject attack profiles, telegraphed windup, active damage window, interruptibility, recovery, and visual-only deformation.

