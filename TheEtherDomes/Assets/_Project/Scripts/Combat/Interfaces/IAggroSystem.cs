using System;
using System.Collections.Generic;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Interface for the threat/aggro management system.
    /// </summary>
    public interface IAggroSystem
    {
        /// <summary>
        /// Add threat from a player to an enemy.
        /// </summary>
        /// <param name="playerId">Player generating threat</param>
        /// <param name="enemyId">Enemy receiving threat</param>
        /// <param name="amount">Amount of threat to add</param>
        void AddThreat(ulong playerId, ulong enemyId, float amount);

        /// <summary>
        /// Execute a taunt, setting player's threat to highest + 10%.
        /// </summary>
        /// <param name="playerId">Player using taunt</param>
        /// <param name="enemyId">Enemy being taunted</param>
        void Taunt(ulong playerId, ulong enemyId);

        /// <summary>
        /// Add healing threat, split among engaged enemies.
        /// </summary>
        /// <param name="healerId">Healer generating threat</param>
        /// <param name="healAmount">Amount healed</param>
        /// <param name="engagedEnemies">Enemies in combat with the group</param>
        void AddHealingThreat(ulong healerId, float healAmount, ulong[] engagedEnemies);

        /// <summary>
        /// Reset all threat for an enemy (combat ended).
        /// </summary>
        /// <param name="enemyId">Enemy to reset</param>
        void ResetThreat(ulong enemyId);

        /// <summary>
        /// Get the player with highest threat on an enemy.
        /// </summary>
        /// <param name="enemyId">Enemy to check</param>
        /// <returns>Player ID with highest threat, or 0 if none</returns>
        ulong GetHighestThreatPlayer(ulong enemyId);

        /// <summary>
        /// Get threat value for a specific player on an enemy.
        /// </summary>
        float GetThreat(ulong playerId, ulong enemyId);

        /// <summary>
        /// Get the full threat table for an enemy.
        /// </summary>
        Dictionary<ulong, float> GetThreatTable(ulong enemyId);

        /// <summary>
        /// Check if a player should pull aggro from current target.
        /// </summary>
        /// <param name="playerId">Player to check</param>
        /// <param name="enemyId">Enemy to check</param>
        /// <param name="isMelee">Whether the player is in melee range</param>
        /// <returns>True if player should pull aggro</returns>
        bool ShouldPullAggro(ulong playerId, ulong enemyId, bool isMelee);

        /// <summary>
        /// Fired when an enemy's target changes due to threat.
        /// </summary>
        event Action<ulong, ulong> OnAggroChanged; // enemyId, newTargetPlayerId

        /// <summary>
        /// Threshold for melee to pull aggro (110%).
        /// </summary>
        float MeleeThreatThreshold { get; }

        /// <summary>
        /// Threshold for ranged to pull aggro (130%).
        /// </summary>
        float RangedThreatThreshold { get; }

        /// <summary>
        /// Multiplier for healing threat (50%).
        /// </summary>
        float HealingThreatMultiplier { get; }
    }
}
