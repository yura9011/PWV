using System;
using System.Collections.Generic;
using EtherDomes.Data;
using UnityEngine;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Core combat system handling damage, healing, death, and resurrection.
    /// </summary>
    public class CombatSystem : MonoBehaviour, ICombatSystem
    {
        public const float DEFAULT_RES_WINDOW = 60f;
        public const float DEFAULT_RESPAWN_HEALTH = 0.5f;
        public const float COMBAT_TIMEOUT = 5f;
        public const float DEATH_STRIKE_DAMAGE_WINDOW = 5f; // Requirements 8.8: Track damage in last 5 seconds
        public const float DEATH_STRIKE_MIN_HEAL_PERCENT = 0.1f; // Requirements 8.8: Minimum 10% max HP
        public const float DEATH_STRIKE_HEAL_PERCENT = 0.25f; // Requirements 8.8: 25% of tracked damage

        [SerializeField] private float _resurrectionWindowSeconds = DEFAULT_RES_WINDOW;
        [SerializeField] private float _respawnHealthPercent = DEFAULT_RESPAWN_HEALTH;

        private IAggroSystem _aggroSystem;

        // Entity health tracking
        private readonly Dictionary<ulong, float> _currentHealth = new Dictionary<ulong, float>();
        private readonly Dictionary<ulong, float> _maxHealth = new Dictionary<ulong, float>();
        private readonly Dictionary<ulong, bool> _isDead = new Dictionary<ulong, bool>();
        private readonly Dictionary<ulong, float> _deathTime = new Dictionary<ulong, float>();
        
        // Combat state
        private readonly Dictionary<ulong, float> _lastCombatTime = new Dictionary<ulong, float>();
        private readonly HashSet<ulong> _inCombat = new HashSet<ulong>();
        private readonly HashSet<ulong> _playerIds = new HashSet<ulong>();

        // Death Strike damage tracking (Requirements 8.8)
        // Stores (timestamp, damage) pairs for each entity
        private readonly Dictionary<ulong, List<(float timestamp, float damage)>> _recentDamageTaken = new Dictionary<ulong, List<(float, float)>>();

        public event Action<ulong> OnEntityDied;
        public event Action<ulong> OnEntityResurrected;
        public event Action OnWipe;
        public event Action<ulong, ulong, float, DamageType> OnDamageDealt;
        public event Action<ulong, ulong, float> OnHealingDone;

        public float ResurrectionWindowSeconds => _resurrectionWindowSeconds;
        public float RespawnHealthPercent => _respawnHealthPercent;

        public void Initialize(IAggroSystem aggroSystem)
        {
            _aggroSystem = aggroSystem;
        }

        private void Update()
        {
            UpdateCombatState();
            CheckResurrectionTimeouts();
            CleanupOldDamageRecords();
        }

        private void UpdateCombatState()
        {
            float currentTime = Time.time;
            var toRemove = new List<ulong>();

            foreach (var entityId in _inCombat)
            {
                if (_lastCombatTime.TryGetValue(entityId, out float lastTime))
                {
                    if (currentTime - lastTime > COMBAT_TIMEOUT)
                    {
                        toRemove.Add(entityId);
                    }
                }
            }

            foreach (var entityId in toRemove)
            {
                LeaveCombat(entityId);
            }
        }

        private void CheckResurrectionTimeouts()
        {
            // In a real implementation, this would handle resurrection window expiry
            // For now, players can release spirit at any time
        }

        /// <summary>
        /// Cleans up damage records older than the Death Strike window.
        /// Requirements 8.8: Track damage taken in last 5 seconds
        /// </summary>
        private void CleanupOldDamageRecords()
        {
            float currentTime = Time.time;
            float cutoffTime = currentTime - DEATH_STRIKE_DAMAGE_WINDOW;

            foreach (var kvp in _recentDamageTaken)
            {
                kvp.Value.RemoveAll(record => record.timestamp < cutoffTime);
            }
        }


        public void ApplyDamage(ulong targetId, float damage, DamageType type, ulong sourceId = 0)
        {
            if (damage <= 0) return;
            if (IsDead(targetId)) return;

            // Apply damage
            float currentHealth = GetHealth(targetId);
            float newHealth = Mathf.Max(0, currentHealth - damage);
            _currentHealth[targetId] = newHealth;

            // Track damage for Death Strike healing (Requirements 8.8)
            TrackDamageTaken(targetId, damage);

            // Enter combat
            EnterCombat(targetId);
            if (sourceId != 0)
            {
                EnterCombat(sourceId);
                
                // Add threat
                _aggroSystem?.AddThreat(sourceId, targetId, damage);
            }

            Debug.Log($"[CombatSystem] Damage: {damage:F0} {type} to {targetId} (Health: {newHealth:F0}/{GetMaxHealth(targetId):F0})");
            OnDamageDealt?.Invoke(targetId, sourceId, damage, type);

            // Check for death
            if (newHealth <= 0)
            {
                HandleDeath(targetId);
            }
        }

        /// <summary>
        /// Tracks damage taken by an entity for Death Strike healing calculation.
        /// Requirements 8.8: Track damage taken in last 5 seconds
        /// </summary>
        private void TrackDamageTaken(ulong entityId, float damage)
        {
            if (!_recentDamageTaken.ContainsKey(entityId))
            {
                _recentDamageTaken[entityId] = new List<(float, float)>();
            }
            _recentDamageTaken[entityId].Add((Time.time, damage));
        }

        /// <summary>
        /// Gets the total damage taken by an entity in the last 5 seconds.
        /// Requirements 8.8: Used for Death Strike healing calculation
        /// </summary>
        public float GetRecentDamageTaken(ulong entityId)
        {
            if (!_recentDamageTaken.TryGetValue(entityId, out var damageRecords))
            {
                return 0f;
            }

            float currentTime = Time.time;
            float cutoffTime = currentTime - DEATH_STRIKE_DAMAGE_WINDOW;
            float totalDamage = 0f;

            foreach (var record in damageRecords)
            {
                if (record.timestamp >= cutoffTime)
                {
                    totalDamage += record.damage;
                }
            }

            return totalDamage;
        }

        /// <summary>
        /// Calculates Death Strike healing amount.
        /// Requirements 8.8: Heal = 25% of damage taken in last 5 seconds (min 10% max HP)
        /// </summary>
        public float CalculateDeathStrikeHealing(ulong entityId)
        {
            float recentDamage = GetRecentDamageTaken(entityId);
            float maxHealth = GetMaxHealth(entityId);
            
            // Calculate 25% of recent damage
            float healFromDamage = recentDamage * DEATH_STRIKE_HEAL_PERCENT;
            
            // Minimum heal is 10% of max health
            float minHeal = maxHealth * DEATH_STRIKE_MIN_HEAL_PERCENT;
            
            return Mathf.Max(healFromDamage, minHeal);
        }

        public void ApplyHealing(ulong targetId, float healing, ulong sourceId = 0)
        {
            if (healing <= 0) return;
            if (IsDead(targetId)) return;

            float currentHealth = GetHealth(targetId);
            float maxHealth = GetMaxHealth(targetId);
            float newHealth = Mathf.Min(maxHealth, currentHealth + healing);
            float actualHealing = newHealth - currentHealth;

            _currentHealth[targetId] = newHealth;

            Debug.Log($"[CombatSystem] Healing: {actualHealing:F0} to {targetId} (Health: {newHealth:F0}/{maxHealth:F0})");
            OnHealingDone?.Invoke(targetId, sourceId, actualHealing);

            // Add healing threat if in combat
            if (sourceId != 0 && IsInCombat(targetId) && actualHealing > 0)
            {
                var engagedEnemies = GetEngagedEnemies(targetId);
                _aggroSystem?.AddHealingThreat(sourceId, actualHealing, engagedEnemies);
            }
        }

        public void Kill(ulong targetId)
        {
            _currentHealth[targetId] = 0;
            HandleDeath(targetId);
        }

        public void Resurrect(ulong targetId, float healthPercent)
        {
            if (!IsDead(targetId)) return;

            float maxHealth = GetMaxHealth(targetId);
            float newHealth = maxHealth * Mathf.Clamp01(healthPercent);

            _currentHealth[targetId] = newHealth;
            _isDead[targetId] = false;
            _deathTime.Remove(targetId);

            Debug.Log($"[CombatSystem] Resurrected: {targetId} with {healthPercent * 100:F0}% health");
            OnEntityResurrected?.Invoke(targetId);
        }

        public void ReleaseSpirit(ulong playerId)
        {
            if (!IsDead(playerId)) return;

            // Resurrect at graveyard with reduced health
            Resurrect(playerId, _respawnHealthPercent);
            
            Debug.Log($"[CombatSystem] Spirit released: {playerId} respawning at graveyard");
        }

        private void HandleDeath(ulong targetId)
        {
            _isDead[targetId] = true;
            _deathTime[targetId] = Time.time;
            LeaveCombat(targetId);

            Debug.Log($"[CombatSystem] Entity died: {targetId}");
            OnEntityDied?.Invoke(targetId);

            // Reset threat for this enemy
            _aggroSystem?.ResetThreat(targetId);

            // Check for wipe
            CheckForWipe();
        }

        private void CheckForWipe()
        {
            if (_playerIds.Count == 0) return;

            bool allDead = true;
            foreach (var playerId in _playerIds)
            {
                if (!IsDead(playerId))
                {
                    allDead = false;
                    break;
                }
            }

            if (allDead)
            {
                Debug.Log("[CombatSystem] WIPE - All players dead");
                OnWipe?.Invoke();
            }
        }


        public bool IsInCombat(ulong entityId)
        {
            return _inCombat.Contains(entityId);
        }

        public bool IsDead(ulong entityId)
        {
            return _isDead.TryGetValue(entityId, out bool dead) && dead;
        }

        public float GetHealth(ulong entityId)
        {
            return _currentHealth.TryGetValue(entityId, out float health) ? health : 0;
        }

        public float GetMaxHealth(ulong entityId)
        {
            return _maxHealth.TryGetValue(entityId, out float max) ? max : 100;
        }

        public float GetHealthPercent(ulong entityId)
        {
            float max = GetMaxHealth(entityId);
            if (max <= 0) return 0;
            return GetHealth(entityId) / max;
        }

        public void SetHealth(ulong entityId, float current, float max)
        {
            _maxHealth[entityId] = max;
            _currentHealth[entityId] = Mathf.Clamp(current, 0, max);
            _isDead[entityId] = current <= 0;
        }

        public void EnterCombat(ulong entityId)
        {
            _inCombat.Add(entityId);
            _lastCombatTime[entityId] = Time.time;
        }

        public void LeaveCombat(ulong entityId)
        {
            _inCombat.Remove(entityId);
            _lastCombatTime.Remove(entityId);
            
            // Reset threat when leaving combat
            _aggroSystem?.ResetThreat(entityId);
        }

        /// <summary>
        /// Register a player for wipe detection.
        /// </summary>
        public void RegisterPlayer(ulong playerId, float maxHealth)
        {
            _playerIds.Add(playerId);
            SetHealth(playerId, maxHealth, maxHealth);
        }

        /// <summary>
        /// Unregister a player.
        /// </summary>
        public void UnregisterPlayer(ulong playerId)
        {
            _playerIds.Remove(playerId);
            _currentHealth.Remove(playerId);
            _maxHealth.Remove(playerId);
            _isDead.Remove(playerId);
            _deathTime.Remove(playerId);
            _inCombat.Remove(playerId);
            _lastCombatTime.Remove(playerId);
            _recentDamageTaken.Remove(playerId);
        }

        /// <summary>
        /// Register an enemy.
        /// </summary>
        public void RegisterEnemy(ulong enemyId, float maxHealth)
        {
            SetHealth(enemyId, maxHealth, maxHealth);
        }

        /// <summary>
        /// Get enemies currently engaged with a player.
        /// </summary>
        private ulong[] GetEngagedEnemies(ulong playerId)
        {
            // In a real implementation, this would track which enemies are in combat with which players
            // For now, return all enemies in combat
            var enemies = new List<ulong>();
            foreach (var entityId in _inCombat)
            {
                if (!_playerIds.Contains(entityId))
                {
                    enemies.Add(entityId);
                }
            }
            return enemies.ToArray();
        }

        /// <summary>
        /// Get time since death for resurrection window.
        /// </summary>
        public float GetTimeSinceDeath(ulong entityId)
        {
            if (_deathTime.TryGetValue(entityId, out float deathTime))
            {
                return Time.time - deathTime;
            }
            return 0;
        }

        /// <summary>
        /// Check if resurrection is still available.
        /// </summary>
        public bool CanBeResurrected(ulong entityId)
        {
            if (!IsDead(entityId)) return false;
            return GetTimeSinceDeath(entityId) <= _resurrectionWindowSeconds;
        }
    }
}
