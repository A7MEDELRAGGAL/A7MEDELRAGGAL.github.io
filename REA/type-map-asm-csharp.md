# Type Map: Assembly-CSharp.dll (How to Fish v1.0.10-OFME)

**Artifact:** `...\How to Fish\How to Fish_Data\Managed\Assembly-CSharp.dll`
**SHA-256:** `0491c7b5286ca37b42d506113a9c7e32e0ad8d9d121c5fe3be8e67ce9e9d036b`
**MVID:** `48815c0b-02c6-4024-a20a-1cb6cbfee2ec`
**Metadata:** v4.0.30319, complete

## Inventory Totals

| Metric | Count |
|---|---|
| TypeDef | 400 |
| Field | 4,407 |
| MethodDef | 5,044 |
| Param | 3,148 |
| InterfaceImpl | 144 |
| MemberRef | 2,043 |
| Call edges | 27,674 |
| Field accesses | 16,800 |
| Exported types | 0 |

> Flag glossary (TypeAttributes): `1048577` = normal public class; `1048833`/`1048576` = static class; `257`/`259` = sealed enum-like; `256` = abstract; `1056769`/`1057033`/`1057035` = sealed struct-ish/value-type-bearing; `1048961` = sealed static helper; `1048579` = sealed with layout.

---

## 1. Player & Character Core (first-priority deep dive)

These types implement the local player, movement, eating, holding, inventory, vitals, and UI.

| Type | Token | Extends | Fields | Methods | Role |
|---|---|---|---|---|---|
| Player | `0x020000f2` | NetworkBehaviour (`0x0100000e`) | 50 | 83 | Local player actor; root of player subsystems |
| PlayerMovement | `0x0200010d` | NetworkBehaviour | 73 | 63 | Character locomotion / movement input |
| PlayerHolding | `0x020000ff` | NetworkBehaviour | 44 | 57 | Held item / tool interaction |
| PlayerInventory | `0x02000100` | NetworkBehaviour | 18 | 63 | Player inventory slots & items |
| PlayerVitals | `0x0200012b` | NetworkBehaviour | 53 | 41 | Health / hunger / vitals |
| PlayerUI | `0x02000117` | MonoBehaviour | 19 | 49 | Player HUD / UI |
| PlayerEating | `0x020000fb` | NetworkBehaviour | 21 | 27 | Eating mechanics |
| PlayerMouth | `0x0200010c` | MonoBehaviour | 21 | 18 | Mouth/feeding animation state |
| PlayerLegs | `0x0200010b` | MonoBehaviour | 50 | 23 | Legs animation / IK |
| PlayerBody | `0x020000f6` | MonoBehaviour | 29 | 14 | Body mesh/pose |
| PlayerArms | `0x020000f5` | MonoBehaviour | 2 | 2 | Arms |
| PlayerCamera | `0x020000f7` | NetworkBehaviour | 56 | 58 | Camera control |
| PlayerToolMovement | `0x02000115` | MonoBehaviour | 53 | 42 | Tool swing/movement |
| PlayerPunching | `0x0200010e` | NetworkBehaviour | 32 | 30 | Punch/combat input |
| PlayerDying | `0x020000fa` | NetworkBehaviour | 12 | 25 | Death state |
| PlayerDeathCam | `0x020000f9` | NetworkBehaviour | 27 | 19 | Death camera |
| PlayerUnderwater | `0x02000127` | MonoBehaviour | 15 | 6 | Underwater state |
| PlayerSkin | `0x02000112` | NetworkBehaviour | 23 | 23 | Skin application |
| PlayerThinking | `0x02000114` | MonoBehaviour | 5 | 15 | Thinking/idle behaviour |
| PlayerSkills | `0x02000111` | MonoBehaviour | 25 | 14 | Skills/progression |
| PlayerKillScore | `0x02000107` | MonoBehaviour | 14 | 6 | Kill score tracking |
| PlayerAimAssist | `0x020000f4` | MonoBehaviour | 14 | 14 | Aim assist |
| PlayerColDetector | `0x020000f8` | MonoBehaviour | 1 | 4 | Collision detector |
| OtherPlayer | `0x020000f0` | NetworkBehaviour | 24 | 32 | Remote player representation |
| DeadPlayer | `0x02000030` | Item | 29 | 41 | Player corpse / dead body |
| PlayerSkinType | `0x02000113` | Enum | 11 | 0 | Skin type enum |

