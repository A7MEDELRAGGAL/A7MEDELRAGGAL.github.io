# Type Map: FishNet.Runtime.dll (Networking Layer, FishNet 4.6.12)

**Artifact:** `...\How to Fish\How to Fish_Data\Managed\FishNet.Runtime.dll`
**SHA-256:** `71ac44a95c7af0cd9333c851f41792ef342b373af26e10f69795e02c9550c61a`
**MVID:** `5254e175-6a7c-489e-beb8-55179113ab9f`

> FishNet version 4.6.12 (confirmed via string literal `0x7000936e`).

## Inventory Totals

| Metric | Count |
|---|---|
| TypeDef | 587 |
| Field | 2,452 |
| MethodDef | 4,935 |
| Param | 5,743 |
| InterfaceImpl | 93 |
| MemberRef | 2,863 |
| Call edges | 17,380 |
| Field accesses | 11,055 |
| Exported types | 0 |
| Namespaces | ~60 (55 `FishNet.*`) |

---

## Subsystem 1: Low-level Transport (LiteNetLib)

LiteNetLib is the underlying UDP transport FishNet builds on. ~100 types.

**Core (`LiteNetLib`):**
- `NetManager` (77 fields, 116 methods) — the LiteNetLib network manager
- `NetPeer` (49 fields, 67 methods) — connected peer
- `NetPacketReader` / `NetPacket` / `NetDataReader` / `NetDataWriter` — packet I/O
- `NetConnectRequestPacket`, `NetConnectAcceptPacket`, `NatIntroduceRequest/ResponsePacket`, `NatPunchPacket` — NAT punchthrough
- `NetStatistics`, `NetConstants`, `NetDebug`, `NetLogLevel`, `NetUtils`
- `ConnectionState`, `DeliveryMethod`, `DisconnectReason`, `UnconnectedMessageType`, `NatAddressType`
- `INetEventListener`, `IDeliveryEventListener`, `INtpEventListener`, `IPeerAddressChangedListener`, `INetLogger`
- `EventBasedNetListener` + `OnPeerConnected/Disconnected/NetworkError/NetworkReceive/...` event callbacks
- `BaseChannel`, `ReliableChannel`, `SequencedChannel`, `PendingPacket`, `IncomingFragments`
- `NativeSocket`, `WinSock`, `UnixSock`, `PausedSocketFix`
- `NtpPacket`, `NtpLeapIndicator`, `NtpMode`, `NtpRequest` — NTP time sync

**LiteNetLib.Utils:**
- `NetDataReader` (4 fields, 103 methods), `NetDataWriter` (7 fields, 51 methods) — binary reader/writer
- `NetPacketProcessor` (2 fields, 23 methods) — typed packet serialization
- `NetSerializer`, `CustomType`/`CustomTypeStruct/Class/Static` — custom type registration
- `CRC32C`, `FastBitConverter`, `StringSerializer` + primitive serializers (`IntSerializer`…`BoolSerializer`…)
- `INetSerializable`, `InvalidTypeException`, `ParseException`

**LiteNetLib.Layers:**
- `PacketLayerBase`, `Crc32cLayer`, `XorEncryptLayer` — packet encryption/integrity layers

---

## Subsystem 2: FishNet Core (`FishNet`, `FishNet.Utility`)

- `InstanceFinder` (1 field, 28 methods) — static service locator for NetworkManager/Client/Server/TransportManager
- `UtilityConstants` — constants
- `TickNetworkBehaviour` — template network behaviour
- `ByteArrayPool`, `DefaultObjectPool`, `ObjectPool`, `ObjectPoolRetrieveOption`, `RetrieveOptionExtensions`, `GetNonAlloc` — object pooling
- `Extension.NetworksFN`, `Extension.Scenes`, `Extension.TransformFN` — extension methods
- `TickCallback` (delegate type)

---

## Subsystem 3: Serialization (`FishNet.Serializing`)

