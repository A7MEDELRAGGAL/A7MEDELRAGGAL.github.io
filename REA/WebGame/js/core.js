(function (global) {
  'use strict';

  var G = global.G || (global.G = {});

  G.TAU = Math.PI * 2;

  G.clamp = function (v, a, b) { return v < a ? a : (v > b ? b : v); };
  G.clamp01 = function (v) { return v < 0 ? 0 : (v > 1 ? 1 : v); };
  G.lerp = function (a, b, t) { return a + (b - a) * t; };
  G.smooth = function (t) { return t * t * (3 - 2 * t); };
  G.rand = function (a, b) { return a + Math.random() * (b - a); };
  G.randInt = function (a, b) { return Math.floor(a + Math.random() * (b - a + 1)); };
  G.pick = function (list) { return list[Math.floor(Math.random() * list.length)]; };
  G.dist2 = function (ax, az, bx, bz) { var dx = ax - bx, dz = az - bz; return dx * dx + dz * dz; };

  G.damp = function (current, target, lambda, dt) {
    return G.lerp(current, target, 1 - Math.exp(-lambda * dt));
  };

  G.easeOutCubic = function (t) { var f = t - 1; return f * f * f + 1; };

  G.Input = {
    keys: Object.create(null),
    pressed: Object.create(null),
    mouse: { dx: 0, dy: 0, left: false, leftDown: false, wheel: 0 },
    locked: false,
    enabled: true,

    init: function (canvas) {
      var self = this;

      global.addEventListener('keydown', function (e) {
        if (!self.enabled) return;
        var code = e.code || e.key;
        if (!self.keys[code]) self.pressed[code] = true;
        self.keys[code] = true;
        if (code === 'Space' || code.indexOf('Arrow') === 0) e.preventDefault();
      });

      global.addEventListener('keyup', function (e) {
        var code = e.code || e.key;
        self.keys[code] = false;
      });

      global.addEventListener('blur', function () { self.keys = Object.create(null); });

      canvas.addEventListener('mousedown', function (e) {
        if (e.button === 0) { self.mouse.left = true; self.mouse.leftDown = true; }
      });

      global.addEventListener('mouseup', function (e) {
        if (e.button === 0) self.mouse.left = false;
      });

      global.addEventListener('mousemove', function (e) {
        if (!self.locked) return;
        self.mouse.dx += e.movementX || 0;
        self.mouse.dy += e.movementY || 0;
      });

      global.addEventListener('wheel', function (e) {
        if (!self.locked) return;
        self.mouse.wheel += e.deltaY;
        e.preventDefault();
      }, { passive: false });

      document.addEventListener('pointerlockchange', function () {
        self.locked = document.pointerLockElement === canvas;
        if (G.onLockChange) G.onLockChange(self.locked);
      });
    },

    lock: function (canvas) {
      if (canvas.requestPointerLock) canvas.requestPointerLock();
    },

    down: function (code) { return !!this.keys[code]; },

    hit: function (code) { return !!this.pressed[code]; },

    axis: function (neg, pos) {
      return (this.down(pos) ? 1 : 0) - (this.down(neg) ? 1 : 0);
    },

    endFrame: function () {
      this.pressed = Object.create(null);
      this.mouse.dx = 0;
      this.mouse.dy = 0;
      this.mouse.leftDown = false;
      this.mouse.wheel = 0;
    }
  };

  G.Loop = {
    running: false,
    last: 0,
    elapsed: 0,
    fps: 0,
    _acc: 0,
    _frames: 0,
    steps: [],
    fixedStep: 1 / 60,

    start: function (stepFn) {
      this.steps = stepFn || [];
      var self = this;

      function frame(now) {
        if (!self.running) return;
        global.requestAnimationFrame(frame);

        var dt = (now - self.last) / 1000;
        self.last = now;
        if (!(dt > 0)) dt = self.fixedStep;
        if (dt > 0.25) dt = 0.25;

        self.elapsed += dt;
        self._acc += dt;
        self._frames++;
        if (self._acc >= 0.5) {
          self.fps = Math.round(self._frames / self._acc);
          self._acc = 0;
          self._frames = 0;
        }

        for (var i = 0; i < self.steps.length; i++) self.steps[i](dt, self.elapsed);

        G.Input.endFrame();
      }

      this.running = true;
      this.last = global.performance.now();
      global.requestAnimationFrame(frame);
    }
  };

  G.toast = (function () {
    var node = null;
    var timer = 0;

    return function (text, seconds) {
      if (!node) node = document.getElementById('toast');
      if (!node) return;
      node.textContent = text;
      node.classList.remove('hidden');
      timer = seconds || 2.2;
    };
  })();

  G.tickToast = function (dt) {
    var node = document.getElementById('toast');
    if (!node || node.classList.contains('hidden')) return;

    var current = parseFloat(node.getAttribute('data-t') || '0');
    var next = current + dt;
    node.setAttribute('data-t', String(next));

    if (next > 0.1) {
      var left = parseFloat(node.getAttribute('data-left') || '0') - dt;
      node.setAttribute('data-left', String(Math.max(0, left)));
      if (left <= 0) node.classList.add('hidden');
    }
  };

  G.showToast = function (text, seconds) {
    var node = document.getElementById('toast');
    if (!node) return;
    node.textContent = text;
    node.classList.remove('hidden');
    node.setAttribute('data-t', '0');
    node.setAttribute('data-left', String(seconds || 2.2));
  };

  G.Noise = (function () {
    var perm = new Uint8Array(512);
    var p = [];
    var i;

    for (i = 0; i < 256; i++) p[i] = i;
    for (i = 255; i > 0; i--) {
      var j = Math.floor(Math.random() * (i + 1));
      var t = p[i]; p[i] = p[j]; p[j] = t;
    }
    for (i = 0; i < 512; i++) perm[i] = p[i & 255];

    function fade(t) { return t * t * t * (t * (t * 6 - 15) + 10); }

    function grad(hash, x, y) {
      switch (hash & 3) {
        case 0: return x + y;
        case 1: return -x + y;
        case 2: return x - y;
        default: return -x - y;
      }
    }

    function value2(x, y) {
      var X = Math.floor(x) & 255;
      var Y = Math.floor(y) & 255;
      var xf = x - Math.floor(x);
      var yf = y - Math.floor(y);
      var u = fade(xf);
      var v = fade(yf);
      var aa = perm[perm[X] + Y];
      var ab = perm[perm[X] + Y + 1];
      var ba = perm[perm[X + 1] + Y];
      var bb = perm[perm[X + 1] + Y + 1];
      var x1 = G.lerp(grad(aa, xf, yf), grad(ba, xf - 1, yf), u);
      var x2 = G.lerp(grad(ab, xf, yf - 1), grad(bb, xf - 1, yf - 1), u);
      return (G.lerp(x1, x2, v) + 1) * 0.5;
    }

    return {
      value2: value2,

      fbm: function (x, y, octaves, gain, lacunarity) {
        octaves = octaves || 4;
        gain = gain === undefined ? 0.5 : gain;
        lacunarity = lacunarity === undefined ? 2.0 : lacunarity;

        var amp = 1;
        var freq = 1;
        var sum = 0;
        var norm = 0;

        for (var o = 0; o < octaves; o++) {
          sum += value2(x * freq, y * freq) * amp;
          norm += amp;
          amp *= gain;
          freq *= lacunarity;
        }

        return norm > 0 ? sum / norm : 0;
      }
    };
  })();

})(window);
