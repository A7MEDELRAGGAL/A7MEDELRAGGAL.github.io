using System;
using System.Collections.Generic;
using Clone.Items;
using Clone.Player;

namespace Clone.Search
{
    [Serializable]
    public class CloneItemSearchFilter
    {
        public string Query;
        public CloneItemCategory? Category;
        public bool? Stackable;
        public int? MinAmount;
        public int? MaxAmount;
        public bool IncludeEmptySlots;
    }

    public static class CloneInventorySearch
    {
        public static List<int> FilterSlots(ClonePlayerInventory inventory, CloneItemSearchFilter filter)
        {
            var matches = new List<int>();
            if (inventory == null)
                return matches;

            for (int i = 0; i < inventory.SlotCount; i++)
            {
                var slot = inventory.GetSlot(i);
                if (slot == null)
                    continue;

                if (slot.IsEmpty && !filter.IncludeEmptySlots)
                    continue;

                if (!Matches(slot, filter))
                    continue;

                matches.Add(i);
            }

            return matches;
        }

        public static List<int> FilterCatalog(CloneItemSearchFilter filter)
        {
            var matches = new List<int>();
            foreach (var def in CloneItemCatalog.All)
            {
                if (!Matches(def, filter))
                    continue;
                matches.Add(def.Id);
            }
            return matches;
        }

        private static bool Matches(CloneInventorySlot slot, CloneItemSearchFilter filter)
        {
            if (string.IsNullOrEmpty(filter.Query))
            {
                if (filter.Category.HasValue && slot.Category != filter.Category.Value)
                    return false;
                if (filter.Stackable.HasValue)
                {
                    var def = CloneItemCatalog.Get(slot.ItemId);
                    if (def == null || def.Stackable != filter.Stackable.Value)
                        return false;
                }
                if (filter.MinAmount.HasValue && slot.Amount < filter.MinAmount.Value)
                    return false;
                if (filter.MaxAmount.HasValue && slot.Amount > filter.MaxAmount.Value)
                    return false;
                return true;
            }

            var query = filter.Query.ToLowerInvariant();
            var def2 = CloneItemCatalog.Get(slot.ItemId);
            if (def2 == null)
                return false;

            if (!def2.Name.ToLowerInvariant().Contains(query))
                return false;

            if (filter.Category.HasValue && slot.Category != filter.Category.Value)
                return false;
            if (filter.Stackable.HasValue && def2.Stackable != filter.Stackable.Value)
                return false;
            if (filter.MinAmount.HasValue && slot.Amount < filter.MinAmount.Value)
                return false;
            if (filter.MaxAmount.HasValue && slot.Amount > filter.MaxAmount.Value)
                return false;

            return true;
        }

        private static bool Matches(CloneItemDefinition def, CloneItemSearchFilter filter)
        {
            if (!string.IsNullOrEmpty(filter.Query))
            {
                var query = filter.Query.ToLowerInvariant();
                if (!def.Name.ToLowerInvariant().Contains(query))
                    return false;
            }

            if (filter.Category.HasValue && def.Category != filter.Category.Value)
                return false;
            if (filter.Stackable.HasValue && def.Stackable != filter.Stackable.Value)
                return false;
            if (filter.MinAmount.HasValue && def.MaxStack < filter.MinAmount.Value)
                return false;
            if (filter.MaxAmount.HasValue && def.MaxStack > filter.MaxAmount.Value)
                return false;

            return true;
        }
    }
}
