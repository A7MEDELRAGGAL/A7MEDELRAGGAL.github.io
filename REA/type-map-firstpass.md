# Type Map: Assembly-CSharp-firstpass.dll

**Artifact:** `...\How to Fish\How to Fish_Data\Managed\Assembly-CSharp-firstpass.dll`
**SHA-256:** `c83eca316187944582e5eee59cebc4c3c8a49ce4256d4273a288016c84cf1733`
**MVID:** `1ca53885-79a9-49fb-9f16-9c0aef414990`

## Inventory Totals

| Metric | Count |
|---|---|
| TypeDef | 49 |
| Field | 388 |
| MethodDef | 972 |
| Param | 1,272 |
| InterfaceImpl | 4 |
| MemberRef | 416 |
| Call edges | 3,764 |
| Field accesses | 3,857 |
| Exported types | 0 |

## Namespaces

| Namespace | Types | Notes |
|---|---|---|
| (global) | 34 | LeanTween animation library + boilerplate |
| DentedPixel | 1 | `LeanDummy` |
| FishySteamworks | 4 | Steamworks sockets (base namespace) |
| FishySteamworks.Server | 1 | `ServerSocket` |
| FishySteamworks.Client | 2 | `ClientHostSocket`, `ClientSocket` |

## Full Type List (49)

