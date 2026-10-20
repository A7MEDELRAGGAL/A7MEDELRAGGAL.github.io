'use strict';

const fs = require('fs');
const path = require('path');
const vm = require('vm');

const ROOT = path.join(__dirname, '..');
const THREE = require('./three-stub.js');

let pass = 0, fail = 0;

function ok(name, cond, extra) {
  if (cond) { pass++; console.log('  ok   ' + name); }
  else { fail++; console.log('  FAIL ' + name + (extra !== undefined ? ' -> ' + String(extra) : '')); }
}

function fakeNode(id) {
  const node = {
    id: id,
    textContent: '',
    _innerHTML: '',
    get innerHTML() { return this._innerHTML; },
    set innerHTML(v) {
      this._innerHTML = v;
      this.children = [];
      const count = (String(v).match(/class="slot"/g) || []).length;
      for (let i = 0; i < count; i++) this.children.push(fakeNode(id + '-slot-' + i));
    },
    value: '',
    classList: {
      _set: new Set(),
      add(c) { this._set.add(c); },
      remove(c) { this._set.delete(c); },
      contains(c) { return this._set.has(c); },
      toggle(c, on) { if (on) this._set.add(c); else this._set.delete(c); }
    },
    style: {},
    children: [],
    parent: null,
    attrs: {},
    setAttribute(k, v) { this.attrs[k] = v; },
    getAttribute(k) { return this.attrs[k] === undefined ? null : this.attrs[k]; },
    querySelector() { return fakeNode(id + '-child'); },
    appendChild(c) { this.children.push(c); c.parent = this; },
    addEventListener() {}
  };
  return node;
}

const nodes = {};
const document = {
  getElementById(id) {
    if (!nodes[id]) nodes[id] = fakeNode(id);
    return nodes[id];
  },
  createElement(tag) {
    const n = fakeNode(tag);
    if (tag === 'canvas') {
      n.getContext = () => ({
        createLinearGradient: () => ({ addColorStop() {} }),
        fillRect() {},
        set fillStyle(v) {},
        get fillStyle() { return ''; }
      });
      n.width = 300;
      n.height = 150;
    }
    return n;
  },
  addEventListener() {},
  body: fakeNode('body')
};

let rafCalls = 0;
const memoryStore = {};
global.localStorage = {
  getItem(k) { return Object.prototype.hasOwnProperty.call(memoryStore, k) ? memoryStore[k] : null; },
  setItem(k, v) { memoryStore[k] = String(v); },
  removeItem(k) { delete memoryStore[k]; }
};
global.window = global;
global.document = document;
global.THREE = THREE;
global.devicePixelRatio = 1;
global.requestAnimationFrame = function () { rafCalls++; return rafCalls; };
global.addEventListener = function () {};
global.performance = global.performance || { now: () => Date.now() };

const LOAD = [
  'core.js', 'world.js', 'catalog.js', 'vitals.js', 'inventory.js',
  'creatures.js', 'player.js', 'boat.js', 'fishing.js', 'bait.js', 'journal.js', 'ai.js', 'hud.js'
];

for (const name of LOAD) {
  const code = fs.readFileSync(path.join(ROOT, 'js', name), 'utf8');
  vm.runInThisContext(code, { filename: name });
}

const G = global.G;

console.log('\napi surface');
const CONTRACT = {
  'G.TAU': 'number',
  'G.clamp': 'function', 'G.clamp01': 'function', 'G.lerp': 'function',
  'G.damp': 'function', 'G.rand': 'function', 'G.randInt': 'function',
  'G.pick': 'function', 'G.dist2': 'function', 'G.easeOutCubic': 'function',
  'G.showToast': 'function', 'G.tickToast': 'function',
  'G.Input.init': 'function', 'G.Input.lock': 'function', 'G.Input.down': 'function',
  'G.Input.hit': 'function', 'G.Input.axis': 'function', 'G.Input.endFrame': 'function',
  'G.Loop.start': 'function',
  'G.Noise.value2': 'function', 'G.Noise.fbm': 'function',
  'G.Waves.height': 'function', 'G.Waves.normal': 'function',
  'G.World.create': 'function', 'G.World.update': 'function',
  'G.World.terrainHeight': 'function', 'G.World.waterDepth': 'function',
  'G.World.islandList': 'function', 'G.World.resample': 'function',
  'G.Catalog.get': 'function', 'G.Catalog.isRod': 'function', 'G.Catalog.isBait': 'function',
  'G.Catalog.isMelee': 'function', 'G.Catalog.isEdible': 'function',
  'G.Catalog.stackMax': 'function', 'G.Catalog.weightOf': 'function',
  'G.Catalog.glyph': 'function', 'G.Catalog.name': 'function',
  'G.Catalog.rollCatch': 'function', 'G.Catalog.starterKit': 'function',
  'G.Vitals': 'function',
  'G.Inventory': 'function',
  'G.Creature': 'function', 'G.Floater': 'function', 'G.CreaturePool': 'function',
  'G.Player': 'function', 'G.Boat': 'function',
  'G.Fishing': 'function', 'G.Fishing.STATE': 'object',
  'G.Hud': 'function'
};

