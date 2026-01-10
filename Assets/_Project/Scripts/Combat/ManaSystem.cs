using System;
using System.Collections.Generic;
using UnityEngine;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Manages mana resources for all players.
    /// Handles spending, restoration, and regeneration with different rates
    /// for in-combat (0.5%) and out-of-combat (2%) states.
    /// </summary>
    public class ManaSystem : IManaSystem
    {
        private readonly Dictionary<ulong, ManaState> _playerMana = new();
        
        public float OutOfCombatRegenRate => 0.02f; // 2% per second
        public float InCombatRegenRate => 0.005f;   // 0.5% per second

        public event Action<ulong, float, float> OnManaChanged;
        public event Action<ulong> OnManaEmpty;

        private class ManaState
        {
            public float CurrentMana;
            public float MaxMana;
            public float RegenRate;
            public bool IsRegenerating;
        }

        public void RegisterPlayer(ulong playerId, float maxMana)
        {
            if (_playerMana.ContainsKey(playerId))
            {
                Debug.LogWarning($"[ManaSystem] Player {playerId} already registered");
                return;
            }

            _playerMana[playerId] = new ManaState
            {
                CurrentMana = maxMana,
                MaxMana = maxMana,
                RegenRate = OutOfCombatRegenRate,
                IsRegenerating = false
            };

            OnManaChanged?.Invoke(playerId, maxMana, maxMana);
        }

        public void UnregisterPlayer(ulong playerId)
        {
            _playerMana.Remove(playerId);
        }

        public float GetCurrentMana(ulong playerId)
        {
            return _playerMana.TryGetValue(playerId, out var state) ? state.CurrentMana : 0f;
        }

        public float GetMaxMana(ulong playerId)
        {
            return _playerMana.TryGetValue(playerId, out var state) ? state.MaxMana : 0f;
        }

        public float GetManaPercent(ulong playerId)
        {
            if (!_playerMana.TryGetValue(playerId, out var state) || state.MaxMana <= 0)
                return 0f;
            
            return state.CurrentMana / state.MaxMana;
        }

        public bool TrySpendMana(ulong playerId, float amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"[ManaSystem] Cannot spend negative mana: {amount}");
                return false;
            }

            if (!_playerMana.TryGetValue(playerId, out var state))
            {
                Debug.LogWarning($"[ManaSystem] Player {playerId} not registered");
                return false;
            }

            if (state.CurrentMana < amount)
            {
                return false; // Not enough mana
            }

            state.CurrentMana -= amount;
            OnManaChanged?.Invoke(playerId, state.CurrentMana, state.MaxMana);

            if (state.CurrentMana <= 0)
            {
                state.CurrentMana = 0;
                OnManaEmpty?.Invoke(playerId);
            }

            return true;
        }

        public void RestoreMana(ulong playerId, float amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"[ManaSystem] Cannot restore negative mana: {amount}");
                return;
            }

            if (!_playerMana.TryGetValue(playerId, out var state))
            {
                Debug.LogWarning($"[ManaSystem] Player {playerId} not registered");
                return;
            }

            float previousMana = state.CurrentMana;
            state.CurrentMana = Mathf.Min(state.CurrentMana + amount, state.MaxMana);

            if (state.CurrentMana != previousMana)
            {
                OnManaChanged?.Invoke(playerId, state.CurrentMana, state.MaxMana);
            }
        }

        public void SetMaxMana(ulong playerId, float maxMana)
        {
            if (maxMana < 0)
            {
                Debug.LogWarning($"[ManaSystem] Max mana cannot be negative: {maxMana}");
                return;
            }

            if (!_playerMana.TryGetValue(playerId, out var state))
            {
                Debug.LogWarning($"[ManaSystem] Player {playerId} not registered");
                return;
            }

            state.MaxMana = maxMana;
            
            // Cap current mana at new max
            if (state.CurrentMana > maxMana)
            {
                state.CurrentMana = maxMana;
            }

            OnManaChanged?.Invoke(playerId, state.CurrentMana, state.MaxMana);
        }

        public void StartCombatRegen(ulong playerId)
        {
            if (_playerMana.TryGetValue(playerId, out var state))
            {
                state.RegenRate = InCombatRegenRate;
                state.IsRegenerating = true;
            }
        }

        public void StartOutOfCombatRegen(ulong playerId)
        {
            if (_playerMana.TryGetValue(playerId, out var state))
            {
                state.RegenRate = OutOfCombatRegenRate;
                state.IsRegenerating = true;
            }
        }

        public void StopRegen(ulong playerId)
        {
            if (_playerMana.TryGetValue(playerId, out var state))
            {
                state.IsRegenerating = false;
            }
        }

        /// <summary>
        /// Updates mana regeneration. Should be called each frame or tick.
        /// </summary>
        /// <param name="deltaTime">Time since last update.</param>
        public void UpdateRegen(float deltaTime)
        {
            foreach (var kvp in _playerMana)
            {
                var state = kvp.Value;
                
                if (!state.IsRegenerating || state.CurrentMana >= state.MaxMana)
                    continue;

                float regenAmount = state.MaxMana * state.RegenRate * deltaTime;
                float previousMana = state.CurrentMana;
                state.CurrentMana = Mathf.Min(state.CurrentMana + regenAmount, state.MaxMana);

                if (state.CurrentMana != previousMana)
                {
                    OnManaChanged?.Invoke(kvp.Key, state.CurrentMana, state.MaxMana);
                }
            }
        }

        /// <summary>
        /// Gets the current regeneration rate for a player.
        /// </summary>
        public float GetRegenRate(ulong playerId)
        {
            return _playerMana.TryGetValue(playerId, out var state) ? state.RegenRate : 0f;
        }

        /// <summary>
        /// Checks if a player is currently regenerating mana.
        /// </summary>
        public bool IsRegenerating(ulong playerId)
        {
            return _playerMana.TryGetValue(playerId, out var state) && state.IsRegenerating;
        }
    }
}
