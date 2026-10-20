using System.Collections.Generic;
using UnityEngine;

namespace Clone.World
{
    public class CloneSpawnManager : MonoBehaviour
    {
        public static CloneSpawnManager Instance { get; private set; }

        [Header("Roots")]
        public Transform PlayerSpawnPoints;
        public Transform IslandRoots;

        [Header("Placement")]
        public float SpawnRadius = 6f;
        public float GroundProbe = 40f;
        public float RayMask = ~0;

        [Header("Fallbacks")]
        public Vector3 DefaultSpawn = new Vector3(0f, 2f, 0f);
        public int MaxAttempts = 12;

        private readonly List<Transform> _players = new List<Transform>();
        private readonly List<Transform> _islands = new List<Transform>();
        private int _cursor;

        private void Awake()
        {
            Instance = this;
            Collect();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public IReadOnlyList<Transform> Islands => _islands;
        public int PlayerSpawnCount => _players.Count;

        public Vector3 GetPlayerSpawnPos(int index = -1)
        {
            if (_players.Count == 0)
                return ProbeGround(DefaultSpawn);

            if (index >= 0)
                return ProbeGround(_players[index % _players.Count].position);

            Transform point = _players[_cursor % _players.Count];
            _cursor++;
            return ProbeGround(Scatter(point.position));
        }

        public Vector3 GetIslandSpawnPos(int islandIndex)
        {
            if (_islands.Count == 0)
                return GetPlayerSpawnPos();

            Transform island = _islands[Mathf.Abs(islandIndex) % _islands.Count];
            return ProbeGround(island.position + island.forward * (island.localScale.z * 2f));
        }

        public Vector3 GetRandomIslandSpawnPos()
        {
            return _islands.Count > 0
                ? GetIslandSpawnPos(Random.Range(0, _islands.Count))
                : GetPlayerSpawnPos();
        }

        public void RegisterPlayerPoint(Transform point)
        {
            if (point != null && !_players.Contains(point))
                _players.Add(point);
        }

        public void RegisterIsland(Transform island)
        {
            if (island != null && !_islands.Contains(island))
                _islands.Add(island);
        }

        public void Collect()
        {
            _players.Clear();
            _islands.Clear();

            if (PlayerSpawnPoints != null)
            {
                for (int i = 0; i < PlayerSpawnPoints.childCount; i++)
                    _players.Add(PlayerSpawnPoints.GetChild(i));
            }

            if (IslandRoots != null)
            {
                for (int i = 0; i < IslandRoots.childCount; i++)
                    _islands.Add(IslandRoots.GetChild(i));
            }
        }

        private Vector3 Scatter(Vector3 origin)
        {
            if (SpawnRadius <= 0f)
                return origin;

            Vector2 offset = Random.insideUnitCircle * SpawnRadius;
            return origin + new Vector3(offset.x, 0f, offset.y);
        }

        private Vector3 ProbeGround(Vector3 candidate)
        {
            Vector3 rayOrigin = candidate + Vector3.up * GroundProbe;
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit,
                    GroundProbe * 2f, RayMask, QueryTriggerInteraction.Ignore))
            {
                return hit.point + Vector3.up * 0.1f;
            }

            return candidate;
        }
    }
}
