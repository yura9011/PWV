using System;
using System.Collections.Generic;
using UnityEngine;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Interface for friendly fire system (AoE affecting allies/enemies).
    /// Requirements: 9.4, 9.5
    /// </summary>
    public interface IFriendlyFireSystem
    {
        /// <summary>
        /// Apply AoE damage at a position.
        /// </summary>
        AoEResult ApplyAoEDamage(Vector3 center, float radius, float damage, 
            ulong casterId, bool affectAllies, bool affectEnemies);

        /// <summary>
        /// Apply AoE healing at a position.
        /// </summary>
        AoEResult ApplyAoEHealing(Vector3 center, float radius, float healing, 
            ulong casterId, bool affectAllies, bool affectEnemies);

        /// <summary>
        /// Get all targets in an AoE area.
        /// </summary>
        List<ITargetable> GetAffectedTargets(Vector3 center, float radius, 
            ulong casterId, bool includeAllies, bool includeEnemies);

        /// <summary>
        /// Check if friendly fire is enabled globally.
        /// </summary>
        bool IsFriendlyFireEnabled { get; }

        /// <summary>
        /// Event fired when AoE affects a target.
        /// </summary>
        event Action<ulong, ITargetable, float, bool> OnAoEApplied; // casterId, target, amount, isDamage
    }

    /// <summary>
    /// Result of an AoE application.
    /// </summary>
    public class AoEResult
    {
        public int AlliesAffected;
        public int EnemiesAffected;
        public float TotalDamageDealt;
        public float TotalHealingDone;
        public List<ITargetable> AffectedTargets = new();
    }
}
