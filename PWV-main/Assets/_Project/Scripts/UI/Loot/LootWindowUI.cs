using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EtherDomes.Data;
using EtherDomes.Progression;

namespace EtherDomes.UI
{
    /// <summary>
    /// UI for displaying loot rolls with Need/Greed/Pass buttons.
    /// 
    /// Features:
    /// - Item display with name, icon, rarity color
    /// - Need, Greed, Pass buttons
    /// - 30 second countdown timer
    /// - Auto-pass on timeout
    /// - Roll status display
    /// 
    /// Requirements: 15.1, 15.2, 15.3, 15.4, 15.5, 15.6
    /// </summary>
    public class LootWindowUI : MonoBehaviour, ILootWindowUI
    {
        [Header("UI References")]
        [SerializeField] private GameObject _windowPanel;
        [SerializeField] private Image _itemIcon;
        [SerializeField] private TextMeshProUGUI _itemNameText;
        [SerializeField] private TextMeshProUGUI _itemLevelText;
        [SerializeField] private TextMeshProUGUI _countdownText;
        [SerializeField] private Button _needButton;
        [SerializeField] private Button _greedButton;
        [SerializeField] private Button _passButton;
        [SerializeField] private Transform _rollStatusContainer;
        [SerializeField] private GameObject _rollStatusPrefab;
        [SerializeField] private GameObject _winnerPanel;
        [SerializeField] private TextMeshProUGUI _winnerText;

        [Header("Rarity Colors")]
        [SerializeField] private Color _commonColor = Color.white;
        [SerializeField] private Color _uncommonColor = Color.green;
        [SerializeField] private Color _rareColor = Color.blue;
        [SerializeField] private Color _epicColor = new Color(0.6f, 0.2f, 0.8f);
        [SerializeField] private Color _legendaryColor = new Color(1f, 0.5f, 0f);

        private LootRollSession _currentSession;
        private float _countdownTimer;
        private bool _hasRolled;
        private Dictionary<ulong, GameObject> _rollStatusEntries = new();

        public bool IsVisible => _windowPanel != null && _windowPanel.activeSelf;
        public event Action<string, LootRollType> OnRollSubmitted;

        private void Awake()
        {
            if (_windowPanel != null)
                _windowPanel.SetActive(false);

            if (_winnerPanel != null)
                _winnerPanel.SetActive(false);

            SetupButtons();
        }

        private void SetupButtons()
        {
            if (_needButton != null)
                _needButton.onClick.AddListener(() => SubmitRoll(LootRollType.Need));

            if (_greedButton != null)
                _greedButton.onClick.AddListener(() => SubmitRoll(LootRollType.Greed));

            if (_passButton != null)
                _passButton.onClick.AddListener(() => SubmitRoll(LootRollType.Pass));
        }

        private void Update()
        {
            if (!IsVisible || _currentSession == null || _hasRolled)
                return;

            _countdownTimer -= Time.deltaTime;
            UpdateCountdown(_countdownTimer);

            // Auto-pass on timeout (Requirements: 15.4)
            if (_countdownTimer <= 0)
            {
                SubmitRoll(LootRollType.Pass);
            }
        }

        public void Show(LootRollSession session)
        {
            if (session == null) return;

            _currentSession = session;
            _countdownTimer = session.TimeoutSeconds;
            _hasRolled = false;

            // Clear previous roll status entries
            ClearRollStatusEntries();

            // Setup item display
            SetupItemDisplay(session.Item);

            // Enable buttons
            SetButtonsInteractable(true);

            // Show window
            if (_windowPanel != null)
                _windowPanel.SetActive(true);

            if (_winnerPanel != null)
                _winnerPanel.SetActive(false);

            UnityEngine.Debug.Log($"[LootWindowUI] Showing loot window for {session.Item.ItemName}");
        }

        public void Hide()
        {
            if (_windowPanel != null)
                _windowPanel.SetActive(false);

            _currentSession = null;
            ClearRollStatusEntries();
        }

        public void UpdateRollStatus(ulong playerId, LootRollResult result)
        {
            if (_rollStatusContainer == null || _rollStatusPrefab == null)
                return;

            // Create or update status entry
            if (!_rollStatusEntries.TryGetValue(playerId, out var entry))
            {
                entry = Instantiate(_rollStatusPrefab, _rollStatusContainer);
                _rollStatusEntries[playerId] = entry;
            }

            // Update entry text
            var text = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                string rollTypeStr = result.RollType.ToString();
                string rollValueStr = result.RollType != LootRollType.Pass ? $" ({result.RollValue})" : "";
                text.text = $"Player {playerId}: {rollTypeStr}{rollValueStr}";
            }
        }

        public void ShowWinner(ulong? winnerId, ItemData item)
        {
            SetButtonsInteractable(false);

            if (_winnerPanel != null)
            {
                _winnerPanel.SetActive(true);

                if (_winnerText != null)
                {
                    if (winnerId.HasValue)
                    {
                        _winnerText.text = $"Player {winnerId.Value} won {item.ItemName}!";
                    }
                    else
                    {
                        _winnerText.text = "No one rolled. Item was not distributed.";
                    }
                }
            }

            // Auto-hide after delay
            Invoke(nameof(Hide), 3f);
        }

        public void UpdateCountdown(float secondsRemaining)
        {
            if (_countdownText != null)
            {
                int seconds = Mathf.CeilToInt(secondsRemaining);
                _countdownText.text = $"{seconds}s";

                // Change color when low
                if (seconds <= 5)
                {
                    _countdownText.color = Color.red;
                }
                else if (seconds <= 10)
                {
                    _countdownText.color = Color.yellow;
                }
                else
                {
                    _countdownText.color = Color.white;
                }
            }
        }

        private void SubmitRoll(LootRollType rollType)
        {
            if (_currentSession == null || _hasRolled)
                return;

            _hasRolled = true;
            SetButtonsInteractable(false);

            UnityEngine.Debug.Log($"[LootWindowUI] Submitting roll: {rollType}");
            OnRollSubmitted?.Invoke(_currentSession.SessionId, rollType);
        }

        private void SetupItemDisplay(ItemData item)
        {
            if (item == null) return;

            // Set item name with rarity color
            if (_itemNameText != null)
            {
                _itemNameText.text = item.ItemName;
                _itemNameText.color = GetRarityColor(item.Rarity);
            }

            // Set item level
            if (_itemLevelText != null)
            {
                _itemLevelText.text = $"Item Level {item.ItemLevel}";
            }

            // Set icon (placeholder - would load from item data)
            if (_itemIcon != null)
            {
                _itemIcon.color = GetRarityColor(item.Rarity);
            }
        }

        private Color GetRarityColor(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => _commonColor,
                ItemRarity.Uncommon => _uncommonColor,
                ItemRarity.Rare => _rareColor,
                ItemRarity.Epic => _epicColor,
                ItemRarity.Legendary => _legendaryColor,
                _ => _commonColor
            };
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (_needButton != null) _needButton.interactable = interactable;
            if (_greedButton != null) _greedButton.interactable = interactable;
            if (_passButton != null) _passButton.interactable = interactable;
        }

        private void ClearRollStatusEntries()
        {
            foreach (var entry in _rollStatusEntries.Values)
            {
                if (entry != null)
                    Destroy(entry);
            }
            _rollStatusEntries.Clear();
        }

        private void OnDestroy()
        {
            if (_needButton != null) _needButton.onClick.RemoveAllListeners();
            if (_greedButton != null) _greedButton.onClick.RemoveAllListeners();
            if (_passButton != null) _passButton.onClick.RemoveAllListeners();
        }
    }
}
