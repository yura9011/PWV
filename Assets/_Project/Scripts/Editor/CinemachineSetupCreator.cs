using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using Unity.Cinemachine;
using EtherDomes.Camera;

namespace EtherDomes.Editor
{
    /// <summary>
    /// Editor utility for Cinemachine camera creation (legacy/optional).
    /// Main setup is now in Tools > EtherDomes > Setup Network and UI
    /// </summary>
    public static class CinemachineSetupCreator
    {
        [MenuItem("EtherDomes/Create Cinemachine Camera (Optional)")]
        public static void CreateCinemachineCamera()
        {
            var existingCamera = Object.FindFirstObjectByType<CinemachineCamera>();
            if (existingCamera != null)
            {
                if (!EditorUtility.DisplayDialog("Camera Exists", 
                    "A CinemachineCamera already exists. Create another?", "Yes", "No"))
                {
                    Selection.activeGameObject = existingCamera.gameObject;
                    return;
                }
            }

            GameObject cameraGO = new GameObject("PlayerCamera");
            var cmCamera = cameraGO.AddComponent<CinemachineCamera>();
            var thirdPerson = cameraGO.AddComponent<CinemachineFollow>();
            thirdPerson.FollowOffset = new Vector3(0, 3f, -6f);
            cameraGO.AddComponent<CinemachineRotationComposer>();
            cameraGO.AddComponent<CinemachineInputAxisController>();
            cameraGO.AddComponent<CinemachineDeoccluder>();
            cameraGO.transform.position = new Vector3(0, 5, -10);

            EditorUtility.SetDirty(cmCamera);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            UnityEngine.Debug.Log("[CinemachineSetup] Cinemachine Camera created!");
            EditorUtility.DisplayDialog("Success", "Cinemachine Camera created!", "OK");
            Selection.activeGameObject = cameraGO;
        }

        [MenuItem("EtherDomes/Clean Scene (Remove Duplicate UI)")]
        public static void CleanScene()
        {
            UnityEngine.Debug.Log("[Setup] Cleaning duplicate UI elements...");
            
            var connectionUI = GameObject.Find("ConnectionUI");
            if (connectionUI != null)
            {
                Object.DestroyImmediate(connectionUI);
            }
            
            var allCanvas = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            Canvas keepCanvas = null;
            foreach (var canvas in allCanvas)
            {
                if (canvas.gameObject.name == "Canvas" && keepCanvas == null)
                {
                    keepCanvas = canvas;
                }
            }
            if (keepCanvas == null && allCanvas.Length > 0)
            {
                keepCanvas = allCanvas[0];
            }
            
            foreach (var canvas in allCanvas)
            {
                if (canvas != keepCanvas)
                {
                    Object.DestroyImmediate(canvas.gameObject);
                }
            }
            
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            
            EditorUtility.DisplayDialog("Scene Cleaned", "Removed duplicate UI elements.", "OK");
        }
    }
}
