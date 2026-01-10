#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Unity.Netcode;
using Unity.Netcode.Components;
using System.Collections.Generic;

namespace EtherDomes.Editor
{
    public class MigrationHelper : EditorWindow
    {
        [MenuItem("EtherDomes/Migration Helper")]
        public static void ShowWindow()
        {
            GetWindow<MigrationHelper>("Migration Helper");
        }

        private void OnGUI()
        {
            GUILayout.Label("Migration Tool: Mirror -> NGO", EditorStyles.boldLabel);

            if (GUILayout.Button("1. Fix Player Prefabs"))
            {
                FixPrefabs("Player");
            }

            if (GUILayout.Button("2. Fix Enemy Prefabs"))
            {
                FixPrefabs("Enemy");
            }
            
            if (GUILayout.Button("3. Fix NetworkManager Scene Object"))
            {
                FixNetworkManager();
            }
        }

        private void FixPrefabs(string filter)
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab " + filter);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab != null)
                {
                    Debug.Log($"Processing {prefab.name}...");
                    
                    using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
                    {
                        GameObject root = scope.prefabContentsRoot;
                        bool changed = false;

                        // 1. Remove NetworkIdentity (Mirror) if present (by name/type check involves reflection if type missing, 
                        // but since we deleted files, it's just a 'Missing Script' or we need to find component by name if Mirror dll is gone?)
                        // If Mirror is still in project (package), we can find it.
                        // Assuming Mirror package is still installed but files deleted locally? 
                        // Actually I deleted the script files, but Mirror package usually inside Packages/. 
                        // I only deleted my Scripts. I assume Mirror package is still there or dependencies.
                        
                        // Try to find NetworkIdentity by type (if Mirror is present)
                        var mirrorIdentity = root.GetComponent("Mirror.NetworkIdentity");
                        if (mirrorIdentity != null)
                        {
                            DestroyImmediate(mirrorIdentity);
                            changed = true;
                            Debug.Log("Removed Mirror NetworkIdentity");
                        }

                        // 2. Add NetworkObject (NGO)
                        if (root.GetComponent<NetworkObject>() == null)
                        {
                            root.AddComponent<NetworkObject>();
                            changed = true;
                            Debug.Log("Added NGO NetworkObject");
                        }

                        // 3. Fix Transforms
                        // If it had NetworkTransform (Mirror), replace with NGO NetworkTransform
                        var mirrorTransform = root.GetComponent("Mirror.NetworkTransform");
                        if (mirrorTransform != null)
                        {
                            DestroyImmediate(mirrorTransform);
                            root.AddComponent<NetworkTransform>(); // NGO Default
                            changed = true;
                            Debug.Log("Replaced NetworkTransform");
                        }
                        
                        // 4. Fix Animators
                        var mirrorAnim = root.GetComponent("Mirror.NetworkAnimator");
                        if (mirrorAnim != null)
                        {
                             DestroyImmediate(mirrorAnim);
                             root.AddComponent<NetworkAnimator>();
                             changed = true;
                             Debug.Log("Replaced NetworkAnimator");
                        }

                        if (changed)
                        {
                            Debug.Log($"Updated {prefab.name}");
                        }
                    }
                }
            }
            AssetDatabase.SaveAssets();
        }

        private void FixNetworkManager()
        {
            // Find object in open scene
            var netManagerObj = GameObject.Find("NetworkManager") ?? GameObject.Find("MirrorNetworkManager");
            if (netManagerObj != null)
            {
                // Remove missing scripts (MirrorSessionManager)
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(netManagerObj);
                
                // Add NetworkManager (NGO) if missing
                if (netManagerObj.GetComponent<NetworkManager>() == null)
                {
                    netManagerObj.AddComponent<NetworkManager>();
                }
                
                // Add NetworkSessionManager (Our script)
                if (netManagerObj.GetComponent<EtherDomes.Network.NetworkSessionManager>() == null)
                {
                    netManagerObj.AddComponent<EtherDomes.Network.NetworkSessionManager>();
                }
                
                // Add ConnectionApprovalManager
                if (netManagerObj.GetComponent<EtherDomes.Network.ConnectionApprovalManager>() == null)
                {
                    netManagerObj.AddComponent<EtherDomes.Network.ConnectionApprovalManager>();
                }
                
                // Add Transport (UnityTransport)
                if (netManagerObj.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>() == null)
                {
                    netManagerObj.AddComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
                }
                
                Debug.Log("Fixed NetworkManager object in scene");
            }
            else
            {
                Debug.LogError("Could not find 'NetworkManager' object in scene");
            }
        }
    }
}
#endif
