using System.IO;
using Clone.Economy;
using Clone.Player;
using FishNet.Component.Transforming;
using FishNet.Object;
using UnityEditor;
using UnityEngine;

namespace Clone.Editor
{
    public static class ClonePlayerPrefabBuilder
    {
        private const string PrefabPath = "Assets/Prefabs/Player.prefab";

        [MenuItem("Clone/Prefabs/Create Player Prefab")]
        public static void CreatePlayerPrefab()
        {
            var root = new GameObject("Player");

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
                body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);

            var camera = new GameObject("PlayerCamera");
            camera.transform.SetParent(root.transform, false);
            camera.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            camera.AddComponent<Camera>();
            camera.AddComponent<AudioListener>();
            camera.AddComponent<ClonePlayerCamera>();

            var rigidbody = root.AddComponent<Rigidbody>();
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

            root.AddComponent<ClonePlayer>();
            root.AddComponent<ClonePlayerMovement>();
            root.AddComponent<ClonePlayerVitals>();
            root.AddComponent<ClonePlayerInventory>();
            root.AddComponent<CloneCurrency>();

            root.AddComponent<NetworkObject>();
            root.AddComponent<NetworkTransform>();

            EnsureFolder("Assets/Prefabs");

            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(PrefabPath);
            }

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);

            if (prefab == null)
            {
                Debug.LogError($"[Clone] Failed to save prefab at {PrefabPath}");
                return;
            }

            EditorGUIUtility.PingObject(prefab);
            Debug.Log($"[Clone] Created {PrefabPath}");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent ?? "Assets", leaf);
        }
    }
}
