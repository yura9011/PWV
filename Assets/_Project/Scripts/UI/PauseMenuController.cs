using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityDebug = UnityEngine.Debug; // Alias to avoid conflict with EtherDomes.UI.Debug namespace

namespace EtherDomes.UI
{
    /// <summary>
    /// Pause menu controller - ESC to toggle, shows exit button.
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private Button _exitButton;
        
        private bool _isPaused = false;
        
        private void Start()
        {
            // Create UI if not assigned
            if (_pausePanel == null)
            {
                CreatePauseUI();
            }
            
            _pausePanel.SetActive(false);
            
            if (_exitButton != null)
            {
                _exitButton.onClick.AddListener(OnExitClicked);
            }
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }
        
        private void TogglePause()
        {
            _isPaused = !_isPaused;
            _pausePanel.SetActive(_isPaused);
        }
        
        private void OnExitClicked()
        {
            // Disconnect from network if connected
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }
            
            // Quit application
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        
        private void CreatePauseUI()
        {
            // Find or create canvas
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                UnityDebug.LogError("[PauseMenu] No Canvas found!");
                return;
            }
            
            // Create pause panel
            _pausePanel = new GameObject("PausePanel");
            _pausePanel.transform.SetParent(canvas.transform, false);
            
            var panelRect = _pausePanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            var panelImage = _pausePanel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f);
            
            // Create exit button
            GameObject buttonGO = new GameObject("ExitButton");
            buttonGO.transform.SetParent(_pausePanel.transform, false);
            
            var buttonRect = buttonGO.AddComponent<RectTransform>();
            buttonRect.anchoredPosition = Vector2.zero;
            buttonRect.sizeDelta = new Vector2(200, 60);
            
            var buttonImage = buttonGO.AddComponent<Image>();
            buttonImage.color = new Color(0.7f, 0.2f, 0.2f);
            
            _exitButton = buttonGO.AddComponent<Button>();
            var colors = _exitButton.colors;
            colors.highlightedColor = new Color(0.9f, 0.3f, 0.3f);
            colors.pressedColor = new Color(0.5f, 0.1f, 0.1f);
            _exitButton.colors = colors;
            _exitButton.onClick.AddListener(OnExitClicked);
            
            // Button text
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);
            
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var text = textGO.AddComponent<Text>();
            text.text = "SALIR";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 28;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
        }
    }
}
