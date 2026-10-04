# Enemy Controller Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Centralize run-time enemy flow and live-enemy access while keeping pooled object creation in `SpawnController`.

**Architecture:** Add a run-scoped `EnemiesController` that owns level selection, wave scheduling, the living-enemy registry, and enemy flow commands. Keep per-instance mutable state in `Enemy`; make `SpawnController` create pooled enemies and pickups. Expose active enemies through a read-only collection whose members remain mutable through their runtime API.

**Tech Stack:** Unity 6000.3.11f1, C#, Unity Test Framework package (installed); existing `ServiceLocator`, `EventBus`, `ObjectPoolManager`, and ScriptableObjects.

**Spec:** `docs/superpowers/specs/2026-10-04-enemy-controller-design.md`

## Global Constraints

- Keep `EnemyData` and `LevelSpawnsData` as authored configuration, not mutable run state.
- Buff behavior, modifier stacking, and duration rules are out of scope.
- `ManualSpawn` is not a supported flow and must not be preserved in the new API.
- `SpawnController` creates pooled spawnables; it does not schedule waves or own the active-enemy registry.
- Despawn-all must not award death drops or kill rewards.
- Preserve existing serialized level and spawn references when moving their owner.
- No new external dependencies.

## Review Focus

- **Pooled tutorial enemy reused as normal:** `Enemy.Initialize` clears tutorial state and stale target before assigning a new target. Verify in Task 2.
- **Enemy during death animation:** it stops counting as living at death start, not at pool return. Verify in Task 3.
- **Despawn-all side effects:** no death/drop/kill events, no duplicate pool release, and all registry entries removed. Verify in Task 3.
- **Horde at the alive limit:** a single horde never pushes the registry above `maxAliveEnemies`. Verify in Task 4.
- **Missing configuration or run teardown:** invalid level/spawn references do not leave scheduling active or service registrations behind. Verify in Task 4.

---

### Task 1: Add pooled enemy creation API

**Files:**
- Modify: `Assets/Scripts/ScriptsEnemies/Spawner/SpawnController.cs`

**Interfaces:**
- Produces: `public Enemy SpawnEnemy(GameObject prefab, EnemyData data, Vector3 position, Quaternion rotation)`; obtains an instance through `ObjectPoolManager.SpawnObject`, initializes it with `EnemyData`, and returns the initialized `Enemy`.
- Existing coin and coal creation remains in `SpawnController`.

- [ ] Add `SpawnEnemy` with null checks for the prefab and data. If the spawned object lacks `Enemy`, log an error, return it to the pool, and return `null`.
- [ ] Verify in Play Mode that a level enemy prefab spawns active at the requested pose, receives the requested `EnemyData`, and is reused after pool return.

### Task 2: Define enemy inactive lifecycle

**Files:**
- Modify: `Assets/Scripts/ScriptsEnemies/Enemy.cs`
- Modify: `Assets/Scripts/Events/GameEvents.cs`

**Interfaces:**
- Produces: `OnEnemyBecameInactiveEvent(Enemy enemy)` through `EventBus`, plus `public void Despawn()` on `Enemy`.
- The event is declared in `GameEvents.cs` and published once when an enemy dies, is removed by a dead wall, or is explicitly despawned. It does not mean the GameObject is already pooled.

- [ ] Add `OnEnemyBecameInactiveEvent` in `GameEvents.cs` with an `Enemy` payload; add an idempotent publish method in `Enemy` and invoke it at each transition out of living state, before returning an object to the pool.
- [ ] Implement `Despawn()` to stop coroutines, hide/reset relevant presentation, notify inactive listeners, and return to the pool without publishing death, drop, or kill events.
- [ ] In `Initialize`, reset `IsTutorialEnemy` and `targetWagon` before initializing a reused instance; preserve existing cooldown, health, animation, and target setup behavior after those resets.
- [ ] Verify in Play Mode that death, dead-wall removal, and explicit despawn each publish `OnEnemyBecameInactiveEvent` once; despawn publishes no death/drop/kill events; a tutorial enemy reused normally does not retain tutorial state or its previous target.

### Task 3: Add run-scoped enemy registry and commands

**Files:**
- Create: `Assets/Scripts/ScriptsEnemies/EnemiesController.cs`
- Modify: `Assets/Scripts/ScriptsEnemies/Enemy.cs`

