# REA digest — Muck Assembly-CSharp (reference for the How to Fish track)

Artifact sha256: 5f9d0dc1e0e72f5493013c92d05ed9fb8628019f54f8bc02162dad0d2f20810c
Module MVID: 8db17789-d6d5-48de-86ab-e696a5ff6b2b

Extracted from the REA managed-member evidence. Structure, names and tunables are
used as a mechanics reference; all art, text and code in this project is original.

## Chest  (6 fields, 8 methods)

### fields
- `cells` : class:708[]  [flags 6]
- `<id>k__BackingField` : i4  [flags 1]
- `chestSize` : i4  [flags 6]
- `inUse` : bool  [flags 6]
- `locked` : bool[]  [flags 6]
- `animator` : class:221  [flags 1]

### methods
- `i4 get_id()`
- `void set_id(i4)`
- `void Start()`
- `void Use(bool)`
- `bool IsUsed()`
- `void UpdateCraftables()`
- `void InitChest(class:113<class:708>, class:904)`
- `void .ctor()`

## ChestManager  (3 fields, 10 methods)

### fields
- `chests` : class:137<i4,class:112>  [flags 6]
- `chestId` : i4  [flags 1]
- `Instance` : class:752  [flags 22]

### methods
- `void Awake()`
- `void AddChest(class:112, i4)`
- `i4 GetNextId()`
- `void UseChest(i4, bool)`
- `void SendChestUpdate(i4, i4)`
- `void UpdateChest(i4, i4, i4, i4)`
- `bool IsChestOpen(i4)`
- `void RemoveChest(i4)`
- `void DropChest(class:112)`
- `void .ctor()`

## ChestUI  (1 fields, 5 methods)

### fields
- `cells` : class:704[]  [flags 6]

### methods
- `void Awake()`
- `void CopyChest(class:112, bool)`
- `class:708[] AddMapToCells(class:708[])`
- `void UpdateCraftables()`
- `void .ctor()`

## ChestInteract  (4 fields, 11 methods)

### fields
- `state` : valuetype:1440  [flags 6]
- `chest` : class:112  [flags 1]
- `cooldownTime` : r4  [flags 1]
- `ready` : bool  [flags 1]

### methods
- `void Awake()`
- `void Interact()`
- `void LocalExecute()`
- `void AllExecute()`
- `void ServerExecute(i4)`
- `void WhenOpened()`
- `void RemoveObject()`
- `string GetName()`
- `bool IsStarted()`
- `void GetReady()`
- `void .ctor()`

## ShipChest  (2 fields, 2 methods)

### fields
- `spawnLoot` : class:708[]  [flags 6]
- `rand` : class:904  [flags 1]

### methods
- `void Start()`
- `void .ctor()`

## StartChest  (2 fields, 2 methods)

### fields
- `loot` : class:740  [flags 6]
- `rand` : class:904  [flags 1]

### methods
- `void Start()`
- `void .ctor()`

## HitableChest  (0 fields, 2 methods)

### fields

### methods
- `void OnKill(valuetype:81)`
- `void .ctor()`

## CraftingUI  (18 fields, 7 methods)

### fields
- `nCells` : i4  [flags 6]
- `cellsParent` : class:237  [flags 6]
- `cellsParentParent` : class:237  [flags 6]
- `cellsParentParentParent` : class:237  [flags 6]
- `rect` : valuetype:241  [flags 1]
- `cellPrefab` : class:25  [flags 6]
- `requirementPrefab` : class:25  [flags 6]
- `cells` : class:113<class:704>  [flags 1]
- `tabSelected` : i4  [flags 1]
- `tabs` : class:1396[]  [flags 6]
- `tabParent` : class:29  [flags 6]
- `tabImgs` : class:41[]  [flags 1]
- `tabTexts` : class:45[]  [flags 1]
- `selectedTabColor` : valuetype:201  [flags 6]
- `unselectedTabColor` : valuetype:201  [flags 6]
- `selectedTextColor` : valuetype:201  [flags 6]
- `unselectedTextColor` : valuetype:201  [flags 6]
- `handCrafts` : bool  [flags 6]

### methods
- `void Awake()`
- `void Start()`
- `void OnEnable()`
- `void OpenTab(i4)`
- `void UpdateTabs()`
- `void UpdateCraftables()`
- `void .ctor()`

## CraftingInteract  (1 fields, 8 methods)

### fields
- `state` : valuetype:1440  [flags 6]

### methods
- `void Interact()`
- `void LocalExecute()`
- `void AllExecute()`
- `void ServerExecute(i4)`
- `void RemoveObject()`
- `string GetName()`
- `bool IsStarted()`
- `void .ctor()`

## CraftingState  (8 fields, 0 methods)

### fields
- `value__` : i4  [flags 1542]
- `Inventory` : valuetype:1440  [flags 32854]
- `Workbench` : valuetype:1440  [flags 32854]
- `Anvil` : valuetype:1440  [flags 32854]
- `Cauldron` : valuetype:1440  [flags 32854]
- `Fletch` : valuetype:1440  [flags 32854]
- `Furnace` : valuetype:1440  [flags 32854]
- `Chest` : valuetype:1440  [flags 32854]

### methods

## CraftRequirement  (2 fields, 1 methods)

### fields
- `item` : class:708  [flags 6]
- `amount` : i4  [flags 6]

### methods
- `void .ctor()`

## SaveManager  (2 fields, 9 methods)

### fields
- `state` : class:948  [flags 6]
- `<Instance>k__BackingField` : class:952  [flags 17]

### methods
- `class:952 get_Instance()`
- `void set_Instance(class:952)`
- `void Awake()`
- `void Save()`
- `void Load()`
- `void NewSave()`
- `string Serialize(mvar:0)`
- `mvar:0 Deserialize(string)`
- `void .ctor()`
