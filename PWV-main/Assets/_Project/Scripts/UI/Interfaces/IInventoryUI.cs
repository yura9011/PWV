using System;
using EtherDomes.Data;

namespace EtherDomes.UI
{
    /// <summary>
    /// Interface for the inventory UI.
    /// Requirements: 16.1, 16.2
    /// </summary>
    public interface IInventoryUI
    {
        /// <summary>
        /// Number of inventory slots. Should be 30.
        /// </summary>
        int SlotCount { get; }
        
        /// <summary>
        /// Shows the inventory UI.
        /// </summary>
        void Show();
        
        /// <summary>
        /// Hides the inventory UI.
        /// </summary>
        void Hide();
        
        /// <summary>
        /// Toggles the inventory UI visibility.
        /// </summary>
        void Toggle();
        
        /// <summary>
        /// Refreshes all inventory slots.
        /// </summary>
        void RefreshSlots();
        
        /// <summary>
        /// Sets the item in a specific slot.
        /// </summary>
        void SetSlot(int slotIndex, ItemData item);
        
        /// <summary>
        /// Clears a specific slot.
        /// </summary>
        void ClearSlot(int slotIndex);
        
        /// <summary>
        /// Gets whether the inventory is currently visible.
        /// </summary>
        bool IsVisible { get; }
        
        /// <summary>
        /// Event fired when a slot is left-clicked.
        /// </summary>
        event Action<int, ItemData> OnSlotClicked;
        
        /// <summary>
        /// Event fired when a slot is right-clicked.
        /// </summary>
        event Action<int, ItemData> OnSlotRightClicked;
        
        /// <summary>
        /// Event fired when equip is selected from context menu.
        /// </summary>
        event Action<int, ItemData> OnEquipRequested;
        
        /// <summary>
        /// Event fired when salvage is selected from context menu.
        /// </summary>
        event Action<int, ItemData> OnSalvageRequested;
    }
}
