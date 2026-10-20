(function (global) {
  'use strict';

  var G = global.G;
  var THREE = global.THREE;

  var canvas = document.getElementById('view');
  var menu = document.getElementById('menu');
  var playBtn = document.getElementById('playBtn');
  var crash = document.getElementById('crash');

  var bootErrors = [];

  function reportFault(label, detail) {
    var text = label + ': ' + detail;
    bootErrors.push(text);
    if (!crash) return;
    crash.textContent = 'Tidebreak stopped\n\n' + bootErrors.join('\n\n');
    crash.classList.remove('hidden');
  }

  global.addEventListener('error', function (e) {
    reportFault('Runtime error', (e.message || 'unknown') +
      (e.filename ? '\n  at ' + e.filename + ':' + e.lineno : ''));
  });

  global.addEventListener('unhandledrejection', function (e) {
    var reason = e && e.reason ? (e.reason.message || e.reason) : 'unknown';
    reportFault('Promise rejection', reason);
  });

  var renderer = null;
  try {
    renderer = new THREE.WebGLRenderer({ canvas: canvas, antialias: true, powerPreference: 'high-performance' });
    renderer.setPixelRatio(Math.min(global.devicePixelRatio || 1, 2));
    renderer.setSize(global.innerWidth, global.innerHeight, false);
    renderer.outputEncoding = THREE.sRGBEncoding;
    if (THREE.ACESFilmicToneMapping !== undefined) {
      renderer.toneMapping = THREE.ACESFilmicToneMapping;
      renderer.toneMappingExposure = 1.12;
    }
  } catch (e) {
    reportFault('WebGL', 'this browser could not create a 3D context (' +
      (e && e.message ? e.message : 'unknown') + '). Try a desktop browser with hardware acceleration on.');
  }

  var scene = new THREE.Scene();
  var camera = new THREE.PerspectiveCamera(62, global.innerWidth / global.innerHeight, 0.1, 900);

  var world = G.World.create(scene, renderer);

  if (!world.water || !world.mesh) {
    reportFault('Boot', 'the scene did not build: ocean or terrain is missing');
  }

  var vitals = new G.Vitals({
    onDeath: function () {
      G.showToast('You blacked out — hauling yourself back to the beach', 3);
    },
    onRespawn: function () { }
  });

  var inventory = new G.Inventory();

  var player = new G.Player(scene, world, { vitals: vitals, inventory: inventory });
  player.spawn(2.5, 24);

  inventory.grantStarter();

  var boat = new G.Boat(scene, world, { x: 7, z: 46, yaw: 0.1 });
  var fish = new G.CreaturePool(scene, world, 34);
  player.pool = fish;

  player.onCatch = function (id) {
    recordCatch(id, 'landed');
  };

  var fishing = new G.Fishing(scene, world, player, { rod: G.Catalog.get(1) });
  var hud = new G.Hud();
  var aiChat = new G.AiChat();
  var journal = new G.Journal({ pageSize: 8 });

  var closeBtn = document.getElementById('aiChatClose');
  if (closeBtn) {
    closeBtn.addEventListener('click', function () { aiChat.close(); });
  }

  var journalPanel = document.getElementById('journal');
  var journalSearch = document.getElementById('journalSearch');
  var journalClose = document.getElementById('journalClose');
  var journalPrev = document.getElementById('journalPrev');
  var journalNext = document.getElementById('journalNext');

  if (journalClose) {
    journalClose.addEventListener('click', function () { toggleJournal(false); });
  }

  if (journalSearch) {
    journalSearch.addEventListener('input', function () {
      journal.setQuery(journalSearch.value);
    });
  }

  if (journalPrev) {
    journalPrev.addEventListener('click', function () { journal.scrollPage(-1); });
  }

  if (journalNext) {
    journalNext.addEventListener('click', function () { journal.scrollPage(1); });
  }

  function toggleJournal(force) {
    if (!journalPanel) return;

    var open = force === undefined ? journalPanel.classList.contains('hidden') : force;
    journalPanel.classList.toggle('hidden', !open);

    if (open && journalSearch) {
      journalSearch.value = journal.query;
      journalSearch.focus();
    }
  }

  var dockhand = G.World.makeDockhand ? G.World.makeDockhand(scene) : null;
  var dockhandPos = new THREE.Vector3(3.6, 0, 29);

  if (dockhand) {
    dockhandPos.y = world.terrainHeight(dockhandPos.x, dockhandPos.z);
    dockhand.group.position.copy(dockhandPos);
    scene.add(dockhand.group);
  }

  var camPos = new THREE.Vector3();
  var camLook = new THREE.Vector3();
  var shake = 0;
  var camReady = false;
  var fovBase = 62;
  var fovNow = 62;
  var overlay = document.getElementById('overlay');
  var wasUnderwater = false;

  var playerShadow = G.World.makeContactShadow(0.55, 0.34);
  scene.add(playerShadow);

  var boatShadow = G.World.makeContactShadow(2.6, 0.26);
  scene.add(boatShadow);

  function updateShadows(t) {
    var ground = world.terrainHeight(player.position.x, player.position.z);
    playerShadow.visible = !player.boat && !player.swimming;
    if (playerShadow.visible) {
      playerShadow.position.set(player.position.x, ground + 0.03, player.position.z);
      var lift = G.clamp01((player.position.y - ground) / 1.6);
      playerShadow.material.opacity = 0.34 * (1 - lift * 0.65);
      var grow = 1 + lift * 0.5;
      playerShadow.scale.set(grow, 1, grow);
    }

    boatShadow.visible = !!player.boat;
    if (boatShadow.visible) {
      boatShadow.position.set(boat.position.x, world.seaLevel + G.Waves.height(boat.position.x, boat.position.z, t) + 0.04, boat.position.z);
      boatShadow.rotation.y = boat.yaw;
    }
  }

  function updateUnderwater(t) {
    var camSurface = world.seaLevel + G.Waves.height(camera.position.x, camera.position.z, t);
    var under = camera.position.y < camSurface - 0.15;

    if (under === wasUnderwater) return;
    wasUnderwater = under;

    if (overlay) overlay.classList.toggle('underwater', under);
    if (under) scene.fog.color.setHex(0x1d5a6b);
    else scene.fog.color.setHex(0x9fc4d4);
  }

  var pendingTap = false;
  var pressing = false;
  var pressTravel = 0;
  var lastX = 0;
  var lastY = 0;

  vitals.onRespawn = function () {
    player.respawn();
  };

  function updateCamera(dt) {
    if (player.boat) {
      camLook.copy(boat.position);
      camLook.y += 1.6;
      var back = new THREE.Vector3(Math.sin(boat.yaw), 0, Math.cos(boat.yaw)).multiplyScalar(-7.5);
      camPos.copy(camLook).add(back);
      camPos.y += 3.1;
    } else {
      var pivot = player.position.clone();
      pivot.y += 1.55;

      var dir = player.aimVector();
      var desired = pivot.clone().addScaledVector(dir, 4.6);

      var ground = world.terrainHeight(desired.x, desired.z);
      desired.y = Math.max(desired.y, ground + 0.6);
      desired.y = Math.max(desired.y, world.seaLevel + 0.8);

      camPos.copy(desired);
      camLook.copy(pivot).addScaledVector(dir, 1.5);
    }

    if (shake > 0) {
      shake = Math.max(0, shake - dt * 2.4);
      camPos.x += (Math.random() - 0.5) * shake * 0.6;
      camPos.y += (Math.random() - 0.5) * shake * 0.6;
      camPos.z += (Math.random() - 0.5) * shake * 0.6;
    }

    camera.position.lerp(camPos, 1 - Math.exp(-16 * dt));
    camera.lookAt(camLook);

    var speedKick = player.boat ? G.clamp01(boat.speedKmh() / 55) : 0;
    fovNow = G.damp(fovNow, fovBase + speedKick * 13, 3.2, dt);
    if (Math.abs(camera.fov - fovNow) > 0.01) {
      camera.fov = fovNow;
      camera.updateProjectionMatrix();
    }

    if (!camReady) {
      camera.position.copy(camPos);
      camera.lookAt(camLook);
      camReady = true;
    }
  }

  function nearDockhand() {
    if (!dockhand) return false;
    var dx = player.position.x - dockhandPos.x;
    var dz = player.position.z - dockhandPos.z;
    return Math.sqrt(dx * dx + dz * dz) < 6.5;
  }

  function animateDockhand(t) {
    if (!dockhand) return;

    if (nearDockhand()) {
      var ang = Math.atan2(player.position.x - dockhandPos.x, player.position.z - dockhandPos.z);
      dockhand.group.rotation.y = ang;
      dockhand.arm.rotation.x = -0.9 + Math.sin(t * 3.2) * 0.25;
      dockhand.head.rotation.y = Math.sin(t * 0.9) * 0.12;
    } else {
      dockhand.group.rotation.y = Math.PI;
      dockhand.arm.rotation.x = Math.sin(t * 1.1) * 0.14;
      dockhand.head.rotation.y = 0;
    }
  }

  function handleActions() {
    var input = G.Input;

    if (input.hit('KeyF')) {
      if (player.boat) {
        player.leaveBoat();
      } else if (boat.canBoard(player)) {
        player.enterBoat(boat);
      } else {
        G.showToast('Move closer to the boat');
      }
    }

    if (pendingTap) {
      pendingTap = false;
      var held = inventory.held();

      if (player.boat) {
        return;
      }

      if (held && G.Catalog.isRod(held.id) && fishing.canFish()) {
        return;
      }

      if (held && G.Catalog.isMelee(held.id)) {
        swing();
        return;
      }

      if (held && G.Catalog.isEdible(held.id)) {
        eat(held);
      }
    }

    if (input.hit('KeyR')) eat(inventory.held());

    if (input.hit('KeyG')) drop();

    if (input.hit('KeyB')) {
      inventory.setBaitFrom(inventory.selected);
      var baitName = inventory.baitId > 0 ? G.Catalog.name(inventory.baitId) : 'no bait';
      G.showToast('Bait set: ' + baitName);
    }

    if (input.hit('KeyM')) {
      world.resample(Math.floor(Math.random() * 100000), 7);
      G.showToast('New island chain charted');
    }

    if (input.hit('KeyT')) {
      if (aiChat.open) {
        aiChat.close();
      } else if (nearDockhand()) {
        aiChat.show();
      } else {
        G.showToast('The dockhand is down by the pier');
      }
    }

    if (input.hit('KeyJ')) {
      toggleJournal();
    }

    for (var i = 1; i <= 9; i++) {
      if (input.hit('Digit' + i)) inventory.select(i - 1);
    }

    if (input.mouse.wheel !== 0) {
      inventory.cycle(input.mouse.wheel > 0 ? -1 : 1);
    }
  }

  function recordCatch(id, note) {
    var def = G.Catalog.get(id);
    if (!def) return;

    journal.record(id, def.name, {
      count: 1,
      worth: G.Catalog.worth(id),
      defeated: true,
      note: note || 'caught'
    });
  }

  function recordCreature(creature, note) {
    if (!creature) return;

    recordCatch(creature.itemId, note || 'hunted');
  }

  function swing() {
    var held = inventory.held();
    if (!held) return;

    var def = G.Catalog.get(held.id);
    var reach = (def.reach || 2.2) + 0.6;
    player.startSwing();

    var origin = player.position.clone();
    origin.y += 1.0;
    var aim = player.forwardVector();
    var victims = fish.creaturesInRadius(origin, reach);

    var landed = 0;
    for (var i = 0; i < victims.length; i++) {
      var c = victims[i];
      var to = c.position.clone().sub(origin);
      if (to.length() > reach) continue;
      if (to.normalize().dot(aim) < 0.1) continue;

      c.damage(def.damage || 10);
      c.flee(player.position);
      if (!c.alive) recordCreature(c, 'hunted');
      landed++;
    }

    if (landed > 0) shake = 0.25;
  }

  function eat(held) {
    if (!held) return;

    var def = inventory.consume(inventory.selected);
    if (!def) {
      G.showToast('Nothing edible in hand');
      return;
    }

    if (def.food) vitals.feed(def.food);
    if (def.heal) vitals.heal(def.heal);
    if (def.cure) {
      vitals.cure(def.cure, def.cure * 0.6);
      G.showToast('That tasted like charcoal. Better.');
    }

    G.showToast('Ate ' + def.name);
  }

  function drop() {
    var held = inventory.held();
    if (!held) return;

    inventory.removeFrom(inventory.selected, 1);
    fish.spawnFloater(player.position, held.id);
    G.showToast('Dropped ' + G.Catalog.name(held.id));
  }

  function step(dt, t) {
    G.tickToast(dt);
    G.World.update(t);

    if (world.sky) world.sky.position.copy(camera.position);

    if (G.Input.locked) {
      player.look(G.Input.mouse.dx, G.Input.mouse.dy, 0.0022);
    }

    handleActions();
    player.update(dt, t);
    boat.update(dt, t);
    fish.update(dt, t, player, inventory);

    var heldNow = inventory.held();
    if (heldNow && G.Catalog.isRod(heldNow.id)) {
      fishing.rodDef = G.Catalog.get(heldNow.id);
    }
    fishing.baitId = inventory.baitId > 0 ? inventory.baitId : -1;

    fishing.update(dt, t);
    updateCamera(dt);

    hud.refreshHotbar(inventory);
    hud.refreshVitals(vitals);
    hud.refreshWeight(inventory);
    hud.refreshRod(fishing, inventory);
    hud.refreshJournal(journal);

    var baitLabel = inventory.baitId > 0 ? G.Catalog.name(inventory.baitId) : 'none';
    var depth = world.waterDepth(player.position.x, player.position.z, t);

    hud.refreshStatus({
      mode: player.boat ? 'sailing' : (player.swimming ? 'swimming' : 'on foot'),
      speed: player.boat ? boat.speedKmh() : null,
      draft: player.boat ? boat.draftMeters() : null,
      pos: Math.round(player.position.x) + ' / ' + Math.round(player.position.z),
      bait: baitLabel,
      depth: depth,
      state: player.boat ? boat.driveMode : (player.swimming ? 'swimming' : player.grounded ? 'grounded' : 'airborne'),
      fps: G.Loop.fps,
      errors: bootErrors.length
    });

    if (player.boat) {
      hud.setPrompt('F  step ashore');
    } else if (nearDockhand()) {
      hud.setPrompt('T  talk to the dockhand');
    } else if (boat.canBoard(player)) {
      hud.setPrompt('F  take the helm');
    } else {
      hud.setPrompt('');
    }

    if (vitals.dead) {
      vitals.resurrect();
    }

    updateShadows(t);
    updateUnderwater(t);
    animateDockhand(t);

    renderer.render(scene, camera);
  }

  G.onLockChange = function (locked) {
    if (locked) {
      menu.classList.add('hidden');
      hud.setHint('WASD move · F boat · LMB use · 1-9 slots');
    } else {
      menu.classList.remove('hidden');
      hud.setHint('Click to look around');
    }
  };

  G.Input.init(canvas);

  playBtn.addEventListener('click', function () {
    enterPlay();
  });

  canvas.addEventListener('click', function () {
    if (!menu.classList.contains('hidden')) {
      enterPlay();
      return;
    }
    if (!G.Input.locked) enterPlay();
  });

  canvas.addEventListener('mousedown', function (e) {
    pressing = true;
    pressTravel = 0;
    lastX = e.clientX;
    lastY = e.clientY;

    var held = inventory.held();
    if (held && G.Catalog.isRod(held.id) && fishing.canFish() && !player.boat) {
      fishing.press();
    }
  });

  global.addEventListener('mouseup', function () {
    if (pressing && pressTravel < 8) pendingTap = true;
    pressing = false;

    if (fishing.holding || fishing.reelHeld) {
      fishing.release();
    }
  });

  global.addEventListener('mousemove', function (e) {
    var dx = e.clientX - lastX;
    var dy = e.clientY - lastY;
    if (pressing) pressTravel += Math.abs(dx) + Math.abs(dy);
    if (G.Input.locked || pressing) {
      G.Input.mouse.dx += dx;
      G.Input.mouse.dy += dy;
    }
    lastX = e.clientX;
    lastY = e.clientY;
  });

  function enterPlay() {
    menu.classList.add('hidden');
    hud.setHint('WASD move · F boat · tap to use · 1-9 slots · drag to look');

    if (G.Input.locked) return;

    try {
      G.Input.lock(canvas);
    } catch (e) {
      reportFault('Pointer lock', 'unavailable — drag to look instead');
    }
  }

  global.addEventListener('resize', function () {
    camera.aspect = global.innerWidth / global.innerHeight;
    camera.updateProjectionMatrix();
    renderer.setSize(global.innerWidth, global.innerHeight, false);
  });

  G.Loop.start([step]);

  global.Tidebreak = {
    scene: scene,
    world: world,
    player: player,
    boat: boat,
    fishing: fishing,
    inventory: inventory,
    vitals: vitals,
    renderer: renderer,
    camera: camera,
    step: step,
    errors: function () { return bootErrors.slice(); }
  };

})(window);
