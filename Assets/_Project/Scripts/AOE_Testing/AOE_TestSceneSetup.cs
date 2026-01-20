using UnityEngine;

namespace AOETesting
{
    /// <summary>
    /// Helper script to set up the AOE test scene programmatically.
    /// This script can be used to create and position test objects in the scene.
    /// </summary>
    public class AOE_TestSceneSetup : MonoBehaviour
    {
        [Header("Scene Setup Configuration")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Material enemyMaterial;
        [SerializeField] private Material groundMaterial;
        
        [Header("Enemy Positioning")]
        [SerializeField] private int numberOfEnemies = 10;
        [SerializeField] private float spawnRadius = 15f;
        [SerializeField] private bool useFixedPositions = true;
        
        void Start()
        {
            if (autoSetupOnStart)
            {
                SetupTestScene();
            }
        }
        
        [ContextMenu("Setup Test Scene")]
        public void SetupTestScene()
        {
            Debug.Log("[AOE_TestSceneSetup] Setting up AOE test scene...");
            
            CreateGround();
            CreateEnemies();
            SetupCamera();
            CreateInstructions();
            
            Debug.Log("[AOE_TestSceneSetup] AOE test scene setup complete!");
        }
        
        void CreateGround()
        {
            // Create ground plane
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(5, 1, 5); // 50x50 units
            ground.layer = 0; // Default layer for ground raycast
            
            // Apply material if available
            if (groundMaterial != null)
            {
                ground.GetComponent<Renderer>().material = groundMaterial;
            }
            else
            {
                // Create simple gray material
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = Color.gray;
                ground.GetComponent<Renderer>().material = mat;
            }
            
            Debug.Log("[AOE_TestSceneSetup] Ground plane created");
        }
        
        void CreateEnemies()
        {
            if (useFixedPositions)
            {
                CreateEnemiesAtFixedPositions();
            }
            else
            {
                CreateEnemiesRandomly();
            }
        }
        
        void CreateEnemiesAtFixedPositions()
        {
            // Create enemies at specific positions for testing different AOE scenarios
            Vector3[] positions = new Vector3[]
            {
                // Close circle around origin (for player-centered AOE)
                new Vector3(2, 0, 0),
                new Vector3(-2, 0, 0),
                new Vector3(0, 0, 2),
                new Vector3(0, 0, -2),
                
                // Medium distance (for ground targeting)
                new Vector3(5, 0, 5),
                new Vector3(-5, 0, 5),
                new Vector3(5, 0, -5),
                
                // Cone testing positions (in front of player)
                new Vector3(0, 0, 6),
                new Vector3(2, 0, 8),
                new Vector3(-2, 0, 8),
                
                // Far positions (outside most AOE ranges)
                new Vector3(12, 0, 0),
                new Vector3(0, 0, 12),
                new Vector3(-12, 0, -12)
            };
            
            for (int i = 0; i < positions.Length; i++)
            {
                CreateEnemyAt(positions[i], $"TestEnemy_{i + 1}");
            }
            
            Debug.Log($"[AOE_TestSceneSetup] Created {positions.Length} enemies at fixed positions");
        }
        
        void CreateEnemiesRandomly()
        {
            for (int i = 0; i < numberOfEnemies; i++)
            {
                // Random position within spawn radius
                Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                Vector3 position = new Vector3(randomCircle.x, 0, randomCircle.y);
                
                CreateEnemyAt(position, $"RandomEnemy_{i + 1}");
            }
            
            Debug.Log($"[AOE_TestSceneSetup] Created {numberOfEnemies} enemies at random positions");
        }
        
        void CreateEnemyAt(Vector3 position, string name)
        {
            GameObject enemy;
            
            if (enemyPrefab != null)
            {
                enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
            }
            else
            {
                // Create simple capsule enemy
                enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                enemy.transform.position = position;
                enemy.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
                
                // Apply material
                if (enemyMaterial != null)
                {
                    enemy.GetComponent<Renderer>().material = enemyMaterial;
                }
                else
                {
                    // Create red material for enemies
                    Material mat = new Material(Shader.Find("Standard"));
                    mat.color = Color.red;
                    enemy.GetComponent<Renderer>().material = mat;
                }
            }
            
            enemy.name = name;
            enemy.tag = "Enemy"; // Important: Set Enemy tag for detection
            
            // Add a simple identifier component
            EnemyIdentifier identifier = enemy.AddComponent<EnemyIdentifier>();
            identifier.enemyId = name;
        }
        
        void SetupCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                // Create camera if none exists
                GameObject cameraObject = new GameObject("Main Camera");
                mainCamera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
            }
            
            // Position camera for good view of test area
            mainCamera.transform.position = new Vector3(0, 15, -10);
            mainCamera.transform.rotation = Quaternion.Euler(45, 0, 0);
            
            Debug.Log("[AOE_TestSceneSetup] Camera positioned for testing");
        }
        
        void CreateInstructions()
        {
            // Create a UI Canvas with instructions
            GameObject canvasObject = new GameObject("InstructionsCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Instructions will be shown via OnGUI in the test scripts
            Debug.Log("[AOE_TestSceneSetup] Instructions canvas created");
        }
        
        [ContextMenu("Clear Test Objects")]
        public void ClearTestObjects()
        {
            // Clean up test objects
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemies)
            {
                if (Application.isPlaying)
                    Destroy(enemy);
                else
                    DestroyImmediate(enemy);
            }
            
            GameObject ground = GameObject.Find("Ground");
            if (ground != null)
            {
                if (Application.isPlaying)
                    Destroy(ground);
                else
                    DestroyImmediate(ground);
            }
            
            Debug.Log("[AOE_TestSceneSetup] Test objects cleared");
        }
        
        void OnGUI()
        {
            // Scene setup instructions
            GUILayout.BeginArea(new Rect(Screen.width - 250, 10, 240, 200));
            GUILayout.Label("AOE Test Scene Setup", GUI.skin.box);
            
            if (GUILayout.Button("Setup Scene"))
            {
                SetupTestScene();
            }
            
            if (GUILayout.Button("Clear Objects"))
            {
                ClearTestObjects();
            }
            
            GUILayout.Label($"Enemies in scene: {GameObject.FindGameObjectsWithTag("Enemy").Length}");
            
            GUILayout.EndArea();
        }
    }
    
    /// <summary>
    /// Simple component to identify enemies in the test scene.
    /// </summary>
    public class EnemyIdentifier : MonoBehaviour
    {
        public string enemyId;
        
        void OnDrawGizmos()
        {
            // Draw a small sphere above the enemy for identification
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2, 0.3f);
        }
    }
}