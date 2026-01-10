using System;
using System.Collections.Generic;
using System.Linq;
using EtherDomes.Data;
using UnityEngine;

namespace EtherDomes.Combat
{
    /// <summary>
    /// System for managing buffs and debuffs on entities.
    /// Handles application, expiration, DoT/HoT ticks, and duration tracking.
    /// </summary>
    public class BuffSystem : MonoBehaviour, IBuffSystem
    {
        #region Constants
        
        public const int DEFAULT_MAX_BUFFS = 20;
        public const int DEFAULT_MAX_DEBUFFS = 20;
        public const float DEFAULT_MIN_DURATION = 1f;
        public const float DEFAULT_MAX_DURATION = 300f;

        public int MaxBuffsPerEntity => DEFAULT_MAX_BUFFS;
        public int MaxDebuffsPerEntity => DEFAULT_MAX_DEBUFFS;
        public float MinDuration => DEFAULT_MIN_DURATION;
        public float MaxDuration => DEFAULT_MAX_DURATION;
        
        #endregion

        #region Private Fields
        
        // Active buffs per entity
        private readonly Dictionary<ulong, List<BuffInstance>> _activeBuffs = new Dictionary<ulong, List<BuffInstance>>();
        
        // Active debuffs per entity
        private readonly Dictionary<ulong, List<BuffInstance>> _activeDebuffs = new Dictionary<ulong, List<BuffInstance>>();
        
        // Reference to combat system for applying DoT/HoT damage/healing
        private ICombatSystem _combatSystem;
        
        #endregion

        #region Events
        
        public event Action<ulong, BuffInstance> OnBuffApplied;
        public event Action<ulong, BuffInstance> OnBuffExpired;
        public event Action<ulong, BuffInstance> OnDebuffApplied;
        public event Action<ulong, BuffInstance> OnDebuffExpired;
        public event Action<ulong, float, DamageType, ulong> OnDoTTick;
        public event Action<ulong, float, ulong> OnHoTTick;
        public event Action<ulong, CCType, float, ulong> OnCCApplied;
        public event Action<ulong, CCType> OnCCExpired;
        
        #endregion

        #region Initialization
        
        /// <summary>
        /// Initialize the buff system with required dependencies.
        /// </summary>
        public void Initialize(ICombatSystem combatSystem)
        {
            _combatSystem = combatSystem;
        }
        
        #endregion

        #region Unity Lifecycle
        
        private void Update()
        {
            float deltaTime = Time.deltaTime;
            UpdateBuffs(deltaTime);
            UpdateDebuffs(deltaTime);
        }
        
        #endregion

        #region Core Operations
        
        public void ApplyBuff(ulong targetId, BuffData buff, ulong sourceId = 0)
        {
            if (buff == null)
            {
                Debug.LogWarning("[BuffSystem] Attempted to apply null buff");
                return;
            }

            // Clamp duration to valid range
            float clampedDuration = ClampDuration(buff.Duration);
            BuffData clampedBuff = buff.Clone();
            clampedBuff.Duration = clampedDuration;

            // Get or create buff list for entity
            if (!_activeBuffs.TryGetValue(targetId, out var buffList))
            {
                buffList = new List<BuffInstance>();
                _activeBuffs[targetId] = buffList;
            }

            // Check for existing buff with same ID
            var existingBuff = buffList.FirstOrDefault(b => b.BuffId == clampedBuff.BuffId);
            if (existingBuff != null)
            {
                // Refresh duration or add stack
                if (clampedBuff.IsStackable && existingBuff.AddStack())
                {
                    existingBuff.RefreshDuration();
                }
                else
                {
                    existingBuff.RefreshDuration();
                }
                return;
            }

            // Check buff limit
            if (buffList.Count >= MaxBuffsPerEntity)
            {
                // Remove oldest buff
                var oldest = buffList.OrderBy(b => b.AppliedTime).First();
                buffList.Remove(oldest);
                OnBuffExpired?.Invoke(targetId, oldest);
                Debug.Log($"[BuffSystem] Removed oldest buff '{oldest.DisplayName}' to make room for new buff");
            }

            // Create and add new buff instance
            var instance = new BuffInstance(clampedBuff, sourceId, true, Time.time);
            buffList.Add(instance);

            Debug.Log($"[BuffSystem] Applied buff '{clampedBuff.DisplayName}' to {targetId} (Duration: {clampedDuration}s)");
            OnBuffApplied?.Invoke(targetId, instance);
        }

