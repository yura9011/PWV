using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using Unity.Cinemachine;
using EtherDomes.Network;
using EtherDomes.Camera;
using Mirror;

namespace EtherDomes.Editor
{
    /// <summary>
    /// Editor utility to create and configure Cinemachine camera and complete scene setup.
    /// </summary>
    public static class CinemachineSetupCreator
    {
        [MenuItem("EtherDomes/Create Cinemachine Camera")]
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

            UnityEngine.Debug.Log("[CinemachineSetup] Camera created!");
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

        [MenuItem("EtherDomes/Setup Complete Scene")]
        public static void SetupCompleteScene()
        {
            UnityEngine.Debug.Log("[Setup] ========== STARTING COMPLETE SCENE SETUP ==========");
            
            // 0. Clean everything first
            CleanSceneSilent();
            
            // 1. Create ground plane
            CreateGroundPlaneSilent();
            
            // 2. Create camera
            CreateCameraSilent();

            // 3. Setup Network (Mirror)
            SetupNetworkSilent();

            // 4. Create Main Menu UI
            CreateMainMenuUISilent();
            
            // 5. Save scene
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            UnityEngine.Debug.Log("[Setup] ========== COMPLETE SCENE SETUP FINISHED ==========");
            EditorUtility.DisplayDialog("Setup Complete", 
                "Scene setup complete!\n\n" +
                "✓ Scene cleaned\n" +
                "✓ Ground plane created\n" +
                "✓ Camera configured\n" +
                "✓ NetworkManager (Mirror) configured\n" +
                "✓ Main Menu UI created\n\n" +
                "Press Play and click Host to test!", 
                "OK");
        }
        
        public static void CleanSceneSilent()
        {
            var allCanvas = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var canvas in allCanvas)
            {
                Object.DestroyImmediate(canvas.gameObject);
            }
            
            var allEventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            foreach (var es in allEventSystems)
            {
                Object.DestroyImmediate(es.gameObject);
            }
            
            var connectionUI = GameObject.Find("ConnectionUI");
            if (connectionUI != null)
            {
                Object.DestroyImmediate(connectionUI);
            }
        }
        
        private static void CreateGroundPlaneSilent()
        {
            var existingGround = GameObject.Find("Ground");
            if (existingGround != null) return;
            
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(10, 1, 10);
            
            var renderer = ground.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                if (shader != null)
                {
                    Material mat = new Material(shader);
                    mat.color = new Color(0.3f, 0.5f, 0.3f);
                    renderer.material = mat;
                }
            }
            
