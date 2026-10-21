using Clone.Creatures;
using Clone.Player;
using FishNet.Managing;
using FishNet.Object;
using UnityEngine;

namespace Clone.World
{
    public class CloneBossDirector : MonoBehaviour
    {
        public static CloneBossDirector Instance { get; private set; }

        [SerializeField]
        private BossCloneCreature _bossPrefab;

        [SerializeField]
        private float _intervalSeconds = 240f;

        [SerializeField]
        private float _waterSpawnChance = 0.15f;

        private float _timer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (!InstanceFinder.IsServerInitialized)
            {
                return;
            }
            if (_bossPrefab == null)
            {
                return;
            }
            if (_intervalSeconds <= 0f)
            {
                return;
            }
            if (_timer < _intervalSeconds)
            {
                _timer += Time.deltaTime;
                return;
            }
            _timer = 0f;
            TrySpawnBoss();
        }

        [Server]
        private void TrySpawnBoss()
        {
            if (CloneSpawnManager.Instance == null)
            {
                return;
            }
            int playerCount = FindObjectsByType<ClonePlayer>(FindObjectsSortMode.None).Length;
            if (playerCount == 0)
            {
                return;
            }
            Vector3 position = GetSpawnPosition();
            GameObject bossObject = Instantiate(_bossPrefab.gameObject, position, Quaternion.identity);
            NetworkObject networkObject = bossObject.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Destroy(bossObject);
                return;
            }
            InstanceFinder.ServerManager.Spawn(networkObject.gameObject);
        }

        private Vector3 GetSpawnPosition()
        {
            if (_waterSpawnChance > 0f && Random.value < _waterSpawnChance)
            {
                return GetWaterSpawnPosition();
            }
            return CloneSpawnManager.Instance.GetRandomIslandSpawnPos();
        }

        private Vector3 GetWaterSpawnPosition()
        {
            Vector3 center = CloneSpawnManager.Instance.DefaultSpawn;
            float angle = Random.value * Mathf.PI * 2f;
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            float distance = Random.Range(35f, 55f);
            return new Vector3(direction.x * distance, 0.5f, direction.z * distance);
        }

        [Server]
        public void ForceSpawnBoss()
        {
            TrySpawnBoss();
        }

        public void SetInterval(float seconds)
        {
            _intervalSeconds = Mathf.Max(1f, seconds);
        }

        public float GetInterval()
        {
            return _intervalSeconds;
        }

        public float GetTimer()
        {
            return _timer;
        }
    }
}
