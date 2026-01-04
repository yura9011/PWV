using UnityEngine;
using UnityEditor;
using Mirror;
using EtherDomes.Network;

namespace EtherDomes.Player.Editor
{
    /// <summary>
    /// Editor utility to create the NetworkPlayer prefab.
    /// </summary>
    public static class PlayerPrefabCreator
    {
        [MenuItem("EtherDomes/Create Network Player Prefab")]
        public static void CreateNetworkPlayerPrefab()
        {
            // Create the player GameObject
            GameObject playerGO = new GameObject("NetworkPlayer");

            // Add visual representation (Capsule)
            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "PlayerModel";
            capsule.transform.SetParent(playerGO.transform);
            capsule.transform.localPosition = Vector3.zero;
            
            // Remove the default collider from capsule (we'll add one to parent)
            Object.DestroyImmediate(capsule.GetComponent<CapsuleCollider>());

            // Add a colored material to distinguish players
            var renderer = capsule.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0.2f, 0.6f, 1f); // Blue color
                renderer.material = mat;
            }

            // Add NetworkIdentity (required for Mirror)
            var networkIdentity = playerGO.AddComponent<NetworkIdentity>();

            // Add Rigidbody
            var rigidbody = playerGO.AddComponent<Rigidbody>();
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Add Capsule Collider
            var collider = playerGO.AddComponent<CapsuleCollider>();
            collider.height = 2f;
            collider.radius = 0.5f;
            collider.center = new Vector3(0, 1f, 0);

            // Add NetworkPlayerController
            var controller = playerGO.AddComponent<NetworkPlayerController>();

            // Set layer to "Player" if it exists
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0)
            {
                playerGO.layer = playerLayer;
                capsule.layer = playerLayer;
            }

            // Create prefab directory if it doesn't exist
            string prefabPath = "Assets/_Project/Prefabs/Characters";
            if (!AssetDatabase.IsValidFolder(prefabPath))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Characters");
            }

            // Save as prefab
            string fullPath = $"{prefabPath}/NetworkPlayer.prefab";
            
            // Check if prefab already exists
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
            if (existingPrefab != null)
            {
                if (!EditorUtility.DisplayDialog("Prefab Exists", 
                    "NetworkPlayer prefab already exists. Overwrite?", "Yes", "No"))
                {
                    Object.DestroyImmediate(playerGO);
                    return;
                }
            }

            // Save prefab
            PrefabUtility.SaveAsPrefabAsset(playerGO, fullPath);
            
            // Clean up scene object
            Object.DestroyImmediate(playerGO);

            // Select the created prefab
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
            
            Debug.Log($"[PlayerPrefabCreator] NetworkPlayer prefab created at {fullPath}");
            EditorUtility.DisplayDialog("Success", 
                "NetworkPlayer prefab created!\n\nRemember to assign it to the NetworkManager's Player Prefab field.", 
                "OK");
        }

        [MenuItem("EtherDomes/Setup Network Test Scene")]
        public static void SetupNetworkTestScene()
        {
            // Find or create NetworkManager
            var networkManager = Object.FindFirstObjectByType<NetworkSessionManager>();
            if (networkManager == null)
            {
                Debug.LogError("[PlayerPrefabCreator] NetworkSessionManager not found in scene!");
                return;
            }

            // Load the player prefab
            string prefabPath = "Assets/_Project/Prefabs/Characters/NetworkPlayer.prefab";
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (playerPrefab == null)
            {
                Debug.LogError($"[PlayerPrefabCreator] Player prefab not found at {prefabPath}. Create it first using EtherDomes/Create Network Player Prefab");
                return;
            }

            // Assign to NetworkManager
            networkManager.playerPrefab = playerPrefab;
            EditorUtility.SetDirty(networkManager);

            // Register as spawnable prefab
            if (!networkManager.spawnPrefabs.Contains(playerPrefab))
            {
                networkManager.spawnPrefabs.Add(playerPrefab);
            }

            Debug.Log("[PlayerPrefabCreator] NetworkManager configured with player prefab!");
            EditorUtility.DisplayDialog("Success", 
                "NetworkManager configured!\n\nPlayer prefab assigned and registered.", 
                "OK");
        }
    }
}
