using FishNet.Object;
using UnityEngine;

namespace Clone.Items
{
    public class CloneBait : CloneItem
    {
        [Header("Attraction")]
        public float AttractionRadius = 7f;
        public float AttractionStrength = 1.4f;

        [Header("Preference")]
        [Range(0f, 1f)]
        public float LureWeight = 0.5f;
        [Range(0f, 1f)]
        public float LureBrightness = 0.5f;

        public float LureScore
        {
            get
            {
                return LureWeight * 0.6f + LureBrightness * 0.4f;
            }
        }

        public bool IsUsable => !IsEmptyItem && Amount > 0;

        [Server]
        public void ServerConsumeOne(NetworkObject user)
        {
            if (IsEmptyItem)
                return;

            Amount = Mathf.Max(0, Amount - 1);
            if (Amount <= 0)
                Despawn(true);
        }

        public void DetachFromRod()
        {
            transform.SetParent(null, true);
            ApplyWorldPose(transform.position, transform.rotation);
        }
    }
}
