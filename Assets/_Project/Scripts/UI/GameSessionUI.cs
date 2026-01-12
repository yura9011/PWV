using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using EtherDomes.Network;

namespace EtherDomes.UI
{
    /// <summary>
    /// In-game UI showing session info (Relay code) in bottom-left corner.
    /// </summary>
    public class GameSessionUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _sessionPanel;
        [SerializeField] private Text _relayCodeText;
        
        private bool _isInGame = false;
        
        private void Start()
        {
            if (_sessionPanel == null)
            {
                CreateSessionUI();
            }
            
            _sessionPanel.SetActive(false);
        }
        
        private void Update()
        {
            // Show panel only for Host (not for clients)
            bool shouldShow = NetworkManager.Singleton != null && 
                              NetworkManager.Singleton.IsListening &&
                              NetworkManager.Singleton.IsHost; // Solo Host ve el código
            
            if (shouldShow != _isInGame)
            {
                _isInGame = shouldShow;
                _sessionPanel.SetActive(_isInGame);
                
                if (_isInGame)
                {
                    UpdateRelayCode();
                }
            }
        }
        
        private void UpdateRelayCode()
        {
            string code = "---";
            
            // Try to get relay code from RelayManager
            if (RelayManager.Instance != null)
            {
                string relayCode = RelayManager.Instance.JoinCode;
                if (!string.IsNullOrEmpty(relayCode))
                {
                    code = relayCode;
                }
            }
            
            if (_relayCodeText != null)
            {
                _relayCodeText.text = $"Código: {code}";
            }
        }
        
        private void CreateSessionUI()
        {
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;
            
            // Create panel in bottom-left
            _sessionPanel = new GameObject("SessionPanel");
            _sessionPanel.transform.SetParent(canvas.transform, false);
            
            var panelRect = _sessionPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(0, 0);
            panelRect.pivot = new Vector2(0, 0);
            panelRect.anchoredPosition = new Vector2(10, 10);
            panelRect.sizeDelta = new Vector2(200, 40);
            
            var panelImage = _sessionPanel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.5f);
            
            // Create text
            GameObject textGO = new GameObject("RelayCodeText");
            textGO.transform.SetParent(_sessionPanel.transform, false);
            
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, -5);
            
            _relayCodeText = textGO.AddComponent<Text>();
            _relayCodeText.text = "Código: ---";
            _relayCodeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _relayCodeText.fontSize = 16;
            _relayCodeText.alignment = TextAnchor.MiddleLeft;
            _relayCodeText.color = Color.white;
        }
    }
}