| # | Type | Token | Extends | Fields | Methods | Namespace | Role |
|---|---|---|---|---|---|---|---|
| 1 | `<Module>` | 0x02000001 | - | 0 | 0 | - | Module |
| 2 | LTDescr | 0x02000002 | MonoBehaviour | 48 | 311 | - | LeanTween descriptor (core) |
| 3 | EaseTypeDelegate | 0x02000003 | MulticastDelegate (`0x0100002a`) | 0 | 4 | - | Easing delegate |
| 4 | ActionMethodDelegate | 0x02000004 | MulticastDelegate | 0 | 4 | - | Action delegate |
| 5 | `<>c` | 0x02000005 | Object | 3 | 4 | - | Closure |
| 6 | LTDescrOptional | 0x02000006 | MonoBehaviour | 24 | 45 | - | Optional tween descriptor |
| 7 | LTSeq | 0x02000007 | MonoBehaviour | 9 | 16 | - | Tween sequence |
| 8 | LeanAudioStream | 0x02000008 | MonoBehaviour | 3 | 3 | - | Audio stream |
| 9 | LeanAudio | 0x02000009 | MonoBehaviour | 5 | 15 | - | Audio |
| 10 | LeanAudioOptions | 0x0200000a | MonoBehaviour | 8 | 10 | - | Audio options |
| 11 | LeanAudioWaveStyle | 0x0200000b | Enum (`0x01000030`) | 5 | 0 | - | Wave style enum |
| 12 | LeanSmooth | 0x0200000c | MonoBehaviour | 0 | 13 | - | Smoothing |
| 13 | LeanTester | 0x0200000d | MonoBehaviour (`0x01000031`) | 1 | 3 | - | Tester |
| 14 | `<timeoutCheck>d__2` | 0x0200000e | Object | 4 | 6 | - | Generated async |
| 15 | LeanTest | 0x0200000f | MonoBehaviour | 6 | 10 | - | Test util |
| 16 | TweenAction | 0x02000010 | Enum (`0x01000030`) | 52 | 0 | - | Tween action enum |
| 17 | LeanTweenType | 0x02000011 | Enum (`0x01000030`) | 40 | 0 | - | Tween type enum |
| 18 | LeanProp | 0x02000012 | Enum (`0x01000030`) | 11 | 0 | - | LeanProp enum |
| 19 | LeanTween | 0x02000013 | MonoBehaviour (`0x01000031`) | 31 | 177 | - | **LeanTween main (177 methods)** |
| 20 | `<>c__DisplayClass193_0` | 0x02000014 | Object | 3 | 11 | - | Display class |
| 21 | `<>c__DisplayClass194_0` | 0x02000015 | Object | 5 | 11 | - | Display class |
| 22 | `<>c__DisplayClass195_0` | 0x02000016 | Object | 6 | 11 | - | Display class |
| 23 | `<>c__DisplayClass196_0` | 0x02000017 | Object | 2 | 11 | - | Display class |
| 24 | LTUtility | 0x02000018 | MonoBehaviour | 0 | 2 | - | Utility |
| 25 | LTBezier | 0x02000019 | MonoBehaviour | 7 | 4 | - | Bezier |
| 26 | LTBezierPath | 0x0200001a | MonoBehaviour | 8 | 13 | - | Bezier path |
| 27 | LTSpline | 0x0200001b | ValueType | 11 | 20 | - | Spline |
| 28 | LTRect | 0x0200001c | ValueType | 21 | 29 | - | Rect |
| 29 | LTEvent | 0x0200001d | MonoBehaviour | 2 | 1 | - | Event |
| 30 | LTGUI | 0x0200001e | MonoBehaviour | 12 | 18 | - | GUI |
| 31 | Element_Type | 0x0200001f | Enum (`0x01000030`) | 3 | 0 | - | Element type |
| 32 | LeanTweenExt | 0x02000020 | static (`0x01000001`) | 0 | 99 | - | LeanTween extensions (99 methods) |
| 33 | UnitySourceGeneratedAssemblyMonoScriptTypes_v1 | 0x02000021 | Object | 0 | 2 | - | Unity generated |
| 34 | MonoScriptData | 0x02000022 | ScriptableObject (`0x01000054`) | 5 | 0 | - | MonoScript data |
| 35 | LeanDummy | 0x02000023 | MonoBehaviour | 0 | 1 | DentedPixel | DentedPixel dummy |
| 36 | BidirectionalDictionary`2 | 0x02000024 | Object | 2 | 21 | FishySteamworks | Bidirectional dict |
| 37 | CommonSocket | 0x02000025 | Object | 6 | 9 | FishySteamworks | **Socket base** |
| 38 | LocalPacket | 0x02000026 | ScriptableObject | 3 | 1 | FishySteamworks | Local packet |
| 39 | FishySteamworks | 0x02000027 | MonoBehaviour (`0x0100005c`) | 17 | 45 | FishySteamworks | **Steamworks main** |
| 40 | ServerSocket | 0x02000028 | CommonSocket | 12 | 20 | FishySteamworks.Server | Server socket |
| 41 | ConnectionChange | 0x02000029 | ScriptableObject | 3 | 3 | - | Connection change |
| 42 | ClientHostSocket | 0x0200002a | CommonSocket | 2 | 8 | FishySteamworks.Client | Client host socket |
| 43 | ClientSocket | 0x0200002b | CommonSocket | 2 | 8 | FishySteamworks.Client | Client socket |
| 44 | `<PrivateImplementationDetails>` | 0x0200002c | Object | 2 | 0 | - | Private impl |
| 45 | `__StaticArrayInitTypeSize=631` | 0x0200002d | ValueType | 0 | 0 | - | Static buffer |
| 46 | `__StaticArrayInitTypeSize=995` | 0x0200002e | ValueType | 0 | 0 | - | Static buffer |
| 47 | GeneratedComparers___Internal | 0x0200002f | Object | 0 | 1 | FishNet.Serializing.Generated | FishNet generated |
| 48 | GeneratedWriters___Internal | 0x02000030 | Object | 0 | 1 | FishNet.Serializing.Generated | FishNet generated |
| 49 | GeneratedReaders___Internal | 0x02000031 | Object | 0 | 1 | FishNet.Serializing.Generated | FishNet generated |

## Role Summary

- **LeanTween (DentedPixel):** `LTDescr`, `LeanTween`, `LeanTweenExt`, `LTSeq`, `LTBezier`, `LTBezierPath`, `LTSpline`, `LTRect`, `LTGUI`, `LeanAudio*`, `LeanSmooth`, `LTUtility` — UI/animation tweening used throughout the game (menus, HUD, transitions).
- **FishySteamworks:** `FishySteamworks`, `CommonSocket`, `ServerSocket`, `ClientHostSocket`, `ClientSocket`, `LocalPacket`, `BidirectionalDictionary` — Steam networking/sockets transport used by the game layer (complements FishNet).
- **Boilerplate:** 10 generated/static-init types.

> Note: firstpass contributes **no gameplay logic** itself — it's animation (LeanTween) + Steam socket transport. Gameplay lives in Assembly-CSharp.dll; networking authoritative layer lives in FishNet.Runtime.dll.
