using System;
using System.Collections.Generic;
using EtherDomes.Data;
using EtherDomes.Progression;

namespace EtherDomes.UI
{
    /// <summary>
    /// Interface for the loot window UI that displays Need/Greed rolls.
    /// Requirements: 15.1, 15.2
    /// </summary>
    public interface ILootWindowUI
    {
        /// <summary>
        /// Shows the loot window with items to roll on.
        /// </summary>
        /// <param name="session">The loot roll session</param>
        void Show(LootRollSession session);

        /// <summary>
        /// Hides the loot window.
        /// </summary>
        void Hide();

        /// <summary>
        /// Updates the roll status display for a player.
        /// </summary>
        /// <param name="playerId">The player who rolled</param>
        /// <param name="result">The roll result</param>
        void UpdateRollStatus(ulong playerId, LootRollResult result);

        /// <summary>
        /// Shows the winner of the roll.
        /// </summary>
        /// <param name="winnerId">The winning player ID, or null if no winner</param>
        /// <param name="item">The item that was won</param>
        void ShowWinner(ulong? winnerId, ItemData item);

        /// <summary>
        /// Updates the countdown timer display.
        /// </summary>
        /// <param name="secondsRemaining">Seconds remaining before auto-pass</param>
        void UpdateCountdown(float secondsRemaining);

        /// <summary>
        /// Gets whether the window is currently visible.
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// Event fired when player submits a roll.
        /// </summary>
        event Action<string, LootRollType> OnRollSubmitted;
    }
}
