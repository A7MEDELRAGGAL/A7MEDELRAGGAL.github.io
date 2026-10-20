using Clone.Creatures;
using Clone.Player;
using Clone.World;
using FishNet.Object;
using UnityEngine;

namespace Clone.Items
{
    public enum CloneRodState
    {
        Idle,
        Casting,
        Waiting,
        Bite,
        Hooked,
        Reeling,
        Caught,
        Missed
    }

    public class CloneFishingRod : CloneTool
    {
        [Header("Presentation")]
        public GameObject BobberPrefab;
        public Transform TipAnchor;

        [Header("Tuning")]
        public float MaxLineLength = 45f;
        public float BiteHoldSeconds = 1.2f;
        public float HookSearchRadius = 3.5f;
        public float CatchDistance = 2.4f;
        public float ReelPullSpeed = 3.2f;
        public float MissRecoverSeconds = 1.4f;

        [Header("Rewards")]
        public int FallbackCatchItemId = CloneItemCatalog.Mackerel;
        public float FallbackCatchWorth = 10f;

        public CloneRodState State { get; private set; } = CloneRodState.Idle;
        public Vector3 BitePoint { get; private set; }
        public int BaitItemId { get; private set; } = CloneInventorySlot.EmptyId;
        public NetworkObject HookedFish { get; private set; }

        private NetworkObject _owner;
        private GameObject _bobber;
        private float _stateTimer;
        private float _reelTimer;

        public bool IsBusy => State != CloneRodState.Idle;

        public override bool CanUse(NetworkObject user)
        {
            if (Definition == null || !Definition.IsRod)
                return false;

            return !IsBusy || State == CloneRodState.Waiting || State == CloneRodState.Bite
                || State == CloneRodState.Hooked || State == CloneRodState.Reeling;
        }

        public override void ServerUse(NetworkObject user)
        {
            switch (State)
            {
                case CloneRodState.Idle:
                case CloneRodState.Missed:
                    ServerCast(user);
                    return;
                case CloneRodState.Waiting:
                    ServerReelIn(false);
                    return;
                case CloneRodState.Bite:
                    ServerAttemptHook();
                    return;
                case CloneRodState.Hooked:
                case CloneRodState.Reeling:
                    ServerReelIn(true);
                    return;
            }
        }

        private void FixedUpdate()
        {
            if (!IsSpawned || !IsServer)
                return;

            _stateTimer -= Time.fixedDeltaTime;

            switch (State)
            {
                case CloneRodState.Waiting:
                    if (_stateTimer <= 0f)
                        BeginBite();
                    break;
                case CloneRodState.Bite:
                    if (_stateTimer <= 0f)
                        ServerMiss();
                    break;
                case CloneRodState.Reeling:
                    TickReel(Time.fixedDeltaTime);
                    break;
                case CloneRodState.Missed:
                    if (_stateTimer <= 0f)
                        SetState(CloneRodState.Idle);
                    break;
            }
        }

        private void SetState(CloneRodState state)
        {
            State = state;
            _stateTimer = 0f;
            SyncRodStateTarget(State, BitePoint);
        }

        [TargetRpc]
        private void SyncRodStateTarget(CloneRodState state, Vector3 bitePoint)
        {
            State = state;
            BitePoint = bitePoint;
        }

        [Server]
        public void ServerCast(NetworkObject user)
        {
            _owner = user;

            CloneItemDefinition definition = Definition;
            float range = definition != null ? definition.CastRange : 12f;

            Vector3 aim = ResolveAim(user, out Vector3 origin);
            Vector3 destination = origin + aim * range;

            var water = CloneWaterManager.Instance;
            if (water != null && water.GetDepth(destination) < 0.15f)
            {
                ServerMiss();
                return;
            }

            if (water != null)
                destination.y = water.GetSurfaceHeight(destination);

            BitePoint = destination;
            SetState(CloneRodState.Casting);
            SpawnBobber(destination);
            SetState(CloneRodState.Waiting);

            float min = definition != null ? definition.BiteDelayMin : 2f;
            float max = definition != null ? definition.BiteDelayMax : 6f;
            _stateTimer = Random.Range(Mathf.Min(min, max), Mathf.Max(min, max));
        }

        [Server]
        private void BeginBite()
        {
            SetState(CloneRodState.Bite);

            float hold = Definition != null ? Definition.HookWindow : 1.2f;
            _stateTimer = Mathf.Max(0.25f, hold);
        }

        [Server]
        private void ServerAttemptHook()
        {
            CloneFish fish = FindFishNearBite();
            if (fish != null && fish.ServerHook(_owner))
            {
                HookedFish = fish.NetworkObject;
                SetState(CloneRodState.Hooked);
                SetState(CloneRodState.Reeling);
                _reelTimer = 0f;
                return;
            }

            int fallback = Definition != null && Random.value < 0.5f
                ? FallbackCatchItemId
                : PickRandomCatch();

            var player = _owner != null ? _owner.GetComponent<ClonePlayer>() : null;
            if (player == null || player.Inventory == null)
            {
                ServerMiss();
                return;
            }

            int stored = player.Inventory.ServerTryStore(fallback, 1, 0, 0f, out _);
            if (stored <= 0)
            {
                ServerMiss();
                return;
            }

            ConsumeBait();
            DespawnBobber();
            BitePoint = _owner != null ? _owner.transform.position : BitePoint;
            SetState(CloneRodState.Caught);
            SetState(CloneRodState.Idle);
        }

