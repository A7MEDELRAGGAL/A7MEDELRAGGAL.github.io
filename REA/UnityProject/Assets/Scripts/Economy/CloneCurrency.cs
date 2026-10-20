using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Clone.Economy
{
    public class CloneCurrency : NetworkBehaviour
    {
        private readonly SyncVar<int> _money = new SyncVar<int>(0);

        public int Money => _money.Value;

        [Server]
        public void AddMoney(int amount)
        {
            if (amount <= 0)
                return;
            _money.Value = _money.Value + amount;
        }

        [Server]
        public bool SpendMoney(int amount)
        {
            if (amount <= 0 || _money.Value < amount)
                return false;
            _money.Value = _money.Value - amount;
            return true;
        }

        [Server]
        public void SetMoney(int amount)
        {
            _money.Value = Mathf.Max(0, amount);
        }
    }
}
