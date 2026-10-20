# REA digest — Muck Assembly-CSharp (reference for the How to Fish track)

Artifact sha256: 5f9d0dc1e0e72f5493013c92d05ed9fb8628019f54f8bc02162dad0d2f20810c
Module MVID: 8db17789-d6d5-48de-86ab-e696a5ff6b2b

Extracted from the REA managed-member evidence. Structure, names and tunables are
used as a mechanics reference; all art, text and code in this project is original.

## rea-muck-mechanics.md — not present in this artifact

## PlayerMovement  (61 fields, 50 methods)

### fields
- `playerJumpSmokeFx` : class:25  [flags 6]
- `footstepFx` : class:25  [flags 6]
- `playerCam` : class:29  [flags 6]
- `orientation` : class:29  [flags 6]
- `rb` : class:61  [flags 1]
- `dead` : bool  [flags 6]
- `moveSpeed` : r4  [flags 1]
- `maxWalkSpeed` : r4  [flags 1]
- `maxRunSpeed` : r4  [flags 1]
- `maxSpeed` : r4  [flags 1]
- `grounded` : bool  [flags 6]
- `whatIsGround` : valuetype:109  [flags 6]
- `extraGravity` : r4  [flags 6]
- `crouchScale` : valuetype:81  [flags 1]
- `playerScale` : valuetype:81  [flags 1]
- `slideForce` : r4  [flags 1]
- `slideCounterMovement` : r4  [flags 1]
- `readyToJump` : bool  [flags 1]
- `jumpCooldown` : r4  [flags 1]
- `jumpForce` : r4  [flags 1]
- `jumps` : i4  [flags 1]
- `x` : r4  [flags 1]
- `y` : r4  [flags 1]
- `mouseDeltaX` : r4  [flags 1]
- `mouseDeltaY` : r4  [flags 1]
- `<jumping>k__BackingField` : bool  [flags 1]
- `<sliding>k__BackingField` : bool  [flags 1]
- `<crouching>k__BackingField` : bool  [flags 1]
- `<sprinting>k__BackingField` : bool  [flags 1]
- `normalVector` : valuetype:81  [flags 1]
- `ps` : class:85  [flags 6]
- `psEmission` : valuetype:357  [flags 1]
- `playerCollider` : class:157  [flags 1]
- `fallSpeed` : r4  [flags 1]
- `playerSmokeFx` : class:25  [flags 6]
- `playerStatus` : class:552  [flags 1]
- `<Instance>k__BackingField` : class:932  [flags 17]
- `distance` : r4  [flags 1]
- `swimSpeed` : r4  [flags 1]
- `pushed` : bool  [flags 1]
- `onRamp` : bool  [flags 1]
- `extraJumps` : i4  [flags 1]
- `resetJumpCounter` : i4  [flags 1]
- `jumpCounterResetTime` : i4  [flags 1]
- `counterMovement` : r4  [flags 1]
- `threshold` : r4  [flags 1]
- `readyToCounterX` : i4  [flags 1]
- `readyToCounterY` : i4  [flags 1]
- `cancelling` : bool  [flags 1]
- `maxSlopeAngle` : r4  [flags 1]
- `airborne` : bool  [flags 1]
- `onGround` : bool  [flags 1]
- `surfing` : bool  [flags 1]
- `cancellingGrounded` : bool  [flags 1]
- `cancellingSurf` : bool  [flags 1]
- `delay` : r4  [flags 1]
- `groundCancel` : i4  [flags 1]
- `wallCancel` : i4  [flags 1]
- `surfCancel` : i4  [flags 1]
- `whatIsHittable` : valuetype:109  [flags 6]
- `vel` : r4  [flags 1]

### methods
- `bool get_jumping()`
- `void set_jumping(bool)`
- `bool get_sliding()`
- `void set_sliding(bool)`
- `bool get_crouching()`
- `void set_crouching(bool)`
- `bool get_sprinting()`
- `void set_sprinting(bool)`
- `class:932 get_Instance()`
- `void set_Instance(class:932)`
- `void Awake()`
- `void Start()`
- `void Update()`
- `valuetype:245 GetInput()`
- `void SetInput(valuetype:245, bool, bool, bool)`
- `void CheckInput()`
- `void StartCrouch()`
- `void StopCrouch()`
- `void FootSteps()`
- `void WaterMovement()`
- `bool IsUnderWater()`
- `void Movement(r4, r4)`
- `void PushPlayer()`
- `void ResetPush()`
- `void RampMovement(valuetype:245)`
- `void ResetJump()`
- `void Jump()`
- `void JumpCooldown()`
- `void CounterMovement(r4, r4, valuetype:245)`
- `bool IsHoldingAgainstHorizontalVel(valuetype:245)`
- `bool IsHoldingAgainstVerticalVel(valuetype:245)`
- `valuetype:245 FindVelRelativeToLook()`
- `bool IsFloor(valuetype:81)`
- `bool IsSurf(valuetype:81)`
- `bool IsWall(valuetype:81)`
- `bool IsRoof(valuetype:81)`
- `void OnCollisionEnter(class:77)`
- `void OnCollisionStay(class:77)`
- `void UpdateCollisionChecks()`
- `void StopGrounded()`
- `void StopSurf()`
- `valuetype:81 GetVelocity()`
- `r4 GetFallSpeed()`
- `class:157 GetPlayerCollider()`
- `class:29 GetPlayerCamTransform()`
- `valuetype:81 HitPoint()`
- `bool IsCrouching()`
- `bool IsDead()`
- `class:61 GetRb()`
- `void .ctor()`

