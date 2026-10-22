using System.IO;
using Clone.Net;
using Clone.Player;
using Clone.SaveLoad;
using Clone.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using FishNet.Object;
using FishNet.Transporting;

namespace Clone.Editor
{
    public static class CloneSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Main.unity";
        private const string MaterialsDir = "Assets/Materials";

        [MenuItem("Clone/Scene/Create Main Scene")]
        public static void CreateMainScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            EnsureFolder(MaterialsDir);
            EnsureFolder("Assets/Scenes");

            var ocean = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ocean.name = "Ocean";
            ocean.transform.position = new Vector3(0f, 0f, 0f);
            ocean.transform.localScale = new Vector3(100f, 1f, 100f);
            ocean.GetComponent<Renderer>().sharedMaterial = CreateOceanMaterial();

            var island = GameObject.CreatePrimitive(PrimitiveType.Cube);
            island.name = "StarterIsland";
            island.transform.position = new Vector3(0f, 1f, 20f);
            island.transform.localScale = new Vector3(20f, 2f, 20f);

            var spawnPoints = new GameObject("PlayerSpawnPoints");
            var spawnA = new GameObject("SpawnA");
            spawnA.transform.SetParent(spawnPoints.transform, false);
            spawnA.transform.localPosition = new Vector3(0f, 2f, 20f);
            var spawnB = new GameObject("SpawnB");
            spawnB.transform.SetParent(spawnPoints.transform, false);
            spawnB.transform.localPosition = new Vector3(4f, 2f, 20f);

            var hud = new GameObject("CloneNetworkHud");
            hud.AddComponent<CloneNetworkHud>();

            var network = new GameObject("NetworkManager");
            network.AddComponent<FishNet.Managing.NetworkManager>();
            network.AddComponent<Tugboat>();

            var managers = new GameObject("Managers");
            var dayNight = managers.AddComponent<CloneDayNightManager>();
            var sun = new GameObject("Sun");
            sun.transform.SetParent(managers.transform, false);
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(50f, 170f, 0f);
            dayNight.sun = light;
            var spawnManager = managers.AddComponent<CloneSpawnManager>();
            spawnManager.PlayerSpawnPoints = spawnPoints.transform;
            managers.AddComponent<CloneAutoSaver>();

            var runManager = new GameObject("RunManager");
            runManager.AddComponent<CloneRunManager>();
            runManager.AddComponent<NetworkObject>();

            ClonePlayerPrefabBuilder.CreatePlayerPrefab();
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
            if (playerPrefab != null)
            {
                var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
                player.transform.position = new Vector3(0f, 1.5f, 0f);
                player.AddComponent<CloneHostSpawner>();
            }
            else
            {
                Debug.LogWarning("[Clone] Player prefab missing; spawn skipped");
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[Clone] Created {ScenePath}");
        }

        private static Material CreateOceanMaterial()
        {
            string materialPath = Path.Combine(MaterialsDir, "Ocean.mat");

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, materialPath);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
            }

            material.color = new Color(0.05f, 0.25f, 0.45f, 1f);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();

            return material;
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
