'use strict';

const fs = require('fs');
const path = require('path');
const vm = require('vm');

const ROOT = path.join(__dirname, '..');
const DIST = path.join(ROOT, 'dist', 'tidebreak.html');
const THREE = require('./three-stub.js');

let pass = 0, fail = 0;
function ok(name, cond, extra) {
  if (cond) { pass++; console.log('  ok   ' + name); }
  else { fail++; console.log('  FAIL ' + name + (extra !== undefined ? ' -> ' + String(extra) : '')); }
}

if (!fs.existsSync(DIST)) {
  console.error('verify-dist: dist/tidebreak.html missing, run build.js first');
  process.exit(1);
}

const html = fs.readFileSync(DIST, 'utf8');

console.log('\nartifact');
ok('single file present', html.length > 100000, html.length + ' bytes');
ok('no external script tags', !/<script src=/.test(html));
ok('no stylesheet links', !/<link rel="stylesheet"/.test(html));
ok('no cdn references', !/cdn\.jsdelivr|unpkg\.com|https?:\/\/[^"']*\.js/.test(html));
ok('no leftover js paths', !/src="js\//.test(html));
ok('title intact', /<title>Tidebreak<\/title>/.test(html));
ok('build stamp present', /<!-- build \d{4}-/.test(html));

const blocks = [];
const re = /<script>([\s\S]*?)<\/script>/g;
let m;
while ((m = re.exec(html))) blocks.push(m[1]);
ok('all modules inlined', blocks.length === 15, blocks.length + ' blocks');

const styles = /<style>([\s\S]*?)<\/style>/.exec(html);
ok('css inlined', !!styles && styles[1].length > 1000, styles ? styles[1].length : 0);

const isThree = (code) => code.length > 400000;
const vendor = blocks.filter(isThree);
const app = blocks.filter((c) => !isThree(c));
ok('renderer bundled', vendor.length === 1, vendor.length + ' renderer blocks');
ok('app modules bundled', app.length === 14, app.length + ' app blocks');
ok('no api key baked into the artifact', !/sk-[A-Za-z0-9]{20,}/.test(html));

console.log('\nsyntax of bundled app modules');
for (const code of app) {
  let name = 'block';
  const c = code.match(/\/\* ([\w.]+) \*\//);
  if (c) name = c[1];
  let threw = null;
  try { new vm.Script(code, { filename: name }); } catch (e) { threw = e; }
  ok('parses: ' + name, threw === null, threw && threw.message);
}

console.log('\nboots the artifact');

function fakeNode(id) {
  const node = {
    id,
    textContent: '',
    _innerHTML: '',
    get innerHTML() { return this._innerHTML; },
    set innerHTML(v) {
      this._innerHTML = v;
      this.children = [];
      const count = (String(v).match(/class="slot"/g) || []).length;
      for (let i = 0; i < count; i++) this.children.push(fakeNode(id + '-slot-' + i));
    },
    classList: {
      _set: new Set(),
      add(c) { this._set.add(c); },
      remove(c) { this._set.delete(c); },
      contains(c) { return this._set.has(c); },
      toggle(c, on) { if (on) this._set.add(c); else this._set.delete(c); }
    },
    style: {},
    children: [],
    attrs: {},
    setAttribute(k, v) { this.attrs[k] = v; },
    getAttribute(k) { return this.attrs[k] === undefined ? null : this.attrs[k]; },
    querySelector() { return fakeNode(id + '-child'); },
    appendChild(c) { this.children.push(c); },
    addEventListener() {}
  };
  return node;
}

const nodes = {};
const memoryStore = {};
global.localStorage = {
  getItem(k) { return Object.prototype.hasOwnProperty.call(memoryStore, k) ? memoryStore[k] : null; },
  setItem(k, v) { memoryStore[k] = String(v); },
  removeItem(k) { delete memoryStore[k]; }
};
global.window = global;
global.THREE = THREE;
global.document = {
  getElementById(id) {
    if (!nodes[id]) nodes[id] = fakeNode(id);
    return nodes[id];
  },
  createElement(tag) {
    const n = fakeNode(tag);
    if (tag === 'canvas') {
      n.width = 300; n.height = 150;
      n.getContext = () => ({ createLinearGradient: () => ({ addColorStop() {} }), fillRect() {} });
    }
    return n;
  },
  addEventListener() {},
  body: fakeNode('body')
};
global.devicePixelRatio = 1;
global.requestAnimationFrame = function () { return 1; };
global.addEventListener = function () {};
global.performance = global.performance || { now: () => Date.now() };

const GAME_BLOCK = 'game.js';
for (const code of app) {
  const c = code.match(/\/\* ([\w.]+) \*\//);
  const name = c ? c[1] : 'block';
  if (name === GAME_BLOCK) continue;
  vm.runInThisContext(code, { filename: name });
}

const G = global.G;
ok('namespace built from artifact', !!G && !!G.World && !!G.Player && !!G.Boat && !!G.Fishing);

const scene = new THREE.Scene();
const world = G.World.create(scene, new THREE.WebGLRenderer({}));
ok('world boots', !!world.water && !!world.mesh);
ok('islands charted', world.islandList().length === 7);

const vitals = new G.Vitals();
const inventory = new G.Inventory();
const player = new G.Player(scene, world, { vitals, inventory });
player.spawn(2.5, 24);
inventory.grantStarter();
const boat = new G.Boat(scene, world, { x: 7, z: 46, yaw: 0.1 });
const pool = new G.CreaturePool(scene, world, 10);
const fishing = new G.Fishing(scene, world, player, { rod: G.Catalog.get(1) });
const hud = new G.Hud();

let threw = null;
try {
  for (let i = 0; i < 300; i++) {
    const t = i / 60;
    world.update(t);
    player.update(1 / 60, t);
    boat.update(1 / 60, t);
    pool.update(1 / 60, t, player, inventory);
    fishing.update(1 / 60, t);
    vitals.tick(1 / 60);
    hud.refreshHotbar(inventory);
    hud.refreshVitals(vitals);
    hud.refreshWeight(inventory);
    hud.refreshRod(fishing, inventory);
  }
} catch (e) { threw = e; }
ok('300 full game frames without throwing', threw === null, threw && threw.message);
ok('player finite after frames',
  isFinite(player.position.x) && isFinite(player.position.y) && isFinite(player.position.z));
ok('boat finite after frames', isFinite(boat.position.y) && isFinite(boat.yaw));
ok('inventory still consistent', inventory.weight() >= 0 && inventory.weight() <= inventory.maxWeight + 0.001,
  inventory.weight());
let ranGame = null;
try {
  vm.runInThisContext(app.find(c => (c.match(/\/\* ([\w.]+) \*\//) || [])[1] === GAME_BLOCK),
    { filename: 'game.js' });
  ranGame = global.Tidebreak || null;
} catch (e) {
  ok('game.js runs in the artifact', false, e.message);
}

ok('game.js runs in the artifact', ranGame !== null);
if (ranGame) {
  ok('no boot faults', ranGame.errors().length === 0, ranGame.errors().join(' | '));
  ok('scene has the ocean', ranGame.world.water && ranGame.world.water.geometry != null);
  ok('scene has terrain', ranGame.world.mesh != null);
  ok('scene has a sky dome', ranGame.world.sky != null);
  ok('scene has clouds', ranGame.world.clouds != null && ranGame.world.clouds.children.length > 0);
  ok('scene has props', ranGame.world.props != null && ranGame.world.propCount() > 50);
  ok('dockhand is in the scene', (function () {
    var found = false;
    ranGame.scene.traverse(function (o) { if (o.userData && o.userData.armR) found = true; });
    return found;
  })());
  ok('chat panel markup shipped', /id="aiChat"/.test(html) && /id="aiChatForm"/.test(html));
  ok('no secret in the shipped artifact', !/sk-[A-Za-z0-9]{20,}/.test(html));
  ok('sky dome renders behind everything', ranGame.world.sky.renderOrder === -1);

  const before = ranGame.renderer.renderCount;
  let frames = 0;
  let frameThrew = null;
  try {
    for (let i = 0; i < 90; i++) {
      ranGame.step(1 / 60, i / 60);
      frames++;
    }
  } catch (e) { frameThrew = e; }
  ok('90 real game frames run', frameThrew === null, frameThrew && frameThrew.message);
  ok('renderer is called every frame', ranGame.renderer.renderCount - before >= 90,
    ranGame.renderer.renderCount - before);
  ok('renderer received the scene', ranGame.renderer.lastScene === ranGame.scene);
  ok('renderer received the camera', ranGame.renderer.lastCamera === ranGame.camera);
  ok('camera is above the ground, not inside it',
    ranGame.camera.position.y > ranGame.world.terrainHeight(ranGame.camera.position.x, ranGame.camera.position.z) - 0.5,
    ranGame.camera.position.y + ' vs terrain ' +
    ranGame.world.terrainHeight(ranGame.camera.position.x, ranGame.camera.position.z));
  ok('camera is finite', isFinite(ranGame.camera.position.x) && isFinite(ranGame.camera.position.y));
  ok('no faults after driving frames', ranGame.errors().length === 0, ranGame.errors().join(' | '));
}

const manifestPath = path.join(ROOT, 'dist', 'build-manifest.json');
ok('manifest written', fs.existsSync(manifestPath));
if (fs.existsSync(manifestPath)) {
  const manifest = JSON.parse(fs.readFileSync(manifestPath, 'utf8'));
  ok('manifest lists every module', manifest.modules.length === 16, manifest.modules.length);
  ok('manifest byte count matches file',
    manifest.bytes === Buffer.byteLength(html, 'utf8'), manifest.bytes);
}

console.log('\n' + pass + ' passed, ' + fail + ' failed');
process.exit(fail === 0 ? 0 : 1);