## Boat  (31 fields, 20 methods)

### fields
- `status` : valuetype:1368  [flags 6]
- `Instance` : class:44  [flags 22]
- `mapItem` : class:708  [flags 6]
- `gemMap` : class:708  [flags 6]
- `objectivePing` : class:25  [flags 6]
- `boatPing` : class:512  [flags 6]
- `rand` : class:904  [flags 1]
- `chestSpawner` : class:1148  [flags 6]
- `holes` : class:25[]  [flags 6]
- `gemTexture` : class:125  [flags 6]
- `boatTexture` : class:125  [flags 6]
- `gemsDiscovered` : bool  [flags 1]
- `guardians` : class:113<class:1128>  [flags 6]
- `countPlayers` : class:148  [flags 6]
- `<waterHeight>k__BackingField` : r4  [flags 1]
- `heightUnderWater` : r4  [flags 1]
- `rb` : class:61  [flags 1]
- `dragonSpawnPos` : class:29  [flags 6]
- `cinematicCamera` : class:129  [flags 6]
- `dragonBoss` : class:784  [flags 6]
- `rbTransform` : class:29  [flags 6]
- `waterSfx` : class:25  [flags 6]
- `dragonLandingPosition` : class:29  [flags 6]
- `landingNodes` : class:29[]  [flags 6]
- `wheel` : class:25  [flags 6]
- `sinking` : bool  [flags 1]
- `amp` : r4  [flags 1]
- `wheelInteract` : class:256  [flags 1]
- `wheelPing` : class:512  [flags 6]
- `repairs` : class:133[]  [flags 1]
- `boatMapMarker` : class:1600  [flags 6]

### methods
- `r4 get_waterHeight()`
- `void set_waterHeight(r4)`
- `void Start()`
- `void SlowUpdate()`
- `void SendMarkShip()`
- `void SendShipFound()`
- `void SendMarkGems()`
- `void SendBoatFinished()`
- `void UpdateShipStatus(valuetype:1372, i4)`
- `void LeaveIsland()`
- `void FixedUpdate()`
- `void MoveBoat()`
- `void BoatFinished(i4)`
- `bool CheckBoatFullyRepaired()`
- `void CheckForMap()`
- `void MarkGems()`
- `void CheckFound()`
- `void FindShip()`
- `void MarkShip()`
- `void .ctor()`

## Mob  (29 fields, 41 methods)

### fields
- `mobType` : class:784  [flags 6]
- `<target>k__BackingField` : class:29  [flags 1]
- `<targetPlayerId>k__BackingField` : i4  [flags 1]
- `attackCooldown` : r4  [flags 6]
- `id` : i4  [flags 6]
- `<ready>k__BackingField` : bool  [flags 1]
- `<multiplier>k__BackingField` : r4  [flags 1]
- `<bossMultiplier>k__BackingField` : r4  [flags 1]
- `stopOnAttack` : bool  [flags 6]
- `<animator>k__BackingField` : class:221  [flags 1]
- `<agent>k__BackingField` : class:389  [flags 1]
- `<hitable>k__BackingField` : class:644  [flags 1]
- `attacking` : bool  [flags 1]
- `bossType` : valuetype:1512  [flags 6]
- `countedKill` : bool  [flags 6]
- `attackAnimations` : class:481[]  [flags 6]
- `<attackTimes>k__BackingField` : r4[]  [flags 1]
- `footstepFx` : class:25  [flags 6]
- `distance` : r4  [flags 1]
- `footstepFrequency` : r4  [flags 6]
- `knocked` : bool  [flags 6]
- `defaulAngularSpeed` : r4  [flags 1]
- `oldAccel` : r4  [flags 1]
- `oldAngularSpeed` : r4  [flags 1]
- `nRangedAttacks` : i4  [flags 6]
- `currentAttackType` : valuetype:1508  [flags 1]
- `offsetPosition` : valuetype:81  [flags 1]
- `offsetDir` : valuetype:81  [flags 1]
- `syncSpeed` : r4  [flags 1]

