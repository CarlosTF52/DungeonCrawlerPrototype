# Milestones

## Milestone 0 - Project Setup

Status: Effectively complete.

Goal: Create a clean Unity project foundation.

Done when:

- Unity project exists. Done.
- Render pipeline decision is made. Needs explicit doc confirmation.
- Version control decision is made. Done: GitHub repo on `master`.
- ProjectDocs folder exists. Done.
- Initial scope, milestones, status, and decision log exist. Done.

## Milestone 1 - One Good Run

Status: Active, close to first end-to-end proof.

Goal: Prove the core loop with placeholder-quality content.

Done when:

- Player starts in the village hub. Working/in progress.
- Player can choose a character before a run. Working through roster/tavern systems.
- Player can choose whether to carry or bank money before a run. Working through player pouch and physical bank interactions.
- Player can enter one dungeon. Working through `ExpeditionGateway`.
- Player can advance through a small expedition route. Working through procedural/inspector-authored room route.
- Player can explore a small dungeon route. In progress.
- Player can fight at least one enemy. Working; slime launch enemy is the first archetype.
- Player can collect loot or a key item. Working during an active expedition.
- Player can return to the village. Working through extraction flow.
- Extracted loot becomes persistent or carried progress. Working: extraction adds to player pouch, bank interaction secures resources.
- Player can buy or trigger one upgrade. Debug spend exists; real upgrade not started.
- The loop can be repeated once. Not proven yet.

Current Milestone 1 focus:

- Playtest one full authored route from hub to dungeon and back.
- Replace debug spend with one real upgrade effect.
- Show carried/banked loot and active character clearly in the hub.

## Milestone 2 - Tension Pass

Status: Not started.

Goal: Make the run feel like survival horror, not a generic action test.

Done when:

- Dungeon lighting and audio create pressure.
- Enemy tells are readable.
- Resource scarcity affects decisions.
- At least one enemy is scary enough to avoid sometimes.
- Player death or failure has clear consequences.
- The expedition route creates a pressure curve instead of a flat sequence of rooms.

## Milestone 3 - Hub Meaning

Status: Early foundation started.

Goal: Make the village feel worth returning to.

Done when:

- One rescued survivor appears in the hub.
- One station or area improves after a successful run.
- One upgrade has visible or mechanical impact.
- The player has a clear reason to attempt the next run.

Current foundation:

- Physical bank supports deposit/withdraw decisions.
- Tavern/roster supports selecting Warden, Scout, or Occultist.
- Character stress/injury placeholders exist for future pressure systems.

## Milestone 4 - Vertical Slice Candidate

Status: Not started.

Goal: Combine the loop, mood, and progression into a short representative experience.

Done when:

- 10-20 minutes of coherent gameplay exists.
- Hub and dungeon transitions are stable.
- Combat, loot, rescue, upgrade, and return all work together.
- The experience can be shown to another person without explaining every step.

## Future Milestone - Co-op Spike v1

Status: Deferred.

Goal: Prove whether the core loop can support simple 2-player co-op after the single-player loop is fun.

Done when:

- Two players can exist in the same expedition room.
- Both players can fight or be threatened by one enemy.
- Shared loot can be collected and extracted.
- Expedition success/failure rules are clear for a small party.
- The spike identifies whether the project should continue toward co-op or remain single-player.
