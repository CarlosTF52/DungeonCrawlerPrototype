# Feature Tasks

## Project Management

Task: `Dungeon Crawler Prototype - Project Management`

Purpose: Scope, milestones, decisions, project status, repo status, and next-step planning.

Status: Active management hub.

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

Status: Combat v1 working; deeper combat is intentionally paused.

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
- Stamina and basic player stats support.
- Health/stamina HUD bars through `PlayerVitalsHud`.
- Currency HUD through TextMeshPro via `CurrencyHud`.

Future combat ideas:

- Hit pause/camera shake.
- Stamina tuning.
- Blocking or dodge.
- Enemy attack rhythm beyond contact damage.
- Directional attacks or Bannerlord-lite melee as an advanced layer, not a current priority.

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
- `VillageBank`.
- `VillageBankDebugHud`.
- `VillageBankDebugSpend`.
- Debug/testing support for starting an expedition directly in a dungeon scene.
- Successful extraction deposits carried gold/relics into the village bank.

Current rule:

- Hub entrance and dungeon extraction points should use `ExpeditionGateway`.
- Plain door/scene transition scripts do not start an expedition and should not be used for expedition-state interactions.

Next step:

- Replace debug spend with one real upgrade station/effect so a successful expedition changes the player or hub in a concrete way.

## Procedural Expedition Map

Task: `Add procedural expedition map`

Status: Active v1 working.

Delivered:

- Darkest Dungeon-style room route/graph direction.
- Generated room nodes with room type, danger rating, enemy count, pickup count, trap count, objective flag, and extraction flag.
- Room content activator that enables/disables existing scene objects based on current room data.
- Inspector-authored route mode for designer control.
- Default editable route on new/reset run manager components.
- Singleton lookup fix so a configured scene `ExpeditionRunManager` is found before a blank runtime manager is auto-created.

Current model:

- One reusable `Dungeon` scene acts as a room shell.
- The active generated/authored room controls which pre-placed content objects are enabled.
- This is not prefab spawning yet.
- Later evolution can move from one room shell to multiple premade room scenes/prefabs selected by room type and danger.

Next step:

- Use the inspector-authored route to build and test one satisfying five-room expedition before adding more procedural complexity.
