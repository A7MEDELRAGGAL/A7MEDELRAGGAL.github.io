const fs = require('fs');
const path = require('path');

function loadDump(p) {
  let s = fs.readFileSync(p, 'utf16le');
  if (s.charCodeAt(0) === 0xFEFF) s = s.slice(1);
  return JSON.parse(s);
}

const dumps = [
  ['Assembly-CSharp', path.join(process.env.TEMP, 'asm-csharp-full.json')],
  ['Assembly-CSharp-firstpass', path.join(process.env.TEMP, 'firstpass-full.json')],
  ['FishNet.Runtime', path.join(process.env.TEMP, 'fishnet-full.json')],
];

const out = [];
for (const [label, p] of dumps) {
  const j = loadDump(p);
  const nr = j.data.normalized_result;
  const us = nr.user_strings || {};
  const lits = Array.isArray(us.literals) ? us.literals : [];
  const issues = Array.isArray(us.issues) ? us.issues : [];
  out.push(`# ${label}`);
  out.push(`state=${us.state} literals=${lits.length} issues=${issues.length}`);
  out.push(`## LITERALS`);
  for (const l of lits) {
    const v = (l.value === undefined || l.value === null) ? '' : String(l.value);
    out.push(`[${l.token}] @${l.offset}: ${v.replace(/\r?\n/g, '\\n')}`);
  }
  out.push(`## ISSUES`);
  for (const i of issues) {
    out.push(`${i.code} scope=${i.scope} @${i.offset}: ${String(i.detail || '').replace(/\r?\n/g, ' ')}`);
  }
  out.push('');
}

const outPath = path.join(process.env.TEMP, 'strings-all.txt');
fs.writeFileSync(outPath, out.join('\n'), 'utf8');
console.log('WROTE', outPath, fs.statSync(outPath).size);
