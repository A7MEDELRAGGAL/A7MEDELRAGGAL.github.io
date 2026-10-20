using UnityEngine;

namespace Clone.World
{
    public class CloneBoatMotor : MonoBehaviour
    {
        [Header("Thrust")]
        public float MotorPower = 1200f;
        public float ReverseFactor = 0.45f;
        public float MaxForwardSpeed = 12f;
        public float BoostMultiplier = 1.8f;
        public float ThrottleResponse = 3.5f;

        [Header("Steering")]
        public float RudderPower = 1400f;
        public float ReverseSteerFactor = 0.6f;
        public float SteerResponse = 5f;

        [Header("Hull")]
        public float ThrustDepth = 1.2f;
        public float LinearDragInWater = 0.9f;
        public float AngularDragInWater = 2.4f;
        public float GroundedDrag = 0.35f;

        public float Throttle { get; private set; }
        public float Steer { get; private set; }
        public bool Boosting { get; private set; }
        public bool IsReversing => Throttle < -0.01f;

        private float _targetThrottle;
        private float _targetSteer;

        public void SetInput(float throttle, float steer)
        {
            SetInput(throttle, steer, Boosting);
        }

        public void SetInput(float throttle, float steer, bool boost)
        {
            _targetThrottle = Mathf.Clamp(throttle, -1f, 1f);
            _targetSteer = Mathf.Clamp(steer, -1f, 1f);
            Boosting = boost;
        }

        public void ClearInput()
        {
            _targetThrottle = 0f;
            _targetSteer = 0f;
            Boosting = false;
        }

        public void Apply(Rigidbody body, float waterDepth, float deltaTime)
        {
            if (body == null || body.isKinematic)
                return;

            Throttle = Mathf.MoveTowards(Throttle, _targetThrottle, ThrottleResponse * deltaTime);
            Steer = Mathf.MoveTowards(Steer, _targetSteer, SteerResponse * deltaTime);

            float depthRatio = Mathf.Clamp01(waterDepth / Mathf.Max(0.01f, ThrustDepth));

            if (depthRatio > 0f)
            {
                ApplyThrust(body, depthRatio);
                ApplyRudder(body, depthRatio);
                ApplyWaterDrag(body, depthRatio);
            }
            else
            {
                ApplyGroundedDrag(body);
            }
        }

        private void ApplyThrust(Rigidbody body, float depthRatio)
        {
            Vector3 forward = transform.forward;
            float speed = Vector3.Dot(body.linearVelocity, forward);
            float limit = MaxForwardSpeed * (IsReversing ? ReverseFactor : 1f) * depthRatio;
            float overSpeed = Mathf.Clamp01((Mathf.Abs(speed) - limit) / Mathf.Max(0.5f, limit));
            float power = MotorPower * Mathf.Lerp(1f, 0.3f, overSpeed);
            if (Boosting && !IsReversing)
                power *= BoostMultiplier;

            float signedPower = IsReversing ? -power * ReverseFactor : power;
            body.AddForce(forward * (signedPower * depthRatio), ForceMode.Force);
        }

        private void ApplyRudder(Rigidbody body, float depthRatio)
        {
            float speed = Vector3.Dot(body.linearVelocity, transform.forward);
            float speedRatio = Mathf.Clamp01(Mathf.Abs(speed) / Mathf.Max(1f, MaxForwardSpeed));
            float authority = Mathf.Lerp(0.25f, 1f, speedRatio);
            float steerScale = speed < -0.1f ? -ReverseSteerFactor : 1f;
            body.AddTorque(transform.up * (Steer * RudderPower * authority * steerScale * depthRatio), ForceMode.Force);
        }

        private void ApplyWaterDrag(Rigidbody body, float depthRatio)
        {
            body.AddForce(-body.linearVelocity * (LinearDragInWater * depthRatio), ForceMode.Force);
            body.AddTorque(-body.angularVelocity * (AngularDragInWater * depthRatio), ForceMode.Force);
        }

        private void ApplyGroundedDrag(Rigidbody body)
        {
            body.AddForce(-body.linearVelocity * GroundedDrag, ForceMode.Force);
            body.AddTorque(-body.angularVelocity * GroundedDrag, ForceMode.Force);
        }
    }
}
