# CLONE_PLAN.md — Clean-Room Muck Track Plan

> **Note:** This is the Muck fishing-clone track plan for the *REA* project only. Any other Unity project runs must record its plan in a separate file before modifications. The fast-paced FPS track lives separately in `CLONE_PLAN-FPS.md`.

هذا مسار الكلون Muck — أي جولة تعمل على مشروع مختلف تنقل خطتها لملف منفصل قبل التعديل.

- **Target Reference:** Muck (Dani) — study-only digest notes in `artifacts/`
- **Project Objective:** Reconstruct the core Muck loop (survive day/night, gather, craft, fight, shared chest, currency, save) from scratch in `UnityProject/`
- **Strict Clean-Room Constraint:** This project is built from scratch. We never copy Muck's source code, assets, textures, names, or strings. All code in this repository is original work informed only by high-level behavioral observation and study digests under `artifacts/`.

---

## 0. Evidence & Research Inputs

| File | Contents |
| ---- | -------- |
| `artifacts/rea-muck-chests-craft-save.md` | Chest, crafting-station and save-manager type digests |
| `artifacts/` (earlier REA runs) | Preceded current phase with a full study/recon campaign of Muck's gameplay systems, including networked player mechanics, peer-to-peer versus FPS semantics, combat interaction logic, hit registration semantics, Phase-1 roadmap lines, and the clean-room plan shape adopted thereafter |

---

## 1. Compliance & Original-Only Rules

- Do not copy original gameplay implementation code, generated assets, or UI source code — only read the original logic, never bring it into the new project.
- Namespaces/types for Muck's unlocks/cultural callbacks and Battle-level asset bundles... forged from scratch.

```text
+--------------------+-----------------------------+
| Current Project    | Original Game               |
| (Unity, C#,        | Reference for behaviors     |
|  original assets)  | via digests/scans           |
+--------------------+-----------------------------+
CloneChest Systems
This is a small networked gameplay project, that must reach looping from start and finish:
```

- **1. Player Movement & Steering** — Complete basic WASD/… movement, camera control, jumping parkour, and terrain surface speeds. **[x]**
- **2. Combat & Hit Registration** — High-tier attack, damage, health, and death state logic. **[x]**
- **3. Inventory & Item Interaction** — Inventoryslots, UI inventory, pickups system. **[x]**
- **4. Crafting **[x]** — Crafting station and recipe consumption.
- **5. Shared Chest** — Sysetem to share a chest, UI and clicks. **[x]** (implemented in `CloneChest.cs` and `Chest/CloneChestUI.cs`)
- **6. Day/Night Cycle** — `CloneDayNightManager.cs` with day time windows for events and dangerous-wave of spawn of hostile creatures. **[x]**
- **7. Currency** — `CloneCurrency.cs` implementing cash-credits that will then be used to purchase Hoodie perks/stat boosts/upgrades. **[x]**
- **8. Save/Load** — `CloneSaveManager.cs, CloneSaveData.cs, CloneAutoSaver.cs` — system that serializes world state and daytime, currency, inventory items, chest contents, status. **[x]**

## 2. Implementation Roadmap

### Phase 1 — Core Loop Foundation

```text
Blueprint:                                      Status:
Movement core                    ████████ [x]
Inventory & gross item plumbing  ████████ [x]
PVP-FPS combat baseline          ████████ [x]
Crafting (station + recipes)     ████████ [x]
Day/night cycle                  ████     [x]
Save/load system                 ████     [x]
Currency                         ████     [x]
Shared chest (networked)         ████     [x]
```

## Engine Notes

```text
- Compliant: building and executing via Unity Engine is disabled by policy.
- Notes: compilation checks are done through manual static inspection only.
```

Now the plan is in progress.

---

## Milestones

| Milestone | Status | Testing |
| --------- | ------ | ------- |
| Movement + combat + inventory playable loop | [x] | [ ] |
| Basic ItemStack/crafting UI | [x] | [ ] |
| Shared chest (networked) + chest UI | [x] | [ ] |
| Day/night + spawn waves | [x] | [ ] |
| Currency & perks | [x] | [ ] |
| Save/load + auto-save | [x] | [ ] |
| Full loop: run, die, restart | [x] | [ ] |
| Chest loot tables + boss key drop | [x] | [ ] |
| Craftable gear/weapons progression | [x] | [ ] |

## Milestone 10: Chest Loot Tables + Boss Key Drop (Muck premise: boss creatures drop a key)
Clean-room decisions (no source/constants copied):
- Loot entries: `struct CloneChestLootEntry { int ItemId; int Amount; float Weight; }` — weight-pick roll, schema follows the existing `ClonePurchasable` entry precedent.
- `World/CloneChest.cs`: server-side weighted roll on FIRST open (open ServerRpc path), skips already-occupied slots; rolled items land in empty chest slots via the existing store path so `SyncChestState`/`BroadcastChestState` handles replication.
- Boss key: item ID 21, resource category, registered in `CloneItemCatalog.BuildDefaultCatalog()`; no unique prefab needed because `CloneLibrary.GetPrefab` falls back to `ResourcePrefab` for resources.
- Boss creature: `BossCloneCreature : CloneCreature` with `ServerDie` override — award the boss key item before the existing `Despawn(true)`.
- Boss spawn director mirrors the observed dragon spawn pattern (water/owner gates, weighted position choice, `ServerSpawnNewMob` style) — see artifacts/rea-muck-il.txt dragon IL notes.
- `locked[]` chest semantics observed in artifacts are acknowledged and intentionally NOT replicated (keep first-open roll simple).

## Milestone 11: Craftable gear/weapons progression
Clean-room decisions (no source/constants copied):
- Craft UI: cell-grid chest; recipes client-safe, server-authoritative.
- Gear: per-piece stat tariffs (agility/health-regen/stamina/encumbrance tiers).
- Armor mitigation: ServerDamage %; regen via ServerTickVitals.
- Encumbrance scales FpsPlayerMotor speed constants at one point; ratios preserved.
- Save: update SaveLoad scripts.
- Do not transcribe names/strings/constants; original code only.

Implemented (M11):
- `CloneItemDefinition`: `Gear` category added; armor fields (`ArmorTier`, `Protection`).
- `CloneItemCatalog`: resource/tool/sword/armor ids 22–32 registered in `BuildDefaultCatalog()`.
- `ClonePlayerInventory`: `ServerHasItems`/`ServerConsume` (server-authoritative ingredient checks).
- `CloneCraftingManager`: recipe table (axe/pick/armor/planks, etc.), server-side craft flow; gear recipes auto-equip.
- `ClonePlayerEquipment`: 4 wearable slots (Helm/Chest/Legs/Boots), `TotalProtection`, `EncumbranceFactor`; motor speed scaling, ratios preserved.
- `ClonePlayerVitals`: `ServerDamage` applies protection mitigation.
- SaveLoad: equipment persisted (per-slot item ids) + inventory.
- Runtime bootstrap: `ClonePlayer` ensures `ClonePlayerEquipment`/`CloneCraftingManager` exist via `AddComponent` in Awake (no prefab edit needed).
- `CloneCraftingUI`: code-built UGUI panel ([C] toggles) — recipe rows with costs, equipped slots with unequip; server-authoritative craft RPC.
- `ServerTickVitals`: passive regen while fullness >= 60 (Muck-style fullness-driven healing). Stamina: no system in clone yet — deferred.
