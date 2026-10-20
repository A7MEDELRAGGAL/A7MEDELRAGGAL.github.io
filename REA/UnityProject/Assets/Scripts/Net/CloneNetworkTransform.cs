using FishNet.Object;
using UnityEngine;

namespace Clone.Net
{
    public enum CloneTransformAuthority
    {
        ServerAuthoritative,
        OwnerAuthoritative
    }

    [RequireComponent(typeof(NetworkObject))]
    public class CloneNetworkTransform : NetworkBehaviour
    {
        [Header("Authority")]
        public CloneTransformAuthority Authority = CloneTransformAuthority.OwnerAuthoritative;

        [Header("Rates")]
        public float SendInterval = 0.08f;
        public float InterpolationSpeed = 14f;
        public float SnapDistance = 6f;

        [Header("Content")]
        public bool SyncScale;
        public bool UseRigidbodyVelocity;

        private Transform _root;
        private Rigidbody _body;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation = Quaternion.identity;
        private Vector3 _targetScale = Vector3.one;
        private bool _hasPose;
        private float _sendTimer;

        private void Awake()
        {
            _root = transform;
            _body = GetComponent<Rigidbody>();
            _targetPosition = _root.position;
            _targetRotation = _root.rotation;
            _targetScale = _root.localScale;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (IsSimulationAuthority())
                return;

            _targetPosition = _root.position;
            _targetRotation = _root.rotation;
            _targetScale = _root.localScale;
        }

        private void Update()
        {
            if (!IsSpawned)
                return;

            if (IsSimulationAuthority())
                SubmitPose();
            else
                ApplyPose();
        }

        public bool IsSimulationAuthority()
        {
            return Authority == CloneTransformAuthority.ServerAuthoritative ? IsServer : IsOwner;
        }

        private void SubmitPose()
        {
            _sendTimer -= Time.deltaTime;
            if (_sendTimer > 0f)
                return;

            _sendTimer = Mathf.Max(0.02f, SendInterval);
            Vector3 position = _body != null && UseRigidbodyVelocity ? _body.position : _root.position;
            Vector3 scale = SyncScale ? _root.localScale : Vector3.one;

            if (IsServer)
            {
                BroadcastPose(position, _root.rotation, scale);
                return;
            }

            ServerSubmitPose(position, _root.rotation, scale);
        }

        private void ApplyPose()
        {
            if (!_hasPose)
                return;

            if ((_root.position - _targetPosition).sqrMagnitude > SnapDistance * SnapDistance)
            {
                SnapToTarget();
                return;
            }

            float blend = 1f - Mathf.Exp(-InterpolationSpeed * Time.deltaTime);
            _root.position = Vector3.Lerp(_root.position, _targetPosition, blend);
            _root.rotation = Quaternion.Slerp(_root.rotation, _targetRotation, blend);
            if (SyncScale)
                _root.localScale = Vector3.Lerp(_root.localScale, _targetScale, blend);
        }

        private void SnapToTarget()
        {
            _root.position = _targetPosition;
            _root.rotation = _targetRotation;
            if (SyncScale)
                _root.localScale = _targetScale;

            if (_body != null && UseRigidbodyVelocity)
                _body.linearVelocity = Vector3.zero;
        }

        [ServerRpc]
        private void ServerSubmitPose(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            BroadcastPose(position, rotation, scale);
        }

        [ObserversRpc]
        private void BroadcastPose(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (IsSpawned && IsSimulationAuthority())
                return;

            _targetPosition = position;
            _targetRotation = rotation;
            _targetScale = scale;
            _hasPose = true;
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            if (!IsSpawned)
            {
                transform.SetPositionAndRotation(position, rotation);
                return;
            }

            if (IsSimulationAuthority())
            {
                transform.SetPositionAndRotation(position, rotation);
                if (_body != null)
                {
                    _body.position = position;
                    _body.linearVelocity = Vector3.zero;
                    _body.angularVelocity = Vector3.zero;
                }

                if (IsServer)
                    BroadcastPose(position, rotation, SyncScale ? transform.localScale : Vector3.one);
                else
                    ServerSubmitPose(position, rotation, SyncScale ? transform.localScale : Vector3.one);
                return;
            }

            _targetPosition = position;
            _targetRotation = rotation;
            _hasPose = true;
        }
    }
}
