using UnityEngine;

namespace Clone.Fps
{
    [RequireComponent(typeof(Rigidbody))]
    public class FpsPlayerMotor : MonoBehaviour
    {
        [Header("Speeds")]
        public float WalkSpeed = 5f;
        public float SprintSpeed = 8.5f;
        public float CrouchSpeed = 2.4f;
        public float SpeedScale = 1f;
        public float AirControl = 0.32f;
        public float MoveAcceleration = 80f;

        [Header("Slide")]
        public float SlideEntryImpulse = 11f;
        public float SlideFriction = 3.5f;
        public float SlideMinDuration = 0.3f;
        public float SlideMaxDuration = 1.2f;
        public float SlideHeightFactor = 0.45f;
        public KeyCode SlideKey = KeyCode.LeftControl;

        [Header("Jump / Ground")]
        public float JumpVelocity = 5.8f;
        public float Gravity = 24f;
        public float ExtraGravityWhenFalling = 6f;
        public float GroundCheckDistance = 0.4f;
        public float MantleCheckDistance = 1.6f;
        public float MantlePullSpeed = 6f;
        public float LandingImpactThreshold = 8f;
        public LayerMask GroundMask = ~0;

        [Header("Input")]
        public KeyCode SprintKey = KeyCode.LeftShift;
        public KeyCode JumpKey = KeyCode.Space;

        public FpsMotorState State { get; private set; } = FpsMotorState.GroundedJog;
        public Vector3 Velocity => _body != null ? _body.linearVelocity : Vector3.zero;
        public float HorizontalSpeed { get; private set; }
        public bool IsGrounded { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool IsSliding { get; private set; }
        public bool IsCrouching { get; private set; }
        public bool JustLanded { get; private set; }
        public float SlideTime { get; private set; }

        private Rigidbody _body;
        private float _capsuleDefaultHeight;
        private float _crouchTimer;
        private float _jumpBuffer;
        private float _mantleTimer;
        private Vector2 _input;
        private Vector3 _mantleTarget;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _body.freezeRotation = true;
            _body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            var capsule = GetComponent<CapsuleCollider>();
            if (capsule != null)
                _capsuleDefaultHeight = capsule.height;
        }

        private void Update()
        {
            _input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (Input.GetKeyDown(JumpKey))
                _jumpBuffer = 0.15f;
            _jumpBuffer = Mathf.Max(0f, _jumpBuffer - Time.deltaTime);
        }

        private void FixedUpdate()
        {
            IsGrounded = CheckGrounded();
            JustLanded = false;

            if (IsGrounded && State == FpsMotorState.AirborneFall)
                JustLanded = true;

            ResolveState();

            if (State == FpsMotorState.TraversingCable)
                return;

            if (State == FpsMotorState.LedgeClimb)
            {
                ApplyMantle();
                return;
            }

            ApplyHorizontal();
            ApplyGravity();
            TryStartSlide();
            ApplySlideDecay();
            TryJump();
            TryMantle();

            HorizontalSpeed = new Vector2(_body.linearVelocity.x, _body.linearVelocity.z).magnitude;
        }

        private void ResolveState()
        {
            if (State == FpsMotorState.PropelledByPad)
            {
                _crouchTimer -= Time.fixedDeltaTime;
                if (_crouchTimer <= 0f)
                    State = IsGrounded ? FpsMotorState.GroundedJog : FpsMotorState.AirborneFall;
                return;
            }

            if (State == FpsMotorState.SlideDecelerating)
                return;

            if (!IsGrounded)
            {
                State = _body.linearVelocity.y > 0.2f ? FpsMotorState.AirborneJump : FpsMotorState.AirborneFall;
                return;
            }

            IsSprinting = Input.GetKey(SprintKey) && _input.y > 0.1f && !IsCrouching;
            IsCrouching = Input.GetKey(KeyCode.C);

            if (IsCrouching)
                State = FpsMotorState.GroundedJog;
            else if (IsSprinting)
                State = FpsMotorState.TacticalSprint;
            else
                State = FpsMotorState.GroundedJog;
        }

        private void ApplyHorizontal()
        {
            float speed = State == FpsMotorState.SlideDecelerating ? SlideEntryImpulse
                : IsCrouching ? CrouchSpeed
                : IsSprinting ? SprintSpeed
                : WalkSpeed;
            speed *= SpeedScale;

            Vector3 forward = Quaternion.Euler(0f, transform.eulerAngles.y, 0f) * Vector3.forward;
            Vector3 right = new Vector3(forward.z, 0f, -forward.x);
            Vector3 desired = forward * _input.y + right * _input.x;
            if (desired.sqrMagnitude > 1f)
                desired.Normalize();

            float acceleration = MoveAcceleration * (IsGrounded ? 1f : AirControl);

            Vector3 velocity = _body.linearVelocity;
            velocity.x += desired.x * acceleration * Time.fixedDeltaTime;
            velocity.z += desired.z * acceleration * Time.fixedDeltaTime;

            float horizontal = new Vector2(velocity.x, velocity.z).magnitude;
            if (horizontal > speed && State != FpsMotorState.SlideDecelerating)
            {
                float k = speed / horizontal;
                velocity.x *= k;
                velocity.z *= k;
            }

            _body.linearVelocity = new Vector3(velocity.x, _body.linearVelocity.y, velocity.z);
        }