- `Reader` (9 fields, 239 methods) / `PooledReader`, `ReaderPool` — deserialization
- `Writer` (23 fields, 224 methods) / `PooledWriter`, `WriterPool` — serialization
- `GenericReader`/`GenericWriter`/`GenericDeltaReader`/`GenericDeltaWriter` (`T`) — generic typed IO
- `DeltaSerializerOption`, `DeltaVector2Type`, `DeltaVector3Type`, `DeltaWholeType`, `UDeltaPrecisionType`, `DeltaTypeExtensions` — delta (compressed) serialization
- `ReaderExtensions`, `PooledReader`, `SubStream`
- `TransformPackingData`, `UnsignedVector3DeltaFlag`
- `Helping`: `PublicPropertyComparer`, `Comparers`, `SceneComparer`, `Quaternion32Compression`, `Quaternion64Compression`, `QuaternionDeltaPrecisionCompression`, `ReservedLengthWriter`, `ReservedWritersExtensions`, `UIntFloat/Double/Decimal`, `SceneHandleEqualityComparer`
- `AutoPackType`, `ComponentType`
- `Generated`: `GeneratedComparers___Internal`, `GeneratedWriters___Internal`, `GeneratedReaders___Internal` — code-generated serialization (61 writers + 61 readers)

---

## Subsystem 4: Transporting (`FishNet.Transporting`)

- `Transport` (2 fields, 45 methods) — abstract transport base
- `TransportConsts`, `Channel` (enum), `LocalConnectionState`, `RemoteConnectionState`, `IPAddressType`, `PacketId` (22 fields)
- `NetworkReaderLoop`, `NetworkWriterLoop`
- Connection args: `ConnectedClientsArgs`, `ServerReceivedDataArgs`, `ClientReceivedDataArgs`, `RemoteConnectionStateArgs`, `ServerConnectionStateArgs`, `ClientConnectionStateArgs`
- **Tugboat** (built-in reliable transport): `CommonSocket`, `Packet`, `Tugboat` (23 fields, 54 methods), `Server.ServerSocket`, `Client.ClientSocket`
- **Multipass:** `Multipass` (13 fields, 77 methods) — combines transports
- **Yak:** `CommonSocket`, `LocalPacket`, `Yak` (8 fields, 41 methods), `Server.ServerSocket`, `Client.ClientSocket`

**Managing.Transporting:** `IntermediateLayer`, `LatencySimulator` (12 fields, 21 methods), `SplitReader`, `TransportManager` (25 fields, 51 methods), `Message`, `DisconnectingClient`

---

## Subsystem 5: Observing (`FishNet.Observing`, `FishNet.Component.Observing`)

- `NetworkObserver` (13 fields, 18 methods) — object visibility/observers
- `ObserverCondition` (4 fields, 9 methods), `ObserverConditionType` (enum), `HostVisibilityUpdateTypes`
- `ConditionOverrideType`, `ObserverStateChange`
- Conditions: `DistanceCondition`, `GridCondition` (+`GridEntry`, `HashGrid`, `GridAxes`), `HostOnlyCondition`, `MatchCondition` (1 field, 41 methods), `OwnerOnlyCondition`, `SceneCondition`
- `ConditionCollections`

---

## Subsystem 6: Network Objects (`FishNet.Object`)

- `NetworkBehaviour` (45 fields, 186 methods) — **base class for all networked game objects** (Player, Server, Client, Boat, etc. derive from this)
- `NetworkObject` (79 fields, 212 methods) — networked object identity/spawn
- `EmptyNetworkBehaviour` (2 fields, 5 methods)
- `RpcLink` (4 fields, 1 method), `RpcLinkType`, `DataOrderType`
- `NetworkObjectIdComparer`, `NetworkObjectState` (enum), `GetNetworkObjectOption` (enum)
- `TransformProperties`/`Cls`/`Flag`/`Extensions` — transform sync options
- `GlobalPreserveOwnedObjects`, `NetworkPreserveOwnedObjects`, `DespawnType`
- `MissingObjectPacketLength`

**Attributes (RPC):**
- `RpcAttribute` (3 fields, 1 method)
- `ServerRpcAttribute` (2 fields), `ObserversRpcAttribute` (4 fields), `TargetRpcAttribute` (3 fields)
- `ServerAttribute` (2 fields), `ClientAttribute` (3 fields)
- `ReplicateAttribute`, `ReconcileAttribute` (prediction)

---

