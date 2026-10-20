using Clone.Player;
using FishNet.Object;
using UnityEngine;

namespace Clone.World
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CloneBoatMotor))]
    public class CloneBoat : NetworkBehaviour
    {
        [Header("Seat")]
        public Transform DriverSeat;

        [Header("Buoyancy")]
        public float BuoyancyPerMeter = 1100f;
        public float MaxBuoyancyDepth = 2.2f;
        public float UprightTorque = 260f;
        public float UprightDamping = 90f;

        [Header("Limits")]
        public float MaxLinearSpeed = 26f;
        public float MaxAngularSpeed = 3.5f;

        [Header("Replication")]
        public float ReplicateInterval = 0.1f;
        public float SnapDistance = 5f;
        public float InterpolationSpeed = 12f;

        public CloneBoatMotor Motor { get; private set; }
        public ClonePlayer Driver { get; private set; }
        public bool HasDriver => Driver != null;
        public bool IsLocalDriver { get; private set; }

        private Rigidbody _body;
        private Vector3[] _buoyancyPoints;
        private bool _hasReplicatedState;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation = Quaternion.identity;
        private Vector3 _targetVelocity;
        private Vector3 _lastReplicatedPosition;
        private float _replicateTimer;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            Motor = GetComponent<CloneBoatMotor>();
            BuildBuoyancyPoints();
            _targetPosition = transform.position;
            _targetRotation = transform.rotation;
            _lastReplicatedPosition = transform.position;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (IsServer || _body == null)
                return;
            _body.isKinematic = true;
        }

        public override void OnStopClient()
        {
            IsLocalDriver = false;
            base.OnStopClient();
        }

        private void Update()
        {
            if (!IsSpawned || !IsLocalDriver)
                return;

            float throttle = 0f;
            float steer = 0f;

            if (Input.GetKey(KeyCode.W))
                throttle += 1f;
            if (Input.GetKey(KeyCode.S))
                throttle -= 1f;
            if (Input.GetKey(KeyCode.A))
                steer -= 1f;
            if (Input.GetKey(KeyCode.D))
                steer += 1f;

            bool boost = Input.GetKey(KeyCode.LeftShift);

            if (Motor != null)
                Motor.SetInput(throttle, steer, boost);

            if (!IsServer)
                ServerSetControl(throttle, steer, boost);
        }

        private void FixedUpdate()
        {
            if (!IsSpawned)
                return;

            if (IsServer)
            {
                SimulatePhysics(Time.fixedDeltaTime);
                ServerReplication(Time.fixedDeltaTime);
                return;
            }

            InterpolateState(Time.fixedDeltaTime);
        }

        [ServerRpc]
        private void ServerSetControl(float throttle, float steer, bool boost)
        {
            if (Motor != null)
                Motor.SetInput(throttle, steer, boost);
        }

        [ObserversRpc]
        private void ReplicateState(Vector3 position, Quaternion rotation, Vector3 velocity)
        {
            _targetPosition = position;
            _targetRotation = rotation;
            _targetVelocity = velocity;
            _hasReplicatedState = true;
        }

        [Server]
        public void ServerSetDriver(ClonePlayer driver)
        {
            if (!IsServer)
                return;

            if (Driver != null && Driver != driver)
                ServerSetDriver(null);

            Driver = driver;

            if (NetworkObject != null && NetworkObject.IsSpawned)
                NetworkObject.ChangeOwnership(driver != null
                    ? driver.OwnerClientId
                    : NetworkManager.ServerClientId);

            if (driver != null && Motor != null)
                Motor.ClearInput();
        }

        public void SetLocalDriver(bool isLocalDriver)
        {
            IsLocalDriver = isLocalDriver;
            if (!isLocalDriver && Motor != null)
                Motor.ClearInput();
        }

        public Vector3 GetBoatVelocity()
        {
            return _body != null ? _body.linearVelocity : Vector3.zero;
        }

        private void SimulatePhysics(float deltaTime)
        {
            if (_body == null)
                return;

            ApplyBuoyancy();
            ApplyUprightStability(deltaTime);

            var water = CloneWaterManager.Instance;
            float depth = water != null ? water.GetDepth(transform.position) : 0f;
            if (Motor != null)
                Motor.Apply(_body, depth, deltaTime);

            _body.linearVelocity = Vector3.ClampMagnitude(_body.linearVelocity, MaxLinearSpeed);
            _body.angularVelocity = Vector3.ClampMagnitude(_body.angularVelocity, MaxAngularSpeed);
        }

        private void ApplyBuoyancy()
        {
            var water = CloneWaterManager.Instance;
            if (water == null || _buoyancyPoints == null)
                return;

            for (int i = 0; i < _buoyancyPoints.Length; i++)
            {
                Vector3 world = transform.TransformPoint(_buoyancyPoints[i]);
                float depth = water.GetDepth(world);
                if (depth <= 0f)
                    continue;

                float submerged = Mathf.Clamp01(depth / Mathf.Max(0.01f, MaxBuoyancyDepth));
                _body.AddForceAtPosition(Vector3.up * (BuoyancyPerMeter * submerged), world, ForceMode.Force);
            }
        }

        private void ApplyUprightStability(float deltaTime)
        {
            Vector3 correction = Vector3.Cross(transform.up, Vector3.up);
            _body.AddTorque(correction * UprightTorque * deltaTime, ForceMode.Acceleration);
            _body.AddTorque(-_body.angularVelocity * UprightDamping * deltaTime, ForceMode.Acceleration);
        }

        private void ServerReplication(float deltaTime)
        {
            _replicateTimer -= deltaTime;
            if (_replicateTimer > 0f)
                return;

            bool moved = (transform.position - _lastReplicatedPosition).sqrMagnitude > 0.0001f;
            if (!HasDriver && !moved && _hasReplicatedState)
                return;

            _replicateTimer = Mathf.Max(0.02f, ReplicateInterval);
            _lastReplicatedPosition = transform.position;
            ReplicateState(transform.position, transform.rotation,
                _body != null ? _body.linearVelocity : Vector3.zero);
        }

        private void InterpolateState(float deltaTime)
        {
            if (!_hasReplicatedState)
                return;

            if ((transform.position - _targetPosition).sqrMagnitude > SnapDistance * SnapDistance)
            {
                transform.SetPositionAndRotation(_targetPosition, _targetRotation);
                return;
            }

            float blend = 1f - Mathf.Exp(-InterpolationSpeed * deltaTime);
            transform.SetPositionAndRotation(
                Vector3.Lerp(transform.position, _targetPosition, blend),
                Quaternion.Slerp(transform.rotation, _targetRotation, blend));
        }

        private void BuildBuoyancyPoints()
        {
            Bounds bounds = default;
            bool any = false;

            foreach (Collider child in GetComponentsInChildren<Collider>())
            {
                if (child.isTrigger)
                    continue;

                if (!any)
                {
                    bounds = child.bounds;
                    any = true;
                }
                else
                {
                    bounds.Encapsulate(child.bounds);
                }
            }

            if (!any)
                bounds = new Bounds(transform.position, new Vector3(2f, 1f, 4f));

            Vector3 center = transform.InverseTransformPoint(bounds.center);
            float halfWidth = Mathf.Max(0.25f, bounds.extents.x * 0.75f);
            float halfLength = Mathf.Max(0.5f, bounds.extents.z * 0.75f);
            float y = center.y;

            _buoyancyPoints = new[]
            {
                new Vector3(0f, y, halfLength),
                new Vector3(0f, y, -halfLength),
                new Vector3(-halfWidth, y, halfLength * 0.5f),
                new Vector3(halfWidth, y, halfLength * 0.5f),
                new Vector3(-halfWidth, y, -halfLength * 0.5f),
                new Vector3(halfWidth, y, -halfLength * 0.5f),
                new Vector3(0f, y, 0f)
            };
        }
    }
}
