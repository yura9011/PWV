using UnityEngine;
using UnityEditor;
using Mirror;
using EtherDomes.Network;
using EtherDomes.Movement;
using EtherDomes.Camera;
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

            playerGO.AddComponent<NetworkIdentity>();

            var charController = playerGO.AddComponent<CharacterController>();
            charController.height = 2f;
            charController.radius = 0.5f;
            charController.center = new Vector3(0, 1f, 0);
            charController.slopeLimit = 45f;
            charController.stepOffset = 0.3f;

            playerGO.AddComponent<NetworkTransformReliable>();
            playerGO.AddComponent<WoWMovementController>();
            playerGO.AddComponent<PlayerVisualController>();
            playerGO.AddComponent<CinemachinePlayerFollow>();

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

            PrefabUtility.SaveAsPrefabAsset(playerGO, fullPath);
            Object.DestroyImmediate(playerGO);

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
            
            Debug.Log($"[PlayerPrefabCreator] NetworkPlayer prefab created at {fullPath}");
            EditorUtility.DisplayDialog("Success", 
                "NetworkPlayer prefab created!\n\nComponents:\n- CharacterController\n- WoWMovementController\n- PlayerVisualController\n- NetworkTransform\n- CinemachinePlayerFollow", 
                "OK");
        }

        [MenuItem("EtherDomes/Setup Network Test Scene")]
        public static void SetupNetworkTestScene()
        {
            var networkManager = Object.FindFirstObjectByType<MirrorNetworkSessionManager>();
            if (networkManager == null)
            {
                Debug.LogError("[PlayerPrefabCreator] MirrorNetworkSessionManager not found!");
                EditorUtility.DisplayDialog("Error", 
                    "MirrorNetworkSessionManager not found!\n\nAdd MirrorNetworkSessionManager to scene first.", 
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

            networkManager.playerPrefab = playerPrefab;
            
            if (!networkManager.spawnPrefabs.Contains(playerPrefab))
            {
                networkManager.spawnPrefabs.Add(playerPrefab);
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
