using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;

namespace EtherDomes.Editor
{
    /// <summary>
    /// Editor utility for creating pet prefabs.
    /// </summary>
    public static class PetPrefabCreator
    {
        private const string PETS_FOLDER = "Assets/_Project/Prefabs/Pets";
        private const string BASE_PET_PREFAB_NAME = "BasePet.prefab";

        /// <summary>
        /// Create the base pet prefab with all required components.
        /// </summary>
        [MenuItem("EtherDomes/Create Base Pet Prefab", false, 100)]
        public static void CreateBasePetPrefab()
        {
            // Ensure Pets folder exists
            if (!AssetDatabase.IsValidFolder(PETS_FOLDER))
            {
                string parentFolder = "Assets/_Project/Prefabs";
                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
                }
                AssetDatabase.CreateFolder(parentFolder, "Pets");
            }

            // Create root object
            GameObject petRoot = new GameObject("BasePet");

            // Add capsule mesh as placeholder
            GameObject meshObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            meshObject.name = "Mesh";
            meshObject.transform.SetParent(petRoot.transform);
            meshObject.transform.localPosition = new Vector3(0, 0.5f, 0);
            meshObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            // Remove collider from mesh (we'll add one to root)
            var meshCollider = meshObject.GetComponent<Collider>();
            if (meshCollider != null)
            {
                Object.DestroyImmediate(meshCollider);
            }

            // Set mesh material to a distinct color
            var renderer = meshObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material petMaterial = new Material(Shader.Find("Standard"));
                petMaterial.color = new Color(0.4f, 0.6f, 0.8f); // Light blue
                renderer.sharedMaterial = petMaterial;
                
                // Save material
                string materialPath = $"{PETS_FOLDER}/BasePetMaterial.mat";
                AssetDatabase.CreateAsset(petMaterial, materialPath);
            }

            // Add NavMeshAgent
            var navAgent = petRoot.AddComponent<NavMeshAgent>();
            navAgent.radius = 0.3f;
            navAgent.height = 1f;
            navAgent.speed = 5f;
            navAgent.acceleration = 8f;
            navAgent.angularSpeed = 120f;
            navAgent.stoppingDistance = 2.5f;
            navAgent.autoBraking = true;
            navAgent.autoRepath = true;

            // Add capsule collider
            var capsuleCollider = petRoot.AddComponent<CapsuleCollider>();
            capsuleCollider.radius = 0.3f;
            capsuleCollider.height = 1f;
            capsuleCollider.center = new Vector3(0, 0.5f, 0);

            // Add PetBehaviour component
            petRoot.AddComponent<Combat.PetBehaviour>();

            // Save as prefab
            string prefabPath = $"{PETS_FOLDER}/{BASE_PET_PREFAB_NAME}";
            
            // Check if prefab already exists
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                if (!EditorUtility.DisplayDialog("Overwrite Prefab?", 
                    $"A prefab already exists at {prefabPath}. Do you want to overwrite it?", 
                    "Yes", "No"))
                {
                    Object.DestroyImmediate(petRoot);
                    return;
                }
            }

            // Create prefab
            PrefabUtility.SaveAsPrefabAsset(petRoot, prefabPath);
            
            // Clean up scene object
            Object.DestroyImmediate(petRoot);

            // Refresh and select
            AssetDatabase.Refresh();
            var createdPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Selection.activeObject = createdPrefab;
            EditorGUIUtility.PingObject(createdPrefab);

            Debug.Log($"[PetPrefabCreator] Created base pet prefab at {prefabPath}");
        }

        /// <summary>
        /// Check if the base pet prefab exists.
        /// </summary>
        [MenuItem("EtherDomes/Create Base Pet Prefab", true)]
        public static bool ValidateCreateBasePetPrefab()
        {
            return true;
        }
    }
}
#endif