        private void ApplyGravity()
        {
            Vector3 velocity = _body.linearVelocity;
            float gravity = Gravity + (!IsGrounded && velocity.y < 0f ? ExtraGravityWhenFalling : 0f);
            velocity.y -= gravity * Time.fixedDeltaTime;
            _body.linearVelocity = velocity;
        }

        private void TryStartSlide()
        {
            if (!IsGrounded || IsSliding)
                return;

            float planarSpeed = new Vector2(_body.linearVelocity.x, _body.linearVelocity.z).magnitude;
            bool pressed = Input.GetKeyDown(SlideKey) || Input.GetKey(SlideKey);
            if (!pressed || planarSpeed < SprintSpeed * SpeedScale * 0.6f)
                return;

            IsSliding = true;
            SlideTime = 0f;
            State = FpsMotorState.SlideDecelerating;
            SetCapsuleHeight(_capsuleDefaultHeight * SlideHeightFactor);

            Vector3 planar = new Vector3(_body.linearVelocity.x, 0f, _body.linearVelocity.z).normalized;
            Vector3 velocity = planar * SlideEntryImpulse;
            velocity.y = _body.linearVelocity.y;
            _body.linearVelocity = velocity;
        }

        private void ApplySlideDecay()
        {
            if (!IsSliding)
                return;

            SlideTime += Time.fixedDeltaTime;

            Vector3 velocity = _body.linearVelocity;
            float decel = SlideFriction * Time.fixedDeltaTime;
            float planar = new Vector2(velocity.x, velocity.z).magnitude;
            if (planar > decel)
            {
                Vector3 dir = new Vector3(velocity.x, 0f, velocity.z).normalized;
                velocity.x = dir.x * (planar - decel);
                velocity.z = dir.z * (planar - decel);
                _body.linearVelocity = velocity;
            }

            bool jumpCancel = _jumpBuffer > 0f && SlideTime >= SlideMinDuration;
            bool expired = SlideTime >= SlideMaxDuration || planar <= WalkSpeed * SpeedScale * 1.05f;
            if (jumpCancel || expired)
                EndSlide(jumpCancel);
        }

        private void EndSlide(bool bunnyHop)
        {
            IsSliding = false;
            SetCapsuleHeight(_capsuleDefaultHeight);
            State = IsGrounded ? FpsMotorState.GroundedJog : FpsMotorState.AirborneFall;

            if (bunnyHop)
            {
                Vector3 velocity = _body.linearVelocity;
                velocity.y = JumpVelocity;
                _body.linearVelocity = velocity;
                _jumpBuffer = 0f;
            }
        }

        private void TryJump()
        {
            if (_jumpBuffer <= 0f || !IsGrounded || IsSliding)
                return;

            _jumpBuffer = 0f;
            Vector3 velocity = _body.linearVelocity;
            velocity.y = JumpVelocity;
            _body.linearVelocity = velocity;
        }

        private void TryMantle()
        {
            if (!IsGrounded || _mantleTimer > 0f)
                return;

            if (Mathf.Abs(_input.y) < 0.5f)
                return;

            Vector3 origin = transform.position + Vector3.up * 0.6f;
            Vector3 dir = transform.forward;
            if (!Physics.Raycast(origin, dir, out RaycastHit hit, MantleCheckDistance, GroundMask))
                return;

            Vector3 probe = hit.point + Vector3.up * 0.35f;
            if (Physics.Raycast(probe, Vector3.up, out RaycastHit top, 1.2f, GroundMask))
                return;

            _mantleTarget = new Vector3(hit.point.x + dir.x * 0.45f, hit.point.y + 1.05f, hit.point.z + dir.z * 0.45f);
            _mantleTimer = 0.35f;
            State = FpsMotorState.LedgeClimb;
        }

        private void ApplyMantle()
        {
            _mantleTimer -= Time.fixedDeltaTime;
            transform.position = Vector3.MoveTowards(transform.position, _mantleTarget, MantlePullSpeed * Time.fixedDeltaTime);

            if (_mantleTimer <= 0f || Vector3.Distance(transform.position, _mantleTarget) < 0.05f)
            {
                _mantleTimer = 0f;
                State = FpsMotorState.GroundedJog;
                if (_body != null)
                    _body.linearVelocity = Vector3.zero;
            }
        }

        private void SetCapsuleHeight(float height)
        {
            var capsule = GetComponent<CapsuleCollider>();
            if (capsule == null)
                return;
            capsule.height = Mathf.Max(1f, height);
            capsule.center = new Vector3(0f, capsule.height * 0.5f, 0f);
        }

        private bool CheckGrounded()
        {
            if (IsSliding && State == FpsMotorState.SlideDecelerating)
                return true;

            return Physics.Raycast(transform.position + Vector3.up * 0.5f,
                Vector3.down, GroundCheckDistance + 0.5f, GroundMask, QueryTriggerInteraction.Ignore);
        }

        public void Launch(Vector3 impulse, float controlLockSeconds)
        {
            if (_body == null)
                return;

            IsSliding = false;
            SetCapsuleHeight(_capsuleDefaultHeight);
            _body.linearVelocity = impulse;
            State = FpsMotorState.PropelledByPad;
            _crouchTimer = controlLockSeconds;
        }

        public void AttachToCable()
        {
            State = FpsMotorState.TraversingCable;
            if (_body != null)
                _body.useGravity = false;
        }

        public void DetachFromCable(Vector3 releaseVelocity)
        {
            State = FpsMotorState.AirborneFall;
            if (_body != null)
            {
                _body.useGravity = true;
                _body.linearVelocity = releaseVelocity;
            }
        }
    }
}
