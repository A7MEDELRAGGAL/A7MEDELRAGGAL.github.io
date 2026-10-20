'use strict';

const fs = require('fs');
const path = require('path');

const bundlePath = process.argv[2];
const outDir = process.argv[3] || path.join(__dirname, '..', 'artifacts');
const outName = process.argv[4] || 'rea-muck-mechanics.md';

const raw = fs.readFileSync(bundlePath, 'utf8');
let bundle;
try {
  bundle = JSON.parse(raw);
} catch (e) {
  console.error('digest: bundle is not valid json:', e.message);
  process.exit(1);
}

const records = bundle.records || bundle.results || [];
console.log('digest: records in bundle =', records.length);

const normalized = [];
for (const r of records) {
  const nr = r.normalized_result || r;
  if (!nr) continue;
  if (nr.kind === 'managed-members' || (nr.page && nr.page.collection)) {
    normalized.push(nr);
  }
}

let fields = [];
let methods = [];
let types = [];
let strings = [];

for (const nr of normalized) {
  const page = nr.page;
  if (!page || !page.rows) continue;
  for (const row of page.rows) {
    if (page.collection === 'fields') fields.push(row);
    else if (page.collection === 'methods') methods.push(row);
    else if (page.collection === 'types') types.push(row);
    else if (page.collection === 'user_strings') strings.push(row);
  }
}

console.log('digest: types=%d fields=%d methods=%d strings=%d', types.length, fields.length, methods.length, strings.length);

const WANT = process.argv.slice(4);
const targets = WANT.length ? WANT : [
  'PlayerMovement', 'Player', 'Boat', 'Mob', 'MobType', 'InventoryItem',
  'PlayerStatus', 'PlayerSave', 'UseInventory', 'InventoryUI', 'DayCycle',
  'ItemManager', 'PlayerInput', 'OtherInput', 'MoveCamera', 'Settings'
];

function sigOf(m) {
  const s = m.signature || {};
  const ret = s.return_type || '';
  const ps = (s.parameter_types || []).map((p) => p.kind || p).join(', ');
  return ret + ' ' + (m.name || '') + '(' + ps + ')';
}

const lines = [];
lines.push('# REA digest — Muck Assembly-CSharp (reference for the How to Fish track)');
lines.push('');
lines.push('Artifact sha256: 5f9d0dc1e0e72f5493013c92d05ed9fb8628019f54f8bc02162dad0d2f20810c');
lines.push('Module MVID: 8db17789-d6d5-48de-86ab-e696a5ff6b2b');
lines.push('');
lines.push('Extracted from the REA managed-member evidence. Structure, names and tunables are');
lines.push('used as a mechanics reference; all art, text and code in this project is original.');
lines.push('');

for (const ty of targets) {
  const tf = fields.filter((f) => f.declaring_type === ty);
  const tm = methods.filter((m) => m.declaring_type === ty);
  if (!tf.length && !tm.length) {
    lines.push('## ' + ty + ' — not present in this artifact');
    lines.push('');
    continue;
  }

  lines.push('## ' + ty + '  (' + tf.length + ' fields, ' + tm.length + ' methods)');
  lines.push('');
  lines.push('### fields');
  for (const f of tf) {
    const t = (f.signature && f.signature.field_type) || '?';
    const flags = f.flags === undefined ? '' : '  [flags ' + f.flags + ']';
    lines.push('- `' + f.name + '` : ' + t + flags);
  }
  lines.push('');
  lines.push('### methods');
  for (const m of tm) {
    lines.push('- `' + sigOf(m) + '`');
  }
  lines.push('');
}

const outFile = path.join(outDir, outName);
fs.writeFileSync(outFile, lines.join('\n'), 'utf8');
console.log('digest: wrote', outFile, (Buffer.byteLength(lines.join('\n')) / 1024).toFixed(1) + ' KB');
