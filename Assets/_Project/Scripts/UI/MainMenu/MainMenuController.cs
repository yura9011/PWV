using UnityEngine;
using UnityEngine.UI;
using EtherDomes.Network;
using EtherDomes.Core;
using Unity.Netcode;

namespace EtherDomes.UI
{
    /// <summary>
    /// Main menu controller with multi-panel navigation flow:
    /// 1. Main Menu: Iniciar | Salir
    /// 2. Mode Menu: Crear | Unirse | Atrás
    /// 3. Create Menu: Iniciar Servidor | Servidor Dedicado | Atrás
    /// 4. Host Info Menu: Shows Relay Code + IP | Aceptar | Atrás
    /// 5. Class Menu: Guerrero | Mago | Aceptar
    /// 6. Join Menu: Input (IP/Code) + Relay Toggle | Aceptar | Atrás
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Background")]
        [SerializeField] private Image _backgroundImage;
        
        [Header("Panels")]
        [SerializeField] private GameObject _mainMenuPanel;
        [SerializeField] private GameObject _modeMenuPanel;
        [SerializeField] private GameObject _createMenuPanel;
        [SerializeField] private GameObject _hostInfoPanel;
        [SerializeField] private GameObject _joinMenuPanel;
        [SerializeField] private GameObject _classMenuPanel;

        [Header("Main Menu")]
        [SerializeField] private Button _iniciarButton;
        [SerializeField] private Button _salirButton;

        [Header("Mode Menu")]
        [SerializeField] private Button _crearButton;
        [SerializeField] private Button _unirseButton;
        [SerializeField] private Button _modeBackButton;

        [Header("Create Menu")]
        [SerializeField] private Button _iniciarServidorButton;
        [SerializeField] private Button _servidorDedicadoButton;
        [SerializeField] private Button _createBackButton;

        [Header("Host Info Menu")]
        [SerializeField] private Text _relayCodeText;
        [SerializeField] private Text _ipAddressText;
        [SerializeField] private Button _hostInfoAcceptButton;
        [SerializeField] private Button _hostInfoBackButton;

        [Header("Join Menu")]
        [SerializeField] private InputField _connectionInput;
        [SerializeField] private Text _connectionInputLabel;
        [SerializeField] private Toggle _useRelayToggle;
        [SerializeField] private Button _joinAcceptButton;
        [SerializeField] private Button _joinBackButton;

        [Header("Class Menu")]
        [SerializeField] private Button _guerreroButton;
        [SerializeField] private Button _magoButton;
        [SerializeField] private Button _classAcceptButton;

        [Header("Status")]
        [SerializeField] private Text _statusText;

        private NetworkSessionManager _networkManager;
        private PlayerClass? _selectedClass = null;
        private bool _isHost = false;
        private bool _isConnecting = false;
        private string _currentRelayCode = "";
        private string _currentIP = "";

        private void Start()
        {
            _networkManager = FindFirstObjectByType<NetworkSessionManager>();
            
            if (_networkManager != null)
            {
                _networkManager.OnConnectionFailed += OnConnectionFailed;
                _networkManager.OnPlayerConnected += OnPlayerConnected;
            }

            // Setup background wallpaper if not assigned
            SetupBackgroundWallpaper();
            
            SetupButtonListeners();
            ShowPanel(MenuPanel.Main);
            UpdateStatus("Bienvenido a The Ether Domes");
        }

        private void OnDestroy()
        {
            if (_networkManager != null)
            {
                _networkManager.OnConnectionFailed -= OnConnectionFailed;
                _networkManager.OnPlayerConnected -= OnPlayerConnected;
            }
        }

        private void SetupButtonListeners()
        {
            // Main Menu
            if (_iniciarButton != null) _iniciarButton.onClick.AddListener(OnIniciarClicked);
            if (_salirButton != null) _salirButton.onClick.AddListener(OnSalirClicked);

            // Mode Menu
            if (_crearButton != null) _crearButton.onClick.AddListener(OnCrearClicked);
            if (_unirseButton != null) _unirseButton.onClick.AddListener(OnUnirseClicked);
            if (_modeBackButton != null) _modeBackButton.onClick.AddListener(OnModeBackClicked);

            // Create Menu
            if (_iniciarServidorButton != null) _iniciarServidorButton.onClick.AddListener(OnIniciarServidorClicked);
            if (_servidorDedicadoButton != null) _servidorDedicadoButton.onClick.AddListener(OnServidorDedicadoClicked);
            if (_createBackButton != null) _createBackButton.onClick.AddListener(OnCreateBackClicked);

            // Host Info Menu
            if (_hostInfoAcceptButton != null) _hostInfoAcceptButton.onClick.AddListener(OnHostInfoAcceptClicked);
            if (_hostInfoBackButton != null) _hostInfoBackButton.onClick.AddListener(OnHostInfoBackClicked);

            // Join Menu
            if (_joinAcceptButton != null) _joinAcceptButton.onClick.AddListener(OnJoinAcceptClicked);
            if (_joinBackButton != null) _joinBackButton.onClick.AddListener(OnJoinBackClicked);
            if (_useRelayToggle != null) _useRelayToggle.onValueChanged.AddListener(OnRelayToggleChanged);

            // Class Menu
            if (_guerreroButton != null) _guerreroButton.onClick.AddListener(OnGuerreroClicked);
            if (_magoButton != null) _magoButton.onClick.AddListener(OnMagoClicked);
            if (_classAcceptButton != null) _classAcceptButton.onClick.AddListener(OnClassAcceptClicked);
        }