### methods
- `class:29 get_target()`
- `void set_target(class:29)`
- `i4 get_targetPlayerId()`
- `void set_targetPlayerId(i4)`
- `bool get_ready()`
- `void set_ready(bool)`
- `r4 get_multiplier()`
- `void set_multiplier(r4)`
- `r4 get_bossMultiplier()`
- `void set_bossMultiplier(r4)`
- `class:221 get_animator()`
- `void set_animator(class:221)`
- `class:389 get_agent()`
- `void set_agent(class:389)`
- `class:644 get_hitable()`
- `void set_hitable(class:644)`
- `void TestSpawn()`
- `bool IsBuff()`
- `void Start()`
- `void SetSpeed(r4)`
- `void Awake()`
- `r4[] get_attackTimes()`
- `void set_attackTimes(r4[])`
- `void Update()`
- `void FootSteps()`
- `void ExtraUpdate()`
- `void Knockback(valuetype:81)`
- `void StopKnockback()`
- `void LateUpdate()`
- `bool IsAttacking()`
- `bool IsRangedAttacking()`
- `void Attack(i4, i4)`
- `void FinishAttacking()`
- `void Animate()`
- `void SetDestination(valuetype:81)`
- `void SetTarget(i4)`
- `void SetPosition(valuetype:81)`
- `void UpdateOffsetPosition()`
- `void SetId(i4)`
- `i4 GetId()`
- `void .ctor()`

## MobType  (21 fields, 3 methods)

### fields
- `<id>k__BackingField` : i4  [flags 1]
- `name` : string  [flags 6]
- `mobPrefab` : class:25  [flags 6]
- `behaviour` : valuetype:1516  [flags 6]
- `ranged` : bool  [flags 6]
- `rangedCooldown` : r4  [flags 6]
- `startAttackDistance` : r4  [flags 6]
- `startRangedAttackDistance` : r4  [flags 6]
- `maxAttackDistance` : r4  [flags 6]
- `speed` : r4  [flags 6]
- `spawnTime` : r4  [flags 6]
- `minAttackAngle` : r4  [flags 6]
- `sharpDefense` : r4  [flags 6]
- `defense` : r4  [flags 6]
- `knockbackThreshold` : r4  [flags 6]
- `ignoreBuilds` : bool  [flags 6]
- `followPlayerDistance` : r4  [flags 6]
- `followPlayerAccuracy` : r4  [flags 6]
- `onlyRangedInRangedPattern` : bool  [flags 6]
- `weaknesses` : valuetype:1520[]  [flags 6]
- `boss` : bool  [flags 6]

### methods
- `i4 get_id()`
- `void set_id(i4)`
- `void .ctor()`

## InventoryItem  (44 fields, 6 methods)

### fields
- `important` : bool  [flags 6]
- `id` : i4  [flags 6]
- `name` : string  [flags 6]
- `description` : string  [flags 6]
- `type` : valuetype:1476  [flags 6]
- `tier` : i4  [flags 6]
- `sprite` : class:473  [flags 6]
- `material` : class:93  [flags 6]
- `mesh` : class:297  [flags 6]
- `rotationOffset` : valuetype:81  [flags 6]
- `positionOffset` : valuetype:81  [flags 6]
- `scale` : r4  [flags 6]
- `stackable` : bool  [flags 6]
- `amount` : i4  [flags 6]
- `max` : i4  [flags 6]
- `resourceDamage` : i4  [flags 6]
- `attackDamage` : i4  [flags 6]
- `attackSpeed` : r4  [flags 6]
- `attackRange` : r4  [flags 6]
- `sharpness` : r4  [flags 6]
- `craftable` : bool  [flags 6]
- `unlockWithFirstRequirementOnly` : bool  [flags 6]
- `requirements` : class:1492[]  [flags 6]
- `craftAmount` : i4  [flags 6]
- `stationRequirement` : class:708  [flags 6]
- `buildable` : bool  [flags 6]
- `grid` : bool  [flags 6]
- `prefab` : class:25  [flags 6]
- `buildRotation` : valuetype:81  [flags 6]
- `processable` : bool  [flags 6]
- `processType` : valuetype:1480  [flags 6]
- `processedItem` : class:708  [flags 6]
- `processTime` : r4  [flags 6]
- `heal` : r4  [flags 6]
- `hunger` : r4  [flags 6]
- `stamina` : r4  [flags 6]
- `armor` : i4  [flags 6]
- `swingFx` : bool  [flags 6]
- `bowComponent` : class:172  [flags 6]
- `armorComponent` : class:168  [flags 6]
- `fuel` : class:716  [flags 6]
- `tag` : valuetype:1484  [flags 6]
- `rarity` : valuetype:1488  [flags 6]
- `attackTypes` : valuetype:1520[]  [flags 6]