**Player async/closure generated:** `<WaitForClientInit>d__135`, `<ScoreAnimation>d__18`, `<ShowNextKillScore>d__16`, `<>c`, `<>c__DisplayClass28_0/36_0/47_0`, `<InitializeInventory>d__36`, `<SpawnStartingItems>d__38`.

### Top player deep-dive targets
1. `Player` (0x020000f2) — player root, NetworkBehaviour subclass
2. `PlayerMovement` (0x0200010d) — locomotion
3. `PlayerInventory` (0x02000100) — inventory system
4. `PlayerHolding` (0x020000ff) — held items/tools
5. `PlayerVitals` (0x0200012b) — health/hunger

---

## 2. Creatures, Fish & Wildlife (second-priority deep dive)

| Type | Token | Extends | Fields | Methods | Role |
|---|---|---|---|---|---|
| Creature | `0x0200002e` | Item (`0x0200003a`) | 39 | 66 | Base creature/wildlife actor |
| Fish | `0x02000034` | Creature | 20 | 14 | Base fish |
| AttackingFish | `0x02000025` | Fish | 14 | 14 | Aggressive fish |
| Piranha | `0x02000045` | AttackingFish | 26 | 22 | Piranha |
| RunningFish | `0x0200004b` | Fish | 17 | 13 | Fleeing fish |
| Pufferfish | `0x02000047` | Fish | 53 | 32 | Pufferfish |
| BowheadWhale | `0x02000028` | AttackingFish | 61 | 38 | Boss whale |
| Bird | `0x02000027` | Creature | 13 | 34 | Base bird |
| Albatross | `0x0200001a` | Bird | 56 | 36 | Albatross |
| Crab | `0x0200002a` | Creature | 20 | 13 | Crab |
| CrabLegs | `0x0200002c` | MonoBehaviour | 36 | 26 | Crab legs |
| CrabArms | `0x0200002b` | MonoBehaviour | 30 | 18 | Crab arms |
| Spidercrab | `0x02000053` | Crab | 44 | 32 | Spider crab |
| Leg | `0x0200002d` | ValueType | 2 | 4 | Leg data |
| NPC | `0x02000010` | MonoBehaviour | 46 | 36 | NPC (quest givers etc.) |
| DeadPlayer | `0x02000030` | Item | 29 | 41 | Corpse |

**Creature generated:** `<EatEffects>d__74`, `<SpeakEffects>d__69`, `<DelayedNoise>d__68`, `<Poop>d__71`, `<ShootLava>d__71`, `<SpawnSummons>d__27`, `<SpawnMutatedWhale>d__12`.

### Enums
- `FishType` (`0x02000006`, 12 fields) — fish species enum
- `MouthState` (`0x02000011`, 5 fields) — mouth state
- `BossType` (`0x0200002f`, 4 fields) — boss type
- `KillType` (`0x020000db`, 4 fields) — kill type
- `CrabState` (`0x02000054`, 4 fields) — crab state

---

## 3. Items, Tools, Weapons & Fishing Gear

| Type | Token | Extends | Fields | Methods | Role |
|---|---|---|---|---|---|
| Item | `0x0200003a` | MonoBehaviour | 101 | 167 | Base item |
| FishingRod | `0x02000037` | Tool | 75 | 63 | Fishing rod |
| Tool | `0x02000055` | Item | 20 | 36 | Base tool |
| Weapon | `0x02000057` | Tool | 40 | 43 | Base weapon |
| Melee | `0x02000043` | Weapon | 42 | 34 | Melee weapon |
| InventorySlot | `0x020000d4` | MonoBehaviour | 7 | 10 | Inventory slot UI/data |
| Bait | `0x020000bf` | MonoBehaviour | 23 | 35 | Bait |
| SlotInfo | `0x02000052` | ScriptableObject | 3 | 4 | Slot info (ScriptableObject) |
| ItemInfoWeight | `0x02000036` | ValueType | 2 | 3 | Item weight info |
| ItemExtraRigidbody | `0x02000041` | MonoBehaviour | 13 | 17 | Item rigidbody |
| FloatInfo | `0x0200003f` | ValueType | 2 | 3 | Float info |
| HeldInfo | `0x02000040` | ValueType | 2 | 4 | Held item info |

**Item generated:** `<>c` (0x0200003b), `<CheckForParentAfterServer>d__270`.

---

## 4. Purchasables, Upgrades & Economy (Shop / Casino)

