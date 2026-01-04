#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Transports.UTP;
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
    /// Editor utility to automatically setup the game scene and prefabs.
    /// Access via Tools > EtherDomes menu.
    /// </summary>
    public static class EtherDomesSetup
    {
        private const string PREFABS_PATH = "Assets/_Project/Prefabs";
        private const string SCENES_PATH = "Assets/_Project/Scenes";

        [MenuItem("Tools/EtherDomes/Setup Complete Test Scene", false, 0)]
        public static void SetupCompleteTestScene()
        {
            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // Setup all systems
            CreateNetworkManager();
            CreateGameManager();
            CreatePlayerPrefabAsset();
            CreateEnemyPrefabAsset();
            CreateTestEnvironment();
            CreateTestEnemies();
            CreateTestUI();
            
            // Save scene
            EnsureDirectoryExists(SCENES_PATH);
            string scenePath = $"{SCENES_PATH}/TestScene.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            
            Debug.Log("[EtherDomesSetup] ✅ Test scene created successfully!");
            Debug.Log("[EtherDomesSetup] Press Play, then click 'Start Host' in the Game view to test.");
            
            EditorUtility.DisplayDialog("Setup Complete", 
                "Test scene created!\n\n" +
                "1. Press Play\n" +
                "2. Click 'Start Host' button\n" +
                "3. Use WASD to move\n" +
                "4. Press Tab to cycle targets\n\n" +
                "To test multiplayer:\n" +
                "- Build the project\n" +
                "- Run build as Host\n" +
                "- In Editor, click 'Join as Client'", 
                "OK");
        }

        [MenuItem("Tools/EtherDomes/Create Network Manager", false, 20)]
        public static GameObject CreateNetworkManager()
        {
            // Check if already exists
            var existing = Object.FindFirstObjectByType<NetworkManager>();
            if (existing != null)
            {
                Debug.Log("[EtherDomesSetup] NetworkManager already exists");
                return existing.gameObject;
            }

            var go = new GameObject("NetworkManager");
            
            // Add Unity Netcode components
            var networkManager = go.AddComponent<NetworkManager>();
            var transport = go.AddComponent<UnityTransport>();
            
            // Configure transport
            transport.ConnectionData.Address = "127.0.0.1";
            transport.ConnectionData.Port = 7777;
            
            // Use SerializedObject to properly configure NetworkManager
            // NetworkConfig is a public field with [HideInInspector], and NetworkTransport is inside it
            var serializedObject = new SerializedObject(networkManager);
            
            // Find NetworkConfig and set its NetworkTransport field
            var configProp = serializedObject.FindProperty("NetworkConfig");
            if (configProp != null)
            {
                // Set the transport reference inside NetworkConfig
                var transportProp = configProp.FindPropertyRelative("NetworkTransport");
                if (transportProp != null)
                {
                    transportProp.objectReferenceValue = transport;
                }
                
                // Enable connection approval
                var approvalProp = configProp.FindPropertyRelative("ConnectionApproval");
                if (approvalProp != null)
                {
                    approvalProp.boolValue = true;
                }
            }
            
            serializedObject.ApplyModifiedProperties();
            
            // Add our custom components
            var sessionManager = go.AddComponent<NetworkSessionManager>();
            
            // Create and assign player prefab - use SerializedObject for this too
            var playerPrefab = CreatePlayerPrefabAsset();
            if (playerPrefab != null)
            {
                // Re-fetch serialized object to ensure it's up to date
                serializedObject.Update();
                configProp = serializedObject.FindProperty("NetworkConfig");
                if (configProp != null)
                {
                    var playerPrefabProp = configProp.FindPropertyRelative("PlayerPrefab");
                    if (playerPrefabProp != null)
                    {
                        playerPrefabProp.objectReferenceValue = playerPrefab;
                    }
                }
                serializedObject.ApplyModifiedProperties();
            }
            
            // Register enemy prefab with NetworkManager
            var enemyPrefab = CreateEnemyPrefabAsset();
            if (enemyPrefab != null)
            {
                // Add to NetworkPrefabs list
                serializedObject.Update();
                configProp = serializedObject.FindProperty("NetworkConfig");
                if (configProp != null)
                {
                    var prefabsProp = configProp.FindPropertyRelative("Prefabs");
                    if (prefabsProp != null)
                    {
                        var prefabsListProp = prefabsProp.FindPropertyRelative("m_Prefabs");
                        if (prefabsListProp != null && prefabsListProp.isArray)
                        {
                            // Add enemy prefab to the list
                            prefabsListProp.InsertArrayElementAtIndex(prefabsListProp.arraySize);
                            var newElement = prefabsListProp.GetArrayElementAtIndex(prefabsListProp.arraySize - 1);
                            var prefabField = newElement.FindPropertyRelative("Prefab");
                            if (prefabField != null)
                            {
                                prefabField.objectReferenceValue = enemyPrefab;
                            }
                        }
                    }
                }
                serializedObject.ApplyModifiedProperties();
            }

            Debug.Log("[EtherDomesSetup] ✅ NetworkManager created");
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
            
            // Create child objects for each system
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
            
            // Check if prefab already exists
            var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                Debug.Log("[EtherDomesSetup] Player prefab already exists");
                return existingPrefab;
            }

            // Create player GameObject
            var playerGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerGO.name = "Player";
            
            // Remove default collider (CharacterController will handle collision)
            Object.DestroyImmediate(playerGO.GetComponent<CapsuleCollider>());
            
            // Add required components
            playerGO.AddComponent<NetworkObject>();
            playerGO.AddComponent<NetworkTransform>();
            playerGO.AddComponent<CharacterController>();
            playerGO.AddComponent<PlayerController>();
            playerGO.AddComponent<NetworkPlayer>();
            
            // Configure CharacterController
            var cc = playerGO.GetComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.5f;
            cc.center = new Vector3(0, 1f, 0);
            
            // Set layer
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer == -1)
            {
                Debug.LogWarning("[EtherDomesSetup] 'Player' layer not found. Create it in Tags & Layers.");
            }
            else
            {
                playerGO.layer = playerLayer;
            }
            
            // Create material
            var material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.2f, 0.6f, 1f); // Blue color
            playerGO.GetComponent<MeshRenderer>().material = material;
            
            // Save as prefab
            var prefab = PrefabUtility.SaveAsPrefabAsset(playerGO, prefabPath);
            Object.DestroyImmediate(playerGO);
            
            Debug.Log("[EtherDomesSetup] ✅ Player prefab created at " + prefabPath);
            return prefab;
        }

        private static void CreateTestEnvironment()
        {
            // Create ground plane
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(10, 1, 10);
            
            var groundMat = new Material(Shader.Find("Standard"));
            groundMat.color = new Color(0.3f, 0.5f, 0.3f); // Green
            ground.GetComponent<MeshRenderer>().material = groundMat;
            
            // Create some obstacles for testing
            for (int i = 0; i < 5; i++)
            {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = $"Obstacle_{i}";
                cube.transform.position = new Vector3(
                    Random.Range(-20f, 20f),
                    0.5f,
                    Random.Range(-20f, 20f)
                );
                
                var cubeMat = new Material(Shader.Find("Standard"));
                cubeMat.color = new Color(0.6f, 0.4f, 0.2f); // Brown
                cube.GetComponent<MeshRenderer>().material = cubeMat;
            }
            
            // Adjust camera
            var camera = Camera.main;
            if (camera != null)
            {
                camera.transform.position = new Vector3(0, 15, -15);
                camera.transform.rotation = Quaternion.Euler(45, 0, 0);
            }
            
            // Add directional light if not present
            var light = Object.FindFirstObjectByType<Light>();
            if (light == null)
            {
                var lightGO = new GameObject("Directional Light");
                light = lightGO.AddComponent<Light>();
                light.type = LightType.Directional;
                light.transform.rotation = Quaternion.Euler(50, -30, 0);
            }

            Debug.Log("[EtherDomesSetup] ✅ Test environment created");
        }

        private static void CreateTestUI()
        {
            // Create Canvas
            var canvasGO = new GameObject("TestUI");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Add NetworkTestUI component
            canvasGO.AddComponent<NetworkTestUI>();
            
            // Add CombatTestUI component (Target Frame + Action Bar)
            canvasGO.AddComponent<CombatTestUI>();

            Debug.Log("[EtherDomesSetup] ✅ Test UI created (Network + Combat)");
        }

        private static void CreateTestEnemies()
        {
            // Load enemy prefab
            var enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PREFABS_PATH}/Enemies/BasicEnemy.prefab");
            if (enemyPrefab == null)
            {
                Debug.LogWarning("[EtherDomesSetup] Enemy prefab not found. Creating it first.");
                enemyPrefab = CreateEnemyPrefabAsset();
            }
            
            if (enemyPrefab == null)
            {
                Debug.LogError("[EtherDomesSetup] Failed to create enemy prefab");
                return;
            }
            
            // Create enemy spawn points container with NetworkObject and EnemySpawner
            var enemiesContainer = new GameObject("Enemies");
            enemiesContainer.AddComponent<NetworkObject>();
            enemiesContainer.AddComponent<EnemySpawner>();
            
            // Create 5 enemies at different positions
            Vector3[] positions = new Vector3[]
            {
                new Vector3(10, 0, 10),
                new Vector3(-10, 0, 10),
                new Vector3(15, 0, -5),
                new Vector3(-15, 0, -5),
                new Vector3(0, 0, 20)
            };
            
            string[] names = new string[]
            {
                "Goblin Scout",
                "Goblin Warrior",
                "Orc Grunt",
                "Skeleton",
                "Dark Mage"
            };
            
            for (int i = 0; i < positions.Length; i++)
            {
                var enemyGO = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab);
                enemyGO.name = names[i];
                enemyGO.transform.position = positions[i];
                enemyGO.transform.SetParent(enemiesContainer.transform);
                
                // Set display name via SerializedObject
                var enemy = enemyGO.GetComponent<EnemyClass>();
                if (enemy != null)
                {
                    var serializedEnemy = new SerializedObject(enemy);
                    var nameProp = serializedEnemy.FindProperty("_displayName");
                    if (nameProp != null)
                    {
                        nameProp.stringValue = names[i];
                    }
                    var levelProp = serializedEnemy.FindProperty("_level");
                    if (levelProp != null)
                    {
                        levelProp.intValue = i + 1; // Levels 1-5
                    }
                    serializedEnemy.ApplyModifiedProperties();
                }
            }
            
            Debug.Log($"[EtherDomesSetup] ✅ Created {positions.Length} test enemies with EnemySpawner");
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

        [MenuItem("Tools/EtherDomes/Add Combat UI to Scene", false, 30)]
        public static void AddCombatUIToScene()
        {
            // Find existing TestUI canvas or create one
            var existingUI = Object.FindFirstObjectByType<CombatTestUI>();
            if (existingUI != null)
            {
                UnityEngine.Debug.Log("[EtherDomesSetup] CombatTestUI already exists in scene");
                return;
            }
            
            // Find canvas
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasGO = new GameObject("TestUI");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
            
            canvas.gameObject.AddComponent<CombatTestUI>();
            UnityEngine.Debug.Log("[EtherDomesSetup] ✅ CombatTestUI added to scene");
            
            EditorUtility.DisplayDialog("Combat UI Added", 
                "CombatTestUI has been added to the scene.\n\n" +
                "Press Play, Start Host, then:\n" +
                "- Tab to select target\n" +
                "- Keys 1-4 to use abilities", 
                "OK");
        }

        [MenuItem("Tools/EtherDomes/Setup Physics Layers", false, 40)]
        public static void SetupPhysicsLayers()
        {
            // This needs to be done manually in Unity
            EditorUtility.DisplayDialog("Setup Physics Layers",
                "Please manually create these layers in Edit > Project Settings > Tags and Layers:\n\n" +
                "Layer 8: Player\n" +
                "Layer 9: Enemy\n" +
                "Layer 10: Projectile\n\n" +
                "Then in Edit > Project Settings > Physics:\n" +
                "- Uncheck Player vs Player collision",
                "OK");
        }

        [MenuItem("Tools/EtherDomes/Create Enemy Prefab", false, 23)]
        public static GameObject CreateEnemyPrefabAsset()
        {
            EnsureDirectoryExists($"{PREFABS_PATH}/Enemies");
            string prefabPath = $"{PREFABS_PATH}/Enemies/BasicEnemy.prefab";
            
            // Check if prefab already exists
            var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                Debug.Log("[EtherDomesSetup] Enemy prefab already exists");
                return existingPrefab;
            }

            // Create enemy GameObject
            var enemyGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemyGO.name = "BasicEnemy";
            
            // Add required components
            enemyGO.AddComponent<NetworkObject>();
            enemyGO.AddComponent<EnemyClass>();
            
            // Configure collider for targeting
            var collider = enemyGO.GetComponent<CapsuleCollider>();
            collider.height = 2f;
            collider.radius = 0.5f;
            collider.center = new Vector3(0, 1f, 0);
            
            // Set layer
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer == -1)
            {
                Debug.LogWarning("[EtherDomesSetup] 'Enemy' layer not found. Using default layer.");
            }
            else
            {
                enemyGO.layer = enemyLayer;
            }
            
            // Create red material
            var material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.9f, 0.2f, 0.2f); // Red color
            enemyGO.GetComponent<MeshRenderer>().material = material;
            
            // Create target indicator (circle under enemy)
            var indicatorGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            indicatorGO.name = "TargetIndicator";
            indicatorGO.transform.SetParent(enemyGO.transform);
            indicatorGO.transform.localPosition = new Vector3(0, 0.01f, 0);
            indicatorGO.transform.localScale = new Vector3(1.5f, 0.01f, 1.5f);
            
            // Remove collider from indicator
            Object.DestroyImmediate(indicatorGO.GetComponent<CapsuleCollider>());
            
            // Yellow indicator material
            var indicatorMat = new Material(Shader.Find("Standard"));
            indicatorMat.color = new Color(1f, 0.9f, 0.2f); // Yellow
            indicatorMat.SetFloat("_Mode", 3); // Transparent mode
            indicatorMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            indicatorMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            indicatorMat.EnableKeyword("_ALPHABLEND_ON");
            indicatorMat.renderQueue = 3000;
            indicatorGO.GetComponent<MeshRenderer>().material = indicatorMat;
            indicatorGO.SetActive(false); // Hidden by default
            
            // Link indicator to Enemy component via SerializedObject
            var enemy = enemyGO.GetComponent<EnemyClass>();
            var serializedEnemy = new SerializedObject(enemy);
            var indicatorProp = serializedEnemy.FindProperty("_targetIndicator");
            if (indicatorProp != null)
            {
                indicatorProp.objectReferenceValue = indicatorGO;
            }
            var meshProp = serializedEnemy.FindProperty("_meshRenderer");
            if (meshProp != null)
            {
                meshProp.objectReferenceValue = enemyGO.GetComponent<MeshRenderer>();
            }
            serializedEnemy.ApplyModifiedProperties();
            
            // Save as prefab
            var prefab = PrefabUtility.SaveAsPrefabAsset(enemyGO, prefabPath);
            Object.DestroyImmediate(enemyGO);
            
            Debug.Log("[EtherDomesSetup] ✅ Enemy prefab created at " + prefabPath);
            return prefab;
        }
    }
}
#endif
