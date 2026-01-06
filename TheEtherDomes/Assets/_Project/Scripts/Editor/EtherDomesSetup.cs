#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Mirror;
using EtherDomes.Network;
using EtherDomes.Combat;
using EtherDomes.Classes;
using EtherDomes.Progression;
using EtherDomes.World;
using EtherDomes.Core;
using EtherDomes.Player;
using EtherDomes.UI.Debug;
using EnemyClass = EtherDomes.Enemy.Enemy;
using EnemySpawner = EtherDomes.Enemy.EnemySpawner;

namespace EtherDomes.Editor
{
    /// <summary>
    /// Editor utility to setup the game scene and prefabs using Mirror networking.
    /// Access via Tools > EtherDomes menu.
    /// </summary>
    public static class EtherDomesSetup
    {
        private const string PREFABS_PATH = "Assets/_Project/Prefabs";
        private const string SCENES_PATH = "Assets/_Project/Scenes";

        [MenuItem("Tools/EtherDomes/Setup Complete Test Scene", false, 0)]
        public static void SetupCompleteTestScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            CreateNetworkManager();
            CreateGameManager();
            CreatePlayerPrefabAsset();
            CreateEnemyPrefabAsset();
            CreateTestEnvironment();
            CreateTestEnemies();
            CreateTestUI();
            
            EnsureDirectoryExists(SCENES_PATH);
            string scenePath = $"{SCENES_PATH}/TestScene.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            
            Debug.Log("[EtherDomesSetup] ✅ Test scene created successfully!");
            
            EditorUtility.DisplayDialog("Setup Complete", 
                "Test scene created!\n\n" +
                "1. Press Play\n" +
                "2. Click 'Start Host' button\n" +
                "3. Use WASD to move\n" +
                "4. Press Tab to cycle targets", 
                "OK");
        }

        [MenuItem("Tools/EtherDomes/Create Network Manager", false, 20)]
        public static GameObject CreateNetworkManager()
        {
            var existing = Object.FindFirstObjectByType<MirrorNetworkSessionManager>();
            if (existing != null)
            {
                Debug.Log("[EtherDomesSetup] NetworkManager already exists");
                return existing.gameObject;
            }

            var go = new GameObject("NetworkManager");
            var networkManager = go.AddComponent<MirrorNetworkSessionManager>();
            go.AddComponent<kcp2k.KcpTransport>();
            
            var playerPrefab = CreatePlayerPrefabAsset();
            if (playerPrefab != null)
            {
                networkManager.playerPrefab = playerPrefab;
            }

            Debug.Log("[EtherDomesSetup] ✅ NetworkManager created with Mirror");
            return go;
        }

        [MenuItem("Tools/EtherDomes/Create Game Manager", false, 21)]
        public static GameObject CreateGameManager()
        {
            var existing = Object.FindFirstObjectByType<GameManager>();
            if (existing != null)
            {
                Debug.Log("[EtherDomesSetup] GameManager already exists");
                return existing.gameObject;
            }

            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
            
            CreateSystemChild<TargetSystem>(go, "TargetSystem");
            CreateSystemChild<AbilitySystem>(go, "AbilitySystem");
            CreateSystemChild<AggroSystem>(go, "AggroSystem");
            CreateSystemChild<CombatSystem>(go, "CombatSystem");
            CreateSystemChild<ClassSystem>(go, "ClassSystem");
            CreateSystemChild<ProgressionSystem>(go, "ProgressionSystem");
            CreateSystemChild<LootSystem>(go, "LootSystem");
            CreateSystemChild<EquipmentSystem>(go, "EquipmentSystem");
            CreateSystemChild<WorldManager>(go, "WorldManager");
            CreateSystemChild<DungeonSystem>(go, "DungeonSystem");
            CreateSystemChild<BossSystem>(go, "BossSystem");
            CreateSystemChild<GuildBaseSystem>(go, "GuildBaseSystem");

            Debug.Log("[EtherDomesSetup] ✅ GameManager created with all systems");
            return go;
        }

