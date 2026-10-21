// Reference (study-only): artifacts/rea-muck-chests-craft-save.md
// Artifact sha256: 5f9d0dc1e0e72f5493013c92d05ed9fb8628019f54f8bc02162dad0d2f20810c (MVID 8db17789-d6d5-48de-86ab-e696a5ff6b2b)
// REA evidence: ev_3ca9f23cd1304b68216cc51dc9d2e1141e425000c11c22287b39080fd4a39b1d, ev_62d154c8cc1cbb2a406332941ff436d862a3c9774351fc7007106fb12a8d32fe
// Mechanic referenced: a world chest with shared slots, opened/stored/withdrawn actions, server-side contents.
// All names, fields, constants and text below are original to this project.

using System;
using Clone.Items;
using Clone.Player;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Clone.World
{
    public class CloneChest : NetworkBehaviour
    {
        public const int ChestSlotCount = 12;
        public const float DefaultReach = 3.5f;

        public event Action ChestOpened;
        public event Action<int, int, int> ChestStored;
        public event Action<int, int, int> ChestWithdrawn;
        public event Action ChestStateRefreshed;

        [SerializeField, Min(0.5f)]
        private float interactReach = DefaultReach;

        private readonly SyncVar<bool> _open = new SyncVar<bool>(false);

        private readonly int[] _itemId = new int[ChestSlotCount];
        private readonly int[] _amount = new int[ChestSlotCount];
        private readonly int[] _skinId = new int[ChestSlotCount];
        private readonly float[] _cookness = new float[ChestSlotCount];

        private int[] _viewItemId = new int[ChestSlotCount];
        private int[] _viewAmount = new int[ChestSlotCount];
        private int[] _viewSkinId = new int[ChestSlotCount];
        private float[] _viewCookness = new float[ChestSlotCount];

        public bool IsOpen => _open.Value;
        public float InteractReach => interactReach;

        private void Awake()
        {
            InitServerSlots();
            ClearViewArrays();
        }

        private void InitServerSlots()
        {
            for (int i = 0; i < ChestSlotCount; i++)
            {
                _itemId[i] = CloneInventorySlot.EmptyId;
                _amount[i] = 0;
                _skinId[i] = 0;
                _cookness[i] = 0f;
            }
        }

        public int GetViewItemId(int slot)
        {
            return IsValidViewSlot(slot) ? _viewItemId[slot] : CloneInventorySlot.EmptyId;
        }

        public int GetViewAmount(int slot)
        {
            return IsValidViewSlot(slot) ? _viewAmount[slot] : 0;
        }

        public int GetViewSkinId(int slot)
        {
            return IsValidViewSlot(slot) ? _viewSkinId[slot] : 0;
        }

        public float GetViewCookness(int slot)
        {
            return IsValidViewSlot(slot) ? _viewCookness[slot] : 0f;
        }

        private struct LootEntry
        {
            public int ItemId;
            public int Amount;
            public float Weight;
        }

        private static readonly LootEntry[] LootTable = new LootEntry[]
        {
            new LootEntry { ItemId = 1, Amount = 1, Weight = 0.6f },
            new LootEntry { ItemId = 2, Amount = 1, Weight = 0.6f },
            new LootEntry { ItemId = 3, Amount = 1, Weight = 0.8f },
            new LootEntry { ItemId = 4, Amount = 1, Weight = 0.5f },
            new LootEntry { ItemId = 5, Amount = 1, Weight = 0.8f },
            new LootEntry { ItemId = 6, Amount = 3, Weight = 1.2f },
            new LootEntry { ItemId = 7, Amount = 2, Weight = 1.0f },
            new LootEntry { ItemId = 8, Amount = 6, Weight = 3.0f },
            new LootEntry { ItemId = 9, Amount = 4, Weight = 2.5f },
            new LootEntry { ItemId = 10, Amount = 5, Weight = 2.5f },
            new LootEntry { ItemId = 11, Amount = 3, Weight = 4.0f },
            new LootEntry { ItemId = 12, Amount = 2, Weight = 3.0f },
            new LootEntry { ItemId = 13, Amount = 2, Weight = 3.0f },
            new LootEntry { ItemId = 14, Amount = 3, Weight = 3.5f },
            new LootEntry { ItemId = 15, Amount = 2, Weight = 3.5f },
            new LootEntry { ItemId = 16, Amount = 4, Weight = 3.0f },
            new LootEntry { ItemId = 17, Amount = 5, Weight = 4.0f },
            new LootEntry { ItemId = 18, Amount = 3, Weight = 2.0f },
            new LootEntry { ItemId = 19, Amount = 2, Weight = 1.5f },
            new LootEntry { ItemId = 20, Amount = 4, Weight = 2.5f },
        };

        private bool _lootRolled;

        private void RollLoot()
        {
            float total = 0f;
            for (int i = 0; i < LootTable.Length; i++)
            {
                total += LootTable[i].Weight;
            }

            for (int slot = 0; slot < ChestSlotCount; slot++)
            {
                float pick = UnityEngine.Random.Range(0f, total);
                LootEntry chosen = LootTable[LootTable.Length - 1];
                for (int i = 0; i < LootTable.Length; i++)
                {
                    pick -= LootTable[i].Weight;
                    if (pick <= 0f)
                    {
                        chosen = LootTable[i];
                        break;
                    }
                }

                _itemId[slot] = chosen.ItemId;
                _amount[slot] = chosen.Amount;
                _skinId[slot] = 0;
                _cookness[slot] = 0f;
            }
        }

        private static bool IsValidViewSlot(int slot)
        {
            return slot >= 0 && slot < ChestSlotCount;
        }

        [ServerRpc(RequireOwnership = false)]
        public void ServerRequestOpenChest()
        {
            _open.Value = true;
            if (!_lootRolled)
            {
                _lootRolled = true;
                RollLoot();
            }
            BroadcastOpened();
            SyncChestState(-1, CloneInventorySlot.EmptyId, 0, 0);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ServerRequestStore(int playerSlotIndex, int amount)
        {
            ApplyStore(playerSlotIndex, amount);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ServerRequestWithdraw(int chestSlotIndex, int amount)
        {
            ApplyWithdraw(chestSlotIndex, amount);
        }

        [Server]
        private void ApplyStore(int playerSlotIndex, int amount)
        {
            var player = ClonePlayer.LocalPlayer;
            var inventory = player != null ? player.Inventory : null;
            if (inventory == null || amount <= 0)
                return;

            if (playerSlotIndex < 0 || playerSlotIndex >= ClonePlayerInventory.SlotCount)
                return;

            CloneInventorySlot slot = inventory.GetSlot(playerSlotIndex);
            if (slot == null || slot.IsEmpty)
                return;

            int itemId = slot.ItemId;
            int skinId = slot.SkinId;
            float cookness = slot.Cookness;

            CloneItemDefinition definition = CloneItemCatalog.Get(itemId);
            if (definition == null)
                return;

            int stackLimit = definition.Stackable ? Mathf.Max(1, definition.MaxStack) : 1;
            int available = slot.Amount;
            int targetSlot = -1;
            int moved = 0;

            for (int i = 0; i < ChestSlotCount && targetSlot == -1; i++)
            {
                if (_itemId[i] != itemId || _skinId[i] != skinId || !definition.Stackable)
                    continue;

                int space = stackLimit - _amount[i];
                if (space <= 0)
                    continue;

                targetSlot = i;
                moved = Mathf.Min(space, available);
            }

            for (int i = 0; i < ChestSlotCount && targetSlot == -1; i++)
            {
                if (_itemId[i] != CloneInventorySlot.EmptyId)
                    continue;

                targetSlot = i;
                moved = Mathf.Min(stackLimit, available);
            }

            if (targetSlot == -1 || moved <= 0)
                return;

            int removed = inventory.ServerDetach(playerSlotIndex, moved);
            if (removed <= 0)
                return;

            _itemId[targetSlot] = itemId;
            _skinId[targetSlot] = skinId;
            _cookness[targetSlot] = cookness;
            _amount[targetSlot] += Mathf.Min(removed, stackLimit - _amount[targetSlot]);

            SyncChestState(targetSlot, itemId, _amount[targetSlot], 1);
        }

        [Server]
        private void ApplyWithdraw(int chestSlotIndex, int amount)
        {
            var player = ClonePlayer.LocalPlayer;
            var inventory = player != null ? player.Inventory : null;
            if (inventory == null || amount <= 0)
                return;

            if (chestSlotIndex < 0 || chestSlotIndex >= ChestSlotCount)
                return;

            int itemId = _itemId[chestSlotIndex];
            if (itemId == CloneInventorySlot.EmptyId)
                return;

            int requested = Mathf.Min(Mathf.Max(1, amount), _amount[chestSlotIndex]);
            int stored = inventory.ServerTryStore(itemId, requested, _skinId[chestSlotIndex], _cookness[chestSlotIndex], out int remaining);
            int taken = requested - remaining;

            if (taken <= 0)
                return;

            _amount[chestSlotIndex] -= taken;
            if (_amount[chestSlotIndex] <= 0)
            {
                _itemId[chestSlotIndex] = CloneInventorySlot.EmptyId;
                _amount[chestSlotIndex] = 0;
                _skinId[chestSlotIndex] = 0;
                _cookness[chestSlotIndex] = 0f;
            }

            SyncChestState(chestSlotIndex, itemId, _amount[chestSlotIndex], 2);
        }

        [Server]
        private void SyncChestState(int changedSlot, int changedItemId, int changedAmount, byte changeKind)
        {
            BroadcastChestState(
                (int[])_itemId.Clone(),
                (int[])_amount.Clone(),
                (int[])_skinId.Clone(),
                (float[])_cookness.Clone(),
                changedSlot,
                changedItemId,
                changedAmount,
                changeKind);
        }

        [ObserversRpc]
        private void BroadcastChestState(int[] itemIds, int[] amounts, int[] skinIds, float[] cookness, int changedSlot, int changedItemId, int changedAmount, byte changeKind)
        {
            if (itemIds != null && itemIds.Length == ChestSlotCount)
                _viewItemId = itemIds;
            if (amounts != null && amounts.Length == ChestSlotCount)
                _viewAmount = amounts;
            if (skinIds != null && skinIds.Length == ChestSlotCount)
                _viewSkinId = skinIds;
            if (cookness != null && cookness.Length == ChestSlotCount)
                _viewCookness = cookness;

            if (changeKind == 1)
                ChestStored?.Invoke(changedSlot, changedItemId, changedAmount);
            else if (changeKind == 2)
                ChestWithdrawn?.Invoke(changedSlot, changedItemId, changedAmount);

            ChestStateRefreshed?.Invoke();
        }

        [ObserversRpc]
        private void BroadcastOpened()
        {
            ChestOpened?.Invoke();
            ChestStateRefreshed?.Invoke();
        }

        private void ClearViewArrays()
        {
            _viewItemId = new int[ChestSlotCount];
            _viewAmount = new int[ChestSlotCount];
            _viewSkinId = new int[ChestSlotCount];
            _viewCookness = new float[ChestSlotCount];

            for (int i = 0; i < ChestSlotCount; i++)
                _viewItemId[i] = CloneInventorySlot.EmptyId;
        }

        public static bool TryGetNearbyChest(ClonePlayer player, float reach, out CloneChest chest)
        {
            chest = null;
            if (player == null)
                return false;

            CloneChest[] chests = FindObjectsOfType<CloneChest>();
            float best = reach * reach;
            Vector3 origin = player.transform.position;

            foreach (var candidate in chests)
            {
                if (candidate == null)
                    continue;

                float distance = (candidate.transform.position - origin).sqrMagnitude;
                if (distance < best)
                {
                    best = distance;
                    chest = candidate;
                }
            }

            return chest != null;
        }
    }
}
