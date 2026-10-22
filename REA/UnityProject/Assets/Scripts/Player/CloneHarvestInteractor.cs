using Clone.Items;
using FishNet.Object;
using UnityEngine;

namespace Clone.Player
{
    // Host-mode harvesting: [E] swings at the aimed node; the server checks
    // the hit, applies tool bonus, and stores the yield straight to inventory.
    public class CloneHarvestInteractor : NetworkBehaviour
    {
        public float Range = 4.2f;
        public float HandPower = 25f;
        public float ToolPower = 50f;

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.E))
                return;

            var player = GetComponent<ClonePlayer>();
            if (player == null || !player.IsOwner)
                return;

            ServerRequestHarvest();
        }

        [ServerRpc]
        private void ServerRequestHarvest()
        {
            Camera camera = GetComponentInChildren<Camera>();
            Vector3 origin = transform.position;
            Vector3 direction = transform.forward;
            if (camera != null)
            {
                origin = camera.transform.position;
                direction = camera.transform.forward;
            }

            if (!Physics.Raycast(origin, direction, out RaycastHit hit, Range))
                return;

            CloneHarvestNode node = hit.collider != null
                ? hit.collider.GetComponentInParent<CloneHarvestNode>()
                : null;
            if (node == null || node.IsDepleted)
                return;

            bool withTool = ToolOwned(node.Kind);
            node.ServerHarvest(withTool ? ToolPower : HandPower);
            if (!node.IsDepleted)
                return;

            if (!node.ServerTryTake(out int itemId, out int amount))
                return;

            if (withTool)
                amount *= 2;

            var inventory = GetComponent<ClonePlayerInventory>();
            if (inventory == null)
                return;

            inventory.ServerTryStore(itemId, amount, 0, 0f, out int remaining);
            if (remaining > 0)
                Debug.LogWarning($"[CloneHarvest] Inventory full; lost {remaining} of itemId {itemId}");
        }

        [Server]
        private bool ToolOwned(CloneHarvestKind kind)
        {
            var inventory = GetComponent<ClonePlayerInventory>();
            if (inventory == null)
                return false;

            switch (kind)
            {
                case CloneHarvestKind.Wood:
                    return inventory.ServerHasItems(CloneItemCatalog.WoodAxe, 1);
                case CloneHarvestKind.Stone:
                    return inventory.ServerHasItems(CloneItemCatalog.WoodPick, 1);
                case CloneHarvestKind.Fiber:
                    return inventory.ServerHasItems(CloneItemCatalog.WoodAxe, 1)
                        || inventory.ServerHasItems(CloneItemCatalog.WoodPick, 1);
                default:
                    return false;
            }
        }
    }
}
