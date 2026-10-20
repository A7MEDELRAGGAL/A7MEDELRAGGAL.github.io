# Tidebreak — browser build

An original open-water fishing sandbox that runs in a browser. Same gameplay loop as the
Unity project (walk, swim, board a boat, cast, hook, reel, land, eat, survive), original
code, original art primitives, original text.

## Run it

Open `index.html` in a modern browser. No build step, no install.

If your browser blocks the module/CORS rules for `file://` (rare, but Safari sometimes does),
serve the folder instead:

```
python -m http.server 8080
```

Then open `http://localhost:8080/`.

three.js r128 is vendored in `vendor/three.min.js` and loaded as a classic script, so the game
works fully offline.

## Build

```
npm run build      # -> dist/tidebreak.html  (one self-contained offline file)
npm test           # headless game logic: api contract + 3000+ simulation steps
npm run verify     # test, build, then boot the built artifact and simulate 300 frames
```

`npm run verify` is the gate: it fails if a module is missing, if the bundle still points at
a CDN, if any bundled script stops parsing, or if the shipped file throws while running.

Output: `dist/tidebreak.html` (single file, three.js inlined), `dist/build-manifest.json`
with per-module byte counts, and a copy of this readme.

## Controls

| Key | Action |
|---|---|
| `W A S D` | move / drive |
| `Mouse` | look (drag if pointer lock is unavailable) |
| `Space` | jump, swim up |
| `Shift` | sprint, boat boost |
| `C` | crouch, swim down |
| `F` | board / leave the boat |
| `T` | talk to the dockhand (stand next to the pier) |
| `J` | open the journal and search your records |
| `Left mouse` | cast and strike, swing a melee tool, eat what is held |
| `1`–`9`, `Wheel` | select a slot |
| `B` | set the selected stack as bait |
| `G` | drop one |
| `R` | consume |
| `M` | chart a new island chain |

## The journal

Press `J`. Every fish you land and every creature you hunt is written to your journal, with the
name, the count, what it is worth and a short note about how it went down.

The search box filters the list as you type. It matches the name, the note, the id or the count,
and it is case insensitive. `prev`/`next` page through the results.

The store keeps a record per species, keyed by a small numeric id, and a lookup returns nothing
rather than inventing a record when a species has not been seen yet. That behaviour was recovered
from the original game's journal and is written up in `artifacts/REA-JOURNAL-SEARCH.md`.

## The dockhand

Stand on the pier and press `T`. The dockhand is an in-world character that answers through a
language model.

**No API key ships with the game.** Paste your own key into the key field in the chat panel;
it is stored in that browser's `localStorage` and never written into the build or the
repository. `npm run verify` fails the build if a key ever appears in the artifact.

If you prefer a file, create `config.local.js` (git-ignored) next to `index.html`:

```js
window.G.AI_CONFIG = { base: 'https://api.beatapi.io', key: 'sk-...' };
```

and load it before `js/ai.js`.

## What is implemented

- Procedural ocean: GPU wave surface with matching CPU sampling so buoyancy, wading and
  bobbing all agree with what you see.
- Procedural islands generated from fBm noise with sand, grass, rock and snow banding.
- Character controller with ground check, slope handling, crouch, sprint, jump, swimming,
  and a cold-water shock on a deep dive.
- Motorboat with thrust, reverse, boost, rudder authority that scales with speed, drag that
  changes when it beaches, and a wake ring.
- Fishing loop: cast, wait, bite, strike window, catch, reel-in, plus bait consumption.
- Creatures that wander, flee when struck, die, and leave loot you can walk over and collect.
- Vitals: health, fullness, poison, fire, i-frames, death and automatic return to the beach.
- Inventory: 24 slots, 9-slot hotbar, per-item stack caps, a carry-weight ceiling, bait slot.

## Layout

```
index.html        markup, HUD overlay, script order
css/hud.css       HUD styling
js/core.js        namespace, math, seeded noise, input, frame loop
js/world.js       wave field, ocean shader, terrain, sky, dock
js/catalog.js     item definitions
js/vitals.js      health / fullness / poison / fire
js/inventory.js   slots, stacking, weight, bait
js/creatures.js   fish AI, floaters, pickup
js/player.js      character controller and boat boarding
js/boat.js        boat physics and driving
js/fishing.js     rod state machine
js/hud.js         DOM binding
js/game.js        wiring and main step
```

## Notes

- Item ids and names mirror the Unity `CloneItemCatalog` so both implementations stay aligned.
- Nothing here is copied from any shipped game: the art is procedural primitives, the text is
  original, and the code is written from scratch.
- `window.Tidebreak` exposes the live scene, player, boat, fishing and inventory for poking
  around in the console.
