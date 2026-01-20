using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using EtherDomes.Data;

namespace EtherDomes.UI
{
    /// <summary>
    /// Inventory UI implementation with 30 slots.
    /// Requirements: 16.1, 16.2, 16.3, 16.4, 16.5, 16.6
    /// </summary>
    public class InventoryUI : MonoBehaviour, IInventoryUI
    {
        public const int INVENTORY_SLOT_COUNT = 30;
        
        [Header("Main Panel")]
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private Transform _slotsContainer;
        [SerializeField] private GameObject _slotPrefab;
        
        [Header("Tooltip")]
        [SerializeField] private GameObject _tooltipPanel;
        [SerializeField] private TextMeshProUGUI _tooltipNameText;
        [SerializeField] private TextMeshProUGUI _tooltipDescText;
        [SerializeField] private TextMeshProUGUI _tooltipStatsText;
        
        [Header("Context Menu")]
        [SerializeField] private GameObject _contextMenuPanel;
        [SerializeField] private Button _equipButton;
        [SerializeField] private Button _salvageButton;
        
        [Header("Input")]
        [SerializeField] private KeyCode _toggleKey = KeyCode.I;
        
        // State
        private readonly ItemData[] _slots = new ItemData[INVENTORY_SLOT_COUNT];
        private readonly List<InventorySlot> _slotComponents = new();
        private int _contextMenuSlotIndex = -1;
        
        // Events
        public event Action<int, ItemData> OnSlotClicked;
        public event Action<int, ItemData> OnSlotRightClicked;
        public event Action<int, ItemData> OnEquipRequested;
        public event Action<int, ItemData> OnSalvageRequested;
        
        // Properties
        public int SlotCount => INVENTORY_SLOT_COUNT;
        public bool IsVisible => _mainPanel != null && _mainPanel.activeSelf;
        
        private void Awake()
        {
            CreateSlots();
            SetupContextMenu();
        }
        
        private void Start()
        {
            Hide();
            HideTooltip();
            HideContextMenu();
        }
        
        private void Update()
        {
            // Toggle with I key (Requirement 16.6)
            if (Input.GetKeyDown(_toggleKey))
            {
                Toggle();
            }
            
            // Close context menu on click outside
            if (_contextMenuPanel != null && _contextMenuPanel.activeSelf)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                {
                    if (!IsPointerOverContextMenu())
                    {
                        HideContextMenu();
                    }
                }
            }
        }
        
        private void CreateSlots()
        {
            if (_slotPrefab == null || _slotsContainer == null)
            {
                UnityEngine.Debug.LogWarning("[InventoryUI] Missing slot prefab or container");
                return;
            }
            
            for (int i = 0; i < INVENTORY_SLOT_COUNT; i++)
            {
                var slotObj = Instantiate(_slotPrefab, _slotsContainer);
                var slot = slotObj.GetComponent<InventorySlot>();
                
                if (slot == null)
                {
                    slot = slotObj.AddComponent<InventorySlot>();
                }
                
                int slotIndex = i;
                slot.Initialize(slotIndex, this);
                _slotComponents.Add(slot);
            }
        }
        
        private void SetupContextMenu()
        {
            if (_equipButton != null)
                _equipButton.onClick.AddListener(OnEquipClicked);
            
            if (_salvageButton != null)
                _salvageButton.onClick.AddListener(OnSalvageClicked);
        }

        public void Show()
        {
            if (_mainPanel != null)
            {
                _mainPanel.SetActive(true);
            }
            RefreshSlots();
        }
        
        public void Hide()
        {
            if (_mainPanel != null)
            {
                _mainPanel.SetActive(false);
            }
            HideTooltip();
            HideContextMenu();
        }
        
        public void Toggle()
        {
            if (IsVisible)
                Hide();
            else
                Show();
        }
        
        public void RefreshSlots()
        {
            for (int i = 0; i < _slotComponents.Count && i < INVENTORY_SLOT_COUNT; i++)
            {
                _slotComponents[i].SetItem(_slots[i]);
            }
        }
        
