using System;
using UnityEngine;
using EtherDomes.Data;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Interface for entities that can be targeted (enemies, players, NPCs).
    /// </summary>
    public interface ITargetable
    {
        /// <summary>
        /// Unique network ID for this entity.
        /// </summary>
        ulong NetworkId { get; }

        /// <summary>
        /// Display name shown in target frame.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Current world position.
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// Whether this entity is alive and can be targeted.
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// Type of target (Enemy, Friendly, Neutral).
        /// </summary>
        TargetType Type { get; }

        /// <summary>
        /// Current health percentage (0-1).
        /// </summary>
        float HealthPercent { get; }

        /// <summary>
        /// Current health value.
        /// </summary>
        float CurrentHealth { get; }

        /// <summary>
        /// Maximum health value.
        /// </summary>
        float MaxHealth { get; }

        /// <summary>
        /// Level of this entity.
        /// </summary>
        int Level { get; }

        /// <summary>
        /// Transform component for visual effects.
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// Event fired when this target dies.
        /// </summary>
        event Action<ITargetable> OnDeath;

        /// <summary>
        /// Gets the threat level this target has toward a specific player.
        /// Used for auto-switch priority.
        /// </summary>
        float GetThreatTo(ulong playerId);
    }
}
