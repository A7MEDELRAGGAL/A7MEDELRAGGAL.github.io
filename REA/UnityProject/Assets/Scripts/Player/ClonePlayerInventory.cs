using Clone.Items;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Clone.Player
{
    public class ClonePlayerInventory : NetworkBehaviour
    {
        public const int SlotCount = 24;
        public const int HotbarCount = 9;
        public const float MaxCarryWeight = 140f;

        private readonly SyncVar<int> _selectedSlot = new SyncVar<int>(0);
        private readonly SyncVar<int> _baitId = new SyncVar<int>(CloneInventorySlot.EmptyId);
        private readonly SyncVar<int> _unlockedSlots = new SyncVar<int>(HotbarCount);
        private readonly SyncVar<float> _carryWeight = new SyncVar<float>(0f);
        private readonly SyncVar<int> _revision = new SyncVar<int>(0);
        private readonly SyncVar<NetworkObject> _heldRef = new SyncVar<NetworkObject>(null);

        private readonly CloneInventorySlot[] _slots = new CloneInventorySlot[SlotCount];

        private CloneItem _heldItem;
        private int _sentRevision = -1;

        public int SelectedSlot => _selectedSlot.Value;
        public int BaitId => _baitId.Value;
        public int UnlockedSlots => _unlockedSlots.Value;
        public float CarryWeight => _carryWeight.Value;
        public float WeightCapacity01 => Mathf.Clamp01(_carryWeight.Value / MaxCarryWeight);
        public bool IsOverweight => _carryWeight.Value > MaxCarryWeight;

        public CloneItem HeldItem
        {
            get
            {
                if (_heldItem != null)
                    return _heldItem;

                return _heldRef.Value != null ? _heldRef.Value.GetComponent<CloneItem>() : null;
            }
        }

        public CloneInventorySlot this[int index] => GetSlot(index);

        private void Awake()
        {
            for (int i = 0; i < _slots.Length; i++)
                _slots[i] = new CloneInventorySlot();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (IsServer)
                return;

            RequestSlotSnapshot();
        }

        private void Update()
        {
            var player = GetComponent<ClonePlayer>();
            if (player != null && !player.IsOwner)
                return;

            HandleHotbar();
            HandleActions();

            if (!IsServer)
                return;

            if (_sentRevision != _revision.Value)
                SyncSlots();
        }

        public CloneInventorySlot GetSlot(int index)
        {
            return index >= 0 && index < _slots.Length ? _slots[index] : null;
        }

        public CloneInventorySlot SelectedSlotData => GetSlot(SelectedSlot);

        public bool IsSlotUnlocked(int index)
        {
            return index >= 0 && index < UnlockedSlots;
        }

        public int FirstEmptySlot()
        {
            int limit = Mathf.Min(UnlockedSlots, _slots.Length);
            for (int i = 0; i < limit; i++)
            {
                if (_slots[i].IsEmpty)
                    return i;
            }

            return -1;
        }

        private void HandleHotbar()
        {
            for (int i = 0; i < HotbarCount && i < SlotCount; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                    SelectSlot(i);
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) < 0.01f)
                return;

            int step = scroll > 0f ? -1 : 1;
            SelectSlot((SelectedSlot + step + SlotCount) % SlotCount);
        }

        private void HandleActions()
        {
            if (Input.GetMouseButtonDown(0))
                ServerRequestUse();
            if (Input.GetKeyDown(KeyCode.G))
                ServerRequestDrop(SelectedSlot);
            if (Input.GetKeyDown(KeyCode.R))
                ServerRequestConsume(SelectedSlot);
            if (Input.GetKeyDown(KeyCode.B))
                ServerRequestSetBaitFromSelected();
        }

        public void SelectSlot(int slot)
        {
            if (slot < 0 || slot >= SlotCount)
                return;

            if (IsServer)
                ApplySelectSlot(slot);
            else
                ServerRequestSelectSlot(slot);
        }

        [Server]
        public void ApplySelectSlot(int slot)
        {
            if (slot < 0 || slot >= SlotCount)
                return;

            _selectedSlot.Value = slot;
            RefreshHeldItem();
        }

[ServerRpc]
    private void ServerRequestSelectSlot(int slot)
    {
        if (!IsOwner) return;
        ApplySelectSlot(slot);
    }

        [ServerRpc]
        private void ServerRequestSlotSnapshot()
        {
            if (!IsOwner)
                return;

            SlotSnapshotTarget(BuildItemIds(), BuildAmounts(), BuildSkins(), BuildCookness(), _revision.Value);
        }

        [TargetRpc]
        private void SlotSnapshotTarget(int[] itemIds, int[] amounts, int[] skins, float[] cookness, int revision)
        {
            ApplySlotArrays(itemIds, amounts, skins, cookness);
            _sentRevision = revision;
        }

        [Server]
        public int ServerTryStore(int itemId, int amount, int skinId, float cookness, out int remaining)
        {
            remaining = Mathf.Max(0, amount);

            if (itemId == CloneInventorySlot.EmptyId || amount <= 0)
                return 0;

            CloneItemDefinition definition = CloneItemCatalog.Get(itemId);
            if (definition == null)
                return 0;

            int added = 0;
            int limit = Mathf.Min(UnlockedSlots, _slots.Length);
            int stackLimit = definition.Stackable ? Mathf.Max(1, definition.MaxStack) : 1;
            float runningWeight = _carryWeight.Value;
            bool capacityExceeded = false;

            for (int i = 0; i < limit && added < amount && !capacityExceeded; i++)
            {
                CloneInventorySlot slot = _slots[i];
                if (slot.IsEmpty || slot.ItemId != itemId || slot.SkinId != skinId || !definition.Stackable)
                    continue;

                int space = stackLimit - slot.Amount;
                if (space <= 0)
                    continue;

                int moved = Mathf.Min(space, amount - added);
                float weight = definition.TotalWeight(moved);
                if (runningWeight + weight > MaxCarryWeight)
                {
                    capacityExceeded = true;
                    break;
                }

                slot.Add(moved, out _);
                runningWeight += weight;
                added += moved;
            }

            for (int i = 0; i < limit && added < amount && !capacityExceeded; i++)
            {
                CloneInventorySlot slot = _slots[i];
                if (!slot.IsEmpty)
                    continue;

                int moved = Mathf.Min(stackLimit, amount - added);
                float weight = definition.TotalWeight(moved);
                if (runningWeight + weight > MaxCarryWeight)
                {
                    capacityExceeded = true;
                    break;
                }

                slot.Set(itemId, moved, skinId, cookness);
                runningWeight += weight;
                added += moved;
            }

            remaining = Mathf.Max(0, amount - added);
            if (added > 0)
                CommitMutation();
            return added;
        }

        [Server]
        public void ServerConsumeBait(int count)
        {
            if (_baitId.Value == CloneInventorySlot.EmptyId || count <= 0)
                return;

            int remaining = count;
            for (int i = 0; i < _slots.Length && remaining > 0; i++)
            {
                CloneInventorySlot slot = _slots[i];
                if (slot.IsEmpty || slot.ItemId != _baitId.Value || slot.Category != CloneItemCategory.Bait)
                    continue;

                remaining -= slot.Remove(remaining);
            }

            if (remaining > 0)
                _baitId.Value = CloneInventorySlot.EmptyId;

            CommitMutation();
        }

        [ServerRpc]
		public void ServerRequestSetBait(int itemId)
		{
			if (!IsOwner)
				return;

            ApplySetBait(itemId);
        }

        [Server]
        public void ApplySetBait(int itemId)
        {
            if (itemId == CloneInventorySlot.EmptyId)
            {
                _baitId.Value = CloneInventorySlot.EmptyId;
                CommitMutation();
                return;
            }

            CloneItemDefinition definition = CloneItemCatalog.Get(itemId);
            if (definition == null || !definition.IsBait)
                return;

            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].IsEmpty || _slots[i].ItemId != itemId)
                    continue;

                _baitId.Value = itemId;
                CommitMutation();
                return;
            }
        }

        [ServerRpc]
        private void ServerRequestSetBaitFromSelected()
        {
            CloneInventorySlot selected = SelectedSlotData;
            ApplySetBait(selected != null && !selected.IsEmpty
                ? selected.ItemId
                : CloneInventorySlot.EmptyId);
        }

        [ServerRpc]
        public void ServerRequestPickup(NetworkObject item)
        {
        if (!IsOwner)
            return;

        if (item == null)
            return;

        var worldItem = item.GetComponent<CloneItem>();
            if (worldItem != null && worldItem.IsSpawned)
                worldItem.ServerPickup(NetworkObject);
        }