### Purchasable hierarchy (`Purchasable` base extends Interactable)

| Type | Token | Extends | Fields | Methods | Role |
|---|---|---|---|---|---|
| Purchasable | `0x02000016` | Interactable | 5 | 6 | Base purchasable |
| ItemPurchasable | `0x0200000e` | Purchasable | 2 | 7 | Item purchasable |
| AttachmentPurchasable | `0x02000007` | Purchasable | 3 | 4 | Attachment purchasable |
| BaitPurchasable | `0x02000008` | Purchasable | 2 | 5 | Bait purchasable |
| BulletPurchasable | `0x0200000b` | Purchasable | 3 | 4 | Bullet/ammo purchasable |
| BoatRadarPurchasable | `0x0200000a` | Purchasable | 1 | 5 | Boat radar purchasable |
| MotorPurchasable | `0x0200000f` | Purchasable | 2 | 5 | Motor purchasable |
| SharpnessPurchasable | `0x02000018` | Purchasable | 3 | 4 | Sharpness purchasable |
| SlotPurchasable | `0x02000019` | Purchasable | 2 | 3 | Slot purchasable |

### Attachments (`Attachment` base extends MonoBehaviour)

| Type | Token | Extends | Fields | Methods | Role |
|---|---|---|---|---|---|
| Attachment | `0x0200001d` | MonoBehaviour | 2 | 3 | Base attachment |
| AttachmentInfo | `0x0200001e` | ScriptableObject | 4 | 3 | Attachment data (ScriptableObject) |
| BarrelAttachment | `0x02000021` | Attachment | 18 | 18 | Barrel attachment |
| LaserSight | `0x02000022` | Attachment | 3 | 5 | Laser sight |
| Sight | `0x02000024` | Attachment | 10 | 11 | Sight base |
| RedDotSight | `0x02000023` | Sight | 4 | 3 | Red dot sight |

### Economy managers

| Type | Token | Extends | Fields | Methods | Role |
|---|---|---|---|---|---|
| MoneyManager | `0x0200008f` | NetworkBehaviour | 7 | 24 | Player money/currency |
| CasinoManager | `0x0200006a` | NetworkBehaviour | 7 | 32 | Casino/minigame |
| SlotMachine | `0x020000ea` | MonoBehaviour | 34 | 28 | Slot machine |
| BetButton | `0x0200013a` | Interactable | 4 | 10 | Bet button |
| BonusScoreRow | `0x0200013b` | MonoBehaviour | 13 | 11 | Bonus score row |
| LocalCasino | `0x02000145` | MonoBehaviour | 38 | 20 | Local casino |
| GameInfo | `0x02000077` | MonoBehaviour | 66 | 76 | Global game info/state |

### Enums
- `Rarity` (`0x02000051`, 5 fields)
- `SkinNoise` (`0x0200004f`, 4 fields)
- `SkinAffects` (`0x02000050`, 4 fields)
- `BetColor` (`0x0200006b`, 4 fields)
- `Difficulty` (`0x0200009f`, 4 fields)
- `FogState` (`0x020000a2`, 4 fields)
- `DamageType` (`0x0200012c`, 5 fields)

---

## 5. Boat & Water Vehicles

| Type | Token | Extends | Fields | Methods | Role |
|---|---|---|---|---|---|
| Boat | `0x020000c2` | NetworkBehaviour | 105 | 96 | Player boat |
| BoatMotor | `0x020000c5` | MonoBehaviour | 10 | 18 | Boat motor |
| BoatTrigger | `0x020000c6` | MonoBehaviour | 6 | 6 | Boat trigger |
| SelfDrivingBoat | `0x020000e6` | MonoBehaviour | 67 | 21 | AI boat |
| BoatManager | `0x02000064` | NetworkBehaviour | 5 | 13 | Boat manager |
| BoatInteractable | `0x02000009` | Interactable | 1 | 6 | Boat interactable |
| ExtraLava | `0x020000d0` | MonoBehaviour | 1 | 2 | Lava hazard |
| MainLava | `0x020000de` | MonoBehaviour | 7 | 11 | Main lava |
| HoopTrigger | `0x020000d1` | MonoBehaviour | 0 | 2 | Hoop trigger |

---

## 6. Managers & Systems (static-ish singletons)

