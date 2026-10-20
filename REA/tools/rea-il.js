'use strict';

const fs = require('fs');

const bundlePath = process.argv[2];
const typeName = process.argv[3];
const methodFilter = process.argv[4] || '';

const bundle = JSON.parse(fs.readFileSync(bundlePath, 'utf8'));

let fields = [];
let methods = [];

for (const r of (bundle.records || [])) {
  const nr = r.normalized_result || r;
  if (!nr || !nr.page || !nr.page.rows) continue;
  for (const row of nr.page.rows) {
    if (nr.page.collection === 'fields') fields.push(row);
    else if (nr.page.collection === 'methods') methods.push(row);
  }
}

const fieldByToken = new Map();
for (const f of fields) {
  fieldByToken.set(f.token, f.name);
  const owner = f.declaring_type;
  fieldByToken.set(f.token, owner + '.' + f.name);
}

const methodByToken = new Map();
for (const m of methods) methodByToken.set(m.token, m.declaring_type + '.' + m.name);

function resolve(op) {
  if (fieldByToken.has(op)) return fieldByToken.get(op);
  if (methodByToken.has(op)) return methodByToken.get(op);
  return op;
}

const picked = methods.filter((m) => m.declaring_type === typeName &&
  (!methodFilter || (m.name || '').toLowerCase().includes(methodFilter.toLowerCase())));

if (!picked.length) {
  console.log('no method matched ' + typeName + '.' + methodFilter);
  process.exit(0);
}

for (const m of picked) {
  const body = m.body || {};
  const anchors = body.anchors || [];
  const s = m.signature || {};
  const ps = (s.parameter_types || []).map((p) => p.kind || p).join(', ');

  console.log('=== ' + typeName + '.' + m.name + '(' + ps + ') -> ' + (s.return_type || 'void') +
    '  [' + (body.decoded_instruction_count || 0) + ' instr, il_size ' + (body.il_size || 0) +
    ', rva ' + (body.rva || 0) + '] ===');

  if (!anchors.length) {
    console.log('  (no decoded anchors; status=' + (body.status || '?') + ')');
    console.log('');
    continue;
  }

  for (const a of anchors) {
    const op = a.opcode || '';
    let text = op;
    if (op === 'ldfld' || op === 'stfld' || op === 'ldsfld' || op === 'stsfld') text += ' ' + resolve(a.operand);
    else if (op === 'call' || op === 'callvirt' || op === 'newobj') text += ' ' + resolve(a.operand);
    else if (op === 'ldstr') text += ' "' + String(a.operand) + '"';
    else if (a.operand !== undefined && a.operand !== null) text += ' ' + a.operand;
    console.log('  ' + String(a.il_offset).padStart(4) + '  ' + text);
  }

  console.log('');
}
