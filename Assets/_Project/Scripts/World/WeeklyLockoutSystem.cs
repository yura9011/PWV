using System;
using System.Collections.Generic;
using UnityEngine;

namespace EtherDomes.World
{
    /// <summary>
    /// Tracks weekly boss lockouts for players.
    /// 
    /// - Records boss kills per player
    /// - Resets every Monday at 00:00 UTC
    /// - Locked players can participate but get no loot
    /// 
    /// Requirements: 5.7, 5.8
    /// </summary>
    public class WeeklyLockoutSystem : IWeeklyLockoutSystem
    {
        private readonly Dictionary<ulong, HashSet<string>> _playerLockouts = new();
        private DateTime _lastResetTime;

        public event Action<ulong, string> OnBossKillRecorded;
        public event Action OnLockoutsReset;

        public WeeklyLockoutSystem()
        {
            _lastResetTime = GetLastMondayMidnightUtc();
        }

        public bool IsLockedOut(ulong playerId, string bossId)
        {
            if (string.IsNullOrEmpty(bossId))
                return false;

            CheckAndPerformReset();

            if (!_playerLockouts.TryGetValue(playerId, out var lockedBosses))
                return false;

            return lockedBosses.Contains(bossId);
        }

        public void RecordKill(ulong playerId, string bossId)
        {
            if (string.IsNullOrEmpty(bossId))
                return;

            CheckAndPerformReset();

            if (!_playerLockouts.ContainsKey(playerId))
            {
                _playerLockouts[playerId] = new HashSet<string>();
            }

            if (_playerLockouts[playerId].Add(bossId))
            {
                Debug.Log($"[WeeklyLockout] Player {playerId} locked to boss {bossId}");
                OnBossKillRecorded?.Invoke(playerId, bossId);
            }
        }

        public DateTime GetResetTime()
        {
            return GetNextMondayMidnightUtc();
        }

        public List<string> GetLockedBosses(ulong playerId)
        {
            CheckAndPerformReset();

            if (_playerLockouts.TryGetValue(playerId, out var lockedBosses))
            {
                return new List<string>(lockedBosses);
            }

            return new List<string>();
        }

        public void CheckAndPerformReset()
        {
            var now = DateTime.UtcNow;
            var nextReset = GetNextMondayMidnightUtc(_lastResetTime);

            if (now >= nextReset)
            {
                PerformReset();
                _lastResetTime = GetLastMondayMidnightUtc();
            }
        }

        private void PerformReset()
        {
            Debug.Log("[WeeklyLockout] Performing weekly reset");
            _playerLockouts.Clear();
            OnLockoutsReset?.Invoke();
        }

        /// <summary>
        /// Gets the last Monday at 00:00 UTC.
        /// </summary>
        private static DateTime GetLastMondayMidnightUtc()
        {
            var now = DateTime.UtcNow;
            int daysUntilMonday = ((int)now.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            var lastMonday = now.Date.AddDays(-daysUntilMonday);
            return lastMonday;
        }

        /// <summary>
        /// Gets the next Monday at 00:00 UTC.
        /// </summary>
        private static DateTime GetNextMondayMidnightUtc()
        {
            return GetNextMondayMidnightUtc(DateTime.UtcNow);
        }

        /// <summary>
        /// Gets the next Monday at 00:00 UTC from a given date.
        /// </summary>
        private static DateTime GetNextMondayMidnightUtc(DateTime from)
        {
            int daysUntilMonday = ((int)DayOfWeek.Monday - (int)from.DayOfWeek + 7) % 7;
            if (daysUntilMonday == 0)
                daysUntilMonday = 7; // If today is Monday, get next Monday
            
            return from.Date.AddDays(daysUntilMonday);
        }

        /// <summary>
        /// Loads lockout data for a player (from CharacterData).
        /// </summary>
        public void LoadPlayerLockouts(ulong playerId, List<string> lockedBossIds)
        {
            if (lockedBossIds == null || lockedBossIds.Count == 0)
                return;

            _playerLockouts[playerId] = new HashSet<string>(lockedBossIds);
        }

        /// <summary>
        /// Gets lockout data for saving to CharacterData.
        /// </summary>
        public List<string> GetPlayerLockoutsForSave(ulong playerId)
        {
            return GetLockedBosses(playerId);
        }

        /// <summary>
        /// Forces a reset (for testing).
        /// </summary>
        public void ForceReset()
        {
            PerformReset();
        }
    }
}
