using FishNet;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Clone.World
{
    public class CloneRunManager : NetworkBehaviour
    {
        public static CloneRunManager Instance { get; private set; }

        [SyncVar]
        private int _deathsThisRun;

        [SyncVar]
        private bool _runActive;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            _deathsThisRun = 0;
            _runActive = true;
        }

        public static void NotifyPlayerDeath()
        {
            if (Instance == null)
                return;

            if (!InstanceFinder.IsServerInitialized)
                return;

            Instance.ServerRecordDeath();
        }

        [Server]
        private void ServerRecordDeath()
        {
            _deathsThisRun++;
        }

        [ServerRpc(RequireOwnership = false)]
        public void ServerStartRun()
        {
            _deathsThisRun = 0;
            _runActive = true;
        }

        [ServerRpc(RequireOwnership = false)]
        public void ServerRestartRun()
        {
            if (InstanceFinder.NetworkManager == null)
                return;

            Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            SceneReference sceneReference = new SceneReference();
            sceneReference.SetScene(scene);

            InstanceFinder.NetworkManager.SceneManager.LoadScene(sceneReference, LoadSceneMode.Single);
        }

        public int DeathsThisRun => _deathsThisRun;
        public bool RunActive => _runActive;
    }
}
