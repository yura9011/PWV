using System;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Reason why stealth was broken.
    /// </summary>
    public enum StealthBreakReason
    {
        Attack,         // Player attacked
        DamageReceived, // Player received damage
        AbilityUsed,    // Player used an ability that breaks stealth
        Manual          // Player manually exited stealth
    }

    /// <summary>
    /// Interface for the stealth system handling invisibility, stealth abilities, and movement speed.
    /// Requirements: 4.1, 4.4, 4.6
    /// </summary>
    public interface IStealthSystem
    {
        #region Constants
        
        /// <summary>
        /// Movement speed multiplier while in stealth (70%).
        /// Requirements: 4.4
        /// </summary>
        float MovementSpeedMultiplier { get; } // 0.7
        
        /// <summary>
        /// Cooldown before re-entering stealth after it breaks (in seconds).
        /// Requirements: 4.6
        /// </summary>
        float StealthCooldown { get; } // 2 seconds
        
        /// <summary>
        /// Opacity for local player while in stealth (30%).
        /// Requirements: 4.7
        /// </summary>
        float LocalPlayerOpacity { get; } // 0.3
        
        /// <summary>
        /// Opacity for enemies viewing a stealthed player (0%).
        /// Requirements: 4.7
        /// </summary>
        float EnemyViewOpacity { get; } // 0.0
        
        #endregion

        #region Core Operations
        
        /// <summary>
        /// Attempt to enter stealth for a player.
        /// Requirements: 4.1
        /// </summary>
        /// <param name="playerId">Entity ID of the player</param>
        /// <returns>True if stealth was entered successfully</returns>
        bool TryEnterStealth(ulong playerId);
        
        /// <summary>
        /// Break stealth for a player with a specified reason.
        /// Requirements: 4.2, 4.3
        /// </summary>
        /// <param name="playerId">Entity ID of the player</param>
        /// <param name="reason">Reason for breaking stealth</param>
        void BreakStealth(ulong playerId, StealthBreakReason reason);
        
        /// <summary>
        /// Manually exit stealth (toggle off).
        /// </summary>
        /// <param name="playerId">Entity ID of the player</param>
        void ExitStealth(ulong playerId);
        
        /// <summary>
        /// Notify the system that a player received damage.
        /// This will break stealth if the player is stealthed.
        /// Requirements: 4.3
        /// </summary>
        /// <param name="playerId">Entity ID of the player</param>
        void OnDamageReceived(ulong playerId);
        
        /// <summary>
        /// Notify the system that a player performed an attack.
        /// This will break stealth if the player is stealthed.
        /// Requirements: 4.2
        /// </summary>
        /// <param name="playerId">Entity ID of the player</param>
        void OnAttackPerformed(ulong playerId);
        
        #endregion

        #region Queries
        
        /// <summary>
        /// Check if a player is currently in stealth.
        /// </summary>
        /// <param name="playerId">Entity ID of the player</param>
        /// <returns>True if player is in stealth</returns>
        bool IsInStealth(ulong playerId);
        
        /// <summary>
        /// Get the remaining cooldown before a player can enter stealth.
        /// Requirements: 4.6
        /// </summary>
        /// <param name="playerId">Entity ID of the player</param>
        /// <returns>Remaining cooldown in seconds, or 0 if ready</returns>
        float GetStealthCooldownRemaining(ulong playerId);
        
        /// <summary>
        /// Check if a player can enter stealth (not on cooldown).
        /// Requirements: 4.6
        /// </summary>
        /// <param name="playerId">Entity ID of the player</param>
        /// <returns>True if stealth can be entered</returns>
        bool CanEnterStealth(ulong playerId);
        
        /// <summary>
        /// Check if a player can use a stealth-requiring ability.
        /// Requirements: 4.5
        /// </summary>
        /// <param name="playerId">Entity ID of the player</param>
        /// <param name="abilityId">ID of the ability to check</param>
        /// <returns>True if the ability can be used</returns>
        bool CanUseStealthAbility(ulong playerId, string abilityId);
        
        /// <summary>
        /// Get the effective movement speed multiplier for a player.
        /// Returns 1.0 if not in stealth, MovementSpeedMultiplier if in stealth.
        /// Requirements: 4.4
        /// </summary>
        /// <param name="playerId">Entity ID of the player</param>
        /// <returns>Movement speed multiplier (0.7 in stealth, 1.0 otherwise)</returns>
        float GetMovementSpeedMultiplier(ulong playerId);
        
        #endregion

        #region Events
        
        /// <summary>
        /// Fired when a player enters stealth.
        /// Parameters: playerId
        /// Requirements: 4.1
        /// </summary>
        event Action<ulong> OnStealthEntered;
        
        /// <summary>
        /// Fired when a player's stealth is broken.
        /// Parameters: playerId, reason
        /// Requirements: 4.2, 4.3
        /// </summary>
        event Action<ulong, StealthBreakReason> OnStealthBroken;
        
        #endregion
    }
}
