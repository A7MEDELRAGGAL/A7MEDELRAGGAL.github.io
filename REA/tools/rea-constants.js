'use strict';

const fs = require('fs');
const path = require('path');

const bundlePath = process.argv[2];
const outFile = process.argv[3];
const types = process.argv.slice(4);

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
for (const f of fields) fieldByToken.set(f.token, f.name + ' : ' + ((f.signature && f.signature.field_type) || '?'));

const methodByToken = new Map();
for (const m of methods) methodByToken.set(m.token, m.declaring_type + '.' + m.name);

function resolve(op) {
  if (!op) return '';
  if (fieldByToken.has(op)) return fieldByToken.get(op);
  if (methodByToken.has(op)) return methodByToken.get(op);
  return op;
}

function numOf(op) {
  if (op === undefined || op === null) return null;
  const s = String(op);
  if (/^-?\d+(\.\d+)?$/.test(s)) return parseFloat(s);
  return null;
}

const lines = [];
lines.push('# REA constant + logic extract');
lines.push('');
lines.push('Source: Muck Assembly-CSharp.dll (sha256 5f9d0dc1e0e72f5493013c92d05ed9fb8628019f54f8bc02162dad0d2f20810c)');
lines.push('');
lines.push('Field reads/writes and float/int literals recovered from decoded CIL anchors.');
lines.push('Used as a mechanics reference only: every implementation in this project is written');
lines.push('from scratch, with original art, text and code.');
lines.push('');

let constantCount = 0;

for (const ty of types) {
  const tm = methods.filter((m) => m.declaring_type === ty);
  if (!tm.length) {
    lines.push('## ' + ty + ' — no methods found');
    lines.push('');
    continue;
  }

  lines.push('## ' + ty);
  lines.push('');

  for (const m of tm) {
    const body = m.body || {};
    const anchors = body.anchors || [];
    if (!anchors.length) continue;

    const consts = [];
    const flow = [];

    for (const a of anchors) {
      const op = a.opcode || '';
      const n = numOf(a.operand);

      if (op === 'ldc.r4' || op === 'ldc.r8' || op === 'ldc.i4' || op === 'ldc.i8') {
        consts.push(n);
        flow.push(op + ' ' + n);
        constantCount++;
        continue;
      }

      if (op === 'ldfld' || op === 'stfld' || op === 'ldsfld' || op === 'stsfld') {
        flow.push(op + ' ' + resolve(a.operand));
        continue;
      }

      if (op === 'call' || op === 'callvirt' || op === 'newobj') {
        flow.push(op + ' ' + resolve(a.operand));
        continue;
      }

      if (op === 'ldstr') {
        flow.push('ldstr "' + String(a.operand) + '"');
        continue;
      }

      flow.push(op);
    }

    const interesting = consts.length > 0 || flow.some((f) => /ldfld|stfld/.test(f));
    if (!interesting) continue;

    lines.push('### ' + m.name + (consts.length ? '   constants: ' + consts.join(', ') : ''));
    lines.push('```');
    for (const f of flow) lines.push('  ' + f);
    lines.push('```');
    lines.push('');
  }
}

fs.writeFileSync(outFile, lines.join('\n'), 'utf8');
console.log('extract: wrote', outFile, (Buffer.byteLength(lines.join('\n')) / 1024).toFixed(1) + ' KB,',
  constantCount, 'literals recovered');
