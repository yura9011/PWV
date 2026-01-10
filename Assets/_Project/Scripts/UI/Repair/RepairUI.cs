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
    /// UI for displaying equipment durability and repair options.
    /// 
    /// Features:
    /// - Equipment list with durability bars
    /// - Repair cost per item
    /// - Repair and Repair All buttons
    /// - Disabled buttons when insufficient gold
    /// 
    /// Requirements: 18.1, 18.2, 18.3, 18.4, 18.5
    /// </summary>
    public class RepairUI : MonoBehaviour, IRepairUI
    {
        [Header("UI References")]
        [SerializeField] private GameObject _windowPanel;
        [SerializeField] private Transform _equipmentListContainer;
        [SerializeField] private GameObject _equipmentSlotPrefab;
        [SerializeField] private TextMeshProUGUI _goldText;
        [SerializeField] private TextMeshProUGUI _totalRepairCostText;
        [SerializeField] private Button _repairAllButton;
        [SerializeField] private Button _closeButton;

        private Dictionary<EquipmentSlot, ItemData> _currentEquipment = new();
        private Dictionary<EquipmentSlot, RepairSlotUI> _slotUIs = new();
        private int _playerGold;
        private IDurabilitySystem _durabilitySystem;

        public bool IsVisible => _windowPanel != null && _windowPanel.activeSelf;
        public event Action<EquipmentSlot> OnRepairClicked;
        public event Action OnRepairAllClicked;

        private void Awake()
        {
            _durabilitySystem = new DurabilitySystem();

            if (_windowPanel != null)
                _windowPanel.SetActive(false);

            SetupButtons();
        }

        private void SetupButtons()
        {
            if (_repairAllButton != null)
                _repairAllButton.onClick.AddListener(HandleRepairAllClicked);

            if (_closeButton != null)
                _closeButton.onClick.AddListener(Hide);
        }

        public void Show()
        {
            if (_windowPanel != null)
                _windowPanel.SetActive(true);

            RefreshUI();
            UnityEngine.Debug.Log("[RepairUI] Showing repair window");
        }

        public void Hide()
        {
            if (_windowPanel != null)
                _windowPanel.SetActive(false);
        }

        public void RefreshEquipment(Dictionary<EquipmentSlot, ItemData> equipment)
        {
            _currentEquipment = equipment ?? new Dictionary<EquipmentSlot, ItemData>();
            RefreshUI();
        }

        public void UpdatePlayerGold(int gold)
        {
            _playerGold = gold;

            if (_goldText != null)
                _goldText.text = $"Gold: {gold}";

            RefreshUI();
        }

        private void RefreshUI()
        {
            ClearSlotUIs();

            int totalRepairCost = 0;
            bool anyNeedsRepair = false;

            foreach (var kvp in _currentEquipment)
            {
                var slot = kvp.Key;
                var item = kvp.Value;

                if (item == null) continue;

                // Create slot UI
                var slotUI = CreateSlotUI(slot, item);
                _slotUIs[slot] = slotUI;

                // Calculate repair cost
                int repairCost = _durabilitySystem.GetRepairCost(item);
                totalRepairCost += repairCost;

                if (_durabilitySystem.NeedsRepair(item))
                    anyNeedsRepair = true;
            }

            // Update total repair cost
            if (_totalRepairCostText != null)
                _totalRepairCostText.text = $"Total: {totalRepairCost} gold";

            // Update Repair All button
            if (_repairAllButton != null)
            {
                bool canAfford = _playerGold >= totalRepairCost;
                _repairAllButton.interactable = anyNeedsRepair && canAfford;
            }
        }

        private RepairSlotUI CreateSlotUI(EquipmentSlot slot, ItemData item)
        {
            if (_equipmentListContainer == null || _equipmentSlotPrefab == null)
                return null;

            var slotObj = Instantiate(_equipmentSlotPrefab, _equipmentListContainer);
            var slotUI = slotObj.GetComponent<RepairSlotUI>();

            if (slotUI == null)
                slotUI = slotObj.AddComponent<RepairSlotUI>();

            int repairCost = _durabilitySystem.GetRepairCost(item);
            bool needsRepair = _durabilitySystem.NeedsRepair(item);
            bool canAfford = _playerGold >= repairCost;

            slotUI.Setup(slot, item, repairCost, needsRepair && canAfford);
            slotUI.OnRepairClicked += HandleSlotRepairClicked;

            return slotUI;
        }

        private void HandleSlotRepairClicked(EquipmentSlot slot)
        {
            UnityEngine.Debug.Log($"[RepairUI] Repair clicked for slot: {slot}");
            OnRepairClicked?.Invoke(slot);
        }

        private void HandleRepairAllClicked()
        {
            UnityEngine.Debug.Log("[RepairUI] Repair All clicked");
            OnRepairAllClicked?.Invoke();
        }

        private void ClearSlotUIs()
        {
            foreach (var slotUI in _slotUIs.Values)
            {
                if (slotUI != null)
                {
                    slotUI.OnRepairClicked -= HandleSlotRepairClicked;
                    Destroy(slotUI.gameObject);
                }
            }
            _slotUIs.Clear();
        }

        private void OnDestroy()
        {
            if (_repairAllButton != null) _repairAllButton.onClick.RemoveAllListeners();
            if (_closeButton != null) _closeButton.onClick.RemoveAllListeners();
            ClearSlotUIs();
        }
    }

    /// <summary>
    /// UI component for a single equipment slot in the repair window.
    /// </summary>
    public class RepairSlotUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _itemNameText;
        [SerializeField] private TextMeshProUGUI _durabilityText;
        [SerializeField] private Slider _durabilityBar;
        [SerializeField] private TextMeshProUGUI _repairCostText;
        [SerializeField] private Button _repairButton;

        private EquipmentSlot _slot;

        public event Action<EquipmentSlot> OnRepairClicked;

        public void Setup(EquipmentSlot slot, ItemData item, int repairCost, bool canRepair)
        {
            _slot = slot;

            if (_itemNameText != null)
                _itemNameText.text = $"[{slot}] {item.ItemName}";

            if (_durabilityText != null)
                _durabilityText.text = $"{item.CurrentDurability}/{item.MaxDurability}";

            if (_durabilityBar != null)
            {
                _durabilityBar.maxValue = item.MaxDurability;
                _durabilityBar.value = item.CurrentDurability;

                // Color based on durability
                var fill = _durabilityBar.fillRect?.GetComponent<Image>();
                if (fill != null)
                {
                    float percent = item.DurabilityPercent;
                    if (percent <= 0)
                        fill.color = Color.red;
                    else if (percent < 0.25f)
                        fill.color = new Color(1f, 0.5f, 0f); // Orange
                    else if (percent < 0.5f)
                        fill.color = Color.yellow;
                    else
                        fill.color = Color.green;
                }
            }

            if (_repairCostText != null)
            {
                _repairCostText.text = repairCost > 0 ? $"{repairCost}g" : "OK";
            }

            if (_repairButton != null)
            {
                _repairButton.interactable = canRepair && repairCost > 0;
                _repairButton.onClick.RemoveAllListeners();
                _repairButton.onClick.AddListener(() => OnRepairClicked?.Invoke(_slot));
            }
        }

        private void OnDestroy()
        {
            if (_repairButton != null)
                _repairButton.onClick.RemoveAllListeners();
        }
    }
}