### methods
- `void Copy(class:708, i4)`
- `bool IsArmour()`
- `valuetype:201 GetOutlineColor()`
- `bool Compare(class:708)`
- `string GetAmount()`
- `void .ctor()`

## PlayerStatus  (43 fields, 57 methods)

### fields
- `hp` : r4  [flags 6]
- `maxHp` : i4  [flags 6]
- `<draculaStacks>k__BackingField` : i4  [flags 1]
- `shield` : r4  [flags 6]
- `maxShield` : i4  [flags 6]
- `<stamina>k__BackingField` : r4  [flags 1]
- `<maxStamina>k__BackingField` : r4  [flags 1]
- `<hunger>k__BackingField` : r4  [flags 1]
- `<maxHunger>k__BackingField` : r4  [flags 1]
- `dead` : bool  [flags 1]
- `<strength>k__BackingField` : i4  [flags 1]
- `<speed>k__BackingField` : i4  [flags 1]
- `staminaRegenRate` : r4  [flags 1]
- `staminaDrainRate` : r4  [flags 1]
- `staminaBoost` : r4  [flags 1]
- `running` : bool  [flags 1]
- `jumpDrain` : r4  [flags 1]
- `hungerDrainRate` : r4  [flags 1]
- `healingDrainMultiplier` : r4  [flags 1]
- `staminaDrainMultiplier` : r4  [flags 1]
- `healing` : bool  [flags 1]
- `healingRate` : r4  [flags 1]
- `readyToRegenShield` : bool  [flags 1]
- `shieldRegenRate` : r4  [flags 1]
- `regenShieldDelay` : r4  [flags 1]
- `player` : class:932  [flags 1]
- `Instance` : class:552  [flags 22]
- `invincible` : bool  [flags 1]
- `oneShotThreshold` : r4  [flags 1]
- `oneShotProtectionCooldown` : r4  [flags 1]
- `protectionActive` : bool  [flags 1]
- `readyToAdrenalineBoost` : bool  [flags 1]
- `<adrenalineBoost>k__BackingField` : bool  [flags 1]
- `playerRagdoll` : class:25  [flags 6]
- `drownParticles` : class:25  [flags 6]
- `underwaterAudio` : class:65  [flags 6]
- `leafParticles` : class:25  [flags 6]
- `windParticles` : class:25  [flags 6]
- `underwater` : bool  [flags 1]
- `armor` : class:708[]  [flags 6]
- `armorTotal` : r4  [flags 1]
- `currentSpeedArmorMultiplier` : r4  [flags 6]
- `currentChunkArmorMultiplier` : r4  [flags 6]

### methods
- `i4 get_draculaStacks()`
- `void set_draculaStacks(i4)`
- `r4 get_stamina()`
- `void set_stamina(r4)`
- `r4 get_maxStamina()`
- `void set_maxStamina(r4)`
- `r4 get_hunger()`
- `void set_hunger(r4)`
- `r4 get_maxHunger()`
- `void set_maxHunger(r4)`
- `i4 get_strength()`
- `void set_strength(i4)`
- `i4 get_speed()`
- `void set_speed(i4)`
- `void Awake()`
- `void Respawn()`
- `void StopInvincible()`
- `void UpdateStats()`
- `void Damage(i4, i4, bool)`
- `void DealDamage(i4, i4, bool, i4)`
- `void HandleDamage(i4, i4, bool, i4)`
- `i4 OneShotProtection(i4)`
- `void ActivateProtection()`
- `void StopAdrenaline()`
- `void ReadyAdrenaline()`
- `bool get_adrenalineBoost()`
- `void set_adrenalineBoost(bool)`
- `void PlayerDied(i4, i4)`
- `bool IsPlayerDead()`
- `void DropAllItems(class:113<class:704>)`
- `bool IsFullyHealed()`
- `i4 HpAndShield()`
- `i4 MaxHpAndShield()`
- `r4 GetArmorRatio()`
- `void Update()`
- `void EnterOcean()`
- `void SlowUpdate()`
- `void OutOfMap()`
- `void Shield()`
- `void Hunger()`
- `void Healing()`
- `void Stamina()`
- `void Heal(i4)`
- `void Eat(class:708)`
- `void RegenShield()`
- `r4 GetHpRatio()`
- `r4 GetShieldRatio()`
- `r4 GetStaminaRatio()`
- `r4 GetHungerRatio()`
- `void Jump()`
- `void AddKill(i4, class:760)`
- `void Dracula()`
- `void UpdateArmor(i4, i4)`
- `void CheckArmorSetBonus()`
- `bool CanRun()`
- `bool CanJump()`
- `void .ctor()`

## Player  (19 fields, 8 methods)

