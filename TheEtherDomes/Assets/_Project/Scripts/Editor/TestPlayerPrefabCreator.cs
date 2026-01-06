#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Mirror;

namespace EtherDomes.Editor
{
    /// <summary>
    /// Editor utility to create the test player prefab.
    /// </summary>
    public static class TestPlayerPrefabCreator
    {
        [MenuItem("EtherDomes/Create Test Player Prefab")]
        public static void CreateTestPlayerPrefab()
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "TestPlayer";
            
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0) player.layer = playerLayer;

            player.transform.position = Vector3.zero;
            player.transform.localScale = Vector3.one;

            player.AddComponent<NetworkIdentity>();
            player.AddComponent<NetworkTransformReliable>();

            var rigidbody = player.AddComponent<Rigidbody>();
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

            var collider = player.GetComponent<CapsuleCollider>();
            if (collider != null)
            {
                collider.height = 2f;
                collider.radius = 0.5f;
                collider.center = new Vector3(0, 1f, 0);
            }

            var renderer = player.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material material = new Material(Shader.Find("Standard"));
                material.color = new Color(0.2f, 0.6f, 1f);
                renderer.material = material;

                if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/Characters"))
                {
                    if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs"))
                        AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
                    AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Characters");
                }
                
                AssetDatabase.CreateAsset(material, "Assets/_Project/Prefabs/Characters/TestPlayerMaterial.mat");
            }

            string prefabPath = "Assets/_Project/Prefabs/Characters/TestPlayer.prefab";
            PrefabUtility.SaveAsPrefabAsset(player, prefabPath);
            Object.DestroyImmediate(player);

            UnityEngine.Debug.Log($"[TestPlayerPrefabCreator] Created test player prefab at: {prefabPath}");
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            EditorUtility.DisplayDialog("Success", 
                $"Test Player prefab created at:\n{prefabPath}\n\n" +
                "Components:\n- NetworkIdentity\n- NetworkTransformReliable\n- Rigidbody\n- CapsuleCollider",
                "OK");
        }
    }
}
#endif
