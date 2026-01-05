using System;
using EtherDomes.Data;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Interface for secondary resource system (Rage, Holy Power, etc.).
    /// Requirements: 9.3
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
        /// Apply decay to resources that decay out of combat.
        /// </summary>
        void ApplyDecay(ulong playerId, float deltaTime, bool inCombat);

        /// <summary>
        /// Event fired when resource changes.
        /// </summary>
        event Action<ulong, float, float> OnResourceChanged; // playerId, current, max
    }
}
