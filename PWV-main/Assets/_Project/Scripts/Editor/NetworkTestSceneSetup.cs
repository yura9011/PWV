#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using EtherDomes.Network;

namespace EtherDomes.Editor
{
    /// <summary>
    /// Editor utility to create the network test scene with Mirror.
    /// </summary>
    public static class NetworkTestSceneSetup
    {
        [MenuItem("EtherDomes/Create Network Test Scene")]
        public static void CreateNetworkTestScene()
        {
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // Ground
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(10, 1, 10);
            var groundMat = new Material(Shader.Find("Standard"));
            groundMat.color = new Color(0.3f, 0.5f, 0.3f);
            ground.GetComponent<MeshRenderer>().material = groundMat;
            
            // NetworkManager with NGO
            GameObject networkManager = new GameObject("NetworkManager");
            networkManager.AddComponent<Unity.Netcode.NetworkManager>();
            networkManager.AddComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            networkManager.AddComponent<NetworkSessionManager>();
            networkManager.AddComponent<ConnectionApprovalManager>();
            
            // Spawn points
            for (int i = 0; i < 4; i++)
            {
                GameObject spawnPoint = new GameObject($"SpawnPoint_{i}");
                // spawnPoint.AddComponent<NetworkStartPosition>(); // NGO uses NetworkConfig or custom logic
                float angle = i * 90f * Mathf.Deg2Rad;
                spawnPoint.transform.position = new Vector3(Mathf.Cos(angle) * 3, 0.5f, Mathf.Sin(angle) * 3);
            }
            
            // Camera
            var mainCamera = UnityEngine.Camera.main;
            if (mainCamera != null)
            {
                mainCamera.transform.position = new Vector3(0, 10, -10);
                mainCamera.transform.rotation = Quaternion.Euler(45, 0, 0);
            }
            
            // UI
            CreateConnectionUI();
            
            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
            }
            
            string scenePath = "Assets/_Project/Scenes/NetworkTestScene.unity";
            EditorSceneManager.SaveScene(newScene, scenePath);
            
            UnityEngine.Debug.Log($"[NetworkTestSceneSetup] Created network test scene at: {scenePath}");
            
            EditorUtility.DisplayDialog(
                "Scene Created",
                $"Network test scene created at:\n{scenePath}\n\n" +
                "Next steps:\n" +
                "1. Create player prefab: EtherDomes > Create Network Player Prefab\n" +
                "2. Assign prefab to NetworkManager's Player Prefab field\n" +
                "3. Press Play and click Host/Join",
                "OK"
            );
        }

        private static void CreateConnectionUI()
        {
            GameObject canvasObj = new GameObject("Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            
            // Main panel
            GameObject mainPanel = new GameObject("MainMenuPanel");
            mainPanel.transform.SetParent(canvasObj.transform, false);
            var panelRect = mainPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            var panelImage = mainPanel.AddComponent<UnityEngine.UI.Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);
            
            // Title
            CreateText(mainPanel.transform, "TitleText", "NETWORK TEST", new Vector2(0, 150), 32);
            
            // Buttons
            CreateButton(mainPanel.transform, "HostButton", "HOST", new Vector2(-100, 0), new Color(0.2f, 0.6f, 0.2f));
            CreateButton(mainPanel.transform, "JoinButton", "JOIN", new Vector2(100, 0), new Color(0.3f, 0.5f, 0.7f));
            
            // IP Input
            CreateInputField(mainPanel.transform, "IPInput", "127.0.0.1", new Vector2(0, -60));
            
            // Status
            CreateText(mainPanel.transform, "StatusText", "Ready", new Vector2(0, -120), 16);
            
            // Add controller
            var controller = canvasObj.AddComponent<UI.MainMenuController>();
        }

        private static void CreateButton(Transform parent, string name, string text, Vector2 pos, Color color)
        {
            GameObject buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent, false);

            RectTransform rect = buttonGO.AddComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(150, 50);

            var image = buttonGO.AddComponent<UnityEngine.UI.Image>();
            image.color = color;

            buttonGO.AddComponent<UnityEngine.UI.Button>();

            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);

            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var textComp = textGO.AddComponent<UnityEngine.UI.Text>();
            textComp.text = text;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComp.fontSize = 20;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.color = Color.white;
        }

        private static void CreateText(Transform parent, string name, string content, Vector2 pos, int fontSize)
        {
            GameObject textGO = new GameObject(name);
            textGO.transform.SetParent(parent, false);

            RectTransform rect = textGO.AddComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(400, 50);

            var text = textGO.AddComponent<UnityEngine.UI.Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
        }

        private static void CreateInputField(Transform parent, string name, string placeholder, Vector2 pos)
        {
            GameObject inputGO = new GameObject(name);
            inputGO.transform.SetParent(parent, false);

            RectTransform rect = inputGO.AddComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(200, 35);

            var image = inputGO.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0.2f, 0.2f, 0.25f);

            var input = inputGO.AddComponent<UnityEngine.UI.InputField>();

            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(inputGO.transform, false);

            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5, 0);
            textRect.offsetMax = new Vector2(-5, 0);

            var textComp = textGO.AddComponent<UnityEngine.UI.Text>();
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComp.fontSize = 16;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.color = Color.white;

            input.textComponent = textComp;
            input.text = placeholder;
        }
    }
}
#endif
