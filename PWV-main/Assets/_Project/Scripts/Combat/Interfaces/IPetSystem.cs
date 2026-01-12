using System;
using System.Collections.Generic;
using EtherDomes.Data;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Interface for the pet system handling pet summoning, commands, and behavior.
    /// </summary>
    public interface IPetSystem
    {
        #region Constants
        
        /// <summary>
        /// Distance at which pet follows the owner (in meters).
        /// </summary>
        float FollowDistance { get; } // 3 meters
        
        /// <summary>
        /// Distance at which pet teleports to owner (in meters).
        /// </summary>
        float TeleportDistance { get; } // 40 meters
        
        /// <summary>
        /// Cooldown before pet can be resummoned after death (in seconds).
        /// </summary>
        float ResummonCooldown { get; } // 10 seconds
        
        #endregion

        #region Core Operations
        
        /// <summary>
        /// Summon a pet for the specified owner.
        /// </summary>
        /// <param name="ownerId">Entity ID of the pet owner</param>
        /// <param name="petData">Data defining the pet to summon</param>
        /// <returns>True if pet was summoned successfully</returns>
        bool SummonPet(ulong ownerId, PetData petData);
        
        /// <summary>
        /// Dismiss the owner's current pet.
        /// </summary>
        /// <param name="ownerId">Entity ID of the pet owner</param>
        void DismissPet(ulong ownerId);
        
        /// <summary>
        /// Command the pet to attack a specific target.
        /// </summary>
        /// <param name="ownerId">Entity ID of the pet owner</param>
        /// <param name="targetId">Entity ID of the target to attack</param>
        void CommandAttack(ulong ownerId, ulong targetId);
        
        /// <summary>
        /// Command the pet to follow the owner.
        /// </summary>
        /// <param name="ownerId">Entity ID of the pet owner</param>
        void CommandFollow(ulong ownerId);
        
        #endregion

        #region Queries
        
        /// <summary>
        /// Check if an owner has an active pet.
        /// </summary>
        /// <param name="ownerId">Entity ID of the pet owner</param>
        /// <returns>True if owner has an active pet</returns>
        bool HasPet(ulong ownerId);
        
        /// <summary>
        /// Get the pet instance for an owner.
        /// </summary>
        /// <param name="ownerId">Entity ID of the pet owner</param>
        /// <returns>Pet instance or null if no pet</returns>
        PetInstance GetPet(ulong ownerId);
        
        /// <summary>
        /// Get the current health of the owner's pet.
        /// </summary>
        /// <param name="ownerId">Entity ID of the pet owner</param>
        /// <returns>Current health or 0 if no pet</returns>
        float GetPetHealth(ulong ownerId);
        
        /// <summary>
        /// Get the maximum health of the owner's pet.
        /// </summary>
        /// <param name="ownerId">Entity ID of the pet owner</param>
        /// <returns>Maximum health or 0 if no pet</returns>
        float GetPetMaxHealth(ulong ownerId);
        
        /// <summary>
        /// Check if the owner's pet is alive.
        /// </summary>
        /// <param name="ownerId">Entity ID of the pet owner</param>
        /// <returns>True if pet exists and is alive</returns>
        bool IsPetAlive(ulong ownerId);
        
        /// <summary>
        /// Get the remaining resummon cooldown for an owner.
        /// </summary>
        /// <param name="ownerId">Entity ID of the pet owner</param>
        /// <returns>Remaining cooldown in seconds, or 0 if ready</returns>
        float GetResummonCooldown(ulong ownerId);
        
        /// <summary>
        /// Check if the owner can summon a pet (not on cooldown).
        /// </summary>
        /// <param name="ownerId">Entity ID of the pet owner</param>
        /// <returns>True if summoning is allowed</returns>
        bool CanSummonPet(ulong ownerId);
        
        /// <summary>
        /// Get all active pets in the system.
        /// </summary>
        /// <returns>Read-only collection of all active pet instances</returns>
        IReadOnlyList<PetInstance> GetAllActivePets();
        
        #endregion

        #region Events
        
        /// <summary>
        /// Fired when a pet is summoned.
        /// Parameters: ownerId, petInstance
        /// </summary>
        event Action<ulong, PetInstance> OnPetSummoned;
        
        /// <summary>
        /// Fired when a pet is dismissed.
        /// Parameters: ownerId
        /// </summary>
        event Action<ulong> OnPetDismissed;
        
        /// <summary>
        /// Fired when a pet dies.
        /// Parameters: ownerId
        /// </summary>
        event Action<ulong> OnPetDied;
        
        /// <summary>
        /// Fired when a pet deals damage.
        /// Parameters: ownerId, targetId, damage
        /// </summary>
        event Action<ulong, ulong, float> OnPetDamageDealt;
        
        /// <summary>
        /// Fired when a pet's state changes.
        /// Parameters: ownerId, newState
        /// </summary>
        event Action<ulong, PetState> OnPetStateChanged;
        
        #endregion
    }
}
