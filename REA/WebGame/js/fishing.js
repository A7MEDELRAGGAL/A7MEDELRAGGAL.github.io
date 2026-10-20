(function (global) {
  'use strict';

  var G = global.G;
  var THREE = global.THREE;

  var STATE = {
    IDLE: 'idle',
    CHARGING: 'charging',
    THROWING: 'throwing',
    BAITING: 'baiting',
    BITE: 'bite',
    HOOKED: 'hooked',
    REELING: 'reeling',
    CAUGHT: 'caught',
    MISSED: 'missed'
  };

  // The rod model follows the original's structure: the line has a length, the
  // bait is a physical point with drag, reeling is a held action with a ramp,
  // and the fish's pull builds tension on the line.
  var TUNE = {
    minLineLength: 1.5,
    maxLineLength: 26,
    lineReelStepLength: 0.55,
    lineReelDamping: 2.2,
    holdReelSpeed: 4.2,
    reelRampUp: 1.6,
    reelRampDown: 2.4,
    lineThrowSpeed: 17,
    baitDrag: 0.9,
    baitStuckDist: 1.5,
    baitStuckTime: 0.55,
    maxTension: 1.0,
    tensionRise: 1.25,
    tensionFall: 0.85,
    strikeWindowPad: 0.2,
    attractRadius: 9,
    fishPull: 0.55
  };

  G.Fishing = function (scene, world, player, opts) {
    opts = opts || {};

    this.scene = scene;
    this.world = world;
    this.player = player;

    this.state = STATE.IDLE;
    this.timer = 0;
    this.rodDef = opts.rod || G.Catalog.get(1);
    this.baitId = opts.baitId || -1;

    this.charge = 0;
    this.holding = false;
    this.reelHeld = false;
    this.reelSpeedMulti = 0;
    this.tension = 0;
    this.lineLength = 0;

    this.baitPos = new THREE.Vector3();
    this.baitVel = new THREE.Vector3();
    this.hookedFish = null;
    this.hookTimer = 0;
    this.stuckTimer = 0;
    this.candidate = null;
    this.biteWindow = 2.5;

    this.line = new THREE.Line(
      new THREE.BufferGeometry().setFromPoints([new THREE.Vector3(), new THREE.Vector3()]),
      new THREE.LineBasicMaterial({ color: 0xf2f6f7, transparent: true, opacity: 0.75 })
    );
    this.line.frustumCulled = false;
    this.line.visible = false;
    scene.add(this.line);

    this.bobber = new THREE.Group();
    var body = new THREE.Mesh(new THREE.SphereGeometry(0.13, 12, 10),
      new THREE.MeshStandardMaterial({ color: 0xe8453c, roughness: 0.45 }));
    var top = new THREE.Mesh(new THREE.SphereGeometry(0.1, 12, 10),
      new THREE.MeshStandardMaterial({ color: 0xf5f5f5, roughness: 0.5 }));
    top.position.y = 0.06;
    this.bobber.add(body);
    this.bobber.add(top);
    this.bobber.visible = false;
    scene.add(this.bobber);

    this.rodGroup = player.rodGroup;
  };

  G.Fishing.STATE = STATE;
  G.Fishing.prototype.tune = TUNE;

  G.Fishing.prototype.rodTipWorld = function (out) {
    out = out || new THREE.Vector3();
    var local = this.player.rod.userData.tipLocal;
    out.copy(local);
    this.rodGroup.updateMatrixWorld(true);
    return this.rodGroup.localToWorld(out);
  };

  G.Fishing.prototype.canFish = function () {
    return !!this.rodDef && this.rodDef.cat === 'rod';
  };

  G.Fishing.prototype.baitDefinition = function () {
    return this.baitId > 0 ? G.Bait.get(this.baitId) : null;
  };

  G.Fishing.prototype.isBusy = function () {
    return this.state !== STATE.IDLE && this.state !== STATE.CAUGHT;
  };

  // --- input surface -------------------------------------------------------

  G.Fishing.prototype.press = function () {
    if (!this.canFish()) return;
    this.holding = true;

    if (this.state === STATE.IDLE || this.state === STATE.MISSED) {
      this.state = STATE.CHARGING;
      this.charge = 0;
      this.timer = 0;
    } else if (this.state === STATE.BITE) {
      this.strike();
    } else if (this.state === STATE.HOOKED || this.state === STATE.REELING) {
      this.reelHeld = true;
    }
  };

  G.Fishing.prototype.release = function () {
    this.holding = false;

    if (this.state === STATE.CHARGING) {
      this.throwBait();
      return;
    }

    this.reelHeld = false;
  };

  G.Fishing.prototype.use = function () {
    // kept for callers that only want a single-shot action
    this.press();
    this.release();
  };

  // --- the cast ------------------------------------------------------------

  G.Fishing.prototype.throwBait = function () {
    var power = Math.max(0.08, Math.min(1, this.charge));
    var range = TUNE.minLineLength + (TUNE.maxLineLength - TUNE.minLineLength) * power;

    var origin = this.rodTipWorld();
    var aim = this.player.aimVector();

    var dest = origin.clone().addScaledVector(aim, range);

    if (this.hitsGround(origin, aim, range)) {
      this.miss('the cast landed on land');
      return;
    }

    var water = this.world;

    var surface = water.seaLevel + G.Waves.height(dest.x, dest.z, 0);
    if (dest.y < surface) dest.y = surface;

    this.baitPos.copy(dest);
    this.baitVel.copy(aim).multiplyScalar(TUNE.lineThrowSpeed * power * 0.5);
    this.baitVel.y = 2.5 * power;

    this.lineLength = range;
    this.state = STATE.THROWING;
    this.timer = 0.32;
    this.tension = 0;
    this.stuckTimer = 0;
    this.candidate = null;

    this.bobber.visible = true;
    this.bobber.position.copy(this.baitPos);
    this.line.visible = true;
  };

  G.Fishing.prototype.hitsGround = function (origin, aim, range) {
    var step = 1.0;

    for (var d = step; d <= range; d += step) {
      var x = origin.x + aim.x * d;
      var y = origin.y + aim.y * d;
      var z = origin.z + aim.z * d;

      if (this.world.terrainHeight(x, z) > y) return true;
    }

    return false;
  };

  G.Fishing.prototype.miss = function (reason) {
    this.state = STATE.MISSED;
    this.timer = 1.2;
    this.tension = 0;
    this.stuckTimer = 0;
    this.candidate = null;
    this.hookedFish = null;

    if (reason) G.showToast(reason);
  };

  // --- the bite ------------------------------------------------------------

  G.Fishing.prototype.strike = function () {
    var bait = this.baitDefinition();
    var lost = Math.random() < G.Bait.lostOnBiteChance(this.baitId);

    if (lost) {
      this.miss('the bait was gone');
      return { state: this.state, caught: 0 };
    }

    if (!this.candidate) {
      this.miss('nothing was on the line');
      return { state: this.state, caught: 0 };
    }

    this.hookedFish = this.candidate;
    this.candidate = null;
    this.state = STATE.HOOKED;
    this.timer = 0;
    this.tension = 0.25;

    G.showToast('Hooked it');
    return { state: this.state, caught: 0 };
  };

  G.Fishing.prototype.biteFraction = function () {
    if (this.state === STATE.BITE) {
      return G.clamp01(this.timer / Math.max(0.01, this.biteWindow));
    }
    if (this.state === STATE.CHARGING) return this.charge;
    if (this.state === STATE.REELING) return G.clamp01(this.tension);
    return 0;
  };

  // --- per frame -----------------------------------------------------------

  G.Fishing.prototype.update = function (dt, t) {
    this.player.rodGroup.visible = this.canFish();

    if (this.state === STATE.CHARGING) {
      this.charge = Math.min(1, this.charge + dt * 0.9);
      this.timer += dt;
    }

    if (this.state === STATE.THROWING) {
      this.timer -= dt;
      this.integrateBait(dt, t);
      if (this.timer <= 0) {
        this.state = STATE.BAITING;
        this.timer = 0;
      }
    } else if (this.state === STATE.BAITING) {
      this.integrateBait(dt, t);
      this.updateAttraction(dt);
    } else if (this.state === STATE.BITE) {
      this.timer -= dt;
      if (this.timer <= 0) this.miss('it spat the hook');
    } else if (this.state === STATE.HOOKED || this.state === STATE.REELING) {
      this.updateFight(dt, t);
    } else if (this.state === STATE.MISSED || this.state === STATE.CAUGHT) {
      this.timer -= dt;
      if (this.timer <= 0) {
        this.state = STATE.IDLE;
        this.bobber.visible = false;
        this.line.visible = false;
      }
    }

    this.updateLine();
  };

  G.Fishing.prototype.integrateBait = function (dt, t) {
    var surface = this.world.seaLevel + G.Waves.height(this.baitPos.x, this.baitPos.z, t);

    this.baitVel.y -= 18 * dt;
    this.baitVel.multiplyScalar(Math.max(0, 1 - TUNE.baitDrag * dt));
    this.baitPos.addScaledVector(this.baitVel, dt);

    if (this.baitPos.y < surface) {
      this.baitPos.y = surface;
      this.baitVel.y = 0;
      this.baitVel.x *= 0.82;
      this.baitVel.z *= 0.82;
    }

    this.lineLength = Math.max(TUNE.minLineLength,
      this.rodTipWorld().distanceTo(this.baitPos));

    this.bobber.position.copy(this.baitPos);
  };

  G.Fishing.prototype.updateAttraction = function (dt) {
    var pool = this.player.pool;
    if (!pool) return;

    var best = null;
    var bestSqr = TUNE.attractRadius * TUNE.attractRadius;

    for (var i = 0; i < pool.creatures.length; i++) {
      var c = pool.creatures[i];
      if (!c.alive) continue;
      if (c.position.y > this.baitPos.y + 0.6) continue;

      var sqr = c.position.distanceToSquared(this.baitPos);
      if (sqr > bestSqr) continue;

      bestSqr = sqr;
      best = c;
    }

    this.candidate = best;

    if (!best) {
      this.stuckTimer = 0;
      return;
    }

    // holding the bait next to a fish hooks it, without waiting for a bite
    if (best.position.distanceTo(this.baitPos) < TUNE.baitStuckDist) {
      this.stuckTimer += dt;
      if (this.stuckTimer >= TUNE.baitStuckTime) {
        this.state = STATE.BITE;
        var window = G.Bait.catchWindow(this.baitId);
        this.biteWindow = G.rand(window.min, window.max) + TUNE.strikeWindowPad;
        this.timer = this.biteWindow;
        G.showToast('Something is taking it');
        return;
      }
    } else {
      this.stuckTimer = 0;
    }
  };

  G.Fishing.prototype.updateFight = function (dt, t) {
    var fish = this.hookedFish;

    if (!fish || !fish.alive) {
      this.hookedFish = null;
      this.miss('it got away');
      return;
    }

    if (this.reelHeld) {
      this.reelSpeedMulti = Math.min(1, this.reelSpeedMulti + dt * TUNE.reelRampUp);
    } else {
      this.reelSpeedMulti = Math.max(0, this.reelSpeedMulti - dt * TUNE.reelRampDown);
    }

    if (this.reelSpeedMulti > 0) {
      this.lineLength -= TUNE.lineReelStepLength * this.reelSpeedMulti * TUNE.holdReelSpeed * dt;
      this.lineLength = Math.max(0, this.lineLength);
    }

    var dest = this.rodTipWorld();
    var toRod = dest.clone().sub(fish.position);
    var dist = toRod.length();

    if (dist > 0.001) {
      toRod.normalize();
      var allowed = Math.min(dist, this.lineLength);
      var want = dest.clone().addScaledVector(toRod, -allowed);
      fish.position.lerp(want, Math.min(1, dt * 6));
    }

    var pull = 0.35 + (1 - G.clamp01(dist / 12)) * 0.65;
    this.tension = G.clamp01(this.tension + (pull * this.reelSpeedMulti - 0.5) * dt * 2.2);

    if (this.tension > TUNE.maxTension) {
      this.hookedFish = null;
      this.miss('the line snapped');
      return;
    }

    if (this.lineLength <= TUNE.minLineLength + 0.4 || dist < 1.8) {
      this.catchFish(fish);
    }
  };

  G.Fishing.prototype.catchFish = function (fish) {
    var id = fish.itemId;
    var player = this.player;

    if (player.inventory) {
      player.inventory.add(id, 1);
    }

    if (player.onCatch) player.onCatch(id, fish);

    this.hookedFish = null;
    this.state = STATE.CAUGHT;
    this.timer = 1.1;
    this.tension = 0;

    G.showToast('Landed a ' + G.Catalog.name(id));
  };

  G.Fishing.prototype.updateLine = function () {
    var busy = this.state === STATE.THROWING || this.state === STATE.BAITING
      || this.state === STATE.BITE || this.state === STATE.HOOKED
      || this.state === STATE.REELING || this.state === STATE.CHARGING;

    if (!busy) {
      this.line.visible = false;
      this.bobber.visible = false;
      return;
    }

    var tip = this.rodTipWorld();
    var end = this.bobber.visible ? this.bobber.position : this.baitPos;

    this.line.visible = true;
    this.bobber.position.copy(end);

    var pts = this.line.geometry.attributes.position;
    var sag = 0.3 + tip.distanceTo(end) * 0.03;
    for (var i = 0; i < pts.count; i++) {
      var f = i / (pts.count - 1);
      pts.setXYZ(i,
        G.lerp(tip.x, end.x, f),
        G.lerp(tip.y, end.y, f) - Math.sin(f * Math.PI) * sag,
        G.lerp(tip.z, end.z, f));
    }
    pts.needsUpdate = true;
    this.line.geometry.computeBoundingSphere();
  };

})(window);
