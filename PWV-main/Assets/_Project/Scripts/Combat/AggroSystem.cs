using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Manages threat/aggro for enemies in combat.
    /// </summary>
    public class AggroSystem : MonoBehaviour, IAggroSystem
    {
        public const float DEFAULT_MELEE_THRESHOLD = 1.1f;  // 110%
        public const float DEFAULT_RANGED_THRESHOLD = 1.3f; // 130%
        public const float DEFAULT_HEALING_MULTIPLIER = 0.5f; // 50%
        public const float TAUNT_BONUS = 1.1f; // 110% of highest

        [SerializeField] private float _meleeThreatThreshold = DEFAULT_MELEE_THRESHOLD;
        [SerializeField] private float _rangedThreatThreshold = DEFAULT_RANGED_THRESHOLD;
        [SerializeField] private float _healingThreatMultiplier = DEFAULT_HEALING_MULTIPLIER;

        // enemyId -> (playerId -> threat)
        private readonly Dictionary<ulong, Dictionary<ulong, float>> _threatTables = 
            new Dictionary<ulong, Dictionary<ulong, float>>();

        // enemyId -> current target playerId
        private readonly Dictionary<ulong, ulong> _currentTargets = 
            new Dictionary<ulong, ulong>();

        public event Action<ulong, ulong> OnAggroChanged;

        public float MeleeThreatThreshold => _meleeThreatThreshold;
        public float RangedThreatThreshold => _rangedThreatThreshold;
        public float HealingThreatMultiplier => _healingThreatMultiplier;

        public void AddThreat(ulong playerId, ulong enemyId, float amount)
        {
            if (amount <= 0) return;

            EnsureThreatTable(enemyId);
            
            if (!_threatTables[enemyId].ContainsKey(playerId))
                _threatTables[enemyId][playerId] = 0;

            _threatTables[enemyId][playerId] += amount;

            Debug.Log($"[AggroSystem] +{amount:F0} threat: Player {playerId} -> Enemy {enemyId} (Total: {_threatTables[enemyId][playerId]:F0})");

            CheckAggroSwitch(enemyId);
        }

        public void Taunt(ulong playerId, ulong enemyId)
        {
            EnsureThreatTable(enemyId);

            float highestThreat = GetHighestThreat(enemyId);
            float newThreat = highestThreat * TAUNT_BONUS;

            _threatTables[enemyId][playerId] = newThreat;

            Debug.Log($"[AggroSystem] Taunt: Player {playerId} -> Enemy {enemyId} (Threat: {newThreat:F0})");

            // Force aggro switch
            SetCurrentTarget(enemyId, playerId);
        }

        public void AddHealingThreat(ulong healerId, float healAmount, ulong[] engagedEnemies)
        {
            if (engagedEnemies == null || engagedEnemies.Length == 0)
                return;

            float totalThreat = healAmount * _healingThreatMultiplier;
            float threatPerEnemy = totalThreat / engagedEnemies.Length;

            foreach (var enemyId in engagedEnemies)
            {
                AddThreat(healerId, enemyId, threatPerEnemy);
            }

            Debug.Log($"[AggroSystem] Healing threat: {totalThreat:F0} split among {engagedEnemies.Length} enemies");
        }


        public void ResetThreat(ulong enemyId)
        {
            if (_threatTables.ContainsKey(enemyId))
            {
                _threatTables[enemyId].Clear();
                Debug.Log($"[AggroSystem] Threat reset for enemy {enemyId}");
            }

            if (_currentTargets.ContainsKey(enemyId))
            {
                _currentTargets.Remove(enemyId);
            }
        }

        public ulong GetHighestThreatPlayer(ulong enemyId)
        {
            if (!_threatTables.ContainsKey(enemyId) || _threatTables[enemyId].Count == 0)
                return 0;

            return _threatTables[enemyId]
                .OrderByDescending(kvp => kvp.Value)
                .First()
                .Key;
        }

        public float GetThreat(ulong playerId, ulong enemyId)
        {
            if (!_threatTables.ContainsKey(enemyId))
                return 0;

            return _threatTables[enemyId].TryGetValue(playerId, out float threat) ? threat : 0;
        }

        public Dictionary<ulong, float> GetThreatTable(ulong enemyId)
        {
            if (!_threatTables.ContainsKey(enemyId))
                return new Dictionary<ulong, float>();

            return new Dictionary<ulong, float>(_threatTables[enemyId]);
        }

        public bool ShouldPullAggro(ulong playerId, ulong enemyId, bool isMelee)
        {
            if (!_currentTargets.TryGetValue(enemyId, out ulong currentTarget))
                return true; // No current target, anyone can pull

            if (currentTarget == playerId)
                return false; // Already has aggro

            float currentTargetThreat = GetThreat(currentTarget, enemyId);
            float playerThreat = GetThreat(playerId, enemyId);

            if (currentTargetThreat <= 0)
                return playerThreat > 0;

            float threshold = isMelee ? _meleeThreatThreshold : _rangedThreatThreshold;
            return playerThreat >= currentTargetThreat * threshold;
        }

        private void EnsureThreatTable(ulong enemyId)
        {
            if (!_threatTables.ContainsKey(enemyId))
            {
                _threatTables[enemyId] = new Dictionary<ulong, float>();
            }
        }

        private float GetHighestThreat(ulong enemyId)
        {
            if (!_threatTables.ContainsKey(enemyId) || _threatTables[enemyId].Count == 0)
                return 0;

            return _threatTables[enemyId].Values.Max();
        }

        private void CheckAggroSwitch(ulong enemyId)
        {
            ulong highestThreatPlayer = GetHighestThreatPlayer(enemyId);
            
            if (highestThreatPlayer == 0)
                return;

            if (!_currentTargets.TryGetValue(enemyId, out ulong currentTarget))
            {
                // No current target, set to highest threat
                SetCurrentTarget(enemyId, highestThreatPlayer);
                return;
            }

            // Check if highest threat player should pull aggro
            // Use melee threshold as default (more conservative)
            if (ShouldPullAggro(highestThreatPlayer, enemyId, true))
            {
                SetCurrentTarget(enemyId, highestThreatPlayer);
            }
        }

        private void SetCurrentTarget(ulong enemyId, ulong playerId)
        {
            ulong previousTarget = _currentTargets.TryGetValue(enemyId, out ulong prev) ? prev : 0;
            
            if (previousTarget == playerId)
                return;

            _currentTargets[enemyId] = playerId;
            
            Debug.Log($"[AggroSystem] Aggro switch: Enemy {enemyId} now targeting Player {playerId}");
            OnAggroChanged?.Invoke(enemyId, playerId);
        }

        /// <summary>
        /// Get the current target of an enemy.
        /// </summary>
        public ulong GetCurrentTarget(ulong enemyId)
        {
            return _currentTargets.TryGetValue(enemyId, out ulong target) ? target : 0;
        }

        /// <summary>
        /// Clear all threat data (e.g., when combat ends globally).
        /// </summary>
        public void ClearAll()
        {
            _threatTables.Clear();
            _currentTargets.Clear();
        }
    }
}
