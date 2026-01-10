using System;
using System.Collections.Generic;
using EtherDomes.Data;

namespace EtherDomes.UI
{
    /// <summary>
    /// Interface for the repair UI that displays equipment durability and repair options.
    /// Requirements: 18.1, 18.2
    /// </summary>
    public interface IRepairUI
    {
        /// <summary>
        /// Shows the repair UI.
        /// </summary>
        void Show();

        /// <summary>
        /// Hides the repair UI.
        /// </summary>
        void Hide();

        /// <summary>
        /// Refreshes the equipment display with current durability.
        /// </summary>
        /// <param name="equipment">Dictionary of equipped items by slot</param>
        void RefreshEquipment(Dictionary<EquipmentSlot, ItemData> equipment);

        /// <summary>
        /// Updates the player's gold display.
        /// </summary>
        /// <param name="gold">Current gold amount</param>
        void UpdatePlayerGold(int gold);

        /// <summary>
        /// Gets whether the UI is currently visible.
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// Event fired when player clicks repair on a specific item.
        /// </summary>
        event Action<EquipmentSlot> OnRepairClicked;

        /// <summary>
        /// Event fired when player clicks repair all.
        /// </summary>
        event Action OnRepairAllClicked;
    }
}