        public void ApplyDebuff(ulong targetId, BuffData debuff, ulong sourceId = 0)
        {
            if (debuff == null)
            {
                Debug.LogWarning("[BuffSystem] Attempted to apply null debuff");
                return;
            }

            // Clamp duration to valid range
            float clampedDuration = ClampDuration(debuff.Duration);
            BuffData clampedDebuff = debuff.Clone();
            clampedDebuff.Duration = clampedDuration;

            // Get or create debuff list for entity
            if (!_activeDebuffs.TryGetValue(targetId, out var debuffList))
            {
                debuffList = new List<BuffInstance>();
                _activeDebuffs[targetId] = debuffList;
            }

            // Check for existing debuff with same ID
            var existingDebuff = debuffList.FirstOrDefault(d => d.BuffId == clampedDebuff.BuffId);
            if (existingDebuff != null)
            {
                // Refresh duration or add stack
                if (clampedDebuff.IsStackable && existingDebuff.AddStack())
                {
                    existingDebuff.RefreshDuration();
                }
                else
                {
                    existingDebuff.RefreshDuration();
                }
                return;
            }

            // Check debuff limit
            if (debuffList.Count >= MaxDebuffsPerEntity)
            {
                // Remove oldest debuff
                var oldest = debuffList.OrderBy(d => d.AppliedTime).First();
                debuffList.Remove(oldest);
                OnDebuffExpired?.Invoke(targetId, oldest);
                Debug.Log($"[BuffSystem] Removed oldest debuff '{oldest.DisplayName}' to make room for new debuff");
            }

            // Create and add new debuff instance
            var instance = new BuffInstance(clampedDebuff, sourceId, false, Time.time);
            debuffList.Add(instance);

            Debug.Log($"[BuffSystem] Applied debuff '{clampedDebuff.DisplayName}' to {targetId} (Duration: {clampedDuration}s)");
            OnDebuffApplied?.Invoke(targetId, instance);

            // Fire CC event if this is a CC effect (Requirements 11.1, 11.2, 11.3)
            if (clampedDebuff.CCType != CCType.None)
            {
                Debug.Log($"[BuffSystem] CC effect applied: {clampedDebuff.CCType} to {targetId} for {clampedDuration}s");
                OnCCApplied?.Invoke(targetId, clampedDebuff.CCType, clampedDuration, sourceId);
            }
        }

        public void RemoveBuff(ulong targetId, string buffId)
        {
            if (!_activeBuffs.TryGetValue(targetId, out var buffList)) return;

            var buff = buffList.FirstOrDefault(b => b.BuffId == buffId);
            if (buff != null)
            {
                buffList.Remove(buff);
                Debug.Log($"[BuffSystem] Removed buff '{buff.DisplayName}' from {targetId}");
                OnBuffExpired?.Invoke(targetId, buff);
            }
        }

        public void RemoveDebuff(ulong targetId, string debuffId)
        {
            if (!_activeDebuffs.TryGetValue(targetId, out var debuffList)) return;

            var debuff = debuffList.FirstOrDefault(d => d.BuffId == debuffId);
            if (debuff != null)
            {
                debuffList.Remove(debuff);
                Debug.Log($"[BuffSystem] Removed debuff '{debuff.DisplayName}' from {targetId}");
                OnDebuffExpired?.Invoke(targetId, debuff);
            }
        }

        public void RemoveAllBuffs(ulong targetId)
        {
            if (!_activeBuffs.TryGetValue(targetId, out var buffList)) return;

            foreach (var buff in buffList.ToList())
            {
                OnBuffExpired?.Invoke(targetId, buff);
            }
            buffList.Clear();
            Debug.Log($"[BuffSystem] Removed all buffs from {targetId}");
        }

        public void RemoveAllDebuffs(ulong targetId)
        {
            if (!_activeDebuffs.TryGetValue(targetId, out var debuffList)) return;

            foreach (var debuff in debuffList.ToList())
            {
                OnDebuffExpired?.Invoke(targetId, debuff);
            }
            debuffList.Clear();
            Debug.Log($"[BuffSystem] Removed all debuffs from {targetId}");
        }
        
        #endregion

        #region Queries
        
        public IReadOnlyList<BuffInstance> GetActiveBuffs(ulong targetId)
        {
            if (_activeBuffs.TryGetValue(targetId, out var buffList))
            {
                return buffList.AsReadOnly();
            }
            return Array.Empty<BuffInstance>();
        }

