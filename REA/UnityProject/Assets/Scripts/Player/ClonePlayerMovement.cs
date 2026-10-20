using Clone.World;
using UnityEngine;

namespace Clone.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class ClonePlayerMovement : MonoBehaviour
    {
        [Header("Speeds")]
        public float WalkSpeed = 6.5f;
        public float SprintSpeed = 13f;
        public float CrouchSpeed = 2.1f;
        public float AirControl = 0.35f;
        public float MoveAcceleration = 70f;

        [Header("Jump / Ground")]
        public float JumpVelocity = 6f;
        public float JumpCooldown = 0.25f;
        public float Gravity = 19.6f;
        public float ExtraGravity = 5f;
        public float GroundCheckDistance = 0.35f;
        public float MaxSlopeAngle = 50f;
        public float SlideForce = 800f;
        public float CounterMovement = 0.14f;
        public float CounterCooldown = 0.25f;
        public float Threshold = 0.01f;
        public LayerMask GroundMask = ~0;

        [Header("Water")]
        public float SwimSpeed = 2.6f;
        public float SwimAcceleration = 30f;
        public float SwimVerticalSpeed = 1.5f;
        public float SwimDepthThreshold = 1.1f;
        public float BoatCarryLerp = 0.35f;

        [Header("Knockback")]
        public float KnockbackThreshold = 4f;
        public float KnockbackMultiplier = 0.35f;
        public float KnockbackLift = 0.4f;
        public float KnockbackDuration = 0.45f;
        public float KnockbackControl = 0.15f;
        public float KnockbackDamping = 2.2f;
        public float KnockbackStopRetain = 0.6f;
        public float SelfKnockbackFactor = 0.5f;

        [Header("References")]
        public ClonePlayerCamera CameraRig;

        public Vector3 Velocity => _body.linearVelocity;
        public float SlipAngle { get; private set; }
        public bool IsGrounded { get; private set; }
        public bool IsCrouching { get; private set; }
        public bool OnBoat { get; private set; }
        public bool IsDriver { get; private set; }
        public bool IsSwimming { get; private set; }
        public bool IsUnderWater { get; private set; }
        public bool IsSliding { get; private set; }
        public float WaterDepth { get; private set; }

        private Rigidbody _body;
        private Rigidbody _boatBody;
        private Transform _driverSeat;
        private Vector3 _slopeNormal = Vector3.up;
        private float _knockbackTimer;
        private float _counterCooldownX;
        private float _counterCooldownY;
        private Vector2 _input;
        private bool _jumpRequested;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _body.freezeRotation = true;
            _body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            if (CameraRig == null)
                CameraRig = GetComponentInChildren<ClonePlayerCamera>();
        }

        private void Update()
        {
            if (!TryGetLocalAuthority())
                return;

            _input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (Input.GetKeyDown(KeyCode.Space))
                _jumpRequested = true;
            if (Input.GetKeyDown(KeyCode.F))
                HandleBoatSeatInput();
            IsCrouching = Input.GetKey(KeyCode.C);
        }

        private void HandleBoatSeatInput()
        {
            var player = GetComponent<ClonePlayer>();
            if (player == null)
                return;

            if (player.IsDrivingClient)
            {
                player.ServerReleaseBoatSeat();
                return;
            }

            Transform parent = transform.parent;
            if (parent == null)
                return;

            var boat = parent.GetComponent<CloneBoat>();
            if (boat == null || boat.NetworkObject == null)
                return;

            player.ServerRequestBoatSeat(boat.NetworkObject);
        }

        private void FixedUpdate()
        {
            if (!TryGetLocalAuthority())
                return;

            UnderWaterCheck();
            IsGrounded = CheckGrounded();
            IsSliding = false;

            _counterCooldownX = Mathf.Max(0f, _counterCooldownX - Time.fixedDeltaTime);
            _counterCooldownY = Mathf.Max(0f, _counterCooldownY - Time.fixedDeltaTime);

            if (_body != null)
                _body.useGravity = !IsSwimming;

            if (IsDriver)
            {
                ApplyKnockbackDecay();
                return;
            }

            if (OnBoat)
            {
                ApplyBoatCarry();
                ApplyKnockbackDecay();
                return;
            }

            if (IsSwimming)
            {
                ApplySwim();
                ApplyKnockbackDecay();
                return;
            }

            ApplyMovement();
            ApplyGravity();
            ApplyJump();
            ApplyKnockbackDecay();
        }

        private void ApplyMovement()
        {
            float speed = IsCrouching ? CrouchSpeed
                : Input.GetKey(KeyCode.LeftShift) ? SprintSpeed
                : WalkSpeed;

            Vector3 forward = CameraRig != null
                ? Quaternion.Euler(0f, CameraRig.Yaw, 0f) * Vector3.forward
                : Vector3.forward;
            Vector3 right = new Vector3(forward.z, 0f, -forward.x);
            Vector3 desired = (forward * _input.y + right * _input.x);
            if (desired.sqrMagnitude > 1f)
                desired.Normalize();

            float acceleration = MoveAcceleration
                * (IsCrouching ? 0.55f : 1f)
                * (IsGrounded ? 1f : AirControl)
                * (_knockbackTimer > 0f ? KnockbackControl : 1f);

            Vector3 velocity = _body.linearVelocity;
            velocity.x += desired.x * acceleration * Time.fixedDeltaTime;
            velocity.z += desired.z * acceleration * Time.fixedDeltaTime;

            ApplyCounterMovement(ref velocity);
            ApplySlide(ref velocity);

            float horizontal = new Vector2(velocity.x, velocity.z).magnitude;
            if (horizontal > speed)
            {
                float k = speed / horizontal;
                velocity.x *= k;
                velocity.z *= k;
            }

            _body.linearVelocity = new Vector3(velocity.x, _body.linearVelocity.y, velocity.z);

            Vector3 planar = new Vector3(velocity.x, 0f, velocity.z);
            SlipAngle = planar.sqrMagnitude > 0.01f
                ? Vector3.Angle(planar, desired.sqrMagnitude > 0.01f ? desired : planar)
                : 0f;
        }

        private void ApplyCounterMovement(ref Vector3 velocity)
        {
            float push = MoveAcceleration * CounterMovement;

            if (Mathf.Abs(_input.x) > 0.01f
                && _input.x * velocity.x < 0f
                && Mathf.Abs(velocity.x) > Threshold)
            {
                if (_counterCooldownX <= 0f && IsBlockedAhead(true, _input.x > 0f ? 1f : -1f))
                {
                    velocity.x = velocity.x * 0.5f + _input.x * push * Time.fixedDeltaTime;
                    _counterCooldownX = CounterCooldown;
                }
                else
                {
                    velocity.x += _input.x * push * Time.fixedDeltaTime;
                }
            }

            if (Mathf.Abs(_input.y) > 0.01f
                && _input.y * velocity.z < 0f
                && Mathf.Abs(velocity.z) > Threshold)
            {
                if (_counterCooldownY <= 0f && IsBlockedAhead(false, _input.y > 0f ? 1f : -1f))
                {
                    velocity.z = velocity.z * 0.5f + _input.y * push * Time.fixedDeltaTime;
                    _counterCooldownY = CounterCooldown;
                }
                else
                {
                    velocity.z += _input.y * push * Time.fixedDeltaTime;
                }
            }
        }

        private bool IsBlockedAhead(bool alongX, float sign)
        {
            float reach = 0.9f;
            Vector3 ahead = transform.position;
            if (alongX)
                ahead.x += sign * reach;
            else
                ahead.z += sign * reach;

            float ground = SampleGround(ahead.x, ahead.z);
            return ground - transform.position.y > 1.05f;
        }

        private void ApplySlide(ref Vector3 velocity)
        {
            if (!IsGrounded)
                return;

            float slope = GetSlopeAngle();
            if (slope <= MaxSlopeAngle)
                return;

            IsSliding = true;
            Vector3 downhill = new Vector3(_slopeNormal.x, 0f, _slopeNormal.z);
            if (downhill.sqrMagnitude < 0.0001f)
                return;

            downhill.Normalize();
            float slide = SlideForce * 0.02f * Time.fixedDeltaTime;
            velocity.x += downhill.x * slide;
            velocity.z += downhill.z * slide;
        }

        private float GetSlopeAngle()
        {
            float step = 1.2f;
            float hx = SampleGround(transform.position.x + step, transform.position.z);
            float hz = SampleGround(transform.position.x, transform.position.z + step);
            float h0 = SampleGround(transform.position.x, transform.position.z);

            _slopeNormal = new Vector3(h0 - hx, 1f, h0 - hz).normalized;
            return Mathf.Acos(Mathf.Clamp(_slopeNormal.y, -1f, 1f)) * Mathf.Rad2Deg;
        }

        private float SampleGround(float x, float z)
        {
            if (Physics.Raycast(new Vector3(x, transform.position.y + 0.5f, z), Vector3.down,
                    out RaycastHit hit, GroundCheckDistance + 3f, GroundMask, QueryTriggerInteraction.Ignore))
            {
                return hit.point.y;
            }

            return transform.position.y;
        }

        private void ApplyGravity()
        {
            if (!_body.useGravity)
                _body.useGravity = true;

            float gravity = Gravity + (IsGrounded ? 0f : ExtraGravity);
            if (!IsGrounded && _body.linearVelocity.y > -gravity)
            {
                Vector3 v = _body.linearVelocity;
                v.y -= gravity * Time.fixedDeltaTime;
                _body.linearVelocity = v;
            }
        }

        private void ApplyJump()
        {
            if (!_jumpRequested)
                return;
            _jumpRequested = false;
            if (!IsGrounded || OnBoat)
                return;

            Vector3 v = _body.linearVelocity;
            v.y = JumpVelocity;
            _body.linearVelocity = v;
            StopKnockback();
        }

        private bool CheckGrounded()
        {
            return Physics.Raycast(transform.position + Vector3.up * 0.5f,
                Vector3.down, GroundCheckDistance + 0.5f, GroundMask, QueryTriggerInteraction.Ignore);
        }

        public void SetBoat(GameObject boat, bool onBoat)
        {
            OnBoat = onBoat && boat != null;
            _boatBody = OnBoat ? boat.GetComponent<Rigidbody>() : null;
            if (_body == null)
                return;
            transform.SetParent(OnBoat ? boat.transform : null, true);
        }

        public void SetDriver(Transform seat, bool isDriver)
        {
            IsDriver = isDriver && seat != null;
            _driverSeat = IsDriver ? seat : null;

            if (_body == null)
                return;

            _body.isKinematic = IsDriver;

            if (IsDriver)
            {
                _body.linearVelocity = Vector3.zero;
                _body.angularVelocity = Vector3.zero;
                transform.SetParent(_driverSeat, true);
                MoveToBoatDriverPos(_driverSeat);
            }
            else
            {
                transform.SetParent(OnBoat && _boatBody != null ? _boatBody.transform : null, true);
                StopKnockback();
            }
        }

        public Vector3 GetBoatVel()
        {
            return _boatBody != null ? _boatBody.linearVelocity : Vector3.zero;
        }

        public void MoveToBoatDriverPos(Transform seat)
        {
            if (seat == null || _body == null)
                return;
            transform.position = seat.position;
            transform.rotation = seat.rotation;
            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;
        }

        private void ApplyBoatCarry()
        {
            Vector3 boatVelocity = GetBoatVel();
            if (_body == null || boatVelocity == Vector3.zero)
                return;

            Vector3 current = _body.linearVelocity;
            Vector3 carried = Vector3.Lerp(current, boatVelocity, Mathf.Clamp01(BoatCarryLerp));
            _body.linearVelocity = new Vector3(carried.x, boatVelocity.y, carried.z);
        }

        private void ApplySwim()
        {
            Vector3 forward = CameraRig != null
                ? Quaternion.Euler(0f, CameraRig.Yaw, 0f) * Vector3.forward
                : Vector3.forward;
            Vector3 right = new Vector3(forward.z, 0f, -forward.x);
            Vector3 desired = forward * _input.y + right * _input.x;
            if (desired.sqrMagnitude > 1f)
                desired.Normalize();

            float speed = IsCrouching ? SwimSpeed * 0.5f : SwimSpeed;
            Vector3 current = _body.linearVelocity;
            Vector3 horizontal = Vector3.MoveTowards(
                new Vector3(current.x, 0f, current.z),
                desired * speed,
                Mathf.Max(0.01f, SwimAcceleration) * Time.fixedDeltaTime);

            float verticalTarget = 0f;
            if (Input.GetKey(KeyCode.C))
                verticalTarget = -SwimVerticalSpeed;
            else if (Input.GetKey(KeyCode.LeftControl))
                verticalTarget = SwimVerticalSpeed;

            float vertical = Mathf.MoveTowards(current.y, verticalTarget,
                Mathf.Max(0.01f, SwimVerticalSpeed) * 2f * Time.fixedDeltaTime);

            _body.linearVelocity = new Vector3(horizontal.x, vertical, horizontal.z);
            IsGrounded = false;
        }

        private void UnderWaterCheck()
        {
            var water = CloneWaterManager.Instance;
            if (water == null)
            {
                WaterDepth = 0f;
                IsUnderWater = false;
                IsSwimming = false;
                return;
            }

            WaterDepth = water.GetDepth(transform.position);
            IsUnderWater = WaterDepth > 0f;
            IsSwimming = !OnBoat && !IsDriver && WaterDepth >= SwimDepthThreshold;
        }

        private void ApplyKnockbackDecay()
        {
            if (_knockbackTimer <= 0f || _body == null)
                return;

            _knockbackTimer -= Time.fixedDeltaTime;
            Vector3 velocity = _body.linearVelocity;
            velocity.x = Mathf.MoveTowards(velocity.x, 0f, KnockbackDamping * Time.fixedDeltaTime);
            velocity.z = Mathf.MoveTowards(velocity.z, 0f, KnockbackDamping * Time.fixedDeltaTime);
            _body.linearVelocity = velocity;
        }

        public void StopKnockback()
        {
            _knockbackTimer = 0f;
            if (_body == null)
                return;

            Vector3 velocity = _body.linearVelocity;
            velocity.x *= Mathf.Clamp01(KnockbackStopRetain);
            velocity.z *= Mathf.Clamp01(KnockbackStopRetain);
            _body.linearVelocity = velocity;
        }

        public void TeleportToLand(Vector3 position)
        {
            if (_body == null)
                return;
            _body.linearVelocity = Vector3.zero;
            transform.position = position;
        }

        public void ApplyKnockback(Vector3 force)
        {
            if (_body == null || _body.isKinematic)
                return;

            _knockbackTimer = KnockbackDuration;
            _body.AddForce(force, ForceMode.Impulse);
        }

        private void OnCollisionEnter(Collision collision)
        {
            var player = GetComponent<ClonePlayer>();
            if (player == null || !player.IsSpawned || !player.IsServer)
                return;

            float threshold = Mathf.Max(0.01f, KnockbackThreshold);
            float relativeSpeed = collision.relativeVelocity.magnitude;
            if (relativeSpeed < threshold)
                return;

            Vector3 direction = collision.relativeVelocity.normalized;
            float magnitude = relativeSpeed * KnockbackMultiplier;
            Vector3 impulse = (direction + Vector3.up * KnockbackLift).normalized * magnitude;

            var other = collision.collider != null
                ? collision.collider.GetComponentInParent<ClonePlayer>()
                : null;

            if (other != null && other != player)
            {
                other.ServerKnockback(impulse);
            }

            player.ServerKnockback(-direction * (magnitude * SelfKnockbackFactor));
        }

        private bool TryGetLocalAuthority()
        {
            var player = GetComponent<ClonePlayer>();
            return player == null || player.IsOwner;
        }
    }
}
