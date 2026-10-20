using UnityEngine;

namespace Clone.Fps
{
    public class FpsCameraRig : MonoBehaviour
    {
        [Header("Look")]
        public float MouseSensitivity = 2.2f;
        public float PitchClamp = 85f;

        [Header("FOV")]
        public Camera TargetCamera;
        public float BaseFov = 90f;
        public float SprintFovKick = 8f;
        public float SlideFovKick = 4f;
        public float AdsFov = 60f;
        public float FovLerpSpeed = 10f;

        [Header("Head Bob")]
        public float BobFrequencySprint = 14f;
        public float BobAmplitudeSprint = 0.035f;
        public float BobFrequencyWalk = 9f;
        public float BobAmplitudeWalk = 0.018f;
        public float LandShakeAmplitude = 0.09f;
        public float LandShakeDuration = 0.18f;

        public float Yaw { get; private set; }
        public float Pitch { get; private set; }
        public bool IsAds { get; set; }

    public void AddRecoil(float pitchAmount)
    {
        Pitch = Mathf.Clamp(Pitch + pitchAmount, -80f, 80f);
    }

    public void AddYawRecoil(float yawAmount)
    {
        Yaw += yawAmount;
    }

        private FpsPlayerMotor _motor;
        private float _bobTimer;
        private float _landShakeTimer;
        private Vector3 _cameraLocalBase;

        private void Awake()
        {
            _motor = GetComponentInParent<FpsPlayerMotor>();
            if (TargetCamera == null)
                TargetCamera = GetComponent<Camera>();
            if (TargetCamera != null)
                _cameraLocalBase = TargetCamera.transform.localPosition;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            HandleLook();
            HandleFov();
            HandleBobAndShake();
        }

        private void HandleLook()
        {
            float mx = Input.GetAxis("Mouse X") * MouseSensitivity;
            float my = Input.GetAxis("Mouse Y") * MouseSensitivity;

            Yaw += mx;
            Pitch = Mathf.Clamp(Pitch - my, -PitchClamp, PitchClamp);

            transform.rotation = Quaternion.Euler(0f, Yaw, 0f);
            if (TargetCamera != null)
                TargetCamera.transform.rotation = Quaternion.Euler(Pitch, Yaw, 0f);
        }

        private void HandleFov()
        {
            if (TargetCamera == null)
                return;

            float target = BaseFov;
            if (IsAds)
                target = AdsFov;
            else if (_motor != null)
            {
                if (_motor.State == FpsMotorState.SlideDecelerating)
                    target = BaseFov + SlideFovKick;
                else if (_motor.State == FpsMotorState.TacticalSprint)
                    target = BaseFov + SprintFovKick;
            }

            TargetCamera.fieldOfView = Mathf.Lerp(TargetCamera.fieldOfView, target, Time.deltaTime * FovLerpSpeed);
        }

        private void HandleBobAndShake()
        {
            if (TargetCamera == null || _motor == null)
                return;

            if (_motor.JustLanded)
            {
                float impact = Mathf.Clamp01(_motor.Velocity.y == 0f ? 0.5f : Mathf.Abs(_motor.Velocity.y) / _motor.LandingImpactThreshold);
                _landShakeTimer = LandShakeDuration;
                _cameraLocalBase += Vector3.down * LandShakeAmplitude * impact;
            }

            float amplitude = 0f;
            float frequency = 0f;
            if (_motor.State == FpsMotorState.TacticalSprint)
            {
                amplitude = BobAmplitudeSprint;
                frequency = BobFrequencySprint;
            }
            else if (_motor.State == FpsMotorState.GroundedJog && _motor.HorizontalSpeed > 0.5f)
            {
                amplitude = BobAmplitudeWalk;
                frequency = BobFrequencyWalk;
            }

            if (amplitude > 0f && _motor.IsGrounded)
                _bobTimer += Time.deltaTime * frequency;
            else
                _bobTimer = 0f;

            Vector3 bobOffset = new Vector3(
                Mathf.Cos(_bobTimer) * amplitude * 0.6f,
                Mathf.Sin(_bobTimer * 2f) * amplitude,
                0f);

            Vector3 shakeOffset = Vector3.zero;
            if (_landShakeTimer > 0f)
            {
                _landShakeTimer -= Time.deltaTime;
                shakeOffset = (Vector3.up + Vector3.right) * (LandShakeAmplitude * (_landShakeTimer / LandShakeDuration));
            }

            TargetCamera.transform.localPosition = _cameraLocalBase + bobOffset + shakeOffset;
        }
    }
}
