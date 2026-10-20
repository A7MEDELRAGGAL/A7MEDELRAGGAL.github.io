# The original's fishing system — evidence and our rebuild

Artifact: `artifacts/HowToFish-Assembly-CSharp.dll`
sha256 `0491c7b5286ca37b42d506113a9c7e32e0ad8d9d121c5fe3be8e67ce9e9d036b`
Extracted with REA and reduced locally by `tools/rea-il.js`, `tools/rea-digest.js`,
`tools/rea-constants.js`. Nothing here is copied into the implementation.

## 1. The pieces

| Type | Fields | Methods | Role |
|---|---|---|---|
| `FishingRod` | 75 | 63 | the rod: line, bend, casting, reeling |
| `FishingRodCast` | 14 | 28 | the cast/line presentation: reel rotation, the "bail" (line) open state |
| `FishingRodCrab` | 5 | 24 | the crab-on-the-line variant |
| `Bait` | 23 | 35 | the live bait object |
| `BaitInfo` | 15 | 14 | the bait definition |
| `CreatureManager` | 9 | 20 | which creatures are alive and which one a bait draws |
| `Creature` / `Fish` | 39 / 20 | 66 / 14 | the fish themselves |

## 2. The rod model (from the field set)

**Line.** `_minLineLength`, `_maxLineLength`, `_lineReelStepLength` (how much line one reel
tick takes up), `_lineReelDamping`, `_holdReelSpeed` (the speed while the reel is held),
`_reelMultiIncreaseSpeed` / `_reelMultiDecreaseSpeed` (the reel speed ramps up while held and
decays when released), `_curLineLengthMulti` / `_receivedCurLineLengthMulti` (the line length
multiplier is network state), `_isReelingIn` / `_isReelingOut` (the two directions).

**Bait and rod bend.** `_bait`, `_baitJointForceForMaxBend`, `_maxRodBendAngle`,
`_rodBendForceSmoothSpeed`, `_rodBendRotSmoothSpeed`, `_rodBendDistribution` and
`_rodBendForceMultiCurve` (curves), `_receivedBaitPos`, `_receivedBaitForce`,
`_curRodBendForce`, `_curRodBendForceRefVel`. The rod bends under the fish's pull, and the bend
force is sent over the network, so the client sees the fish pull the rod.

**Cast.** `_isPullingBack`, `_curRodRot` / `_receivedRodRot`, `_castingRotCurve`,
`_castingRodRotSpeed`, `_castingRodRotBackSpeed`, `_releaseBaitRotPercent` (the bait is
released at a percentage of the rotation), `_lineThrowSpeed`, `_baitDragByLineLength` (a curve:
the drag on the bait scales with how much line is out).

**Hooking.** `_baitStuckDistToTP`, `_baitStuckTimeToTP`, `_baitStuckDistToTPSqr`,
`_baitStuckTimer`: the bait has to stay within a distance of the target point for a time, and
then it is stuck — the fish is hooked. This is not a random timer; it is a proximity hold.

**Fighting.** `_minMaxFishVelForDamage` (a range), `_velDamageMulti`: the fish's velocity
converts to damage. `_holdReelTimer`, `_curReelSpeedMulti`.

## 3. The bait rules (`BaitInfo`)

| Field | Meaning |
|---|---|
| `_catchTimeMinMax` | the bite window for this bait |
| `_lostOnBiteChance` | the chance the bait is lost when you miss |
| `_requireReelingToCatch` | some catches have to be reeled in rather than taken |
| `_itemWeights` | a dictionary of item -> weight: which fish this bait draws |
| `_cost`, `_name`, `_description`, `_mesh` | the shop and presentation side |
| `_hookPoint`, `inventoryMeshPos/Rot/Scale` | where the bait sits on the hook and in the pack |

## 4. Which fish a bait draws (`CreatureManager`)

| Field | Meaning |
|---|---|
| `_aliveCreatures`, `_aliveCreaturesToRemove` | the live population |
| `_minBaitLevel` | the minimum bait level a fish will take |
| `_shinyCreatureChance` | the chance of a rare variant |
| `_removeAliveFishLevel` | when a fish is despawned |

`FindFishForBait` and `FindFishForAllBaits` are the two lookups: one bait draws one fish, and
the "all baits" variant considers every bait at once.

## 5. The loop, as recovered

1. Hold to pull the rod back; the rotation follows a curve and the throw speed comes from it.
2. Release: the bait is thrown, the line pays out, the bait has drag that grows with line length.
3. A fish is drawn to the bait, weighted by the bait's own item table.
4. The bite window comes from the bait's `catchTimeMinMax`.
5. Holding the bait next to a fish for long enough sticks it — the hook is a proximity hold.
6. Reeling ramps the speed up, shortens the line, and the fish's pull bends the rod and builds
   tension; the fish's velocity does damage.
7. The catch lands in the pack. Miss and the bait can be lost.

## 6. What we built, and how it maps

`WebGame/js/fishing.js` and `WebGame/js/bait.js`, reimplemented from the above shape:

| Original | Ours | Notes |
|---|---|---|
| `_minLineLength` / `_maxLineLength` | `TUNE.minLineLength` / `maxLineLength` | 1.5 m to 26 m |
| `_lineReelStepLength` x `_holdReelSpeed` | `lineReelStepLength` x `holdReelSpeed` | the line shortens while the reel is held |
| `_reelMultiIncreaseSpeed` / `Decrease` | `reelRampUp` / `reelRampDown` | the reel speed ramps while held and decays when released |
| `_lineThrowSpeed` | `lineThrowSpeed` | the bait is thrown, then falls under gravity |
| `_baitDragByLineLength` | `baitDrag` | the bait loses speed as it settles |
| `_baitStuckDistToTP` / `_baitStuckTimeToTP` | `baitStuckDist` / `baitStuckTime` | holding the bait next to a fish hooks it |
| `_catchTimeMinMax` | `Bait.catchWindow(id)` | per bait bite window |
| `_lostOnBaitChance` | `Bait.lostOnBiteChance(id)` | missing the strike can cost the bait |
| `_itemWeights` | `Bait.candidates(id)` / `rollFish(id)` | a weighted draw from the bait's own table |
| `_requireReelingToCatch` | the fight must be reeled in | `Bait.requiresReeling(id)` |
| rod bend under the fish's pull | `tension` | the tension bar in the HUD; too much snaps the line |
| `FishingUI.OnNewFishCaught` | `journal.record` on catch | the catch also lands in the journal |

**Where the numbers are not.** The rod's own tunables (`_maxLineLength`, `_holdReelSpeed`, the
rod bend curves) are **inspector values on the prefab**, not code constants: the rod's
`.cctor` is 4 instructions and its instance constructor sets no field. So the exact shipped
numbers live in the serialized asset, not in the metadata. Our values are our own and are marked
as such in `WebGame/js/fishing.js`; they can be dialled against the prefab later without touching
the structure.

## 7. What is still open

- The rod's *feel* curves (bend distribution, cast rotation curve, the audio mix) are animation
  and prefab data.
- The crab-on-the-line variant (`FishingRodCrab`) and the boss fish are separate types.
- The prefab values themselves: recoverable from the Unity serialized assets, not from the
  assembly.
