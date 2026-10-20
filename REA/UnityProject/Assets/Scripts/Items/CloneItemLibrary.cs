using System;
using System.Collections.Generic;
using UnityEngine;

namespace Clone.Items
{
    public class CloneItemLibrary : MonoBehaviour
    {
        public static CloneItemLibrary Instance { get; private set; }

        [Serializable]
        public struct Entry
        {
            public int ItemId;
            public GameObject Prefab;
        }

        [Header("World prefabs")]
        public List<Entry> Prefabs = new List<Entry>();

        [Header("Fallbacks")]
        public GameObject ConsumablePrefab;
        public GameObject ResourcePrefab;
        public GameObject ToolPrefab;

        private readonly Dictionary<int, GameObject> _lookup = new Dictionary<int, GameObject>();

        private void Awake()
        {
            Instance = this;
            Rebuild();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Rebuild()
        {
            _lookup.Clear();
            for (int i = 0; i < Prefabs.Count; i++)
            {
                Entry entry = Prefabs[i];
                if (entry.Prefab != null)
                    _lookup[entry.ItemId] = entry.Prefab;
            }
        }

        public void Register(int itemId, GameObject prefab)
        {
            if (prefab == null)
                return;

            Prefabs.Add(new Entry { ItemId = itemId, Prefab = prefab });
            _lookup[itemId] = prefab;
        }

        public GameObject GetPrefab(int itemId)
        {
            if (_lookup.TryGetValue(itemId, out GameObject prefab))
                return prefab;

            CloneItemDefinition definition = CloneItemCatalog.Get(itemId);
            if (definition == null)
                return null;

            switch (definition.Category)
            {
                case CloneItemCategory.Consumable:
                    return ConsumablePrefab;
                case CloneItemCategory.Resource:
                case CloneItemCategory.Bait:
                    return ResourcePrefab;
                default:
                    return ToolPrefab;
            }
        }
    }
}
