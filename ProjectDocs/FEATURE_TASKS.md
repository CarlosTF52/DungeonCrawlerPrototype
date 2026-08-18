# Feature Tasks

## Project Management

Task: `Dungeon Crawler Prototype - Project Management`

Purpose: Scope, milestones, decisions, project status, repo status, and next-step planning.

## Scene Management

Task: `Plan first scene setup`

Status: Scene Management v2 complete for prototype needs.

Delivered:

- First-person movement setup.
- Persistent player across scenes.
- Named spawn points.
- Bidirectional room travel.
- Interact-to-enter doors.

Notes:

- Use `DoorSceneTransition` or `SceneExitTrigger` for plain scene movement.
- Use `ExpeditionGateway` when the interaction changes expedition state.

## Combat

Task: `Develop game combat`

Status: Combat v1 working.

Delivered:

- Player/enemy `Damageable` flow.
- Enemy health/death.
- Contact damage with layer/trigger filtering.
- Invincibility frames.
- Damage flash and knockback/stagger hooks.
- Rigidbody-friendly enemy chase.
- Enemy pause after contact hit.
- Player knockback integrated through Starter Assets `FirstPersonController` external velocity.
- Input Actions `Attack` action.
- Animancer swing playback.
- Timed `PlayerWeaponHitbox` active window.

Next combat polish later:

- Hit pause/camera shake.
- Stamina costs.
- Blocking or dodge.
- Enemy attack rhythm beyond contact damage.

## Expedition Loop

Task: `Start expedition loop v1`

Status: Active.

Delivered:

- `ExpeditionRunManager`.
- `ExpeditionGateway`.
- `ExpeditionLootPickup`.
- `ExpeditionObjectiveTrigger`.
- `ExpeditionDebugHud`.
- `ExpeditionOutcome`.
- Debug/testing support for starting an expedition directly in a dungeon scene.

Current rule:

- Hub entrance and dungeon extraction points should use `ExpeditionGateway`.
- Plain door/scene transition scripts do not start an expedition and should not be used for expedition-state interactions.

Next step:

- Add a persistent village inventory/progression component so extracted gold/relics are banked before the next run resets temporary expedition counters.
