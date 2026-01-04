#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Mirror;

namespace EtherDomes.Network.Editor
{
    /// <summary>
    /// Editor utility to create the network test scene.
    /// </summary>
    public static class NetworkTestSceneSetup
    {
        [MenuItem("EtherDomes/Create Network Test Scene")]
        public static void CreateNetworkTestScene()
        {
            // Create new scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // Create ground plane
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(10, 1, 10);
            
            // Create NetworkManager
            GameObject networkManager = new GameObject("NetworkManager");
            var sessionManager = networkManager.AddComponent<NetworkSessionManager>();
            var authenticator = networkManager.AddComponent<ConnectionApprovalAuthenticator>();
            
            // Note: Transport will be added automatically by NetworkManager or manually by user
            // User should add their preferred transport (KCP, Telepathy, etc.)
            
            // Create spawn points
            for (int i = 0; i < 4; i++)
            {
                GameObject spawnPoint = new GameObject($"SpawnPoint_{i}");
                spawnPoint.AddComponent<NetworkStartPosition>();
                float angle = i * 90f * Mathf.Deg2Rad;
                spawnPoint.transform.position = new Vector3(Mathf.Cos(angle) * 3, 0.5f, Mathf.Sin(angle) * 3);
            }
            
            // Create UI Canvas
            CreateConnectionUI();
            
            // Save scene
            string scenePath = "Assets/_Project/Scenes/NetworkTestScene.unity";
            EditorSceneManager.SaveScene(newScene, scenePath);
            
            Debug.Log($"[NetworkTestSceneSetup] Created network test scene at: {scenePath}");
            
            EditorUtility.DisplayDialog(
                "Scene Created",
                $"Network test scene created at:\n{scenePath}\n\n" +
                "Next steps:\n" +
                "1. Add a Transport component to NetworkManager (e.g., KCP Transport)\n" +
                "2. Create the TestPlayer prefab (EtherDomes > Create Test Player Prefab)\n" +
                "3. Assign the prefab to NetworkManager's Player Prefab field",
                "OK"
            );
        }

        private static void CreateConnectionUI()
        {
            // Create Canvas
            GameObject canvasObj = new GameObject("ConnectionUI");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Create EventSystem
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            
            // Create Panel
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(canvasObj.transform, false);
            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0.7f);
            panelRect.anchorMax = new Vector2(0.3f, 1f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            var panelImage = panel.AddComponent<UnityEngine.UI.Image>();
            panelImage.color = new Color(0, 0, 0, 0.5f);
            
            // Add ConnectionUIController
            panel.AddComponent<ConnectionUIController>();
            
            // Create buttons and input field
            CreateButton(panel.transform, "HostButton", "HOST", new Vector2(0.5f, 0.8f));
            CreateButton(panel.transform, "ClientButton", "CLIENT", new Vector2(0.5f, 0.5f));
            CreateButton(panel.transform, "ServerButton", "SERVER", new Vector2(0.5f, 0.2f));
            CreateInputField(panel.transform, "IPInput", "127.0.0.1", new Vector2(0.5f, 0.35f));
        }

        private static void CreateButton(Transform parent, string name, string text, Vector2 anchorPos)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);
            
            var rect = buttonObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(anchorPos.x - 0.4f, anchorPos.y - 0.1f);
            rect.anchorMax = new Vector2(anchorPos.x + 0.4f, anchorPos.y + 0.1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            var image = buttonObj.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0.2f, 0.6f, 1f);
            
            var button = buttonObj.AddComponent<UnityEngine.UI.Button>();
            
            // Create text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var textComp = textObj.AddComponent<UnityEngine.UI.Text>();
            textComp.text = text;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.color = Color.white;
            textComp.fontSize = 20;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static void CreateInputField(Transform parent, string name, string placeholder, Vector2 anchorPos)
        {
            GameObject inputObj = new GameObject(name);
            inputObj.transform.SetParent(parent, false);
            
            var rect = inputObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(anchorPos.x - 0.4f, anchorPos.y - 0.05f);
            rect.anchorMax = new Vector2(anchorPos.x + 0.4f, anchorPos.y + 0.05f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            var image = inputObj.AddComponent<UnityEngine.UI.Image>();
            image.color = Color.white;
            
            var inputField = inputObj.AddComponent<UnityEngine.UI.InputField>();
            
            // Create text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(inputObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5, 0);
            textRect.offsetMax = new Vector2(-5, 0);
            
            var textComp = textObj.AddComponent<UnityEngine.UI.Text>();
            textComp.color = Color.black;
            textComp.fontSize = 16;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComp.supportRichText = false;
            
            inputField.textComponent = textComp;
            inputField.text = placeholder;
        }
    }
}
#endif
