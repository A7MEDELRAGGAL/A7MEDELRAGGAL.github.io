'use strict';

const fs = require('fs');

const bundlePath = process.argv[2];
const bundle = JSON.parse(fs.readFileSync(bundlePath, 'utf8'));

let fields = [];
let methods = [];
let strings = [];

for (const r of (bundle.records || [])) {
  const nr = r.normalized_result || r;
  if (!nr || !nr.page || !nr.page.rows) continue;
  for (const row of nr.page.rows) {
    if (nr.page.collection === 'fields') fields.push(row);
    else if (nr.page.collection === 'methods') methods.push(row);
    else if (nr.page.collection === 'user_strings') strings.push(row);
  }
}

console.log('rows: fields=' + fields.length + ' methods=' + methods.length + ' strings=' + strings.length);

// how to fish is the larger artifact; use its types as the universe
const htfTypes = new Set();
for (const m of methods) if (m.declaring_type) htfTypes.add(m.declaring_type);
for (const f of fields) if (f.declaring_type) htfTypes.add(f.declaring_type);

const SEARCHY = /^(search|filter|find|query|match|lookup|contains|startswith|endswith|indexof|sort|orderby|groupby|select|where)/i;

console.log('');
console.log('== method names that look like search/filter/query ==');
const hits = new Map();
for (const m of methods) {
  const n = m.name || '';
  if (!SEARCHY.test(n)) continue;
  if (!hits.has(n)) hits.set(n, []);
  hits.get(n).push(m.declaring_type);
}
for (const [name, owners] of [...hits.entries()].sort((a, b) => b[1].length - a[1].length)) {
  console.log('  ' + name.padEnd(22) + ' x' + String(owners.length).padStart(3) + '  ' +
    [...new Set(owners)].slice(0, 4).join(', '));
}

console.log('');
console.log('== owners of the most search-like methods ==');
const KEY = ['Search', 'Filter', 'Contains', 'Find', 'Query', 'Match', 'IndexOf', 'Sort'];
for (const m of methods) {
  if (!KEY.includes(m.name)) continue;
  console.log('  ' + m.declaring_type.padEnd(26) + m.name);
}

const wantType = process.argv[3];
if (wantType) {
  console.log('');
  console.log('== ' + wantType + ' methods ==');
  for (const m of methods) {
    if (m.declaring_type !== wantType) continue;
    const s = m.signature || {};
    const ps = (s.parameter_types || []).map((p) => p.kind || p).join(', ');
    console.log('  ' + (s.return_type || 'void') + ' ' + m.name + '(' + ps + ')');
  }
  console.log('');
  console.log('== ' + wantType + ' fields ==');
  for (const f of fields) {
    if (f.declaring_type !== wantType) continue;
    console.log('  ' + f.name + ' : ' + ((f.signature && f.signature.field_type) || '?'));
  }
}
