(function (global) {
  'use strict';

  var G = global.G;
  var THREE = global.THREE;

  // Tuning recovered from the original movement controller (REA, Muck Assembly-CSharp):
  //   moveSpeed 3500, maxWalkSpeed 6.5, maxRunSpeed 13, maxSpeed 6.5, extraGravity 5,
  //   slideForce 800, slideCounterMovement 0.12, jumpCooldown 0.25, jumpForce 12,
  //   swimSpeed 50, counterMovement 0.14, threshold 0.01, maxSlopeAngle 50, delay 5.
  // The original drives a force-based body: input is normalised and pushed into the
  // rigidbody, and the walk/run caps do the limiting. We keep that shape.
  var TUNE = {
    moveSpeed: 3500,
    maxWalkSpeed: 6.5,
    maxRunSpeed: 13,
    maxSpeed: 6.5,
    extraGravity: 5,
    slideForce: 800,
    slideCounterMovement: 0.12,
    jumpCooldown: 0.25,
    jumpForce: 12,
    swimSpeed: 50,
    counterMovement: 0.14,
    threshold: 0.01,
    maxSlopeAngle: 50,
    delay: 5,
    crouchScale: 1.05,

    accel: 70,
    slideAccel: 16,
    swimAccel: 30,
    jumpVelocity: 6,
    gravity: 19.6,
    height: 1.75,
    radius: 0.34,
    stepUp: 0.45,
    groundProbe: 0.6,
    swimDepth: 1.15,
    surfInputDeadzone: 0.05,
    counterCooldown: 0.25,
    airControl: 0.35
  };

  G.Player = function (scene, world, opts) {
    opts = opts || {};

    this.world = world;
    this.vitals = opts.vitals || new G.Vitals();
    this.inventory = opts.inventory || new G.Inventory();

    this.position = new THREE.Vector3(0, 3, 0);
    this.velocity = new THREE.Vector3();
    this.yaw = 0;
    this.pitch = 0;

    this.grounded = false;
    this.crouching = false;
    this.sprinting = false;
    this.swimming = false;
    this.inWater = false;
    this.depth = 0;
    this.sliding = false;
    this.surfing = false;
    this.onRamp = false;

    this.jumps = 0;
    this.jumpCooldown = 0;
    this.jumpCounterReset = 0;
    this.counterCooldownX = 0;
    this.counterCooldownY = 0;

    this.boat = null;
    this.walkPhase = 0;
    this.swing = 0;

    this.mesh = buildBody();
    this.mesh.position.copy(this.position);
    scene.add(this.mesh);

    this.rodGroup = new THREE.Group();
    this.rod = buildRod();
    this.rodGroup.add(this.rod);
    this.mesh.add(this.rodGroup);

    this.handAnchor = new THREE.Object3D();
    this.handAnchor.position.set(0.42, 1.15, 0.28);
    this.mesh.add(this.handAnchor);
    this.rodGroup.position.set(0, 0, 0);
    this.rodGroup.rotation.set(-0.35, 0.2, 0.1);
  };

  function buildBody() {
    var group = new THREE.Group();

    var skin = new THREE.MeshStandardMaterial({ color: 0xd8a882, roughness: 0.85 });
    var shirt = new THREE.MeshStandardMaterial({ color: 0x3f6f86, roughness: 0.9 });
    var pants = new THREE.MeshStandardMaterial({ color: 0x37424f, roughness: 0.9 });

    var torso = new THREE.Mesh(new THREE.BoxGeometry(0.62, 0.78, 0.34), shirt);
    torso.position.y = 1.12;
    group.add(torso);

    var head = new THREE.Mesh(new THREE.BoxGeometry(0.34, 0.34, 0.32), skin);
    head.position.y = 1.68;
    group.add(head);

    var hat = new THREE.Mesh(new THREE.BoxGeometry(0.4, 0.1, 0.38),
      new THREE.MeshStandardMaterial({ color: 0xc9b06a, roughness: 0.9 }));
    hat.position.y = 1.88;
    group.add(hat);

    var legL = new THREE.Mesh(new THREE.BoxGeometry(0.22, 0.78, 0.24), pants);
    legL.position.set(-0.16, 0.39, 0);
    group.add(legL);

    var legR = legL.clone();
    legR.position.x = 0.16;
    group.add(legR);

    var armL = new THREE.Mesh(new THREE.BoxGeometry(0.16, 0.6, 0.18), skin);
    armL.position.set(-0.39, 1.16, 0.02);
    group.add(armL);

    var armR = armL.clone();
    armR.position.x = 0.39;
    group.add(armR);

    group.userData.legL = legL;
    group.userData.legR = legR;
    group.userData.armL = armL;
    group.userData.armR = armR;
    group.userData.torso = torso;

    return group;
  }

  function buildRod() {
    var group = new THREE.Group();

    var wood = new THREE.MeshStandardMaterial({ color: 0x7a5a34, roughness: 0.85 });
    var tip = new THREE.MeshStandardMaterial({ color: 0xcfd8dc, roughness: 0.4, metalness: 0.5 });

    var shaft = new THREE.Mesh(new THREE.CylinderGeometry(0.022, 0.03, 2.3, 6), wood);
    shaft.rotation.x = Math.PI / 2;
    shaft.position.z = 1.0;
    group.add(shaft);

    var tipRod = new THREE.Mesh(new THREE.CylinderGeometry(0.006, 0.02, 0.5, 6), tip);
    tipRod.rotation.x = Math.PI / 2;
    tipRod.position.z = 2.35;
    group.add(tipRod);

    var reel = new THREE.Mesh(new THREE.CylinderGeometry(0.09, 0.09, 0.07, 10),
      new THREE.MeshStandardMaterial({ color: 0x2f3742, roughness: 0.5, metalness: 0.4 }));
    reel.rotation.z = Math.PI / 2;
    reel.position.set(0, -0.05, 0.55);
    group.add(reel);

    group.userData.tipLocal = new THREE.Vector3(0, 0, 2.6);
    return group;
  }

  G.Player.prototype.tune = TUNE;

  G.Player.prototype.spawn = function (x, z) {
    var h = this.world.terrainHeight(x, z);
    this.position.set(x, Math.max(h, 0) + 1.2, z);
    this.velocity.set(0, 0, 0);
    this.mesh.position.copy(this.position);
  };

  G.Player.prototype.look = function (dx, dy, sensitivity) {
    this.yaw -= dx * sensitivity;
    this.pitch -= dy * sensitivity;
    this.pitch = G.clamp(this.pitch, -1.45, 1.35);
    if (this.yaw > Math.PI) this.yaw -= G.TAU;
    if (this.yaw < -Math.PI) this.yaw += G.TAU;
  };

  G.Player.prototype.forwardVector = function (out) {
    out = out || new THREE.Vector3();
    return out.set(Math.sin(this.yaw), 0, Math.cos(this.yaw));
  };

  G.Player.prototype.aimVector = function (out) {
    out = out || new THREE.Vector3();
    var cp = Math.cos(this.pitch);
    out.set(Math.sin(this.yaw) * cp, Math.sin(this.pitch), Math.cos(this.yaw) * cp);
    return out.normalize();
  };

  G.Player.prototype.update = function (dt, t) {
    this.vitals.tick(dt);

    if (this.boat) {
      this.updateOnBoat(dt, t);
      this.animate(dt, 0);
      return;
    }

    this.readInput(dt);
    this.integrate(dt, t);
    this.animate(dt, this.horizontalSpeed());
  };

  G.Player.prototype.readInput = function (dt) {
    var input = G.Input;

    var moveZ = input.axis('KeyS', 'KeyW');
    var moveX = input.axis('KeyA', 'KeyD');

    this.inputX = moveX;
    this.inputY = moveZ;

    this.crouching = input.down('KeyC') && !this.swimming;
    this.sprinting = (input.down('ShiftLeft') || input.down('ShiftRight'))
      && moveZ > 0.1 && !this.crouching && !this.swimming && this.vitals.stamina > 0;

    this.vitals.sprinting = this.sprinting;

    var forward = this.forwardVector();
    var right = new THREE.Vector3(forward.z, 0, -forward.x);

    var wish = new THREE.Vector3();
    wish.addScaledVector(forward, moveZ);
    wish.addScaledVector(right, moveX);
    if (wish.lengthSq() > 1) wish.normalize();
    this.wish = wish;

    this.jumpRequested = input.hit('Space');
    this.jumpCooldown = Math.max(0, this.jumpCooldown - dt);
    this.counterCooldownX = Math.max(0, this.counterCooldownX - dt);
    this.counterCooldownY = Math.max(0, this.counterCooldownY - dt);

    if (this.jumpCounterReset > 0) {
      this.jumpCounterReset -= dt;
      if (this.jumpCounterReset <= 0) this.jumps = 0;
    }
  };

  G.Player.prototype.integrate = function (dt, t) {
    var wish = this.wish || new THREE.Vector3();
    var v = this.velocity;

    var ground = this.world.terrainHeight(this.position.x, this.position.z);
    var surface = this.world.seaLevel + G.Waves.height(this.position.x, this.position.z, t);
    var waterDepth = Math.max(0, surface - ground);

    this.depth = waterDepth;
    this.inWater = waterDepth > 0;

    var chestDepth = surface - (this.position.y + 1.0);
    this.swimming = this.inWater && chestDepth > TUNE.swimDepth;

    if (this.swimming) {
      this.swimStep(dt, t);
      return;
    }

    var slopeNormal = this.groundProbe(t);
    var slopeAngle = slopeNormal ? this.slopeAngleOf(slopeNormal) : 0;
    this.sliding = this.grounded && slopeAngle > TUNE.maxSlopeAngle;

    // momentum: the original pushes normalised input into the body and lets the
    // walk/run caps do the limiting.
    var cap = this.sprinting ? TUNE.maxRunSpeed : TUNE.maxWalkSpeed;
    if (this.crouching) cap = TUNE.maxWalkSpeed * 0.55;
    cap = Math.min(cap, this.sprinting ? TUNE.maxRunSpeed : TUNE.maxSpeed);

    var accel = TUNE.accel * (this.crouching ? 0.55 : 1) * (this.grounded ? 1 : TUNE.airControl);
    v.x += wish.x * accel * dt;
    v.z += wish.z * accel * dt;

    if (this.sliding) {
      var downhill = new THREE.Vector3(slopeNormal.x, 0, slopeNormal.z);
      if (downhill.lengthSq() > 0.0001) {
        downhill.normalize();
        v.x += downhill.x * TUNE.slideAccel * dt;
        v.z += downhill.z * TUNE.slideAccel * dt;
      }
    }

    this.applyCounterMovement(dt);

    var horizontal = Math.sqrt(v.x * v.x + v.z * v.z);
    if (horizontal > cap) {
      var k = cap / horizontal;
      v.x *= k;
      v.z *= k;
    }

    if (this.jumpRequested && this.jumpCooldown <= 0 && this.vitals.canJump()) {
      if (this.grounded || this.jumps > 0) {
        v.y = TUNE.jumpVelocity;
        if (!this.grounded) this.jumps--;
        this.grounded = false;
        this.jumpCooldown = TUNE.jumpCooldown;
        this.jumpCounterReset = 1.2;
        this.vitals.spendStamina(6);
        this.startSwing();
      }
    }

    var gravity = TUNE.gravity + (this.grounded ? 0 : TUNE.extraGravity);
    v.y -= gravity * dt;

    this.position.x += v.x * dt;
    this.position.z += v.z * dt;
    this.position.y += v.y * dt;

    this.resolveGround(ground);

    this.mesh.position.copy(this.position);
    this.mesh.rotation.y = this.yaw;

    this.surfing = this.grounded && this.onRamp
      && Math.abs(this.inputX) < TUNE.surfInputDeadzone
      && Math.abs(this.inputY) < TUNE.surfInputDeadzone
      && !this.crouching;
  };

  G.Player.prototype.groundProbe = function (t) {
    var origin = this.position + new THREE.Vector3(0, 0.5, 0);
    var step = 1.2;
    var hx = this.world.terrainHeight(this.position.x + step, this.position.z);
    var hz = this.world.terrainHeight(this.position.x, this.position.z + step);
    var h0 = this.world.terrainHeight(this.position.x, this.position.z);

    var nx = h0 - hx;
    var nz = h0 - hz;
    var len = Math.sqrt(nx * nx + 1 + nz * nz);
    void origin;
    void t;
    return { x: nx / len, y: 1 / len, z: nz / len };
  };

  G.Player.prototype.slopeAngleOf = function (normal) {
    var dot = G.clamp(normal.y, -1, 1);
    return Math.acos(dot) * 180 / Math.PI;
  };

  G.Player.prototype.applyCounterMovement = function (dt) {
    var v = this.velocity;
    var push = TUNE.accel * TUNE.counterMovement;

    if (this.inputX !== 0) {
      var againstX = this.inputX * v.x < 0 && Math.abs(v.x) > TUNE.threshold;
      if (againstX) {
        if (this.counterCooldownX <= 0 && this.isBlockedAhead('x', this.inputX > 0 ? 1 : -1)) {
          v.x = v.x * 2 + this.inputX * push * dt;
          this.counterCooldownX = TUNE.counterCooldown;
        } else {
          v.x += this.inputX * push * dt;
        }
      }
    }

    if (this.inputY !== 0) {
      var againstZ = this.inputY * v.z < 0 && Math.abs(v.z) > TUNE.threshold;
      if (againstZ) {
        if (this.counterCooldownY <= 0 && this.isBlockedAhead('z', this.inputY > 0 ? 1 : -1)) {
          v.z = v.z * 2 + this.inputY * push * dt;
          this.counterCooldownY = TUNE.counterCooldown;
        } else {
          v.z += this.inputY * push * dt;
        }
      }
    }

    void dt;
  };

  G.Player.prototype.isBlockedAhead = function (axis, sign) {
    var reach = 0.9;
    var x = this.position.x;
    var z = this.position.z;

    if (axis === 'x') x += sign * reach;
    else z += sign * reach;

    var h = this.world.terrainHeight(x, z);
    return h - this.position.y > TUNE.height * 0.6;
  };

  G.Player.prototype.swimStep = function (dt, t) {
    var v = this.velocity;
    var wish = this.wish || new THREE.Vector3();

    var forward = this.forwardVector();
    var right = new THREE.Vector3(forward.z, 0, -forward.x);

    var staminaFactor = this.vitals.stamina > 0 ? 1 : 0.5;
    var accel = TUNE.swimAccel * staminaFactor;

    v.x += (forward.x * wish.z + right.x * wish.x) * accel * dt;
    v.z += (forward.z * wish.z + right.z * wish.x) * accel * dt;

    var vertical = 0;
    if (G.Input.down('Space')) vertical = 1;
    else if (G.Input.down('KeyC')) vertical = -1;

    if (vertical > 0) {
      v.y = Math.max(v.y, 2);
    } else {
      v.y -= TUNE.gravity * 0.35 * dt;
    }

    v.x *= Math.max(0, 1 - 2.2 * dt);
    v.z *= Math.max(0, 1 - 2.2 * dt);

    this.position.x += v.x * dt;
    this.position.z += v.z * dt;
    this.position.y += v.y * dt;

    var ground = this.world.terrainHeight(this.position.x, this.position.z);
    if (this.position.y <= ground) {
      this.position.y = ground;
      if (v.y < 0) v.y = 0;
      this.grounded = true;
    } else {
      this.grounded = false;
    }

    this.mesh.position.copy(this.position);
    this.mesh.rotation.y = this.yaw;
    void t;
  };

  G.Player.prototype.resolveGround = function (ground) {
    var v = this.velocity;

    if (this.position.y <= ground) {
      this.position.y = ground;
      if (v.y < 0) v.y = 0;
      this.grounded = true;
      if (this.jumps <= 0) this.jumpCounterReset = 0;
    } else {
      this.grounded = false;
    }
  };

  G.Player.prototype.horizontalSpeed = function () {
    return Math.sqrt(this.velocity.x * this.velocity.x + this.velocity.z * this.velocity.z);
  };

  G.Player.prototype.updateOnBoat = function (dt, t) {
    var boat = this.boat;
    if (!boat || !boat.active) {
      this.leaveBoat();
      return;
    }

    this.position.set(
      boat.position.x + boat.seat.x,
      boat.position.y + boat.seat.y,
      boat.position.z + boat.seat.z
    );

    this.velocity.set(boat.velocity.x, boat.velocity.y, boat.velocity.z);
    this.mesh.position.copy(this.position);
    this.mesh.rotation.y = boat.yaw;
    void dt;
    void t;
  };

  G.Player.prototype.animate = function (dt, speed) {
    var data = this.mesh.userData;

    if (this.swimming) {
      data.armL.rotation.x = -1.2 + Math.sin(this.walkPhase * 2) * 0.5;
      data.armR.rotation.x = -1.2 - Math.sin(this.walkPhase * 2) * 0.5;
      data.legL.rotation.x = Math.sin(this.walkPhase * 2) * 0.4;
      data.legR.rotation.x = -Math.sin(this.walkPhase * 2) * 0.4;
      this.walkPhase += dt * 4;
      this.mesh.rotation.x = Math.PI / 2 * 0.35;
      return;
    }

    this.mesh.rotation.x = 0;

    var ratio = G.clamp01(speed / TUNE.maxWalkSpeed);
    this.walkPhase += dt * (3 + ratio * 7) * (speed > 0.2 ? 1 : 0);

    var swing = Math.sin(this.walkPhase) * 0.7 * ratio;
    data.legL.rotation.x = swing;
    data.legR.rotation.x = -swing;

    if (this.sliding) {
      data.armL.rotation.x = -0.5;
      data.armR.rotation.x = -0.5;
      data.legL.rotation.x = 0.3;
      data.legR.rotation.x = -0.3;
    } else if (this.swing > 0) {
      this.swing -= dt;
      var punch = Math.sin(G.clamp01(this.swing / 0.35) * Math.PI);
      data.armR.rotation.x = -2.2 * punch;
      data.armL.rotation.x = -2.2 * punch;
    } else {
      data.armL.rotation.x = -swing * 0.7;
      data.armR.rotation.x = swing * 0.7;
    }

    var crouchDrop = this.crouching ? 0.28 : 0;
    data.torso.position.y = 1.12 - crouchDrop;
    data.legL.position.y = 0.39 - crouchDrop * 0.5;
    data.legR.position.y = 0.39 - crouchDrop * 0.5;
  };

  G.Player.prototype.startSwing = function () {
    this.swing = 0.35;
  };

  G.Player.prototype.enterBoat = function (boat) {
    if (!boat || this.boat) return false;

    this.boat = boat;
    boat.occupy(this);
    this.position.set(
      boat.position.x + boat.seat.x,
      boat.position.y + boat.seat.y,
      boat.position.z + boat.seat.z
    );
    G.showToast('At the helm — W/S throttle, A/D steer, Shift boost');
    return true;
  };

  G.Player.prototype.leaveBoat = function () {
    if (!this.boat) return;

    var boat = this.boat;
    boat.vacate();

    var dir = new THREE.Vector3(Math.sin(boat.yaw), 0, Math.cos(boat.yaw)).multiplyScalar(-2.6);
    var x = boat.position.x + dir.x;
    var z = boat.position.z + dir.z;
    var ground = this.world.terrainHeight(x, z);

    this.position.set(x, Math.max(ground, 0) + 0.4, z);
    this.velocity.set(0, 0, 0);
    this.boat = null;
    G.showToast('Stepped ashore');
  };

  G.Player.prototype.canBoard = function () {
    return !this.boat;
  };

  G.Player.prototype.respawn = function () {
    var angle = Math.random() * G.TAU;
    var r = 12 + Math.random() * 6;
    this.spawn(Math.cos(angle) * r, Math.sin(angle) * r);
  };

})(window);
