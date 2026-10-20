(function (global) {
  'use strict';

  var G = global.G;
  var THREE = global.THREE;

  G.Creature = function (scene, world, itemId) {
    this.world = world;
    this.itemId = itemId || G.Catalog.rollCatch(0.4);
    this.hp = 30 + Math.random() * 40;
    this.alive = true;
    this.fearTimer = 0;
    this.wanderTimer = 0;
    this.heading = Math.random() * G.TAU;
    this.speed = 1.1 + Math.random() * 1.1;

    this.position = new THREE.Vector3();
    this.velocity = new THREE.Vector3();
    this.fleeFrom = null;

    this.mesh = buildFish();
    scene.add(this.mesh);
    this.scene = scene;
  };

  function buildFish() {
    var group = new THREE.Group();

    var body = new THREE.Mesh(
      new THREE.SphereGeometry(0.42, 12, 10),
      new THREE.MeshStandardMaterial({ color: 0x5d8fb3, roughness: 0.45, metalness: 0.15 })
    );
    body.scale.set(0.55, 0.75, 1.5);
    group.add(body);

    var tail = new THREE.Mesh(
      new THREE.ConeGeometry(0.28, 0.6, 4),
      new THREE.MeshStandardMaterial({ color: 0x3f6d8c, roughness: 0.5 })
    );
    tail.rotation.x = -Math.PI / 2;
    tail.position.z = -0.78;
    group.add(tail);

    var fin = new THREE.Mesh(
      new THREE.BoxGeometry(0.05, 0.28, 0.4),
      new THREE.MeshStandardMaterial({ color: 0x3f6d8c, roughness: 0.5 })
    );
    fin.position.y = 0.34;
    group.add(fin);

    group.userData.tail = tail;
    return group;
  }

  G.Creature.prototype.placeNear = function (island) {
    var angle = Math.random() * G.TAU;
    var dist = island.radius + 6 + Math.random() * 22;

    this.position.set(
      island.x + Math.cos(angle) * dist,
      0,
      island.z + Math.sin(angle) * dist
    );

    this.position.y = this.world.seaLevel - 0.8 - Math.random() * 2.2;
    this.mesh.position.copy(this.position);
    return this;
  };

  G.Creature.prototype.damage = function (amount) {
    this.hp -= amount;
    if (this.hp <= 0) this.alive = false;
    return this.alive;
  };

  G.Creature.prototype.flee = function (from) {
    this.fleeFrom = from;
    this.fearTimer = 4;
  };

  G.Creature.prototype.update = function (dt, t) {
    if (!this.alive) return;

    var surface = this.world.seaLevel + G.Waves.height(this.position.x, this.position.z, t);
    var ground = this.world.terrainHeight(this.position.x, this.position.z);
    var waterDepth = surface - ground;

    if (waterDepth < 0.9) {
      this.steerAway(ground, 1);
    }

    if (this.fearTimer > 0) {
      this.fearTimer -= dt;
    } else {
      this.fleeFrom = null;
      this.wanderTimer -= dt;
      if (this.wanderTimer <= 0) {
        this.wanderTimer = 2 + Math.random() * 4;
        this.heading += (Math.random() - 0.5) * 2.4;
      }
    }

    var speed = this.fleeFrom ? this.speed * 3.4 : this.speed;

    if (this.fleeFrom) {
      var away = new THREE.Vector3()
        .subVectors(this.position, this.fleeFrom)
        .setY(0)
        .normalize();
      this.velocity.x = G.damp(this.velocity.x, away.x * speed, 2.5, dt);
      this.velocity.z = G.damp(this.velocity.z, away.z * speed, 2.5, dt);
    } else {
      this.velocity.x = G.damp(this.velocity.x, Math.sin(this.heading) * speed, 1.6, dt);
      this.velocity.z = G.damp(this.velocity.z, Math.cos(this.heading) * speed, 1.6, dt);
    }

    this.position.x += this.velocity.x * dt;
    this.position.z += this.velocity.z * dt;

    var targetY = G.clamp(surface - 0.7, ground + 0.4, surface - 0.15);
    this.position.y = G.damp(this.position.y, targetY, 3, dt);

    var center = new THREE.Vector3(this.position.x, this.position.y, this.position.z);
    var want = Math.atan2(this.velocity.x, this.velocity.z);

    this.mesh.position.copy(center);
    this.mesh.rotation.y = G.damp(
      this.mesh.rotation.y === 0 ? want : shortestAngle(this.mesh.rotation.y, want),
      want, 4, dt
    );

    var tailMesh = this.mesh.userData.tail;
    if (tailMesh) tailMesh.rotation.y = Math.sin(t * 9) * 0.5;
  };

  G.Creature.prototype.steerAway = function (ground, amount) {
    var dx = this.position.x - this.lastGroundX;
    var dz = this.position.z - this.lastGroundZ;
    if (!isFinite(dx) || !isFinite(dz)) {
      dx = 1;
      dz = 0;
    }
    this.lastGroundX = this.position.x;
    this.lastGroundZ = this.position.z;
    this.heading = Math.atan2(-dx, -dz) + (Math.random() - 0.5) * 0.6;
    void ground;
    void amount;
  };

  function shortestAngle(from, to) {
    var delta = (to - from + Math.PI) % G.TAU;
    if (delta < 0) delta += G.TAU;
    return from + delta - Math.PI;
  }

  G.Floater = function (scene, world, itemId, position) {
    this.itemId = itemId;
    this.world = world;
    this.position = position.clone();
    this.life = 90;
    this.taken = false;

    this.mesh = new THREE.Mesh(
      new THREE.BoxGeometry(0.32, 0.32, 0.32),
      new THREE.MeshStandardMaterial({
        color: 0xf0d68a,
        emissive: 0x6b5a20,
        emissiveIntensity: 0.5,
        roughness: 0.5
      })
    );
    this.mesh.position.copy(this.position);
    scene.add(this.mesh);
    this.scene = scene;
  };

  G.Floater.prototype.update = function (dt, t, player, inventory) {
    if (this.taken) return;

    this.life -= dt;
    if (this.life <= 0) {
      this.destroy();
      return;
    }

    var ground = this.world.terrainHeight(this.position.x, this.position.z);
    var surface = this.world.seaLevel + G.Waves.height(this.position.x, this.position.z, t);
    var rest = Math.max(ground, surface);

    this.position.y = G.damp(this.position.y, rest + 0.25, 4, dt);
    this.mesh.position.copy(this.position);
    this.mesh.rotation.y += dt * 1.4;
    this.mesh.rotation.x += dt * 0.9;

    var dx = player.position.x - this.position.x;
    var dz = player.position.z - this.position.z;
    var dy = player.position.y - this.position.y;

    if (dx * dx + dz * dz < 2.6 && Math.abs(dy) < 2.2) {
      if (inventory.add(this.itemId, 1) > 0) {
        this.taken = true;
        G.showToast('Picked up ' + G.Catalog.name(this.itemId));
        this.destroy();
      } else {
        this.taken = true;
        G.showToast('Your hold is full');
        this.destroy();
      }
    }
  };

  G.Floater.prototype.destroy = function () {
    if (this.mesh && this.mesh.parent) this.scene.remove(this.mesh);
    this.mesh = null;
  };

  G.CreaturePool = function (scene, world, count) {
    this.scene = scene;
    this.world = world;
    this.creatures = [];
    this.floaters = [];

    for (var i = 0; i < count; i++) this.creatures.push(this.makeCreature());
  };

  G.CreaturePool.prototype.makeCreature = function () {
    var islands = this.world.islandList();
    var island = islands.length ? G.pick(islands) : { x: 0, z: 0, radius: 20 };
    return new G.Creature(this.scene, this.world).placeNear(island);
  };

  G.CreaturePool.prototype.creaturesInRadius = function (center, radius) {
    var out = [];
    var r2 = radius * radius;

    for (var i = 0; i < this.creatures.length; i++) {
      var c = this.creatures[i];
      if (!c.alive) continue;
      if (G.dist2(c.position.x, c.position.z, center.x, center.z) <= r2) out.push(c);
    }

    return out;
  };

  G.CreaturePool.prototype.spawnFloater = function (position, itemId) {
    this.floaters.push(new G.Floater(this.scene, this.world, itemId, position));
  };

  G.CreaturePool.prototype.update = function (dt, t, player, inventory) {
    for (var i = 0; i < this.creatures.length; i++) {
      var c = this.creatures[i];

      if (!c.alive) {
        if (c.mesh.parent) this.scene.remove(c.mesh);
        this.spawnFloater(c.position, c.itemId);
        G.showToast('You gutted a ' + G.Catalog.name(c.itemId));
        this.creatures[i] = this.makeCreature();
        continue;
      }

      c.update(dt, t);
    }

    for (var j = this.floaters.length - 1; j >= 0; j--) {
      this.floaters[j].update(dt, t, player, inventory);
      if (!this.floaters[j].mesh) this.floaters.splice(j, 1);
    }
  };

})(window);
