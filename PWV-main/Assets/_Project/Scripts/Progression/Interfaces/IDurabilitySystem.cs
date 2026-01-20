using System;
using EtherDomes.Data;

namespace EtherDomes.Progression
{
    /// <summary>
    /// Interface for the durability system that manages item degradation and repair.
    /// Requirements: 8.5, 8.6
    /// </summary>
    public interface IDurabilitySystem
    {
        /// <summary>
        /// Stat penalty multiplier for broken items (0.5 = 50% stats).
        /// </summary>
        float BrokenItemStatPenalty { get; }

        /// <summary>
        /// Degrades durability of an item by the specified amount.
        /// </summary>
        /// <param name="item">The item to degrade</param>
        /// <param name="amount">Amount of durability to remove</param>
        void DegradeDurability(ItemData item, int amount = 1);

        /// <summary>
        /// Repairs an item to full durability.
        /// </summary>
        /// <param name="item">The item to repair</param>
        /// <returns>The gold cost of the repair</returns>
        int RepairItem(ItemData item);

        /// <summary>
        /// Gets the stat penalty multiplier for an item based on its durability.
        /// Returns 1.0 for items with durability > 0, BrokenItemStatPenalty for broken items.
        /// </summary>
        /// <param name="item">The item to check</param>
        /// <returns>Stat multiplier (0.5 for broken, 1.0 for functional)</returns>
        float GetStatPenalty(ItemData item);

        /// <summary>
        /// Calculates the repair cost for an item.
        /// </summary>
        /// <param name="item">The item to calculate repair cost for</param>
        /// <returns>Gold cost to repair</returns>
        int GetRepairCost(ItemData item);

        /// <summary>
        /// Checks if an item needs repair.
        /// </summary>
        bool NeedsRepair(ItemData item);

        /// <summary>
        /// Event fired when an item breaks (durability reaches 0).
        /// </summary>
        event Action<ItemData> OnItemBroken;

        /// <summary>
        /// Event fired when an item is repaired.
        /// </summary>
        event Action<ItemData> OnItemRepaired;

        /// <summary>
        /// Event fired when durability changes.
        /// </summary>
        event Action<ItemData, int, int> OnDurabilityChanged;
    }
}
