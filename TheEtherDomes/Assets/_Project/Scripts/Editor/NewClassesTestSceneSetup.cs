using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using EtherDomes.Combat;
using EtherDomes.UI.Debug;

namespace EtherDomes.Editor
{
    /// <summary>
    /// Creates a test scene specifically for testing new classes and combat systems.
    /// Menu: EtherDomes > Create New Classes Test Scene
    /// </summary>
    public static class NewClassesTestSceneSetup
    {
        [MenuItem("EtherDomes/Create New Classes Test Scene")]
        public static void CreateTestScene()
        {
            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // Setup lighting
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.55f);
            
            // Position camera
            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.transform.position = new Vector3(0, 20, -25);
                mainCamera.transform.rotation = Quaternion.Euler(45, 0, 0);
                mainCamera.farClipPlane = 200f;
            }

            // Create arena floor
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "ArenaFloor";
            floor.transform.localScale = new Vector3(5, 1, 5);
            floor.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.35f);

            // Create walls
            CreateWall("Wall_North", new Vector3(0, 2.5f, 25), new Vector3(50, 5, 1));
            CreateWall("Wall_South", new Vector3(0, 2.5f, -25), new Vector3(50, 5, 1));
            CreateWall("Wall_East", new Vector3(25, 2.5f, 0), new Vector3(1, 5, 50));
            CreateWall("Wall_West", new Vector3(-25, 2.5f, 0), new Vector3(1, 5, 50));

            // Create systems holder
            var systemsHolder = new GameObject("--- COMBAT SYSTEMS ---");
            
            // Add all combat systems
            var targetSystem = systemsHolder.AddComponent<TargetSystem>();
            var abilitySystem = systemsHolder.AddComponent<AbilitySystem>();
            var resourceSystem = systemsHolder.AddComponent<SecondaryResourceSystem>();
            var buffSystem = systemsHolder.AddComponent<BuffSystem>();
            var stealthSystem = systemsHolder.AddComponent<StealthSystem>();
            var drSystem = systemsHolder.AddComponent<DiminishingReturnsSystem>();
            
            // Create test player
            var player = CreateTestPlayer(new Vector3(0, 1, -15));

            // Create training dummies (enemies that don't fight back)
            CreateTrainingDummy("Dummy_Melee", new Vector3(-5, 1, 5), 10000f);
            CreateTrainingDummy("Dummy_Ranged", new Vector3(5, 1, 5), 10000f);
            CreateTrainingDummy("Dummy_Boss", new Vector3(0, 1.5f, 15), 50000f, true);

            // Create hostile enemies for real combat testing
            CreateHostileEnemy("Skeleton", new Vector3(-15, 1, 0), 1000f, 30f);
            CreateHostileEnemy("Ghoul", new Vector3(15, 1, 0), 1500f, 50f);

            // Create UI Canvas with test UI
            var canvas = CreateUICanvas();
            
            // Add NewClassesTestUI
            var testUI = canvas.AddComponent<NewClassesTestUI>();
            
            // Wire up systems via SerializedObject
            var so = new SerializedObject(testUI);
            so.FindProperty("_abilitySystem").objectReferenceValue = abilitySystem;
            so.FindProperty("_targetSystem").objectReferenceValue = targetSystem;
            so.FindProperty("_resourceSystem").objectReferenceValue = resourceSystem;
            so.FindProperty("_buffSystem").objectReferenceValue = buffSystem;
            so.FindProperty("_stealthSystem").objectReferenceValue = stealthSystem;
            so.FindProperty("_drSystem").objectReferenceValue = drSystem;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Save scene
            string scenePath = "Assets/_Project/Scenes/Test/NewClasses_Test.unity";
            EnsureDirectoryExists(scenePath);
            EditorSceneManager.SaveScene(scene, scenePath);
            
            Debug.Log($"[NewClassesTestSceneSetup] Created test scene at: {scenePath}");
            Debug.Log("=== NEW CLASSES TEST SCENE READY ===");
            Debug.Log("Controls:");
            Debug.Log("  F1-F4: Switch class (Rogue/Hunter/Warlock/DK)");
            Debug.Log("  F5: Toggle specialization");
            Debug.Log("  1-9: Use abilities");
            Debug.Log("  Tab: Cycle targets");
            Debug.Log("  WASD: Move");
            Debug.Log("  Click: Target enemy");
            Debug.Log("");
            Debug.Log("Targets:");
            Debug.Log("  - Training Dummies (don't fight back, high HP)");
            Debug.Log("  - Skeleton & Ghoul (hostile, will attack)");
            
            Selection.activeGameObject = player;
            SceneView.lastActiveSceneView?.FrameSelected();
        }

        private static GameObject CreateTestPlayer(Vector3 position)
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "TestPlayer";
            player.transform.position = position;
            player.GetComponent<Renderer>().material.color = new Color(0.2f, 0.8f, 0.3f);
            
            var cc = player.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.5f;
            cc.center = Vector3.zero;
            
            player.AddComponent<Testing.TestPlayer>();
            
            return player;
        }

        private static void CreateTrainingDummy(string name, Vector3 position, float health, bool isBoss = false)
        {
            var dummy = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            dummy.name = name;
            dummy.transform.position = position;
            
            if (isBoss)
            {
                dummy.transform.localScale = new Vector3(2f, 3f, 2f);
                dummy.GetComponent<Renderer>().material.color = new Color(0.6f, 0.2f, 0.6f);
            }
            else
            {
                dummy.transform.localScale = new Vector3(1f, 2f, 1f);
                dummy.GetComponent<Renderer>().material.color = new Color(0.5f, 0.4f, 0.3f);
            }
            
            // Add TestEnemy but with no aggro (training dummy)
            var comp = dummy.AddComponent<Testing.TestEnemy>();
            var so = new SerializedObject(comp);
            so.FindProperty("_displayName").stringValue = name;
            so.FindProperty("_maxHealth").floatValue = health;
            so.FindProperty("_damage").floatValue = 0f; // No damage
            so.FindProperty("_aggroRange").floatValue = 0f; // No aggro
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateHostileEnemy(string name, Vector3 position, float health, float damage)
        {
            var enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            enemy.name = name;
            enemy.transform.position = position;
            enemy.transform.localScale = new Vector3(1.2f, 2f, 1.2f);
            enemy.GetComponent<Renderer>().material.color = Color.red;
            
            var comp = enemy.AddComponent<Testing.TestEnemy>();
            var so = new SerializedObject(comp);
            so.FindProperty("_displayName").stringValue = name;
            so.FindProperty("_maxHealth").floatValue = health;
            so.FindProperty("_damage").floatValue = damage;
            so.FindProperty("_aggroRange").floatValue = 15f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateWall(string name, Vector3 position, Vector3 scale)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.position = position;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().material.color = new Color(0.4f, 0.35f, 0.3f);
            wall.isStatic = true;
        }

        private static GameObject CreateUICanvas()
        {
            var canvasGO = new GameObject("UICanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Info panel
            var infoPanel = new GameObject("InfoPanel");
            infoPanel.transform.SetParent(canvasGO.transform, false);
            var rect = infoPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(10, -10);
            rect.sizeDelta = new Vector2(350, 80);

            var text = infoPanel.AddComponent<UnityEngine.UI.Text>();
            text.text = "NEW CLASSES TEST SCENE\n" +
                        "F1-F4: Switch Class | F5: Toggle Spec\n" +
                        "1-9: Abilities | Tab: Target | WASD: Move";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 14;
            text.color = Color.white;

            return canvasGO;
        }

        private static void EnsureDirectoryExists(string path)
        {
            var directory = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }
        }
    }
}
