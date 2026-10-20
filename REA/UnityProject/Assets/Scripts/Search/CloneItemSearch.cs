using System.Collections.Generic;
using UnityEngine;
using Clone.Items;
using Clone.Player;

namespace Clone.Search
{
    public class CloneItemSearch : MonoBehaviour
    {
        [SerializeField]
        private ClonePlayerInventory targetInventory;

        public ClonePlayerInventory TargetInventory
        {
            get => targetInventory;
            set => targetInventory = value;
        }

        public List<int> SearchInventory(string query)
        {
            var filter = new CloneItemSearchFilter { Query = query, IncludeEmptySlots = false };
            return CloneInventorySearch.FilterSlots(targetInventory, filter);
        }

        public List<int> SearchInventoryByCategory(CloneItemCategory category)
        {
            var filter = new CloneItemSearchFilter { Category = category, IncludeEmptySlots = false };
            return CloneInventorySearch.FilterSlots(targetInventory, filter);
        }

        public List<int> SearchCatalog(string query)
        {
            var filter = new CloneItemSearchFilter { Query = query };
            return CloneInventorySearch.FilterCatalog(filter);
        }

        private void Awake()
        {
            if (targetInventory == null)
                targetInventory = GetComponent<ClonePlayerInventory>();
        }
    }
}
