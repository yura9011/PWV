using UnityEngine;
using UnityEditor;
using EtherDomes.Network;
using EtherDomes.Player;

namespace EtherDomes.Editor
{
    /// <summary>
    /// Editor utility to create the NetworkPlayer prefab with WoW-style movement.
    /// </summary>
    public static class PlayerPrefabCreator
    {
        [MenuItem("EtherDomes/Create Network Player Prefab")]
        public static void CreateNetworkPlayerPrefab()
        {
            GameObject playerGO = new GameObject("NetworkPlayer");

            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "PlayerModel";
            capsule.transform.SetParent(playerGO.transform);
            capsule.transform.localPosition = new Vector3(0, 1f, 0);
            
            Object.DestroyImmediate(capsule.GetComponent<CapsuleCollider>());

            var renderer = capsule.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                if (shader == null) shader = Shader.Find("Sprites/Default");
                
                if (shader != null)
                {
                    Material mat = new Material(shader);
                    mat.color = Color.gray;
                    renderer.material = mat;
                }
            }

            playerGO.AddComponent<Unity.Netcode.NetworkObject>();

            var charController = playerGO.AddComponent<CharacterController>();
            charController.height = 2f;
            charController.radius = 0.5f;
            charController.center = new Vector3(0, 1f, 0);
            charController.slopeLimit = 45f;
            charController.stepOffset = 0.3f;

            playerGO.AddComponent<Unity.Netcode.Components.NetworkTransform>();
            
            // Use PlayerController (new WoW-style movement with camera control)
            playerGO.AddComponent<PlayerController>();

            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0)
            {
                playerGO.layer = playerLayer;
                capsule.layer = playerLayer;
            }

            string prefabPath = "Assets/_Project/Prefabs/Characters";
            if (!AssetDatabase.IsValidFolder(prefabPath))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs"))
                    AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Characters");
            }

            string fullPath = $"{prefabPath}/NetworkPlayer.prefab";
            
            // Always overwrite
            PrefabUtility.SaveAsPrefabAsset(playerGO, fullPath);
            Object.DestroyImmediate(playerGO);

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
            
            Debug.Log($"[PlayerPrefabCreator] NetworkPlayer prefab created at {fullPath}");
        }

        [MenuItem("EtherDomes/Setup Network Test Scene")]
        public static void SetupNetworkTestScene()
        {
            var networkManager = Object.FindFirstObjectByType<Unity.Netcode.NetworkManager>();
            if (networkManager == null)
            {
                Debug.LogError("[PlayerPrefabCreator] Unity.Netcode.NetworkManager not found!");
                EditorUtility.DisplayDialog("Error", 
                    "NetworkManager not found!\n\nUse 'EtherDomes > Setup Network and UI' first.", 
                    "OK");
                return;
            }

            string prefabPath = "Assets/_Project/Prefabs/Characters/NetworkPlayer.prefab";
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (playerPrefab == null)
            {
                Debug.LogError($"[PlayerPrefabCreator] Player prefab not found. Create it first.");
                EditorUtility.DisplayDialog("Error", 
                    "Player prefab not found!\n\nRun: EtherDomes > Create Network Player Prefab", 
                    "OK");
                return;
            }

            if (networkManager.NetworkConfig == null) networkManager.NetworkConfig = new Unity.Netcode.NetworkConfig();
            networkManager.NetworkConfig.PlayerPrefab = playerPrefab;

            // Add to NetworkPrefabs list logic
            bool exists = false;
            // Check if Prefabs is null (it's a definition property usually initialized)
            if (networkManager.NetworkConfig.Prefabs == null) 
                 networkManager.NetworkConfig.Prefabs = new Unity.Netcode.NetworkPrefabs();

            foreach (var item in networkManager.NetworkConfig.Prefabs.Prefabs)
            {
                if (item.Prefab == playerPrefab) 
                {
                    exists = true; 
                    break; 
                }
            }
            
            if (!exists)
            {
                networkManager.NetworkConfig.Prefabs.Add(new Unity.Netcode.NetworkPrefab { Prefab = playerPrefab });
            }

            EditorUtility.SetDirty(networkManager);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

            Debug.Log($"[PlayerPrefabCreator] NetworkManager configured!");
            EditorUtility.DisplayDialog("Success", 
                $"NetworkManager configured!\n\nPlayer Prefab: {playerPrefab.name}\nScene saved.", 
                "OK");
        }
    }
}
