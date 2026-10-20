# REA digest — Muck Assembly-CSharp (reference for the How to Fish track)

Artifact sha256: 5f9d0dc1e0e72f5493013c92d05ed9fb8628019f54f8bc02162dad0d2f20810c
Module MVID: 8db17789-d6d5-48de-86ab-e696a5ff6b2b

Extracted from the REA managed-member evidence. Structure, names and tunables are
used as a mechanics reference; all art, text and code in this project is original.

## rea-htf-journal-mechanics.md — not present in this artifact

## SavedCreature  (3 fields, 1 methods)

### fields
- `ID` : u1  [flags 6]
- `BeenKilled` : bool  [flags 6]
- `KilledDrip` : bool  [flags 6]

### methods
- `void .ctor()`

## LocalSaveObject  (24 fields, 1 methods)

### fields
- `LocalSkinColor` : valuetype:9  [flags 6]
- `LocalHatColor` : valuetype:9  [flags 6]
- `LocalHatColor2` : valuetype:9  [flags 6]
- `LocalHatColor3` : valuetype:9  [flags 6]
- `LocalOutfitColor` : valuetype:9  [flags 6]
- `LocalOutfitColor2` : valuetype:9  [flags 6]
- `LocalOutfitColor3` : valuetype:9  [flags 6]
- `LocalAccessoryColor` : valuetype:9  [flags 6]
- `LocalAccessoryColor2` : valuetype:9  [flags 6]
- `LocalAccessoryColor3` : valuetype:9  [flags 6]
- `LocalHatIndex` : u1  [flags 6]
- `LocalOutfitIndex` : u1  [flags 6]
- `LocalAccessoryIndex` : u1  [flags 6]
- `IsBean` : bool  [flags 6]
- `InvertXInput` : bool  [flags 6]
- `InvertYInput` : bool  [flags 6]
- `UnlockedItemSkins` : class:165<class:1248>  [flags 6]
- `UnlockedBoatSkins` : class:165<u1>  [flags 6]
- `CreaturesToList` : class:165<class:1244>  [flags 6]
- `Creatures` : class:129<u1,class:1244>  [flags 6]
- `SeenHats` : bool[]  [flags 6]
- `SeenOutfits` : bool[]  [flags 6]
- `SeenAccesories` : bool[]  [flags 6]
- `SavedLanguage` : i4  [flags 6]

### methods
- `void .ctor()`

## ThinkingUI  (19 fields, 16 methods)

### fields
- `_thinkingCanvas` : class:497  [flags 1]
- `_progressText` : class:613  [flags 1]
- `_dripProgressText` : class:613  [flags 1]
- `_thinkingHolderGroup` : class:497  [flags 1]
- `_journalCamera` : class:369  [flags 1]
- `_slots` : class:165<class:864>  [flags 1]
- `_slotAnimDelay` : r4  [flags 1]
- `_closeEvent` : class:1257  [flags 1]
- `_closeInput` : class:493  [flags 1]
- `_cloudlyTransforms` : class:165<class:53>  [flags 1]
- `_cloudyCurveX` : class:329  [flags 1]
- `_cloudyCurveY` : class:329  [flags 1]
- `_animSpeed` : r4  [flags 1]
- `_offsetStrength` : r4  [flags 1]
- `_animStrength` : r4  [flags 1]
- `_curPage` : i4  [flags 1]
- `_animTime` : r4  [flags 1]
- `_thinkingAlpha` : r4  [flags 1]
- `_animID` : u1  [flags 1]

### methods
- `void Awake()`
- `void InitializeLocal()`
- `void Start()`
- `void OnDestroy()`
- `void OnControlsChanged(class:829)`
- `void SetupCloseInputText()`
- `void Update()`
- `void CloudyAnimations()`
- `class:201 UpdateJournal()`
- `void UpdateJournalCamera()`
- `void ToggleThinking()`
- `void SetProgressTexts()`
- `void SetThinkingAlpha(r4)`
- `void OnThinkingComplete()`
- `void ScrollThinkingPage(i4)`
- `void .ctor()`

## JournalSlot  (6 fields, 3 methods)

### fields
- `_nameText` : class:613  [flags 1]
- `_worthText` : class:613  [flags 1]
- `_filter` : class:253  [flags 4]
- `_slotToScale` : class:25  [flags 1]
- `_image` : class:1185  [flags 1]
- `_dripText` : class:1284  [flags 1]

### methods
- `void SetCreature(class:184)`
- `void ShowSlot()`
- `void .ctor()`

## NPCQuest  (12 fields, 14 methods)

### fields
- `_questItems` : class:165<class:232>  [flags 1]
- `_onlyCreatures` : bool  [flags 1]
- `_totalItems` : u1  [flags 1]
- `_interactableMesh` : class:257  [flags 1]
- `_baitToReceive` : class:152  [flags 1]
- `_type` : valuetype:904  [flags 1]
- `_islandToUnlock` : u1  [flags 1]
- `_linesLocalized` : class:105[]  [flags 1]
- `_onItemReceivedLinesLocalized` : class:105[]  [flags 1]
- `_onQuestCompletedLinesLocalized` : class:105[]  [flags 1]
- `_holdingItemLinesLocalized` : class:105[]  [flags 1]
- `_alreadyCompletedLinesLocalized` : class:105[]  [flags 1]

### methods
- `class:105[] get_Lines()`
- `class:105[] get_OnItemReceivedLines()`
- `class:105[] get_OnQuestCompletedLines()`
- `class:105[] get_HoldingItemLines()`
- `class:105[] get_AlreadyCompletedLines()`
- `u1 get_IslandToUnlock()`
- `class:165<class:232> get_QuestItems()`
- `bool get_OnlyCreatures()`
- `u1 get_TotalItems()`
- `class:257 get_InteractableMesh()`
- `class:152 get_BaitToReceive()`
- `valuetype:904 get_Type()`
- `bool HasItem(class:232)`
- `void .ctor()`
