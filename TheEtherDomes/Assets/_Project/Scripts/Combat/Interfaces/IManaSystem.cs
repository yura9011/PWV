using System;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Interface for managing player mana resources.
    /// Handles mana spending, restoration, and regeneration rates.
    /// </summary>
    public interface IManaSystem
    {
        /// <summary>
        /// Gets the current mana for a player.
        /// </summary>
        float GetCurrentMana(ulong playerId);

        /// <summary>
        /// Gets the maximum mana for a player.
        /// </summary>
        float GetMaxMana(ulong playerId);

        /// <summary>
        /// Gets the current mana as a percentage (0-1).
        /// </summary>
        float GetManaPercent(ulong playerId);

        /// <summary>
        /// Attempts to spend mana. Returns false if insufficient mana.
        /// </summary>
        /// <param name="playerId">The player ID.</param>
        /// <param name="amount">Amount of mana to spend.</param>
        /// <returns>True if mana was spent, false if insufficient.</returns>
        bool TrySpendMana(ulong playerId, float amount);

        /// <summary>
        /// Restores mana to a player, capped at max mana.
        /// </summary>
        void RestoreMana(ulong playerId, float amount);

        /// <summary>
        /// Sets the maximum mana for a player.
        /// </summary>
        void SetMaxMana(ulong playerId, float maxMana);

        /// <summary>
        /// Initializes mana tracking for a player.
        /// </summary>
        void RegisterPlayer(ulong playerId, float maxMana);

        /// <summary>
        /// Removes mana tracking for a player.
        /// </summary>
        void UnregisterPlayer(ulong playerId);

        /// <summary>
        /// Starts in-combat mana regeneration (0.5% per second).
        /// </summary>
        void StartCombatRegen(ulong playerId);

        /// <summary>
        /// Starts out-of-combat mana regeneration (2% per second).
        /// </summary>
        void StartOutOfCombatRegen(ulong playerId);

        /// <summary>
        /// Stops mana regeneration for a player.
        /// </summary>
        void StopRegen(ulong playerId);

        /// <summary>
        /// Event fired when mana changes. Args: playerId, currentMana, maxMana.
        /// </summary>
        event Action<ulong, float, float> OnManaChanged;

        /// <summary>
        /// Event fired when a player's mana reaches zero.
        /// </summary>
        event Action<ulong> OnManaEmpty;

        /// <summary>
        /// Out of combat regeneration rate (2% per second).
        /// </summary>
        float OutOfCombatRegenRate { get; }

        /// <summary>
        /// In combat regeneration rate (0.5% per second).
        /// </summary>
        float InCombatRegenRate { get; }
    }
}
