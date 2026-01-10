using System;
using System.Collections.Generic;
using EtherDomes.Data;
using EtherDomes.Progression;
using UnityEngine;

namespace EtherDomes.World
{
    /// <summary>
    /// Manages boss encounters, phases, and loot generation.
    /// </summary>
    public class BossSystem : MonoBehaviour, IBossSystem
    {
        // Phase thresholds (health percentages)
        public const float PHASE_2_THRESHOLD = 0.75f;
        public const float PHASE_3_THRESHOLD = 0.50f;
        public const float PHASE_4_THRESHOLD = 0.25f;

        private IDungeonSystem _dungeonSystem;
        private ILootSystem _lootSystem;

        // Active encounters
        private readonly Dictionary<(string, int), BossEncounterData> _activeEncounters = new();

        public event Action<string, int> OnEncounterStarted;
        public event Action<string, int, BossPhase> OnPhaseTransition;
        public event Action<string, int, bool> OnEncounterEnded;

        public void Initialize(IDungeonSystem dungeonSystem, ILootSystem lootSystem)
        {
            _dungeonSystem = dungeonSystem;
            _lootSystem = lootSystem;
        }

        public void StartEncounter(string instanceId, int bossIndex)
        {
            var key = (instanceId, bossIndex);
            
            if (_activeEncounters.ContainsKey(key))
            {
                Debug.LogWarning($"[BossSystem] Encounter already active: {instanceId} boss {bossIndex}");
                return;
            }

            // Get boss data (would come from BossDataSO in production)
            float maxHealth = GetBossMaxHealth(instanceId, bossIndex);

            var encounter = new BossEncounterData
            {
                InstanceId = instanceId,
                BossIndex = bossIndex,
                CurrentHealth = maxHealth,
                MaxHealth = maxHealth,
                CurrentPhase = BossPhase.Phase1,
                StartTime = Time.time
            };

            _activeEncounters[key] = encounter;

            Debug.Log($"[BossSystem] Encounter started: {instanceId} boss {bossIndex}");
            OnEncounterStarted?.Invoke(instanceId, bossIndex);
        }

        public void EndEncounter(string instanceId, int bossIndex, bool victory)
        {
            var key = (instanceId, bossIndex);

            if (!_activeEncounters.TryGetValue(key, out var encounter))
            {
                Debug.LogWarning($"[BossSystem] No active encounter: {instanceId} boss {bossIndex}");
                return;
            }

            _activeEncounters.Remove(key);

            if (victory)
            {
                // Mark boss as defeated in dungeon
                _dungeonSystem?.MarkBossDefeated(instanceId, bossIndex);

                // Generate loot
                var instanceData = _dungeonSystem?.GetInstanceData(instanceId);
                if (instanceData != null && _lootSystem != null)
                {
                    var loot = _lootSystem.GenerateLoot(
                        $"{instanceData.DungeonId}_boss{bossIndex}",
                        instanceData.GroupSize
                    );
                    _lootSystem.DistributeLoot(loot, instanceData.GroupMembers, LootDistributionMode.RoundRobin);
                }
            }

            float duration = Time.time - encounter.StartTime;
            Debug.Log($"[BossSystem] Encounter ended: {instanceId} boss {bossIndex} - {(victory ? "Victory" : "Defeat")} ({duration:F1}s)");
            OnEncounterEnded?.Invoke(instanceId, bossIndex, victory);
        }

        public BossPhase GetCurrentPhase(string instanceId, int bossIndex)
        {
            var key = (instanceId, bossIndex);
            return _activeEncounters.TryGetValue(key, out var encounter) 
                ? encounter.CurrentPhase 
                : BossPhase.Phase1;
        }

        public float GetHealthPercent(string instanceId, int bossIndex)
        {
            var key = (instanceId, bossIndex);
            if (!_activeEncounters.TryGetValue(key, out var encounter))
                return 1f;

            return encounter.MaxHealth > 0 
                ? encounter.CurrentHealth / encounter.MaxHealth 
                : 0f;
        }


        /// <summary>
        /// Apply damage to a boss and check for phase transitions.
        /// </summary>
        public void ApplyDamage(string instanceId, int bossIndex, float damage)
        {
            var key = (instanceId, bossIndex);
            if (!_activeEncounters.TryGetValue(key, out var encounter))
                return;

            encounter.CurrentHealth = Mathf.Max(0, encounter.CurrentHealth - damage);
            float healthPercent = GetHealthPercent(instanceId, bossIndex);

            // Check for phase transitions
            CheckPhaseTransition(encounter, healthPercent);

            // Check for death
            if (encounter.CurrentHealth <= 0)
            {
                EndEncounter(instanceId, bossIndex, true);
            }
        }

        private void CheckPhaseTransition(BossEncounterData encounter, float healthPercent)
        {
            BossPhase newPhase = encounter.CurrentPhase;

            if (healthPercent <= PHASE_4_THRESHOLD && encounter.CurrentPhase != BossPhase.Phase4)
            {
                newPhase = BossPhase.Phase4;
            }
            else if (healthPercent <= PHASE_3_THRESHOLD && encounter.CurrentPhase < BossPhase.Phase3)
            {
                newPhase = BossPhase.Phase3;
            }
            else if (healthPercent <= PHASE_2_THRESHOLD && encounter.CurrentPhase < BossPhase.Phase2)
            {
                newPhase = BossPhase.Phase2;
            }

            if (newPhase != encounter.CurrentPhase)
            {
                encounter.CurrentPhase = newPhase;
                Debug.Log($"[BossSystem] Phase transition: {encounter.InstanceId} boss {encounter.BossIndex} -> {newPhase}");
                OnPhaseTransition?.Invoke(encounter.InstanceId, encounter.BossIndex, newPhase);
            }
        }

        /// <summary>
        /// Get boss max health (scaled by difficulty).
        /// </summary>
        private float GetBossMaxHealth(string instanceId, int bossIndex)
        {
            // Base health (would come from BossDataSO)
            float baseHealth = 10000f + (bossIndex * 5000f);

            // Scale by difficulty
            float difficulty = _dungeonSystem?.GetDifficultyMultiplier(instanceId) ?? 1f;
            
            return baseHealth * difficulty;
        }

        /// <summary>
        /// Check if an encounter is active.
        /// </summary>
        public bool IsEncounterActive(string instanceId, int bossIndex)
        {
            return _activeEncounters.ContainsKey((instanceId, bossIndex));
        }

        /// <summary>
        /// Get encounter duration.
        /// </summary>
        public float GetEncounterDuration(string instanceId, int bossIndex)
        {
            var key = (instanceId, bossIndex);
            if (_activeEncounters.TryGetValue(key, out var encounter))
            {
                return Time.time - encounter.StartTime;
            }
            return 0f;
        }

        /// <summary>
        /// Reset an encounter (for wipes).
        /// </summary>
        public void ResetEncounter(string instanceId, int bossIndex)
        {
            var key = (instanceId, bossIndex);
            if (_activeEncounters.ContainsKey(key))
            {
                _activeEncounters.Remove(key);
                Debug.Log($"[BossSystem] Encounter reset: {instanceId} boss {bossIndex}");
            }
        }
    }

    /// <summary>
    /// Data for an active boss encounter.
    /// </summary>
    internal class BossEncounterData
    {
        public string InstanceId;
        public int BossIndex;
        public float CurrentHealth;
        public float MaxHealth;
        public BossPhase CurrentPhase;
        public float StartTime;
    }
}
