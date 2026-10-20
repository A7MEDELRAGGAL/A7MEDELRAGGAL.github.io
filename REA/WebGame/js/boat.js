(function (global) {
  'use strict';

  var G = global.G;
  var THREE = global.THREE;

  var TUNE = {
    mass: 620,
    thrust: 4600,
    reverse: 0.45,
    maxSpeed: 14,
    boost: 1.85,
    thrustResponse: 3.2,
    rudder: 4200,
    steerResponse: 5.5,
    buoyancy: 9800,
    maxDraft: 2.4,
    upright: 5200,
    uprightDamp: 2600,
    waterDrag: 0.85,
    angularDrag: 2.2,
    beachDrag: 1.6,
    restDraft: 0.35,
    buoyancyStiffness: 11,
    verticalDamp: 1.7,
    beachLift: 0.25,
    shallowDepth: 0.8,
    maxPitch: 0.72,
    maxRoll: 0.9,
    gravity: 19.6,
    heightUnderWater: 3
  };

  G.Boat = function (scene, world, opts) {
    opts = opts || {};

    this.world = world;
    this.active = true;

    this.position = new THREE.Vector3(opts.x || 6, 0, opts.z || 10);
    this.velocity = new THREE.Vector3();
    this.angularVelocity = new THREE.Vector3();
    this.yaw = opts.yaw || 0;
    this.pitch = 0;
    this.roll = 0;

    this.throttle = 0;
    this.steer = 0;
    this.boosting = false;
    this.driveMode = opts.driveMode || 'moored';

    this.driver = null;
    this.seat = new THREE.Vector3(0, 1.0, 0.2);

    this.group = buildBoatMesh();
    this.group.position.copy(this.position);
    scene.add(this.group);

    this.foam = buildFoam();
    scene.add(this.foam);
    this.foamTimer = 0;

    this.updateTransform();
  };

  function buildBoatMesh() {
    var group = new THREE.Group();

    var hullMat = new THREE.MeshStandardMaterial({ color: 0xb9502f, roughness: 0.72 });
    var trimMat = new THREE.MeshStandardMaterial({ color: 0xf0e0c4, roughness: 0.66 });
    var woodMat = new THREE.MeshStandardMaterial({ color: 0x6d5334, roughness: 0.88 });

    var hull = new THREE.Mesh(new THREE.BoxGeometry(2.6, 0.85, 6.2), hullMat);
    hull.position.y = 0.1;
    group.add(hull);

    var bow = new THREE.Mesh(new THREE.ConeGeometry(1.3, 1.9, 4), hullMat);
    bow.rotation.x = -Math.PI / 2;
    bow.rotation.y = Math.PI / 4;
    bow.position.set(0, 0.12, 3.6);
    bow.scale.set(1, 1, 0.7);
    group.add(bow);

    var gunwale = new THREE.Mesh(new THREE.BoxGeometry(2.72, 0.14, 6.3), trimMat);
    gunwale.position.y = 0.54;
    group.add(gunwale);

    var floor = new THREE.Mesh(new THREE.BoxGeometry(2.2, 0.1, 5.2), woodMat);
    floor.position.y = 0.3;
    group.add(floor);

    var benchF = new THREE.Mesh(new THREE.BoxGeometry(2.3, 0.12, 0.5), woodMat);
    benchF.position.set(0, 0.72, -1.4);
    group.add(benchF);

    var benchB = benchF.clone();
    benchB.position.z = 1.2;
    group.add(benchB);

    var console3d = new THREE.Mesh(new THREE.BoxGeometry(0.9, 0.7, 0.4), woodMat);
    console3d.position.set(0, 0.85, -0.2);
    group.add(console3d);

    var wheel = new THREE.Mesh(new THREE.TorusGeometry(0.24, 0.045, 6, 16), trimMat);
    wheel.position.set(0, 1.2, -0.25);
    wheel.rotation.x = -0.5;
    group.add(wheel);

    var lamp = new THREE.Mesh(new THREE.SphereGeometry(0.12, 10, 8),
      new THREE.MeshStandardMaterial({ color: 0xffe9a8, emissive: 0xffcc66, emissiveIntensity: 0.6 }));
    lamp.position.set(0, 1.6, 0.1);
    group.add(lamp);

    var post = new THREE.Mesh(new THREE.CylinderGeometry(0.05, 0.05, 1.5, 6), trimMat);
    post.position.set(0, 1.2, 0.1);
    group.add(post);

    group.userData.wheel = wheel;
    return group;
  }

  function buildFoam() {
    var geo = new THREE.RingGeometry(1.4, 3.2, 24);
    geo.rotateX(-Math.PI / 2);
    var mat = new THREE.MeshBasicMaterial({
      color: 0xffffff,
      transparent: true,
      opacity: 0.28,
      depthWrite: false
    });
    var mesh = new THREE.Mesh(geo, mat);
    mesh.frustumCulled = false;
    return mesh;
  }

  G.Boat.prototype.tune = TUNE;

  G.Boat.prototype.occupy = function (player) {
    this.driver = player;
    this.driveMode = 'driven';
  };

  G.Boat.prototype.vacate = function () {
    this.driver = null;
    this.driveMode = 'drifting';
    this.throttle = 0;
    this.steer = 0;
    this.boosting = false;
  };

  G.Boat.prototype.readInput = function (dt) {
    if (!this.driver) {
      this.throttle = G.damp(this.throttle, 0, 4, dt);
      this.steer = G.damp(this.steer, 0, 6, dt);
      this.boosting = false;
      return;
    }

    var input = G.Input;
    var target = input.axis('KeyS', 'KeyW');
    var steerTarget = input.axis('KeyA', 'KeyD');

    this.throttle = G.damp(this.throttle, target, TUNE.thrustResponse, dt);
    this.steer = G.damp(this.steer, steerTarget, TUNE.steerResponse, dt);
    this.boosting = input.down('ShiftLeft') || input.down('ShiftRight');
  };

  G.Boat.prototype.update = function (dt, t) {
    this.readInput(dt);

    var surface = this.world.seaLevel + G.Waves.height(this.position.x, this.position.z, t);
    var ground = this.world.terrainHeight(this.position.x, this.position.z);

    var draft = surface - this.position.y;
    var afloat = G.clamp01(draft / TUNE.restDraft);
    var waterColumn = Math.max(0, surface - ground);
    var shallow = G.clamp01(waterColumn / TUNE.shallowDepth);
    var beached = ground > surface - 0.55;
    var drive = afloat * shallow;

    this.sinking = !beached && this.position.y - surface > TUNE.heightUnderWater;

    if (this.throttle !== 0 && drive > 0.02) this.applyThrust(drive);

    this.applyHydroDrag(afloat, beached);
    this.applyOrientation(afloat, dt);
    this.integrate(dt, t, surface, ground, beached);

    this.updateFoam(dt, t, afloat);
    this.updateTransform();

    this.grounded = beached;
    this.draft = draft;
    this.drive = drive;
  };

  G.Boat.prototype.applyThrust = function (afloat) {
    var forward = new THREE.Vector3(Math.sin(this.yaw), 0, Math.cos(this.yaw));
    var speed = this.velocity.dot(forward);
    var limit = TUNE.maxSpeed * (this.throttle < 0 ? TUNE.reverse : 1);
    var over = G.clamp01((Math.abs(speed) - limit) / Math.max(1, limit));

    var power = TUNE.thrust * G.lerp(1, 0.25, over) * (this.boosting && this.throttle > 0 ? TUNE.boost : 1);
    var signedPower = this.throttle < 0 ? -power * TUNE.reverse : power;

    this.velocity.addScaledVector(forward, signedPower * afloat * 0.0009);
  };

  G.Boat.prototype.applyHydroDrag = function (afloat, grounded) {
    var drag = grounded ? TUNE.beachDrag : TUNE.waterDrag;
    var scale = grounded ? 1 : afloat;

    this.velocity.multiplyScalar(Math.max(0, 1 - drag * scale * 0.016));
    this.angularVelocity.multiplyScalar(Math.max(0, 1 - TUNE.angularDrag * scale * 0.016));

    if (grounded) this.velocity.y *= Math.max(0, 1 - 0.25);
  };

  G.Boat.prototype.applyOrientation = function (afloat, dt) {
    if (afloat <= 0.02) return;

    var speed = Math.abs(this.velocity.x * Math.sin(this.yaw) + this.velocity.z * Math.cos(this.yaw));
    var authority = G.lerp(0.3, 1, G.clamp01(speed / TUNE.maxSpeed));

    this.yaw -= this.steer * TUNE.rudder * authority * afloat * dt * 0.28;

    var targetPitch = G.clamp(-this.velocity.y * 0.045, -TUNE.maxPitch, TUNE.maxPitch);
    var targetRoll = G.clamp(-this.steer * 0.28 * authority, -TUNE.maxRoll, TUNE.maxRoll);

    this.pitch = G.damp(this.pitch, targetPitch, 3.4, dt);
    this.roll = G.damp(this.roll, targetRoll, 4.2, dt);
  };

  G.Boat.prototype.integrate = function (dt, t, surface, ground, beached) {
    this.position.x += this.velocity.x * dt;
    this.position.z += this.velocity.z * dt;

    if (beached) {
      this.position.y = G.damp(this.position.y, ground + TUNE.beachLift, 6, dt);
      this.velocity.y = 0;
    } else if (this.sinking) {
      this.velocity.y -= TUNE.gravity * dt;
      this.position.y += this.velocity.y * dt;
    } else {
      var restY = surface - TUNE.restDraft;
      var error = restY - this.position.y;
      this.velocity.y += error * TUNE.buoyancyStiffness * dt;
      this.velocity.y *= Math.max(0, 1 - TUNE.verticalDamp * dt);
      this.position.y += this.velocity.y * dt;

      if (this.position.y > surface + TUNE.heightUnderWater) {
        this.position.y = surface + TUNE.heightUnderWater;
        if (this.velocity.y > 0) this.velocity.y = 0;
      }
    }
  };

  G.Boat.prototype.updateFoam = function (dt, t, afloat) {
    this.foamTimer -= dt;
    if (this.foamTimer > 0) return;
    this.foamTimer = 0.05;

    var speed = this.velocity.length();
    this.foam.visible = afloat > 0.05 && speed > 1.2;

    if (!this.foam.visible) return;

    var surface = this.world.seaLevel + G.Waves.height(this.position.x, this.position.z, t);
    this.foam.position.set(this.position.x, surface + 0.06, this.position.z);
    var scale = 0.7 + Math.min(2.4, speed * 0.18);
    this.foam.scale.set(scale, 1, scale * 1.35);
    this.foam.material.opacity = G.clamp(0.1 + speed * 0.03, 0, 0.42);
  };

  G.Boat.prototype.updateTransform = function () {
    this.group.position.copy(this.position);
    this.group.rotation.set(this.pitch, this.yaw, this.roll, 'YXZ');

    var wheel = this.group.userData.wheel;
    if (wheel) wheel.rotation.z = -this.steer * 2.2;
  };

  G.Boat.prototype.speedKmh = function () {
    return Math.sqrt(this.velocity.x * this.velocity.x + this.velocity.z * this.velocity.z) * 3.6;
  };

  G.Boat.prototype.draftMeters = function () {
    return Math.max(0, this.draft || 0);
  };

  G.Boat.prototype.canBoard = function (player) {
    if (this.driver) return false;
    var dx = this.position.x - player.position.x;
    var dz = this.position.z - player.position.z;
    return Math.sqrt(dx * dx + dz * dz) < 6.5;
  };

})(window);
