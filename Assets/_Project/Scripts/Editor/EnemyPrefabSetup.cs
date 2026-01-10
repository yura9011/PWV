using UnityEngine;
using UnityEditor;

namespace EtherDomes.Editor
{
    public static class EnemyPrefabSetup
    {
        [MenuItem("EtherDomes/Setup Enemy Target Indicator")]
        public static void SetupTargetIndicator()
        {
            string prefabPath = "Assets/_Project/Prefabs/Enemies/BasicEnemy.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (prefab == null)
            {
                Debug.LogError("[EnemyPrefabSetup] BasicEnemy prefab not found!");
                return;
            }

            // Open prefab for editing
            string assetPath = AssetDatabase.GetAssetPath(prefab);
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);

            // Check if indicator already exists
            Transform existingIndicator = prefabRoot.transform.Find("TargetIndicator");
            if (existingIndicator != null)
            {
                Debug.Log("[EnemyPrefabSetup] Target indicator already exists!");
                PrefabUtility.UnloadPrefabContents(prefabRoot);
                return;
            }

            // Create indicator quad
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Quad);
            indicator.name = "TargetIndicator";
            indicator.transform.SetParent(prefabRoot.transform);
            indicator.transform.localPosition = new Vector3(0, 0.05f, 0);
            indicator.transform.localRotation = Quaternion.Euler(90, 0, 0);
            indicator.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

            // Remove collider from indicator
            Object.DestroyImmediate(indicator.GetComponent<MeshCollider>());

            // Load and assign material
            string matPath = "Assets/_Project/Materials/TargetIndicator.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat != null)
            {
                indicator.GetComponent<MeshRenderer>().sharedMaterial = mat;
            }

            // Start disabled
            indicator.SetActive(false);

            // Update Enemy component reference
            var enemy = prefabRoot.GetComponent<EtherDomes.Enemy.Enemy>();
            if (enemy != null)
            {
                var field = typeof(EtherDomes.Enemy.Enemy).GetField("_targetIndicator", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(enemy, indicator);
                }
            }

            // Save prefab
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);

            Debug.Log("[EnemyPrefabSetup] Target indicator added to BasicEnemy prefab!");
        }
    }
}