### fields
- `id` : i4  [flags 6]
- `username` : string  [flags 6]
- `ready` : bool  [flags 6]
- `joined` : bool  [flags 6]
- `loading` : bool  [flags 6]
- `color` : valuetype:201  [flags 6]
- `pos` : valuetype:81  [flags 6]
- `yOrientation` : r4  [flags 6]
- `xOrientation` : r4  [flags 6]
- `running` : bool  [flags 6]
- `dead` : bool  [flags 6]
- `stats` : class:137<string,i4>  [flags 6]
- `lastPingTime` : r4  [flags 6]
- `powerups` : i4[]  [flags 6]
- `armor` : i4[]  [flags 6]
- `totalArmor` : i4  [flags 6]
- `steamId` : valuetype:413  [flags 6]
- `allStats` : string[]  [flags 22]
- `currentHp` : i4  [flags 6]

### methods
- `void .ctor(i4, string, valuetype:201)`
- `void .ctor(i4, string, valuetype:201, valuetype:413)`
- `void InitStats()`
- `void PingPlayer()`
- `void UpdateArmor(i4, i4)`
- `void Died()`
- `i4 Damage(i4)`
- `void .cctor()`

## PlayerSave  (41 fields, 5 methods)

### fields
- `cameraShake` : bool  [flags 6]
- `fov` : i4  [flags 6]
- `sensMultiplier` : r4  [flags 6]
- `invertedMouseHor` : bool  [flags 6]
- `invertedMouseVert` : bool  [flags 6]
- `grass` : bool  [flags 6]
- `tutorial` : bool  [flags 6]
- `forward` : valuetype:233  [flags 6]
- `backwards` : valuetype:233  [flags 6]
- `left` : valuetype:233  [flags 6]
- `right` : valuetype:233  [flags 6]
- `jump` : valuetype:233  [flags 6]
- `sprint` : valuetype:233  [flags 6]
- `interact` : valuetype:233  [flags 6]
- `inventory` : valuetype:233  [flags 6]
- `map` : valuetype:233  [flags 6]
- `leftClick` : valuetype:233  [flags 6]
- `rightClick` : valuetype:233  [flags 6]
- `shadowQuality` : i4  [flags 6]
- `shadowResolution` : i4  [flags 6]
- `shadowDistance` : i4  [flags 6]
- `shadowCascade` : i4  [flags 6]
- `textureQuality` : i4  [flags 6]
- `antiAliasing` : i4  [flags 6]
- `softParticles` : bool  [flags 6]
- `bloom` : i4  [flags 6]
- `motionBlur` : bool  [flags 6]
- `ambientOcclusion` : bool  [flags 6]
- `resolution` : valuetype:245  [flags 6]
- `refreshRate` : i4  [flags 6]
- `fullscreen` : bool  [flags 6]
- `fullscreenMode` : i4  [flags 6]
- `vSync` : i4  [flags 6]
- `fpsLimit` : i4  [flags 6]
- `<volume>k__BackingField` : i4  [flags 1]
- `<music>k__BackingField` : i4  [flags 1]
- `xp` : i4  [flags 6]
- `money` : i4  [flags 6]
- `bossesBeatEasy` : bool[]  [flags 1]
- `bossesBeatNormal` : bool[]  [flags 1]
- `bossesBeatHard` : bool[]  [flags 1]

### methods
- `i4 get_volume()`
- `void set_volume(i4)`
- `i4 get_music()`
- `void set_music(i4)`
- `void .ctor()`

## UseInventory  (17 fields, 11 methods)

### fields
- `Instance` : class:732  [flags 22]
- `hitBox` : class:344  [flags 6]
- `animator` : class:221  [flags 6]
- `swingTrail` : class:69  [flags 6]
- `swingSfx` : class:632  [flags 6]
- `chargeSfx` : class:65  [flags 6]
- `eatSfx` : class:65  [flags 6]
- `eatingParticles` : class:85  [flags 6]
- `eatingEmission` : valuetype:357  [flags 1]
- `velocity` : valuetype:397  [flags 1]
- `eatTime` : r4  [flags 1]
- `attackTime` : r4  [flags 1]
- `chargeTime` : r4  [flags 1]
- `meshRenderer` : class:197  [flags 6]
- `meshFilter` : class:161  [flags 6]
- `renderTransform` : class:29  [flags 6]
- `currentItem` : class:708  [flags 1]

### methods
- `void Awake()`
- `void SetWeapon(class:708)`
- `void StopUse()`
- `void Update()`
- `void Use()`
- `void StartParticles()`
- `void UseButtonUp()`
- `void ReleaseWeapon()`
- `void FinishEating()`
- `bool IsAnimationPlaying(string)`
- `void .ctor()`

## InventoryUI  (19 fields, 31 methods)

