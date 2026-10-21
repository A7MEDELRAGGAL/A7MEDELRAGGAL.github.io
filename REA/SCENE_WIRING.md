# Scene Wiring Guide — Boss Cycle & Run Management

Manual Unity editor instructions for Milestone 10. No `.unity` scene files are edited by tooling; all wiring below happens in the Unity Inspector.

## 1. CloneRunManager (run state singleton)

- Add a new empty GameObject to the main scene, name it `RunManager`.
- Attach `Clone.World.CloneRunManager` (a `NetworkBehaviour` — the object must be under a NetworkObject hierarchy the same way other network singletons in this project are).
- This object must exist in the scene at startup so `CloneRunManager.Instance` resolves.
- Properties exposed to HUD/UI read: `DeathsThisRun`, `RunActive`.
- RPC entry points (called from UI/HUD buttons, not wired in code):
  - `ServerStartRun()` — resets death counter, marks run active.
  - `ServerRestartRun()` — reloads the active scene (requires `ServerRestartRun().SetScene(scene)` be called with the desired scene reference before invoking).
- Static `CloneRunManager.NotifyPlayerDeath()` is already wired from `ClonePlayerVitals.ServerRespawnOrResurrect()` (death-hook polish, in place at `Assets\Scripts\Player\ClonePlayerVitals.cs:112`).

## 2. CloneSpawnManager (spawn points singleton)

- Add a new empty GameObject, name it `SpawnManager`, attach `Clone.World.CloneSpawnManager`.
- Assign serialized fields:
  - `PlayerSpawnPoints` — a Transform whose direct children are the player spawn markers.
  - `IslandRoots` — a Transform whose direct children are island root Transforms used for respawn scatter.
- If unset at runtime, defaults apply: `DefaultSpawn = (0, 2, 0)`; a `GameObject.Find("PlayerSpawnPoints")` fallback is used by `SpawnPointUtility.GetRespawnPosition()`.
- Must exist in the scene before gameplay so `CloneSpawnManager.Instance` resolves for boss-director spawns and respawn positions.

## 3. CloneBossDirector (server-side boss spawner)

- Add a new empty GameObject, name it `BossDirector`, attach `Clone.World.CloneBossDirector`.
- Assign serialized fields:
  - `_bossPrefab` — the boss creature prefab (a prefab containing `Clone.BossCloneCreature` plus a `NetworkObject`).
  - `_intervalSeconds` — spawn interval in seconds (default `240`).
  - `_waterSpawnChance` — chance the boss spawns on water (default `0.15`; island chance is `1 - _waterSpawnChance`).
- Runtime behavior: waits until `InstanceFinder.IsServerInitialized`, then spawns every interval with ≥1 connected player. Bonus API for testing/UI:
  - `ForceSpawnBoss()`
  - `SetInterval(float)` / `GetInterval()` / `GetTimer()`

## 4. Death → Respawn → Restart flow (wired chain)

1. Player health reaches 0 → `ClonePlayerVitals.ServerRespawnOrResurrect()`.
2. That method calls `Clone.RunManager` polish hook: `Clone.World.CloneRunManager.Instance?.NotifyPlayerDeath()` (already in code).
3. `NotifyPlayerDeath()` increments `DeathsThisRun` on the server.
4. Player is teleported via `ClonePlayer.TeleportTarget(SpawnPointUtility.GetRespawnPosition())`, which pulls a random island spawn from `CloneSpawnManager` (with `GameObject.Find("PlayerSpawnPoints")` fallback).
5. Health/fullness reset to 50%, poison/fire cleared.
6. A full restart (deaths reset + scene reload) is triggered externally via `CloneRunManager.ServerRestartRun()` from HUD/UI — assign the active scene reference with `.SetScene(scene)` before invoking.

## 5. Checklist

- [ ] `RunManager` GameObject in scene with `CloneRunManager` attached (NetworkBehaviour).
- [ ] `SpawnManager` GameObject in scene with `CloneSpawnManager` attached; `PlayerSpawnPoints` and `IslandRoots` assigned.
- [ ] `BossDirector` GameObject in scene with `CloneBossDirector` attached; `_bossPrefab` assigned to the boss creature prefab.
- [ ] Boss prefab has `Clone.BossCloneCreature` + `NetworkObject`.
- [ ] HUD/UI hook: button invoking `ServerStartRun()` (optional).
- [ ] HUD/UI hook: button invoking `ServerRestartRun().SetScene(activeScene)` then the RPC (optional).
- [ ] Death hook verified: `ClonePlayerVitals.cs:112` calls `Clone.RunManager` polish (`Clone.World.CloneRunManager.Instance?.NotifyPlayerDeath()`).
