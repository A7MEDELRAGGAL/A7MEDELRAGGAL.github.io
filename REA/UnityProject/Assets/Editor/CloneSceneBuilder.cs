using System.IO;
using Clone.Items;
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

            EnsureFolder(MaterialsDir);
            SpawnHarvestNodes(island);

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
                player.transform.position = new Vector3(0f, 1.5f, 6f);
                player.AddComponent<CloneHostSpawner>();
                player.AddComponent<CloneHarvestInteractor>();
            }
            else
            {
                Debug.LogWarning("[Clone] Player prefab missing; spawn skipped");
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[Clone] Created {ScenePath}");
        }

        private static void SpawnHarvestNodes(GameObject island)
        {
            Vector3 center = island.transform.position;
            float top = center.y + island.transform.localScale.y * 0.5f;

            for (int i = 0; i < 6; i++)
            {
                AddNode(PrimitiveType.Capsule,
                    new Vector3(center.x - 6f + (i % 3) * 5.2f, top + 1.6f, center.z - 6f + (i / 3) * 9f),
                    new Vector3(0.4f, 1.6f, 0.4f), new Color(0.42f, 0.26f, 0.13f),
                    CloneHarvestKind.Wood, CloneItemCatalog.WoodLog, 4);
            }

            for (int i = 0; i < 4; i++)
            {
                AddNode(PrimitiveType.Cube,
                    new Vector3(center.x - 5f + i * 3.4f, top + 0.45f, center.z + 5.5f),
                    new Vector3(1f, 0.8f, 1f), new Color(0.45f, 0.45f, 0.48f),
                    CloneHarvestKind.Stone, CloneItemCatalog.Stone, 3);
            }

            for (int i = 0; i < 4; i++)
            {
                AddNode(PrimitiveType.Cylinder,
                    new Vector3(center.x + 3f, top + 0.08f, center.z - 4.5f + i * 3f),
                    new Vector3(0.5f, 0.08f, 0.5f), new Color(0.2f, 0.6f, 0.25f),
                    CloneHarvestKind.Fiber, CloneItemCatalog.Fiber, 5);
            }
        }

        private static GameObject AddNode(PrimitiveType type, Vector3 position, Vector3 scale, Color color,
            CloneHarvestKind kind, int yieldItemId, int yieldAmount)
        {
            var nodeObject = GameObject.CreatePrimitive(type);
            nodeObject.name = "Harvest" + kind;
            nodeObject.transform.position = position;
            nodeObject.transform.localScale = scale;
            nodeObject.GetComponent<Renderer>().sharedMaterial = CreateNodeMaterial(color, kind);

            var node = nodeObject.AddComponent<CloneHarvestNode>();
            node.Kind = kind;
            node.YieldItemId = yieldItemId;
            node.YieldAmount = yieldAmount;
            nodeObject.AddComponent<NetworkObject>();
            return nodeObject;
        }

        private static Material CreateNodeMaterial(Color color, CloneHarvestKind kind)
        {
            string materialPath = Path.Combine(MaterialsDir, "Harvest_" + kind + ".mat");
            return CreateMaterial(color, materialPath);
        }

        private static Material CreateMaterial(Color color, string materialPath)
        {
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

            material.color = color;
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();

            return material;
        }

        private static Material CreateOceanMaterial()
        {
            return CreateMaterial(new Color(0.05f, 0.25f, 0.45f, 1f), Path.Combine(MaterialsDir, "Ocean.mat"));
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
