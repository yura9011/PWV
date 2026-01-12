using System;
using System.Collections.Generic;

namespace EtherDomes.World
{
    /// <summary>
    /// Interface for the weekly lockout system that tracks boss kills.
    /// Requirements: 5.7
    /// </summary>
    public interface IWeeklyLockoutSystem
    {
        /// <summary>
        /// Checks if a player is locked out from a boss.
        /// </summary>
        /// <param name="playerId">The player ID</param>
        /// <param name="bossId">The boss ID</param>
        /// <returns>True if player has already killed this boss this week</returns>
        bool IsLockedOut(ulong playerId, string bossId);

        /// <summary>
        /// Records a boss kill for a player.
        /// </summary>
        /// <param name="playerId">The player ID</param>
        /// <param name="bossId">The boss ID</param>
        void RecordKill(ulong playerId, string bossId);

        /// <summary>
        /// Gets the next reset time (Monday 00:00 UTC).
        /// </summary>
        DateTime GetResetTime();

        /// <summary>
        /// Gets all locked bosses for a player.
        /// </summary>
        /// <param name="playerId">The player ID</param>
        /// <returns>List of boss IDs the player is locked to</returns>
        List<string> GetLockedBosses(ulong playerId);

        /// <summary>
        /// Checks if a reset is needed and performs it.
        /// </summary>
        void CheckAndPerformReset();

        /// <summary>
        /// Event fired when a boss kill is recorded.
        /// </summary>
        event Action<ulong, string> OnBossKillRecorded;

        /// <summary>
        /// Event fired when lockouts are reset.
        /// </summary>
        event Action OnLockoutsReset;
    }
}
