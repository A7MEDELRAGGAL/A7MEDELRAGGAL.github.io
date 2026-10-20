using FishNet;
using FishNet.Managing;
using UnityEngine;

namespace Clone.Net
{
    public class CloneNetworkHud : MonoBehaviour
    {
        public bool ShowGui = true;

        private void OnGUI()
        {
            if (!ShowGui || InstanceFinder.NetworkManager == null)
                return;

            const float width = 160f;
            const float height = 30f;
            float x = 10f;
            float y = 10f;

            GUI.enabled = !InstanceFinder.IsServerInitialized && !InstanceFinder.IsClientInitialized;
            if (GUI.Button(new Rect(x, y, width, height), "Host (Server + Client)"))
                StartHost();
            y += height + 6f;

            if (GUI.Button(new Rect(x, y, width, height), "Client"))
                StartClient();
            y += height + 6f;

            GUI.enabled = InstanceFinder.IsServerInitialized || InstanceFinder.IsClientInitialized;
            if (GUI.Button(new Rect(x, y, width, height), "Stop"))
                StopAll();
            GUI.enabled = true;

            y += height + 10f;
            GUI.Label(new Rect(x, y, 300f, height),
                $"Server: {InstanceFinder.IsServerInitialized}  Client: {InstanceFinder.IsClientInitialized}");
        }

        private void StartHost()
        {
            NetworkManager nm = InstanceFinder.NetworkManager;
            if (nm == null)
                return;
            nm.ServerManager.StartConnection();
            nm.ClientManager.StartConnection();
        }

        private void StartClient()
        {
            NetworkManager nm = InstanceFinder.NetworkManager;
            if (nm != null)
                nm.ClientManager.StartConnection();
        }

        private void StopAll()
        {
            NetworkManager nm = InstanceFinder.NetworkManager;
            if (nm == null)
                return;
            nm.ClientManager.StopConnection();
            nm.ServerManager.StopConnection(true);
        }
    }
}
