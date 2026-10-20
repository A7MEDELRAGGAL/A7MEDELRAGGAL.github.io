using Clone.Items;
using FishNet.Object;
using UnityEngine;

namespace Clone.Creatures
{
    public class CloneFish : CloneCreature
    {
        [Header("Catch")]
        public int FishItemId = CloneItemCatalog.Mackerel;
        public float Worth = 12f;
        public float ReelResistance = 1f;
        public float HookHoldDistance = 3.5f;

        [Header("Swim")]
        public float SwimSpeed = 2.4f;
        public float PanicSpeed = 5.5f;

        public bool IsHooked { get; private set; }
        public NetworkObject HookOwner { get; private set; }

        public bool IsCatchable => !IsDead && !IsHooked;

        [Server]
        public bool ServerHook(NetworkObject rodOwner)
        {
            if (IsDead || IsHooked)
                return false;

            IsHooked = true;
            HookOwner = rodOwner;

            if (Body != null)
            {
                Body.isKinematic = true;
                Body.velocity = Vector3.zero;
            }

            return true;
        }

        [Server]
        public void ServerUnhook()
        {
            IsHooked = false;
            HookOwner = null;

            if (Body != null && IsSpawned)
                Body.isKinematic = false;
        }

        [Server]
        public void ServerDrag(Vector3 destination, float deltaTime)
        {
            if (!IsHooked || Body == null)
                return;

            transform.position = Vector3.Lerp(transform.position, destination, 1f);
            Body.position = transform.position;

            Vector3 look = destination - transform.position;
            if (look.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(look.normalized, Vector3.up), 6f * deltaTime);
        }

        [Server]
        public int ResolveInventoryItemId(int fallback)
        {
            return FishItemId != CloneInventorySlot.EmptyId ? FishItemId : fallback;
        }

        protected override void ServerDie(NetworkObject killer)
        {
            ServerUnhook();
            base.ServerDie(killer);
        }

        protected override void ApplyMovement(float deltaTime)
        {
            if (IsHooked)
                return;

            base.ApplyMovement(deltaTime);

            if (Body == null || IsDead)
                return;

            float speed = IsHooked ? PanicSpeed : SwimSpeed;
            float blend = Mathf.Clamp01(2f * deltaTime);

            Vector3 velocity = Body.linearVelocity;
            Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
            Vector3 limited = Vector3.ClampMagnitude(horizontal, speed);
            Body.linearVelocity = new Vector3(
                Mathf.Lerp(velocity.x, limited.x, blend),
                Mathf.MoveTowards(velocity.y, 0f, 4f * deltaTime),
                Mathf.Lerp(velocity.z, limited.z, blend));
        }
    }
}
