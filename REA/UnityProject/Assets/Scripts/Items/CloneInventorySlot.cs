using System;

namespace Clone.Items
{
    [Serializable]
    public class CloneInventorySlot
    {
        public const int EmptyId = -1;

        public int ItemId = EmptyId;
        public int Amount;
        public int SkinId;
        public float Cookness;

        public bool IsEmpty => ItemId == EmptyId || Amount <= 0;

        public CloneItemDefinition Definition => IsEmpty ? null : CloneItemCatalog.Get(ItemId);

        public CloneItemCategory Category => Definition != null ? Definition.Category : CloneItemCategory.Resource;

        public float Weight => IsEmpty ? 0f : Definition.TotalWeight(Amount);

        public bool IsStackableWith(CloneInventorySlot other)
        {
            if (other == null || other.IsEmpty || IsEmpty)
                return false;

            if (other.ItemId != ItemId || other.SkinId != SkinId)
                return false;

            CloneItemDefinition definition = Definition;
            return definition != null && definition.Stackable && other.Amount < definition.MaxStack;
        }

        public void Clear()
        {
            ItemId = EmptyId;
            Amount = 0;
            SkinId = 0;
            Cookness = 0f;
        }

        public void Set(int itemId, int amount, int skinId = 0, float cookness = 0f)
        {
            ItemId = itemId;
            Amount = Max(0, amount);
            SkinId = skinId;
            Cookness = cookness;
        }

        public int Add(int amount, out int remaining)
        {
            if (IsEmpty || amount <= 0)
            {
                remaining = amount;
                return 0;
            }

            CloneItemDefinition definition = Definition;
            int capacity = definition != null && definition.Stackable
                ? Max(0, definition.MaxStack - Amount)
                : 0;

            int added = Min(amount, capacity);
            Amount += added;
            remaining = amount - added;
            return added;
        }

        public int Remove(int amount)
        {
            int removed = Min(amount, Amount);
            Amount -= removed;
            if (Amount <= 0)
                Clear();
            return removed;
        }

        public void Cook(float amount)
        {
            if (IsEmpty)
                return;
            Cookness = System.Math.Min(1f, Cookness + amount);
        }

        private static int Max(int a, int b)
        {
            return a > b ? a : b;
        }

        private static int Min(int a, int b)
        {
            return a < b ? a : b;
        }
    }

    [Serializable]
    public struct CloneSlotInfo
    {
        public int ItemId;
        public int Amount;
        public int SkinId;
        public float Cookness;

        public static CloneSlotInfo FromSlot(CloneInventorySlot slot)
        {
            return new CloneSlotInfo
            {
                ItemId = slot != null ? slot.ItemId : CloneInventorySlot.EmptyId,
                Amount = slot != null ? slot.Amount : 0,
                SkinId = slot != null ? slot.SkinId : 0,
                Cookness = slot != null ? slot.Cookness : 0f
            };
        }

        public void ApplyTo(CloneInventorySlot slot)
        {
            if (slot == null)
                return;

            if (ItemId == CloneInventorySlot.EmptyId || Amount <= 0)
            {
                slot.Clear();
                return;
            }

            slot.Set(ItemId, Amount, SkinId, Cookness);
        }
    }
}
