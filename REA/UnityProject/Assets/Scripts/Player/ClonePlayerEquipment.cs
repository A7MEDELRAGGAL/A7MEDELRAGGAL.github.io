using Clone.Fps;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Clone.Items;
using UnityEngine;

namespace Clone.Player
{
    // Server-authoritative worn gear. Gear items equip directly from the
    // inventory (crafting auto-equips); slots hold item ids, stats are read
    // from the catalog.
    public class ClonePlayerEquipment : NetworkBehaviour
    {
        public const int Helm = 0;
        public const int Chest = 1;
        public const int Legs = 2;
        public const int Boots = 3;
        public const int SlotCount = 4;

        private readonly SyncVar<int> _helm = new SyncVar<int>(CloneInventorySlot.EmptyId);
        private readonly SyncVar<int> _chest = new SyncVar<int>(CloneInventorySlot.EmptyId);
        private readonly SyncVar<int> _legs = new SyncVar<int>(CloneInventorySlot.EmptyId);
        private readonly SyncVar<int> _boots = new SyncVar<int>(CloneInventorySlot.EmptyId);

        public int Helm => _helm.Value;
        public int Chest => _chest.Value;
        public int Legs => _legs.Value;
        public int Boots => _boots.Value;

        public int GetSlotItemId(int slot)
        {
            return slot switch
            {
                0 => _helm.Value,
                1 => _chest.Value,
                2 => _legs.Value,
                3 => _boots.Value,
                _ => CloneInventorySlot.EmptyId
            };
        }

        private SyncVar<int> SlotSync(int slot)
        {
            return slot switch
            {
                Helm => _helm,
                Chest => _chest,
                Legs => _legs,
                Boots => _boots,
                _ => null
            };
        }

        public float TotalProtection
        {
            get
            {
                float total = 0f;
                for (int slot = 0; slot < SlotCount; slot++)
                {
                    CloneItemDefinition definition = CloneItemCatalog.Get(GetSlotItemId(slot));
                    if (definition != null)
                        total += definition.Protection;
                }
                return total;
            }
        }

        public float TotalAgility
        {
            get
            {
                float total = 0f;
                for (int slot = 0; slot < SlotCount; slot++)
                {
                    CloneItemDefinition definition = CloneItemCatalog.Get(GetSlotItemId(slot));
                    if (definition != null)
                        total += definition.Agility;
                }
                return total;
            }
        }

        public float EncumbranceFactor
        {
            get
            {
                float factor = 0f;
                for (int slot = 0; slot < SlotCount; slot++)
                {
                    CloneItemDefinition definition = CloneItemCatalog.Get(GetSlotItemId(slot));
                    if (definition != null && definition.Weight > 0f)
                        factor += Mathf.Min(0.1f, definition.Weight * 0.02f);
                }
                return 1f - Mathf.Min(0.4f, factor);
            }
        }

        public float SpeedFactor => EncumbranceFactor * (1f + TotalAgility);

        private FpsPlayerMotor _motor;
        private ClonePlayerMovement _movement;

        private void Update()
        {
            if (_motor == null)
                _motor = GetComponentInParent<FpsPlayerMotor>();
            if (_motor != null)
                _motor.SpeedScale = EncumbranceFactor;

            if (_movement == null)
                _movement = GetComponentInParent<ClonePlayerMovement>();
            if (_movement != null)
                _movement.SpeedScale = EncumbranceFactor;
        }

        [ServerRpc]
        public void ServerRequestEquip(int itemId)
        {
            ServerEquip(itemId);
        }

        [Server]
        public void ServerEquip(int itemId)
        {
            ServerEquip(itemId, true);
        }

        [Server]
        public void ServerEquip(int itemId, bool fromInventory)
        {
            if (itemId == CloneInventorySlot.EmptyId)
                return;

            CloneItemDefinition definition = CloneItemCatalog.GetById(itemId);
            if (definition == null || definition.Category != CloneItemCategory.Gear)
                return;

            ClonePlayerInventory inventory = GetComponent<ClonePlayerInventory>();
            if (inventory == null)
                return;

            if (fromInventory && !inventory.ServerHasItems(itemId, 1))
                return;

            int slot = SlotForItem(itemId);
            if (slot < 0)
                return;

            SyncVar<int> sync = SlotSync(slot);
            if (sync == null)
                return;

            sync.Value = itemId;
            if (fromInventory)
                inventory.ServerConsume(itemId, 1);
        }

        [ServerRpc]
        public void ServerRequestUnequip(int slot)
        {
            ServerUnequip(slot);
        }

        [Server]
        public void ServerUnequip(int slot)
        {
            if (slot < 0 || slot >= SlotCount)
                return;

            SyncVar<int> sync = SlotSync(slot);
            if (sync == null)
                return;

            int itemId = sync.Value;
            if (itemId == CloneInventorySlot.EmptyId)
                return;

            ClonePlayerInventory inventory = GetComponent<ClonePlayerInventory>();
            if (inventory == null)
                return;

            if (inventory.ServerTryStore(itemId, 1, 0, 0f, out int remaining) && remaining == 0)
                sync.Value = CloneInventorySlot.EmptyId;
        }

        [Server]
        public void ServerApplySavedSlot(int slot, int itemId)
        {
            if (slot < 0 || slot >= SlotCount)
                return;

            SyncVar<int> sync = SlotSync(slot);
            if (sync == null)
                return;

            sync.Value = itemId > 0 ? itemId : CloneInventorySlot.EmptyId;
        }

        private static int SlotForItem(int itemId)
        {
            switch (itemId)
            {
                case CloneItemCatalog.ArmorWoodenHelm:
                    return Helm;
                case CloneItemCatalog.ArmorWoodenChest:
                    return Chest;
                case CloneItemCatalog.ArmorWoodenLegs:
                    return Legs;
                case CloneItemCatalog.ArmorWoodenBoots:
                    return Boots;
                default:
                    return -1;
            }
        }
    }
}
