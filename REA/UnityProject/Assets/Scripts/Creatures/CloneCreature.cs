using Clone.Player;
using FishNet.Object;
using UnityEngine;

namespace Clone.Creatures
{
    [RequireComponent(typeof(Rigidbody))]
    public class CloneCreature : NetworkBehaviour
    {
        [Header("Health")]
        public float MaxHealth = 40f;

        [Header("Movement")]
        public float WanderSpeed = 1.1f;
        public float ChaseSpeed = 3.6f;
        public float TurnSpeed = 4f;
        public float DetectRadius = 12f;

        [Header("Attack")]
        public float AttackDamage = 8f;
        public float AttackRange = 1.9f;
        public float AttackCooldown = 1.4f;
        public float AttackKnockback = 4f;

        [Header("Environment")]
        public LayerMask GroundMask = ~0;

        public float Health { get; private set; }
        public bool IsDead { get; private set; }
        public ClonePlayer Target { get; private set; }

        protected Rigidbody Body { get; private set; }

        private float _attackTimer;
        private float _wanderTimer;
        private Vector3 _wanderDirection = Vector3.forward;
        private bool _fleeing;

        protected virtual void Awake()
        {
            Body = GetComponent<Rigidbody>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            Health = MaxHealth;
            _wanderTimer = Random.Range(0.5f, 3f);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (!IsServer)
            {
                var body = GetComponent<Rigidbody>();
                if (body != null)
                    body.isKinematic = true;
            }
        }

        protected virtual void FixedUpdate()
        {
            if (!IsSpawned || !IsServer || IsDead || Body == null)
                return;

            TickAttackTimer(Time.fixedDeltaTime);
            UpdateTarget();
            ApplyMovement(Time.fixedDeltaTime);
        }

        private void TickAttackTimer(float deltaTime)
        {
            if (_attackTimer > 0f)
                _attackTimer -= deltaTime;
        }

        private void UpdateTarget()
        {
            Target = null;

            ClonePlayer nearest = null;
            float nearestSqr = DetectRadius * DetectRadius;
            Collider[] hits = Physics.OverlapSphere(transform.position, DetectRadius,
                ~0, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hits.Length; i++)
            {
                var player = hits[i] != null ? hits[i].GetComponentInParent<ClonePlayer>() : null;
                if (player == null)
                    continue;

                float sqr = (player.transform.position - transform.position).sqrMagnitude;
                if (sqr >= nearestSqr)
                    continue;

                nearestSqr = sqr;
                nearest = player;
            }

            Target = nearest;
        }

        protected virtual void ApplyMovement(float deltaTime)
        {
            Vector3 velocity = Body.linearVelocity;

            if (Target != null && Vector3.Distance(Target.transform.position, transform.position) <= AttackRange
                && _attackTimer <= 0f)
            {
                TryAttack(Target);
            }

            Vector3 desired;
            float speed;

            if (_fleeing && Target != null)
            {
                desired = (transform.position - Target.transform.position).normalized;
                speed = ChaseSpeed;
            }
            else if (Target != null)
            {
                desired = (Target.transform.position - transform.position).normalized;
                desired.y = 0f;
                speed = ChaseSpeed;
            }
            else
            {
                _wanderTimer -= deltaTime;
                if (_wanderTimer <= 0f)
                {
                    _wanderTimer = Random.Range(1.5f, 4f);
                    float angle = Random.Range(0f, Mathf.PI * 2f);
                    _wanderDirection = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                }

                desired = _wanderDirection;
                speed = WanderSpeed;
            }

            if (desired.sqrMagnitude < 0.0001f)
                return;

            desired.Normalize();
            Vector3 horizontal = new Vector3(desired.x, 0f, desired.z).normalized;
            Vector3 target = horizontal * speed;
            float blend = Mathf.Clamp01(TurnSpeed * deltaTime);

            velocity.x = Mathf.Lerp(velocity.x, target.x, blend);
            velocity.z = Mathf.Lerp(velocity.z, target.z, blend);
            Body.linearVelocity = velocity;
        }

        protected virtual void TryAttack(ClonePlayer target)
        {
            if (target == null || AttackDamage <= 0f)
                return;

            _attackTimer = Mathf.Max(0.1f, AttackCooldown);
            target.Vitals.ServerDamage(Mathf.RoundToInt(AttackDamage));
            target.ServerKnockback((target.transform.position - transform.position).normalized * AttackKnockback);
        }

        [Server]
        public virtual void ServerTakeDamage(float amount, NetworkObject attacker, Vector3 knockback)
        {
            if (IsDead || amount <= 0f)
                return;

            Health = Mathf.Max(0f, Health - amount);
            _fleeing = true;

            if (Body != null && knockback.sqrMagnitude > 0.0001f && !Body.isKinematic)
                Body.AddForce(knockback, ForceMode.Impulse);

            if (Health <= 0f)
                ServerDie(attacker);
        }

        [Server]
        protected virtual void ServerDie(NetworkObject killer)
        {
            IsDead = true;
            Despawn(true);
        }
    }
}
