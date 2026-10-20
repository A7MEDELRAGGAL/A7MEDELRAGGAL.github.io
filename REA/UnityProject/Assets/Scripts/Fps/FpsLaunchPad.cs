using UnityEngine;

namespace Clone.Fps
{
    [RequireComponent(typeof(Collider))]
    public class FpsLaunchPad : MonoBehaviour
    {
        public Vector3 LaunchVelocity = new Vector3(0f, 15.5f, 12f);
        public float ControlLockSeconds = 0.4f;
        public bool UseTransformForward = true;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            var motor = other.GetComponentInParent<FpsPlayerMotor>();
            if (motor == null)
                return;

            Vector3 impulse = UseTransformForward
                ? transform.TransformDirection(LaunchVelocity)
                : LaunchVelocity;

            motor.Launch(impulse, ControlLockSeconds);
        }
    }
}
