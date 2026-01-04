using System;
using System.Collections.Generic;
using EtherDomes.Data;
using UnityEngine;

namespace EtherDomes.World
{
    /// <summary>
    /// Manages dungeon instances, boss progress, and group scaling.
    /// </summary>
    public class DungeonSystem : MonoBehaviour, IDungeonSystem
    {
        public const float DEFAULT_DESTROY_DELAY = 5f; // minutes
        public const int SMALL_DUNGEON_BOSSES = 3;
        public const int LARGE_DUNGEON_BOSSES = 5;
        public const float BASE_DIFFICULTY = 1f;
        public const float DIFFICULTY_PER_PLAYER = 0.1f;

        [SerializeField] private float _instanceDestroyDelayMinutes = DEFAULT_DESTROY_DELAY;

        // Active instances
        private readonly Dictionary<string, DungeonInstanceData> _instances = new();
        
        // Player to instance mapping
        private readonly Dictionary<ulong, string> _playerInstances = new();
        
        // Pending destruction
        private readonly Dictionary<string, float> _pendingDestruction = new();

        public event Action<string, int> OnBossDefeated;
        public event Action<string> OnDungeonCompleted;
        public event Action<string> OnWipe;
        public event Action<string> OnInstanceCreated;
        public event Action<string> OnInstanceDestroyed;

        public float InstanceDestroyDelayMinutes => _instanceDestroyDelayMinutes;

        private void Update()
        {
            ProcessPendingDestructions();
        }

        private void ProcessPendingDestructions()
        {
            var toDestroy = new List<string>();
            float currentTime = Time.time;

            foreach (var kvp in _pendingDestruction)
            {
                if (currentTime >= kvp.Value)
                {
                    toDestroy.Add(kvp.Key);
                }
            }

            foreach (var instanceId in toDestroy)
            {
                _pendingDestruction.Remove(instanceId);
                DestroyInstanceInternal(instanceId);
            }
        }

        public string CreateInstance(string dungeonId, ulong[] groupMembers)
        {
            if (groupMembers == null || groupMembers.Length == 0)
            {
                Debug.LogError("[DungeonSystem] Cannot create instance without group members");
                return null;
            }

            // Determine dungeon size from ID
            DungeonSize size = dungeonId.Contains("large") ? DungeonSize.Large : DungeonSize.Small;
            int bossCount = size == DungeonSize.Large ? LARGE_DUNGEON_BOSSES : SMALL_DUNGEON_BOSSES;

            // Calculate difficulty based on group size
            float difficulty = BASE_DIFFICULTY + (groupMembers.Length - 1) * DIFFICULTY_PER_PLAYER;

            string instanceId = $"{dungeonId}_{Guid.NewGuid():N}";

            var instanceData = new DungeonInstanceData
            {
                InstanceId = instanceId,
                DungeonId = dungeonId,
                Size = size,
                BossCount = bossCount,
                BossesDefeated = new bool[bossCount],
                GroupMembers = groupMembers,
                GroupSize = groupMembers.Length,
                DifficultyMultiplier = difficulty,
                CreationTime = Time.time
            };

            _instances[instanceId] = instanceData;

            Debug.Log($"[DungeonSystem] Created instance {instanceId} for {groupMembers.Length} players (Difficulty: {difficulty:F2}x)");
            OnInstanceCreated?.Invoke(instanceId);

            return instanceId;
        }


        public void EnterInstance(string instanceId, ulong playerId)
        {
            if (!_instances.TryGetValue(instanceId, out var instance))
            {
                Debug.LogError($"[DungeonSystem] Instance {instanceId} not found");
                return;
            }

            // Check if player is in group
            bool inGroup = false;
            foreach (var member in instance.GroupMembers)
            {
                if (member == playerId)
                {
                    inGroup = true;
                    break;
                }
            }

            if (!inGroup)
            {
                Debug.LogWarning($"[DungeonSystem] Player {playerId} not in instance group");
                return;
            }

            // Leave current instance if any
            if (_playerInstances.ContainsKey(playerId))
            {
                LeaveInstance(playerId);
            }

            _playerInstances[playerId] = instanceId;

            // Cancel pending destruction
            _pendingDestruction.Remove(instanceId);

            Debug.Log($"[DungeonSystem] Player {playerId} entered instance {instanceId}");
        }

        public void LeaveInstance(ulong playerId)
        {
            if (!_playerInstances.TryGetValue(playerId, out string instanceId))
            {
                return;
            }

            _playerInstances.Remove(playerId);
            Debug.Log($"[DungeonSystem] Player {playerId} left instance {instanceId}");

            // Check if instance is empty
            CheckInstanceEmpty(instanceId);
        }

