using System;
using System.Collections.Generic;
using EtherDomes.Data;

namespace EtherDomes.World
{
    /// <summary>
    /// Dungeon difficulty levels.
    /// </summary>
    public enum DungeonDifficulty
    {
        Normal = 0,
        Heroic = 1,
        Mythic = 2
    }

    /// <summary>
    /// Modifiers applied based on difficulty.
    /// </summary>
    public struct DifficultyModifiers
    {
        public float HealthMultiplier;
        public float DamageMultiplier;
        public int AdditionalMechanics;
        public bool HasExclusiveLoot;
    }

    /// <summary>
    /// Interface for the dungeon difficulty system.
    /// Requirements: 5.1, 5.2, 5.3
    /// </summary>
    public interface IDungeonDifficultySystem
    {
        /// <summary>
        /// Gets the current difficulty.
        /// </summary>
        DungeonDifficulty CurrentDifficulty { get; }

        /// <summary>
        /// Sets the dungeon difficulty.
        /// </summary>
        void SetDifficulty(DungeonDifficulty difficulty);

        /// <summary>
        /// Gets the modifiers for the current difficulty.
        /// </summary>
        DifficultyModifiers GetModifiers();

        /// <summary>
        /// Gets the modifiers for a specific difficulty.
        /// </summary>
        DifficultyModifiers GetModifiers(DungeonDifficulty difficulty);

        /// <summary>
        /// Gets the exclusive loot table for the current difficulty.
        /// Returns null if no exclusive loot.
        /// </summary>
        List<string> GetExclusiveLoot();

        /// <summary>
        /// Gets the additional mechanics for the current difficulty.
        /// </summary>
        List<string> GetAdditionalMechanics();

        /// <summary>
        /// Event fired when difficulty changes.
        /// </summary>
        event Action<DungeonDifficulty> OnDifficultyChanged;
    }
}
