using System;
using System.Collections.Generic;
using EtherDomes.Data;

namespace EtherDomes.Progression
{
    /// <summary>
    /// Result of salvaging an item.
    /// </summary>
    public struct SalvageResult
    {
        public ItemData SalvagedItem;
        public Dictionary<string, int> Materials;
        public bool Success;
    }

    /// <summary>
    /// Interface for the salvage system that converts items to materials.
    /// Requirements: 4.7
    /// </summary>
    public interface ISalvageSystem
    {
        /// <summary>
        /// Salvages an item and returns materials.
        /// </summary>
        /// <param name="item">The item to salvage</param>
        /// <returns>Salvage result with materials</returns>
        SalvageResult Salvage(ItemData item);

        /// <summary>
        /// Previews what materials would be obtained from salvaging.
        /// </summary>
        /// <param name="item">The item to preview</param>
        /// <returns>Dictionary of material name to quantity</returns>
        Dictionary<string, int> PreviewSalvage(ItemData item);

        /// <summary>
        /// Checks if an item can be salvaged.
        /// </summary>
        bool CanSalvage(ItemData item);

        /// <summary>
        /// Event fired when an item is salvaged.
        /// </summary>
        event Action<SalvageResult> OnItemSalvaged;
    }
}
