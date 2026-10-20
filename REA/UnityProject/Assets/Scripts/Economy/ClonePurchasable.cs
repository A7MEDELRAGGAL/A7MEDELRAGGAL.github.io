using System;
using UnityEngine;

namespace Clone.Economy
{
    [Serializable]
    public class ClonePurchasable
    {
        public int ItemId;
        public int Amount = 1;
        public int Price;
        public bool Unlocked = true;
    }
}