        public IReadOnlyList<BuffInstance> GetActiveDebuffs(ulong targetId)
        {
            if (_activeDebuffs.TryGetValue(targetId, out var debuffList))
            {
                return debuffList.AsReadOnly();
            }
            return Array.Empty<BuffInstance>();
        }

        public bool HasBuff(ulong targetId, string buffId)
        {
            if (!_activeBuffs.TryGetValue(targetId, out var buffList)) return false;
            return buffList.Any(b => b.BuffId == buffId);
        }

        public bool HasDebuff(ulong targetId, string debuffId)
        {
            if (!_activeDebuffs.TryGetValue(targetId, out var debuffList)) return false;
            return debuffList.Any(d => d.BuffId == debuffId);
        }

        public float GetRemainingDuration(ulong targetId, string effectId)
        {
            // Check buffs first
            if (_activeBuffs.TryGetValue(targetId, out var buffList))
            {
                var buff = buffList.FirstOrDefault(b => b.BuffId == effectId);
                if (buff != null) return buff.RemainingDuration;
            }

            // Check debuffs
            if (_activeDebuffs.TryGetValue(targetId, out var debuffList))
            {
                var debuff = debuffList.FirstOrDefault(d => d.BuffId == effectId);
                if (debuff != null) return debuff.RemainingDuration;
            }

            return 0f;
        }

        public int GetBuffCount(ulong targetId)
        {
            if (_activeBuffs.TryGetValue(targetId, out var buffList))
            {
                return buffList.Count;
            }
            return 0;
        }

        public int GetDebuffCount(ulong targetId)
        {
            if (_activeDebuffs.TryGetValue(targetId, out var debuffList))
            {
                return debuffList.Count;
            }
            return 0;
        }

        /// <summary>
        /// Check if an entity is affected by a specific CC type.
        /// Requirements: 11.1, 11.2, 11.3
        /// </summary>
        public bool IsAffectedByCC(ulong targetId, CCType ccType)
        {
            if (ccType == CCType.None) return false;
            
            if (_activeDebuffs.TryGetValue(targetId, out var debuffList))
            {
                return debuffList.Any(d => d.Data.CCType == ccType);
            }
            return false;
        }

        /// <summary>
        /// Get the active CC effect of a specific type on an entity.
        /// Returns null if no such CC is active.
        /// </summary>
        public BuffInstance GetActiveCCEffect(ulong targetId, CCType ccType)
        {
            if (ccType == CCType.None) return null;
            
            if (_activeDebuffs.TryGetValue(targetId, out var debuffList))
            {
                return debuffList.FirstOrDefault(d => d.Data.CCType == ccType);
            }
            return null;
        }

        /// <summary>
        /// Check if an entity is stunned (cannot perform any actions).
        /// Requirements: 11.2
        /// </summary>
        public bool IsStunned(ulong targetId)
        {
            return IsAffectedByCC(targetId, CCType.Stun);
        }

        /// <summary>
        /// Check if an entity is feared (random movement).
        /// Requirements: 11.3
        /// </summary>
        public bool IsFeared(ulong targetId)
        {
            return IsAffectedByCC(targetId, CCType.Fear);
        }

        /// <summary>
        /// Check if an entity is rooted (cannot move).
        /// </summary>
        public bool IsRooted(ulong targetId)
        {
            return IsAffectedByCC(targetId, CCType.Root);
        }

        /// <summary>
        /// Get the total slow percentage on an entity (0.0 to 1.0).
        /// Requirements: 11.1
        /// </summary>
        public float GetSlowPercent(ulong targetId)
        {
            if (!_activeDebuffs.TryGetValue(targetId, out var debuffList))
            {
                return 0f;
            }

            // Find the strongest slow effect (they don't stack, use highest)
            float maxSlow = 0f;
            foreach (var debuff in debuffList)
            {
                if (debuff.Data.CCType == CCType.Slow && debuff.Data.SlowPercent > maxSlow)
                {
                    maxSlow = debuff.Data.SlowPercent;
                }
            }
            return maxSlow;
        }
        
        #endregion

        #region Private Methods
        
        /// <summary>
        /// Clamp duration to valid range [MinDuration, MaxDuration].
        /// </summary>
        private float ClampDuration(float duration)
        {
            return Mathf.Clamp(duration, MinDuration, MaxDuration);
        }

