using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Clone.World
{
    public enum CloneHarvestKind
    {
        Wood = 0,
        Stone = 1,
        Fiber = 2
    }

    // Server-authoritative resource node. Damage it until depleted; grants a
    // one-time yield, then regrows after a delay. Visuals follow the SyncVar.
    public class CloneHarvestNode : NetworkBehaviour
    {
        public CloneHarvestKind Kind = CloneHarvestKind.Wood;
        public float MaxHealth = 100f;
        public int YieldItemId;
        public int YieldAmount = 3;
        public float RespawnDelay = 45f;

        private readonly SyncVar<float> _health = new SyncVar<float>(100f);
        private readonly SyncVar<bool> _depleted = new SyncVar<bool>(false);

        private int _remainingYield;
        private Renderer _renderer;
        private Collider _collider;

        public bool IsDepleted => _depleted.Value;

        private void Awake()
        {
            _renderer = GetComponentInChildren<Renderer>();
            _collider = GetComponentInChildren<Collider>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            _remainingYield = YieldAmount;
            ServerRegrow();
        }

        [Server]
        public void ServerHarvest(float power)
        {
            if (_depleted.Value || power <= 0f)
                return;

            float health = Mathf.Max(0f, _health.Value - power);
            _health.Value = health;

            if (health <= 0f)
                ServerDeplete();
        }

        [Server]
        private void ServerDeplete()
        {
            _depleted.Value = true;
            Invoke(nameof(ServerRegrow), Mathf.Max(1f, RespawnDelay));
        }

        [Server]
        public bool ServerTryTake(out int itemId, out int amount)
        {
            itemId = -1;
            amount = 0;

            if (!_depleted.Value || YieldItemId <= 0 || _remainingYield <= 0)
                return false;

            itemId = YieldItemId;
            amount = _remainingYield;
            _remainingYield = 0;
            return true;
        }

        [Server]
        private void ServerRegrow()
        {
            _health.Value = MaxHealth;
            _remainingYield = Mathf.Max(1, YieldAmount);
            _depleted.Value = false;
        }

        private void Update()
        {
            bool depleted = _depleted.Value;
            if (_renderer != null && _renderer.enabled == depleted)
                _renderer.enabled = !depleted;
            if (_collider != null)
                _collider.enabled = true;
        }
    }
}
