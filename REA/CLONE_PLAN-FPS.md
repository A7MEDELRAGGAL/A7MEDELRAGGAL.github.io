# CLONE_PLAN-FPS.md — Clean-Room Fast-Paced FPS Implementation Plan

> **Note:** this plan was created in a separate work round and is NOT the Muck-clone (fishing) track plan. The fishing-clone track plan lives in `CLONE_PLAN.md`.

**Target Reference:** Combat Master (AlfaBravoInc)  
**Project Objective:** Build an original, high-velocity competitive FPS engine and gameplay prototype completely from scratch.  
**Strict Clean-Room Constraint:** Zero code, assets, audio, models, or text copied from the target. All tunables are original baseline values calibrated within the ±10% feel band. Original class taxonomy and architectural design.

---

## 1. Compliance & Original-Only Rules

| Boundary | Rule |
| :--- | :--- |
| **Code** | 100% original C# code. No decompiled or disassembled IL2CPP functions from `GameAssembly.dll` copied or transliterated. |
| **Assets & Art** | Procedural / original primitives (ProBuilder/Blender blockouts). Zero asset extraction from `data.unity3d` or `StreamingAssets\Bundles`. |
| **Audio** | Procedural synthesizers / original CC0 effects for gunshots, reloads, and slide audio. |
| **Names & Identifiers** | All class names, component names, and UI copy are original (`KineticMotor`, `ArmoryWeapon`, etc.). |
| **Tunables** | Speeds, damage, recoil, and timers are defined independently within the ±10% competitive FPS band. |

---

## 2. Implementation Roadmap

### Phase 1: Core Kinetic Controller & Camera Dynamics (Week 1)
- [ ] **First-Person Camera Rig:**
  - Mouse look pitch (-85° to +85°) and yaw smoothing.
  - Dynamic FOV transitions: Base (90°), Tactical Sprint (98°), Slide (94°), ADS (55° - 65°).
  - Head bobbing during sprint, land shake on falling impact.
- [ ] **Kinetic Character Motor (`KineticCharacterController`):**
  - Jog (4.8 m/s), Tactical Sprint (8.2 m/s).
  - Power Slide state machine: Crouching at speed > 6.0 m/s triggers slide with 10.5 m/s impulse, decaying at 3.2 m/s².
  - Slide-Cancel mechanic: Jump input at t > 0.35s cancels slide and launches forward bunny-hop.
  - Ledge mantling: SphereCast detection of obstacles between 0.8m and 1.8m height with automatic smooth hoist.

### Phase 2: Procedural Weaponry & Ballistics (Week 2)
- [ ] **Procedural Viewmodel Socket:**
  - Camera-relative weapon anchor with spring-damper sway (lagging behind mouse rotations).
  - Walk/Sprint procedural bobbing curves.
  - ADS transition lerp with sub-200ms snappy response.
- [ ] **Hitscan & Ballistic Gunplay Engine (`ArmoryGunController`):**
  - Raycast hit detection with surface material tagging (Concrete, Metal, Flesh).
  - Procedural kickback recoil (Z translation impulse + pitch rotation kick).
  - Dynamic spread bloom (increases with continuous fire and movement, tightens to 0 in ADS).
  - Hitmarker audio feedback + UI screen crosshair tick.
  - Tracer pooling system (`TracerPoolManager`) rendering fast line-renderer trails.

### Phase 3: Vertical Traversal & Environmental Actors (Week 3)
- [ ] **Environmental Launch Pads (`PropulsionJumpPad`):**
  - Trigger volume applying directional impulse ($V_y = 15.5\text{ m/s}$, $V_{xz} = 12.0\text{ m/s}$) with temporary air-control dampening.
- [ ] **Spline Zipline System (`CableZipline`):**
  - Two-point anchor spline allowing player snap-in, gliding at constant 14.0 m/s with detachment jump.
- [ ] **Moving Platform Registry (`DynamicPlatform`):**
  - Character kinematic attachment ensuring correct momentum inheritance when riding escalators and elevators.

### Phase 4: Modular Gunsmith & Attachments (Week 4)
- [ ] **Modular Weapon Assembly:**
  - Base weapon prefab with socket transforms: `MuzzleSocket`, `OpticSocket`, `MagSocket`, `BarrelSocket`.
  - Attachment ScriptableObjects modifying weapon recoil curves, ADS speed, and optics FOV.
  - Dynamic reticle rendering (Reflex red-dot shader, holographic sight frame).

### Phase 5: Arena Map Blockout & Bot Sandbox (Week 5)
- [ ] **Compact 3-Lane Arena ("Courtyard Sector"):**
  - Symmetrical competitive layout designed for fast traversal (12s spawn-to-spawn).
  - High ground jump-pad access, interior CQB corridors, and long rifle sightlines.
- [ ] **Reactive Target Dummies & Simple AI:**
  - Standard humanoid health (100 HP + 3-plate armor).
  - Hit reaction animations / ragdoll on death.
  - Basic patrol and line-of-sight tracking for combat testing.

### Phase 6: Networking Spine & Multiplayer Replication (Week 6)
- [ ] **Authoritative Client-Server Netcode:**
  - Client-side prediction for local movement and gunplay.
  - Server validation of position history and hit registration (lag compensation).
  - State replication for remote player view pitch/yaw, weapon firing bursts, and health.

---

## 3. Immediate Milestone Deliverables (Starting Phase 1)

1. **Unity Asset Hierarchy:**
   - `Assets/Scripts/Kinetic/KineticCharacterController.cs`
   - `Assets/Scripts/Kinetic/KineticCameraLook.cs`
   - `Assets/Scripts/Armory/ArmoryGunController.cs`
   - `Assets/Scripts/Armory/ProceduralWeaponSway.cs`
   - `Assets/Scripts/Environment/PropulsionJumpPad.cs`
2. **Playable Testing Scene:**
   - Flat testing greybox arena with jump pads, obstacles to slide under, ramps, and reactive hit-targets.