        /// <summary>
        /// Update all active buffs, processing ticks and expirations.
        /// </summary>
        private void UpdateBuffs(float deltaTime)
        {
            foreach (var kvp in _activeBuffs.ToList())
            {
                ulong targetId = kvp.Key;
                var buffList = kvp.Value;
                var expiredBuffs = new List<BuffInstance>();

                foreach (var buff in buffList)
                {
                    buff.Update(deltaTime);

                    // Process HoT ticks
                    if (buff.Data.IsPeriodicEffect && buff.Data.TickHealing > 0 && buff.IsTimeForTick)
                    {
                        ProcessHoTTick(targetId, buff);
                    }

                    // Check for expiration
                    if (buff.IsExpired)
                    {
                        expiredBuffs.Add(buff);
                    }
                }

                // Remove expired buffs
                foreach (var expired in expiredBuffs)
                {
                    buffList.Remove(expired);
                    Debug.Log($"[BuffSystem] Buff '{expired.DisplayName}' expired on {targetId}");
                    OnBuffExpired?.Invoke(targetId, expired);
                }
            }
        }

        /// <summary>
        /// Update all active debuffs, processing ticks and expirations.
        /// </summary>
        private void UpdateDebuffs(float deltaTime)
        {
            foreach (var kvp in _activeDebuffs.ToList())
            {
                ulong targetId = kvp.Key;
                var debuffList = kvp.Value;
                var expiredDebuffs = new List<BuffInstance>();

                foreach (var debuff in debuffList)
                {
                    debuff.Update(deltaTime);

                    // Process DoT ticks
                    if (debuff.Data.IsPeriodicEffect && debuff.Data.TickDamage > 0 && debuff.IsTimeForTick)
                    {
                        ProcessDoTTick(targetId, debuff);
                    }

                    // Check for expiration
                    if (debuff.IsExpired)
                    {
                        expiredDebuffs.Add(debuff);
                    }
                }

                // Remove expired debuffs
                foreach (var expired in expiredDebuffs)
                {
                    debuffList.Remove(expired);
                    Debug.Log($"[BuffSystem] Debuff '{expired.DisplayName}' expired on {targetId}");
                    OnDebuffExpired?.Invoke(targetId, expired);

                    // Fire CC expired event if this was a CC effect
                    if (expired.Data.CCType != CCType.None)
                    {
                        Debug.Log($"[BuffSystem] CC effect expired: {expired.Data.CCType} on {targetId}");
                        OnCCExpired?.Invoke(targetId, expired.Data.CCType);
                    }
                }
            }
        }

        /// <summary>
        /// Process a DoT tick, applying damage through CombatSystem.
        /// </summary>
        private void ProcessDoTTick(ulong targetId, BuffInstance debuff)
        {
            float damage = debuff.Data.TickDamage * debuff.CurrentStacks;
            DamageType damageType = debuff.Data.DamageType;
            ulong sourceId = debuff.SourceId;

            // Apply damage through combat system
            _combatSystem?.ApplyDamage(targetId, damage, damageType, sourceId);

            // Reset tick timer
            debuff.ResetTickTimer();

            Debug.Log($"[BuffSystem] DoT tick: {damage:F0} {damageType} damage to {targetId} from '{debuff.DisplayName}'");
            OnDoTTick?.Invoke(targetId, damage, damageType, sourceId);
        }

        /// <summary>
        /// Process a HoT tick, applying healing through CombatSystem.
        /// </summary>
        private void ProcessHoTTick(ulong targetId, BuffInstance buff)
        {
            float healing = buff.Data.TickHealing * buff.CurrentStacks;
            ulong sourceId = buff.SourceId;

            // Apply healing through combat system
            _combatSystem?.ApplyHealing(targetId, healing, sourceId);

            // Reset tick timer
            buff.ResetTickTimer();

            Debug.Log($"[BuffSystem] HoT tick: {healing:F0} healing to {targetId} from '{buff.DisplayName}'");
            OnHoTTick?.Invoke(targetId, healing, sourceId);
        }

        /// <summary>
        /// Clear all buffs and debuffs for an entity (e.g., on death).
        /// </summary>
        public void ClearAllEffects(ulong targetId)
        {
            RemoveAllBuffs(targetId);
            RemoveAllDebuffs(targetId);
        }

        /// <summary>
        /// Unregister an entity, removing all tracking data.
        /// </summary>
        public void UnregisterEntity(ulong entityId)
        {
            _activeBuffs.Remove(entityId);
            _activeDebuffs.Remove(entityId);
        }
        
        #endregion
    }
}