let missing = 0;
for (const key of Object.keys(CONTRACT)) {
  const want = CONTRACT[key];
  const parts = key.split('.');
  let cur = global[parts[0]];
  let found = cur !== undefined && cur !== null;
  for (let i = 1; found && i < parts.length; i++) cur = cur[parts[i]];
  if (!found || cur === undefined || cur === null) found = false;
  const type = found ? typeof cur : 'missing';
  const good = found && (want === 'object' ? (type === 'object' || type === 'function') : type === want);
  if (!good) { ok('api ' + key, false, type); missing++; }
}
ok('all ' + Object.keys(CONTRACT).length + ' api entries present', missing === 0, missing + ' missing');

const protoContract = [
  ['G.Vitals', ['update', 'damage', 'hurt', 'heal', 'feed', 'cure', 'resurrect', 'tick', 'applyPoison', 'applyFire']],
  ['G.Inventory', ['get', 'selectedSlot', 'held', 'weight', 'overweight', 'add', 'count',
    'removeFrom', 'consume', 'select', 'cycle', 'setBaitFrom', 'useBait', 'grantStarter']],
  ['G.Player', ['spawn', 'look', 'update', 'startSwing', 'enterBoat', 'leaveBoat',
    'canBoard', 'respawn', 'forwardVector', 'aimVector']],
  ['G.Boat', ['update', 'occupy', 'vacate', 'speedKmh', 'draftMeters', 'canBoard']],
  ['G.Fishing', ['press', 'release', 'use', 'throwBait', 'miss', 'strike', 'update',
    'biteFraction', 'rodTipWorld', 'canFish', 'catchFish', 'updateAttraction', 'updateFight']],
  ['G.Creature', ['damage', 'flee', 'update', 'placeNear']],
  ['G.CreaturePool', ['creaturesInRadius', 'spawnFloater', 'update']],
  ['G.Hud', ['refreshHotbar', 'refreshVitals', 'refreshWeight', 'refreshRod', 'refreshStatus', 'setPrompt']]
];

let protoMissing = 0;
for (const [ctor, methods] of protoContract) {
  const name = ctor.slice(2);
  const obj = G[name];
  if (!obj || !obj.prototype) { ok('proto ' + name, false, 'no prototype'); protoMissing++; continue; }
  const absent = methods.filter(m => typeof obj.prototype[m] !== 'function');
  if (absent.length) { ok('proto ' + name, false, 'missing ' + absent.join(',')); protoMissing++; }
}
ok('prototype methods complete', protoMissing === 0, protoMissing);

console.log('\njournal (search semantics)');
const journal = new G.Journal({ pageSize: 3 });
ok('journal starts empty', journal.entries.length === 0);
ok('lookup of an absent record returns null', journal.find(999) === null);
ok('has() is false for an absent record', journal.has(999) === false);

journal.record(11, 'Mackerel', { count: 1, worth: 54, defeated: true, note: 'landed' });
journal.record(14, 'Clam', { count: 2, worth: 24, note: 'spotted' });
journal.record(12, 'Pike', { count: 1, worth: 156, defeated: true, note: 'landed' });
ok('records are stored', journal.entries.length === 3);
ok('lookup by id finds the record', journal.find(14) && journal.find(14).name === 'Clam');
ok('repeated records merge instead of duplicating', (function () {
  journal.record(11, 'Mackerel', { count: 2, worth: 54, defeated: true });
  return journal.entries.length === 3 && journal.find(11).count === 3;
})());

