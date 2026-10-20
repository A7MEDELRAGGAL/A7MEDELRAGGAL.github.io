# How the original's journal "search" works — and how we rebuild it

Artifact: `artifacts/HowToFish-Assembly-CSharp.dll`
sha256 `0491c7b5286ca37b42d506113a9c7e32e0ad8d9d121c5fe3be8e67ce9e9d036b`
Module MVID `48815c0b-02c6-4024-a20a-1cb6cbfee2ec`
Extracted with REA (`inspect_managed_artifact`, `inspect_managed_members`) and reduced locally by
`tools/rea-il.js`, `tools/rea-digest.js`, `tools/rea-constants.js`.

Everything below is our own reimplementation. No method body, asset or string is copied.

## 1. The shape of the feature

The original has no free-text search. Its "notes" surface is a **journal of caught creatures**,
and the "search" is the **record lookup** that fills each journal slot.

| Type | Fields | Role |
|---|---|---|
| `LocalSaveObject` | 24 | the per-player save: cosmetics, unlocked skins, `CreaturesToList` (list), `Creatures` (dictionary keyed by `uint`), `SeenHats/SeenOutfits/SeenAccesories` (`bool[]` discovery flags), `SavedLanguage` |
| `SavedCreature` | 3 | `ID : uint`, `BeenKilled : bool`, `KilledDrip : bool` |
| `JournalSlot` | 6 | `_nameText`, `_worthText`, `_dripText`, `_image`, `_filter`, `_slotToScale` + `SetCreature(Creature)`, `ShowSlot()` |
| `ThinkingUI` | 19 | `_slots : JournalSlot[]`, `_curPage : int`, `_journalCamera`, `_slotAnimDelay`, `_animSpeed`, `_thinkingAlpha`, `_animID`, `_cloudlyTransforms/_cloudyCurveX/_cloudyCurveY`, `_progressText`, `_dripProgressText` |
| `SaveManager` | — | owns the local save; `GetSavedCreature(Item)` is the lookup |

UI methods: `UpdateJournal` (coroutine, body in `<UpdateJournal>d__27.MoveNext`),
`UpdateJournalCamera`, `ScrollThinkingPage`, `SetProgressTexts`, `ToggleThinking`,
`OnThinkingComplete`, `CloudyAnimations`.

## 2. The search itself (evidence)

`SaveManager.GetSavedCreature(Item)` — 20 instructions, il_size 64, rva 333248:

```
 0  ldsfld  SaveManager._curLocalSave
 5  brtrue.s 19
 7  ldstr   <error text>            // no local save at all
12  call     <log error>
17  br.s    46                       // -> return null
19  ldsfld  SaveManager._curLocalSave
24  ldfld   LocalSaveObject.Creatures
30  callvirt Item.get_ID             // the key
35  ldloca.s 0                       // predicate closure
37  callvirt List<SavedCreature>.Find
42  brfalse.s 46                      // no match -> return null
46  newobj  SavedCreature..ctor
53  callvirt Item.get_ID
58  stfld   SavedCreature.ID
```

Three properties fall out of it:

1. **Keyed by a compact id.** The record key is the item's `ID` (a `uint`), not a name.
2. **Linear scan with a predicate.** `List<T>.Find(c => c.ID == item.ID)` — a linear scan over
   the player's own record list, not a hash lookup and not a fuzzy match.
3. **Null-safe, two different nulls.** No local save at all → log an error and return `null`.
   Save exists but no record for that id → also `null`, and the *caller* creates a default
   record. The lookup never invents state.

## 3. How a journal entry is filled

`JournalSlot.SetCreature(Creature)` — 122 instructions, il_size 399, rva 230796:

```
  1  call      <is local player?>
  6  brfalse   305
 12  call      SaveManager.GetSavedCreature     // the lookup
 19  ldfld     DamageResult.sniped
 25  ldfld     SavedCreature.BeenKilled         // did we kill it?
 30  brtrue.s  39
 32  call      <set drip: not killed>
 39  call      <set drip: killed>
 44  callvirt  Creature.get_BossType
 55  brtrue.s  65
 58  ldfld     SavedCreature.KilledDrip
 68  ldfld     LootItems.dropChance
 74  ldfld     SavedCreature.BeenKilled
 81  ldstr     <"not killed" text>
 89  callvirt  Item.GetName                    // -> _nameText
 94  callvirt  <set name text>
100  ldfld     DamageResult.damageMultiplier
106  ldfld     SavedCreature.BeenKilled
114  callvirt  Creature.get_BossType
129  ldstr     <boss worth text>
135  callvirt  Item.get_DefaultWorth            // -> _worthText
186  callvirt  DripText.SetDrip                 // -> _dripText
```

So a slot is **name + worth + a "drip" line describing how it went down**, all resolved through
the same per-player record. `ShowSlot()` then reveals the slot with a scale animation, staggered
by `_slotAnimDelay`, while `ThinkingUI` pages through them with `_curPage`.

## 4. What we build, and how it maps

Same three-layer shape, our own code and wording:

| Original | Ours | Why it is the same idea |
|---|---|---|
| `LocalSaveObject.Creatures` | `CloneJournal._entries` (Unity) / `G.Journal.entries` (web) | per-player record store |
| `SavedCreature { ID, BeenKilled, KilledDrip }` | `CloneJournalEntry { Id, Discovered, Defeated, Count, Worth, Note }` | compact key + flags + player-specific note |
| `SaveManager.GetSavedCreature` | `CloneJournal.TryFind(id)` / `G.Journal.find(id)` | linear scan by key, null when absent |
| `JournalSlot.SetCreature` | `CloneJournal.describe(entry)` / `G.Journal.describe(entry)` | one record -> one display line (name, worth, note) |
| `JournalSlot.ShowSlot` | `CloneJournalUI` slot reveal | staggered reveal of the grid |
| `ThinkingUI._curPage` + `ScrollThinkingPage` | `page` + `scrollPage()` | paging over the entry list |
| — (the original has none) | `CloneJournal.Search(query)` / `G.Journal.search(query)` | we add the text search the original lacks: a case-insensitive substring match over name and note, capped by a limit |

Rules we keep on purpose, because they are the behaviour that makes the original's journal
trustworthy:

- A lookup never invents state. No record -> `null`, and the caller decides what to create.
- A missing save is an error path, not a silent empty list.
- The key is the item id, never a display name, so renames never break the record.
- The journal is a **projection** of the store, never the source of truth.