            var existingLight = Object.FindFirstObjectByType<Light>();
            if (existingLight == null)
            {
                GameObject lightGO = new GameObject("Directional Light");
                var light = lightGO.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1f;
                lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);
            }
        }
        
        private static void CreateCameraSilent()
        {
            var mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null)
            {
                mainCamera = Object.FindFirstObjectByType<UnityEngine.Camera>();
                if (mainCamera == null)
                {
                    GameObject mainCamGO = new GameObject("Main Camera");
                    mainCamera = mainCamGO.AddComponent<UnityEngine.Camera>();
                    mainCamGO.AddComponent<AudioListener>();
                    mainCamGO.tag = "MainCamera";
                    mainCamGO.transform.position = new Vector3(0, 5, -10);
                }
                else
                {
                    mainCamera.gameObject.tag = "MainCamera";
                }
            }
            
            var cameraController = mainCamera.GetComponent<ThirdPersonCameraController>();
            if (cameraController == null)
            {
                mainCamera.gameObject.AddComponent<ThirdPersonCameraController>();
            }
        }
        
        private static void SetupNetworkSilent()
        {
            // Find or create NetworkManager with Mirror
            var networkManager = Object.FindFirstObjectByType<MirrorNetworkSessionManager>();
            if (networkManager == null)
            {
                GameObject nmGO = new GameObject("NetworkManager");
                networkManager = nmGO.AddComponent<MirrorNetworkSessionManager>();
                nmGO.AddComponent<kcp2k.KcpTransport>();
                nmGO.AddComponent<MirrorConnectionApprovalAuthenticator>();
                UnityEngine.Debug.Log("[Setup] Created NetworkManager with Mirror");
            }

            // Try to find player prefab
            string prefabPath = "Assets/_Project/Prefabs/Characters/NetworkPlayer.prefab";
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (playerPrefab == null)
            {
                prefabPath = "Assets/_Project/Prefabs/Player.prefab";
                playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            }
            
            if (playerPrefab != null)
            {
                networkManager.playerPrefab = playerPrefab;
                EditorUtility.SetDirty(networkManager);
                UnityEngine.Debug.Log("[Setup] Player prefab assigned to NetworkManager");
            }
            else
            {
                UnityEngine.Debug.LogWarning("[Setup] Player prefab not found. Run 'Create Network Player Prefab' first.");
            }
        }
        
        private static void CreateMainMenuUISilent()
        {
            // Create Canvas
            GameObject canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // Create EventSystem
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();

            // Create main panel
            GameObject mainPanel = new GameObject("MainMenuPanel");
            mainPanel.transform.SetParent(canvasGO.transform, false);
            var panelRect = mainPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            var panelImage = mainPanel.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

            // Title
            CreateTextSilent(mainPanel.transform, "TitleText", "THE ETHER DOMES", new Vector2(0, 200), Color.white, 36);
            
            // Class selection label
            CreateTextSilent(mainPanel.transform, "ClassLabel", "Selecciona tu Clase:", new Vector2(0, 100), Color.white, 24);

            // Class buttons
            CreateButtonSilent(mainPanel.transform, "GuerreroButton", "GUERRERO", new Vector2(-120, 40), new Color(0.8f, 0.2f, 0.2f));
            CreateButtonSilent(mainPanel.transform, "MagoButton", "MAGO", new Vector2(120, 40), new Color(0.2f, 0.4f, 0.8f));
            
            // Connection buttons
            CreateButtonSilent(mainPanel.transform, "HostButton", "HOST", new Vector2(-100, -170), new Color(0.2f, 0.6f, 0.2f));
            CreateButtonSilent(mainPanel.transform, "JoinButton", "JOIN", new Vector2(100, -170), new Color(0.3f, 0.5f, 0.7f));
            
            // IP input
            CreateInputFieldSilent(mainPanel.transform, "IPInput", "127.0.0.1", new Vector2(0, -110));
            
            // Status text
            CreateTextSilent(mainPanel.transform, "StatusText", "Selecciona tu clase y conecta", new Vector2(0, -230), Color.gray);

            // Add MirrorMainMenuController
            GameObject controllerGO = new GameObject("MainMenuController");
            controllerGO.transform.SetParent(canvasGO.transform);
            controllerGO.AddComponent<UI.MirrorMainMenuController>();

            UnityEngine.Debug.Log("[Setup] Main Menu UI created with MirrorMainMenuController");
        }
        
        private static void CreateButtonSilent(Transform parent, string name, string text, Vector2 position, Color color)
        {
            GameObject buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent, false);

            var rect = buttonGO.AddComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(180, 50);

            var image = buttonGO.AddComponent<Image>();
            image.color = color;

            var button = buttonGO.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = color * 1.2f;
            colors.pressedColor = color * 0.8f;
            button.colors = colors;

            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var textComp = textGO.AddComponent<Text>();
            textComp.text = text;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComp.fontSize = 20;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.color = Color.white;
        }
        
        private static void CreateTextSilent(Transform parent, string name, string content, Vector2 position, Color color, int fontSize = 16)
        {
            GameObject textGO = new GameObject(name);
            textGO.transform.SetParent(parent, false);
            var rect = textGO.AddComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(400, 50);

            var text = textGO.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
        }
        
        private static void CreateInputFieldSilent(Transform parent, string name, string placeholder, Vector2 position)
        {
            GameObject inputGO = new GameObject(name);
            inputGO.transform.SetParent(parent, false);
            var rect = inputGO.AddComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(250, 40);

            var image = inputGO.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.25f);

            var input = inputGO.AddComponent<InputField>();

            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(inputGO.transform, false);
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, -5);

            var textComp = textGO.AddComponent<Text>();
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComp.fontSize = 18;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.color = Color.white;

            input.textComponent = textComp;
            input.text = placeholder;
        }
    }
}
