using UnityEngine;

namespace Clone.Fps
{
    public class FpsTargetDummy : MonoBehaviour
    {
        public float MaxHealth = 100f;
        public float RespawnSeconds = 3f;
        public Color HitFlashColor = Color.red;
        public Color HeadshotFlashColor = new Color(1f, 0.6f, 0f);

        public float CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }

        public System.Action<float, bool> OnDamaged;
        public System.Action OnDied;

        private float _respawnTimer;
        private Renderer[] _renderers;
        private Color[] _originalColors;
        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;

        private void Awake()
        {
            CurrentHealth = MaxHealth;
            _renderers = GetComponentsInChildren<Renderer>();
            _originalColors = new Color[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
                _originalColors[i] = _renderers[i].material.color;
            _spawnPosition = transform.position;
            _spawnRotation = transform.rotation;
        }

        private void Update()
        {
            if (!IsDead)
                return;

            _respawnTimer -= Time.deltaTime;
            if (_respawnTimer <= 0f)
                Respawn();
        }

        public void TakeDamage(float amount, Vector3 hitPoint, bool headshot)
        {
            if (IsDead)
                return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            OnDamaged?.Invoke(amount, headshot);
            StartCoroutine(FlashHit(headshot));

            if (CurrentHealth <= 0f)
                Die();
        }

        private System.Collections.IEnumerator FlashHit(bool headshot)
        {
            Color flash = headshot ? HeadshotFlashColor : HitFlashColor;
            for (int i = 0; i < _renderers.Length; i++)
                if (_renderers[i] != null)
                    _renderers[i].material.color = flash;

            yield return new WaitForSeconds(0.08f);

            for (int i = 0; i < _renderers.Length; i++)
                if (_renderers[i] != null)
                    _renderers[i].material.color = _originalColors[i];
        }

        private void Die()
        {
            IsDead = true;
            _respawnTimer = RespawnSeconds;
            OnDied?.Invoke();

            foreach (var col in GetComponentsInChildren<Collider>())
                col.enabled = false;

            foreach (var r in _renderers)
                if (r != null)
                    r.enabled = false;
        }

        private void Respawn()
        {
            IsDead = false;
            CurrentHealth = MaxHealth;
            transform.position = _spawnPosition;
            transform.rotation = _spawnRotation;

            foreach (var col in GetComponentsInChildren<Collider>())
                col.enabled = true;

            foreach (var r in _renderers)
                if (r != null)
                    r.enabled = true;
        }
    }
}