## Subsystem 7: Prediction & Reconciliation (`FishNet.Object.Prediction`, `FishNet.Managing.Predicting`)

- `IReplicateData`, `IReconcileData` — interfaces
- `LocalReconcile`/`ReplicateDataContainer` (`T`)
- `MoveRates` (7 fields, 30 methods), `MoveRatesCls` — movement rates
- `PredictionRigidbody` / `PredictionRigidbody2D` + serializers — physics prediction
- `PredictionigidbodySerializers`, `PredictionRigidbody2DSerializers`
- `AllForceData`, `EntryData`, `IForceData`, `ForceApplicationType` — force data
- `DataCachingType`, `ReplicateState` (enum), `ReplicateStateExtensions`
- `LocalTransformTickSmoother`
- **Delegating:** `ReplicateRpcDelegate`, `ReconcileRpcDelegate`, `ReplicateUserLogicDelegate`/`ReconcileUserLogicDelegate` (`T`)
- `PredictionManager` (37 fields, 51 methods) — manages prediction ticks
- `ReplicateStateOrder` (enum)
- `StatePacket`, `StatePacketTick`, `IncomingData`
- Delegates: `PreReconcileDel`, `ReconcileDel`, `PostReconcileDel`, `PrePhysicsSyncTransformDel`, `PostPhysicsSyncTransformDel`, `PreReplicateReplayDel`, `ReplicateReplayDel`, `PostReplicateReplayDel`

---

## Subsystem 8: RPC Delegates (`FishNet.Object.Delegating`)

- `ServerRpcDelegate`, `ClientRpcDelegate`, `SyncVarReadDelegate` — RPC/sync-var invoke delegates

---

## Subsystem 9: Synchronizing (`FishNet.Object.Synchronizing`, `FishNet.Object.Synchronizing.Internal`)

- `ISyncVar`, `ISyncVar`, `SyncObjectAttribute`, `SyncVarAttribute`, `ICustomSync`
- `SyncVar<T>` (7 fields, 23 methods) — synchronized field
- Primitive sync vars: `FloatSyncVar`, `DoubleSyncVar`, `SbyteSyncVar`, `ByteSyncVar`, `ShortSyncVar`, `UShortSyncVar`, `IntSyncVar`, `UIntSyncVar`, `LongSyncVar`, `ULongSyncVar`, `Vector2SyncVar`, `Vector3SyncVar`, `Vector4SyncVar`, `Vector2IntSyncVar`, `Vector3IntSyncVar`
- `SyncBase` (16 fields, 52 methods) — internal base
- `SyncList<T>` (9 fields, 47 methods), `SyncDictionary<TKey,TValue>` (8 fields, 40 methods), `SyncHashSet<T>` (10 fields, 46 methods) — synchronized collections
- `SyncStopwatch` (8 fields, 29 methods), `SyncTimer` (8 fields, 29 methods)
- Change data / callbacks: `CachedOnChange`, `ChangeData`, `SyncListChanged`, `SyncDictionaryChanged`, `SyncHashSetChanged`, `SyncTypeChanged`, `OnChanged`
- Operations: `SyncListOperation`, `SyncDictionaryOperation`, `SyncHashSetOperation`, `SyncStopwatchOperation`, `SyncTimerOperation`
- `SyncTypeSettings`, `SyncTypeWriteFlag`, `ReadPermission`, `WritePermission`

---

## Subsystem 10: Components (`FishNet.Component.*`)

**ColliderRollback:** `ColliderRollback` (6 fields, 5 methods), `RollbackManager` (4 fields, 17 methods), `BoundingBoxType`, `RollbackPhysicsType`

**Observing:** see Subsystem 5 conditions.

**Scenes:** `DefaultSceneProcessor`, `SceneProcessorBase`, `SceneManager` (33 fields, 117 methods), `DefaultScene` (6 fields, 16 methods), `SceneLoadData`/`SceneUnloadData`, `LoadOptions`/`UnloadOptions`, `SceneScopeType`, `ReplaceOption`, `PreferredScene`, `LoadParams`/`UnloadParams`, `LoadQueueData`/`UnloadQueueData`, `UnloadedScene`, broadcast events (`LoadScenesBroadcast`, `UnloadScenesBroadcast`, `ClientScenesLoadedBroadcast`, `ClientConnectionChangeBroadcast`, `ConnectedClientsBroadcast`, `EmptyStartScenesBroadcast`), `SceneLookupData`