        private void CheckInstanceEmpty(string instanceId)
        {
            bool hasPlayers = false;
            foreach (var kvp in _playerInstances)
            {
                if (kvp.Value == instanceId)
                {
                    hasPlayers = true;
                    break;
                }
            }

            if (!hasPlayers && !_pendingDestruction.ContainsKey(instanceId))
            {
                // Schedule destruction
                float destroyTime = Time.time + (_instanceDestroyDelayMinutes * 60f);
                _pendingDestruction[instanceId] = destroyTime;
                Debug.Log($"[DungeonSystem] Instance {instanceId} scheduled for destruction in {_instanceDestroyDelayMinutes} minutes");
            }
        }

        public void DestroyInstance(string instanceId)
        {
            // Remove all players first
            var playersToRemove = new List<ulong>();
            foreach (var kvp in _playerInstances)
            {
                if (kvp.Value == instanceId)
                {
                    playersToRemove.Add(kvp.Key);
                }
            }

            foreach (var playerId in playersToRemove)
            {
                _playerInstances.Remove(playerId);
            }

            DestroyInstanceInternal(instanceId);
        }

        private void DestroyInstanceInternal(string instanceId)
        {
            if (_instances.Remove(instanceId))
            {
                Debug.Log($"[DungeonSystem] Instance {instanceId} destroyed");
                OnInstanceDestroyed?.Invoke(instanceId);
            }
        }

        public DungeonInstanceData GetInstanceData(string instanceId)
        {
            return _instances.TryGetValue(instanceId, out var data) ? data : null;
        }

        public bool IsBossDefeated(string instanceId, int bossIndex)
        {
            if (!_instances.TryGetValue(instanceId, out var instance))
                return false;

            if (bossIndex < 0 || bossIndex >= instance.BossesDefeated.Length)
                return false;

            return instance.BossesDefeated[bossIndex];
        }

        public void MarkBossDefeated(string instanceId, int bossIndex)
        {
            if (!_instances.TryGetValue(instanceId, out var instance))
            {
                Debug.LogError($"[DungeonSystem] Instance {instanceId} not found");
                return;
            }

            if (bossIndex < 0 || bossIndex >= instance.BossesDefeated.Length)
            {
                Debug.LogError($"[DungeonSystem] Invalid boss index {bossIndex}");
                return;
            }

            if (instance.BossesDefeated[bossIndex])
            {
                Debug.LogWarning($"[DungeonSystem] Boss {bossIndex} already defeated");
                return;
            }

            instance.BossesDefeated[bossIndex] = true;
            Debug.Log($"[DungeonSystem] Boss {bossIndex} defeated in instance {instanceId}");
            OnBossDefeated?.Invoke(instanceId, bossIndex);

            // Check for completion
            if (IsCompleted(instanceId))
            {
                Debug.Log($"[DungeonSystem] Dungeon {instanceId} completed!");
                OnDungeonCompleted?.Invoke(instanceId);
            }
        }

        public bool IsCompleted(string instanceId)
        {
            if (!_instances.TryGetValue(instanceId, out var instance))
                return false;

            foreach (bool defeated in instance.BossesDefeated)
            {
                if (!defeated) return false;
            }
            return true;
        }

        /// <summary>
        /// Handle a wipe (all players dead).
        /// </summary>
        public void HandleWipe(string instanceId)
        {
            if (!_instances.ContainsKey(instanceId))
                return;

            Debug.Log($"[DungeonSystem] WIPE in instance {instanceId}");
            OnWipe?.Invoke(instanceId);
        }

        /// <summary>
        /// Get the instance a player is currently in.
        /// </summary>
        public string GetPlayerInstance(ulong playerId)
        {
            return _playerInstances.TryGetValue(playerId, out string instanceId) ? instanceId : null;
        }

        /// <summary>
        /// Get all players in an instance.
        /// </summary>
        public ulong[] GetInstancePlayers(string instanceId)
        {
            var players = new List<ulong>();
            foreach (var kvp in _playerInstances)
            {
                if (kvp.Value == instanceId)
                {
                    players.Add(kvp.Key);
                }
            }
            return players.ToArray();
        }

        /// <summary>
        /// Get difficulty multiplier for an instance.
        /// </summary>
        public float GetDifficultyMultiplier(string instanceId)
        {
            return _instances.TryGetValue(instanceId, out var instance) 
                ? instance.DifficultyMultiplier 
                : BASE_DIFFICULTY;
        }
    }
}