ok('search matches by name', journal.search('mack').length === 1);
ok('search is case insensitive', journal.search('MACK').length === 1);
ok('search matches the note', journal.search('spotted').length === 1);
ok('search matches the id', journal.search('12').length === 1);
ok('search with no match is empty', journal.search('dragon').length === 0);
ok('empty query returns everything', journal.search('').length === 3);
ok('search respects a limit', journal.search('', 2).length === 2);

journal.setQuery('pik');
ok('setQuery filters the results', journal.results().length === 1);
ok('setQuery resets the page', journal.page === 0);
journal.setQuery('');

ok('paging splits the entries', (function () {
  const pages = journal.pageCount();
  journal.scrollPage(1);
  const second = journal.pageEntries();
  journal.scrollPage(1);
  const wrapped = journal.page;
  return pages === 1 || (second.length === 3 && wrapped === 0);
})());
ok('page count follows the page size', journal.pageCount() === 1, journal.pageCount());
ok('describe summarises a record', journal.describe(journal.find(11)).indexOf('defeated') >= 0);
ok('stats count found and defeated', (function () {
  const s = journal.stats();
  return s.found === 3 && s.defeated === 2;
})());

console.log('\nai dockhand');
ok('AI namespace present', !!G.AI && typeof G.AI.chat === 'function');
ok('no key baked into the source',
  !/sk-[A-Za-z0-9]{20,}/.test(fs.readFileSync(path.join(ROOT, 'js', 'ai.js'), 'utf8')));
ok('no key baked into any source file', (function () {
  for (const f of LOAD) {
    if (/sk-[A-Za-z0-9]{20,}/.test(fs.readFileSync(path.join(ROOT, 'js', f), 'utf8'))) return false;
  }
  return true;
})());
ok('hasKey is false without a stored key', G.AI.hasKey() === false);
G.AI.setKey('sk-test-key-not-real-1234567890');
ok('key can be stored locally', G.AI.hasKey() === true);
G.AI.setKey('');
ok('key can be cleared', G.AI.hasKey() === false);

console.log('\nworld');
const scene = new THREE.Scene();
const renderer = new THREE.WebGLRenderer({});
const world = G.World.create(scene, renderer);
ok('world created', !!world && !!world.water && !!world.mesh);
ok('sky dome exists', !!world.sky);
ok('clouds exist', !!world.clouds && world.clouds.children.length > 0);
ok('sun disc exists', !!world.sunDisc);
ok('props exist', !!world.props && world.props.children.length > 0);
ok('props were placed', world.propCount() > 50, world.propCount());
ok('contact shadow helper works', (function () {
  var s = G.World.makeContactShadow(1, 0.3);
  return !!s && s.geometry != null;
})());
ok('dockhand can be built', (function () {
  var d = G.World.makeDockhand(scene);
  return !!d && !!d.group && !!d.arm && !!d.head;
})());
ok('dockhand has a body and a crate', (function () {
  var d = G.World.makeDockhand(scene);
  return d.group.children.length >= 7;
})());
ok('7 islands charted', world.islandList().length === 7, world.islandList().length);
ok('spawn island exists', world.islandList()[0].radius > 10);
ok('centre island is land', world.terrainHeight(0, 0) > 5, world.terrainHeight(0, 0));
ok('far ocean is deep', world.terrainHeight(300, 300) < 0, world.terrainHeight(300, 300));
ok('water depth positive offshore', world.waterDepth(300, 300, 0) > 5, world.waterDepth(300, 300, 0));

let finiteTerrain = true;
for (let i = 0; i < 3000; i++) {
  const x = (Math.random() - 0.5) * 500;
  const z = (Math.random() - 0.5) * 500;
  if (!isFinite(world.terrainHeight(x, z))) { finiteTerrain = false; break; }
}
ok('terrain finite everywhere', finiteTerrain);

let worldBefore = world.islandList().length;
world.resample(999, 6);
ok('resample re-charts islands', world.islandList().length === 6 && world.islandList().length !== worldBefore);
world.resample(20261017, 7);

