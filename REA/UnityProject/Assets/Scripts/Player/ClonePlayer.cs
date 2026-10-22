using Clone.Crafting;
using Clone.CraftingUI;
using Clone.World;
using FishNet.Object;
using UnityEngine;

namespace Clone.Player
{
    public class ClonePlayer : NetworkBehaviour
    {
        public static ClonePlayer LocalPlayer { get; private set; }

        [SyncVar]
        public bool Afk;

        [SyncVar]
        public bool TutorialDone;

        public ClonePlayerMovement Movement { get; private set; }
        public ClonePlayerCamera CameraRig { get; private set; }
        public ClonePlayerVitals Vitals { get; private set; }
        public ClonePlayerInventory Inventory { get; private set; }
        public ClonePlayerHolding Holding { get; private set; }
        public CloneJournal Journal { get; private set; }
        public ClonePlayerEquipment Equipment { get; private set; }
        public CloneCraftingManager Crafting { get; private set; }
        public CloneBoat CurrentBoat { get; private set; }
        public bool IsDrivingClient { get; private set; }

        private void Awake()
        {
            Movement = GetComponent<ClonePlayerMovement>();
            Vitals = GetComponent<ClonePlayerVitals>();
            Inventory = GetComponent<ClonePlayerInventory>();
            Holding = GetComponent<ClonePlayerHolding>();
            Journal = GetComponent<CloneJournal>();
            Equipment = GetComponent<ClonePlayerEquipment>();
            if (Equipment == null)
                Equipment = gameObject.AddComponent<ClonePlayerEquipment>();
            Crafting = GetComponent<CloneCraftingManager>();
            if (Crafting == null)
                Crafting = gameObject.AddComponent<CloneCraftingManager>();
            CameraRig = GetComponentInChildren<ClonePlayerCamera>();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (IsOwner)
            {
                LocalPlayer = this;
                if (CameraRig != null)
                    CameraRig.Activate();
                if (Crafting != null)
                    CloneCraftingUI.EnsureFor(Crafting);
            }
        }

        public override void OnStopClient()
        {
            if (IsOwner)
            {
                if (CameraRig != null)
                    CameraRig.Deactivate();
                if (LocalPlayer == this)
                    LocalPlayer = null;
            }
            base.OnStopClient();
        }

        [TargetRpc]
        public void TeleportTarget(Vector3 position)
        {
            if (Movement != null)
                Movement.TeleportToLand(position);
        }

        [TargetRpc]
        public void KnockbackTarget(Vector3 force)
        {
            if (Movement != null)
                Movement.ApplyKnockback(force);
        }

        [Server]
        public void ServerKnockback(Vector3 force)
        {
            KnockbackTarget(force);
        }

        [ServerRpc]
        public void ServerSkipTutorial()
        {
            TutorialDone = true;
        }

        [ServerRpc]
        public void ServerSetAfk(bool afk)
        {
            Afk = afk;
        }

        [ServerRpc]
        public void ServerRequestBoatSeat(NetworkObject boat)
        {
            if (boat == null || CurrentBoat != null || Movement == null)
                return;

            var target = boat.GetComponent<CloneBoat>();
            if (target == null || target.HasDriver || !CloneBoatTrigger.IsInsideServer(target, this))
                return;

            CurrentBoat = target;

            Movement.SetBoat(target.gameObject, true);
            Movement.SetDriver(target.DriverSeat, true);
            target.ServerSetDriver(this);

            ConfirmBoatSeatTarget(target.NetworkObject, true);
        }

        [ServerRpc]
        public void ServerReleaseBoatSeat()
        {
            if (CurrentBoat == null)
                return;

            CloneBoat boat = CurrentBoat;
            CurrentBoat = null;

            if (Movement != null)
            {
                Movement.StopKnockback();
                Movement.SetDriver(null, false);
                Movement.SetBoat(null, false);
                Movement.TeleportToLand(GetBoatExitPosition(boat));
            }

            boat.ServerSetDriver(null);
            ConfirmBoatSeatTarget(boat.NetworkObject, false);
        }

        [TargetRpc]
        private void ConfirmBoatSeatTarget(NetworkObject boat, bool accepted)
        {
            var target = boat != null ? boat.GetComponent<CloneBoat>() : null;
            if (target == null)
                return;

            target.SetLocalDriver(accepted);
            IsDrivingClient = accepted;
        }

        private Vector3 GetBoatExitPosition(CloneBoat boat)
        {
            if (boat == null)
                return SpawnPointUtility.GetRespawnPosition();

            var water = CloneWaterManager.Instance;
            Vector3 forward = boat.transform.forward;

            for (int i = 0; i < 8; i++)
            {
                Vector3 candidate = boat.transform.position + forward * (3f + i * 1.5f);
                candidate.y = boat.transform.position.y + 20f;

                if (Physics.Raycast(candidate, Vector3.down, out RaycastHit hit, 80f,
                        ~0, QueryTriggerInteraction.Ignore))
                {
                    if (water == null || water.GetDepth(hit.point) <= 0.2f)
                        return hit.point + Vector3.up * 0.2f;
                }

                forward = Quaternion.Euler(0f, 45f * (i + 1), 0f) * boat.transform.forward;
            }

            return SpawnPointUtility.GetRespawnPosition();
        }

        [ObserversRpc]
        public void ObserverHit(NetworkObject attacker, int damage)
        {
            if (Vitals != null)
                Vitals.NotifyHit(attacker, damage);
        }
    }
}
