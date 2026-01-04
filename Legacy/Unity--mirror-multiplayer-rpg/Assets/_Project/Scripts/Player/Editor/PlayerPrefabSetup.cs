#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Mirror;

namespace EtherDomes.Player.Editor
{
    /// <summary>
    /// Editor utility to create the test player prefab.
    /// </summary>
    public static class PlayerPrefabSetup
    {
        [MenuItem("EtherDomes/Create Test Player Prefab")]
        public static void CreateTestPlayerPrefab()
        {
            // Create the player GameObject
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "TestPlayer";
            player.layer = LayerMask.NameToLayer("Player");

            // Setup transform
            player.transform.position = Vector3.zero;
            player.transform.localScale = Vector3.one;

            // Add NetworkIdentity
            var networkIdentity = player.AddComponent<NetworkIdentity>();

            // Add Rigidbody
            var rigidbody = player.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = player.AddComponent<Rigidbody>();
            }
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Add CapsuleCollider (already added by CreatePrimitive)
            var collider = player.GetComponent<CapsuleCollider>();
            if (collider != null)
            {
                collider.height = 2f;
                collider.radius = 0.5f;
                collider.center = new Vector3(0, 1f, 0);
            }

            // Add NetworkPlayerController
            player.AddComponent<NetworkPlayerController>();

            // Create material
            var renderer = player.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material material = new Material(Shader.Find("Standard"));
                material.color = new Color(0.2f, 0.6f, 1f); // Blue color
                renderer.material = material;

                // Save material
                string materialPath = "Assets/_Project/Prefabs/Characters/TestPlayerMaterial.mat";
                AssetDatabase.CreateAsset(material, materialPath);
            }

            // Save as prefab
            string prefabPath = "Assets/_Project/Prefabs/Characters/TestPlayer.prefab";
            
            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/Characters"))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Characters");
            }

            // Create prefab
            PrefabUtility.SaveAsPrefabAsset(player, prefabPath);

            // Cleanup scene object
            Object.DestroyImmediate(player);

            Debug.Log($"[PlayerPrefabSetup] Created test player prefab at: {prefabPath}");
            
            // Select the created prefab
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        [MenuItem("EtherDomes/Setup Player Layer")]
        public static void SetupPlayerLayer()
        {
            // This needs to be done manually in Project Settings > Tags and Layers
            // But we can provide instructions
            EditorUtility.DisplayDialog(
                "Setup Player Layer",
                "To setup the Player layer:\n\n" +
                "1. Go to Edit > Project Settings > Tags and Layers\n" +
                "2. Add 'Player' to one of the User Layers (e.g., Layer 8)\n" +
                "3. Go to Edit > Project Settings > Physics\n" +
                "4. In the Layer Collision Matrix, uncheck Player-Player collision",
                "OK"
            );
        }
    }
}
#endif