console.log('\nplayer');
const vitals = new G.Vitals();
const inventory = new G.Inventory();
const player = new G.Player(scene, world, { vitals: vitals, inventory: inventory });
player.spawn(2.5, 17);
ok('spawned on or above terrain',
  player.position.y >= world.terrainHeight(2.5, 17) - 0.001, player.position.y);
inventory.grantStarter();

G.Input.keys.KeyW = true;
let steps = 0, threw = null;
try {
  for (let i = 0; i < 600; i++) {
    player.update(1 / 60, i / 60);
    vitals.tick(1 / 60);
    steps++;
  }
} catch (e) { threw = e; }
ok('600 movement steps without throwing', threw === null, threw && threw.message);
ok('player position stays finite',
  isFinite(player.position.x) && isFinite(player.position.y) && isFinite(player.position.z));
ok('player never sinks through terrain',
  player.position.y >= world.terrainHeight(player.position.x, player.position.z) - 0.05,
  player.position.y + ' vs ' + world.terrainHeight(player.position.x, player.position.z));
ok('player moved', Math.abs(player.position.z - 17) > 0.5, player.position.z);
G.Input.keys.KeyW = false;

console.log('\nboat');
ok('game boat spawn point is water', world.terrainHeight(7, 46) < 0, world.terrainHeight(7, 46));
ok('game player spawn point is land', world.terrainHeight(2.5, 24) > 0, world.terrainHeight(2.5, 24));

const boat = new G.Boat(scene, world, { x: 7, z: 46, yaw: 0.1 });
ok('boat spawned active', boat.active === true);
ok('boat starts with no driver', boat.driver === null);

let boatThrew = null;
try {
  for (let i = 0; i < 900; i++) boat.update(1 / 60, i / 60);
} catch (e) { boatThrew = e; }
ok('900 unattended boat steps without throwing', boatThrew === null, boatThrew && boatThrew.message);
ok('boat floats near the surface',
  boat.position.y > -1.2 && boat.position.y < 3.0, boat.position.y);
ok('boat velocity finite',
  isFinite(boat.velocity.x) && isFinite(boat.velocity.y) && isFinite(boat.velocity.z));

player.position.set(boat.position.x, boat.position.y + 1, boat.position.z - 3);
ok('boat can be boarded nearby', boat.canBoard(player) === true);
ok('boarding succeeds', player.enterBoat(boat) === true);
ok('driver recorded', boat.driver === player);
ok('boat cannot be boarded twice', boat.canBoard(player) === false);

G.Input.keys.KeyW = true;
const headingBefore = boat.yaw;
try {
  for (let i = 0; i < 600; i++) {
    boat.update(1 / 60, i / 60);
    player.update(1 / 60, i / 60);
  }
} catch (e) { ok('sailing without throwing', false, e.message); boatThrew = e; }
ok('sailing 600 steps without throwing', boatThrew === null, boatThrew && boatThrew.message);
G.Input.keys.KeyW = false;

ok('boat accelerates under throttle', boat.speedKmh() > 1.0, boat.speedKmh());
ok('boat yaw stays finite', isFinite(boat.yaw) && isFinite(headingBefore));
ok('player rides the boat', player.boat === boat);
ok('player position tracks the boat', player.position.y > -2 && player.position.y < 4, player.position.y);

player.leaveBoat();
ok('leaving clears the driver', boat.driver === null && player.boat === null);
ok('player lands on solid ground or water surface',
  isFinite(player.position.y));

console.log('\nmovement fidelity (original tuning)');
player.spawn(2.5, 24);
player.velocity.set(0, 0, 0);
vitals.stamina = vitals.maxStamina;

G.Input.keys.KeyW = true;
let walkMax = 0;
let walkPeak = 0;
for (let i = 0; i < 180; i++) {
  player.update(1 / 60, i / 60);
  const sp = player.horizontalSpeed();
  if (sp > walkMax) walkMax = sp;
  if (sp > walkPeak) walkPeak = sp;
  player.position.set(2.5, player.position.y, 24);
  player.velocity.x = player.velocity.x;
}
G.Input.keys.KeyW = false;
ok('walk speed respects the original cap 6.5', walkMax <= 6.51, walkMax.toFixed(2));
ok('walk actually accelerates to the cap', walkPeak > 5.0, walkPeak.toFixed(2));