### fields
- `inventoryParent` : class:29  [flags 6]
- `hotkeysTransform` : class:29  [flags 6]
- `armorTransform` : class:29  [flags 6]
- `leftTransform` : class:29  [flags 6]
- `armorCells` : class:704[]  [flags 6]
- `hotkeyCells` : class:704[]  [flags 6]
- `allCells` : class:704[]  [flags 6]
- `leftHand` : class:704  [flags 6]
- `arrows` : class:704  [flags 6]
- `hotbar` : class:700  [flags 6]
- `cells` : class:113<class:704>  [flags 6]
- `currentMouseItem` : class:708  [flags 6]
- `mouseItemSprite` : class:205  [flags 6]
- `mouseItemText` : class:45  [flags 6]
- `Instance` : class:712  [flags 22]
- `<pickupCooldown>k__BackingField` : bool  [flags 1]
- `throwForce` : r4  [flags 54]
- `backDrop` : class:25  [flags 6]
- `CraftingUi` : class:968  [flags 6]

### methods
- `void Awake()`
- `void Start()`
- `bool CanPickup(class:708)`
- `bool IsInventoryFull()`
- `bool get_pickupCooldown()`
- `void set_pickupCooldown(bool)`
- `void CooldownPickup()`
- `void ResetCooldown()`
- `void CheckInventoryAlmostFull()`
- `void PickupItem(class:708)`
- `void PlaceItem(class:708)`
- `void UpdateMouseSprite()`
- `void Update()`
- `void DropItem(class:185)`
- `void DropItemIntoWorld(class:708)`
- `void FillCellList()`
- `void UpdateAllCells()`
- `void ToggleInventory()`
- `i4 AddItemToInventory(class:708)`
- `i4 GetMoney()`
- `void UseMoney(i4)`
- `bool IsCraftable(class:708)`
- `void CraftItem(class:708)`
- `void RemoveItem(class:708)`
- `bool CanRepair(class:708[])`
- `bool Repair(class:708[])`
- `bool HasItem(class:708)`
- `bool AddArmor(class:708)`
- `bool HoldingItem()`
- `void .ctor()`
- `void .cctor()`

## DayCycle  (21 fields, 8 methods)

### fields
- `alwaysDay` : bool  [flags 6]
- `dayDuration` : r4  [flags 22]
- `nightDuration` : r4  [flags 6]
- `timeSpeed` : r4  [flags 6]
- `time` : r4  [flags 22]
- `sun` : class:29  [flags 6]
- `sky` : class:93  [flags 6]
- `skyOffset` : r4  [flags 6]
- `water` : class:197  [flags 6]
- `waterColor` : valuetype:201  [flags 6]
- `waterShallowColor` : valuetype:201  [flags 6]
- `skybox` : class:93  [flags 6]
- `sunLight` : class:265  [flags 6]
- `moonLight` : class:265  [flags 6]
- `dayFog` : valuetype:201  [flags 6]
- `nightFog` : valuetype:201  [flags 6]
- `waterFog` : valuetype:201  [flags 6]
- `desiredSunlight` : r4  [flags 1]
- `desiredMoonlight` : r4  [flags 1]
- `<totalTime>k__BackingField` : r4  [flags 17]
- `waterGraph` : class:269  [flags 6]

### methods
- `void Awake()`
- `r4 get_totalTime()`
- `void set_totalTime(r4)`
- `void Update()`
- `void SunLight()`
- `r4 EvaluateWaterColor(r4)`
- `void .ctor()`
- `void .cctor()`

## ItemManager  (16 fields, 14 methods)

### fields
- `dropItem` : class:25  [flags 6]
- `list` : class:137<i4,class:25>  [flags 6]
- `debug` : class:25  [flags 6]
- `allScriptableItems` : class:708[]  [flags 6]
- `allScriptableDropTables` : class:740[]  [flags 6]
- `powerupsWhite` : class:720[]  [flags 6]
- `powerupsBlue` : class:720[]  [flags 6]
- `powerupsOrange` : class:720[]  [flags 6]
- `allItems` : class:137<i4,class:708>  [flags 6]
- `allPowerups` : class:137<i4,class:720>  [flags 6]
- `allDropTables` : class:137<i4,class:740>  [flags 6]
- `stringToPowerupId` : class:137<string,i4>  [flags 6]
- `random` : class:904  [flags 1]
- `attatchDebug` : bool  [flags 6]
- `Instance` : class:388  [flags 22]
- `currentId` : i4  [flags 22]

### methods
- `void Awake()`
- `void InitAllItems()`
- `void InitAllDropTables()`
- `void InitAllPowerups()`
- `i4 AddPowerupsToList(class:720[], i4)`
- `class:708 GetItemByName(string)`
- `i4 GetNextId()`
- `void DropItem(i4, i4, i4, i4)`
- `void DropItemAtPosition(i4, i4, valuetype:81, i4)`
- `void DropResource(i4, i4, i4)`
- `void DropPowerupAtPosition(i4, valuetype:81, i4)`
- `class:720 GetRandomPowerup(r4, r4, r4)`
- `bool PickupItem(i4)`
- `void .ctor()`