        public void SetSlot(int slotIndex, ItemData item)
        {
            if (slotIndex < 0 || slotIndex >= INVENTORY_SLOT_COUNT)
            {
                UnityEngine.Debug.LogWarning($"[InventoryUI] Invalid slot index: {slotIndex}");
                return;
            }
            
            _slots[slotIndex] = item;
            
            if (slotIndex < _slotComponents.Count)
            {
                _slotComponents[slotIndex].SetItem(item);
            }
        }
        
        public void ClearSlot(int slotIndex)
        {
            SetSlot(slotIndex, null);
        }
        
        /// <summary>
        /// Called by InventorySlot when clicked.
        /// </summary>
        internal void HandleSlotClick(int slotIndex)
        {
            var item = slotIndex < INVENTORY_SLOT_COUNT ? _slots[slotIndex] : null;
            OnSlotClicked?.Invoke(slotIndex, item);
        }
        
        /// <summary>
        /// Called by InventorySlot when right-clicked.
        /// </summary>
        internal void HandleSlotRightClick(int slotIndex)
        {
            var item = slotIndex < INVENTORY_SLOT_COUNT ? _slots[slotIndex] : null;
            OnSlotRightClicked?.Invoke(slotIndex, item);
            
            if (item != null)
            {
                ShowContextMenu(slotIndex, item);
            }
        }
        
        /// <summary>
        /// Called by InventorySlot on hover enter.
        /// </summary>
        internal void HandleSlotHoverEnter(int slotIndex)
        {
            var item = slotIndex < INVENTORY_SLOT_COUNT ? _slots[slotIndex] : null;
            if (item != null)
            {
                ShowTooltip(item);
            }
        }
        
        /// <summary>
        /// Called by InventorySlot on hover exit.
        /// </summary>
        internal void HandleSlotHoverExit(int slotIndex)
        {
            HideTooltip();
        }
        
        private void ShowTooltip(ItemData item)
        {
            if (_tooltipPanel == null) return;
            
            _tooltipPanel.SetActive(true);
            
            if (_tooltipNameText != null)
            {
                _tooltipNameText.text = item.ItemName;
                _tooltipNameText.color = GetRarityColor(item.Rarity);
            }
            
            if (_tooltipDescText != null)
            {
                _tooltipDescText.text = item.Description;
            }
            
            if (_tooltipStatsText != null)
            {
                _tooltipStatsText.text = FormatItemStats(item);
            }
            
            // Position tooltip near mouse
            if (_tooltipPanel.transform is RectTransform rt)
            {
                rt.position = Input.mousePosition + new Vector3(10, -10, 0);
            }
        }
        
        private void HideTooltip()
        {
            if (_tooltipPanel != null)
            {
                _tooltipPanel.SetActive(false);
            }
        }
        
        private void ShowContextMenu(int slotIndex, ItemData item)
        {
            if (_contextMenuPanel == null) return;
            
            _contextMenuSlotIndex = slotIndex;
            _contextMenuPanel.SetActive(true);
            
            // Position near mouse
            if (_contextMenuPanel.transform is RectTransform rt)
            {
                rt.position = Input.mousePosition;
            }
            
            // Enable/disable buttons based on item type
            if (_equipButton != null)
            {
                _equipButton.interactable = item.Type == ItemType.Equipment;
            }
            
            if (_salvageButton != null)
            {
                _salvageButton.interactable = true;
            }
        }
        
        private void HideContextMenu()
        {
            if (_contextMenuPanel != null)
            {
                _contextMenuPanel.SetActive(false);
            }
            _contextMenuSlotIndex = -1;
        }
        
        private void OnEquipClicked()
        {
            if (_contextMenuSlotIndex >= 0 && _contextMenuSlotIndex < INVENTORY_SLOT_COUNT)
            {
                var item = _slots[_contextMenuSlotIndex];
                if (item != null)
                {
                    OnEquipRequested?.Invoke(_contextMenuSlotIndex, item);
                }
            }
            HideContextMenu();
        }
        
