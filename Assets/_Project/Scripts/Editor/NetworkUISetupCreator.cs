using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using EtherDomes.Network;
using EtherDomes.UI;
using EtherDomes.Camera;

namespace EtherDomes.Editor
{
    public static class NetworkUISetupCreator
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/MainGame.unity";
        
        public static void SetupNetworkAndUI()
        {
            Debug.Log("[Setup] ========== CREATING NEW SCENE WITH NETWORK AND UI ==========");
            
            // Create a brand new empty scene
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // Setup everything from scratch
            CreateCameraSilent();
            CreateGroundPlaneSilent();
            SetupNetworkSilent();
            CreateMenuUI();
            CreatePauseMenuController();
            
            // Save the scene
            EditorSceneManager.SaveScene(newScene, SCENE_PATH);
            
            // Add to build settings
            AddSceneToBuildSettings(SCENE_PATH);

            Debug.Log("[Setup] ========== SETUP FINISHED ==========");
            EditorUtility.DisplayDialog("Setup Complete", 
                $"Scene created at:\n{SCENE_PATH}\n\nAdded to Build Settings.\nPress Play to test!", "OK");
        }
        
        private static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
            EditorBuildSettings.scenes = scenes;
            Debug.Log($"[Setup] Build settings updated - only scene: {scenePath}");
        }
        
        private static void CreatePauseMenuController()
        {
            // Add PauseMenuController to Canvas
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                var pauseController = canvas.GetComponent<PauseMenuController>();
                if (pauseController == null)
                {
                    canvas.gameObject.AddComponent<PauseMenuController>();
                    Debug.Log("[Setup] Added PauseMenuController to Canvas");
                }
                
                var sessionUI = canvas.GetComponent<GameSessionUI>();
                if (sessionUI == null)
                {
                    canvas.gameObject.AddComponent<GameSessionUI>();
                    Debug.Log("[Setup] Added GameSessionUI to Canvas");
                }
            }
        }

        public static void SetupFullScene()
        {
            CleanSceneSilent();
            CreateGroundPlaneSilent();
            CreateCameraSilent();
            SetupNetworkSilent();
            CreateMenuUI();
            CreatePauseMenuController();
            
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
        }
        
        private static void CleanSceneSilent()
        {
            var allCanvas = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var canvas in allCanvas)
                Object.DestroyImmediate(canvas.gameObject);
            
            var allEventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            foreach (var es in allEventSystems)
                Object.DestroyImmediate(es.gameObject);

            var oldControllers = Object.FindObjectsByType<MainMenuController>(FindObjectsSortMode.None);
            foreach (var c in oldControllers)
                Object.DestroyImmediate(c.gameObject);
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
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                if (shader != null)
                {
                    Material mat = new Material(shader);
                    mat.color = new Color(0.3f, 0.5f, 0.3f);
                    renderer.material = mat;
                }
            }
            
            if (Object.FindFirstObjectByType<Light>() == null)
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
            
            // Add ThirdPersonCameraController if not present
            var cameraController = mainCamera.GetComponent<EtherDomes.Camera.ThirdPersonCameraController>();
            if (cameraController == null)
            {
                mainCamera.gameObject.AddComponent<EtherDomes.Camera.ThirdPersonCameraController>();
                Debug.Log("[Setup] Added ThirdPersonCameraController to Main Camera");
            }
        }
        
        private static void SetupNetworkSilent()
        {
            var networkManager = Object.FindFirstObjectByType<NetworkManager>();
            if (networkManager == null)
            {
                GameObject nmGO = new GameObject("NetworkManager");
                networkManager = nmGO.AddComponent<NetworkManager>();
                nmGO.AddComponent<UnityTransport>();
                nmGO.AddComponent<NetworkSessionManager>();
                nmGO.AddComponent<ConnectionApprovalManager>();
                nmGO.AddComponent<RelayManager>();
                nmGO.AddComponent<LobbyManager>();

                var transport = nmGO.GetComponent<UnityTransport>();
                if (networkManager.NetworkConfig == null) 
                    networkManager.NetworkConfig = new NetworkConfig();
                
                networkManager.NetworkConfig.NetworkTransport = transport;
                networkManager.NetworkConfig.ConnectionApproval = true;
            }

            // Always recreate the player prefab to ensure it has the correct components
            GameObject playerPrefab = CreatePlayerPrefab();
            
            if (playerPrefab != null)
            {
                if (networkManager.NetworkConfig == null) 
                    networkManager.NetworkConfig = new NetworkConfig();
                
                networkManager.NetworkConfig.PlayerPrefab = playerPrefab;
                
                if (networkManager.NetworkConfig.Prefabs == null) 
                    networkManager.NetworkConfig.Prefabs = new NetworkPrefabs();
                    
                bool exists = false;
                foreach (var item in networkManager.NetworkConfig.Prefabs.Prefabs)
                {
                    if (item.Prefab == playerPrefab) { exists = true; break; }
                }
                
                if (!exists)
                    networkManager.NetworkConfig.Prefabs.Add(new NetworkPrefab { Prefab = playerPrefab });
                
                EditorUtility.SetDirty(networkManager);
            }
        }
        
        private static GameObject CreatePlayerPrefab()
        {
            string prefabPath = "Assets/_Project/Prefabs/Characters";
            if (!AssetDatabase.IsValidFolder(prefabPath))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs"))
                    AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Characters");
            }
            
            string fullPath = $"{prefabPath}/NetworkPlayer.prefab";
            
            // Create new player prefab
            GameObject playerGO = new GameObject("NetworkPlayer");

            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "PlayerModel";
            capsule.transform.SetParent(playerGO.transform);
            capsule.transform.localPosition = new Vector3(0, 1f, 0);
            Object.DestroyImmediate(capsule.GetComponent<CapsuleCollider>());

            var renderer = capsule.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                if (shader != null)
                {
                    Material mat = new Material(shader);
                    mat.color = Color.gray;
                    renderer.material = mat;
                }
            }

            playerGO.AddComponent<NetworkObject>();

            var charController = playerGO.AddComponent<CharacterController>();
            charController.height = 2f;
            charController.radius = 0.5f;
            charController.center = new Vector3(0, 1f, 0);
            charController.slopeLimit = 45f;
            charController.stepOffset = 0.3f;

            // Use our custom ClientNetworkTransform so client has authority over their position
            var networkTransform = playerGO.AddComponent<EtherDomes.Network.ClientNetworkTransform>();
            networkTransform.SyncPositionX = true;
            networkTransform.SyncPositionY = true;
            networkTransform.SyncPositionZ = true;
            networkTransform.SyncRotAngleY = true;
            
            playerGO.AddComponent<EtherDomes.Player.PlayerController>();

            // Save prefab
            var prefab = PrefabUtility.SaveAsPrefabAsset(playerGO, fullPath);
            Object.DestroyImmediate(playerGO);
            
            Debug.Log($"[Setup] Player prefab created/updated at {fullPath}");
            return prefab;
        }

        private static void CreateMenuUI()
        {
            // Cleanup
            var allEventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            foreach (var es in allEventSystems)
                Object.DestroyImmediate(es.gameObject);

            var existingControllers = Object.FindObjectsByType<MainMenuController>(FindObjectsSortMode.None);
            foreach (var c in existingControllers)
                Object.DestroyImmediate(c.gameObject);

            GameObject canvasGO = GameObject.Find("Canvas");
            if (canvasGO != null) Object.DestroyImmediate(canvasGO);

            // EventSystem
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();

            // Canvas
            canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            var controller = canvasGO.AddComponent<MainMenuController>();

            // Create all panels
            var mainMenuPanel = CreatePanel(canvasGO.transform, "MainMenuPanel");
            var modeMenuPanel = CreatePanel(canvasGO.transform, "ModeMenuPanel");
            var createMenuPanel = CreatePanel(canvasGO.transform, "CreateMenuPanel");
            var hostInfoPanel = CreatePanel(canvasGO.transform, "HostInfoPanel");
            var joinMenuPanel = CreatePanel(canvasGO.transform, "JoinMenuPanel");
            var classMenuPanel = CreatePanel(canvasGO.transform, "ClassMenuPanel");

            // === 1. MAIN MENU PANEL ===
            CreateText(mainMenuPanel.transform, "Title", "The Ether Domes", new Vector2(0, 150), Color.white, 48);
            var iniciarBtn = CreateButton(mainMenuPanel.transform, "IniciarButton", "INICIAR", new Vector2(0, 20), new Color(0.2f, 0.6f, 0.2f), 220);
            var salirBtn = CreateButton(mainMenuPanel.transform, "SalirButton", "SALIR", new Vector2(0, -50), new Color(0.6f, 0.2f, 0.2f), 220);

            // === 2. MODE MENU PANEL ===
            CreateText(modeMenuPanel.transform, "Title", "MODO DE JUEGO", new Vector2(0, 100), Color.white, 36);
            var crearBtn = CreateButton(modeMenuPanel.transform, "CrearButton", "CREAR", new Vector2(0, 20), new Color(0.2f, 0.5f, 0.2f), 220);
            var unirseBtn = CreateButton(modeMenuPanel.transform, "UnirseButton", "UNIRSE", new Vector2(0, -50), new Color(0.3f, 0.5f, 0.7f), 220);
            var modeBackBtn = CreateButton(modeMenuPanel.transform, "BackButton", "ATRÁS", new Vector2(0, -120), new Color(0.5f, 0.5f, 0.5f), 220);

            // === 3. CREATE MENU PANEL ===
            CreateText(createMenuPanel.transform, "Title", "CREAR PARTIDA", new Vector2(0, 100), Color.white, 36);
            var iniciarServidorBtn = CreateButton(createMenuPanel.transform, "IniciarServidorButton", "INICIAR SERVIDOR", new Vector2(0, 20), new Color(0.2f, 0.6f, 0.2f), 280);
            var servidorDedicadoBtn = CreateButton(createMenuPanel.transform, "ServidorDedicadoButton", "SERVIDOR DEDICADO", new Vector2(0, -50), new Color(0.4f, 0.4f, 0.4f), 280);
            var createBackBtn = CreateButton(createMenuPanel.transform, "BackButton", "ATRÁS", new Vector2(0, -120), new Color(0.5f, 0.5f, 0.5f), 220);

            // === 4. HOST INFO PANEL ===
            CreateText(hostInfoPanel.transform, "Title", "SERVIDOR INICIADO", new Vector2(0, 120), Color.white, 36);
            var relayCodeText = CreateText(hostInfoPanel.transform, "RelayCodeText", "Código Relay: ---", new Vector2(0, 50), Color.yellow, 24, 400);
            var ipAddressText = CreateText(hostInfoPanel.transform, "IPAddressText", "IP Local: ---", new Vector2(0, 10), Color.cyan, 20, 400);
            CreateText(hostInfoPanel.transform, "InfoText", "Comparte el código o IP con otros jugadores", new Vector2(0, -30), Color.gray, 14, 400);
            var hostInfoAcceptBtn = CreateButton(hostInfoPanel.transform, "AcceptButton", "ACEPTAR", new Vector2(80, -100), new Color(0.2f, 0.6f, 0.2f), 150);
            var hostInfoBackBtn = CreateButton(hostInfoPanel.transform, "BackButton", "ATRÁS", new Vector2(-80, -100), new Color(0.5f, 0.5f, 0.5f), 150);

            // === 5. JOIN MENU PANEL ===
            CreateText(joinMenuPanel.transform, "Title", "UNIRSE A PARTIDA", new Vector2(0, 120), Color.white, 36);
            var connLabel = CreateText(joinMenuPanel.transform, "ConnectionLabel", "Ingresa IP:", new Vector2(0, 50), Color.gray, 18);
            var connInput = CreateInputField(joinMenuPanel.transform, "ConnectionInput", "127.0.0.1", new Vector2(0, 10), 280);
            var relayToggle = CreateToggle(joinMenuPanel.transform, "UseRelayToggle", "Usar Relay", new Vector2(0, -35), false);
            var joinAcceptBtn = CreateButton(joinMenuPanel.transform, "AcceptButton", "ACEPTAR", new Vector2(80, -100), new Color(0.2f, 0.6f, 0.2f), 150);
            var joinBackBtn = CreateButton(joinMenuPanel.transform, "BackButton", "ATRÁS", new Vector2(-80, -100), new Color(0.5f, 0.5f, 0.5f), 150);

            // === 6. CLASS MENU PANEL ===
            CreateText(classMenuPanel.transform, "Title", "SELECCIONA CLASE", new Vector2(0, 120), Color.white, 36);
            var guerreroBtn = CreateButton(classMenuPanel.transform, "GuerreroButton", "GUERRERO", new Vector2(-100, 20), new Color(0.6f, 0.2f, 0.2f), 180);
            var magoBtn = CreateButton(classMenuPanel.transform, "MagoButton", "MAGO", new Vector2(100, 20), new Color(0.2f, 0.3f, 0.6f), 180);
            var classAcceptBtn = CreateButton(classMenuPanel.transform, "AcceptButton", "ACEPTAR", new Vector2(0, -60), new Color(0.2f, 0.6f, 0.2f), 220);

            // Status Text (on canvas root)
            var statusText = CreateText(canvasGO.transform, "StatusText", "Bienvenido", new Vector2(0, -220), Color.gray, 16, 500);

            // === ASSIGN REFERENCES ===
            SerializedObject so = new SerializedObject(controller);
            so.Update();
            
            // Panels
            so.FindProperty("_mainMenuPanel").objectReferenceValue = mainMenuPanel;
            so.FindProperty("_modeMenuPanel").objectReferenceValue = modeMenuPanel;
            so.FindProperty("_createMenuPanel").objectReferenceValue = createMenuPanel;
            so.FindProperty("_hostInfoPanel").objectReferenceValue = hostInfoPanel;
            so.FindProperty("_joinMenuPanel").objectReferenceValue = joinMenuPanel;
            so.FindProperty("_classMenuPanel").objectReferenceValue = classMenuPanel;
            
            // Main Menu
            so.FindProperty("_iniciarButton").objectReferenceValue = iniciarBtn;
            so.FindProperty("_salirButton").objectReferenceValue = salirBtn;
            
            // Mode Menu
            so.FindProperty("_crearButton").objectReferenceValue = crearBtn;
            so.FindProperty("_unirseButton").objectReferenceValue = unirseBtn;
            so.FindProperty("_modeBackButton").objectReferenceValue = modeBackBtn;

            // Create Menu
            so.FindProperty("_iniciarServidorButton").objectReferenceValue = iniciarServidorBtn;
            so.FindProperty("_servidorDedicadoButton").objectReferenceValue = servidorDedicadoBtn;
            so.FindProperty("_createBackButton").objectReferenceValue = createBackBtn;

            // Host Info Menu
            so.FindProperty("_relayCodeText").objectReferenceValue = relayCodeText;
            so.FindProperty("_ipAddressText").objectReferenceValue = ipAddressText;
            so.FindProperty("_hostInfoAcceptButton").objectReferenceValue = hostInfoAcceptBtn;
            so.FindProperty("_hostInfoBackButton").objectReferenceValue = hostInfoBackBtn;
            
            // Join Menu
            so.FindProperty("_connectionInput").objectReferenceValue = connInput;
            so.FindProperty("_connectionInputLabel").objectReferenceValue = connLabel;
            so.FindProperty("_useRelayToggle").objectReferenceValue = relayToggle;
            so.FindProperty("_joinAcceptButton").objectReferenceValue = joinAcceptBtn;
            so.FindProperty("_joinBackButton").objectReferenceValue = joinBackBtn;
            
            // Class Menu
            so.FindProperty("_guerreroButton").objectReferenceValue = guerreroBtn;
            so.FindProperty("_magoButton").objectReferenceValue = magoBtn;
            so.FindProperty("_classAcceptButton").objectReferenceValue = classAcceptBtn;
            
            // Status
            so.FindProperty("_statusText").objectReferenceValue = statusText;
            
            so.ApplyModifiedProperties();

            // Hide all except main
            modeMenuPanel.SetActive(false);
            createMenuPanel.SetActive(false);
            hostInfoPanel.SetActive(false);
            joinMenuPanel.SetActive(false);
            classMenuPanel.SetActive(false);

            Debug.Log("[Setup] Menu UI created successfully");
        }

        private static GameObject CreatePanel(Transform parent, string name)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            
            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            var image = panel.AddComponent<Image>();
            image.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            
            return panel;
        }

        private static Button CreateButton(Transform parent, string name, string text, Vector2 position, Color color, float width = 180f)
        {
            GameObject buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent, false);

            var rect = buttonGO.AddComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(width, 50);

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
            textComp.fontSize = 22;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.color = Color.white;

            return button;
        }

        private static Text CreateText(Transform parent, string name, string content, Vector2 position, Color color, int fontSize = 16, float width = 400)
        {
            GameObject textGO = new GameObject(name);
            textGO.transform.SetParent(parent, false);
            
            var rect = textGO.AddComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(width, fontSize + 20);

            var text = textGO.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            return text;
        }

        private static InputField CreateInputField(Transform parent, string name, string placeholder, Vector2 position, float width = 250)
        {
            GameObject inputGO = new GameObject(name);
            inputGO.transform.SetParent(parent, false);
            
            var rect = inputGO.AddComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(width, 40);

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

            return input;
        }

        private static Toggle CreateToggle(Transform parent, string name, string label, Vector2 position, bool isOn)
        {
            GameObject toggleGO = new GameObject(name);
            toggleGO.transform.SetParent(parent, false);
            
            var rect = toggleGO.AddComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(160, 25);

            var toggle = toggleGO.AddComponent<Toggle>();
            
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(toggleGO.transform, false);
            var bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0.5f);
            bgRect.anchorMax = new Vector2(0, 0.5f);
            bgRect.anchoredPosition = new Vector2(12, 0);
            bgRect.sizeDelta = new Vector2(24, 24);
            var bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f);
            toggle.targetGraphic = bgImage;

            GameObject check = new GameObject("Checkmark");
            check.transform.SetParent(bg.transform, false);
            var checkRect = check.AddComponent<RectTransform>();
            checkRect.anchorMin = Vector2.zero;
            checkRect.anchorMax = Vector2.one;
            checkRect.offsetMin = new Vector2(4, 4);
            checkRect.offsetMax = new Vector2(-4, -4);
            var checkImage = check.AddComponent<Image>();
            checkImage.color = new Color(0.3f, 0.8f, 0.3f);
            toggle.graphic = checkImage;
            
            GameObject labelGO = new GameObject("Label");
            labelGO.transform.SetParent(toggleGO.transform, false);
            var labelRect = labelGO.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(35, 0);
            labelRect.offsetMax = Vector2.zero;
            var labelText = labelGO.AddComponent<Text>();
            labelText.text = label;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 16;
            labelText.alignment = TextAnchor.MiddleLeft;
            labelText.color = Color.white;

            toggle.isOn = isOn;
            
            return toggle;
        }
    }
}
