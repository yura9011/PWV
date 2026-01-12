using System;
using EtherDomes.Data;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Interface for secondary resource system (Rage, Holy Power, Energy, Focus, Combo Points).
    /// Requirements: 9.3, 5.2, 5.3, 5.4, 5.5, 6.2
    /// </summary>
    public interface ISecondaryResourceSystem
    {
        /// <summary>
        /// Register a player with their resource type.
        /// </summary>
        void RegisterResource(ulong playerId, SecondaryResourceType resourceType, float maxValue);

        /// <summary>
        /// Add resource to a player.
        /// </summary>
        void AddResource(ulong playerId, float amount);

        /// <summary>
        /// Try to spend resource. Returns true if successful.
        /// </summary>
        bool TrySpendResource(ulong playerId, float amount);

        /// <summary>
        /// Get current resource value.
        /// </summary>
        float GetResource(ulong playerId);

        /// <summary>
        /// Get max resource value.
        /// </summary>
        float GetMaxResource(ulong playerId);

        /// <summary>
        /// Get resource type for a player.
        /// </summary>
        SecondaryResourceType GetResourceType(ulong playerId);

        /// <summary>
        /// Apply decay/regeneration to resources.
        /// </summary>
        void ApplyDecay(ulong playerId, float deltaTime, bool inCombat);

        /// <summary>
        /// Event fired when resource changes.
        /// </summary>
        event Action<ulong, float, float> OnResourceChanged; // playerId, current, max

        #region Combo Points (Requirements 5.3, 5.4, 5.5)

        /// <summary>
        /// Register combo points for a player (Rogue).
        /// </summary>
        void RegisterComboPoints(ulong playerId);

        /// <summary>
        /// Get current combo points for a player.
        /// </summary>
        int GetComboPoints(ulong playerId);

        /// <summary>
        /// Get max combo points (always 5).
        /// </summary>
        int GetMaxComboPoints();

        /// <summary>
        /// Add a combo point from a generator ability.
        /// Requirements: 5.4
        /// </summary>
        void AddComboPoint(ulong playerId, int amount = 1);

        /// <summary>
        /// Consume all combo points for a finisher ability.
        /// Returns the number of combo points consumed.
        /// Requirements: 5.5
        /// </summary>
        int ConsumeAllComboPoints(ulong playerId);

        /// <summary>
        /// Check if player has at least one combo point.
        /// </summary>
        bool HasComboPoints(ulong playerId);

        /// <summary>
        /// Calculate damage multiplier based on combo points consumed.
        /// Requirements: 5.5
        /// </summary>
        float CalculateComboPointDamageMultiplier(int comboPoints);

        /// <summary>
        /// Event fired when combo points change.
        /// </summary>
        event Action<ulong, int, int> OnComboPointsChanged; // playerId, current, max

        #endregion
    }
}
