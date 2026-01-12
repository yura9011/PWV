using System;
using EtherDomes.Data;

namespace EtherDomes.UI
{
    /// <summary>
    /// Interface for the resource UI component.
    /// Displays class-specific resources like Energy, Focus, and Combo Points.
    /// Requirements: 10.6, 10.7
    /// </summary>
    public interface IResourceUI
    {
        /// <summary>
        /// Initialize the resource UI for a player.
        /// </summary>
        /// <param name="playerId">Player entity ID</param>
        /// <param name="resourceType">Type of secondary resource</param>
        void Initialize(ulong playerId, SecondaryResourceType resourceType);

        /// <summary>
        /// Update the resource bar display.
        /// </summary>
        /// <param name="current">Current resource value</param>
        /// <param name="max">Maximum resource value</param>
        void UpdateResource(float current, float max);

        /// <summary>
        /// Update the combo points display.
        /// </summary>
        /// <param name="current">Current combo points</param>
        /// <param name="max">Maximum combo points</param>
        void UpdateComboPoints(int current, int max);

        /// <summary>
        /// Set the resource type to display.
        /// </summary>
        /// <param name="resourceType">Type of secondary resource</param>
        void SetResourceType(SecondaryResourceType resourceType);

        /// <summary>
        /// Show or hide the resource UI.
        /// </summary>
        /// <param name="visible">Whether to show the UI</param>
        void SetVisible(bool visible);

        /// <summary>
        /// Check if the resource UI is currently visible.
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// Event fired when resource value changes.
        /// Parameters: current, max
        /// </summary>
        event Action<float, float> OnResourceChanged;
    }
}