| Type | Token | Extends | Fields | Methods | Role |
|---|---|---|---|---|---|
| GameInfo | `0x02000077` | MonoBehaviour | 66 | 76 | Global game state |
| LocalizationManager | `0x02000088` | MonoBehaviour | 104 | 101 | Text localization |
| ButtonManager | `0x02000067` | MonoBehaviour | 84 | 86 | UI buttons |
| ShaderManager | `0x020000a1` | MonoBehaviour | 65 | 26 | Shader management |
| SkinManager | `0x020000a3` | MonoBehaviour | 22 | 37 | Skin system |
| WaterManager | `0x020000b1` | MonoBehaviour | 45 | 29 | Water system |
| AudioManager | `0x0200005a` | static (1048833) | 40 | 36 | Audio playback |
| AudioSequenceManager | `0x02000061` | MonoBehaviour | 3 | 6 | Audio sequences |
| MusicManager | `0x02000090` | MonoBehaviour | 11 | 9 | Music |
| ParticleManager | `0x02000094` | MonoBehaviour | 2 | 7 | Particles |
| VFXManager | `0x020000b0` | MonoBehaviour | 2 | 6 | VFX |
| DecalManager | `0x02000070` | MonoBehaviour | 7 | 13 | Decals |
| DecorationManager | `0x02000072` | MonoBehaviour | 7 | 14 | Decorations |
| ExplosionManager | `0x02000075` | NetworkBehaviour | 5 | 22 | Explosions |
| HitmarkerManager | `0x0200007a` | MonoBehaviour | 11 | 4 | Hitmarkers |
| BobbleManager | `0x02000065` | MonoBehaviour | 10 | 8 | Bobble |
| CanvasManager | `0x02000069` | MonoBehaviour | 20 | 13 | Canvas |
| PauseManager | `0x02000096` | MonoBehaviour | 12 | 14 | Pause |
| SkipManager | `0x020000a5` | MonoBehaviour | 7 | 14 | Skip |
| SpawnManager | `0x020000a7` | MonoBehaviour | 8 | 5 | Spawning |
| SpriteManager | `0x020000a8` | MonoBehaviour | 2 | 10 | Sprites |
| CreatureManager | `0x0200006f` | NetworkBehaviour | 9 | 20 | Creature spawning |
| BirdManager | `0x02000063` | NetworkBehaviour | 32 | 28 | Birds |
| BossManager | `0x02000066` | NetworkBehaviour | 21 | 48 | Bosses |
| IslandManager | `0x0200007e` | MonoBehaviour | 13 | 21 | Islands |
| IslandSpawner | `0x020000d6` | MonoBehaviour | 1 | 2 | Island spawner |
| ItemManager | `0x02000082` | NetworkBehaviour | 10 | 35 | Item spawning |
| ProjectileManager | `0x02000098` | NetworkBehaviour | 11 | 37 | Projectiles |
| ServerSettings | `0x0200009e` | NetworkBehaviour | 9 | 21 | Server settings |
| ClientSettings | `0x0200006e` | MonoBehaviour | 1 | 4 | Client settings |
| SettingsInstance | `0x020000a0` | static | 2 | 4 | Settings instance |
| SettingsPreset | `0x020000e9` | ScriptableObject | 2 | 3 | Settings preset |
| TutorialManager | `0x020000af` | MonoBehaviour | 3 | 7 | Tutorial |
| Tutorial | `0x020000ed` | ScriptableObject | 7 | 10 | Tutorial data |
| AchievementManager | `0x02000058` | MonoBehaviour | 32 | 27 | Achievements |
| AudioImpactManager | `0x02000059` | MonoBehaviour | 2 | 6 | Impact audio |
| SurfaceManager | `0x020000ad` | MonoBehaviour | 3 | 5 | Surfaces |
| RigidbodyManager | `0x0200009c` | static (1048961) | 2 | 1 | Rigidbody util |
| Setter | `0x020000e8` | static (1048961) | 0 | 1 | Setter util |
| SpawnUtils | `0x020000eb` | MonoBehaviour | 0 | 2 | Spawn util |
| DazedUtils | `0x020000cb` | static (1048961) | 1 | 12 | Dazed util |
| DazedCommands | `0x020000ca` | MonoBehaviour | 24 | 19 | Commands/cheats |
| InstanceManager | `0x0200007c` | MonoBehaviour | 4 | 12 | Instance |
| LookAtPlayerManager | `0x0200008b` | MonoBehaviour | 1 | 2 | Look at player |
| MainMenuManager | `0x0200008c` | MonoBehaviour | 8 | 12 | Main menu |
| EndGameManager | `0x02000074` | NetworkBehaviour | 5 | 22 | End game |
| EndGameEffects | `0x020000cc` | MonoBehaviour | 13 | 7 | End game effects |
| EndGameToggles | `0x020000cd` | MonoBehaviour | 2 | 3 | End game toggles |
| KillScoreCalculator | `0x020000d9` | MonoBehaviour | 5 | 27 | Kill score calc |