        #region Panel Navigation

        private enum MenuPanel { Main, Mode, Create, HostInfo, Join, Class, Hidden }

        private void ShowPanel(MenuPanel panel)
        {
            if (_mainMenuPanel != null) _mainMenuPanel.SetActive(panel == MenuPanel.Main);
            if (_modeMenuPanel != null) _modeMenuPanel.SetActive(panel == MenuPanel.Mode);
            if (_createMenuPanel != null) _createMenuPanel.SetActive(panel == MenuPanel.Create);
            if (_hostInfoPanel != null) _hostInfoPanel.SetActive(panel == MenuPanel.HostInfo);
            if (_joinMenuPanel != null) _joinMenuPanel.SetActive(panel == MenuPanel.Join);
            if (_classMenuPanel != null) _classMenuPanel.SetActive(panel == MenuPanel.Class);
        }

        private void HideAllPanels()
        {
            ShowPanel(MenuPanel.Hidden);
        }

        #endregion

        #region Main Menu

        private void OnIniciarClicked()
        {
            ShowPanel(MenuPanel.Mode);
            UpdateStatus("Selecciona modo de juego");
        }

        private void OnSalirClicked()
        {
            UnityEngine.Debug.Log("[MainMenu] Saliendo del juego...");
            Application.Quit();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }

        #endregion

        #region Mode Menu

        private void OnCrearClicked()
        {
            _isHost = true;
            ShowPanel(MenuPanel.Create);
            UpdateStatus("Selecciona tipo de servidor");
        }

        private void OnUnirseClicked()
        {
            _isHost = false;
            OnRelayToggleChanged(_useRelayToggle != null && _useRelayToggle.isOn);
            ShowPanel(MenuPanel.Join);
            UpdateStatus("Ingresa los datos de conexión");
        }

        private void OnModeBackClicked()
        {
            ShowPanel(MenuPanel.Main);
            UpdateStatus("Bienvenido a The Ether Domes");
        }

        #endregion

        #region Create Menu

        private async void OnIniciarServidorClicked()
        {
            if (_networkManager == null || _isConnecting) return;
            _isConnecting = true;

            UpdateStatus("Iniciando servidor...");

            // Try Relay first
            bool relaySuccess = false;
            if (RelayManager.Instance != null)
            {
                string joinCode = await _networkManager.StartHostWithRelay();
                if (!string.IsNullOrEmpty(joinCode))
                {
                    _currentRelayCode = joinCode;
                    _currentIP = GetLocalIPAddress();
                    relaySuccess = true;
                }
            }

            if (!relaySuccess)
            {
                // Relay failed or not available, try direct with different ports
                UpdateStatus("Relay no disponible, iniciando directo...");
                
                ushort[] portsToTry = { 7777, 7778, 7779, 7780, 0 }; // 0 = OS assigns free port
                bool success = false;
                ushort usedPort = 7777;

                foreach (var port in portsToTry)
                {
                    success = _networkManager.StartAsHost(port);
                    if (success)
                    {
                        usedPort = port == 0 ? (ushort)7777 : port; // If OS assigned, we don't know the port
                        break;
                    }
                }
                
                if (!success)
                {
                    UpdateStatus("Error: No se pudo iniciar servidor (puertos ocupados)");
                    _isConnecting = false;
                    return;
                }
                
                _currentRelayCode = "No disponible";
                _currentIP = $"{GetLocalIPAddress()}:{usedPort}";
            }

            // Update Host Info panel
            if (_relayCodeText != null) _relayCodeText.text = $"Código Relay: {_currentRelayCode}";
            if (_ipAddressText != null) _ipAddressText.text = $"IP Local: {_currentIP}";

            ShowPanel(MenuPanel.HostInfo);
            UpdateStatus("Servidor iniciado!");
            _isConnecting = false;
        }

