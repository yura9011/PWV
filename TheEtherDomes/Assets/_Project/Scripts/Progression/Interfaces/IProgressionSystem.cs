using System;
using EtherDomes.Data;

namespace EtherDomes.Progression
{
    /// <summary>
    /// Interface for the character progression system.
    /// Handles experience, levels, and stat progression.
    /// </summary>
    public interface IProgressionSystem
    {
        /// <summary>
        /// Add experience to a player.
        /// </summary>
        void AddExperience(ulong playerId, int amount);

        /// <summary>
        /// Get the current level of a player.
        /// </summary>
        int GetLevel(ulong playerId);

        /// <summary>
        /// Get the current experience of a player.
        /// </summary>
        int GetExperience(ulong playerId);

        /// <summary>
        /// Get the experience required to reach the next level.
        /// </summary>
        int GetExperienceToNextLevel(ulong playerId);

        /// <summary>
        /// Calculate experience reward for killing an enemy.
        /// </summary>
        int CalculateExperienceReward(int playerLevel, int enemyLevel);

        /// <summary>
        /// Get base stats for a level and class.
        /// </summary>
        CharacterStats GetBaseStats(int level, CharacterClass charClass);

        /// <summary>
        /// Get stat growth per level for a class.
        /// Requirements: 12.6, 12.7
        /// </summary>
        CharacterStats GetStatGrowthPerLevel(CharacterClass charClass);

        /// <summary>
        /// Event fired when a player levels up.
        /// </summary>
        event Action<ulong, int> OnLevelUp;

        /// <summary>
        /// Event fired when a player unlocks a new ability.
        /// </summary>
        event Action<ulong, AbilityData> OnAbilityUnlocked;

        /// <summary>
        /// Maximum level cap.
        /// </summary>
        int MaxLevel { get; }

        /// <summary>
        /// Levels at which new abilities are unlocked.
        /// </summary>
        int[] AbilityUnlockLevels { get; }
    }
}