        private void OnSalvageClicked()
        {
            if (_contextMenuSlotIndex >= 0 && _contextMenuSlotIndex < INVENTORY_SLOT_COUNT)
            {
                var item = _slots[_contextMenuSlotIndex];
                if (item != null)
                {
                    OnSalvageRequested?.Invoke(_contextMenuSlotIndex, item);
                }
            }
            HideContextMenu();
        }
        
        private bool IsPointerOverContextMenu()
        {
            if (_contextMenuPanel == null) return false;
            
            var eventSystem = EventSystem.current;
            if (eventSystem == null) return false;
            
            var pointerData = new PointerEventData(eventSystem)
            {
                position = Input.mousePosition
            };
            
            var results = new List<RaycastResult>();
            eventSystem.RaycastAll(pointerData, results);
            
            foreach (var result in results)
            {
                if (result.gameObject.transform.IsChildOf(_contextMenuPanel.transform) ||
                    result.gameObject == _contextMenuPanel)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        private Color GetRarityColor(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => Color.white,
                ItemRarity.Uncommon => Color.green,
                ItemRarity.Rare => Color.blue,
                ItemRarity.Epic => new Color(0.6f, 0.2f, 0.8f), // Purple
                ItemRarity.Legendary => new Color(1f, 0.5f, 0f), // Orange
                _ => Color.white
            };
        }
        
        private string FormatItemStats(ItemData item)
        {
            var stats = new List<string>();
            
            if (item.Stats.Strength > 0) stats.Add($"+{item.Stats.Strength} Strength");
            if (item.Stats.Agility > 0) stats.Add($"+{item.Stats.Agility} Agility");
            if (item.Stats.Intellect > 0) stats.Add($"+{item.Stats.Intellect} Intellect");
            if (item.Stats.Stamina > 0) stats.Add($"+{item.Stats.Stamina} Stamina");
            
            return string.Join("\n", stats);
        }
        
        private void OnDestroy()
        {
            if (_equipButton != null)
                _equipButton.onClick.RemoveAllListeners();
            
            if (_salvageButton != null)
                _salvageButton.onClick.RemoveAllListeners();
        }
    }
    
    /// <summary>
    /// Individual inventory slot component.
    /// </summary>
    public class InventorySlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private int _slotIndex;
        private InventoryUI _inventoryUI;
        private ItemData _item;
        
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _rarityBorder;
        [SerializeField] private TextMeshProUGUI _stackText;
        
        public void Initialize(int slotIndex, InventoryUI inventoryUI)
        {
            _slotIndex = slotIndex;
            _inventoryUI = inventoryUI;
        }
        
        public void SetItem(ItemData item)
        {
            _item = item;
            
            if (_iconImage != null)
            {
                _iconImage.enabled = item != null;
                // In production, would load sprite from item.IconPath
            }
            
            if (_rarityBorder != null)
            {
                _rarityBorder.enabled = item != null;
                if (item != null)
                {
                    _rarityBorder.color = GetRarityColor(item.Rarity);
                }
            }
            
            if (_stackText != null)
            {
                _stackText.enabled = item != null && item.StackCount > 1;
                if (item != null)
                {
                    _stackText.text = item.StackCount.ToString();
                }
            }
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_inventoryUI == null) return;
            
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                _inventoryUI.HandleSlotClick(_slotIndex);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                _inventoryUI.HandleSlotRightClick(_slotIndex);
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            _inventoryUI?.HandleSlotHoverEnter(_slotIndex);
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            _inventoryUI?.HandleSlotHoverExit(_slotIndex);
        }
        
        private Color GetRarityColor(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => Color.white,
                ItemRarity.Uncommon => Color.green,
                ItemRarity.Rare => Color.blue,
                ItemRarity.Epic => new Color(0.6f, 0.2f, 0.8f),
                ItemRarity.Legendary => new Color(1f, 0.5f, 0f),
                _ => Color.white
            };
        }
    }
}
