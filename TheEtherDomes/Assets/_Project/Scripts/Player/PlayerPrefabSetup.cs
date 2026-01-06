using UnityEngine;
using Mirror;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EtherDomes.Player
{
    /// <summary>
    /// Utility script to help set up the Player Prefab in the editor.
    /// Uses Mirror networking.
    /// </summary>
    public static class PlayerPrefabSetup
    {
        public const string PLAYER_PREFAB_PATH = "Assets/_Project/Prefabs/Player/Player.prefab";
        
        public const float DEFAULT_HEIGHT = 2f;
        public const float DEFAULT_RADIUS = 0.5f;
        public const float DEFAULT_STEP_OFFSET = 0.3f;

#if UNITY_EDITOR
        [MenuItem("EtherDomes/Create Player Prefab (Legacy)")]
        public static void CreatePlayerPrefab()
        {
            GameObject playerObj = new GameObject("Player");

            playerObj.AddComponent<NetworkIdentity>();
            playerObj.AddComponent<NetworkTransformReliable>();
            var characterController = playerObj.AddComponent<CharacterController>();
            playerObj.AddComponent<PlayerController>();
            playerObj.AddComponent<NetworkPlayer>();

            characterController.height = DEFAULT_HEIGHT;
            characterController.radius = DEFAULT_RADIUS;
            characterController.stepOffset = DEFAULT_STEP_OFFSET;
            characterController.center = new Vector3(0, DEFAULT_HEIGHT / 2f, 0);

            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer != -1)
            {
                playerObj.layer = playerLayer;
            }

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            visual.transform.SetParent(playerObj.transform);
            visual.transform.localPosition = new Vector3(0, DEFAULT_HEIGHT / 2f, 0);
            visual.transform.localScale = new Vector3(DEFAULT_RADIUS * 2f, DEFAULT_HEIGHT / 2f, DEFAULT_RADIUS * 2f);
            
            Object.DestroyImmediate(visual.GetComponent<Collider>());

            string directory = System.IO.Path.GetDirectoryName(PLAYER_PREFAB_PATH);
            if (!AssetDatabase.IsValidFolder(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }

            PrefabUtility.SaveAsPrefabAsset(playerObj, PLAYER_PREFAB_PATH);
            Object.DestroyImmediate(playerObj);

            Debug.Log($"[PlayerPrefabSetup] Created Player Prefab at {PLAYER_PREFAB_PATH}");
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PLAYER_PREFAB_PATH);
        }

        [MenuItem("EtherDomes/Validate Player Prefab (Legacy)")]
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

            if (prefab.GetComponent<NetworkIdentity>() == null)
            {
                report.AppendLine("  ❌ Missing NetworkIdentity");
                isValid = false;
            }
            else
            {
                report.AppendLine("  ✓ NetworkIdentity");
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

            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer != -1 && prefab.layer == playerLayer)
            {
                report.AppendLine("  ✓ Player Layer");
            }
            else
            {
                report.AppendLine("  ⚠ Not on Player layer");
            }

            report.AppendLine($"\nResult: {(isValid ? "VALID" : "INVALID")}");

            if (isValid)
                Debug.Log(report.ToString());
            else
                Debug.LogWarning(report.ToString());
        }
#endif
    }
}
