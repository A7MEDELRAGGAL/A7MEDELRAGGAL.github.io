using FishNet.Object;
using UnityEngine;

namespace Clone.Items
{
    public abstract class CloneWeapon : CloneTool
    {
        [Header("Melee profile")]
        public float WindupSeconds = 0.16f;
        public float ActiveSeconds = 0.12f;
        public float RecoverSeconds = 0.24f;
        public LayerMask HitMask = ~0;

        public float CooldownRemaining { get; protected set; }
        public bool IsSwinging => CooldownRemaining > 0f;

        protected float WindupRemaining { get; private set; }
        protected NetworkObject PendingUser { get; private set; }

        protected virtual void Update()
        {
            if (!IsSpawned || !IsServer)
                return;

            if (CooldownRemaining > 0f)
                CooldownRemaining -= Time.deltaTime;

            if (WindupRemaining <= 0f)
                return;

            WindupRemaining -= Time.deltaTime;
            if (WindupRemaining <= 0f)
                ServerResolveSwing();
        }

        protected float TotalSwingSeconds()
        {
            return Mathf.Max(0f, WindupSeconds) + Mathf.Max(0f, ActiveSeconds) + Mathf.Max(0f, RecoverSeconds);
        }

        protected bool BeginSwing(NetworkObject user)
        {
            if (IsSwinging)
                return false;

            PendingUser = user;
            WindupRemaining = Mathf.Max(0f, WindupSeconds);
            CooldownRemaining = TotalSwingSeconds();
            return true;
        }

        protected abstract void ServerResolveSwing();
    }
}