## PlayerInput  (20 fields, 26 methods)

### fields
- `xRotation` : r4  [flags 1]
- `<sensitivity>k__BackingField` : r4  [flags 1]
- `sensMultiplier` : r4  [flags 22]
- `desiredX` : r4  [flags 1]
- `<x>k__BackingField` : r4  [flags 1]
- `<y>k__BackingField` : r4  [flags 1]
- `<jumping>k__BackingField` : bool  [flags 1]
- `<crouching>k__BackingField` : bool  [flags 1]
- `<sprinting>k__BackingField` : bool  [flags 1]
- `mouseScroll` : r4  [flags 1]
- `playerCam` : class:29  [flags 1]
- `orientation` : class:29  [flags 1]
- `playerMovement` : class:932  [flags 1]
- `<Instance>k__BackingField` : class:928  [flags 17]
- `active` : bool  [flags 6]
- `actualWallRotation` : r4  [flags 1]
- `wallRotationVel` : r4  [flags 1]
- `cameraRot` : valuetype:81  [flags 6]
- `wallRunRotation` : r4  [flags 1]
- `mouseOffsetY` : r4  [flags 6]

### methods
- `r4 get_sensitivity()`
- `void set_sensitivity(r4)`
- `r4 get_x()`
- `void set_x(r4)`
- `r4 get_y()`
- `void set_y(r4)`
- `bool get_jumping()`
- `void set_jumping(bool)`
- `bool get_crouching()`
- `void set_crouching(bool)`
- `bool get_sprinting()`
- `void set_sprinting(bool)`
- `class:928 get_Instance()`
- `void set_Instance(class:928)`
- `void Awake()`
- `void Update()`
- `void FixedUpdate()`
- `void StopInput()`
- `void MyInput()`
- `void Look()`
- `valuetype:245 GetAxisInput()`
- `r4 GetMouseX()`
- `void SetMouseOffset(r4)`
- `r4 GetMouseOffset()`
- `void .ctor()`
- `void .cctor()`

## OtherInput  (20 fields, 16 methods)

### fields
- `<craftingState>k__BackingField` : valuetype:1440  [flags 1]
- `handcrafts` : class:968  [flags 6]
- `furnace` : class:968  [flags 6]
- `workbench` : class:968  [flags 6]
- `anvil` : class:968  [flags 6]
- `fletch` : class:968  [flags 6]
- `chest` : class:968  [flags 6]
- `cauldron` : class:968  [flags 6]
- `hotbar` : class:25  [flags 6]
- `crosshair` : class:25  [flags 6]
- `lockCamera` : bool  [flags 22]
- `_currentCraftingUiMenu` : class:968  [flags 1]
- `currentChest` : class:112  [flags 6]
- `chestsOpened` : class:137<i4,bool>  [flags 1]
- `Instance` : class:520  [flags 22]
- `<paused>k__BackingField` : bool  [flags 1]
- `pauseUi` : class:25  [flags 6]
- `settingsUi` : class:25  [flags 6]
- `UiSfx` : class:1312  [flags 6]
- `craftingOverlay` : class:237  [flags 6]

### methods
- `valuetype:1440 get_craftingState()`
- `void set_craftingState(valuetype:1440)`
- `void Awake()`
- `void Unpause()`
- `void Pause()`
- `bool get_paused()`
- `void set_paused(bool)`
- `bool OtherUiActive()`
- `void Update()`
- `void ToggleInventory(valuetype:1440)`
- `void CenterInventory()`
- `bool IsAnyMenuOpen()`
- `void CheckStationUnlock()`
- `i4 GetStationId()`
- `void FindCurrentCraftingState()`
- `void .ctor()`

## MoveCamera  (28 fields, 23 methods)

### fields
- `player` : class:29  [flags 6]
- `offset` : valuetype:81  [flags 6]
- `desyncOffset` : valuetype:81  [flags 6]
- `vaultOffset` : valuetype:81  [flags 6]
- `cam` : class:129  [flags 1]
- `rb` : class:61  [flags 1]
- `playerInput` : class:928  [flags 6]
- `<Instance>k__BackingField` : class:924  [flags 17]
- `cinematic` : bool  [flags 6]
- `desiredTilt` : r4  [flags 1]
- `tilt` : r4  [flags 1]
- `<state>k__BackingField` : valuetype:1584  [flags 1]
- `desiredDeathPos` : valuetype:81  [flags 1]
- `target` : class:29  [flags 1]
- `desiredSpectateRotation` : valuetype:81  [flags 1]
- `playerTarget` : class:29  [flags 1]
- `whatIsGround` : valuetype:109  [flags 6]
- `spectatingId` : i4  [flags 1]
- `yRotation` : r4  [flags 1]
- `desiredX` : r4  [flags 1]
- `cameraRot` : valuetype:81  [flags 6]
- `desiredBob` : valuetype:81  [flags 1]
- `bobOffset` : valuetype:81  [flags 1]
- `bobSpeed` : r4  [flags 1]
- `bobMultiplier` : r4  [flags 1]
- `bobConstant` : r4  [flags 33]
- `mainCam` : class:129  [flags 6]
- `gunCamera` : class:129  [flags 6]