[ServerRpc]
    private void ServerRequestDrop(int slot)
    {
        if (!IsOwner) return;

        CloneInventorySlot data = GetSlot(slot);
            if (data == null || data.IsEmpty)
                return;

            GameObject prefab = CloneItemLibrary.Instance != null
                ? CloneItemLibrary.Instance.GetPrefab(data.ItemId)
                : null;

            if (prefab != null)
            {
                Vector3 forward = transform.forward;
                Vector3 origin = transform.position + forward * 1.2f + Vector3.up * 0.6f;
                var spawned = CloneItem.SpawnServer(prefab, data.ItemId, data.Amount, origin,
                    Quaternion.LookRotation(forward), data.SkinId, data.Cookness);
                if (spawned != null)
                    data.Clear();
            }

            CommitMutation();
        }

        [ServerRpc]
        public void ServerRequestUse()
        {
            if (!IsOwner) return;
            if (_heldItem == null || _heldItem.IsTaken)
                return;

            var tool = _heldItem as CloneTool;
            if (tool != null && tool.CanUse(NetworkObject))
                tool.ServerUse(NetworkObject);
        }

        [ServerRpc]
        public void ServerRequestConsume(int slot)
        {
            var player = GetComponent<ClonePlayer>();
            CloneInventorySlot data = GetSlot(slot);
            if (player == null || data == null || data.IsEmpty)
                return;

            CloneItemDefinition definition = data.Definition;
            if (definition == null || !definition.IsEdibleOrMedical)
                return;

            if (player.Vitals == null)
                return;

            if (definition.FullnessRestore > 0f)
                player.Vitals.ServerFeed(definition.FullnessRestore);
            if (definition.HealthRestore > 0f)
                player.Vitals.ServerHeal(definition.HealthRestore);
            if (definition.PoisonCure > 0f || definition.FireCure > 0f)
                player.Vitals.ServerCure(definition.PoisonCure, definition.FireCure);

            data.Remove(1);
            CommitMutation();
        }

        [Server]
        public void ServerUnlockSlot(int slot)
        {
            if (slot < 0 || slot >= SlotCount || slot < UnlockedSlots)
                return;

            _unlockedSlots.Value = slot + 1;
            CommitMutation();
        }

        [Server]
        public int ServerDetach(int slotIndex, int amount)
        {
            if (slotIndex < 0 || slotIndex >= SlotCount || amount <= 0)
                return 0;

            CloneInventorySlot slot = _slots[slotIndex];
            if (slot.IsEmpty)
                return 0;

            int removed = slot.Remove(amount);
            if (removed > 0)
                CommitMutation();

            return removed;
        }

        [Server]
        private void CommitMutation()
        {
            RecalculateWeight();
            RefreshHeldItem();
            _revision.Value = _revision.Value + 1;
            SyncSlots();
        }

        [Server]
        private void RecalculateWeight()
        {
            float total = 0f;
            for (int i = 0; i < _slots.Length; i++)
                total += _slots[i].Weight;

            _carryWeight.Value = total;
        }

        [Server]
        private void RefreshHeldItem()
        {
            CloneInventorySlot data = SelectedSlotData;

            if (data == null || data.IsEmpty)
            {
                DespawnHeldItem();
                return;
            }

			if (_heldItem != null && _heldItem.ItemId == data.ItemId && _heldItem.SkinId == data.SkinId && _heldItem.Cookness == data.Cookness && !_heldItem.IsTaken)
				return;

            DespawnHeldItem();

            GameObject prefab = CloneItemLibrary.Instance != null
                ? CloneItemLibrary.Instance.GetPrefab(data.ItemId)
                : null;

            if (prefab == null)
                return;

            Vector3 origin = transform.position + transform.forward * 0.6f;
            _heldItem = CloneItem.SpawnServer(prefab, data.ItemId, 1, origin, transform.rotation,
                data.SkinId, data.Cookness);

            if (_heldItem == null)
            {
                _heldRef.Value = null;
                return;
            }

            var holding = GetComponent<ClonePlayerHolding>();
            _heldRef.Value = _heldItem.NetworkObject;
            _heldItem.transform.SetParent(holding != null && holding.HoldAnchor != null
                ? holding.HoldAnchor
                : transform, true);
        }

        [Server]
        private void DespawnHeldItem()
        {
            if (_heldItem == null)
                return;

            var networkObject = _heldItem.NetworkObject;
            _heldItem = null;
            _heldRef.Value = null;

            if (networkObject != null && networkObject.IsSpawned)
                networkObject.Despawn(true);
        }

        [Server]
        private void SyncSlots()
        {
            _sentRevision = _revision.Value;
            SlotSnapshotObservers(BuildItemIds(), BuildAmounts(), BuildSkins(), BuildCookness(), _revision.Value);
        }

        [ObserversRpc]
        private void SlotSnapshotObservers(int[] itemIds, int[] amounts, int[] skins, float[] cookness, int revision)
        {
            ApplySlotArrays(itemIds, amounts, skins, cookness);
            _sentRevision = revision;
        }

        private void ApplySlotArrays(int[] itemIds, int[] amounts, int[] skins, float[] cookness)
        {
            if (itemIds == null)
                return;

            for (int i = 0; i < _slots.Length && i < itemIds.Length; i++)
            {
                int amount = amounts != null && i < amounts.Length ? amounts[i] : 0;
                int skin = skins != null && i < skins.Length ? skins[i] : 0;
                float cooked = cookness != null && i < cookness.Length ? cookness[i] : 0f;
                _slots[i].Set(itemIds[i], amount, skin, cooked);
            }
        }

        private int[] BuildItemIds()
        {
            int[] result = new int[_slots.Length];
            for (int i = 0; i < _slots.Length; i++)
                result[i] = _slots[i].ItemId;
            return result;
        }

        private int[] BuildAmounts()
        {
            int[] result = new int[_slots.Length];
            for (int i = 0; i < _slots.Length; i++)
                result[i] = _slots[i].Amount;
            return result;
        }

        private int[] BuildSkins()
        {
            int[] result = new int[_slots.Length];
            for (int i = 0; i < _slots.Length; i++)
                result[i] = _slots[i].SkinId;
            return result;
        }

        private float[] BuildCookness()
        {
            float[] result = new float[_slots.Length];
            for (int i = 0; i < _slots.Length; i++)
                result[i] = _slots[i].Cookness;
            return result;
        }
    }
}
