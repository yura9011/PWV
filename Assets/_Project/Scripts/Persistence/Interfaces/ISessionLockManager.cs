using System;

namespace EtherDomes.Persistence
{
    /// <summary>
    /// Interface for managing session locks to prevent simultaneous use of the same character.
    /// Implements character protection against duplication exploits.
    /// </summary>
    public interface ISessionLockManager
    {
        /// <summary>
        /// Attempts to acquire a lock for the specified character.
        /// </summary>
        /// <param name="characterId">The unique identifier of the character.</param>
        /// <returns>True if lock was acquired, false if character is already locked.</returns>
        bool TryAcquireLock(string characterId);

        /// <summary>
        /// Releases the lock for the specified character.
        /// </summary>
        /// <param name="characterId">The unique identifier of the character.</param>
        void ReleaseLock(string characterId);

        /// <summary>
        /// Checks if a character is currently locked.
        /// </summary>
        /// <param name="characterId">The unique identifier of the character.</param>
        /// <returns>True if the character is locked, false otherwise.</returns>
        bool IsLocked(string characterId);

        /// <summary>
        /// Cleans up stale locks that are older than the specified age.
        /// Used to recover from crashes where locks weren't properly released.
        /// </summary>
        /// <param name="maxAge">Maximum age of a lock before it's considered stale.</param>
        void CleanupStaleLocks(TimeSpan maxAge);

        /// <summary>
        /// Releases all locks held by this session.
        /// Should be called on application quit.
        /// </summary>
        void ReleaseAllLocks();

        /// <summary>
        /// Event fired when a lock is successfully acquired.
        /// </summary>
        event Action<string> OnLockAcquired;

        /// <summary>
        /// Event fired when a lock is released.
        /// </summary>
        event Action<string> OnLockReleased;

        /// <summary>
        /// Event fired when a lock acquisition is denied (character already in use).
        /// </summary>
        event Action<string> OnLockDenied;

        /// <summary>
        /// The threshold after which a lock is considered stale (default: 5 minutes).
        /// </summary>
        TimeSpan StaleLockThreshold { get; }
    }
}
