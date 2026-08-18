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