**Utility:** `BandwidthDisplay` (16 fields, 16 methods), `PingDisplay`, `Corner`, `InOutAverage`

**Ownership:** `PredictedOwner` (6 fields, 21 methods), `PredictedSpawn` (4 fields, 13 methods)

**Spawning:** `PlayerSpawner` (6 fields, 10 methods), `ServerSpawner` (3 fields, 7 methods)

**Prediction (components):** `NetworkCollider`/`NetworkCollider2D`/`NetworkColliderBase` (8 fields, ~20 methods), `NetworkCollision`/`NetworkCollision2D`/`NetworkTrigger`/`NetworkTrigger2D`, `OfflineRigidbody`, `RigidbodyPauser`, `RigidbodyData`/`Rigidbody2DData`, `RigidbodyState`/`Rigidbody2DState`, `RigidbodyStateSerializers`, `RigidbodyStateExtensions`, `RigidbodyType`

**Transforming:** `NetworkTransform` (51 fields, 115 methods) — **key transform sync component**
- `TransformTickSmoother` (26 fields, 30 methods), `MonoTickSmoother`, `DetachableNetworkTickSmoother`
- `SynchronizedProperty`, `AdaptiveInterpolationType`, `SnappedAxes`, `SnappedAxesExtensions`
- `UpdateFlagA/B`, `ChangedDelta`, `ChangedFull`, `GoalData`, `RateData`, `TransformData`, `ExtrapolateState`, `DataReceivedChanged`
- `ReceivedClientData`, `ComponentConfigurationType`
- `Beta`: `NetworkTickSmoother`, `OfflineTickSmoother`, `TickSmootherController`, `UniversalTickSmoother`, `InitializationSettings`, `MovementSettings`

**Animating:** `NetworkAnimator` (30 fields, 60 methods) — animator sync
- `ReceivedServerData`, `StateChange`, `ClientAuthoritativeUpdate`, `SmoothedFloat`, `TriggerUpdate`, `ParameterDetail`

---

## Subsystem 11: Managing (`FishNet.Managing.*`)

- `NetworkManager` (29 fields, 90 methods) — **top-level FishNet manager**
- `NetworkManagerExtensions` (10 methods)
- `RemoteTimeoutType`, `PersistenceType`
- `Server.ClientConnectionChangeBroadcast`, `ConnectedClientsBroadcast`, `KickReason`, `ServerObjects`, `ServerManager` (30 fields, 81 methods)
- `Client.ClientManager` (19 fields, 46 methods), `ClientObjects`, `ClientObjectCache` (8 fields, 10 methods), `CachedNetworkObject` (21 fields, 11 methods), `ActionType`, `CacheSearchType`
- `SceneManager` (see components), `SceneProcessorBase`
- `TransportManager` (see transporting)
- `TimeManager` (39 fields, 86 methods), `PreciseTick` (5 fields, 15 methods), `EstimatedTick` (5 fields, 20 methods), `MovingAverage`, `PhysicsMode`, `TickRounding`, `TickType`, `TimingType`, `UpdateOrder`
- `PredictionManager` (see prediction)
- `ObserverManager` (6 fields, 11 methods)
- `Object`: `ManagedObjects` (6 fields, 39 methods), `DualPrefab`, `SpawnType`, `DefaultPrefabObjects`, `DualPrefabObjects`, `PrefabObjects`, `SinglePrefabObjects`
- `Logging`: `LevelLoggingConfiguration`, `LoggingConfiguration`, `LoggingType`
- `Debugging`: `DebugManager`
- `Statistic`: `NetworkTrafficStatistics` (9 fields, 22 methods), `StatisticsManager`, `EnabledMode`, `NetworkTrafficUpdateDel`

---

## Subsystem 12: Connection (`FishNet.Connection`)

- `NetworkConnection` (36 fields, 78 methods) — connection representation
- `NetworkConnectionExtensions`
- `ByteBuffer` (5 fields, 14 methods), `PacketBundle` (7 fields, 10 methods)