        [Server]
        private void TickReel(float deltaTime)
        {
            if (_owner == null)
            {
                ServerMiss();
                return;
            }

            _reelTimer += deltaTime;
            Vector3 destination = _owner.transform.position + Vector3.up * 1.2f;

            if (HookedFish != null)
            {
                var fish = HookedFish.GetComponent<CloneFish>();
                if (fish == null || fish.IsDead)
                {
                    ReleaseFish();
                    ServerMiss();
                    return;
                }

                fish.ServerDrag(destination, deltaTime);
                if (Vector3.Distance(fish.transform.position, destination) > CatchDistance)
                    return;

                int itemId = fish.ResolveInventoryItemId(FallbackCatchItemId);
                var player = _owner.GetComponent<ClonePlayer>();
                if (player != null && player.Inventory != null)
                    player.Inventory.ServerTryStore(itemId, 1, 0, 0f, out _);

                ReleaseFish();
            }
            else if (Vector3.Distance(BitePoint, destination) > CatchDistance)
            {
                BitePoint = Vector3.MoveTowards(BitePoint, destination, ReelPullSpeed * deltaTime);
                MoveBobber(BitePoint);
                if (_reelTimer > 12f)
                {
                    ServerMiss();
                    return;
                }

                return;
            }

            ConsumeBait();
            DespawnBobber();
            BitePoint = destination;
            SetState(CloneRodState.Caught);
            SetState(CloneRodState.Idle);
        }

        [Server]
        public void ServerReelIn(bool keepCatch)
        {
            if (!keepCatch)
                ReleaseFish();

            DespawnBobber();
            ConsumeBait();
            SetState(CloneRodState.Idle);
        }

        [Server]
        private void ServerMiss()
        {
            ReleaseFish();
            DespawnBobber();
            SetState(CloneRodState.Missed);
            _stateTimer = MissRecoverSeconds;
        }

        [Server]
        public void ServerAttachBait(int baitItemId)
        {
            BaitItemId = baitItemId;
        }

        private void ReleaseFish()
        {
            if (HookedFish == null)
                return;

            var fish = HookedFish.GetComponent<CloneFish>();
            if (fish != null)
                fish.ServerUnhook();

            HookedFish = null;
        }

        private void ConsumeBait()
        {
            if (BaitItemId == CloneInventorySlot.EmptyId)
                return;

            var player = _owner != null ? _owner.GetComponent<ClonePlayer>() : null;
            if (player != null && player.Inventory != null)
                player.Inventory.ServerConsumeBait(1);

            BaitItemId = CloneInventorySlot.EmptyId;
        }

        private CloneFish FindFishNearBite()
        {
            Collider[] hits = Physics.OverlapSphere(BitePoint, HookSearchRadius, ~0,
                QueryTriggerInteraction.Ignore);

            CloneFish best = null;
            float bestSqr = HookSearchRadius * HookSearchRadius;

            for (int i = 0; i < hits.Length; i++)
            {
                var fish = hits[i] != null ? hits[i].GetComponentInParent<CloneFish>() : null;
                if (fish == null || !fish.IsCatchable)
                    continue;

                float sqr = (fish.transform.position - BitePoint).sqrMagnitude;
                if (sqr > bestSqr)
                    continue;

                bestSqr = sqr;
                best = fish;
            }

            return best;
        }

        private int PickRandomCatch()
        {
            int[] pool =
            {
                CloneItemCatalog.Mackerel,
                CloneItemCatalog.Clam,
                CloneItemCatalog.Crab,
                CloneItemCatalog.KelpWrack,
                CloneItemCatalog.Pike
            };

            return pool[Random.Range(0, pool.Length)];
        }

        private Vector3 ResolveAim(NetworkObject user, out Vector3 origin)
        {
            origin = user != null ? user.transform.position + Vector3.up * 1.5f : transform.position;

            var player = user != null ? user.GetComponent<ClonePlayer>() : null;
            if (player == null || player.CameraRig == null)
                return transform.forward;

            Quaternion rotation = Quaternion.Euler(player.CameraRig.Pitch, player.CameraRig.Yaw, 0f);
            return rotation * Vector3.forward;
        }

        private void SpawnBobber(Vector3 position)
        {
            DespawnBobber();

            if (BobberPrefab == null)
                return;

            _bobber = Instantiate(BobberPrefab, position, Quaternion.identity);
            var networkObject = _bobber.GetComponent<NetworkObject>();
            if (networkObject != null)
                networkObject.Spawn(true);
        }

        private void MoveBobber(Vector3 position)
        {
            if (_bobber == null)
                return;

            _bobber.transform.position = position;
            var body = _bobber.GetComponent<Rigidbody>();
            if (body != null)
                body.position = position;
        }

        private void DespawnBobber()
        {
            if (_bobber == null)
                return;

            var networkObject = _bobber.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsSpawned)
                networkObject.Despawn(true);
            else
                Destroy(_bobber);

            _bobber = null;
        }

        public override void OnStopServer()
        {
            ReleaseFish();
            DespawnBobber();
            base.OnStopServer();
        }
    }
}
