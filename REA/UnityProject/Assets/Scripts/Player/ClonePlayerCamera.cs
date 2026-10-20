using UnityEngine;

namespace Clone.Player
{
    public class ClonePlayerCamera : MonoBehaviour
    {
        public enum Mode
        {
            ThirdPerson,
            FirstPerson
        }

        [Header("Look")]
        public float Sensitivity = 2.5f;
        public float MinPitch = -80f;
        public float MaxPitch = 80f;

        [Header("Third person")]
        public float Distance = 4f;
        public float ShoulderOffset = 0.6f;
        public LayerMask CollisionMask = ~0;

        [Header("References")]
        public Transform Target;
        public Camera PlayerCamera;

        public float Yaw { get; private set; }
        public float Pitch { get; private set; }
        public Mode CurrentMode { get; private set; } = Mode.ThirdPerson;

        private bool _active;

        private void Awake()
        {
            if (PlayerCamera == null)
                PlayerCamera = GetComponentInChildren<Camera>();
            if (Target == null && transform.parent != null)
                Target = transform.parent;
            Deactivate();
        }

        private void LateUpdate()
        {
            if (!_active)
                return;

            if (Input.GetKeyDown(KeyCode.V))
                CurrentMode = CurrentMode == Mode.ThirdPerson ? Mode.FirstPerson : Mode.ThirdPerson;

            Yaw += Input.GetAxis("Mouse X") * Sensitivity;
            Pitch -= Input.GetAxis("Mouse Y") * Sensitivity;
            Pitch = Mathf.Clamp(Pitch, MinPitch, MaxPitch);

            if (Target == null)
                return;

            Quaternion rotation = Quaternion.Euler(Pitch, Yaw, 0f);

            if (CurrentMode == Mode.FirstPerson)
            {
                transform.position = Target.position + Vector3.up * 1.6f;
                transform.rotation = rotation;
                SetCameraEnabled(false);
            }
            else
            {
                Vector3 pivot = Target.position + Vector3.up * 1.5f;
                Vector3 desired = pivot
                    + rotation * Vector3.back * Distance
                    + rotation * Vector3.right * ShoulderOffset;

                if (Physics.Linecast(pivot, desired, out RaycastHit hit,
                        CollisionMask, QueryTriggerInteraction.Ignore))
                {
                    desired = hit.point + hit.normal * 0.2f;
                }

                transform.position = desired;
                transform.rotation = Quaternion.LookRotation(pivot - desired, Vector3.up);
                SetCameraEnabled(true);
            }
        }

        public void Activate()
        {
            _active = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void Deactivate()
        {
            _active = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SetCameraEnabled(false);
        }

        private void SetCameraEnabled(bool enabled)
        {
            if (PlayerCamera != null && PlayerCamera.enabled != enabled)
                PlayerCamera.enabled = enabled;
        }
    }
}
