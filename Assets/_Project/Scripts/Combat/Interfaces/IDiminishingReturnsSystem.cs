using System;
using EtherDomes.Data;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Interface for the Diminishing Returns system that tracks CC applications
    /// and reduces their effectiveness over time.
    /// Requirements: 11.6, 11.7
    /// </summary>
    public interface IDiminishingReturnsSystem
    {
        #region Constants
        
        /// <summary>
        /// Time in seconds before DR resets for a CC type.
        /// </summary>
        float DRResetTime { get; } // 15 seconds
        
        /// <summary>
        /// Duration of immunity after 3 applications.
        /// </summary>
        float ImmunityDuration { get; } // 15 seconds
        
        #endregion

        #region Core Operations
        
        /// <summary>
        /// Calculate the effective duration of a CC effect after applying DR.
        /// This also records the application for future DR calculations.
        /// </summary>
        /// <param name="targetId">Entity receiving the CC</param>
        /// <param name="ccType">Type of CC being applied</param>
        /// <param name="baseDuration">Original duration of the CC</param>
        /// <returns>Effective duration after DR (0 if immune)</returns>
        float ApplyDiminishingReturns(ulong targetId, CCType ccType, float baseDuration);
        
        /// <summary>
        /// Check if a target is immune to a specific CC type.
        /// </summary>
        /// <param name="targetId">Entity to check</param>
        /// <param name="ccType">Type of CC to check</param>
        /// <returns>True if immune</returns>
        bool IsImmune(ulong targetId, CCType ccType);
        
        /// <summary>
        /// Get the current DR level for a target and CC type.
        /// 0 = no DR (100%), 1 = first DR (50%), 2 = second DR (25%), 3+ = immune
        /// </summary>
        int GetDRLevel(ulong targetId, CCType ccType);
        
        /// <summary>
        /// Get the duration multiplier for the current DR level.
        /// </summary>
        float GetDurationMultiplier(ulong targetId, CCType ccType);
        
        /// <summary>
        /// Get the remaining immunity time for a target and CC type.
        /// Returns 0 if not immune.
        /// </summary>
        float GetImmunityRemaining(ulong targetId, CCType ccType);
        
        /// <summary>
        /// Clear all DR tracking for an entity (e.g., on death or zone change).
        /// </summary>
        void ClearDR(ulong targetId);
        
        /// <summary>
        /// Update the system (call from MonoBehaviour.Update).
        /// </summary>
        void Update(float deltaTime);
        
        #endregion

        #region Events
        
        /// <summary>
        /// Fired when DR is applied to a CC effect.
        /// Parameters: targetId, ccType, drLevel, effectiveDuration
        /// </summary>
        event Action<ulong, CCType, int, float> OnDRApplied;
        
        /// <summary>
        /// Fired when a target becomes immune to a CC type.
        /// Parameters: targetId, ccType
        /// </summary>
        event Action<ulong, CCType> OnImmunityStarted;
        
        /// <summary>
        /// Fired when immunity expires for a CC type.
        /// Parameters: targetId, ccType
        /// </summary>
        event Action<ulong, CCType> OnImmunityExpired;
        
        /// <summary>
        /// Fired when DR resets for a CC type (15s without that CC).
        /// Parameters: targetId, ccType
        /// </summary>
        event Action<ulong, CCType> OnDRReset;
        
        #endregion
    }
}
