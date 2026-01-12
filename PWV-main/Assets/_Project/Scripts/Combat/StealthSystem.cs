using System;
using System.Collections.Generic;
using UnityEngine;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Manages stealth state for players including entering/exiting stealth,
    /// automatic break conditions, cooldowns, and movement speed modifiers.
    /// Requirements: 4.1, 4.2, 4.3, 4.4, 4.6
    /// </summary>
    public class StealthSystem : MonoBehaviour, IStealthSystem
    {
        #region Constants
        
        /// <summary>
        /// Movement speed multiplier while in stealth (70%).
        /// </summary>
        public const float DEFAULT_MOVEMENT_SPEED_MULTIPLIER = 0.7f;
        
        /// <summary>
        /// Cooldown before re-entering stealth after it breaks (in seconds).
        /// </summary>
        public const float DEFAULT_STEALTH_COOLDOWN = 2f;
        
        /// <summary>
        /// Opacity for local player while in stealth (30%).
        /// </summary>
        public const float DEFAULT_LOCAL_PLAYER_OPACITY = 0.3f;
        
        /// <summary>
        /// Opacity for enemies viewing a stealthed player (0%).
        /// </summary>
        public const float DEFAULT_ENEMY_VIEW_OPACITY = 0f;
        
        #endregion

        #region Private State
        
        /// <summary>
        /// Set of player IDs currently in stealth.
        /// </summary>
        private readonly HashSet<ulong> _stealthedPlayers = new HashSet<ulong>();
        
        /// <summary>
        /// Cooldown timers per player (time when cooldown expires).
        /// </summary>
        private readonly Dictionary<ulong, float> _cooldownEndTimes = new Dictionary<ulong, float>();
        
        #endregion

        #region IStealthSystem Properties
        
        public float MovementSpeedMultiplier => DEFAULT_MOVEMENT_SPEED_MULTIPLIER;
        public float StealthCooldown => DEFAULT_STEALTH_COOLDOWN;
        public float LocalPlayerOpacity => DEFAULT_LOCAL_PLAYER_OPACITY;
        public float EnemyViewOpacity => DEFAULT_ENEMY_VIEW_OPACITY;
        
        #endregion

        #region Events
        
        public event Action<ulong> OnStealthEntered;
        public event Action<ulong, StealthBreakReason> OnStealthBroken;
        
        #endregion

        #region Core Operations
        
        /// <summary>
        /// Attempt to enter stealth for a player.
        /// Requirements: 4.1, 4.6
        /// </summary>
        public bool TryEnterStealth(ulong playerId)
        {
            // Check if already in stealth
            if (IsInStealth(playerId))
            {
                Debug.Log($"[StealthSystem] Player {playerId} is already in stealth");
                return true; // Already stealthed, consider it success
            }
            
            // Check cooldown
            if (!CanEnterStealth(playerId))
            {
                float remaining = GetStealthCooldownRemaining(playerId);
                Debug.Log($"[StealthSystem] Player {playerId} cannot enter stealth - cooldown: {remaining:F1}s remaining");
                return false;
            }
            
            // Enter stealth
            _stealthedPlayers.Add(playerId);
            
            Debug.Log($"[StealthSystem] Player {playerId} entered stealth");
            OnStealthEntered?.Invoke(playerId);
            
            return true;
        }
        
        /// <summary>
        /// Break stealth for a player with a specified reason.
        /// Requirements: 4.2, 4.3, 4.6
        /// </summary>
        public void BreakStealth(ulong playerId, StealthBreakReason reason)
        {
            if (!IsInStealth(playerId))
            {
                return; // Not in stealth, nothing to break
            }
            
            // Remove from stealth
            _stealthedPlayers.Remove(playerId);
            
            // Start cooldown
            _cooldownEndTimes[playerId] = Time.time + DEFAULT_STEALTH_COOLDOWN;
            
            Debug.Log($"[StealthSystem] Player {playerId} stealth broken - reason: {reason}");
            OnStealthBroken?.Invoke(playerId, reason);
        }
        
        /// <summary>
        /// Manually exit stealth (toggle off).
        /// </summary>
        public void ExitStealth(ulong playerId)
        {
            BreakStealth(playerId, StealthBreakReason.Manual);
        }
        
        /// <summary>
        /// Notify the system that a player received damage.
        /// Requirements: 4.3
        /// </summary>
        public void OnDamageReceived(ulong playerId)
        {
            if (IsInStealth(playerId))
            {
                BreakStealth(playerId, StealthBreakReason.DamageReceived);
            }
        }
        
        /// <summary>
        /// Notify the system that a player performed an attack.
        /// Requirements: 4.2
        /// </summary>
        public void OnAttackPerformed(ulong playerId)
        {
            if (IsInStealth(playerId))
            {
                BreakStealth(playerId, StealthBreakReason.Attack);
            }
        }
        
        #endregion

        #region Queries
        
        /// <summary>
        /// Check if a player is currently in stealth.
        /// </summary>
        public bool IsInStealth(ulong playerId)
        {
            return _stealthedPlayers.Contains(playerId);
        }
        
        /// <summary>
        /// Get the remaining cooldown before a player can enter stealth.
        /// Requirements: 4.6
        /// </summary>
        public float GetStealthCooldownRemaining(ulong playerId)
        {
            if (!_cooldownEndTimes.TryGetValue(playerId, out float endTime))
            {
                return 0f;
            }
            
            float remaining = endTime - Time.time;
            return remaining > 0f ? remaining : 0f;
        }
        
        /// <summary>
        /// Check if a player can enter stealth (not on cooldown).
        /// Requirements: 4.6
        /// </summary>
        public bool CanEnterStealth(ulong playerId)
        {
            return GetStealthCooldownRemaining(playerId) <= 0f;
        }
        
        /// <summary>
        /// Check if a player can use a stealth-requiring ability.
        /// Requirements: 4.5
        /// </summary>
        public bool CanUseStealthAbility(ulong playerId, string abilityId)
        {
            // For stealth-requiring abilities, player must be in stealth
            return IsInStealth(playerId);
        }
        
        /// <summary>
        /// Get the effective movement speed multiplier for a player.
        /// Requirements: 4.4
        /// </summary>
        public float GetMovementSpeedMultiplier(ulong playerId)
        {
            return IsInStealth(playerId) ? DEFAULT_MOVEMENT_SPEED_MULTIPLIER : 1f;
        }
        
        #endregion

        #region Unity Lifecycle
        
        private void Update()
        {
            // Clean up expired cooldowns to prevent memory growth
            CleanupExpiredCooldowns();
        }
        
        private void CleanupExpiredCooldowns()
        {
            // Only clean up periodically to avoid overhead
            if (Time.frameCount % 60 != 0) return;
            
            List<ulong> expiredKeys = null;
            float currentTime = Time.time;
            
            foreach (var kvp in _cooldownEndTimes)
            {
                if (kvp.Value < currentTime)
                {
                    expiredKeys ??= new List<ulong>();
                    expiredKeys.Add(kvp.Key);
                }
            }
            
            if (expiredKeys != null)
            {
                foreach (var key in expiredKeys)
                {
                    _cooldownEndTimes.Remove(key);
                }
            }
        }
        
        #endregion

        #region Debug/Testing Methods
        
        /// <summary>
        /// Clear all stealth states (for testing).
        /// </summary>
        public void ClearAllStealthStates()
        {
            _stealthedPlayers.Clear();
            _cooldownEndTimes.Clear();
        }
        
        /// <summary>
        /// Force set cooldown for a player (for testing).
        /// </summary>
        public void SetCooldown(ulong playerId, float cooldownDuration)
        {
            _cooldownEndTimes[playerId] = Time.time + cooldownDuration;
        }
        
        /// <summary>
        /// Clear cooldown for a player (for testing).
        /// </summary>
        public void ClearCooldown(ulong playerId)
        {
            _cooldownEndTimes.Remove(playerId);
        }
        
        /// <summary>
        /// Get the number of players currently in stealth (for testing).
        /// </summary>
        public int GetStealthedPlayerCount()
        {
            return _stealthedPlayers.Count;
        }
        
        #endregion
    }
}
