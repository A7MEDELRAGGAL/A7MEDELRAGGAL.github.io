using Clone.Creatures;
using Clone.Player;
using FishNet.Object;
using UnityEngine;

namespace Clone.Items
{
    public class CloneMelee : CloneWeapon
    {
        [Header("Reach")]
        public float RadiusPadding = 0.6f;
        public float SelfKnockbackScale = 0.2f;

        public override bool CanUse(NetworkObject user)
        {
            return !IsSwinging;
        }

        public override void ServerUse(NetworkObject user)
        {
            if (!BeginSwing(user))
                return;
        }

        protected override void ServerResolveSwing()
        {
            NetworkObject user = PendingUser;
            PendingUser = null;

            var player = user != null ? user.GetComponent<ClonePlayer>() : null;
            if (player == null)
                return;

            Vector3 origin = player.transform.position + Vector3.up * 1f;
            Vector3 aim = player.CameraRig != null
                ? Quaternion.Euler(0f, player.CameraRig.Yaw, 0f) * Vector3.forward
                : player.transform.forward;
            if (aim.sqrMagnitude < 0.001f)
                aim = Vector3.forward;

            CloneItemDefinition definition = Definition;
            float damage = definition != null ? definition.Damage : 10f;
            float knockback = definition != null ? definition.Knockback : 4f;
            float range = (definition != null ? definition.AttackRange : 2.2f) + RadiusPadding;

            Collider[] hits = Physics.OverlapSphere(origin, range, HitMask,
                QueryTriggerInteraction.Ignore);

            bool struckAnything = false;
            for (int i = 0; i < hits.Length; i++)
            {
                Collider hit = hits[i];
                if (hit == null)
                    continue;

                Vector3 closest = hit.ClosestPoint(origin);
                Vector3 toTarget = closest - origin;
                if (toTarget.sqrMagnitude > range * range)
                    continue;
                if (Vector3.Dot(toTarget.normalized, aim) < 0.15f)
                    continue;

                var victimPlayer = hit.GetComponentInParent<ClonePlayer>();
                if (victimPlayer != null)
                {
                    if (victimPlayer == player)
                        continue;

                    struckAnything = true;
                    victimPlayer.Vitals.ServerDamage(Mathf.RoundToInt(damage));
                    victimPlayer.ServerKnockback((toTarget.normalized + Vector3.up * 0.3f).normalized * knockback);
                    continue;
                }

                var creature = hit.GetComponentInParent<CloneCreature>();
                if (creature != null && !creature.IsDead)
                {
                    struckAnything = true;
                    creature.ServerTakeDamage(damage, user, toTarget.normalized * knockback);
                    continue;
                }

                var worldItem = hit.GetComponentInParent<CloneItem>();
                if (worldItem != null && !worldItem.IsTaken)
                {
                    struckAnything = true;
                    var rigidbody = worldItem.GetComponent<Rigidbody>();
                    if (rigidbody != null)
                        rigidbody.AddForce(toTarget.normalized * knockback, ForceMode.Impulse);
                }
            }

            if (!struckAnything)
                player.ServerKnockback(-aim * (knockback * SelfKnockbackScale));
        }
    }
}
