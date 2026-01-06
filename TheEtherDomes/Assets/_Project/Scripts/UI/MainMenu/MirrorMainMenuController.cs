using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Mirror;
using EtherDomes.Network;
using EtherDomes.Core;
using EtherDomes.Persistence;

namespace EtherDomes.UI
{
    /// <summary>
    /// Main menu controller handling Host/Join and scene transitions for Mirror networking.
    /// </summary>
    public class MirrorMainMenuController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _joinButton;
        [SerializeField] private InputField _ipInput;
        [SerializeField] private Text _statusText;
        
        [Header("Class Selection")]
        [SerializeField] private Button _guerreroButton;
        [SerializeField] private Button _magoButton;
        [SerializeField] private Image _guerreroHighlight;
        [SerializeField] private Image _magoHighlight;
        
        [Header("Settings")]
        [SerializeField] private string _gameSceneName = "GameScene";
        [SerializeField] private bool _skipSceneLoad = true;
        
        private MirrorNetworkSessionManager _networkManager;

        private void Start()
        {
            UnityEngine.Debug.Log("[MainMenu] ========== MirrorMainMenuController START ==========");
            
            var eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            UnityEngine.Debug.Log($"[MainMenu] EventSystem found: {eventSystem != null}");
            
            _networkManager = FindFirstObjectByType<MirrorNetworkSessionManager>();
            UnityEngine.Debug.Log($"[MainMenu] NetworkManager found: {_networkManager != null}");
            
            AutoFindUIElements();
            SetupButtonListeners();
            
            if (_networkManager != null)
            {
                _networkManager.OnConnectionFailed += OnConnectionFailed;
            }
            else
            {
                UnityEngine.Debug.LogError("[MainMenu] MirrorNetworkSessionManager NOT FOUND!");
            }
            
            UpdateClassSelectionVisual();
            UpdateStatus("Selecciona tu clase y conecta");
            
            UnityEngine.Debug.Log("[MainMenu] ========== MirrorMainMenuController READY ==========");
        }

        private void AutoFindUIElements()
        {
            var allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            UnityEngine.Debug.Log($"[MainMenu] Total buttons in scene: {allButtons.Length}");
            
            foreach (var btn in allButtons)
            {
                string name = btn.gameObject.name;
                if (name == "HostButton" && _hostButton == null) _hostButton = btn;
                if (name == "JoinButton" && _joinButton == null) _joinButton = btn;
                if (name == "GuerreroButton" && _guerreroButton == null) _guerreroButton = btn;
                if (name == "MagoButton" && _magoButton == null) _magoButton = btn;
            }
            
            if (_ipInput == null) _ipInput = FindFirstObjectByType<InputField>();
            if (_statusText == null) _statusText = FindTextByName("StatusText");
        }

        private void SetupButtonListeners()
        {
            if (_hostButton != null)
            {
                _hostButton.onClick.RemoveAllListeners();
                _hostButton.onClick.AddListener(OnHostClicked);
                UnityEngine.Debug.Log("[MainMenu] Host button listener added");
            }
            
            if (_joinButton != null)
            {
                _joinButton.onClick.RemoveAllListeners();
                _joinButton.onClick.AddListener(OnJoinClicked);
                UnityEngine.Debug.Log("[MainMenu] Join button listener added");
            }
            
            if (_guerreroButton != null)
            {
                _guerreroButton.onClick.RemoveAllListeners();
                _guerreroButton.onClick.AddListener(OnGuerreroClicked);
            }
            
            if (_magoButton != null)
            {
                _magoButton.onClick.RemoveAllListeners();
                _magoButton.onClick.AddListener(OnMagoClicked);
            }
        }
        
        private Text FindTextByName(string name)
        {
            var texts = FindObjectsByType<Text>(FindObjectsSortMode.None);
            foreach (var text in texts)
            {
                if (text.gameObject.name == name)
                    return text;
            }
            return null;
        }

        private void OnDestroy()
        {
            if (_networkManager != null)
            {
                _networkManager.OnConnectionFailed -= OnConnectionFailed;
            }
        }

        private void OnHostClicked()
        {
            UnityEngine.Debug.Log("[MainMenu] HOST BUTTON CLICKED!");
            
            if (_networkManager == null)
            {
                UpdateStatus("Error: NetworkManager no encontrado");
                return;
            }
            
            UpdateStatus("Iniciando como Host...");
            _networkManager.StartAsHost();
            HideMenu();
        }

        private void OnJoinClicked()
        {
            UnityEngine.Debug.Log("[MainMenu] JOIN BUTTON CLICKED!");
            
            if (_networkManager == null)
            {
                UpdateStatus("Error: NetworkManager no encontrado");
                return;
            }
            
            string ip = _ipInput != null ? _ipInput.text : "127.0.0.1";
            if (string.IsNullOrWhiteSpace(ip)) ip = "127.0.0.1";
            
            UpdateStatus($"Conectando a {ip}...");
            _networkManager.StartAsClientWithPayload(ip);
            StartCoroutine(WaitForConnectionAndHideMenu());
        }

        private void OnGuerreroClicked()
        {
            ClassSelectionData.SelectedClass = PlayerClass.Guerrero;
            UpdateClassSelectionVisual();
            UpdateStatus("Clase: Guerrero (Rojo)");
        }

        private void OnMagoClicked()
        {
            ClassSelectionData.SelectedClass = PlayerClass.Mago;
            UpdateClassSelectionVisual();
            UpdateStatus("Clase: Mago (Azul)");
        }

        private void HideMenu()
        {
            var panel = GameObject.Find("MainMenuPanel");
            if (panel != null)
            {
                panel.SetActive(false);
                UnityEngine.Debug.Log("[MainMenu] Menu hidden");
            }
        }
        
        private void ShowMenu()
        {
            var panel = GameObject.Find("MainMenuPanel");
            if (panel != null)
            {
                panel.SetActive(true);
                UnityEngine.Debug.Log("[MainMenu] Menu shown");
            }
        }

        private System.Collections.IEnumerator WaitForConnectionAndHideMenu()
        {
            float timeout = 10f;
            float elapsed = 0f;
            
            while (!Mirror.NetworkClient.isConnected && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            if (Mirror.NetworkClient.isConnected)
                HideMenu();
            else
            {
                UpdateStatus("Conexión fallida - timeout");
                ShowMenu();
            }
        }

        private void UpdateClassSelectionVisual()
        {
            bool isGuerrero = ClassSelectionData.SelectedClass == PlayerClass.Guerrero;
            
            if (_guerreroHighlight != null) _guerreroHighlight.enabled = isGuerrero;
            if (_magoHighlight != null) _magoHighlight.enabled = !isGuerrero;
                
            if (_guerreroButton != null)
            {
                var colors = _guerreroButton.colors;
                colors.normalColor = isGuerrero ? Color.white : new Color(0.7f, 0.7f, 0.7f);
                _guerreroButton.colors = colors;
            }
            
            if (_magoButton != null)
            {
                var colors = _magoButton.colors;
                colors.normalColor = !isGuerrero ? Color.white : new Color(0.7f, 0.7f, 0.7f);
                _magoButton.colors = colors;
            }
        }

        private void OnConnectionFailed(string reason)
        {
            UpdateStatus($"Error: {reason}");
        }

        private void UpdateStatus(string message)
        {
            UnityEngine.Debug.Log($"[MainMenu] {message}");
            if (_statusText != null) _statusText.text = message;
        }
    }
}
