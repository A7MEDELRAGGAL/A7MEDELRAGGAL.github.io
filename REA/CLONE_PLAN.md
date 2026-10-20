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