---

## 7. Networking (game-layer, FishNet consumer)

| Type | Token | Extends | Fields | Methods | Role |
|---|---|---|---|---|---|
| Client | `0x020000b2` | NetworkBehaviour | 4 | 14 | Game client |
| Server | `0x020000bb` | NetworkBehaviour | 7 | 246 | Game server (heavy) |
| ConnectionManager | `0x020000b5` | MonoBehaviour | 8 | 14 | Connection |
| SteamManager | `0x020000ab` | MonoBehaviour | 11 | 28 | Steam integration |
| SteamLobbyAuthenticator | `0x020000bc` | NetworkBehaviour | 2 | 6 | Steam lobby auth |
| OnlineChatManager | `0x02000092` | NetworkBehaviour | 3 | 10 | Online chat |
| OnlineIslandManager | `0x02000093` | NetworkBehaviour | 7 | 23 | Online islands |
| RigidbodySync | `0x020000b8` | NetworkBehaviour | 37 | 69 | Rigidbody sync |
| ObserverToLocalAdapter | `0x020000b6` | NetworkBehaviour | 3 | 17 | Observer adapter |

**Networking generated:** `<SendSpawnPlayer>d__9`, `<VolcanoEruption>d__7`, `<AuthenticateSteamLobbyMember>d__5`, `<VolcanoEruption>d__7`.

---

## 8. Interactables & World Objects

| Type | Token | Extends | Fields | Methods | Role |
|---|---|---|---|---|---|
| Interactable | `0x0200000d` | MonoBehaviour | 6 | 13 | Base interactable |
| BoatInteractable | `0x02000009` | Interactable | 1 | 6 | Boat interactable |
| NPCInteractable | `0x02000015` | Interactable | 1 | 5 | NPC interactable |
| QuestInteractable | `0x02000017` | Interactable | 4 | 6 | Quest interactable |
| EndGameInteractable | `0x0200000c` | Interactable | 1 | 5 | End game interactable |
| GrillBox | `0x02000079` | MonoBehaviour | 9 | 10 | Grill/cooking |
| Radio | `0x02000049` | Item | 19 | 22 | Radio |
| Map | `0x02000042` | Item | 18 | 11 | Map |
| SellBox | `0x0200009d` | MonoBehaviour | 0 | 3 | Sell box |
| Island | `0x020000d5` | MonoBehaviour | 4 | 5 | Island |
| IslandWarning | `0x020000d7` | MonoBehaviour | 0 | 3 | Island warning |
| CasinoBox | `0x020000c7` | MonoBehaviour | 1 | 2 | Casino box |
| Clothes | `0x020000c9` | ScriptableObject | 4 | 5 | Clothes data |

---

## 9. Quests & Journal

| Type | Token | Extends | Fields | Methods | Role |
|---|---|---|---|---|---|
| NPCQuest | `0x020000e1` | ScriptableObject | 12 | 14 | NPC quest data |
| QuestType | `0x020000e2` | Enum | 8 | 0 | Quest type |
| QuestLineType | `0x02000014` | Enum | 6 | 0 | Quest line type |
| JournalSlot | `0x020000d8` | MonoBehaviour | 6 | 3 | Journal slot |
| `<UpdateJournal>d__27` | `0x02000123` | ValueType | 5 | 6 | Journal update coroutine |

---

## 10. Save / Load System

| Type | Token | Extends | Fields | Methods | Role |
|---|---|---|---|---|---|
| SaveManager | `0x0200012f` | MonoBehaviour | 8 | 36 | Save manager |
| SaveSystem | `0x02000139` | static | 6 | 9 | Save system |
| ServerSaveObject | `0x02000130` | ValueType | 18 | 1 | Server save DTO |
| LocalSaveObject | `0x02000131` | ValueType | 24 | 1 | Local save DTO |
| SavedPlayer | `0x02000134` | ValueType | 8 | 1 | Saved player |
| SavedItem | `0x02000135` | ValueType | 15 | 1 | Saved item |
| SavedWorldItem | `0x02000136` | ValueType | 4 | 1 | Saved world item |
| SavedCreature | `0x02000137` | ValueType | 3 | 1 | Saved creature |
| SavedItemSkin | `0x02000138` | ValueType | 2 | 1 | Saved item skin |
| AutoSaver | `0x0200012d` | MonoBehaviour | 1 | 3 | Auto-save |
| `<AutoSave>d__2` | `0x0200012e` | ValueType | 3 | 6 | Auto-save coroutine |
| `<LoadWorldItemsRoutine>d__35` | `0x02000133` | ValueType | 3 | 6 | Load world items |