**Interfaces:**
- Produces: `public IReadOnlyCollection<Enemy> ActiveEnemies { get; }`, `public void PauseSpawning()`, `public void ResumeSpawning()`, `public void StartWaves()`, `public void StopWaves()`, and `public void DespawnAll()`.
- Registry membership stays controller-owned. `ActiveEnemies` permits enumeration and mutation through each `Enemy` API, but not collection changes by callers.

- [ ] Create `EnemiesController` with a private active list, a membership set for duplicate protection, and one read-only collection wrapper over the list.
- [ ] Register each spawned living enemy and subscribe `EnemiesController` to `OnEnemyBecameInactiveEvent` through `EventBus`; remove it on the event. Derive the living count from registry membership rather than keeping another count.
- [ ] Implement `DespawnAll()` by iterating a snapshot, calling `Enemy.Despawn()`, and leaving event callbacks to remove membership exactly once.
- [ ] Register/unregister the controller with `ServiceLocator` and subscribe/unsubscribe its `EventBus` handlers symmetrically in `OnEnable`/`OnDisable`. On teardown, stop scheduling and despawn registered enemies before clearing the registry.
- [ ] Define `StopWaves()` as stopping future automatic spawns without despawning living enemies; `PauseSpawning()` pauses scheduling while preserving whether waves were started. Resume/start reverse their matching state.
- [ ] Verify in Play Mode that dead and despawned enemies disappear from `ActiveEnemies` immediately and registry membership remains correct after pool reuse and controller disable/re-enable.

### Task 4: Move wave policy and event routing

**Files:**
- Modify: `Assets/Scripts/ScriptsEnemies/EnemiesController.cs`
- Modify: `Assets/Scripts/ScriptsEnemies/Spawner/SpawnController.cs`
- Delete if no serialized users remain: `Assets/Scripts/ScriptsEnemies/ManualSpawn.cs`
- Modify: scene/prefab components that currently serialize the `SpawnController` level list or `SpawnZone` reference.

**Interfaces:**
- Consumes: `SpawnController.SpawnEnemy(GameObject, EnemyData, Vector3, Quaternion)` and the Task 3 registry/lifecycle API.
- `EnemiesController` handles `OnSpawnEnemyEvent` and `OnStartSpawningEnemiesEvent`; `SpawnController` retains `OnEnemyDeathEvent` pickup creation and `OnEnemyHitEvent` particle creation.

- [ ] Move `levelList`, current-level selection, weighted spawn pool, `SpawnZone`, `TrainRanges`, camera bounds checks, spawn interval, maximum-alive limit, and horde position policy from `SpawnController` into `EnemiesController`.
- [ ] Move tutorial/event spawn handling to `EnemiesController`; route creation through `SpawnController`, register each initialized enemy, and preserve tutorial target-event ordering.
- [ ] Map `OnStartSpawningEnemiesEvent(false/true)` to pause/resume. Keep normal automatic wave startup disabled in tutorial mode, as in the current behavior.
- [ ] Clamp each horde request against `maxAliveEnemies` using current registry count. Use current level `prefab` and selected `EnemyData` when calling `SpawnEnemy`.
- [ ] Remove wave scheduling, weighted selection, spawn request subscriptions, and `aliveEnemies` from `SpawnController`; keep pickup/feedback creation and its service registration.
- [ ] Remove the obsolete `ManualSpawnEnemy` API and delete `ManualSpawn.cs` only after checking scene/prefab references; remove any stale component attachments rather than adding a replacement manual-spawn path.
- [ ] Move serialized `levelList` and `SpawnZone` assignments to the new component in the runtime scenes. Confirm all existing level assets and spawn-zone references remain assigned; do not hand-edit generated `.meta` files.
- [ ] Verify in Play Mode that interval, weighted selection, pause/resume, start/stop, max-alive clamping, tutorial event spawns, death drops, hit particles, and despawn-all behave as specified. Also test empty level data, missing prefab/zone, and run teardown for clear diagnostics and no lingering service or scheduling.

### Task 5: Final lifecycle validation

**Files:**
- No additional code files unless Play Mode exposes a defect in Tasks 1-4.

- [ ] Run the relevant normal-run scene and tutorial scene in Play Mode; inspect the Unity Console for compilation, lifecycle, and event-subscription errors.
- [ ] Exercise enemy death during animation, dead-wall removal, despawn-all, repeated pool reuse, scene/run teardown, and subsequent run startup; verify there are no stale active entries, duplicate rewards, or extra spawns.
- [ ] Confirm all serialized references and the public read-only active-enemy access work in the Unity Inspector/runtime context.
