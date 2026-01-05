using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace EtherDomes.Persistence
{
    /// <summary>
    /// Manages session locks using lock files to prevent simultaneous use of the same character.
    /// Lock files are stored in Application.persistentDataPath/Locks/
    /// </summary>
    public class SessionLockManager : ISessionLockManager
    {
        private const string LOCKS_FOLDER = "Locks";
        private const string LOCK_EXTENSION = ".lock";
        
        private readonly string _locksPath;
        private readonly HashSet<string> _ownedLocks;
        private readonly TimeSpan _staleLockThreshold;

        public event Action<string> OnLockAcquired;
        public event Action<string> OnLockReleased;
        public event Action<string> OnLockDenied;

        public TimeSpan StaleLockThreshold => _staleLockThreshold;

        public SessionLockManager() : this(TimeSpan.FromMinutes(5))
        {
        }

        public SessionLockManager(TimeSpan staleLockThreshold)
        {
            _staleLockThreshold = staleLockThreshold;
            _locksPath = Path.Combine(Application.persistentDataPath, LOCKS_FOLDER);
            _ownedLocks = new HashSet<string>();
            
            EnsureLocksDirectoryExists();
        }

        private void EnsureLocksDirectoryExists()
        {
            if (!Directory.Exists(_locksPath))
            {
                Directory.CreateDirectory(_locksPath);
                Debug.Log($"[SessionLockManager] Created locks directory: {_locksPath}");
            }
        }

        public bool TryAcquireLock(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
            {
                Debug.LogError("[SessionLockManager] Cannot acquire lock: characterId is null or empty");
                return false;
            }

            // Check if we already own this lock (mutual exclusion - same instance can't acquire twice)
            if (_ownedLocks.Contains(characterId))
            {
                Debug.LogWarning($"[SessionLockManager] Lock denied - already owned by this session: {characterId}");
                OnLockDenied?.Invoke(characterId);
                return false;
            }

            var lockFilePath = GetLockFilePath(characterId);

            // Check if lock file exists
            if (File.Exists(lockFilePath))
            {
                // Check if it's a stale lock
                if (IsLockStale(lockFilePath))
                {
                    Debug.Log($"[SessionLockManager] Removing stale lock for character: {characterId}");
                    DeleteLockFile(lockFilePath);
                }
                else
                {
                    Debug.LogWarning($"[SessionLockManager] Lock denied - character in use: {characterId}");
                    OnLockDenied?.Invoke(characterId);
                    return false;
                }
            }

            // Create lock file with timestamp
            try
            {
                var lockData = new LockFileData
                {
                    CharacterId = characterId,
                    LockTimeTicks = DateTime.UtcNow.Ticks,
                    ProcessId = System.Diagnostics.Process.GetCurrentProcess().Id,
                    MachineName = Environment.MachineName
                };

                var json = JsonUtility.ToJson(lockData);
                File.WriteAllText(lockFilePath, json);
                
                _ownedLocks.Add(characterId);
                Debug.Log($"[SessionLockManager] Lock acquired for character: {characterId}");
                OnLockAcquired?.Invoke(characterId);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SessionLockManager] Failed to create lock file: {ex.Message}");
                return false;
            }
        }

        public void ReleaseLock(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
                return;

            var lockFilePath = GetLockFilePath(characterId);
            
            if (DeleteLockFile(lockFilePath))
            {
                _ownedLocks.Remove(characterId);
                Debug.Log($"[SessionLockManager] Lock released for character: {characterId}");
                OnLockReleased?.Invoke(characterId);
            }
        }

        public bool IsLocked(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
                return false;

            // First check if we own this lock (fast path)
            if (_ownedLocks.Contains(characterId))
                return true;

            // Then check if lock file exists (for locks from other processes)
            var lockFilePath = GetLockFilePath(characterId);
            
            if (!File.Exists(lockFilePath))
                return false;

            // Check if stale
            if (IsLockStale(lockFilePath))
            {
                DeleteLockFile(lockFilePath);
                return false;
            }

            return true;
        }

        public void CleanupStaleLocks(TimeSpan maxAge)
        {
            if (!Directory.Exists(_locksPath))
                return;

            var lockFiles = Directory.GetFiles(_locksPath, $"*{LOCK_EXTENSION}");
            int cleanedCount = 0;

            foreach (var lockFile in lockFiles)
            {
                if (IsLockStale(lockFile, maxAge))
                {
                    // Extract characterId from filename to clean up _ownedLocks
                    var fileName = Path.GetFileNameWithoutExtension(lockFile);
                    _ownedLocks.Remove(fileName);
                    
                    DeleteLockFile(lockFile);
                    cleanedCount++;
                }
            }

            if (cleanedCount > 0)
            {
                Debug.Log($"[SessionLockManager] Cleaned up {cleanedCount} stale locks");
            }
        }

        public void ReleaseAllLocks()
        {
            var locksToRelease = new List<string>(_ownedLocks);
            foreach (var characterId in locksToRelease)
            {
                ReleaseLock(characterId);
            }
            Debug.Log($"[SessionLockManager] Released all locks ({locksToRelease.Count} total)");
        }

        private string GetLockFilePath(string characterId)
        {
            // Sanitize characterId for use as filename
            var safeId = characterId.Replace("/", "_").Replace("\\", "_");
            return Path.Combine(_locksPath, $"{safeId}{LOCK_EXTENSION}");
        }

        private bool IsLockStale(string lockFilePath)
        {
            return IsLockStale(lockFilePath, _staleLockThreshold);
        }

        private bool IsLockStale(string lockFilePath, TimeSpan maxAge)
        {
            try
            {
                var json = File.ReadAllText(lockFilePath);
                var lockData = JsonUtility.FromJson<LockFileData>(json);
                
                // Convert ticks back to DateTime
                var lockTime = new DateTime(lockData.LockTimeTicks, DateTimeKind.Utc);
                var lockAge = DateTime.UtcNow - lockTime;
                return lockAge > maxAge;
            }
            catch
            {
                // If we can't read the lock file, consider it stale
                return true;
            }
        }

        private bool DeleteLockFile(string lockFilePath)
        {
            try
            {
                if (File.Exists(lockFilePath))
                {
                    File.Delete(lockFilePath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SessionLockManager] Failed to delete lock file: {ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// Data stored in lock files for tracking and stale detection.
        /// JsonUtility doesn't serialize DateTime properly, so we use ticks.
        /// </summary>
        [Serializable]
        private class LockFileData
        {
            public string CharacterId;
            public long LockTimeTicks; // DateTime.UtcNow.Ticks - JsonUtility can't serialize DateTime
            public int ProcessId;
            public string MachineName;
        }
    }
}
