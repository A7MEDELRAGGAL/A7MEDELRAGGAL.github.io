(function (global) {
  'use strict';

  var G = global.G;
  var THREE = global.THREE;

  var WAVE = {
    a1: 0.62, k1: 0.085, w1: 0.85, dx1: 1.00, dz1: 0.35,
    a2: 0.34, k2: 0.170, w2: 1.35, dx2: -0.40, dz2: 1.00,
    a3: 0.15, k3: 0.360, w3: 2.10, dx3: 0.70, dz3: -0.70
  };

  var WAVE_GLSL = [
    'const float A1 = ' + WAVE.a1.toFixed(4) + ';',
    'const float A2 = ' + WAVE.a2.toFixed(4) + ';',
    'const float A3 = ' + WAVE.a3.toFixed(4) + ';',
    'const float K1 = ' + WAVE.k1.toFixed(4) + ';',
    'const float K2 = ' + WAVE.k2.toFixed(4) + ';',
    'const float K3 = ' + WAVE.k3.toFixed(4) + ';',
    'const float W1 = ' + WAVE.w1.toFixed(4) + ';',
    'const float W2 = ' + WAVE.w2.toFixed(4) + ';',
    'const float W3 = ' + WAVE.w3.toFixed(4) + ';',
    'float waveHeight(vec2 p, float t) {',
    '  float h = 0.0;',
    '  h += A1 * sin((p.x * 1.00 + p.y * 0.35) * K1 + t * W1);',
    '  h += A2 * sin((p.x * -0.40 + p.y * 1.00) * K2 - t * W2);',
    '  h += A3 * sin((p.x * 0.70 + p.y * -0.70) * K3 + t * W3);',
    '  return h;',
    '}',
    'vec3 waveNormal(vec2 p, float t) {',
    '  float e = 0.35;',
    '  float h = waveHeight(p, t);',
    '  float hx = waveHeight(p + vec2(e, 0.0), t);',
    '  float hz = waveHeight(p + vec2(0.0, e), t);',
    '  return normalize(vec3(-(hx - h) / e, 1.0, -(hz - h) / e));',
    '}'
  ].join('\n');

  G.Waves = {
    height: function (x, z, t) {
      var h = 0;
      h += WAVE.a1 * Math.sin((x * WAVE.dx1 + z * WAVE.dz1) * WAVE.k1 + t * WAVE.w1);
      h += WAVE.a2 * Math.sin((x * WAVE.dx2 + z * WAVE.dz2) * WAVE.k2 - t * WAVE.w2);
      h += WAVE.a3 * Math.sin((x * WAVE.dx3 + z * WAVE.dz3) * WAVE.k3 + t * WAVE.w3);
      return h;
    },

    normal: function (x, z, t, out) {
      var e = 0.35;
      var h = this.height(x, z, t);
      var hx = this.height(x + e, z, t);
      var hz = this.height(x, z + e, t);
      var nx = -(hx - h) / e;
      var nz = -(hz - h) / e;
      var len = Math.sqrt(nx * nx + 1 + nz * nz);
      out = out || new THREE.Vector3();
      out.set(nx / len, 1 / len, nz / len);
      return out;
    }
  };

  var ISLANDS = [];

  function islandHeight(x, z) {
    var best = -Infinity;

    for (var i = 0; i < ISLANDS.length; i++) {
      var isl = ISLANDS[i];
      var d = Math.sqrt(G.dist2(x, z, isl.x, isl.z));
      if (d > isl.radius) continue;

      var f = 1 - d / isl.radius;
      var dome = Math.pow(f, 1.65);
      var bumps = (G.Noise.fbm(x * 0.035 + isl.seed, z * 0.035 - isl.seed, 4) - 0.5) * isl.rough;
      var ridge = Math.pow(Math.max(0, 1 - Math.abs(d - isl.radius * 0.62) / (isl.radius * 0.3)), 2) * isl.peak * 0.35;
      var h = isl.height * dome + bumps * dome + ridge;
      if (h > best) best = h;
    }

    return best === -Infinity ? -12 : best;
  }

  function generateIslands(seed, count) {
    ISLANDS.length = 0;
    var rngSeed = seed;

    ISLANDS.push({
      x: 0, z: 0, radius: 34, height: 12.5, rough: 3.4, peak: 9, seed: rngSeed
    });

    for (var i = 1; i < count; i++) {
      var angle = (i / count) * G.TAU + (rngSeed % 7) * 0.31;
      var dist = 74 + (rngSeed % 11) * 5.5 + i * 7.5;
      ISLANDS.push({
        x: Math.cos(angle) * dist,
        z: Math.sin(angle) * dist,
        radius: 17 + ((rngSeed + i * 37) % 13),
        height: 6.5 + ((rngSeed + i * 17) % 8),
        rough: 2.6,
        peak: 5,
        seed: rngSeed + i * 91
      });
    }
  }

  G.World = {
    seaLevel: 0,
    islands: ISLANDS,
    terrainHeight: islandHeight,
    islandList: function () { return ISLANDS; },

    findDock: function () {
      var dock = this.dock;
      if (!dock) return null;

      for (var i = 0; i < dock.children.length; i++) {
        var child = dock.children[i];
        if (child.geometry && child.geometry.parameters && child.geometry.parameters.width === 5.5) {
          return child;
        }
      }

      return dock.children.length ? dock.children[0] : null;
    },

    seabedAt: function (x, z) { return islandHeight(x, z); },

    isLand: function (x, z) { return islandHeight(x, z) > 0.05; },

    waterDepth: function (x, z, t) {
      return Math.max(0, this.seaLevel + G.Waves.height(x, z, t || 0) - islandHeight(x, z));
    },

    resample: function (seed, count) {      generateIslands(seed, count || 7);
      if (this.mesh) this.rebuildTerrain();
    },

    create: function (scene, renderer) {
      var self = this;
      generateIslands(20261017, 7);

      scene.fog = new THREE.FogExp2(0x9fc4d4, 0.0042);
      scene.background = new THREE.Color(0x8fc0d8);

      var sun = new THREE.DirectionalLight(0xfff0d8, 1.15);
      sun.position.set(70, 120, 40);
      scene.add(sun);

      var hemi = new THREE.HemisphereLight(0xbfe0ee, 0x2b4a52, 0.55);
      scene.add(hemi);

      var fill = new THREE.DirectionalLight(0x9fd4ff, 0.25);
      fill.position.set(-60, 40, -70);
      scene.add(fill);

      self.sun = sun;

      var sky = makeSkyDome();
      scene.add(sky);
      self.sky = sky;

      var sunDisc = new THREE.Mesh(
        new THREE.SphereGeometry(16, 12, 10),
        new THREE.MeshBasicMaterial({ color: 0xfff6dd, fog: false })
      );
      sunDisc.position.copy(sun.position).normalize().multiplyScalar(420);
      sky.add(sunDisc);
      self.sunDisc = sunDisc;

      var clouds = makeClouds();
      sky.add(clouds);
      self.clouds = clouds;

      var terrain = new THREE.Group();
      scene.add(terrain);
      self.terrain = terrain;
      self.rebuildTerrain();

      var water = makeWater();
      scene.add(water);
      self.water = water;
      self.refreshWaterShore();

      var dock = makeDock();
      terrain.add(dock);
      self.dock = dock;

      var props = makeProps();
      terrain.add(props);
      self.props = props;

      return self;
    },

    rebuildTerrain: function () {
      var size = 300;
      var seg = 150;
      var geo = new THREE.PlaneGeometry(size, size, seg, seg);
      geo.rotateX(-Math.PI / 2);

      var pos = geo.attributes.position;
      var colors = new Float32Array(pos.count * 3);

      var sand = new THREE.Color(0xc9b58a);
      var grass = new THREE.Color(0x5f7a44);
      var dark = new THREE.Color(0x38502f);
      var rock = new THREE.Color(0x6d6a63);
      var snow = new THREE.Color(0xe8eef2);
      var tmp = new THREE.Color();

      for (var i = 0; i < pos.count; i++) {
        var x = pos.getX(i);
        var z = pos.getZ(i);
        var h = islandHeight(x, z);
        pos.setY(i, h);

        if (h < -1.5) tmp.copy(sand).multiplyScalar(0.42);
        else if (h < 1.1) tmp.copy(sand);
        else if (h < 6) tmp.copy(sand).lerp(grass, (h - 1.1) / 4.9);
        else if (h < 13) tmp.copy(grass).lerp(dark, (h - 6) / 7);
        else tmp.copy(rock).lerp(snow, Math.min(1, (h - 13) / 6));

        colors[i * 3] = tmp.r;
        colors[i * 3 + 1] = tmp.g;
        colors[i * 3 + 2] = tmp.b;
      }

      geo.setAttribute('color', new THREE.BufferAttribute(colors, 3));
      geo.computeVertexNormals();

      if (this.mesh) {
        this.terrain.remove(this.mesh);
        this.mesh.geometry.dispose();
        this.mesh.material.dispose();
      }

      this.mesh = new THREE.Mesh(geo, new THREE.MeshStandardMaterial({
        vertexColors: true,
        roughness: 0.94,
        metalness: 0.0,
        flatShading: false
      }));
      this.mesh.receiveShadow = true;
      this.terrain.add(this.mesh);

      this.refreshWaterShore();
    },

    refreshWaterShore: function () {
      if (!this.water) return;

      var geo = this.water.geometry;
      var shore = geo.attributes.aShore;
      if (!shore) return;

      var pos = geo.attributes.position;
      for (var i = 0; i < pos.count; i++) {
        var h = islandHeight(pos.getX(i), pos.getZ(i));
        shore.setX(i, G.clamp01((h + 1.0) / 3.2));
      }
      shore.needsUpdate = true;
    },

    update: function (t) {
      if (this.water) this.water.material.uniforms.uTime.value = t;
    },

    makeContactShadow: makeContactShadow,
    makeDockhand: makeDockhand,

    propCount: function () {
      if (!this.props) return 0;
      var total = 0;
      for (var i = 0; i < this.props.children.length; i++) {
        var child = this.props.children[i];
        total += child.count ? child.count : 1;
      }
      return total;
    }
  };

  function pseudo(n) {
    var s = Math.sin(n * 12.9898) * 43758.5453;
    return s - Math.floor(s);
  }

  function makeProps() {
    var group = new THREE.Group();

    var trunks = [];
    var canopies = [];
    var palms = [];
    var palmCrowns = [];
    var rocks = [];
    var grass = [];

    for (var i = 0; i < ISLANDS.length; i++) {
      var isl = ISLANDS[i];
      var seed = i * 977;
      var samples = Math.floor(isl.radius * isl.radius / 7);

      for (var k = 0; k < samples; k++) {
        var a = pseudo(seed + k * 13) * G.TAU;
        var r = Math.sqrt(pseudo(seed + k * 29)) * isl.radius * 0.95;
        var x = isl.x + Math.cos(a) * r;
        var z = isl.z + Math.sin(a) * r;
        var h = islandHeight(x, z);

        if (h < 0.8) continue;

        var slope = Math.abs(islandHeight(x + 1.4, z) - islandHeight(x - 1.4, z))
          + Math.abs(islandHeight(x, z + 1.4) - islandHeight(x, z - 1.4));
        if (slope > 2.6) continue;

        var roll = pseudo(seed + k * 41);

        if (h < 2.4) {
          if (roll < 0.32) palms.push([x, h, z, 0.75 + pseudo(seed + k * 7) * 0.5, roll * 6.28]);
          else if (roll < 0.62) rocks.push([x, h, z, 0.4 + pseudo(seed + k * 11) * 0.7, roll * 6.28]);
          else grass.push([x, h, z, 0.6 + pseudo(seed + k * 3) * 0.6, roll * 6.28]);
        } else if (h < 15) {
          if (roll < 0.1) rocks.push([x, h, z, 0.5 + pseudo(seed + k * 11) * 0.9, roll * 6.28]);
          else if (roll < 0.2) grass.push([x, h, z, 0.7 + pseudo(seed + k * 3) * 0.7, roll * 6.28]);
          else {
            var sc = 0.8 + pseudo(seed + k * 5) * 0.9;
            trunks.push([x, h, z, sc, roll * 6.28]);
            canopies.push([x, h, z, sc, roll * 6.28]);
          }
        } else if (roll < 0.35) {
          rocks.push([x, h, z, 0.6 + pseudo(seed + k * 11) * 1.1, roll * 6.28]);
        }
      }
    }

    addInstances(group, new THREE.CylinderGeometry(0.16, 0.26, 2.2, 6),
      new THREE.MeshStandardMaterial({ color: 0x5a4028, roughness: 0.94 }), trunks,
      function (p, out, rot, scale) {
        out.set(p[0], p[1] + 1.1 * p[3], p[2]);
        scale.set(p[3], p[3] * (0.85 + pseudo(p[0] * 7.1) * 0.5), p[3]);
      });

    addInstances(group, new THREE.ConeGeometry(1.25, 2.9, 7),
      new THREE.MeshStandardMaterial({ color: 0x3f6b32, roughness: 0.9 }), canopies,
      function (p, out, rot, scale) {
        out.set(p[0], p[1] + 2.9 * p[3], p[2]);
        scale.set(p[3], p[3] * (0.9 + pseudo(p[2] * 3.7) * 0.4), p[3]);
      });

    addInstances(group, new THREE.CylinderGeometry(0.13, 0.22, 3.4, 6),
      new THREE.MeshStandardMaterial({ color: 0x6b5133, roughness: 0.92 }), palms,
      function (p, out, rot, scale) {
        out.set(p[0], p[1] + 1.7 * p[3], p[2]);
        scale.set(p[3], p[3], p[3]);
        rot.z = (pseudo(p[0] * 3.3) - 0.5) * 0.22;
      });

    addInstances(group, new THREE.SphereGeometry(1.15, 6, 5),
      new THREE.MeshStandardMaterial({ color: 0x4c7a34, roughness: 0.88 }), palmCrowns,
      function (p, out, rot, scale) {
        out.set(p[0], p[1] + 3.5 * p[3], p[2]);
        scale.set(p[3] * 1.15, p[3] * 0.55, p[3] * 1.15);
      });

    addInstances(group, new THREE.DodecahedronGeometry(0.7, 0),
      new THREE.MeshStandardMaterial({ color: 0x77746c, roughness: 0.96 }), rocks,
      function (p, out, rot, scale) {
        out.set(p[0], p[1] + 0.28 * p[3], p[2]);
        scale.set(p[3], p[3] * 0.8, p[3] * 1.1);
      });

    addInstances(group, new THREE.ConeGeometry(0.3, 0.9, 4),
      new THREE.MeshStandardMaterial({ color: 0x6f8f3c, roughness: 0.95 }), grass,
      function (p, out, rot, scale) {
        out.set(p[0], p[1] + 0.4 * p[3], p[2]);
        scale.set(p[3], p[3], p[3]);
        rot.z = (pseudo(p[2] * 5.1) - 0.5) * 0.35;
      });

    return group;
  }

  function addInstances(group, geometry, material, list, place) {
    if (!list.length) return null;

    var mesh = new THREE.InstancedMesh(geometry, material, list.length);
    var m = new THREE.Matrix4();
    var pos = new THREE.Vector3();
    var rot = new THREE.Euler();
    var q = new THREE.Quaternion();
    var scale = new THREE.Vector3();

    for (var i = 0; i < list.length; i++) {
      place(list[i], pos, rot, scale);
      q.setFromAxisAngle(new THREE.Vector3(0, 1, 0), list[i][4] || 0);
      m.compose(pos, q, scale);
      mesh.setMatrixAt(i, m);
    }

    mesh.instanceMatrix.needsUpdate = true;
    mesh.receiveShadow = true;
    group.add(mesh);
    return mesh;
  }

  function makeDockhand(scene) {
    var group = new THREE.Group();

    var skin = new THREE.MeshStandardMaterial({ color: 0xd8a882, roughness: 0.85 });
    var shirt = new THREE.MeshStandardMaterial({ color: 0x9a4a3c, roughness: 0.9 });
    var pants = new THREE.MeshStandardMaterial({ color: 0x33404d, roughness: 0.9 });
    var hat = new THREE.MeshStandardMaterial({ color: 0xdcc07a, roughness: 0.9 });

    var torso = new THREE.Mesh(new THREE.BoxGeometry(0.64, 0.8, 0.36), shirt);
    torso.position.y = 1.12;
    group.add(torso);

    var head = new THREE.Mesh(new THREE.BoxGeometry(0.34, 0.34, 0.32), skin);
    head.position.y = 1.7;
    group.add(head);

    var cap = new THREE.Mesh(new THREE.BoxGeometry(0.42, 0.1, 0.4), hat);
    cap.position.y = 1.9;
    group.add(cap);

    var brim = new THREE.Mesh(new THREE.BoxGeometry(0.44, 0.04, 0.24), hat);
    brim.position.set(0, 1.85, 0.26);
    group.add(brim);

    var legL = new THREE.Mesh(new THREE.BoxGeometry(0.22, 0.8, 0.24), pants);
    legL.position.set(-0.16, 0.4, 0);
    group.add(legL);

    var legR = legL.clone();
    legR.position.x = 0.16;
    group.add(legR);

    var armL = new THREE.Mesh(new THREE.BoxGeometry(0.16, 0.62, 0.18), skin);
    armL.position.set(-0.4, 1.16, 0.02);
    group.add(armL);

    var armR = armL.clone();
    armR.position.x = 0.4;
    group.add(armR);

    var crate = new THREE.Mesh(new THREE.BoxGeometry(0.9, 0.6, 0.9),
      new THREE.MeshStandardMaterial({ color: 0x8a6a42, roughness: 0.92 }));
    crate.position.set(1.3, 0.3, 0.2);
    group.add(crate);

    group.userData.armR = armR;
    group.userData.head = head;

    return {
      group: group,
      arm: armR,
      head: head
    };
  }

  function makeContactShadow(radius, opacity) {    var geo = new THREE.CircleGeometry(radius, 18);
    geo.rotateX(-Math.PI / 2);
    var mat = new THREE.MeshBasicMaterial({
      color: 0x0a1a1e,
      transparent: true,
      opacity: opacity,
      depthWrite: false
    });
    var mesh = new THREE.Mesh(geo, mat);
    mesh.renderOrder = 2;
    return mesh;
  }

  function makeSkyDome() {
    var radius = 500;
    var geo = new THREE.SphereGeometry(radius, 24, 16);

    var pos = geo.attributes.position;
    var colors = new Float32Array(pos.count * 3);

    var zenith = new THREE.Color(0x2f6f9e);
    var middle = new THREE.Color(0x9cc6dc);
    var horizon = new THREE.Color(0xf0dcbb);
    var tmp = new THREE.Color();

    for (var i = 0; i < pos.count; i++) {
      var h = pos.getY(i) / radius;
      var t = h * 0.5 + 0.5;

      if (t > 0.5) tmp.copy(horizon).lerp(middle, (t - 0.5) / 0.5);
      else tmp.copy(middle).lerp(zenith, 1 - t / 0.5);

      if (h < 0.02) tmp.copy(horizon);

      colors[i * 3] = tmp.r;
      colors[i * 3 + 1] = tmp.g;
      colors[i * 3 + 2] = tmp.b;
    }

    geo.setAttribute('color', new THREE.BufferAttribute(colors, 3));

    var mesh = new THREE.Mesh(geo, new THREE.MeshBasicMaterial({
      vertexColors: true,
      side: THREE.BackSide,
      fog: false,
      depthWrite: false
    }));

    mesh.frustumCulled = false;
    mesh.renderOrder = -1;
    return mesh;
  }

  function makeClouds() {
    var group = new THREE.Group();
    var material = new THREE.MeshBasicMaterial({
      color: 0xffffff,
      transparent: true,
      opacity: 0.62,
      fog: false,
      depthWrite: false
    });

    var puffs = 26;
    var geo = new THREE.SphereGeometry(1, 8, 6);

    for (var i = 0; i < puffs; i++) {
      var puff = new THREE.Mesh(geo, material);
      var a = pseudo(i * 3.7) * G.TAU;
      var r = 120 + pseudo(i * 5.1) * 220;
      puff.position.set(
        Math.cos(a) * r,
        55 + pseudo(i * 7.3) * 70,
        Math.sin(a) * r
      );
      var sx = 16 + pseudo(i * 11.3) * 26;
      puff.scale.set(sx, sx * 0.32, sx * 0.7);
      group.add(puff);
    }

    group.renderOrder = -1;
    return group;
  }

  function makeWater() {
    var size = 460;
    var seg = 128;
    var geo = new THREE.PlaneGeometry(size, size, seg, seg);
    geo.rotateX(-Math.PI / 2);

    var shore = new Float32Array(geo.attributes.position.count);
    geo.setAttribute('aShore', new THREE.BufferAttribute(shore, 1));

    var material = new THREE.ShaderMaterial({
      transparent: true,
      depthWrite: false,
      uniforms: {
        uTime: { value: 0 },
        uDeep: { value: new THREE.Color(0x0d3550) },
        uShallow: { value: new THREE.Color(0x3f9fb0) },
        uSun: { value: new THREE.Vector3(0.45, 0.78, 0.32).normalize() },
        uSunColor: { value: new THREE.Color(0xfff2d6) }
      },
      vertexShader: [
        'attribute float aShore;',
        'varying float vShore;',
        'varying vec3 vNormalW;',
        'varying vec3 vPosW;',
        'uniform float uTime;',
        WAVE_GLSL,
        'void main() {',
        '  vShore = aShore;',
        '  vec3 p = position;',
        '  p.y += waveHeight(p.xz, uTime);',
        '  vNormalW = waveNormal(p.xz, uTime);',
        '  vec4 world = modelMatrix * vec4(p, 1.0);',
        '  vPosW = world.xyz;',
        '  gl_Position = projectionMatrix * viewMatrix * world;',
        '}'
      ].join('\n'),
      fragmentShader: [
        'varying float vShore;',
        'varying vec3 vNormalW;',
        'varying vec3 vPosW;',
        'uniform float uTime;',
        'uniform vec3 uDeep;',
        'uniform vec3 uShallow;',
        'uniform vec3 uSun;',
        'uniform vec3 uSunColor;',
        'void main() {',
        '  vec3 viewDir = normalize(cameraPosition - vPosW);',
        '  vec3 nrm = normalize(vNormalW);',
        '  float fres = pow(1.0 - clamp(dot(viewDir, nrm), 0.0, 1.0), 4.0);',
        '  float shore = clamp(vShore, 0.0, 1.0);',
        '  vec3 base = mix(uDeep, uShallow, shore);',
        '  vec3 col = mix(base, vec3(0.78, 0.90, 0.96), fres * 0.7);',
        '  vec3 hv = normalize(nrm + normalize(uSun));',
        '  float spec = pow(max(dot(hv, normalize(uSun)), 0.0), 220.0);',
        '  float glitter = pow(max(dot(hv, normalize(uSun)), 0.0), 1400.0);',
        '  col += uSunColor * spec * 2.1;',
        '  col += uSunColor * glitter * 5.0;',
        '  float foam = smoothstep(0.5, 0.9, shore)',
        '    * (0.45 + 0.55 * sin(vPosW.x * 2.3 + vPosW.z * 1.9 - uTime * 2.6));',
        '  col = mix(col, vec3(0.96, 0.99, 1.0), clamp(foam, 0.0, 0.8));',
        '  float alpha = mix(0.78, 0.96, shore);',
        '  float dist = length(cameraPosition - vPosW);',
        '  float fog = smoothstep(150.0, 430.0, dist);',
        '  col = mix(col, vec3(0.62, 0.77, 0.84), fog * 0.9);',
        '  gl_FragColor = vec4(col, mix(alpha, 1.0, fog));',
        '}'
      ].join('\n')
    });

    var mesh = new THREE.Mesh(geo, material);
    mesh.frustumCulled = false;
    mesh.renderOrder = 1;
    mesh.receiveShadow = false;
    return mesh;
  }

  function makeDock() {
    var group = new THREE.Group();

    var plankMat = new THREE.MeshStandardMaterial({ color: 0x6b5236, roughness: 0.9 });
    var postMat = new THREE.MeshStandardMaterial({ color: 0x4a3a28, roughness: 0.95 });

    var deck = new THREE.Mesh(new THREE.BoxGeometry(5.5, 0.22, 17), plankMat);
    deck.position.set(0, 0.55, 32);
    group.add(deck);

    for (var i = 0; i < 6; i++) {
      var z = 25.5 + i * 3.4;
      for (var k = 0; k < 2; k++) {
        var side = k === 0 ? -1 : 1;
        var post = new THREE.Mesh(new THREE.CylinderGeometry(0.14, 0.16, 3.2, 8), postMat);
        post.position.set(side * 2.5, -0.7, z);
        group.add(post);
      }
    }

    var rail = new THREE.Mesh(new THREE.BoxGeometry(0.1, 0.1, 17), postMat);
    rail.position.set(-2.5, 1.2, 32);
    group.add(rail);

    var rail2 = new THREE.Mesh(new THREE.BoxGeometry(0.1, 0.1, 17), postMat);
    rail2.position.set(2.5, 1.2, 32);
    group.add(rail2);

    return group;
  }

})(window);
