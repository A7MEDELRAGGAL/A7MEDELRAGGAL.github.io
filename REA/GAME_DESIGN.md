# GAME_DESIGN.md — Competitive High-Velocity FPS Game Design

**Document Status:** Clean-Room Original Design Specification  
**Reference Target Analyzed:** Combat Master (AlfaBravoInc)  
**Naming Note:** Any working titles used in previous drafts (such as "Project Kinetic") are purely project-internal labels and DO NOT exist in the analyzed target artifacts. All game mode structures, map guidelines, and feel principles described below are clean-room original designs.

---

## 1. Traceability & Classification Ledger

| Design Element | Classification | Source / Proof |
| :--- | :--- | :--- |
| Jump pads in environment | **Observation** | Target contains `CombatMaster.Battle.Gameplay.Actors.ActorJumpPad` (`link.xml` line 38) |
| Ziplines in environment | **Observation** | Target contains `CombatMaster.Battle.Gameplay.Actors.ActorZipLine` (`link.xml` line 43) |
| Moving escalators / platforms | **Observation** | Target contains `MovingPlatformRegistry`, `EscalatorsRegistry` (`link.xml` lines 44-45) |
| Search & Destroy mode presence | **Observation** | Target contains `SNDMapLevelDesign`, `ActorPlantedBomb` (`link.xml` lines 39, 87) |
| Capture The Flag mode presence | **Observation** | Target contains `CTFMapLevelDesign`, `CTFScoreBoard` (`link.xml` line 30) |
| Battle Royale & Gulag presence | **Observation** | Target contains `BRMapLevelDesign`, `BRMapGulagLevelDesign` (`link.xml` lines 27-28) |
| Fast ADS (<200ms) & Movement Feel | **Inference** | Inferred from genre benchmarking of Combat Master gameplay and fast viewmodel offsets. |
| Specific weapon stats (RPM, damage) | **Unknown** in target | Designed from scratch as original balanced FPS values. |

---

## 2. Core Gameplay Loop & Mechanics

### 2.1 The High-Velocity Traversal Loop
1. **Fluid Momentum:**
   - Players transition seamlessly between Jogging, Tactical Sprinting, and Ground Slides.
   - Crouching while sprinting initiates a high-momentum low-friction slide.
   - A jump command during mid-slide cancels the slide into a forward hop, retaining momentum without entering sprint wind-up delay.
2. **Environmental Elevation:**
   - **Launch Pads:** Stepping on a launch pad trigger immediately applies a vertical and horizontal impulse vector to bypass ground obstacles.
   - **Cable Ziplines:** Interacting with a cable suspends the player into a fixed-velocity slide along the spline trajectory toward high vantage points.

### 2.2 Gunplay Dynamics
- **Hitscan Primary Ballistics:** Direct raycasting against colliders with instantaneous damage application to provide crisp competitive feedback.
- **Dynamic ADS Transitions:** Snappy transitions between hipfire and aim-down-sights (sub-200ms) to reward aggressive target acquisition.
- **Visual Feedback:**
  - Screen crosshair expands dynamically based on movement speed and continuous firing.
  - Directional hit indicators flash toward incoming damage origins.
  - Confirmed hits trigger a crosshair tick and distinct audio impact chirp.

---

## 3. Competitive Game Modes (Clean-Room Rules)

### 3.1 Mode Specifications
- **Objective Bomb Defusal (5v5):**
  - Attacking team attempts to plant a device at Site A or Site B; defending team guards or defuses.
  - One life per round; round ends on defusal, detonation, or full team elimination.
- **Fast Arena Deathmatch:**
  - 10-player free-for-all or 5v5 team battle. Fast respawns (2.0s) with 3-second spawn invulnerability.
- **Territory Control (Flag / Zones):**
  - Symmetrical capture points that grant continuous scoring while defended.

---

## 4. Map Layout Principles

1. **Symmetrical 3-Lane Architecture:**
   - **Lane A (Interior):** Tight corridors, doorways, and low ceilings favoring shotguns, SMGs, and rapid sliding.
   - **Lane B (Courtyard):** Central open area with launch pads and long sightlines favoring assault rifles and snipers.
   - **Lane C (Exterior Flank):** Perimeter route with zipline connectors and vertical balconies for tactical flanking.
2. **Pacing Benchmarks:**
   - Spawn-to-encounter traversal time: 6 to 9 seconds under tactical sprint.
   - Spawn-to-opposing-spawn time: 14 to 18 seconds.
