using System;
using UnityEngine;
using Unity.Netcode;
using EtherDomes.Data;

namespace EtherDomes.Combat
{
    /// <summary>
    /// NetworkBehaviour that manages entity health with server authority.
    /// Implements ITargetable for targeting system integration.
    /// </summary>
    public class HealthSystem : NetworkBehaviour, IDamageable, ITargetable
    {
        #region Configuration

        [Header("Configuration")]
        [SerializeField] private string _displayName = "Entity";
        [SerializeField] private float _baseMaxHealth = 100f;
        [SerializeField] private int _level = 1;
        [SerializeField] private TargetType _targetType = TargetType.Enemy;

        #endregion

        #region Network Variables

        /// <summary>Current health synchronized across network.</summary>
        public NetworkVariable<float> CurrentHealth = new NetworkVariable<float>(
            100f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        /// <summary>Maximum health synchronized across network.</summary>
        public NetworkVariable<float> MaxHealth = new NetworkVariable<float>(
            100f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        /// <summary>Death state synchronized across network.</summary>
        public NetworkVariable<bool> IsDeadNetwork = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        #endregion

        #region Events

        /// <summary>Fired when health changes (current, max).</summary>
        public event Action<float, float> OnHealthChanged;

        /// <summary>Fired when damage is taken (amount, type, attackerId).</summary>
        public event Action<float, DamageType, ulong> OnDamageTaken;

        /// <summary>Fired when healing is received (amount, healerId).</summary>
        public event Action<float, ulong> OnHealingReceived;

        /// <summary>Fired when entity dies.</summary>
        public event Action<ITargetable> OnDeath;

        /// <summary>Fired when entity is revived.</summary>
        public event Action OnRevived;

        #endregion

        #region ITargetable Implementation

        public ulong NetworkId => NetworkObject != null ? NetworkObject.NetworkObjectId : 0;
        public string DisplayName => _displayName;
        float ITargetable.CurrentHealth => CurrentHealth.Value;
        float ITargetable.MaxHealth => MaxHealth.Value;
        public bool IsAlive => !IsDeadNetwork.Value;
        public Vector3 Position => transform.position;
        public Transform Transform => transform;
        public TargetType Type => _targetType;
        public float HealthPercent => GetHealthPercent();
        public int Level => _level;

        // Simple threat table - can be expanded
        private System.Collections.Generic.Dictionary<ulong, float> _threatTable = new();

        public float GetThreatTo(ulong playerId)
        {
            return _threatTable.TryGetValue(playerId, out float threat) ? threat : 0f;
        }

        #endregion

        #region IDamageable Implementation

        public bool IsDead => IsDeadNetwork.Value;

        public void TakeDamage(float damage)
        {
            TakeDamageInternal(damage, DamageType.Physical, 0);
        }

        public void TakeDamage(float damage, ulong sourceId)
        {
            TakeDamageInternal(damage, DamageType.Physical, sourceId);
        }

        public void TakeDamage(float amount, DamageType type, ulong attackerId)
        {
            TakeDamageInternal(amount, type, attackerId);
        }

        private void TakeDamageInternal(float amount, DamageType type, ulong attackerId)
        {
            if (!IsServer) return;
            if (IsDead) return;
            if (amount <= 0) return;

            // Apply damage
            float newHealth = Mathf.Max(0, CurrentHealth.Value - amount);
            CurrentHealth.Value = newHealth;

            // Update threat
            if (!_threatTable.ContainsKey(attackerId))
                _threatTable[attackerId] = 0f;
            _threatTable[attackerId] += amount;

            // Notify clients
            NotifyDamageTakenClientRpc(amount, type, attackerId);

            // Check death
            if (newHealth <= 0)
            {
                Die();
            }
        }

        #endregion

        #region Unity Lifecycle

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Initialize health on server
            if (IsServer)
            {
                MaxHealth.Value = _baseMaxHealth;
                CurrentHealth.Value = _baseMaxHealth;
            }

            // Subscribe to network variable changes
            CurrentHealth.OnValueChanged += HandleHealthChanged;
            MaxHealth.OnValueChanged += HandleMaxHealthChanged;
            IsDeadNetwork.OnValueChanged += HandleDeathStateChanged;
        }

        public override void OnNetworkDespawn()
        {
            CurrentHealth.OnValueChanged -= HandleHealthChanged;
            MaxHealth.OnValueChanged -= HandleMaxHealthChanged;
            IsDeadNetwork.OnValueChanged -= HandleDeathStateChanged;

            base.OnNetworkDespawn();
        }

        #endregion

        #region Public Methods (Server)

        /// <summary>
        /// Heals the entity. Server only.
        /// </summary>
        [Server]
        public void Heal(float amount, ulong healerId)
        {
            if (!IsServer) return;
            if (IsDead) return;
            if (amount <= 0) return;

            float newHealth = Mathf.Min(MaxHealth.Value, CurrentHealth.Value + amount);
            float actualHeal = newHealth - CurrentHealth.Value;
            CurrentHealth.Value = newHealth;

            if (actualHeal > 0)
            {
                NotifyHealingReceivedClientRpc(actualHeal, healerId);
            }
        }

        /// <summary>
        /// Sets the maximum health. Server only.
        /// </summary>
        [Server]
        public void SetMaxHealth(float max, bool healToFull = false)
        {
            if (!IsServer) return;
            if (max <= 0) return;

            MaxHealth.Value = max;
            
            if (healToFull)
            {
                CurrentHealth.Value = max;
            }
            else if (CurrentHealth.Value > max)
            {
                CurrentHealth.Value = max;
            }
        }

        /// <summary>
        /// Revives the entity with specified health percentage. Server only.
        /// </summary>
        [Server]
        public void Revive(float healthPercent = 1f)
        {
            if (!IsServer) return;
            if (!IsDead) return;

            IsDeadNetwork.Value = false;
            CurrentHealth.Value = MaxHealth.Value * Mathf.Clamp01(healthPercent);
            _threatTable.Clear();

            NotifyRevivedClientRpc();
        }

        /// <summary>
        /// Adds threat from a specific attacker. Server only.
        /// </summary>
        [Server]
        public void AddThreat(ulong attackerId, float amount)
        {
            if (!IsServer) return;
            
            if (!_threatTable.ContainsKey(attackerId))
                _threatTable[attackerId] = 0f;
            _threatTable[attackerId] += amount;
        }

        /// <summary>
        /// Clears all threat. Server only.
        /// </summary>
        [Server]
        public void ClearThreat()
        {
            if (!IsServer) return;
            _threatTable.Clear();
        }

        #endregion

        #region Public Methods (Client)

        /// <summary>
        /// Gets health as a percentage (0-1).
        /// </summary>
        public float GetHealthPercent()
        {
            if (MaxHealth.Value <= 0) return 0f;
            return CurrentHealth.Value / MaxHealth.Value;
        }

        #endregion

        #region Internal Methods

        private void Die()
        {
            if (!IsServer) return;

            IsDeadNetwork.Value = true;
            NotifyDeathClientRpc();
        }

        private void HandleHealthChanged(float previous, float current)
        {
            OnHealthChanged?.Invoke(current, MaxHealth.Value);
        }

        private void HandleMaxHealthChanged(float previous, float current)
        {
            OnHealthChanged?.Invoke(CurrentHealth.Value, current);
        }

        private void HandleDeathStateChanged(bool previous, bool current)
        {
            if (current && !previous)
            {
                // Just died
                OnDeath?.Invoke(this);
            }
        }

        #endregion

        #region RPCs

        [ClientRpc]
        private void NotifyDamageTakenClientRpc(float amount, DamageType type, ulong attackerId)
        {
            OnDamageTaken?.Invoke(amount, type, attackerId);
        }

        [ClientRpc]
        private void NotifyHealingReceivedClientRpc(float amount, ulong healerId)
        {
            OnHealingReceived?.Invoke(amount, healerId);
        }

        [ClientRpc]
        private void NotifyDeathClientRpc()
        {
            // OnDeath is handled by NetworkVariable change callback
        }

        [ClientRpc]
        private void NotifyRevivedClientRpc()
        {
            OnRevived?.Invoke();
        }

        #endregion

        #region Testing Support

        /// <summary>
        /// Sets display name for testing.
        /// </summary>
        public void SetDisplayName(string name)
        {
            _displayName = name;
        }

        /// <summary>
        /// Sets base max health for testing.
        /// </summary>
        public void SetBaseMaxHealth(float health)
        {
            _baseMaxHealth = health;
        }

        /// <summary>
        /// Forces health values for testing (bypasses network).
        /// </summary>
        public void ForceHealth(float current, float max)
        {
            if (IsServer || !IsSpawned)
            {
                CurrentHealth.Value = current;
                MaxHealth.Value = max;
            }
        }

        /// <summary>
        /// Forces death state for testing.
        /// </summary>
        public void ForceDeath(bool isDead)
        {
            if (IsServer || !IsSpawned)
            {
                IsDeadNetwork.Value = isDead;
            }
        }

        #endregion
    }

    /// <summary>
    /// Attribute to mark methods as server-only.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ServerAttribute : Attribute { }
}
