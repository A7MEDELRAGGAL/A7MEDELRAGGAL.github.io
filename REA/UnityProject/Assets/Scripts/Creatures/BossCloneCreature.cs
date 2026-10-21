using Clone.Items;
using Clone.Player;
using FishNet.Object;
using UnityEngine;

namespace Clone.Creatures
{
    public class BossCloneCreature : CloneCreature
    {
        [Server]
        protected override void ServerDie(NetworkObject killer)
        {
            ClonePlayer player = killer != null ? killer.GetComponent<ClonePlayer>() : null;
            ClonePlayerInventory inventory = player != null ? player.Inventory : null;

            if (inventory == null && ClonePlayer.LocalPlayer != null)
            {
                inventory = ClonePlayer.LocalPlayer.Inventory;
            }

            if (inventory != null)
            {
                inventory.ServerTryStore(CloneItemCatalog.BossKey, 1, 0, 0, out int remaining);
            }

            base.ServerDie(killer);
        }
    }
}
