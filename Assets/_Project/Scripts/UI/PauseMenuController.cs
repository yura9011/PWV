using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityDebug = UnityEngine.Debug;

namespace EtherDomes.UI
{
    /// <summary>
    /// Pause menu controller - ESC to toggle, shows menu and exit buttons.
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private Button _menuButton;
        [SerializeField] private Button _exitButton;
        
        private bool _isPaused = false;
        
        private void Start()
        {
            if (_pausePanel == null)
            {
                CreatePauseUI();
            }
            
            _pausePanel.SetActive(false);
        }
        
        private void Update()
        {
            // Solo mostrar menú de pausa si estamos en el juego (no en menús)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (MenuNavigator.Instance != null && MenuNavigator.Instance.CurrentMenu == MenuType.None)
                {
                    TogglePause();
                }
            }
        }
        
        private void TogglePause()
        {
            _isPaused = !_isPaused;
            _pausePanel.SetActive(_isPaused);
        }
        
        private void OnMenuClicked()
        {
            _isPaused = false;
            _pausePanel.SetActive(false);
            
            // Volver al menú principal
            if (GameStarter.Instance != null)
            {
                GameStarter.Instance.ReturnToMainMenu();
            }
        }
        
        private void OnExitClicked()
        {
            // Disconnect from network if connected
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        
        private void CreatePauseUI()
        {
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
            
            // Create container for buttons
            GameObject container = new GameObject("ButtonContainer");
            container.transform.SetParent(_pausePanel.transform, false);
            
            var containerRect = container.AddComponent<RectTransform>();
            containerRect.anchoredPosition = Vector2.zero;
            containerRect.sizeDelta = new Vector2(220, 150);
            
            // Create "Volver al Menú" button
            _menuButton = CreateButton(container.transform, "MenuButton", "VOLVER AL MENÚ", 
                new Vector2(0, 35), new Color(0.3f, 0.5f, 0.7f));
            _menuButton.onClick.AddListener(OnMenuClicked);
            
            // Create exit button
            _exitButton = CreateButton(container.transform, "ExitButton", "SALIR DEL JUEGO", 
                new Vector2(0, -35), new Color(0.7f, 0.2f, 0.2f));
            _exitButton.onClick.AddListener(OnExitClicked);
            
            // Title
            GameObject titleGO = new GameObject("Title");
            titleGO.transform.SetParent(_pausePanel.transform, false);
            
            var titleRect = titleGO.AddComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 120);
            titleRect.sizeDelta = new Vector2(300, 50);
            
            var titleText = titleGO.AddComponent<Text>();
            titleText.text = "PAUSA";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 36;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;
        }
        
        private Button CreateButton(Transform parent, string name, string label, Vector2 position, Color color)
        {
            GameObject buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent, false);
            
            var buttonRect = buttonGO.AddComponent<RectTransform>();
            buttonRect.anchoredPosition = position;
            buttonRect.sizeDelta = new Vector2(200, 50);
            
            var buttonImage = buttonGO.AddComponent<Image>();
            buttonImage.color = color;
            
            var button = buttonGO.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = new Color(color.r + 0.2f, color.g + 0.2f, color.b + 0.2f);
            colors.pressedColor = new Color(color.r - 0.2f, color.g - 0.2f, color.b - 0.2f);
            button.colors = colors;
            
            // Button text
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);
            
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var text = textGO.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 18;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            
            return button;
        }
    }
}
