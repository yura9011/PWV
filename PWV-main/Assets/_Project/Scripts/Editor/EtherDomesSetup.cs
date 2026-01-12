#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using Unity.Netcode;
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
        private const string BUILD_PATH = "Builds/EtherDomes_Client";

        [MenuItem("Tools/EtherDomes/Setup Network and UI", false, 0)]
        public static void SetupNetworkAndUI()
        {
            NetworkUISetupCreator.CrearEscenaMenuPrincipal();
        }

        [MenuItem("Tools/EtherDomes/Create Network Manager", false, 20)]
        public static GameObject CreateNetworkManager()
        {
            var existing = Object.FindFirstObjectByType<NetworkSessionManager>();
            if (existing != null)
            {
                Debug.Log("[EtherDomesSetup] NetworkManager already exists");
                return existing.gameObject;
            }

            var go = new GameObject("NetworkManager");
            var netManager = go.AddComponent<Unity.Netcode.NetworkManager>();
            var transport = go.AddComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            netManager.NetworkConfig = new Unity.Netcode.NetworkConfig(); // Basic init
            
            go.AddComponent<NetworkSessionManager>();
            go.AddComponent<ConnectionApprovalManager>();
            go.AddComponent<RelayManager>();
            go.AddComponent<LobbyManager>();
            
            var playerPrefab = CreatePlayerPrefabAsset();
            if (playerPrefab != null)
            {
                // In NGO, we add prefabs to the NetworkManager's list (NetworkPrefabs)
                // This is complex to do via script without serializing properly, but we can try basic setup
                // or just leave it for manual assignment
            }

            Debug.Log("[EtherDomesSetup] ✅ NetworkManager created with NGO");
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
            
            playerGO.AddComponent<Unity.Netcode.NetworkObject>();
            playerGO.AddComponent<Unity.Netcode.Components.NetworkTransform>();
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
            
            enemyGO.AddComponent<Unity.Netcode.NetworkObject>();
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
            // Instead of the old NetworkTestUI (green screen), we want the full Main Menu
            // We can reuse the logic from NetworkUISetupCreator if accessible, or just remove the old UI
            // and let the user run correct setup.
            
            // However, to ensure automation:
            NetworkUISetupCreator.CrearEscenaMenuPrincipal();
            
            // Remove the old NetworkTestUI if it was added by above
            var oldUI = Object.FindFirstObjectByType<NetworkTestUI>();
            if (oldUI != null) Object.DestroyImmediate(oldUI.gameObject);

            Debug.Log("[EtherDomesSetup] ✅ Main Menu UI created (Port 3000 ready)");
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
            enemiesContainer.AddComponent<Unity.Netcode.NetworkObject>();
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
        [MenuItem("Tools/EtherDomes/Build Windows Client", false, 100)]
        public static void BuildWindowsClient()
        {
            try
            {
                Debug.Log("[Build] Starting Build Process...");

                // AUTOMATION: Always regenerate the scene before building
                SetupNetworkAndUI();
                
                string scenePath = $"{SCENES_PATH}/MainGame.unity";
                
                // Verify scene exists
                if (!System.IO.File.Exists(scenePath))
                {
                    throw new System.Exception($"Scene file not found at: {scenePath}\nRun Setup Network and UI first.");
                }

                BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
                buildPlayerOptions.scenes = new[] { scenePath };
                buildPlayerOptions.locationPathName = "Builds/EtherDomes_Client.exe";
                buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
                buildPlayerOptions.options = BuildOptions.None;

                Debug.Log($"[Build] Building to: {buildPlayerOptions.locationPathName}");

                BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
                BuildSummary summary = report.summary;

                if (summary.result == BuildResult.Succeeded)
                {
                    Debug.Log("[Build] Build succeeded: " + summary.totalSize + " bytes");
                    EditorUtility.DisplayDialog("Build Success", 
                        $"Build succeeded!\nPath: {buildPlayerOptions.locationPathName}\nTime: {summary.totalTime}", "OK");
                        
                    // Open folder
                    EditorUtility.RevealInFinder(buildPlayerOptions.locationPathName);
                }
                else if (summary.result == BuildResult.Failed)
                {
                    Debug.LogError("[Build] Build failed");
                    throw new System.Exception($"Build Failed with {summary.totalErrors} errors.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Build] CRITICAL ERROR: {e.Message}\n{e.StackTrace}");
                EditorUtility.DisplayDialog("Build Error", 
                    $"Hubo un error al crear la Build:\n\n{e.Message}\n\nRevisa la consola para mas detalles.", "OK");
            }
        }
    }
}
#endif
