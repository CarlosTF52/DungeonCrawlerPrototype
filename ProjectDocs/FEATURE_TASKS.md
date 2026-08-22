# Feature Tasks

## Project Management

Task: `Dungeon Crawler Prototype - Project Management`

Purpose: Scope, milestones, decisions, project status, repo status, and next-step planning.

Status: Active management hub.

Related backlog:

- `ProjectDocs/FUTURE_FEATURES.md` tracks nice-to-have future systems that should stay outside Milestone 1 unless explicitly promoted.

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
- Runtime stat refresh support for character roster swaps.

## Player UIX

Task: `Develop player UI indicators`

Status: Active foundation started.

Delivered:

- `PlayerVitalsHud` supports health/stamina/sanity fill indicators, optional TextMeshPro number labels, smoothed fill updates, and low-resource warning colors.
- `CurrencyHud` remains the currency indicator for expedition loot, player pouch, or safe village bank values.
- `PlayerCharacterHud` shows active character name, age, status, job, health, stamina, injury, current/max stress, stress tolerance, and attack.
- `ExpeditionInfoHud` shows current run/depth, room type, danger, objective progress, carried loot, and extraction status as a real TMP information box instead of relying on `OnGUI` debug output.
- `PlayerCharacterDefeatHandler` can add stress to the active character when the player takes damage, currently using a temporary 1 stress per health lost rule. Stress starts at 0 and defaults to a tunable max of 10.

Current rule:

- Persistent player canvas owns player-state UI: vitals, carried currency, active character indicator, expedition info, damage feedback, interact prompts, and future stress/horror overlays.
- Hub Canvas owns village-location panels: tavern roster, bank, blacksmith, ward, cemetery, and future village management UI.

Next step:

- Build and tune the Unity canvas layout, wire the new stress/sanity bar, then retire matching debug HUDs after playtesting.

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
- `PlayerCurrencyPouch`.
- `VillageBankInteractable`.
- `DungeonMerchantDebugPurchase`.
- Debug/testing support for starting an expedition directly in a dungeon scene.
- Dungeon pickups immediately update the player pouch; physical bank interaction secures or withdraws resources.
- Modular village upgrade system with `UpgradeDefinition`, `UpgradeOwnerScope`, `VillageUpgradeManager`, `BlacksmithUpgradeInteractable`, `TavernUpgradeInteractable`, and `VillageUpgradeDebugHud`.
- First concrete character upgrade: `BlacksmithWeaponDamage` / Sharpened Weapons, which spends safe banked resources and increases weapon damage for the active character only.
- First concrete village upgrade: `TavernRecovery` / Better Beds, which spends safe banked resources and increases resting health recovery after successful extraction.

Current rule:

- Hub entrance and dungeon extraction points should use `ExpeditionGateway`.
- Plain door/scene transition scripts do not start an expedition and should not be used for expedition-state interactions.
- Village bank is safe storage; player pouch is money carried into risk and future merchant interactions.

Next step:

- Place and test the blacksmith station in the hub scene, then retire or hide `VillageBankDebugSpend` once the upgrade path is comfortable.

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

## Village Systems

Task: `Village Systems - Bank, Upgrades, Tavern`

Status: Active foundation working.

Delivered:

- Physical bank flow: dungeon loot enters the player pouch, not the safe bank automatically.
- Deposit/withdraw bank interactions with an in-range HUD showing carried currency, banked currency, and the configured transfer action.
- Player carried pouch for future dungeon merchants.
- Currency HUD hardening for current expedition, player pouch, and village bank sources.
- Character roster foundation with `CharacterDefinition`, `CharacterRosterManager`, `PlayerCharacterApplier`, `TavernCharacterSelector`, and roster HUD/debug display that includes active character progression and stored current health; the main roster HUD is hidden until the player stands in a tavern selector trigger.
- Starter characters: Warden, Scout, Occultist.
- Runtime character identity/state: generated names, starting age, status, village job placeholder, stress placeholder, injury severity, random stat offsets, and age-modified effective stats.
- Roster-owned health state: wounded characters keep missing health when swapped out, and resting characters recover a small amount after another character successfully extracts. Better Beds increases that recovery amount.
- Resurrection Ward interactable shows recoverable fallen characters only while standing in its trigger, including age, status, runs until return, injuries, stress, and job; pressing E cycles fallen characters.
- Cemetery interactable shows permanently dead characters only while standing in its trigger, including age at death, no-return status, injuries, stress, and job; pressing E cycles graves.
- Cursed defeat v1: `PlayerCharacterDefeatHandler` can age the active character by `10 + overkill damage`, mark them fallen or permanently dead at age 100, add injury severity, clear carried pouch currency, and fail the active expedition.

Next step:

- Add `PlayerCharacterDefeatHandler` to the player, then test the defeat/return flow and tune age penalties before building real job assignment UI.

## Enemies

Task: `Enemies - Dungeon Crawler Prototype`

Status: First archetype working.

Delivered:

- `EnemyLaunchAttackProfile` ScriptableObject for reusable launch attack tuning.
- `SlimeLaunchEnemy` physics-based slime lunge controller.
- Slime launch damage window using existing `ContactDamage` hitbox.
- Damageable C# events used for enemy interruption/death handling.
- Player weapon and fallback melee can trigger target knockback.
- Slime launch can cancel when damaged.
- Slime visual deformation for idle wobble, windup squash, launch stretch, damage pulse, and death squish.
- Launch ends when damage is dealt to avoid extreme player shove at edge cases.

Current rule:

- Enemy gameplay root/colliders should stay stable.
- Deform only a visual child mesh/root.
- Use ScriptableObject profiles where possible so future enemies reuse attack tuning patterns.

Next step:

- Tune slime timing and collision layers inside a full expedition route.
