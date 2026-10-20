using Clone.Player;
using System.Collections.Generic;
using UnityEngine;

namespace Clone.World
{
    [RequireComponent(typeof(Collider))]
    public class CloneBoatTrigger : MonoBehaviour
    {
        public CloneBoat Boat;

        private static readonly Dictionary<int, HashSet<ClonePlayer>> Occupants =
            new Dictionary<int, HashSet<ClonePlayer>>();

        private void Reset()
        {
            Collider trigger = GetComponent<Collider>();
            if (trigger != null)
                trigger.isTrigger = true;
        }

        private void Awake()
        {
            if (Boat == null)
                Boat = GetComponentInParent<CloneBoat>();
        }

        private void OnDisable()
        {
            if (Boat != null)
                Occupants.Remove(Boat.GetInstanceID());
        }

        private void OnTriggerEnter(Collider other)
        {
            ClonePlayer player = other != null ? other.GetComponentInParent<ClonePlayer>() : null;
            if (player == null)
                return;

            if (IsServerPlayer(player))
                SetOccupant(player, true);

            if (player.IsOwner && player.Movement != null && !player.Movement.IsDriver)
                player.Movement.SetBoat(Boat != null ? Boat.gameObject : gameObject, true);
        }

        private void OnTriggerExit(Collider other)
        {
            ClonePlayer player = other != null ? other.GetComponentInParent<ClonePlayer>() : null;
            if (player == null || Boat == null)
                return;

            if (IsServerPlayer(player))
                SetOccupant(player, false);

            if (player.IsOwner && player.Movement != null && !player.Movement.IsDriver)
                player.Movement.SetBoat(null, false);
        }

        public static bool IsInsideServer(CloneBoat boat, ClonePlayer player)
        {
            if (boat == null || player == null || !player.IsSpawned || !player.IsServer)
                return false;

            return Occupants.TryGetValue(boat.GetInstanceID(), out HashSet<ClonePlayer> occupants)
                && occupants.Contains(player);
        }

        private void SetOccupant(ClonePlayer player, bool inside)
        {
            if (Boat == null)
                return;

            int key = Boat.GetInstanceID();
            if (!Occupants.TryGetValue(key, out HashSet<ClonePlayer> occupants))
            {
                if (!inside)
                    return;
                occupants = new HashSet<ClonePlayer>();
                Occupants[key] = occupants;
            }

            if (inside)
                occupants.Add(player);
            else
                occupants.Remove(player);
        }

        private static bool IsServerPlayer(ClonePlayer player)
        {
            return player != null && player.IsSpawned && player.IsServer;
        }
    }
}