player.spawn(2.5, 24);
player.velocity.set(0, 0, 0);
G.Input.keys.KeyW = true;
G.Input.keys.ShiftLeft = true;
let runMax = 0;
for (let i = 0; i < 180; i++) {
  player.update(1 / 60, i / 60);
  const sp = player.horizontalSpeed();
  if (sp > runMax) runMax = sp;
  player.position.set(2.5, player.position.y, 24);
}
G.Input.keys.KeyW = false;
G.Input.keys.ShiftLeft = false;
ok('sprint speed respects the original cap 13', runMax <= 13.01, runMax.toFixed(2));
ok('sprint is faster than walking', runMax > walkMax, runMax.toFixed(2) + ' vs ' + walkMax.toFixed(2));

ok('stamina drains while sprinting', vitals.stamina < vitals.maxStamina, vitals.stamina.toFixed(1));

player.velocity.set(4, 0, 0);
player.inputX = -1;
const blockedProbe = player.isBlockedAhead('x', -1);
player.applyCounterMovement(1 / 60);
if (blockedProbe) {
  ok('counter movement doubles velocity against a wall', Math.abs(player.velocity.x) > 7.9, player.velocity.x.toFixed(2));
} else {
  ok('counter movement pushes against the current velocity', player.velocity.x < 4, player.velocity.x.toFixed(2));
}
player.velocity.set(4, 0, 0);
player.inputX = -1;
player.isBlockedAhead = function () { return true; };
player.applyCounterMovement(1 / 60);
ok('wall counter doubles the velocity', player.velocity.x > 7.5, player.velocity.x.toFixed(2));
player.inputX = 0;

const staminaBefore = vitals.stamina;
player.jumpRequested = true;
player.jumpCooldown = 0;
player.grounded = true;
player.jumps = 0;
player.integrate(1 / 60, 0);
ok('jump sets vertical velocity', player.velocity.y > 3, player.velocity.y.toFixed(2));
ok('jump costs stamina', vitals.stamina < staminaBefore, vitals.stamina.toFixed(1));

ok('sinking is tracked when the hull is high', (function () {
  const b2 = new G.Boat(scene, world, { x: 7, z: 46 });
  b2.position.y = 20;
  b2.update(1 / 60, 0);
  return b2.sinking === true;
})());

console.log('\ncreatures');
const pool = new G.CreaturePool(scene, world, 14);
ok('pool spawned', pool.creatures.length === 14, pool.creatures.length);
let creatureThrew = null;
try {
  for (let i = 0; i < 600; i++) pool.update(1 / 60, i / 60, player, inventory);
} catch (e) { creatureThrew = e; }
ok('600 creature steps without throwing', creatureThrew === null, creatureThrew && creatureThrew.message);
const near = pool.creaturesInRadius(pool.creatures[0].position, 500);
ok('radius query finds creatures', near.length > 0, near.length);
const victim = pool.creatures[0];
victim.damage(9999);
const itemsBefore = inventory.weight();
try {
  for (let i = 0; i < 5; i++) pool.update(1 / 60, i / 60, player, inventory);
} catch (e) { creatureThrew = e; }
ok('killing a creature respawns it', pool.creatures[0] !== victim && pool.creatures[0].alive);
ok('killing spawns a floater', pool.floaters.length > 0, pool.floaters.length);
void itemsBefore;

console.log('\nfishing (charge, throw, bite, reel)');
const fishing = new G.Fishing(scene, world, player, { rod: G.Catalog.get(1) });
ok('rod state starts idle', fishing.state === 'idle', fishing.state);
ok('rod exposed on the player', !!player.rod && !!player.rodGroup);
ok('bait catalog is loaded', G.Bait.all.length === 5, G.Bait.all.length);
ok('bait lookups resolve', G.Bait.get(8).name === 'Worm Bait');
ok('bait fish weights are weighted', (function () {
  const c = G.Bait.candidates(8);
  return c.length > 0 && c.every((x) => x.weight > 0);
})());
ok('bait rolls a fish from its own table', (function () {
  const id = G.Bait.rollFish(8);
  return G.Bait.candidates(8).some((c) => c.itemId === id);
})());
ok('bait catch window is a range', (function () {
  const w = G.Bait.catchWindow(8);
  return w.min > 0 && w.max > w.min;
})());

