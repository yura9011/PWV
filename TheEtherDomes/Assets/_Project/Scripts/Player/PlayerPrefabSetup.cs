using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EtherDomes.Player
{
    /// <summary>
    /// Utility script to help set up the Player Prefab in the editor.
    /// Provides menu items and validation for prefab configuration.
    /// </summary>
    public static class PlayerPrefabSetup
    {
        public const string PLAYER_PREFAB_PATH = "Assets/_Project/Prefabs/Player/Player.prefab";
        
        // Default player dimensions
        public const float DEFAULT_HEIGHT = 2f;
        public const float DEFAULT_RADIUS = 0.5f;
        public const float DEFAULT_STEP_OFFSET = 0.3f;

#if UNITY_EDITOR
        /// <summary>
        /// Create a new Player Prefab with all required components.
        /// </summary>
        [MenuItem("EtherDomes/Create Player Prefab")]
        public static void CreatePlayerPrefab()
        {
            // Create the player GameObject
            GameObject playerObj = new GameObject("Player");

            // Add required components
            var networkObject = playerObj.AddComponent<Unity.Netcode.NetworkObject>();
            var characterController = playerObj.AddComponent<CharacterController>();
            var playerController = playerObj.AddComponent<PlayerController>();
            var networkPlayer = playerObj.AddComponent<NetworkPlayer>();

            // Configure CharacterController
            characterController.height = DEFAULT_HEIGHT;
            characterController.radius = DEFAULT_RADIUS;
            characterController.stepOffset = DEFAULT_STEP_OFFSET;
            characterController.center = new Vector3(0, DEFAULT_HEIGHT / 2f, 0);

            // Set layer
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer != -1)
            {
                playerObj.layer = playerLayer;
            }

            // Create visual placeholder (capsule)
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            visual.transform.SetParent(playerObj.transform);
            visual.transform.localPosition = new Vector3(0, DEFAULT_HEIGHT / 2f, 0);
            visual.transform.localScale = new Vector3(DEFAULT_RADIUS * 2f, DEFAULT_HEIGHT / 2f, DEFAULT_RADIUS * 2f);
            
            // Remove collider from visual (CharacterController handles collision)
            Object.DestroyImmediate(visual.GetComponent<Collider>());

            // Ensure prefab directory exists
            string directory = System.IO.Path.GetDirectoryName(PLAYER_PREFAB_PATH);
            if (!AssetDatabase.IsValidFolder(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }

            // Save as prefab
            PrefabUtility.SaveAsPrefabAsset(playerObj, PLAYER_PREFAB_PATH);
            
            // Clean up scene object
            Object.DestroyImmediate(playerObj);

            Debug.Log($"[PlayerPrefabSetup] Created Player Prefab at {PLAYER_PREFAB_PATH}");
            Debug.Log("[PlayerPrefabSetup] Note: Add NetworkTransform component manually if needed for position sync");
            
            // Select the created prefab
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PLAYER_PREFAB_PATH);
        }

        /// <summary>
        /// Validate an existing Player Prefab.
        /// </summary>
        [MenuItem("EtherDomes/Validate Player Prefab")]
        public static void ValidatePlayerPrefab()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PLAYER_PREFAB_PATH);
            
            if (prefab == null)
            {
                Debug.LogError($"[PlayerPrefabSetup] Player Prefab not found at {PLAYER_PREFAB_PATH}");
                return;
            }

            bool isValid = true;
            System.Text.StringBuilder report = new System.Text.StringBuilder();
            report.AppendLine("Player Prefab Validation Report:");

            // Check required components
            if (prefab.GetComponent<Unity.Netcode.NetworkObject>() == null)
            {
                report.AppendLine("  ❌ Missing NetworkObject");
                isValid = false;
            }
            else
            {
                report.AppendLine("  ✓ NetworkObject");
            }

            if (prefab.GetComponent<CharacterController>() == null)
            {
                report.AppendLine("  ❌ Missing CharacterController");
                isValid = false;
            }
            else
            {
                report.AppendLine("  ✓ CharacterController");
            }

            if (prefab.GetComponent<PlayerController>() == null)
            {
                report.AppendLine("  ❌ Missing PlayerController");
                isValid = false;
            }
            else
            {
                report.AppendLine("  ✓ PlayerController");
            }

            if (prefab.GetComponent<NetworkPlayer>() == null)
            {
                report.AppendLine("  ❌ Missing NetworkPlayer");
                isValid = false;
            }
            else
            {
                report.AppendLine("  ✓ NetworkPlayer");
            }

            // Check layer
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer != -1 && prefab.layer == playerLayer)
            {
                report.AppendLine("  ✓ Player Layer");
            }
            else
            {
                report.AppendLine("  ⚠ Not on Player layer (player-to-player collision may occur)");
            }

            report.AppendLine($"\nResult: {(isValid ? "VALID" : "INVALID")}");

            if (isValid)
            {
                Debug.Log(report.ToString());
            }
            else
            {
                Debug.LogWarning(report.ToString());
            }
        }
#endif
    }
}
