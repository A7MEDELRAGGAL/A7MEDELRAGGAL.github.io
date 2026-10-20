using Clone.Player;
using FishNet.Object;
using UnityEngine;

namespace Clone.Items
{
    [RequireComponent(typeof(Collider))]
    public class CloneItem : NetworkBehaviour
    {
        [Header("Identity")]
        public int ItemId = CloneInventorySlot.EmptyId;
        public int Amount = 1;
        public int SkinId;
        public float Cookness;

        [Header("Pickup")]
        public float PickupRange = 3f;
        public bool AutoPickup;

        public CloneItemDefinition Definition => CloneItemCatalog.Get(ItemId);
        public bool IsTaken { get; private set; }
        public float Weight => IsEmptyItem ? 0f : Definition.TotalWeight(Amount);

        public bool IsEmptyItem => ItemId == CloneInventorySlot.EmptyId || Amount <= 0;

        public static CloneItem SpawnServer(GameObject prefab, int itemId, int amount,
            Vector3 position, Quaternion rotation, int skinId = 0, float cookness = 0f)
        {
            if (prefab == null || !InstanceFinderServer())
                return null;

            GameObject instance = Instantiate(prefab, position, rotation);
            var item = instance.GetComponent<CloneItem>();
            if (item == null)
            {
                Destroy(instance);
                return null;
            }

            item.ItemId = itemId;
            item.Amount = Mathf.Max(1, amount);
            item.SkinId = skinId;
            item.Cookness = cookness;

            var networkObject = instance.GetComponent<NetworkObject>();
            if (networkObject != null)
                networkObject.Spawn(true);
            else
                item.ApplyWorldPose(position, rotation);

            return item;
        }

        private static bool InstanceFinderServer()
        {
            return FishNet.InstanceFinder.IsServerInitialized;
        }

        private void OnMouseDown()
        {
            RequestPickup();
        }

        public void RequestPickup()
        {
            if (IsTaken || IsEmptyItem || !IsSpawned)
                return;

            ClonePlayer player = ClonePlayer.LocalPlayer;
            if (player == null || player.Inventory == null)
                return;

            if (Vector3.Distance(player.transform.position, transform.position) > PickupRange)
                return;

            player.Inventory.ServerRequestPickup(NetworkObject);
        }

        [ServerRpc]
        public void ServerPickup(NetworkObject collector)
        {
            if (IsTaken || IsEmptyItem)
                return;

            var player = collector != null ? collector.GetComponent<ClonePlayer>() : null;
            if (player == null || player.Inventory == null)
                return;

            int stored = player.Inventory.ServerTryStore(ItemId, Amount, SkinId, Cookness, out int remaining);
            if (stored <= 0)
                return;

            if (remaining <= 0)
            {
                IsTaken = true;
                Despawn(true);
                return;
            }

            Amount = remaining;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!AutoPickup || IsTaken)
                return;

            var player = other != null ? other.GetComponentInParent<ClonePlayer>() : null;
            if (player == null || !player.IsOwner)
                return;

            RequestPickup();
        }

        public void ApplyWorldPose(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);

            var body = GetComponent<Rigidbody>();
            if (body != null)
            {
                body.position = position;
                body.rotation = rotation;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
        }

        public void SetContents(int itemId, int amount, int skinId, float cookness)
        {
            ItemId = itemId;
            Amount = amount;
            this.SkinId = skinId;
            Cookness = cookness;
        }
    }
}
