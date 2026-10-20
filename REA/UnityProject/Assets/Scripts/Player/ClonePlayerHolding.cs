using Clone.Items;
using UnityEngine;

namespace Clone.Player
{
    public class ClonePlayerHolding : MonoBehaviour
    {
        [Header("Anchor")]
        public Transform HoldAnchor;

        [Header("Pose")]
        public Vector3 LocalOffset = new Vector3(0.24f, -0.12f, 0.42f);
        public Vector3 LocalEuler = Vector3.zero;
        public float PoseLerp = 14f;

        [Header("Sway / Bob")]
        public float SwaySpeed = 2.1f;
        public float SwayAmount = 0.02f;
        public float BobSpeed = 1.7f;
        public float BobAmount = 0.015f;

        [Header("Unstuck")]
        public float UnstuckRadius = 0.09f;
        public float UnstuckPush = 0.35f;
        public LayerMask UnstuckMask = ~0;

        public Transform Held { get; private set; }

        private float _phase;
        private Vector3 _posePosition;
        private Quaternion _poseRotation = Quaternion.identity;
        private bool _poseReady;

        private void Awake()
        {
            if (HoldAnchor == null)
                HoldAnchor = transform;
        }

        private void LateUpdate()
        {
            FollowInventoryHeld();
            _phase += Time.deltaTime;

            if (Held != null && !_poseReady)
                CapturePose();

            ApplyPose();
            PushOutOfGeometry();
        }

        private void FollowInventoryHeld()
        {
            var player = GetComponent<ClonePlayer>();
            if (player == null || player.Inventory == null)
                return;

            CloneItem held = player.Inventory.HeldItem;
            Transform target = held != null ? held.transform : null;

            if (Held != target)
                Attach(target);
        }

        public void Attach(Transform held)
        {
            if (Held == held)
                return;

            Detach();
            Held = held;
            _poseReady = false;
        }

        public void Detach()
        {
            Held = null;
            _poseReady = false;
        }

        public void UseSelected()
        {
            var player = GetComponent<ClonePlayer>();
            if (player == null || player.Inventory == null)
                return;

            player.Inventory.ServerRequestUse();
        }

        public void EatSelected()
        {
            var player = GetComponent<ClonePlayer>();
            if (player == null || player.Inventory == null)
                return;

            player.Inventory.ServerRequestConsume(player.Inventory.SelectedSlot);
        }

        public bool IsHoldingTool => Held != null && Held.GetComponentInParent<CloneTool>() != null;

        public CloneTool HeldTool => Held != null ? Held.GetComponentInParent<CloneTool>() : null;

        private void CapturePose()
        {
            if (HoldAnchor == null || Held == null)
                return;

            _posePosition = LocalOffset;
            _poseRotation = Quaternion.Euler(LocalEuler);
            _poseReady = true;
        }

        private void ApplyPose()
        {
            if (HoldAnchor == null || Held == null)
                return;

            Vector3 sway = new Vector3(
                Mathf.Sin(_phase * SwaySpeed) * SwayAmount,
                Mathf.Abs(Mathf.Cos(_phase * BobSpeed)) * BobAmount,
                0f);

            Vector3 targetPosition = _posePosition + sway;
            Quaternion targetRotation = _poseRotation * Quaternion.Euler(0f, sway.z * 40f, 0f);

            Held.localPosition = Vector3.Lerp(Held.localPosition, targetPosition,
                1f - Mathf.Exp(-PoseLerp * Time.deltaTime));
            Held.localRotation = Quaternion.Slerp(Held.localRotation, targetRotation,
                1f - Mathf.Exp(-PoseLerp * Time.deltaTime));
        }

        private void PushOutOfGeometry()
        {
            if (HoldAnchor == null || Held == null || UnstuckRadius <= 0f || UnstuckMask.value == 0)
                return;

            Vector3 world = Held.position;
            if (!Physics.CheckSphere(world, UnstuckRadius, UnstuckMask, QueryTriggerInteraction.Ignore))
                return;

            if (!Physics.SphereCast(world, UnstuckRadius, Vector3.up, out RaycastHit hit, UnstuckPush + UnstuckRadius,
                    UnstuckMask, QueryTriggerInteraction.Ignore))
            {
                if (!Physics.SphereCast(world, UnstuckRadius, Vector3.down, out hit,
                        UnstuckPush + UnstuckRadius, UnstuckMask, QueryTriggerInteraction.Ignore))
                {
                    return;
                }
            }

            Held.position = hit.point + hit.normal * (UnstuckRadius + 0.02f);
        }
    }
}
