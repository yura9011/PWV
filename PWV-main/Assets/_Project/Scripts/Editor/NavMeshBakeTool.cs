#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.AI;
using System.Collections.Generic;
#if UNITY_AI_NAVIGATION
using Unity.AI.Navigation;
#endif

namespace EtherDomes.Editor
{
    /// <summary>
    /// Editor tool for baking NavMesh in scenes.
    /// Requirements: 19.5
    /// </summary>
    public static class NavMeshBakeTool
    {
        // NavMesh Agent settings
        public const float AGENT_RADIUS = 0.5f;
        public const float AGENT_HEIGHT = 2.0f;
        public const float STEP_HEIGHT = 0.4f;
        public const float MAX_SLOPE = 45f;

        [MenuItem("EtherDomes/NavMesh/Bake Current Scene")]
        public static void BakeCurrentScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogError("[NavMeshBakeTool] No valid scene loaded");
                return;
            }

            BakeScene(scene.path);
        }

        [MenuItem("EtherDomes/NavMesh/Bake All Scenes")]
        public static void BakeAllScenes()
        {
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
            List<string> scenePaths = new List<string>();

            foreach (string guid in sceneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.Contains("Packages/"))
                {
                    scenePaths.Add(path);
                }
            }

            Debug.Log($"[NavMeshBakeTool] Found {scenePaths.Count} scenes to bake");

            int baked = 0;
            foreach (string scenePath in scenePaths)
            {
                if (BakeScene(scenePath))
                {
                    baked++;
                }
            }

            Debug.Log($"[NavMeshBakeTool] Baked NavMesh in {baked}/{scenePaths.Count} scenes");
        }

        [MenuItem("EtherDomes/NavMesh/Configure Agent Settings")]
        public static void ConfigureAgentSettings()
        {
            // Get or create NavMesh settings
            var settings = NavMesh.GetSettingsByID(0);
            
            Debug.Log($"[NavMeshBakeTool] Current NavMesh Settings:");
            Debug.Log($"  Agent Radius: {settings.agentRadius}");
            Debug.Log($"  Agent Height: {settings.agentHeight}");
            Debug.Log($"  Agent Climb: {settings.agentClimb}");
            Debug.Log($"  Agent Slope: {settings.agentSlope}");
            
            Debug.Log($"\n[NavMeshBakeTool] Recommended Settings:");
            Debug.Log($"  Agent Radius: {AGENT_RADIUS}");
            Debug.Log($"  Agent Height: {AGENT_HEIGHT}");
            Debug.Log($"  Step Height: {STEP_HEIGHT}");
            Debug.Log($"  Max Slope: {MAX_SLOPE}");
            
            Debug.Log("\n[NavMeshBakeTool] To change settings, go to Window > AI > Navigation");
        }

        private static bool BakeScene(string scenePath)
        {
            try
            {
                // Save current scene
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

                // Open the scene
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                if (!scene.IsValid())
                {
                    Debug.LogWarning($"[NavMeshBakeTool] Could not open scene: {scenePath}");
                    return false;
                }

#if UNITY_AI_NAVIGATION
                // Find all NavMesh surfaces
                var surfaces = Object.FindObjectsOfType<NavMeshSurface>();
                
                if (surfaces.Length > 0)
                {
                    // Use NavMeshSurface components
                    foreach (var surface in surfaces)
                    {
                        surface.BuildNavMesh();
                    }
                    Debug.Log($"[NavMeshBakeTool] Baked {surfaces.Length} NavMeshSurface(s) in: {scenePath}");
                }
                else
                {
                    Debug.Log($"[NavMeshBakeTool] No NavMeshSurface found in: {scenePath}");
                    Debug.Log("  Add a NavMeshSurface component to a GameObject for automatic baking");
                    Debug.Log("  Or use Window > AI > Navigation for legacy baking");
                }
#else
                // Legacy bake - requires manual setup in Navigation window
                Debug.Log($"[NavMeshBakeTool] Unity.AI.Navigation package not installed for: {scenePath}");
                Debug.Log("  Install via Package Manager: AI Navigation");
                Debug.Log("  Or use Window > AI > Navigation for legacy baking");
#endif

                // Save the scene
                EditorSceneManager.SaveScene(scene);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[NavMeshBakeTool] Error baking scene {scenePath}: {e.Message}");
                return false;
            }
        }

#if UNITY_AI_NAVIGATION
        [MenuItem("EtherDomes/NavMesh/Add NavMeshSurface to Scene")]
        public static void AddNavMeshSurfaceToScene()
        {
            // Check if one already exists
            var existing = Object.FindObjectOfType<NavMeshSurface>();
            if (existing != null)
            {
                Debug.Log("[NavMeshBakeTool] NavMeshSurface already exists in scene");
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            // Create new GameObject with NavMeshSurface
            var go = new GameObject("NavMesh Surface");
            var surface = go.AddComponent<NavMeshSurface>();
            
            // Configure settings
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
            
            Debug.Log("[NavMeshBakeTool] Created NavMeshSurface. Configure and bake using the Inspector.");
            Selection.activeGameObject = go;
        }
#endif

        [MenuItem("EtherDomes/NavMesh/Setup Enemy NavMeshAgent")]
        public static void SetupEnemyNavMeshAgent()
        {
            var selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("[NavMeshBakeTool] Select a GameObject first");
                return;
            }

            var agent = selected.GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                agent = selected.AddComponent<NavMeshAgent>();
            }

            // Configure agent
            agent.radius = AGENT_RADIUS;
            agent.height = AGENT_HEIGHT;
            agent.speed = 3.5f;
            agent.acceleration = 8f;
            agent.angularSpeed = 120f;
            agent.stoppingDistance = 2f;
            agent.autoBraking = true;
            agent.autoRepath = true;

            Debug.Log($"[NavMeshBakeTool] Configured NavMeshAgent on {selected.name}");
            EditorUtility.SetDirty(selected);
        }
    }
}
#endif
