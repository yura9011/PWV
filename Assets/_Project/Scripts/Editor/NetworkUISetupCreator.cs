using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using EtherDomes.Network;
using EtherDomes.UI;
using EtherDomes.UI.Menus;
using EtherDomes.Camera;

namespace EtherDomes.Editor
{
    public static class NetworkUISetupCreator
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/MainGame.unity";
        
        [MenuItem("Tools/EtherDomes/Crear Escena Menu Principal")]
        public static void CrearEscenaMenuPrincipal()
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
            // Cleanup old UI systems
            var allEventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            foreach (var es in allEventSystems)
                Object.DestroyImmediate(es.gameObject);

            var existingControllers = Object.FindObjectsByType<MainMenuController>(FindObjectsSortMode.None);
            foreach (var c in existingControllers)
                Object.DestroyImmediate(c.gameObject);

            GameObject canvasGO = GameObject.Find("Canvas");
            if (canvasGO != null) Object.DestroyImmediate(canvasGO);
            
            // Remove old menu navigator if exists
            var oldNavigators = Object.FindObjectsByType<EtherDomes.UI.MenuNavigator>(FindObjectsSortMode.None);
            foreach (var nav in oldNavigators)
                Object.DestroyImmediate(nav.gameObject);

            // EventSystem (still needed for some Unity systems)
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();

            // Create MenuSystem GameObject with all menu components (OnGUI based)
            GameObject menuSystemGO = new GameObject("MenuSystem");
            menuSystemGO.AddComponent<EtherDomes.UI.MenuNavigator>();
            menuSystemGO.AddComponent<EtherDomes.UI.GameStarter>();
            menuSystemGO.AddComponent<EtherDomes.UI.MenuBackground>();
            menuSystemGO.AddComponent<EtherDomes.UI.MenuMusic>();
            menuSystemGO.AddComponent<AudioSource>(); // Required by MenuMusic
            menuSystemGO.AddComponent<EtherDomes.UI.Menus.Menu1_Principal>();
            menuSystemGO.AddComponent<EtherDomes.UI.Menus.Menu2_FormaDeJuego>();
            menuSystemGO.AddComponent<EtherDomes.UI.Menus.Menu3_CrearPartida>();
            menuSystemGO.AddComponent<EtherDomes.UI.Menus.Menu4_SeleccionPersonaje>();
            menuSystemGO.AddComponent<EtherDomes.UI.Menus.Menu5_UnirsePartida>();
            
            Debug.Log("[Setup] New OnGUI Menu System created");
            
            // Keep legacy Canvas for PauseMenu and GameSessionUI (they still use Canvas)
            canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // Above OnGUI
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            
            Debug.Log("[Setup] Menu UI created successfully (OnGUI system)");
        }
    }
}
