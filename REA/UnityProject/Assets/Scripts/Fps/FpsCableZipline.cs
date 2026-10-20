using UnityEngine;

namespace Clone.Fps
{
    public class FpsCableZipline : MonoBehaviour
    {
        public Transform StartPoint;
        public Transform EndPoint;
        public float GlideSpeed = 14f;
        public float ExitForwardBoost = 3f;
        public KeyCode AttachKey = KeyCode.E;
        public float AttachRadius = 2.5f;

        private FpsPlayerMotor _attachedMotor;
        private Rigidbody _attachedBody;
        private float _progress;

        private void Update()
        {
            if (_attachedMotor == null)
            {
                TryDetectAttach();
                return;
            }

            HandleGlide();
        }

        private void TryDetectAttach()
        {
            if (!Input.GetKeyDown(AttachKey) || StartPoint == null || EndPoint == null)
                return;

            Vector3 closest = FindClosestPointOnSegment(transform.position);
            if (Vector3.Distance(transform.position, closest) > AttachRadius)
                return;

            var motor = FindNearestMotor();
            if (motor == null)
                return;

            Attach(motor);
        }

        private FpsPlayerMotor FindNearestMotor()
        {
            var all = FindObjectsByType<FpsPlayerMotor>(FindObjectsSortMode.None);
            FpsPlayerMotor best = null;
            float bestDist = AttachRadius;
            foreach (var m in all)
            {
                float d = Vector3.Distance(m.transform.position, transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = m;
                }
            }
            return best;
        }

        private void Attach(FpsPlayerMotor motor)
        {
            _attachedMotor = motor;
            _attachedBody = motor.GetComponent<Rigidbody>();
            _progress = 0f;
            motor.AttachToCable();
            if (_attachedBody != null)
            {
                _attachedBody.linearVelocity = Vector3.zero;
                _attachedBody.angularVelocity = Vector3.zero;
            }
        }

        private void HandleGlide()
        {
            if (StartPoint == null || EndPoint == null)
            {
                Detach(Vector3.zero);
                return;
            }

            float totalLength = Vector3.Distance(StartPoint.position, EndPoint.position);
            if (totalLength < 0.01f)
            {
                Detach(Vector3.zero);
                return;
            }

            _progress += (GlideSpeed / totalLength) * Time.deltaTime;

            if (_progress >= 1f)
            {
                Vector3 dir = (EndPoint.position - StartPoint.position).normalized;
                Detach(dir * ExitForwardBoost + Vector3.up * 1.5f);
                return;
            }

            Vector3 position = Vector3.Lerp(StartPoint.position, EndPoint.position, _progress);
            _attachedMotor.transform.position = position;

            Vector3 lookDir = (EndPoint.position - StartPoint.position).normalized;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion look = Quaternion.LookRotation(lookDir);
                _attachedMotor.transform.rotation = Quaternion.Slerp(
                    _attachedMotor.transform.rotation, look, Time.deltaTime * 8f);
            }
        }

        private void Detach(Vector3 releaseVelocity)
        {
            if (_attachedMotor != null)
            {
                _attachedMotor.DetachFromCable(releaseVelocity);
                _attachedMotor = null;
            }
            _attachedBody = null;
        }

        private Vector3 FindClosestPointOnSegment(Vector3 point)
        {
            Vector3 a = StartPoint.position;
            Vector3 b = EndPoint.position;
            Vector3 ab = b - a;
            float t = Mathf.Clamp01(Vector3.Dot(point - a, ab) / Mathf.Max(0.001f, ab.sqrMagnitude));
            return a + ab * t;
        }
    }
}
