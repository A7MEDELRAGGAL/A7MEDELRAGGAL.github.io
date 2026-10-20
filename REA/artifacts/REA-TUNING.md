# Reference tuning recovered by REA

Target: `artifacts/Muck-Assembly-CSharp.dll`
sha256 `5f9d0dc1e0e72f5493013c92d05ed9fb8628019f54f8bc02162dad0d2f20810c`
Module MVID `8db17789-d6d5-48de-86ab-e696a5ff6b2b`

Recovered with REA `inspect_managed_artifact` and `inspect_managed_members` (managed/IL view),
then reduced locally by `tools/rea-digest.js` and `tools/rea-constants.js`.

**Use:** mechanics and tuning reference for the How to Fish track. Everything in this repository
is our own code, art and text — no method bodies, assets or strings are copied.

## How the numbers were obtained

`PlayerMovement..ctor` and `Boat..ctor` assign the default field values, so the constructor
bodies carry the shipped tuning directly. `tools/rea-constants.js` decodes the CIL anchors and
pairs each `ldc.r4` / `ldc.i4` literal with the `stfld` that stores it.

Raw output:

- `artifacts/rea-muck-mechanics.md` — fields and signatures per type.
- `artifacts/rea-muck-il.txt` — per-method CIL flow with the literals inline.

## Movement — `PlayerMovement` (61 fields, 50 methods)

| Field | Value | Meaning |
|---|---|---|
| `moveSpeed` | 3500 | input force applied to the body |
| `maxWalkSpeed` | 6.5 | walking cap |
| `maxRunSpeed` | 13 | sprinting cap |
| `maxSpeed` | 6.5 | hard cap when not sprinting |
| `extraGravity` | 5 | additional gravity while airborne |
| `crouchScale` | (1, 1.05, 1) | crouch collider scale |
| `slideForce` | 800 | downhill slide force |
| `slideCounterMovement` | 0.12 | slide counter-movement factor |
| `jumpCooldown` | 0.25 | seconds between jumps |
| `jumpForce` | 12 | jump impulse |
| `jumps` | 12 | jump counter seed |
| `swimSpeed` | 50 | swim force |
| `jumpCounterResetTime` | small int | counter reset window |
| `counterMovement` | 0.14 | wall counter-movement factor |
| `threshold` | 0.01 | input/velocity deadzone |
| `maxSlopeAngle` | 50 | degrees before the player slides |
| `delay` | 5 | grounded/surf cancel delay |

### Behaviour recovered from the IL

- `Movement` pushes normalised input into the body as a force and clamps the result to the
  walk/run caps. The body keeps momentum; it is not steered toward a target velocity.
- `CounterMovement` doubles the velocity component you are pushing *against*, gated by
  `threshold` and per-axis cooldowns (`readyToCounterX`, `readyToCounterY`).
- `RampMovement` engages surfing only when grounded, on a ramp, not crouching, not jumping and
  with both input axes under 0.05.
- `WaterMovement` uses camera-relative forward/right with `swimSpeed`, and halves it when
  `PlayerStatus.stamina` is empty. Jumping out of the water sets vertical velocity to 2.
- `Jump` requires `PlayerStatus.CanJump()`, applies `jumpForce * 1.5` plus `normalVector * 0.5`,
  and tracks the jump counter with `resetJumpCounter` / `jumpCounterResetTime`.
- `PlayerStatus` gates the jump and carries stamina, hunger and health.

## Boat — `Boat` (31 fields)

| Field | Value | Meaning |
|---|---|---|
| `heightUnderWater` | 3 | hull height under the surface before it is considered sunk |
| `amp` | 20 | wave amplitude of the animated water surface |
| `MoveBoat` | 2, 85, 0.85, 0.15 | direction normaliser, sink threshold, server-owner and event factors |

The boat follows the **water surface transform** rather than simulating its own buoyancy, and
sets `sinking` when the hull sits more than `heightUnderWater` above that surface.

## Items, mobs and status

| Type | Fields | What it tells us |
|---|---|---|
| `InventoryItem` | 44 | item record shape: stack, amount, durability, cook state, price |
| `PlayerStatus` | 43 | health, stamina, hunger, poison, fire and their tick rates |
| `PlayerSave` | 41 | the save DTO fields |
| `Mob` / `MobType` | 29 / 21 | creature stats: health, damage, speed, drops |
| `Player` | 19 | the accessor surface (`Movement`, `Inventory`, `Vitals`, …) |

## What How to Fish adds on top of this base

`How to Fish` is a re-skin and expansion of the same codebase lineage, so it adds the systems
that are absent from this artifact: a dedicated water system with buoyancy, a boat motor with
owned seats, a fishing rod with cast/bite/reel, vendors and attachments, a casino minigame,
NPCs with quests, a journal, and a settings/localisation layer. Those systems are described in
`GAME_DESIGN.md` and implemented in the Unity track under `Assets/Scripts/`.
