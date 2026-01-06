using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using EtherDomes.Testing;

namespace EtherDomes.Editor
{
    /// <summary>
    /// Creates a test dungeon scene with all MVP features ready to test.
    /// Menu: EtherDomes > Create Test Dungeon Scene
    /// </summary>
    public static class DungeonTestSceneSetup
    {
        [MenuItem("EtherDomes/Create Test Dungeon Scene")]
        public static void CreateTestDungeonScene()
        {
            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // Setup lighting
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.4f, 0.4f, 0.5f);
            
            // Position the main camera (will be controlled by player)
            var mainCamera = UnityEngine.Camera.main;
            if (mainCamera != null)
            {
                mainCamera.transform.position = new Vector3(0, 15, -50);
                mainCamera.transform.rotation = Quaternion.Euler(25, 0, 0);
                mainCamera.farClipPlane = 200f;
            }

            // Create floor
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "DungeonFloor";
            floor.transform.localScale = new Vector3(10, 1, 10);
            floor.GetComponent<Renderer>().material.color = new Color(0.25f, 0.25f, 0.3f);

            // Create walls
            CreateWall("Wall_North", new Vector3(0, 2.5f, 50), new Vector3(100, 5, 1));
            CreateWall("Wall_South", new Vector3(0, 2.5f, -50), new Vector3(100, 5, 1));
            CreateWall("Wall_East", new Vector3(50, 2.5f, 0), new Vector3(1, 5, 100));
            CreateWall("Wall_West", new Vector3(-50, 2.5f, 0), new Vector3(1, 5, 100));

            // Create test player
            var player = CreateTestPlayer(new Vector3(0, 1, -40));

            // Create test enemies (Room 1)
            CreateTestEnemy("Skeleton_1", new Vector3(-8, 1, -20), 500f, 20f);
            CreateTestEnemy("Skeleton_2", new Vector3(0, 1, -18), 500f, 20f);
            CreateTestEnemy("Skeleton_3", new Vector3(8, 1, -20), 500f, 20f);

            // Create test enemies (Room 2)
            CreateTestEnemy("Ghoul_1", new Vector3(-8, 1, 0), 750f, 30f);
            CreateTestEnemy("Ghoul_2", new Vector3(0, 1, -2), 750f, 30f);
            CreateTestEnemy("Ghoul_3", new Vector3(8, 1, 0), 750f, 30f);
            CreateTestEnemy("Ghoul_4", new Vector3(0, 1, 5), 750f, 30f);

            // Create boss (Room 3)
            CreateTestBoss("Crypt Lord", new Vector3(0, 1.5f, 30), 5000f, 75f);

            // Create room dividers (visual)
            CreateRoomDivider("Divider_1", new Vector3(0, 0, -10));
            CreateRoomDivider("Divider_2", new Vector3(0, 0, 15));

            // Create test systems holder
            var systemsHolder = new GameObject("--- SYSTEMS ---");
            
            // Create UI Canvas
            var canvas = CreateUICanvas();

            // Save scene
            string scenePath = "Assets/_Project/Scenes/Dungeons/Dungeon_Crypt_Test.unity";
            EnsureDirectoryExists(scenePath);
            EditorSceneManager.SaveScene(scene, scenePath);
            
            Debug.Log($"[DungeonTestSceneSetup] Created test dungeon scene at: {scenePath}");
            Debug.Log("[DungeonTestSceneSetup] Scene ready for testing!");
            Debug.Log("  - Player: Green capsule at south entrance");
            Debug.Log("  - Room 1: 3 Skeletons (500 HP each)");
            Debug.Log("  - Room 2: 4 Ghouls (750 HP each)");
            Debug.Log("  - Room 3: Crypt Lord boss (5000 HP)");
            Debug.Log("  - Controls: WASD=Move, Tab=Target, Space/1=Attack, 2=Heavy Attack");
            
            Selection.activeGameObject = player;
            SceneView.lastActiveSceneView?.FrameSelected();
        }

        private static GameObject CreateTestPlayer(Vector3 position)
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "TestPlayer";
            player.transform.position = position;
            player.GetComponent<Renderer>().material.color = new Color(0.2f, 0.8f, 0.3f);
            
            // Add CharacterController
            var cc = player.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.5f;
            cc.center = Vector3.zero;
            
            // Add TestPlayer component
            player.AddComponent<Testing.TestPlayer>();
            
            return player;
        }

        private static void CreateTestEnemy(string name, Vector3 position, float health, float damage)
        {
            var enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            enemy.name = name;
            enemy.transform.position = position;
            enemy.transform.localScale = new Vector3(1.2f, 2f, 1.2f);
            enemy.GetComponent<Renderer>().material.color = Color.red;
            
            // Add TestEnemy component and configure via SerializedObject
            var comp = enemy.AddComponent<Testing.TestEnemy>();
            
            // Use SerializedObject to set private serialized fields
            var so = new SerializedObject(comp);
            so.FindProperty("_displayName").stringValue = name;
            so.FindProperty("_maxHealth").floatValue = health;
            so.FindProperty("_damage").floatValue = damage;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateTestBoss(string name, Vector3 position, float health, float damage)
        {
            var boss = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boss.name = name;
            boss.transform.position = position;
            boss.transform.localScale = new Vector3(3f, 4f, 3f);
            boss.GetComponent<Renderer>().material.color = new Color(0.6f, 0f, 0.6f); // Purple
            
            // Add TestEnemy component (boss is just a bigger enemy)
            var comp = boss.AddComponent<Testing.TestEnemy>();
            
            var so = new SerializedObject(comp);
            so.FindProperty("_displayName").stringValue = name;
            so.FindProperty("_maxHealth").floatValue = health;
            so.FindProperty("_damage").floatValue = damage;
            so.FindProperty("_level").intValue = 15;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateWall(string name, Vector3 position, Vector3 scale)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.position = position;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().material.color = new Color(0.35f, 0.3f, 0.25f);
            wall.isStatic = true;
        }

        private static void CreateRoomDivider(string name, Vector3 position)
        {
            var divider = GameObject.CreatePrimitive(PrimitiveType.Cube);
            divider.name = name;
            divider.transform.position = position;
            divider.transform.localScale = new Vector3(80, 0.1f, 0.5f);
            divider.GetComponent<Renderer>().material.color = new Color(0.5f, 0.45f, 0.4f);
            
            // Remove collider so players can walk through
            Object.DestroyImmediate(divider.GetComponent<Collider>());
        }

        private static GameObject CreateUICanvas()
        {
            var canvasGO = new GameObject("UICanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Create test info panel
            var infoPanel = new GameObject("TestInfoPanel");
            infoPanel.transform.SetParent(canvasGO.transform, false);
            var rect = infoPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(10, -10);
            rect.sizeDelta = new Vector2(400, 60);

            var text = infoPanel.AddComponent<UnityEngine.UI.Text>();
            text.text = "MVP Test Dungeon - Press PLAY to test!\nWASD=Move | Tab=Target | Space/1=Attack | 2=Heavy Attack";
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

    // Gizmo components for visualization in editor
    public class SpawnPointGizmo : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 1f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2);
        }
    }

    public class EnemySpawnGizmo : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.up, new Vector3(1, 2, 1));
        }
    }

    public class BossSpawnGizmo : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, 3f);
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2, new Vector3(2, 4, 2));
        }
    }
}
