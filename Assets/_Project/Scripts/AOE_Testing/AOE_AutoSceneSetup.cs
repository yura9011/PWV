using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace AOETesting
{
    /// <summary>
    /// Automatic scene setup that runs immediately when Unity loads.
    /// This will convert the current scene into an AOE test scene automatically.
    /// </summary>
    [System.Serializable]
    public class AOE_AutoSceneSetup
    {
#if UNITY_EDITOR
        // [InitializeOnLoadMethod] // DISABLED - Causing Unity crash
        static void AutoSetupAOEScene()
        {
            // Only run in play mode or when specifically requested
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        
        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                // Check if we're in a scene that should be converted
                Scene currentScene = SceneManager.GetActiveScene();
                if (ShouldConvertScene(currentScene.name))
                {
                    SetupAOETestingInCurrentScene();
                }
            }
        }
        
        static bool ShouldConvertScene(string sceneName)
        {
            // Convert these scenes automatically
            string[] targetScenes = { "RegionInicio", "Region1", "Mazmorra1_1", "MainGame" };
            foreach (string target in targetScenes)
            {
                if (sceneName.Contains(target))
                {
                    return true;
                }
            }
            return false;
        }
        
        [MenuItem("AOE Testing/Setup Current Scene for AOE Testing")]
        static void SetupCurrentSceneMenuItem()
        {
            SetupAOETestingInCurrentScene();
        }
        
        [MenuItem("AOE Testing/Create AOE Test Scene")]
        static void CreateAOETestScene()
        {
            // Create a new scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            newScene.name = "AOE_TestScene";
            
            SetupAOETestingInCurrentScene();
            
            // Save the scene
            string scenePath = "Assets/_Project/Scenes/AOE_Testing/AOE_TestScene.unity";
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(scenePath));
            EditorSceneManager.SaveScene(newScene, scenePath);
            
            Debug.Log($"[AOE_AutoSceneSetup] Created and saved AOE test scene at {scenePath}");
        }
#endif
        
        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] // DISABLED - Causing Unity crash
        static void RuntimeSetup()
        {
            // This runs when entering play mode
            GameObject setupObject = new GameObject("AOE_RuntimeSetup");
            setupObject.AddComponent<AOE_RuntimeSceneSetup>();
        }
        
        public static void SetupAOETestingInCurrentScene()
        {
            Debug.Log("[AOE_AutoSceneSetup] Setting up AOE testing in current scene...");
            
            // Find or create player
            GameObject player = FindOrCreatePlayer();
            
            // Add AOE components to player
            if (player.GetComponent<AOE_MasterTestController>() == null)
            {
                player.AddComponent<AOE_MasterTestController>();
            }
            
            // Create test enemies
            CreateTestEnemies();
            
            // Setup camera
            SetupCamera();
            
            Debug.Log("[AOE_AutoSceneSetup] AOE testing setup complete!");
            Debug.Log("Use G (Ground), R (Player-centered), T (Cone) to test AOE abilities");
        }
        
        static GameObject FindOrCreatePlayer()
        {
            // Try to find existing player
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) return player;
            
            // Look for common player objects
            string[] playerNames = { "TestPlayer", "TestPlayerWithAbilities", "NetworkPlayer", "Player" };
            foreach (string name in playerNames)
            {
                player = GameObject.Find(name);
                if (player != null)
                {
                    if (player.tag != "Player") player.tag = "Player";
                    return player;
                }
            }
            
            // Look for player components
            var networkPlayer = Object.FindObjectOfType<Mirror.NetworkBehaviour>();
            if (networkPlayer != null)
            {
                if (networkPlayer.tag != "Player") networkPlayer.tag = "Player";
                return networkPlayer.gameObject;
            }
            
            // Create basic player if none found
            player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "AOE_TestPlayer";
            player.tag = "Player";
            player.transform.position = Vector3.zero;
            
            // Add basic movement
            player.AddComponent<SimplePlayerMovement>();
            
            Debug.Log("[AOE_AutoSceneSetup] Created basic test player");
            return player;
        }
        
        static void CreateTestEnemies()
        {
            // Remove existing test enemies
            GameObject[] existingEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in existingEnemies)
            {
                if (enemy.name.StartsWith("AOE_TestEnemy"))
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        Object.DestroyImmediate(enemy);
                    else
#endif
                        Object.Destroy(enemy);
                }
            }
            
            // Create strategic enemy positions
            Vector3[] positions = {
                // Close range (Player-centered AOE)
                new Vector3(3, 0, 0), new Vector3(-3, 0, 0), new Vector3(0, 0, 3), new Vector3(0, 0, -3),
                // Medium range (Ground targeting)
                new Vector3(6, 0, 6), new Vector3(-6, 0, 6), new Vector3(6, 0, -6),
                // Cone testing (front)
                new Vector3(0, 0, 8), new Vector3(2, 0, 10), new Vector3(-2, 0, 10)
            };
            
            for (int i = 0; i < positions.Length; i++)
            {
                GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                enemy.name = $"AOE_TestEnemy_{i + 1}";
                enemy.tag = "Enemy";
                enemy.transform.position = positions[i];
                enemy.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
                
                // Red material
                Renderer renderer = enemy.GetComponent<Renderer>();
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = Color.red;
                renderer.material = mat;
                
                // Add identifier
                enemy.AddComponent<EnemyIdentifier>();
            }
            
            Debug.Log($"[AOE_AutoSceneSetup] Created {positions.Length} test enemies");
        }
        
        static void SetupCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = Object.FindObjectOfType<Camera>();
            }
            
            if (mainCamera != null)
            {
                mainCamera.transform.position = new Vector3(0, 12, -8);
                mainCamera.transform.rotation = Quaternion.Euler(35, 0, 0);
                Debug.Log("[AOE_AutoSceneSetup] Camera positioned for AOE testing");
            }
        }
    }
    
    /// <summary>
    /// Runtime component that sets up AOE testing when the scene loads in play mode.
    /// </summary>
    public class AOE_RuntimeSceneSetup : MonoBehaviour
    {
        void Start()
        {
            // Small delay to ensure everything is loaded
            Invoke(nameof(SetupAOE), 0.5f);
        }
        
        void SetupAOE()
        {
            // Check if AOE is already set up
            if (FindObjectOfType<AOE_MasterTestController>() != null)
            {
                Debug.Log("[AOE_RuntimeSceneSetup] AOE already set up in scene");
                Destroy(gameObject);
                return;
            }
            
            // Set up AOE testing
            AOE_AutoSceneSetup.SetupAOETestingInCurrentScene();
            
            // Clean up this setup object
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Simple movement controller for basic testing.
    /// </summary>
    public class SimplePlayerMovement : MonoBehaviour
    {
        public float moveSpeed = 5f;
        public float rotateSpeed = 100f;
        
        void Update()
        {
            // WASD movement
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            
            Vector3 movement = new Vector3(h, 0, v) * moveSpeed * Time.deltaTime;
            transform.Translate(movement);
            
            // Mouse rotation
            if (Input.GetMouseButton(1))
            {
                float mouseX = Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
                transform.Rotate(0, mouseX, 0);
            }
        }
    }
}