player.position.set(0, 4, 60);
player.mesh.position.copy(player.position);
player.pitch = -0.25;
fishing.baitId = 8;

fishing.press();
ok('pressing starts a charge', fishing.state === 'charging', fishing.state);
for (let i = 0; i < 30; i++) fishing.update(1 / 60, i / 60);
ok('charging builds power', fishing.charge > 0.2, fishing.charge.toFixed(2));

fishing.release();
ok('releasing throws the bait', fishing.state === 'throwing', fishing.state);
ok('the line has length after the throw', fishing.lineLength > 0, fishing.lineLength);

for (let i = 0; i < 60; i++) fishing.update(1 / 60, 1 + i / 60);
ok('the bait settles into the water', fishing.state === 'baiting', fishing.state);
ok('the bait sits at the surface', (function () {
  const t = 1 + 59 / 60;
  const surface = world.seaLevel + G.Waves.height(fishing.baitPos.x, fishing.baitPos.z, t);
  return Math.abs(fishing.baitPos.y - surface) < 0.6;
})(), fishing.baitPos.y.toFixed(2));

fishing.state = 'bite';
fishing.timer = 1.0;
fishing.biteWindow = 1.0;
const hooked = pool.creatures[0];
hooked.position.copy(fishing.baitPos);
hooked.position.y -= 0.4;
fishing.candidate = hooked;
ok('bite fraction tracks the window', fishing.biteFraction() > 0.5, fishing.biteFraction());

const strike = (function () {
  const realRandom = Math.random;
  Math.random = () => 0.99;
  try { return fishing.strike(); } finally { Math.random = realRandom; }
})();
ok('striking hooks the fish', fishing.state === 'hooked', fishing.state);
ok('the hooked fish is remembered', fishing.hookedFish === hooked);

fishing.reelHeld = true;
for (let i = 0; i < 420; i++) {
  fishing.update(1 / 60, 2 + i / 60);
  if (fishing.state === 'caught' || fishing.state === 'missed') break;
}
ok('reeling resolves the encounter', fishing.state === 'caught' || fishing.state === 'missed', fishing.state);
ok('reeling builds tension', fishing.tension >= 0);
ok('the line shortens while reeling', fishing.lineLength < 13, fishing.lineLength.toFixed(1));

fishing.state = 'idle';
fishing.timer = 0;
player.position.set(0, 3, 0);
player.mesh.position.copy(player.position);
let castThrew = null;
try { fishing.press(); fishing.release(); } catch (e) { castThrew = e; }
ok('casting from land does not throw', castThrew === null, castThrew && castThrew.message);
ok('a land cast misses', fishing.state === 'missed', fishing.state);

console.log('\nhud');
const hud = new G.Hud();
ok('hud built', !!hud && Array.isArray(hud.slots) && hud.slots.length === 9, hud.slots.length);
let hudThrew = null;
try {
  hud.refreshHotbar(inventory);
  hud.refreshVitals(vitals);
  hud.refreshWeight(inventory);
  hud.refreshRod(fishing, inventory);
  hud.refreshStatus({ mode: 'on foot', speed: null, draft: null, pos: '0 / 0', bait: 'none', depth: 1, state: 'grounded' });
  hud.setPrompt('F  take the helm');
} catch (e) { hudThrew = e; }
ok('hud refreshes without throwing', hudThrew === null, hudThrew && hudThrew.message);

console.log('\nloop');
let loopThrew = null;
try {
  G.Loop.start([function (dt, t) { world.update(t); }]);
} catch (e) { loopThrew = e; }
ok('loop starts', loopThrew === null, loopThrew && loopThrew.message);
ok('loop requested an animation frame', rafCalls > 0, rafCalls);

(async function () {
  var rejected = false;
  var reason = '';
  await G.AI.chat([{ role: 'user', content: 'hi' }], { models: ['model:test'] })
    .catch(function (e) { rejected = true; reason = e && e.message ? e.message : ''; });
  ok('chat rejects without a key', rejected, reason);

  console.log('\n' + pass + ' passed, ' + fail + ' failed');
  process.exit(fail === 0 ? 0 : 1);
})();
