using UnityEngine;
using UnityEngine.UI;

namespace EtherDomes.Network
{
    /// <summary>
    /// Simple UI controller for network connection testing.
    /// Connects buttons to NetworkSessionManager.
    /// </summary>
    public class ConnectionUIController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _clientButton;
        [SerializeField] private Button _serverButton;
        [SerializeField] private InputField _ipInput;
        [SerializeField] private Text _statusText;

        [Header("Network")]
        [SerializeField] private NetworkSessionManager _networkManager;

        private void Start()
        {
            // Find NetworkManager if not assigned
            if (_networkManager == null)
            {
                _networkManager = FindFirstObjectByType<NetworkSessionManager>();
            }

            // Auto-find UI elements if not assigned - search in entire scene
            if (_hostButton == null)
                _hostButton = FindButtonByName("HostButton");
            if (_clientButton == null)
                _clientButton = FindButtonByName("ClientButton");
            if (_serverButton == null)
                _serverButton = FindButtonByName("ServerButton");
            if (_ipInput == null)
                _ipInput = FindFirstObjectByType<InputField>();

            Debug.Log($"[ConnectionUI] Found buttons - Host: {_hostButton != null}, Client: {_clientButton != null}, Server: {_serverButton != null}, IP: {_ipInput != null}");
            Debug.Log($"[ConnectionUI] NetworkManager: {_networkManager != null}");

            // Setup button listeners
            if (_hostButton != null)
                _hostButton.onClick.AddListener(OnHostClicked);
            else
                Debug.LogError("[ConnectionUI] HostButton not found!");
                
            if (_clientButton != null)
                _clientButton.onClick.AddListener(OnClientClicked);
            else
                Debug.LogError("[ConnectionUI] ClientButton not found!");
                
            if (_serverButton != null)
                _serverButton.onClick.AddListener(OnServerClicked);
            else
                Debug.LogError("[ConnectionUI] ServerButton not found!");

            // Subscribe to events
            if (_networkManager != null)
            {
                _networkManager.OnConnectionFailed += OnConnectionFailed;
                _networkManager.OnPlayerConnected += (conn) => UpdateStatus($"Player connected: {conn.connectionId}");
                _networkManager.OnPlayerDisconnected += (conn) => UpdateStatus($"Player disconnected: {conn.connectionId}");
            }
        }

        private void OnDestroy()
        {
            if (_networkManager != null)
            {
                _networkManager.OnConnectionFailed -= OnConnectionFailed;
            }
        }

        private void Update()
        {
            // Update UI visibility based on connection state
            bool isConnected = _networkManager != null && 
                (_networkManager.IsHost || _networkManager.IsClient || _networkManager.IsServer);

            if (_hostButton != null)
                _hostButton.gameObject.SetActive(!isConnected);
            if (_clientButton != null)
                _clientButton.gameObject.SetActive(!isConnected);
            if (_serverButton != null)
                _serverButton.gameObject.SetActive(!isConnected);
            if (_ipInput != null)
                _ipInput.gameObject.SetActive(!isConnected);
        }

        private void OnHostClicked()
        {
            if (_networkManager != null)
            {
                _networkManager.StartAsHost();
                UpdateStatus("Started as Host");
            }
        }

        private void OnClientClicked()
        {
            if (_networkManager != null)
            {
                string ip = _ipInput != null ? _ipInput.text : "127.0.0.1";
                _networkManager.StartAsClient(ip);
                UpdateStatus($"Connecting to {ip}...");
            }
        }

        private void OnServerClicked()
        {
            if (_networkManager != null)
            {
                _networkManager.StartAsDedicatedServer();
                UpdateStatus("Started as Dedicated Server");
            }
        }

        private void OnConnectionFailed(string reason)
        {
            UpdateStatus($"Connection failed: {reason}");
        }

        private void UpdateStatus(string message)
        {
            Debug.Log($"[ConnectionUI] {message}");
            if (_statusText != null)
            {
                _statusText.text = message;
            }
        }

        private Button FindButtonByName(string name)
        {
            var buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            foreach (var button in buttons)
            {
                if (button.gameObject.name == name)
                    return button;
            }
            return null;
        }
    }
}