---

## Subsystem 13: CodeGen / Broadcast / Auth / Editor / Documenting

- `CodeGenerating`: `AllowMutableSyncTypeAttribute`, `IncludeSerializationAttribute`, `ExcludeSerializationAttribute`, `NotSerializerAttribute`, `MakePublicAttribute`, `CustomComparerAttribute`, `UseGlobalCustomSerializerAttribute`, `ReadUnallocatedAttribute`, `DefaultWriterAttribute`, `DefaultReaderAttribute`, `DefaultDeltaWriterAttribute`, `DefaultDeltaReaderAttribute`
- `Broadcast`: `IBroadcast`, `BroadcastsSerializers`, `BroadcastExtensions`, `BroadcastHandlerBase`, `ClientBroadcastHandler<T>`, `ServerBroadcastHandler<T>`
- `Authenticating`: `Authenticator` (2 fields, 9 methods)
- `Editing`: `EditingConstants`, `WindowResizeData`, `BidirectionalNetworkTraffic`, `NetworkTraffic`, `Packet`, `PacketGroup`, `ProfiledTickData`
- `Configuring.EditorCloning`: `CloneChecker`, `EditorCloneType`
- `Documenting`: `APIExcludeAttribute`

---

## Subsystem 14: Boilerplate / Generated (~120 types)

- `GeneratedComparers___Internal`, `GeneratedWriters___Internal`, `GeneratedReaders___Internal` (3 x, incl. one in firstpass)
- `__StaticArrayInitTypeSize=28/288/20559/21263` (4 static buffers)
- `<PrivateImplementationDetails>` (4 fields)
- ~70 `<>c`, `<>c__DisplayClassN_M`, `<>c__DisplayClassN_M<T>` closures and display classes (async/lambda state)
- `Microsoft.CodeAnalysis.EmbeddedAttribute`, `System.Runtime.CompilerServices.IsUnmanagedAttribute` (compiler attrs)
- `UnitySourceGeneratedAssemblyMonoScriptTypes_v1`, `MonoScriptData` (Unity scripting)
- `LiteNetLib` delegate/event types (`OnPeerConnected`, `SubscribeDelegate`, `FastCall*`, `Serializer*`, `Enum*Serializer`, `ClassInfo`)

---

## Key Networking Contracts for the Clone

| Contract | FishNet type | Game-layer consumer (Assembly-CSharp) |
|---|---|---|
| Networked object base | `NetworkBehaviour` (0x020000ec) | `Player`, `Server`, `Client`, `Boat`, `FishingRod`, `Creature`, `Fish`, `NPC`, etc. |
| Networked object identity | `NetworkObject` (0x020000f8) | all spawned networked entities |
| Server RPC | `ServerRpcAttribute` (0x020000e6) | client→server requests |
| Observers RPC | `ObserversRpcAttribute` (0x020000e7) | server→all clients |
| Target RPC | `TargetRpcAttribute` (0x020000e8) | server→specific client |
| SyncVar (synced field) | `SyncVar<T>` (0x02000157) | synced state (health, score, etc.) |
| SyncList/SyncDict/SyncHashSet | `SyncList<T>`/`SyncDictionary<T>`/`SyncHashSet<T>` | synced collections (inventory, players) |
| Prediction/reconciliation | `ReplicateAttribute`/`ReconcileAttribute` | client-side prediction (movement) |
| Transform sync | `NetworkTransform` (0x02000189) | position/rotation sync |
| Animator sync | `NetworkAnimator` (0x020001a9) | animation state sync |
| Transport | `Transport` + `Tugboat`/`Multipass`/`Yak` | UDP layer |
| Connection | `NetworkConnection` | per-client connection |
| Object spawning | `NetworkObject`, `PlayerSpawner`/`ServerSpawner` | entity spawn |
| Scene management | `SceneManager`, `SceneLoadData` | level loading |
| Time/tick | `TimeManager`, `PreciseTick` | fixed tick loop |

> Next step: deep-dive the top Assembly-CSharp classes to extract their RPCs/SyncVars and field signatures, then write GAME_DESIGN.md / ARCHITECTURE.md / DATA_MODELS.md.
