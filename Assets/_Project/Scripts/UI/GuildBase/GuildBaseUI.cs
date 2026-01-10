using EtherDomes.World;
using UnityEngine;
using UnityEngine.UI;

namespace EtherDomes.UI
{
    /// <summary>
    /// UI controller for the Guild Base scene.
    /// Handles furniture placement and trophy display.
    /// </summary>
    public class GuildBaseUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject _furniturePanel;
        [SerializeField] private GameObject _trophyPanel;

        [Header("Furniture")]
        [SerializeField] private Button _placeFurnitureButton;
        [SerializeField] private Button _removeFurnitureButton;
        [SerializeField] private Transform _furnitureListContent;

        [Header("Trophies")]
        [SerializeField] private Transform _trophyListContent;
        [SerializeField] private GameObject _trophyItemPrefab;

        [Header("Navigation")]
        [SerializeField] private Button _exitButton;

        private IGuildBaseSystem _guildBaseSystem;
        private bool _isPlacingFurniture;

        private void Start()
        {
            _guildBaseSystem = FindFirstObjectByType<GuildBaseSystem>();
            SetupButtons();
            RefreshTrophies();
        }

        private void SetupButtons()
        {
            if (_placeFurnitureButton != null)
                _placeFurnitureButton.onClick.AddListener(OnPlaceFurnitureClicked);

            if (_removeFurnitureButton != null)
                _removeFurnitureButton.onClick.AddListener(OnRemoveFurnitureClicked);

            if (_exitButton != null)
                _exitButton.onClick.AddListener(OnExitClicked);
        }

        private void OnPlaceFurnitureClicked()
        {
            _isPlacingFurniture = true;
            UnityEngine.Debug.Log("[GuildBaseUI] Furniture placement mode enabled");
        }

        private void OnRemoveFurnitureClicked()
        {
            _isPlacingFurniture = false;
            UnityEngine.Debug.Log("[GuildBaseUI] Furniture removal mode enabled");
        }

        private void OnExitClicked()
        {
            if (_guildBaseSystem != null)
            {
                // Get local player ID (would come from NetworkManager)
                ulong localPlayerId = 0;
                _guildBaseSystem.Leave(localPlayerId);
            }

            // Load previous scene or main menu
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        private void RefreshTrophies()
        {
            if (_guildBaseSystem == null || _trophyListContent == null) return;

            var state = _guildBaseSystem.GetState();
            if (state?.UnlockedTrophies == null) return;

            // Clear existing
            foreach (Transform child in _trophyListContent)
            {
                Destroy(child.gameObject);
            }

            // Add trophy items
            foreach (var bossId in state.UnlockedTrophies)
            {
                if (_trophyItemPrefab != null)
                {
                    var item = Instantiate(_trophyItemPrefab, _trophyListContent);
                    var text = item.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (text != null)
                        text.text = bossId;
                }
            }
        }

        private void Update()
        {
            if (_isPlacingFurniture && Input.GetMouseButtonDown(0))
            {
                TryPlaceFurniture();
            }
        }

        private void TryPlaceFurniture()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                // Create test furniture
                var furniture = new FurnitureData
                {
                    FurnitureId = "test_furniture",
                    FurnitureName = "Test Furniture"
                };

                _guildBaseSystem?.PlaceFurniture(furniture, hit.point, Quaternion.identity);
                _isPlacingFurniture = false;
            }
        }
    }
}
