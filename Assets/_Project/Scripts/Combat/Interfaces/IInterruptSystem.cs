using System;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Interface for the interrupt system.
    /// Requirements: 9.6, 9.7
    /// </summary>
    public interface IInterruptSystem
    {
        /// <summary>
        /// Default lockout duration in seconds.
        /// </summary>
        const float DEFAULT_LOCKOUT_DURATION = 4f;

        /// <summary>
        /// Try to interrupt a target's cast.
        /// </summary>
        bool TryInterrupt(ulong interrupterId, ulong targetId);

        /// <summary>
        /// Check if a target is locked out from casting.
        /// </summary>
        bool IsLockedOut(ulong targetId);

        /// <summary>
        /// Get remaining lockout time for a target.
        /// </summary>
        float GetLockoutRemaining(ulong targetId);

        /// <summary>
        /// Apply lockout to a target.
        /// </summary>
        void ApplyLockout(ulong targetId, float duration);

        /// <summary>
        /// Clear lockout for a target.
        /// </summary>
        void ClearLockout(ulong targetId);

        /// <summary>
        /// Event fired when a cast is interrupted.
        /// </summary>
        event Action<ulong, ulong> OnCastInterrupted; // interrupterId, targetId

        /// <summary>
        /// Event fired when lockout is applied.
        /// </summary>
        event Action<ulong, float> OnLockoutApplied; // targetId, duration

        /// <summary>
        /// Event fired when lockout expires.
        /// </summary>
        event Action<ulong> OnLockoutExpired; // targetId
    }
}