---

## 11. UI (Menus, HUD, Screens)

| Type | Token | Extends | Fields | Methods | Role |
|---|---|---|---|---|---|
| BossUI | `0x02000118` | MonoBehaviour | 19 | 11 | Boss UI |
| CloseItemsUI | `0x02000119` | MonoBehaviour | 11 | 7 | Close items UI |
| DeathUI | `0x0200011a` | MonoBehaviour | 9 | 12 | Death UI |
| FishingUI | `0x0200011b` | MonoBehaviour | 14 | 10 | Fishing UI |
| IslandUI | `0x0200011c` | MonoBehaviour | 1 | 3 | Island UI |
| ItemUI | `0x0200011d` | MonoBehaviour | 11 | 18 | Item UI |
| MoneyUI | `0x0200011e` | MonoBehaviour | 14 | 5 | Money UI |
| NpcUI | `0x02000120` | MonoBehaviour | 10 | 9 | NPC UI |
| OnHitUI | `0x02000121` | MonoBehaviour | 14 | 8 | On-hit UI |
| ThinkingUI | `0x02000122` | MonoBehaviour | 19 | 16 | Thinking UI |
| VitalsUI | `0x02000124` | MonoBehaviour | 22 | 14 | Vitals UI |
| VoiceUI | `0x02000125` | MonoBehaviour | 3 | 3 | Voice UI |
| WeaponUI | `0x02000126` | MonoBehaviour | 14 | 14 | Weapon UI |
| EndGameUI | `0x02000142` | MonoBehaviour | 15 | 17 | End game UI |
| LocalSkin | `0x02000147` | MonoBehaviour | 37 | 43 | Local skin UI |
| NetworkPrompts | `0x02000148` | MonoBehaviour | 2 | 6 | Network prompts |
| RadarUI | `0x02000149` | MonoBehaviour | 27 | 18 | Radar UI |
| TutorialBox | `0x0200014c` | MonoBehaviour | 11 | 8 | Tutorial box |
| UIButton | `0x0200014e` | MonoBehaviour | 6 | 12 | UI button |
| UIInputField | `0x0200014f` | MonoBehaviour | 3 | 6 | UI input field |
| ColorPicker | `0x0200013d` | MonoBehaviour | 32 | 28 | Color picker |
| ColoringPart | `0x0200013e` | Enum | 5 | 0 | Coloring part |
| CreditsScroll | `0x0200013f` | MonoBehaviour | 4 | 3 | Credits scroll |
| CreditsTitle | `0x02000140` | MonoBehaviour | 1 | 3 | Credits title |
| DripText | `0x02000141` | MonoBehaviour | 7 | 3 | Drip text |
| LoadingManager | `0x02000144` | MonoBehaviour | 7 | 8 | Loading |
| MapDot | `0x0200014a` | ValueType | 5 | 8 | Map dot |
| SwapOutfitButtons | `0x0200014b` | MonoBehaviour | 3 | 2 | Swap outfit buttons |
| WorldText | `0x020000ef` | MonoBehaviour | 9 | 8 | World text |
| ScreenshotNPC | `0x020000e5` | MonoBehaviour | 27 | 6 | Screenshot NPC |
| MusicTrigger | `0x020000e0` | MonoBehaviour | 2 | 4 | Music trigger |

**UI generated:** `<TextAnimation>d__22`, `<AnimateTotalWorthText>d__50`, `<HideCreditsDelayed>d__28`, `<FinishAnimation>d__16`, `<UpdateJournal>d__27`, `<DelayedMainMenu>d__12`, `<HideBoatDealyed>d__18`, `<LowerChatHeight>d__30`, `<WaitForLocalization>d__290`.

---

## 12. Audio (voice chat, music, sound)

