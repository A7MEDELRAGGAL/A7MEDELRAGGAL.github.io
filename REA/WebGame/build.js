'use strict';

const fs = require('fs');
const path = require('path');

const ROOT = __dirname;
const DIST = path.join(ROOT, 'dist');

const ORDER = [
  'core.js',
  'world.js',
  'catalog.js',
  'vitals.js',
  'inventory.js',
  'creatures.js',
  'player.js',
  'boat.js',
  'fishing.js',
  'bait.js',
  'journal.js',
  'ai.js',
  'hud.js',
  'game.js'
];

function read(rel) {
  return fs.readFileSync(path.join(ROOT, rel), 'utf8');
}

function squeeze(code) {
  return code
    .split('\n')
    .map(line => line.replace(/^\s+/, '').replace(/\s+$/, ''))
    .filter(line => line.length > 0 && !line.startsWith('//'))
    .join('\n');
}

function guard(label, rel) {
  const full = path.join(ROOT, rel);
  if (!fs.existsSync(full)) {
    console.error('build: missing ' + rel + ' (' + label + ')');
    process.exit(1);
  }
  return full;
}

function build() {
  const vendorPath = guard('renderer', 'vendor/three.min.js');
  const htmlPath = guard('shell', 'index.html');
  const cssPath = guard('styles', 'css/hud.css');

  const vendor = fs.readFileSync(vendorPath, 'utf8');
  const css = squeeze(fs.readFileSync(cssPath, 'utf8'));
  let html = fs.readFileSync(htmlPath, 'utf8');

  const scripts = [];
  for (const name of ORDER) {
    guard('script', path.join('js', name));
    scripts.push({ name, code: squeeze(read(path.join('js', name))) });
  }

  html = html.replace(
    /<link rel="stylesheet" href="css\/hud\.css" \/>/,
    '<style>\n' + css + '\n</style>'
  );

  html = html.replace(
    /<script src="https:\/\/cdn\.jsdelivr\.net\/npm\/three@0\.128\.0\/build\/three\.min\.js"><\/script>/,
    '<script>\n' + vendor + '\n</script>'
  );

  for (const script of scripts) {
    html = html.replace(
      new RegExp('<script src="js/' + script.name.replace('.', '\\.') + '"><\\/script>'),
      '<script>\n/* ' + script.name + ' */\n' + script.code + '\n</script>'
    );
  }

  const leftover = html.match(/<script src="[^"]+"><\/script>/g);
  if (leftover) {
    console.error('build: unreplaced script tags -> ' + leftover.join(', '));
    process.exit(1);
  }

  const stamp = new Date().toISOString();
  html = html.replace('<title>Tidebreak</title>',
    '<title>Tidebreak</title>\n<!-- build ' + stamp + ' -->');

  fs.mkdirSync(DIST, { recursive: true });
  const outFile = path.join(DIST, 'tidebreak.html');
  fs.writeFileSync(outFile, html, 'utf8');

  const manifest = {
    name: 'Tidebreak',
    built: stamp,
    entry: 'dist/tidebreak.html',
    bytes: Buffer.byteLength(html, 'utf8'),
    modules: [
      { file: 'vendor/three.min.js', bytes: Buffer.byteLength(vendor, 'utf8') },
      { file: 'css/hud.css', bytes: Buffer.byteLength(css, 'utf8') }
    ].concat(scripts.map(s => ({ file: 'js/' + s.name, bytes: Buffer.byteLength(s.code, 'utf8') })))
  };

  fs.writeFileSync(path.join(DIST, 'build-manifest.json'), JSON.stringify(manifest, null, 2), 'utf8');
  fs.copyFileSync(path.join(ROOT, 'README.md'), path.join(DIST, 'README.md'));

  console.log('build: dist/tidebreak.html  ' + (manifest.bytes / 1024).toFixed(1) + ' KB');
  console.log('build: ' + (scripts.length + 2) + ' modules inlined');
  for (const m of manifest.modules) {
    console.log('  ' + m.file.padEnd(24) + (m.bytes / 1024).toFixed(1).padStart(8) + ' KB');
  }
}

build();