### methods
- `class:924 get_Instance()`
- `void set_Instance(class:924)`
- `void Start()`
- `void LateUpdate()`
- `valuetype:1584 get_state()`
- `void set_state(valuetype:1584)`
- `void PlayerRespawn(valuetype:81)`
- `void PlayerDied(class:29)`
- `void StartSpectating()`
- `void SpectateCamera()`
- `void FreeCam()`
- `void FreeCamRotation()`
- `bool TryStartFreecam()`
- `bool TryStartLockedCam()`
- `void SpectateToggle(i4)`
- `void PlayerDeathCamera()`
- `void PlayerCamera()`
- `void MoveGun()`
- `void UpdateFov(r4)`
- `void BobOnce(valuetype:81)`
- `void UpdateBob()`
- `valuetype:81 ClampVector(valuetype:81, r4, r4)`
- `void .ctor()`

## Settings  (36 fields, 41 methods)

### fields
- `backBtn` : class:285  [flags 6]
- `camShake` : class:496  [flags 6]
- `fov` : class:1140  [flags 6]
- `sens` : class:1140  [flags 6]
- `invertedHor` : class:496  [flags 6]
- `invertedVer` : class:496  [flags 6]
- `grass` : class:496  [flags 6]
- `tutorial` : class:496  [flags 6]
- `forward` : class:140  [flags 6]
- `backward` : class:140  [flags 6]
- `left` : class:140  [flags 6]
- `right` : class:140  [flags 6]
- `jump` : class:140  [flags 6]
- `sprint` : class:140  [flags 6]
- `interact` : class:140  [flags 6]
- `inventory` : class:140  [flags 6]
- `map` : class:140  [flags 6]
- `leftClick` : class:140  [flags 6]
- `rightClick` : class:140  [flags 6]
- `shadowQuality` : class:1068  [flags 6]
- `shadowResolution` : class:1068  [flags 6]
- `shadowDistance` : class:1068  [flags 6]
- `shadowCascades` : class:1068  [flags 6]
- `textureQuality` : class:1068  [flags 6]
- `antiAliasing` : class:1068  [flags 6]
- `softParticles` : class:496  [flags 6]
- `bloom` : class:1068  [flags 6]
- `motionBlur` : class:496  [flags 6]
- `ao` : class:496  [flags 6]
- `resolution` : class:592  [flags 6]
- `fullscreen` : class:496  [flags 6]
- `fullscreenMode` : class:1068  [flags 6]
- `vSync` : class:1068  [flags 6]
- `fpsLimit` : class:1140  [flags 6]
- `volume` : class:1140  [flags 6]
- `music` : class:1140  [flags 6]

### methods
- `void Start()`
- `void UpdateSave()`
- `void UpdateCamShake()`
- `void UpdateInverted()`
- `void UpdateGrass()`
- `void UpdateTutorial()`
- `void UpdateSens()`
- `void UpdateFov()`
- `void UpdateForwardKey()`
- `void UpdateBackwardKey()`
- `void UpdateLeftKey()`
- `void UpdateRightKey()`
- `void UpdateJumpKey()`
- `void UpdateSprintKey()`
- `void UpdateInteractKey()`
- `void UpdateInventoryKey()`
- `void UpdateMapKey()`
- `void UpdateLeftClickKey()`
- `void UpdateRightClickKey()`
- `void UpdateShadowQuality()`
- `void UpdateShadowResolution()`
- `void UpdateShadowDistance()`
- `void UpdateShadowCascades()`
- `void UpdateTextureRes()`
- `void UpdateAntiAliasing()`
- `void UpdateSoftParticles()`
- `void UpdateBloom()`
- `void UpdateMotionBlur()`
- `void UpdateAO()`
- `void UpdateFullscreen()`
- `void UpdateFullscreenMode()`
- `void UpdateVSync()`
- `void UpdateMaxFps()`
- `void UpdateVolume()`
- `void UpdateMusic()`
- `r4 IntToFloat(i4)`
- `i4 FloatToInt(r4)`
- `i4 BoolToInt(bool)`
- `bool IntToBool(i4)`
- `void ResetSaveFile()`
- `void .ctor()`
