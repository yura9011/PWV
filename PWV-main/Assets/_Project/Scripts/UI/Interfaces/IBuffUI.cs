using System;
using System.Collections.Generic;
using EtherDomes.Data;

namespace EtherDomes.UI
{
    /// <summary>
    /// Interface for the buff/debuff UI component.
    /// Displays active buffs above health bar and debuffs below.
    /// Requirements: 1.9, 1.10
    /// </summary>
    public interface IBuffUI
    {
        /// <summary>
        /// Initialize the buff UI with the entity ID to track.
        /// </summary>
        /// <param name="entityId">Entity ID to display buffs/debuffs for</param>
        void Initialize(ulong entityId);

        /// <summary>
        /// Set the entity ID to track buffs/debuffs for.
        /// </summary>
        /// <param name="entityId">Entity ID to track</param>
        void SetEntity(ulong entityId);

        /// <summary>
        /// Update the buff display with current active buffs.
        /// </summary>
        /// <param name="buffs">List of active buff instances</param>
        void UpdateBuffs(IReadOnlyList<BuffInstance> buffs);

        /// <summary>
        /// Update the debuff display with current active debuffs.
        /// </summary>
        /// <param name="debuffs">List of active debuff instances</param>
        void UpdateDebuffs(IReadOnlyList<BuffInstance> debuffs);

        /// <summary>
        /// Clear all buff/debuff displays.
        /// </summary>
        void ClearAll();

        /// <summary>
        /// Show or hide the buff UI.
        /// </summary>
        /// <param name="visible">Whether to show the UI</param>
        void SetVisible(bool visible);

        /// <summary>
        /// Event fired when a buff icon is clicked.
        /// Parameters: buffId
        /// </summary>
        event Action<string> OnBuffClicked;

        /// <summary>
        /// Event fired when a debuff icon is clicked.
        /// Parameters: debuffId
        /// </summary>
        event Action<string> OnDebuffClicked;
    }
}
