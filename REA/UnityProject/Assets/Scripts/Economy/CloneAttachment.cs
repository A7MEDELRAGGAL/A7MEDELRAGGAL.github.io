using UnityEngine;

namespace Clone.Economy
{
    public class CloneAttachment : MonoBehaviour
    {
        [SerializeField]
        private string _attachmentName;
        [SerializeField]
        private float _damageMultiplier = 1f;
        [SerializeField]
        private float _rangeMultiplier = 1f;
        [SerializeField]
        private float _knockbackMultiplier = 1f;

        public string AttachmentName => _attachmentName;
        public float DamageMultiplier => _damageMultiplier;
        public float RangeMultiplier => _rangeMultiplier;
        public float KnockbackMultiplier => _knockbackMultiplier;
    }
}