        private void OnServidorDedicadoClicked()
        {
            UpdateStatus("Servidor Dedicado: En construcción");
        }

        private void OnCreateBackClicked()
        {
            ShowPanel(MenuPanel.Mode);
            UpdateStatus("Selecciona modo de juego");
        }

        #endregion

        #region Host Info Menu

        private void OnHostInfoAcceptClicked()
        {
            _selectedClass = null;
            UpdateClassButtons();
            ShowPanel(MenuPanel.Class);
            UpdateStatus("Selecciona tu clase");
        }

        private void OnHostInfoBackClicked()
        {
            // Disconnect and go back
            _networkManager?.Disconnect();
            _currentRelayCode = "";
            _currentIP = "";
            ShowPanel(MenuPanel.Create);
            UpdateStatus("Selecciona tipo de servidor");
        }

        #endregion

        #region Join Menu

        private void OnRelayToggleChanged(bool useRelay)
        {
            if (_connectionInputLabel != null)
            {
                _connectionInputLabel.text = useRelay ? "Ingresa código:" : "Ingresa IP:";
            }
            if (_connectionInput != null)
            {
                _connectionInput.text = useRelay ? "" : "127.0.0.1";
            }
        }

        private async void OnJoinAcceptClicked()
        {
            if (_networkManager == null || _isConnecting) return;
            
            string input = _connectionInput != null ? _connectionInput.text.Trim() : "";
            if (string.IsNullOrEmpty(input))
            {
                UpdateStatus("Error: Ingresa un valor válido");
                return;
            }

            _isConnecting = true;
            bool useRelay = _useRelayToggle != null && _useRelayToggle.isOn;

            // Start connection with timeout
            float timeout = 10f;
            float elapsed = 0f;
            bool connected = false;

            if (useRelay)
            {
                UpdateStatus($"Conectando con código: {input.ToUpper()}...");
                var connectTask = _networkManager.StartClientWithRelay(input.ToUpper(), ""); // Sin contraseña
                
                while (!connectTask.IsCompleted && elapsed < timeout)
                {
                    await System.Threading.Tasks.Task.Delay(100);
                    elapsed += 0.1f;
                    
                    // Check if connected
                    if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
                    {
                        connected = true;
                        break;
                    }
                }
                
                if (!connected && connectTask.IsCompleted)
                {
                    connected = connectTask.Result;
                }
            }
            else
            {
                UpdateStatus($"Conectando a {input}:7777...");
                _networkManager.StartAsClient(input, 7777);
                
                // Wait for connection with timeout
                while (elapsed < timeout)
                {
                    await System.Threading.Tasks.Task.Delay(100);
                    elapsed += 0.1f;
                    
                    if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
                    {
                        connected = true;
                        break;
                    }
                }
            }

            if (!connected)
            {
                // Timeout - disconnect and show error
                _networkManager.Disconnect();
                UpdateStatus("No se pudo conectar (timeout)");
                _isConnecting = false;
            }
            // If connected, OnPlayerConnected will handle the rest
        }

        private void OnJoinBackClicked()
        {
            ShowPanel(MenuPanel.Mode);
            UpdateStatus("Selecciona modo de juego");
        }

        #endregion

        #region Class Menu

        private void OnGuerreroClicked()
        {
            _selectedClass = PlayerClass.Guerrero;
            ClassSelectionData.SelectedClass = PlayerClass.Guerrero;
            UpdateClassButtons();
            UpdateStatus("Clase seleccionada: Guerrero");
        }

        private void OnMagoClicked()
        {
            _selectedClass = PlayerClass.Mago;
            ClassSelectionData.SelectedClass = PlayerClass.Mago;
            UpdateClassButtons();
            UpdateStatus("Clase seleccionada: Mago");
        }

        private void UpdateClassButtons()
        {
            if (_guerreroButton != null)
            {
                var colors = _guerreroButton.colors;
                colors.normalColor = _selectedClass == PlayerClass.Guerrero 
                    ? new Color(1f, 0.3f, 0.3f) 
                    : new Color(0.6f, 0.2f, 0.2f);
                _guerreroButton.colors = colors;
            }

            if (_magoButton != null)
            {
                var colors = _magoButton.colors;
                colors.normalColor = _selectedClass == PlayerClass.Mago 
                    ? new Color(0.3f, 0.5f, 1f) 
                    : new Color(0.2f, 0.3f, 0.6f);
                _magoButton.colors = colors;
            }
        }

