using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace AOETesting
{
    /// <summary>
    /// Editor script that automatically sets up AOE testing when Unity compiles.
    /// This will create the AOE test scene and configure it properly.
    /// </summary>
    // [InitializeOnLoad] // DISABLED - Causing Unity crash
    public class AOE_EditorSetup
    {
        static AOE_EditorSetup()
        {
            // DISABLED - Causing Unity crash
            // EditorApplication.delayCall += SetupAOETestingEnvironment;
        }
        
        static void SetupAOETestingEnvironment()
        {
            // Only run once
            EditorApplication.delayCall -= SetupAOETestingEnvironment;
            
            Debug.Log("[AOE_EditorSetup] Setting up AOE testing environment...");
            
            // Create AOE test scene if it doesn't exist
            CreateAOETestSceneIfNeeded();
            
            // Add menu items
            Debug.Log("[AOE_EditorSetup] AOE testing environment ready!");
            Debug.Log("Use menu: AOE Testing > Setup Current Scene or Create AOE Test Scene");
        }
        
        static void CreateAOETestSceneIfNeeded()
        {
            string scenePath = "Assets/_Project/Scenes/AOE_Testing/AOE_TestScene.unity";
            
            // Check if scene already exists
            if (System.IO.File.Exists(scenePath))
            {
                Debug.Log("[AOE_EditorSetup] AOE test scene already exists");
                return;
            }
            
            // Create directory if needed
            string directory = System.IO.Path.GetDirectoryName(scenePath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            
            // Save current scene
            Scene currentScene = SceneManager.GetActiveScene();
            bool wasCurrentSceneDirty = currentScene.isDirty;
            
            // Create new scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // Setup the scene
            SetupAOETestScene();
            
            // Save the new scene
            EditorSceneManager.SaveScene(newScene, scenePath);
            
            Debug.Log($"[AOE_EditorSetup] Created AOE test scene at {scenePath}");
            
            // Restore previous scene if it was open
            if (!string.IsNullOrEmpty(currentScene.path))
            {
                EditorSceneManager.OpenScene(currentScene.path);
            }
        }
        
        static void SetupAOETestScene()
        {
            // Create ground
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(5, 1, 5);
            
            // Gray material for ground
            Material groundMat = new Material(Shader.Find("Standard"));
            groundMat.color = Color.gray;
            ground.GetComponent<Renderer>().material = groundMat;
            
            // Create test player
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "AOE_TestPlayer";
            player.tag = "Player";
            player.transform.position = Vector3.zero;
            
            // Add AOE components
            player.AddComponent<AOE_MasterTestController>();
            player.AddComponent<SimplePlayerMovement>();
            
            // Create test enemies at strategic positions
            Vector3[] enemyPositions = {
                // Close range
                new Vector3(3, 0, 0), new Vector3(-3, 0, 0), new Vector3(0, 0, 3), new Vector3(0, 0, -3),
                // Medium range
                new Vector3(6, 0, 6), new Vector3(-6, 0, 6), new Vector3(6, 0, -6),
                // Cone testing
                new Vector3(0, 0, 8), new Vector3(2, 0, 10), new Vector3(-2, 0, 10)
            };
            
            Material enemyMat = new Material(Shader.Find("Standard"));
            enemyMat.color = Color.red;
            
            for (int i = 0; i < enemyPositions.Length; i++)
            {
                GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                enemy.name = $"TestEnemy_{i + 1}";
                enemy.tag = "Enemy";
                enemy.transform.position = enemyPositions[i];
                enemy.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
                enemy.GetComponent<Renderer>().material = enemyMat;
                enemy.AddComponent<EnemyIdentifier>();
            }
            
            // Setup camera
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.transform.position = new Vector3(0, 12, -8);
                mainCamera.transform.rotation = Quaternion.Euler(35, 0, 0);
            }
            
            // Create instructions object
            GameObject instructions = new GameObject("AOE_Instructions");
            instructions.AddComponent<AOE_InstructionsDisplay>();
            
            Debug.Log("[AOE_EditorSetup] AOE test scene setup complete");
        }
        
        [MenuItem("AOE Testing/Create AOE Test Scene", false, 1)]
        static void CreateAOETestSceneMenu()
        {
            CreateAOETestSceneIfNeeded();
            
            // Open the scene
            string scenePath = "Assets/_Project/Scenes/AOE_Testing/AOE_TestScene.unity";
            if (System.IO.File.Exists(scenePath))
            {
                EditorSceneManager.OpenScene(scenePath);
                Debug.Log("[AOE_EditorSetup] Opened AOE test scene");
            }
        }
        
        [MenuItem("AOE Testing/Setup Current Scene for AOE Testing", false, 2)]
        static void SetupCurrentSceneMenu()
        {
            AOE_AutoSceneSetup.SetupAOETestingInCurrentScene();
        }
        
        [MenuItem("AOE Testing/Open AOE Test Scene", false, 3)]
        static void OpenAOETestSceneMenu()
        {
            string scenePath = "Assets/_Project/Scenes/AOE_Testing/AOE_TestScene.unity";
            if (System.IO.File.Exists(scenePath))
            {
                EditorSceneManager.OpenScene(scenePath);
            }
            else
            {
                Debug.LogWarning("AOE test scene not found. Use 'Create AOE Test Scene' first.");
            }
        }
    }
    
    /// <summary>
    /// Displays instructions in the scene.
    /// </summary>
    public class AOE_InstructionsDisplay : MonoBehaviour
    {
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("AOE Test Scene", GUI.skin.box);
            GUILayout.Label("Controls:");
            GUILayout.Label("G - Ground Targeting AOE");
            GUILayout.Label("R - Player-Centered AOE");
            GUILayout.Label("T - Cone Attack AOE");
            GUILayout.Label("WASD - Move");
            GUILayout.Label("Right-click + Mouse - Rotate");
            GUILayout.Label("");
            GUILayout.Label("Watch Console for detection results!");
            GUILayout.EndArea();
        }
    }
}