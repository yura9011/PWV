using EtherDomes.Network;
using UnityEngine;
using UnityEngine.UI;

namespace EtherDomes.UI
{
    /// <summary>
    /// Main menu UI controller for Host/Client/Server selection.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Connection Panel")]
        [SerializeField] private GameObject _connectionPanel;
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _clientButton;
        [SerializeField] private Button _serverButton;
        [SerializeField] private TMPro.TMP_InputField _ipAddressInput;
        [SerializeField] private TMPro.TMP_InputField _portInput;

        [Header("Character Panel")]
        [SerializeField] private GameObject _characterPanel;
        [SerializeField] private TMPro.TMP_InputField _characterNameInput;
        [SerializeField] private Button _createCharacterButton;

        [Header("Status")]
        [SerializeField] private TMPro.TextMeshProUGUI _statusText;
        [SerializeField] private GameObject _loadingIndicator;

        private INetworkSessionManager _networkManager;
        private string _defaultIP = "127.0.0.1";
        private ushort _defaultPort = 7777;

        private void Start()
        {
            // Find network manager
            _networkManager = FindFirstObjectByType<NetworkSessionManager>();

            SetupButtons();
            SetupDefaults();
            ShowConnectionPanel();
        }

        private void SetupButtons()
        {
            if (_hostButton != null)
                _hostButton.onClick.AddListener(OnHostClicked);

            if (_clientButton != null)
                _clientButton.onClick.AddListener(OnClientClicked);

            if (_serverButton != null)
                _serverButton.onClick.AddListener(OnServerClicked);

            if (_createCharacterButton != null)
                _createCharacterButton.onClick.AddListener(OnCreateCharacterClicked);
        }

        private void SetupDefaults()
        {
            if (_ipAddressInput != null)
                _ipAddressInput.text = _defaultIP;

            if (_portInput != null)
                _portInput.text = _defaultPort.ToString();
        }

        private void ShowConnectionPanel()
        {
            if (_connectionPanel != null)
                _connectionPanel.SetActive(true);

            if (_characterPanel != null)
                _characterPanel.SetActive(false);

            if (_loadingIndicator != null)
                _loadingIndicator.SetActive(false);
        }

        private void ShowCharacterPanel()
        {
            if (_connectionPanel != null)
                _connectionPanel.SetActive(false);

            if (_characterPanel != null)
                _characterPanel.SetActive(true);
        }

        private void OnHostClicked()
        {
            if (_networkManager == null)
            {
                SetStatus("Error: Network Manager not found");
                return;
            }

            ushort port = GetPort();
            SetStatus($"Starting Host on port {port}...");
            ShowLoading(true);

            _networkManager.StartAsHost(port);
            SetStatus("Host started successfully");
            ShowLoading(false);
        }

        private void OnClientClicked()
        {
            if (_networkManager == null)
            {
                SetStatus("Error: Network Manager not found");
                return;
            }

            string ip = GetIPAddress();
            ushort port = GetPort();
            SetStatus($"Connecting to {ip}:{port}...");
            ShowLoading(true);

            _networkManager.OnConnectionFailed += OnConnectionFailed;
            _networkManager.StartAsClient(ip, port);
        }

        private void OnServerClicked()
        {
            if (_networkManager == null)
            {
                SetStatus("Error: Network Manager not found");
                return;
            }

            ushort port = GetPort();
            SetStatus($"Starting Dedicated Server on port {port}...");
            ShowLoading(true);

            _networkManager.StartAsDedicatedServer(port);
            SetStatus("Dedicated Server started");
            ShowLoading(false);
        }

        private void OnCreateCharacterClicked()
        {
            string characterName = _characterNameInput?.text ?? "Player";
            if (string.IsNullOrWhiteSpace(characterName))
            {
                SetStatus("Please enter a character name");
                return;
            }

            SetStatus($"Creating character: {characterName}");
            // Character creation logic would go here
        }

        private void OnConnectionFailed(string reason)
        {
            SetStatus($"Connection failed: {reason}");
            ShowLoading(false);
            _networkManager.OnConnectionFailed -= OnConnectionFailed;
        }

        private string GetIPAddress()
        {
            return _ipAddressInput?.text ?? _defaultIP;
        }

        private ushort GetPort()
        {
            if (_portInput != null && ushort.TryParse(_portInput.text, out ushort port))
                return port;
            return _defaultPort;
        }

        private void SetStatus(string message)
        {
            if (_statusText != null)
                _statusText.text = message;
            UnityEngine.Debug.Log($"[MainMenu] {message}");
        }

        private void ShowLoading(bool show)
        {
            if (_loadingIndicator != null)
                _loadingIndicator.SetActive(show);
        }
    }
}
