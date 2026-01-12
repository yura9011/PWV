using System;
using UnityEngine;

namespace EtherDomes.UI
{
    /// <summary>
    /// Interface for aggro indicator system.
    /// Shows visual feedback for threat/aggro state.
    /// Requirements: 6.2
    /// </summary>
    public interface IAggroIndicator
    {
        /// <summary>
        /// Show aggro icon over a player who has aggro.
        /// </summary>
        void ShowAggroIcon(ulong playerId, bool hasAggro);

        /// <summary>
        /// Update party frame color based on threat level.
        /// </summary>
        void UpdatePartyFrameColor(ulong playerId, ThreatLevel threatLevel);

        /// <summary>
        /// Draw visual line between enemy and its current target.
        /// </summary>
        void DrawAggroLine(ulong enemyId, Vector3 enemyPosition, Vector3 targetPosition);

        /// <summary>
        /// Hide aggro line for an enemy.
        /// </summary>
        void HideAggroLine(ulong enemyId);

        /// <summary>
        /// Clear all aggro indicators.
        /// </summary>
        void ClearAll();

        /// <summary>
        /// Event fired when aggro state changes for a player.
        /// </summary>
        event Action<ulong, bool> OnAggroStateChanged;
    }

    /// <summary>
    /// Threat level for color coding.
    /// </summary>
    public enum ThreatLevel
    {
        None,       // No threat
        Low,        // < 50% of tank
        Medium,     // 50-80% of tank
        High,       // 80-100% of tank
        Aggro       // Has aggro
    }
}
