using UnityEngine;
using UnityEditor;
using TMPro;

namespace EtherDomes.Editor
{
    public static class FCTPrefabSetup
    {
        [MenuItem("EtherDomes/Setup Floating Combat Text")]
        public static void SetupFCT()
        {
            // Create FCT prefab
            CreateFCTPrefab();
            
            // Add FloatingCombatText to scene
            AddFCTToScene();
            
            Debug.Log("[FCTPrefabSetup] Floating Combat Text setup complete!");
        }

        private static void CreateFCTPrefab()
        {
            string prefabPath = "Assets/_Project/Prefabs/UI/FCTText.prefab";
            
            // Check if already exists
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                Debug.Log("[FCTPrefabSetup] FCT prefab already exists");
                return;
            }

            // Create root object
            var root = new GameObject("FCTText");

            // Create text child
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(root.transform);
            textObj.transform.localPosition = Vector3.zero;

            // Add TextMeshPro component
            var tmp = textObj.AddComponent<TextMeshPro>();
            tmp.text = "0";
            tmp.fontSize = 8;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Overflow;
            
            // Set sorting
            tmp.sortingOrder = 100;

            // Save as prefab
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);

            Debug.Log("[FCTPrefabSetup] Created FCT prefab at " + prefabPath);
        }

        private static void AddFCTToScene()
        {
            // Check if already in scene
            var existing = Object.FindFirstObjectByType<EtherDomes.UI.FloatingCombatText>();
            if (existing != null)
            {
                Debug.Log("[FCTPrefabSetup] FloatingCombatText already in scene");
                AssignPrefabToFCT(existing);
                return;
            }

            // Create new GameObject
            var fctObj = new GameObject("FloatingCombatText");
            var fct = fctObj.AddComponent<EtherDomes.UI.FloatingCombatText>();
            
            AssignPrefabToFCT(fct);
            
            Debug.Log("[FCTPrefabSetup] Added FloatingCombatText to scene");
        }

        private static void AssignPrefabToFCT(EtherDomes.UI.FloatingCombatText fct)
        {
            string prefabPath = "Assets/_Project/Prefabs/UI/FCTText.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (prefab == null)
            {
                Debug.LogWarning("[FCTPrefabSetup] FCT prefab not found!");
                return;
            }

            var field = typeof(EtherDomes.UI.FloatingCombatText).GetField("_fctPrefab",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                field.SetValue(fct, prefab);
                EditorUtility.SetDirty(fct);
                Debug.Log("[FCTPrefabSetup] Assigned prefab to FloatingCombatText");
            }
        }
    }
}
