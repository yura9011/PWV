using System;
using System.Collections.Generic;
using UnityEngine;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Manages interrupt and lockout mechanics.
    /// 
    /// Features:
    /// - Interrupt enemy casts
    /// - Apply lockout preventing casting for duration
    /// - Track lockout timers
    /// 
    /// Requirements: 9.6, 9.7
    /// </summary>
    public class InterruptSystem : MonoBehaviour, IInterruptSystem
    {
        public const float DEFAULT_LOCKOUT_DURATION = 4f;

        // targetId -> lockout end time
        private readonly Dictionary<ulong, float> _lockouts = new();

        // Reference to ability system for interrupting casts
        private IAbilitySystem _abilitySystem;

        public event Action<ulong, ulong> OnCastInterrupted;
        public event Action<ulong, float> OnLockoutApplied;
        public event Action<ulong> OnLockoutExpired;

        public void Initialize(IAbilitySystem abilitySystem)
        {
            _abilitySystem = abilitySystem;
        }

        private void Update()
        {
            UpdateLockouts();
        }

        public bool TryInterrupt(ulong interrupterId, ulong targetId)
        {
            // In a real implementation, we'd check if the target is casting
            // For now, we assume the interrupt is valid and apply lockout
            
            Debug.Log($"[InterruptSystem] Player {interrupterId} interrupted target {targetId}");
            
            // Apply lockout
            ApplyLockout(targetId, DEFAULT_LOCKOUT_DURATION);
            
            OnCastInterrupted?.Invoke(interrupterId, targetId);
            
            return true;
        }

        public bool IsLockedOut(ulong targetId)
        {
            if (!_lockouts.TryGetValue(targetId, out float endTime))
                return false;

            return Time.time < endTime;
        }

        public float GetLockoutRemaining(ulong targetId)
        {
            if (!_lockouts.TryGetValue(targetId, out float endTime))
                return 0f;

            float remaining = endTime - Time.time;
            return remaining > 0 ? remaining : 0f;
        }

        public void ApplyLockout(ulong targetId, float duration)
        {
            if (duration <= 0)
                return;

            float endTime = Time.time + duration;
            _lockouts[targetId] = endTime;

            Debug.Log($"[InterruptSystem] Lockout applied to {targetId} for {duration}s");
            OnLockoutApplied?.Invoke(targetId, duration);
        }

        public void ClearLockout(ulong targetId)
        {
            if (_lockouts.Remove(targetId))
            {
                Debug.Log($"[InterruptSystem] Lockout cleared for {targetId}");
                OnLockoutExpired?.Invoke(targetId);
            }
        }

        private void UpdateLockouts()
        {
            var expiredLockouts = new List<ulong>();

            foreach (var kvp in _lockouts)
            {
                if (Time.time >= kvp.Value)
                {
                    expiredLockouts.Add(kvp.Key);
                }
            }

            foreach (var targetId in expiredLockouts)
            {
                _lockouts.Remove(targetId);
                Debug.Log($"[InterruptSystem] Lockout expired for {targetId}");
                OnLockoutExpired?.Invoke(targetId);
            }
        }

        /// <summary>
        /// Check if a target can cast (not locked out).
        /// </summary>
        public bool CanCast(ulong targetId)
        {
            return !IsLockedOut(targetId);
        }

        /// <summary>
        /// Clear all lockouts.
        /// </summary>
        public void ClearAll()
        {
            var allTargets = new List<ulong>(_lockouts.Keys);
            _lockouts.Clear();

            foreach (var targetId in allTargets)
            {
                OnLockoutExpired?.Invoke(targetId);
            }
        }

        /// <summary>
        /// Get count of active lockouts (for testing).
        /// </summary>
        public int GetActiveLockoutCount()
        {
            int count = 0;
            foreach (var kvp in _lockouts)
            {
                if (Time.time < kvp.Value)
                    count++;
            }
            return count;
        }
    }
}
