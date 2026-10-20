'use strict';

const fs = require('fs');

const file = process.argv[2];
const pattern = process.argv[3] || '(note|search|journal|quest|query|filter|index)';
const mode = process.argv[4] || 'types';

const text = fs.readFileSync(file, 'utf8');
const re = new RegExp(pattern, 'i');

if (mode === 'types') {
  const names = new Set();
  for (const m of text.matchAll(/"name":"([^"]+)"/g)) names.add(m[1]);
  for (const m of text.matchAll(/"full_name":"([^"]+)"/g)) names.add(m[1]);
  for (const m of text.matchAll(/"type_name":"([^"]+)"/g)) names.add(m[1]);
  for (const m of text.matchAll(/"declaring_type":"([^"]+)"/g)) names.add(m[1]);

  const all = [...names].sort();
  console.log('distinct names: ' + all.length);
  console.log('');
  console.log('== matches for ' + pattern + ' ==');
  for (const n of all) {
    if (re.test(n)) console.log('  ' + n);
  }
} else {
  const rows = [];
  for (const m of text.matchAll(/\{"token":"0x0[46][0-9a-f]{6}","row_offset":\d+,"declaring_type_token":"0x02[0-9a-f]{6}","declaring_type":"([^"]+)","name":"([^"]+)"([^}]*)\}/g)) {
    rows.push({ type: m[1], name: m[2], rest: m[3] });
  }

  const byType = new Map();
  for (const r of rows) {
    if (!byType.has(r.type)) byType.set(r.type, []);
    byType.get(r.type).push(r);
  }

  console.log('rows: ' + rows.length + '   types: ' + byType.size);
  console.log('');
  console.log('== types matching ' + pattern + ' ==');
  for (const [type, list] of [...byType.entries()].sort()) {
    if (!re.test(type)) continue;
    console.log('');
    console.log('## ' + type + '  (' + list.length + ')');
    for (const r of list) console.log('  - ' + r.name);
  }
}
