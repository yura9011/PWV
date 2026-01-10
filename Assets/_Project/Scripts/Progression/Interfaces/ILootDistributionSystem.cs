using System;
using System.Collections.Generic;
using EtherDomes.Data;

namespace EtherDomes.Progression
{
    /// <summary>
    /// Roll type for loot distribution.
    /// </summary>
    public enum LootRollType
    {
        Pass = 0,
        Greed = 1,
        Need = 2
    }

    /// <summary>
    /// Result of a loot roll.
    /// </summary>
    public struct LootRollResult
    {
        public ulong PlayerId;
        public LootRollType RollType;
        public int RollValue;
        public bool Won;
    }

    /// <summary>
    /// Active loot roll session.
    /// </summary>
    public class LootRollSession
    {
        public string SessionId;
        public ItemData Item;
        public List<ulong> EligiblePlayers;
        public Dictionary<ulong, LootRollResult> Rolls;
        public DateTime StartTime;
        public float TimeoutSeconds;
        public bool IsFinalized;
        public ulong? WinnerId;
    }

    /// <summary>
    /// Interface for the loot distribution system using Need/Greed rolls.
    /// Requirements: 4.1, 4.4
    /// </summary>
    public interface ILootDistributionSystem
    {
        /// <summary>
        /// Timeout in seconds for loot rolls (30s).
        /// </summary>
        float RollTimeout { get; }

        /// <summary>
        /// Starts a Need/Greed roll for an item.
        /// </summary>
        /// <param name="item">The item to roll for</param>
        /// <param name="eligiblePlayers">List of player IDs eligible to roll</param>
        /// <returns>Session ID for the roll</returns>
        string StartNeedGreedRoll(ItemData item, List<ulong> eligiblePlayers);

        /// <summary>
        /// Submits a roll for a player.
        /// </summary>
        /// <param name="sessionId">The roll session ID</param>
        /// <param name="playerId">The player submitting the roll</param>
        /// <param name="rollType">Need, Greed, or Pass</param>
        void SubmitRoll(string sessionId, ulong playerId, LootRollType rollType);

        /// <summary>
        /// Finalizes rolls and determines winner.
        /// Called automatically on timeout or when all players have rolled.
        /// </summary>
        /// <param name="sessionId">The roll session ID</param>
        /// <returns>The winning player ID, or null if all passed</returns>
        ulong? FinalizeRolls(string sessionId);

        /// <summary>
        /// Gets the bad luck protection bonus for a player and rarity.
        /// </summary>
        /// <param name="playerId">The player ID</param>
        /// <param name="rarity">The item rarity</param>
        /// <returns>Bonus percentage (0.0 to 1.0)</returns>
        float GetBadLuckProtectionBonus(ulong playerId, ItemRarity rarity);

        /// <summary>
        /// Records a loot attempt for bad luck protection tracking.
        /// </summary>
        /// <param name="playerId">The player ID</param>
        /// <param name="rarity">The item rarity</param>
        /// <param name="won">Whether the player won the item</param>
        void RecordLootAttempt(ulong playerId, ItemRarity rarity, bool won);

        /// <summary>
        /// Gets the current roll session.
        /// </summary>
        LootRollSession GetSession(string sessionId);

        /// <summary>
        /// Event fired when a roll session starts.
        /// </summary>
        event Action<LootRollSession> OnRollStarted;

        /// <summary>
        /// Event fired when a player submits a roll.
        /// </summary>
        event Action<string, ulong, LootRollResult> OnRollSubmitted;

        /// <summary>
        /// Event fired when rolls are finalized.
        /// </summary>
        event Action<string, ulong?, ItemData> OnRollFinalized;
    }
}
