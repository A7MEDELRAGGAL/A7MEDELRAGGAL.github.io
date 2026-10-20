using UnityEngine;

namespace Clone.Fps
{
    public class FpsWeaponView : MonoBehaviour
    {
        [Header("Sway")]
        public float MouseLagResponsiveness = 8f;
        public float MaxTranslationalSway = 0.04f;
        public float MaxAngularSway = 6f;

        [Header("Bob")]
        public float LocomotionBobFrequency = 12f;
        public float LocomotionBobAmplitude = 0.02f;

        [Header("Breath")]
        public float IdleBreathingRate = 1.8f;
        public float IdleBreathingMagnitude = 0.003f;

        [Header("ADS")]
        public Vector3 HipPosition;
        public Vector3 AdsPosition;
        public float AdsLerpSpeed = 14f;

        public bool IsAds { get; set; }

        private FpsPlayerMotor _motor;
        private Vector3 _targetOffset;
        private Vector3 _currentOffset;
        private Quaternion _targetRotation;
        private Quaternion _currentRotation;
        private float _bobTimer;
        private float _breathTimer;

        private void Awake()
        {
            _motor = GetComponentInParent<FpsPlayerMotor>();
            HipPosition = transform.localPosition;
        }

        private void Update()
        {
            HandleLookLag();
            HandleLocomotionBob();
            HandleBreathing();
            HandleAdsPosition();
        }

        private void HandleLookLag()
        {
            float mx = Input.GetAxis("Mouse X");
            float my = Input.GetAxis("Mouse Y");

            Vector3 lagOffset = new Vector3(
                -mx * MaxTranslationalSway * 2f,
                my * MaxTranslationalSway * 2f,
                0f);
            lagOffset = Vector3.ClampMagnitude(lagOffset, MaxTranslationalSway);

            Quaternion lagRot = Quaternion.Euler(
                my * MaxAngularSway,
                -mx * MaxAngularSway,
                -mx * MaxAngularSway * 0.5f);

            _targetOffset = Vector3.Lerp(_targetOffset, lagOffset, Time.deltaTime * MouseLagResponsiveness);
            _targetRotation = Quaternion.Slerp(_targetRotation, lagRot, Time.deltaTime * MouseLagResponsiveness);
        }

        private void HandleLocomotionBob()
        {
            if (_motor == null || !_motor.IsGrounded || _motor.HorizontalSpeed < 0.5f)
            {
                _bobTimer = 0f;
                return;
            }

            _bobTimer += Time.deltaTime * LocomotionBobFrequency;
            Vector3 bob = new Vector3(
                Mathf.Cos(_bobTimer) * LocomotionBobAmplitude * 0.5f,
                Mathf.Sin(_bobTimer * 2f) * LocomotionBobAmplitude,
                0f);
            _targetOffset += bob;
        }

        private void HandleBreathing()
        {
            if (_motor != null && _motor.HorizontalSpeed > 0.5f)
                return;

            _breathTimer += Time.deltaTime * IdleBreathingRate;
            _targetOffset += new Vector3(0f, Mathf.Sin(_breathTimer) * IdleBreathingMagnitude, 0f);
        }

        private void HandleAdsPosition()
        {
            Vector3 basePos = IsAds ? AdsPosition : HipPosition;
            Vector3 desired = basePos + _targetOffset;

            transform.localPosition = Vector3.Lerp(transform.localPosition, desired, Time.deltaTime * AdsLerpSpeed);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, _targetRotation, Time.deltaTime * AdsLerpSpeed);

            _targetOffset = Vector3.Lerp(_targetOffset, Vector3.zero, Time.deltaTime * MouseLagResponsiveness * 0.5f);
        }
    }
}