        private static T CreateSystemChild<T>(GameObject parent, string name) where T : Component
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent.transform);
            return child.AddComponent<T>();
        }

        [MenuItem("Tools/EtherDomes/Create Player Prefab", false, 22)]
        public static GameObject CreatePlayerPrefabAsset()
        {
            EnsureDirectoryExists(PREFABS_PATH);
            string prefabPath = $"{PREFABS_PATH}/Player.prefab";
            
            var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                Debug.Log("[EtherDomesSetup] Player prefab already exists");
                return existingPrefab;
            }

            var playerGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerGO.name = "Player";
            
            Object.DestroyImmediate(playerGO.GetComponent<CapsuleCollider>());
            
            playerGO.AddComponent<NetworkIdentity>();
            playerGO.AddComponent<NetworkTransformReliable>();
            playerGO.AddComponent<CharacterController>();
            playerGO.AddComponent<PlayerController>();
            playerGO.AddComponent<NetworkPlayer>();
            
            var cc = playerGO.GetComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.5f;
            cc.center = new Vector3(0, 1f, 0);
            
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0)
            {
                playerGO.layer = playerLayer;
            }
            
            var material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.2f, 0.6f, 1f);
            playerGO.GetComponent<MeshRenderer>().material = material;
            
            var prefab = PrefabUtility.SaveAsPrefabAsset(playerGO, prefabPath);
            Object.DestroyImmediate(playerGO);
            
            Debug.Log("[EtherDomesSetup] ✅ Player prefab created at " + prefabPath);
            return prefab;
        }

        [MenuItem("Tools/EtherDomes/Create Enemy Prefab", false, 23)]
        public static GameObject CreateEnemyPrefabAsset()
        {
            EnsureDirectoryExists($"{PREFABS_PATH}/Enemies");
            string prefabPath = $"{PREFABS_PATH}/Enemies/BasicEnemy.prefab";
            
            var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                Debug.Log("[EtherDomesSetup] Enemy prefab already exists");
                return existingPrefab;
            }

            var enemyGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemyGO.name = "BasicEnemy";
            
            enemyGO.AddComponent<NetworkIdentity>();
            enemyGO.AddComponent<EnemyClass>();
            
            var collider = enemyGO.GetComponent<CapsuleCollider>();
            collider.height = 2f;
            collider.radius = 0.5f;
            collider.center = new Vector3(0, 1f, 0);
            
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer >= 0)
            {
                enemyGO.layer = enemyLayer;
            }
            
            var material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.9f, 0.2f, 0.2f);
            enemyGO.GetComponent<MeshRenderer>().material = material;
            
            var prefab = PrefabUtility.SaveAsPrefabAsset(enemyGO, prefabPath);
            Object.DestroyImmediate(enemyGO);
            
            Debug.Log("[EtherDomesSetup] ✅ Enemy prefab created at " + prefabPath);
            return prefab;
        }

        private static void CreateTestEnvironment()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(10, 1, 10);
            
            var groundMat = new Material(Shader.Find("Standard"));
            groundMat.color = new Color(0.3f, 0.5f, 0.3f);
            ground.GetComponent<MeshRenderer>().material = groundMat;
            
            var camera = UnityEngine.Camera.main;
            if (camera != null)
            {
                camera.transform.position = new Vector3(0, 15, -15);
                camera.transform.rotation = Quaternion.Euler(45, 0, 0);
            }

            Debug.Log("[EtherDomesSetup] ✅ Test environment created");
        }

        private static void CreateTestUI()
        {
            var canvasGO = new GameObject("TestUI");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            canvasGO.AddComponent<NetworkTestUI>();

            Debug.Log("[EtherDomesSetup] ✅ Test UI created");
        }

        private static void CreateTestEnemies()
        {
            var enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFABS_PATH}/Enemies/BasicEnemy.prefab");
            if (enemyPrefab == null)
            {
                enemyPrefab = CreateEnemyPrefabAsset();
            }
            
            if (enemyPrefab == null) return;
            
            var enemiesContainer = new GameObject("Enemies");
            enemiesContainer.AddComponent<NetworkIdentity>();
            enemiesContainer.AddComponent<EnemySpawner>();
            
            Vector3[] positions = new Vector3[]
            {
                new Vector3(10, 0, 10),
                new Vector3(-10, 0, 10),
                new Vector3(15, 0, -5),
            };
            
            for (int i = 0; i < positions.Length; i++)
            {
                var enemyGO = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab);
                enemyGO.name = $"Enemy_{i}";
                enemyGO.transform.position = positions[i];
                enemyGO.transform.SetParent(enemiesContainer.transform);
            }
            
            Debug.Log($"[EtherDomesSetup] ✅ Created {positions.Length} test enemies");
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string[] folders = path.Split('/');
                string currentPath = folders[0];
                
                for (int i = 1; i < folders.Length; i++)
                {
                    string newPath = currentPath + "/" + folders[i];
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                    }
                    currentPath = newPath;
                }
            }
        }
    }
}
#endif
