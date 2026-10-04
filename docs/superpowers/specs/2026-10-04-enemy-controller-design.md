# Enemy Controller Design

## Goal

Give run-time gameplay one reliable place to manage live enemies and enemy flow, so later systems can query current enemies and apply buffs or commands without changing shared enemy assets. Keep object construction and pooling separate from wave policy.

## Current constraints

- `SpawnController` currently combines wave timing, spawn limits, weighted enemy selection, object-pool calls, pickup creation, death-drop handling, and hit-particle handling.
- `Enemy` owns per-instance combat state and is returned to `ObjectPoolManager` after its death animation.
- `EnemyData` and `LevelSpawnsData` are ScriptableObjects and must remain authored configuration, not mutable run state.
- `ManualSpawn` is no longer a viable flow and is excluded from the new API. Tutorial spawns enter through an existing event path.
- The level list is serialized on `SpawnController`; moving its owner requires preserving/reassigning those Inspector references.

## Ownership

### `EnemiesController`

A run-scoped `MonoBehaviour` registered as a service for the active run. It owns:

- Selecting the current `LevelSpawnsData` and maintaining the weighted enemy spawn selection.
- Wave timing, maximum-alive enforcement, and the living-enemy registry.
- Pause/resume spawning, start/stop wave flow, tutorial/event spawn requests, and despawning all living enemies.
- Direct access to currently living `Enemy` instances for gameplay systems that need to inspect or mutate per-instance runtime state.

The registry remains controller-owned and is exposed as a read-only collection so callers can operate on each `Enemy` without changing membership behind the controller's back. State changes go through the enemy's runtime API. Buff behavior is not part of this first implementation: no modifier format, stacking, or duration rules are introduced. Future buffs must modify per-instance runtime state and must not mutate `EnemyData` assets.

### `SpawnController`

The object creation boundary for spawnable instances. It retains pooled creation/initialization of enemies and creation of coins and coal, and returns the created enemy instance to its caller. It does not choose enemy types, schedule waves, maintain the alive limit, or own the live-enemy registry.

Death-drop and hit-feedback reactions can remain attached to existing events as appropriate; they must not become wave-policy responsibilities again.

### `Enemy`

Continues to own per-instance health, target, cooldowns, behavior, and death animation. It publishes an `OnEnemyBecameInactiveEvent` through `EventBus`, declared in `GameEvents.cs`, so `EnemiesController` removes the instance when it dies or is despawned, before it is returned to the pool.

## Runtime flow

1. `EnemiesController` initializes for the run, reads the selected level configuration, and registers/unregisters with `ServiceLocator` symmetrically with its enabled lifetime.
2. When wave policy requests an enemy, it selects an `EnemyData` and asks `SpawnController` to create and initialize a pooled enemy instance.
3. `EnemiesController` registers the initialized living instance. The alive limit is derived from the registry rather than maintained as a second independently mutable count.
4. Death and explicit despawn publish the inactive event exactly once; `EnemiesController` removes the instance from the registry. Pool release remains owned by the existing enemy/pool lifecycle.
5. Pickup creation remains a `SpawnController` responsibility in response to enemy-death drop data. Tutorial/event enemy requests route through `EnemiesController` for policy and then through `SpawnController` for creation. The legacy `ManualSpawn` path is not preserved.

## Lifecycle and edge cases

- Duplicate registration/removal must be harmless.
- A dead enemy must not receive later buffs or count against the living-enemy cap during its death animation.
- Despawn-all must release enemies without awarding death drops or kill rewards.
- Callers may mutate an active enemy's runtime state through its public API, but cannot add/remove entries from the controller's registry directly.
- Reused pooled enemies must be initialized and registered as fresh living instances; stale registry entries must not survive return-to-pool or run teardown.
- Missing level configuration or spawn references should stop the affected spawn request with a clear diagnostic, not leave counters inconsistent.
- Run teardown must stop scheduling, clear the registry, unsubscribe events, and unregister the service.
- Before moving serialized level configuration, inspect the actual scene/prefab owners and preserve all existing `LevelSpawnsData` references.

## Validation

No gameplay test files currently exist. Validate in Unity Play Mode:

- Automatic waves respect interval and max-alive settings; pausing/resuming and start/stop behave as specified.
- Tutorial/event spawns pass through the same registry and creation path; the legacy `ManualSpawn` path is not required.
- Enemy death and despawn remove each instance once; death drops occur only on death, not despawn.
- Pooled reuse registers the instance again without stale state or duplicate registry entries.
- Current-enemy queries exclude dead/despawned enemies, and scene/run teardown leaves no registered service or active scheduling.
- Confirm all serialized level assets remain assigned after the ownership move and inspect the Unity Console for lifecycle or compile errors.