### MetaVoiceChat (third-party voice integration)
| Type | Token | Namespace | Fields | Methods | Role |
|---|---|---|---|---|---|
| MetaVc | `0x0200016f` | MetaVoiceChat | 19 | 11 | Voice chat main |
| VcConfig | `0x02000170` | MetaVoiceChat | 19 | 2 | Voice config |
| VcAudioInput | `0x02000184` | MetaVoiceChat.Input | 3 | 5 | Audio input |
| VcInputFilter | `0x02000185` | MetaVoiceChat.Input | 1 | 4 | Input filter |
| VcMic | `0x02000186` | MetaVoiceChat.Input.Mic | 10 | 20 | Microphone |
| VcMicAudioInput | `0x02000188` | MetaVoiceChat.Input.Mic | 2 | 13 | Mic audio input |
| VcAudioOutput | `0x02000176` | MetaVoiceChat.Output | 2 | 3 | Audio output |
| VcJitter | `0x02000177` | MetaVoiceChat.Output | 5 | 3 | Jitter buffer |
| MulticastVcAudioOutput | `0x0200017b` | MetaVoiceChat.Output.Multicast | 1 | 2 | Multicast output |
| VcAudioClip | `0x0200017c` | MetaVoiceChat.Output.AudioSource | 5 | 7 | Audio clip |
| VcAudioSourceOutput | `0x0200017d` | MetaVoiceChat.Output.AudioSource | 15 | 10 | Audio source output |
| VcDecoder | `0x0200017e` | MetaVoiceChat.Opus | 3 | 7 | Opus decoder |
| VcEncoder | `0x0200017f` | MetaVoiceChat.Opus | 4 | 7 | Opus encoder |
| INetProvider | `0x02000180` | MetaVoiceChat.NetProviders | 0 | 2 | Net provider interface |
| FishNetFrame | `0x02000181` | MetaVoiceChat.NetProviders.FishNet | 4 | 3 | FishNet frame |
| FishNetFrameReaderWriter | `0x02000182` | MetaVoiceChat.NetProviders.FishNet | 1 | 2 | Frame reader/writer |
| FishNetNetProvider | `0x02000183` | MetaVoiceChat.NetProviders.FishNet | 6 | 28 | FishNet net provider |
| FixedLengthArrayPool`1 | `0x02000172` | MetaVoiceChat.Utils | 2 | 3 | Array pool |
| FrameStopwatch | `0x02000173` | MetaVoiceChat.Utils | 3 | 4 | Stopwatch |
| MetaSerializableReactiveProperty`1 | `0x02000174` | MetaVoiceChat.Utils | 2 | 6 | Reactive property |
| MicrophoneDevicesListener | `0x02000175` | MetaVoiceChat.Utils | 2 | 3 | Mic devices |

**Voice generated:** `<CoRecord>d__34`, `<CoReconnect>d__13`, `<>c__DisplayClass8_0`, `Entry`, `AudioGainFilter`, `AudioInputTypeFilter` (filters extend `0x02000185`).

### Built-in audio enums
- `VoiceInputType` (`0x0200005b`, 4 fields)
- `VolumeType` (`0x0200005c`, 6 fields)
- `AudioDistance` (`0x0200005d`, 5 fields)
- `WaterCollisionAudio` (`0x0200005e`, 4 fields)

---

## 13. Input (Unity Input System sample UI)

| Type | Token | Namespace | Fields | Methods | Role |
|---|---|---|---|---|---|
| RebindUI.* (17 types) | `0x02000154`–`0x0200016d` | UnityEngine.InputSystem.Samples.RebindUI | varies | varies | Input rebinding UI |
| GameplayManager | `0x0200015a` | RebindUI | 23 | 28 | Gameplay |
| PlayerController | `0x02000160` | RebindUI | 10 | 7 | Player controller |
| FeedbackController | `0x02000159` | RebindUI | 17 | 18 | Feedback |
| RebindActionUI | `0x02000168` | RebindUI | 18 | 40 | Rebind action UI |
| GamepadIcons | `0x02000162` | ValueType | 19 | 1 | Gamepad icons |
| GameplayState | `0x0200015b` | Enum | 7 | 0 | Gameplay state |
| GameState | `0x0200016e` | Enum | 4 | 0 | Game state |

**Input generated:** `<>c` (x2), `<>c__DisplayClass0_0`, `<>c__DisplayClass40_0`.

---

## 14. IK, Collision, Effects

