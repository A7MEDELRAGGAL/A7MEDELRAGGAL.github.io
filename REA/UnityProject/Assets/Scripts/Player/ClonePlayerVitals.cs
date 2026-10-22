using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Clone.Player
{
    public class ClonePlayerVitals : NetworkBehaviour
    {
        public const float MaxHealth = 100f;
        public const float MaxFullness = 100f;

        private readonly SyncVar<float> _health = new SyncVar<float>(MaxHealth);
        private readonly SyncVar<float> _fullness = new SyncVar<float>(MaxFullness);
        private readonly SyncVar<float> _poison = new SyncVar<float>(0f);
        private readonly SyncVar<float> _fire = new SyncVar<float>(0f);

        public float Health => _health.Value;
        public float Fullness => _fullness.Value;
        public float Poison => _poison.Value;
        public float Fire => _fire.Value;
        public bool IsDead => Health <= 0f;

        public override void OnStartServer()
        {
            base.OnStartServer();
            Invoke(nameof(ServerTickVitals), 1f);
        }

        private void ServerTickVitals()
        {
            if (!IsServerStarted)
                return;

            float fullness = Mathf.Max(0f, _fullness.Value - 0.05f);
            _fullness.Value = fullness;

            if (fullness >= 60f)
                ServerHeal(0.5f);

            if (fullness <= 0f)
                ServerDamage(0.5f);

            if (_fire.Value > 0f)
            {
                _fire.Value = Mathf.Max(0f, _fire.Value - 1f);
                ServerDamage(2f);
            }

            if (_poison.Value > 0f)
            {
                _poison.Value = Mathf.Max(0f, _poison.Value - 1f);
                ServerDamage(1f);
            }

            Invoke(nameof(ServerTickVitals), 1f);
        }

        [Server]
        public void ServerDamage(float amount)
        {
            if (IsDead || amount <= 0f)
                return;

            var equipment = GetComponent<ClonePlayerEquipment>();
            float mitigation = equipment != null ? Mathf.Clamp01(equipment.TotalProtection) : 0f;
            _health.Value = Mathf.Max(0f, _health.Value - amount * (1f - mitigation));
            if (IsDead)
                ServerRespawnOrResurrect();
        }

        [Server]
        public void ServerHeal(float amount)
        {
            if (IsDead || amount <= 0f)
                return;
            _health.Value = Mathf.Min(MaxHealth, _health.Value + amount);
        }

        [Server]
        public void ServerFeed(float amount)
        {
            _fullness.Value = Mathf.Min(MaxFullness, Mathf.Max(0f, _fullness.Value + amount));
        }

        [Server]
        public void ServerApplyPoison(float amount)
        {
            _poison.Value = Mathf.Max(0f, _poison.Value + amount);
        }

        [Server]
        public void ServerApplyFire(float amount)
        {
            _fire.Value = Mathf.Max(0f, _fire.Value + amount);
        }

        [Server]
        public void ServerCure(float poison, float fire)
        {
            if (poison > 0f)
                _poison.Value = Mathf.Max(0f, _poison.Value - poison);
            if (fire > 0f)
                _fire.Value = Mathf.Max(0f, _fire.Value - fire);
        }

        [Server]
        public void ServerRestoreAll(float health, float fullness, float poison, float fire)
        {
            _health.Value = Mathf.Clamp(health, 0f, MaxHealth);
            _fullness.Value = Mathf.Clamp(fullness, 0f, MaxFullness);
            _poison.Value = Mathf.Max(0f, poison);
            _fire.Value = Mathf.Max(0f, fire);
        }

        [Server]
        private void ServerRespawnOrResurrect()
        {
            Clone.World.CloneRunManager.Instance?.NotifyPlayerDeath();
            _health.Value = MaxHealth * 0.5f;
            _fullness.Value = MaxFullness * 0.5f;
            _poison.Value = 0f;
            _fire.Value = 0f;

            var player = GetComponent<ClonePlayer>();
            if (player != null)
                player.TeleportTarget(SpawnPointUtility.GetRespawnPosition());
        }

        public void NotifyHit(NetworkObject attacker, int damage)
        {
            Debug.Log($"Hit for {damage} from {(attacker != null ? attacker.name : "unknown")}");
        }
    }

    public static class SpawnPointUtility
    {
        public static Vector3 GetRespawnPosition()
        {
            if (Clone.World.CloneSpawnManager.Instance != null)
                return Clone.World.CloneSpawnManager.Instance.GetRandomIslandSpawnPos();

            var container = GameObject.Find("PlayerSpawnPoints");
            if (container != null && container.transform.childCount > 0)
            {
                Transform point = container.transform.GetChild(0);
                return point.position;
            }
            return new Vector3(0f, 2f, 0f);
        }
    }
}
