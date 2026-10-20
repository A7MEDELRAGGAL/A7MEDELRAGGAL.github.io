# DATA_MODELS.md — Clean-Room Data Models & Schema Design

**Scope:** Independent Clean-Room Engine & FPS System Architecture  
**Target Reference Analyzed:** Combat Master (AlfaBravoInc)  
**Methodology:** Pure functional clean-room abstraction. No class, field, or variable names from target artifacts are used. Tunables are original baseline values within an estimated ±10% band of competitive fast-paced FPS games.

---

## 1. Traceability & Classification Ledger

| Component | Status | Source / Derivation |
| :--- | :--- | :--- |
| JumpPad / Launchers existence | **Observation** | Target has `ActorJumpPad` (`link.xml` line 38) |
| Ziplines / Cable traversal existence | **Observation** | Target has `ActorZipLine`, `ZipLineAttachView` (`link.xml` lines 43, 72) |
| Procedural weapon sway existence | **Observation** | Target has `WeaponSway` (`link.xml` line 103) |
| Dual-wielding shooting existence | **Observation** | Target has `DualShootWeapon` (`link.xml` line 97) |
| Modular weapon attachments existence | **Observation** | Target has `HandguardBarrelAttachmentView`, `LaserAttachmentView`, `MagazineAttachmentView`, `ScopeAttachmentView` (`link.xml` lines 99-102) |
| Exact movement speeds (e.g. 4.8 m/s, 8.2 m/s) | **Unknown** in target | **Original Design Value** (synthesized for our clean-room implementation) |
| Exact weapon damage numbers & fire rates | **Unknown** in target | **Original Design Value** (synthesized for our clean-room implementation) |

---

## 2. Character Movement & Traversal System

### 2.1 Clean-Room State Machine (`PlayerLocomotionState`)
```csharp
public enum PlayerLocomotionState
{
    GroundedJog,
    TacticalSprint,
    SlideDecelerating,
    AirborneJump,
    AirborneFall,
    LedgeClimb,
    PropelledByPad,
    TraversingCable
}
```

### 2.2 Movement Parameters (Original Clean-Room Baseline)
*Note: Target binary tunables are UNKNOWN due to encrypted data. The following values are original engineering baseline estimates:*

| Parameter | Baseline Value | Working Tolerance Band (±10%) | Rationale |
| :--- | :--- | :--- | :--- |
| `JogSpeed` | 5.0 m/s | 4.5 – 5.5 m/s | Standard tactical walk |
| `TacticalSprintSpeed` | 8.5 m/s | 7.65 – 9.35 m/s | High-tempo repositioning |
| `SlideEntryImpulse` | 11.0 m/s | 9.9 – 12.1 m/s | Initial forward velocity boost upon crouch |
| `SlideFrictionDrag` | 3.5 m/s² | 3.15 – 3.85 m/s² | Deceleration rate to standard crouch speed |
| `SlideMinCancelTime` | 0.30 s | 0.27 – 0.33 s | Window before jump-cancel is allowed |
| `JumpVerticalVelocity` | 5.8 m/s | 5.22 – 6.38 s | Instantaneous upward impulse |
| `GravityAcceleration` | 24.0 m/s² | 21.6 – 26.4 m/s² | Snappy competitive downward pull |

---

## 3. Weaponry & Ballistic Architecture

### 3.1 Weapon Configuration Schema (`FirearmConfiguration`)
```csharp
[System.Serializable]
public class FirearmConfiguration
{
    public string weaponIdentifier;
    public FirearmFiringMode firingMode;         // SemiAuto, FullAuto, Burst
    public float roundsPerMinute;                 // e.g., 750 RPM
    public int magazineCapacity;                  // e.g., 30 rounds
    public float baseDamage;                      // e.g., 34 HP
    public float headshotBonusMultiplier;         // e.g., 1.5x
    public float adsTransitionTime;               // e.g., 0.18 s
    public float reloadCycleTime;                 // e.g., 1.60 s
    public Vector2 recoilImpulsePerShot;          // (pitch, yaw) in degrees
    public float recoilRecoveryRate;              // recovery speed degrees/sec
}

public enum FirearmFiringMode
{
    SemiAuto,
    FullAuto,
    Burst
}
```

### 3.2 Modular Attachment Schema (`WeaponAttachmentData`)
```csharp
public enum AttachmentCategory
{
    MuzzleDevice,
    BarrelExtension,
    OpticalSight,
    StockSupport,
    AmmunitionMagazine,
    UnderbarrelAccessory
}

[System.Serializable]
public class WeaponAttachmentData
{
    public string attachmentIdentifier;
    public AttachmentCategory category;
    public float adsSpeedModifier = 1.0f;
    public float recoilStabilityModifier = 1.0f;
    public float effectiveRangeModifier = 1.0f;
    public Vector3 opticViewOffset;
    public float targetOpticFov = 55.0f;
}
```

---

## 4. Procedural Weapon Sway & Viewmodel Inertia

### 4.1 Inertia Data Structure (`ViewmodelSwayProfile`)
```csharp
[System.Serializable]
public struct ViewmodelSwayProfile
{
    public float mouseLagResponsiveness;   // Speed of spring following mouse delta
    public float maxTranslationalSway;     // Maximum offset displacement in meters
    public float maxAngularSway;           // Maximum angular roll/pitch tilt in degrees
    public float locomotionBobFrequency;   // Frequency of step-bobbing curve
    public float locomotionBobAmplitude;   // Magnitude of step-bobbing curve
    public float idleBreathingRate;        // Natural breathing frequency
    public float idleBreathingMagnitude;   // Natural breathing offset
}
```

---

## 5. Network Replication State Structure

### 5.1 Replicated Character Packet (`PlayerNetworkSnapshot`)
```csharp
public struct PlayerNetworkSnapshot
{
    public uint networkEntityId;
    public Vector3 kinematicPosition;
    public Vector3 kinematicVelocity;
    public short compressedViewPitch;     // Scaled short (-85 to +85 deg)
    public short compressedViewYaw;       // Scaled short (0 to 360 deg)
    public byte locomotionStateCode;
    public byte currentHealthPoints;      // 0 - 100
    public byte activeWeaponSlotIndex;
    public bool isAimingDownSights;
    public bool isShooting;
}
```
