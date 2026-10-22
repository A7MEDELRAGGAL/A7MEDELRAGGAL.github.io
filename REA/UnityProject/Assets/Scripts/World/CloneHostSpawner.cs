using FishNet;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

namespace Clone.World
{
    // Host-mode test spawner: the player is a scene-placed NetworkObject; this
    // grants ownership to the first connection that reaches the server.
    public class CloneHostSpawner : NetworkBehaviour
    {
        public override void OnStartServer()
        {
            base.OnStartServer();
            InstanceFinder.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            if (InstanceFinder.NetworkManager != null && InstanceFinder.ServerManager != null)
                InstanceFinder.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
        }

        private void OnRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs args)
        {
            if (args.ConnectionState != RemoteConnectionState.Started)
                return;

            NetworkObject playerObject = GetComponent<NetworkObject>();
            if (playerObject == null)
                return;

            if (playerObject.Owner.IsValid)
                return;

            playerObject.GiveOwnership(connection);
            Debug.Log($"[Clone] Granted player ownership to connection {connection.ClientId}");
        }
    }
}
