using System;
using EtherDomes.Data;

namespace EtherDomes.Progression
{
    /// <summary>
    /// Interface for the loot generation and distribution system.
    /// </summary>
    public interface ILootSystem
    {
        /// <summary>
        /// Generate loot drops from a boss.
        /// </summary>
        LootDrop[] GenerateLoot(string bossId, int groupSize);

        /// <summary>
        /// Distribute loot to players.
        /// </summary>
        void DistributeLoot(LootDrop[] loot, ulong[] playerIds, LootDistributionMode mode);

        /// <summary>
        /// Event fired when an item is awarded to a player.
        /// </summary>
        event Action<ulong, ItemData> OnItemAwarded;
    }

    /// <summary>
    /// Represents a loot drop from an enemy.
    /// </summary>
    [Serializable]
    public class LootDrop
    {
        public ItemData Item;
        public ulong? AwardedTo;
        public bool IsAwarded => AwardedTo.HasValue;
    }
}
