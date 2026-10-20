const fs = require('fs');
const path = require('path');

function loadDump(p) {
  let s = fs.readFileSync(p, 'utf16le');
  if (s.charCodeAt(0) === 0xFEFF) s = s.slice(1);
  return JSON.parse(s);
}

function summarize(p, outPath) {
  const j = loadDump(p);
  const nr = j.data && j.data.normalized_result;
  if (!nr) { console.error('NO normalized_result in', p); return; }
  const arr = (x) => Array.isArray(x) ? x : [];
  const types = arr(nr.types);
  const exported = arr(nr.exported_types);
  const fields = arr(nr.fields);
  const methods = arr(nr.methods);
  const memberRefs = arr(nr.member_refs);
  const callEdges = arr(nr.call_edges);
  const fieldAccesses = arr(nr.field_accesses);
  const userStrings = nr.user_strings;

  const lines = [];
  lines.push('# DUMP INVENTORY: ' + path.basename(p));
  lines.push('artifact: ' + JSON.stringify(nr.artifact));
  lines.push('module: ' + JSON.stringify(nr.module));
  lines.push('metadata: ' + JSON.stringify(nr.metadata));
  lines.push('selection: ' + JSON.stringify(nr.selection));
  lines.push('coverage: ' + JSON.stringify(nr.coverage));
  lines.push('counts: types=' + types.length + ' exported_types=' + exported.length +
    ' fields=' + fields.length + ' methods=' + methods.length +
    ' member_refs=' + memberRefs.length + ' call_edges=' + callEdges.length +
    ' field_accesses=' + fieldAccesses.length);
  lines.push('');
  lines.push('## TYPES (' + types.length + ')');
  for (const t of types) {
    const fl = t.field_list || {};
    const ml = t.method_list || {};
    lines.push(
      '[' + (t.token || '?') + '] ' + (t.full_name || t.name || '?') +
      ' flags=' + (t.flags != null ? t.flags : '?') +
      ' extends=' + (t.extends_token || '-') +
      ' fields=' + (fl.count != null ? fl.count : '-') +
      ' methods=' + (ml.count != null ? ml.count : '-') +
      (t.namespace ? ' ns=' + t.namespace : '')
    );
  }
  lines.push('');
  lines.push('## EXPORTED_TYPES (' + exported.length + ')');
  for (const t of exported) lines.push(JSON.stringify(t));
  lines.push('');
  if (userStrings) {
    const us = Array.isArray(userStrings) ? userStrings : (userStrings.items || userStrings.values || null);
    lines.push('## USER_STRINGS (' + (Array.isArray(userStrings) ? userStrings.length : 'object') + ')');
    if (Array.isArray(us)) {
      for (const u of us) lines.push(typeof u === 'string' ? u : JSON.stringify(u));
    } else {
      lines.push(JSON.stringify(userStrings, null, 1));
    }
    lines.push('');
  }
  fs.writeFileSync(outPath, lines.join('\n'), 'utf8');
  console.log('WROTE', outPath, 'bytes=' + fs.statSync(outPath).size);
}

const T = process.env.TEMP;
summarize(path.join(T, 'firstpass-full.json'), path.join(T, 'inv-firstpass.txt'));
summarize(path.join(T, 'asm-csharp-full.json'), path.join(T, 'inv-asm-csharp.txt'));
summarize(path.join(T, 'fishnet-full.json'), path.join(T, 'inv-fishnet.txt'));