| Type | Token | Extends | Fields | Methods | Role |
|---|---|---|---|---|---|
| IK | `0x020000d2` | MonoBehaviour | 18 | 11 | Inverse kinematics |
| ImpactSoundType | `0x020000d3` | ScriptableObject | 6 | 7 | Impact sound data |
| Explosion | `0x02000002` | MonoBehaviour | 5 | 6 | Explosion |
| Disc | `0x02000032` | Item | 10 | 9 | Disc projectile |
| Explosive | `0x02000033` | Item | 30 | 26 | Explosive item |
| Projectile | `0x02000099` | ValueType | 16 | 1 | Projectile data |
| ProjectileType | `0x0200009a` | ValueType | 18 | 1 | Projectile type |
| WeaponInfo | `0x0200009b` | ValueType | 7 | 15 | Weapon info |
| HitMarker | `0x0200007b` | ValueType | 3 | 1 | Hit marker |
| BodyPartSpawnRot | `0x02000031` | ValueType | 2 | 0 | Body part rotation |
| ParticleAudio | `0x020000e3` | MonoBehaviour | 6 | 3 | Particle audio |
| RigSyncBackup | `0x020000e4` | MonoBehaviour | 0 | 1 | Rig sync backup |

---

## 15. Generated / Boilerplate (exclude from clone design)

~73 types (~18%) are compiler-generated or Unity-internal:

- **Async state machines** (`<...>d__N`): ~41
- **Closures** (`<>c`): ~14
- **Display classes** (`<>c__DisplayClassN_M`): ~7
- **FishNet generated Sample structs**: 4 (`Sample` x4)
- **Static init buffers** (`__StaticArrayInitTypeSize=...`): 3
- **`<PrivateImplementationDetails>`**: 1
- **Unity scripting:** `UnitySourceGeneratedAssemblyMonoScriptTypes_v1`, `MonoScriptData`
- **FishNet.Serializing.Generated** (in this assembly too): `GeneratedComparers___Internal`, `GeneratedWriters___Internal`, `GeneratedReaders___Internal`

---

## 16. Top-15 Prioritized Deep-Dive List (Assembly-CSharp)

| # | Type | Token | Why it matters for the clone |
|---|---|---|---|
| 1 | `Player` | `0x020000f2` | Local player root; NetworkBehaviour; composes all player subsystems |
| 2 | `PlayerMovement` | `0x0200010d` | Locomotion core (73 fields) |
| 3 | `PlayerInventory` | `0x02000100` | Inventory slots & items (63 methods) |
| 4 | `PlayerHolding` | `0x020000ff` | Held item/tool interaction (57 methods) |
| 5 | `PlayerVitals` | `0x0200012b` | Health/hunger/vitals (41 methods) |
| 6 | `Item` | `0x0200003a` | Base item (101 fields, 167 methods) — data model root |
| 7 | `FishingRod` | `0x02000037` | Fishing mechanic (75 fields, 63 methods) |
| 8 | `Creature` | `0x0200002e` | Base creature/wildlife (66 methods) |
| 9 | `Fish` | `0x02000034` | Base fish (14 methods) |
| 10 | `NPC` | `0x02000010` | NPC/quest givers (36 methods) |
| 11 | `SaveManager` | `0x0200012f` | Save/load system (36 methods) |
| 12 | `MoneyManager` | `0x0200008f` | Economy/currency (24 methods) |
| 13 | `GameInfo` | `0x02000077` | Global game state (76 methods) |
| 14 | `Server` | `0x020000bb` | Game server authority (246 methods — largest) |
| 15 | `Client` | `0x020000b2` | Game client (14 methods) |

> Next: deep-dive each via `inspect-managed-members --type <full_name>` to extract field/method signatures, then port the design into `GAME_DESIGN.md` / `DATA_MODELS.md`.

---

## Appendix: Type count by category

| Category | Types |
|---|---|
| Player & character | 25 |
| Creatures / fish / wildlife | 15 |
| Items / tools / weapons | 11 |
| Purchasables / attachments / economy | 16 |
| Boat & water | 9 |
| Managers & systems | 32 |
| Networking (game layer) | 9 |
| Interactables & world objects | 12 |
| Quests & journal | 5 |
| Save / load | 11 |
| UI | 27 |
| Audio (incl. MetaVoiceChat) | 24 |
| Input (RebindUI) | 17 |
| IK / collision / effects | 13 |
| Enums | 13 |
| Generated / boilerplate | ~73 |
| **Total** | **400** |