        private void OnClassAcceptClicked()
        {
            if (_selectedClass == null)
            {
                UpdateStatus("Error: Selecciona una clase primero");
                return;
            }

            // Hide all UI including status and background
            HideAllPanels();
            if (_statusText != null) _statusText.gameObject.SetActive(false);
            if (_backgroundImage != null) _backgroundImage.gameObject.SetActive(false);
            
            // Apply color to local player based on class
            ApplyClassColorToPlayer();
        }

        private void ApplyClassColorToPlayer()
        {
            // Find local player, show it and change color
            var players = Object.FindObjectsByType<NetworkBehaviour>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                if (player.IsOwner && player.gameObject.name.Contains("Player"))
                {
                    var renderer = player.GetComponentInChildren<MeshRenderer>();
                    if (renderer != null)
                    {
                        // Show the player
                        renderer.enabled = true;
                        
                        // Apply class color
                        Color classColor = _selectedClass == PlayerClass.Guerrero 
                            ? new Color(0.8f, 0.2f, 0.2f) // Rojo
                            : new Color(0.2f, 0.4f, 0.8f); // Azul
                        
                        renderer.material.color = classColor;
                        UnityEngine.Debug.Log($"[MainMenu] Jugador visible con color: {_selectedClass}");
                    }
                    break;
                }
            }
        }

        #endregion

        #region Network Callbacks

        private void OnPlayerConnected(ulong clientId)
        {
            if (NetworkManager.Singleton == null) return;
            
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                // Hide player until class is selected
                HideLocalPlayer();
                
                if (!_isHost)
                {
                    // Client connected, show class selection
                    _selectedClass = null;
                    UpdateClassButtons();
                    ShowPanel(MenuPanel.Class);
                    UpdateStatus("Conectado! Selecciona tu clase");
                }
                _isConnecting = false;
            }
        }

        private void HideLocalPlayer()
        {
            // Find and hide local player until class is selected
            StartCoroutine(HideLocalPlayerCoroutine());
        }

        private System.Collections.IEnumerator HideLocalPlayerCoroutine()
        {
            // Wait a frame for player to spawn
            yield return null;
            yield return null;
            
            var players = Object.FindObjectsByType<NetworkBehaviour>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                if (player.IsOwner && player.gameObject.name.Contains("Player"))
                {
                    var renderer = player.GetComponentInChildren<MeshRenderer>();
                    if (renderer != null)
                    {
                        renderer.enabled = false;
                    }
                    break;
                }
            }
        }

        private void OnConnectionFailed(string reason)
        {
            _isConnecting = false;
            UpdateStatus($"Error: {reason}");
            
            if (_isHost)
                ShowPanel(MenuPanel.Create);
            else
                ShowPanel(MenuPanel.Join);
        }

        #endregion

        #region Utilities

        private void SetupBackgroundWallpaper()
        {
            // If already assigned, just ensure it's at the back
            if (_backgroundImage != null)
            {
                _backgroundImage.transform.SetAsFirstSibling();
                return;
            }
            
            // Try to load wallpaper from Resources
            var sprite = Resources.Load<Sprite>("Wallpapers/Coralwallpaper");
            if (sprite == null)
            {
                // Try loading as Texture2D and convert to sprite
                var texture = Resources.Load<Texture2D>("Wallpapers/Coralwallpaper");
                if (texture != null)
                {
                    sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
            }
            
            if (sprite == null)
            {
                UnityEngine.Debug.LogWarning("[MainMenu] No se encontró Coralwallpaper en Resources/Wallpapers/");
                return;
            }
            
            // Create background image as first child of canvas (behind everything)
            var canvas = GetComponent<Canvas>();
            if (canvas == null) canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;
            
            GameObject bgGO = new GameObject("BackgroundWallpaper");
            bgGO.transform.SetParent(canvas.transform, false);
            bgGO.transform.SetAsFirstSibling(); // Put at back
            
            var rect = bgGO.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            _backgroundImage = bgGO.AddComponent<Image>();
            _backgroundImage.sprite = sprite;
            _backgroundImage.preserveAspect = false; // Stretch to fill
            _backgroundImage.raycastTarget = false; // Don't block clicks
            
            UnityEngine.Debug.Log("[MainMenu] Background wallpaper configurado");
        }

        private void UpdateStatus(string message)
        {
            UnityEngine.Debug.Log($"[MainMenu] {message}");
            if (_statusText != null) _statusText.text = message;
        }

        private string GetLocalIPAddress()
        {
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch { }
            return "127.0.0.1";
        }

        #endregion
    }
}
