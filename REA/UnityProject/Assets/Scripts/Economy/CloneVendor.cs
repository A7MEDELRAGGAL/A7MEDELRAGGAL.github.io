using System.Collections.Generic;
using Clone.Player;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Clone.Economy
{
    public class CloneVendor : NetworkBehaviour
    {
        [SerializeField]
        private List<ClonePurchasable> _stock = new();

        public IReadOnlyList<ClonePurchasable> Stock => _stock;

        [ServerRpc(RequireOwnership = false)]
        public void ServerRequestPurchase(int index)
        {
            if (index < 0 || index >= _stock.Count)
                return;

            var entry = _stock[index];
            if (!entry.Unlocked || entry.Price <= 0)
                return;

            var player = Owner?.GetComponent<ClonePlayer>();
            if (player == null)
                return;

            var currency = player.GetComponent<CloneCurrency>();
            if (currency == null || !currency.SpendMoney(entry.Price))
                return;

            if (entry.Amount <= 0)
            {
                currency.AddMoney(entry.Price);
                return;
            }

            var inventory = player.GetComponent<ClonePlayerInventory>();
            if (inventory == null)
                return;

            int remaining = entry.Amount;
            inventory.ServerTryStore(entry.ItemId, entry.Amount, 0, 0f, out remaining);
            currency.AddMoney(remaining * entry.Price / entry.Amount);
        }
    }
}
