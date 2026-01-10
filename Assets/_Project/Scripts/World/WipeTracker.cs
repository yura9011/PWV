using System;
using System.Collections.Generic;
using UnityEngine;

namespace EtherDomes.World
{
    /// <summary>
    /// Tracks wipes in dungeon instances and handles group expulsion.
    /// 
    /// - Counts wipes per instance
    /// - Respawns group at dungeon entrance on wipe
    /// - Expels and resets instance after 3 wipes
    /// 
    /// Requirements: 5.4, 5.5
    /// </summary>
    public class WipeTracker : IWipeTracker
    {
        public const int DEFAULT_MAX_WIPES = 3;

        private readonly Dictionary<string, int> _wipeCounts = new();
        private readonly int _maxWipes;

        public int MaxWipesBeforeExpulsion => _maxWipes;

        public event Action<string, int> OnWipeRecorded;
        public event Action<string> OnGroupExpelled;

        public WipeTracker() : this(DEFAULT_MAX_WIPES) { }

        public WipeTracker(int maxWipes)
        {
            _maxWipes = maxWipes;
        }

        public int GetWipeCount(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId))
                return 0;

            return _wipeCounts.TryGetValue(instanceId, out int count) ? count : 0;
        }

        public void RecordWipe(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId))
                return;

            if (!_wipeCounts.ContainsKey(instanceId))
            {
                _wipeCounts[instanceId] = 0;
            }

            _wipeCounts[instanceId]++;
            int currentCount = _wipeCounts[instanceId];

            Debug.Log($"[WipeTracker] Wipe recorded for instance {instanceId}. Count: {currentCount}/{_maxWipes}");
            OnWipeRecorded?.Invoke(instanceId, currentCount);

            // Check for expulsion
            if (ShouldExpelGroup(instanceId))
            {
                Debug.Log($"[WipeTracker] Group expelled from instance {instanceId} after {currentCount} wipes");
                OnGroupExpelled?.Invoke(instanceId);
            }
        }

        public bool ShouldExpelGroup(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId))
                return false;

            int wipeCount = GetWipeCount(instanceId);
            return wipeCount >= _maxWipes;
        }

        public void ResetWipeCount(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId))
                return;

            _wipeCounts[instanceId] = 0;
            Debug.Log($"[WipeTracker] Wipe count reset for instance {instanceId}");
        }

        /// <summary>
        /// Clears all tracked instances.
        /// </summary>
        public void ClearAll()
        {
            _wipeCounts.Clear();
        }
    }
}
