using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using EtherDomes.Combat;
using EtherDomes.Data;

namespace EtherDomes.Enemy
{
    /// <summary>
    /// Basic enemy entity that implements ITargetable for Tab-Target combat.
    /// Includes damage tracking for target selection.
    /// Uses Unity Netcode for GameObjects (NGO).
    /// </summary>
    public class Enemy : NetworkBehaviour, ITargetable, ITargetIndicator
    {
        [Header("Identity")]
        [SerializeField] private string _displayName = "Enemy";
        [SerializeField] private int _level = 1;
        
        [Header("Stats")]
        [SerializeField] private float _maxHealth = 100f;
        
        [Header("Visual")]
        [SerializeField] private GameObject _targetIndicator;
        [SerializeField] private MeshRenderer _meshRenderer;
        
        // NGO Network Variables
        private NetworkVariable<float> _currentHealth = new NetworkVariable<float>(100f);
        private NetworkVariable<bool> _isAlive = new NetworkVariable<bool>(true);
        
        private bool _isTargeted;
        
        // Damage tracking: Key is now ulong for NGO ClientId
        private readonly Dictionary<ulong, float> _damageByPlayer = new();
        
        // ITargetable implementation
        public ulong NetworkId => NetworkObjectId;
        public string DisplayName => _displayName;
        public Vector3 Position => transform.position;
        public bool IsAlive => _isAlive.Value;
        public TargetType Type => TargetType.Enemy;
        public float HealthPercent => _maxHealth > 0 ? _currentHealth.Value / _maxHealth : 0f;
        public int Level => _level;
        public Transform Transform => transform;
        
        // Public properties
        public float CurrentHealth => _currentHealth.Value;
        public float MaxHealth => _maxHealth;
        
        // ITargetable events
        public event System.Action<ITargetable> OnDeath;
        
        /// <summary>
        /// Gets the threat level this enemy has toward a specific player.
        /// Based on damage dealt by that player.
        /// </summary>
        public float GetThreatTo(ulong playerId) => GetTotalDamage(playerId);
        
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                _currentHealth.Value = _maxHealth;
                _isAlive.Value = true;
            }

            _currentHealth.OnValueChanged += OnHealthChanged;
            _isAlive.OnValueChanged += OnAliveChanged;

            SetTargetIndicator(false);
            RegisterWithTargetSystem();
            Debug.Log($"[Enemy] Spawned: {_displayName} (Level {_level})");
        }
        
        public override void OnNetworkDespawn()
        {
            UnregisterFromTargetSystem();
            _currentHealth.OnValueChanged -= OnHealthChanged;
            _isAlive.OnValueChanged -= OnAliveChanged;
        }
        
        private void RegisterWithTargetSystem()
        {
            var targetSystem = FindFirstObjectByType<TargetSystem>();
            if (targetSystem != null)
            {
                targetSystem.RegisterTarget(this);
            }
        }
        
        private void UnregisterFromTargetSystem()
        {
            var targetSystem = FindFirstObjectByType<TargetSystem>();
            if (targetSystem != null)
            {
                targetSystem.UnregisterTarget(this);
            }
        }
        
        /// <summary>
        /// Apply damage to this enemy.
        /// </summary>
        public void TakeDamage(float damage)
        {
            TakeDamage(damage, 0);
        }

        /// <summary>
        /// Apply damage to this enemy from a specific player.
        /// </summary>
        public void TakeDamage(float damage, ulong sourcePlayerId)
        {
            if (!_isAlive.Value) return;
            
            if (IsServer)
            {
                ApplyDamageInternal(damage, sourcePlayerId);
            }
            else
            {
                TakeDamageServerRpc(damage, sourcePlayerId);
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void TakeDamageServerRpc(float damage, ulong sourcePlayerId)
        {
            ApplyDamageInternal(damage, sourcePlayerId);
        }
        
        private void ApplyDamageInternal(float damage, ulong sourcePlayerId)
        {
            if (!_isAlive.Value) return;
            
            if (sourcePlayerId != 0)
            {
                RecordDamage(sourcePlayerId, damage);
            }
            
            float newHealth = Mathf.Max(0, _currentHealth.Value - damage);
            _currentHealth.Value = newHealth;

            Debug.Log($"[Enemy] {_displayName} took {damage} damage. Health: {newHealth}/{_maxHealth}");
            
            // Show floating combat text on all clients
            ShowDamageClientRpc(damage, transform.position);
            
            if (newHealth <= 0)
            {
                Die();
            }
        }

        public void RecordDamage(ulong playerId, float damage)
        {
            if (damage <= 0) return;
            
            if (_damageByPlayer.ContainsKey(playerId))
            {
                _damageByPlayer[playerId] += damage;
            }
            else
            {
                _damageByPlayer[playerId] = damage;
            }
        }

        public float GetTotalDamage(ulong playerId)
        {
            return _damageByPlayer.TryGetValue(playerId, out float damage) ? damage : 0f;
        }

        public ulong GetHighestDamageDealer()
        {
            if (_damageByPlayer.Count == 0)
                return 0;

            return _damageByPlayer.OrderByDescending(kvp => kvp.Value).First().Key;
        }

        public void ClearDamageTracking()
        {
            _damageByPlayer.Clear();
        }
        
        public void Die()
        {
            if (!IsServer) return;

            _isAlive.Value = false;
            _currentHealth.Value = 0;
            UnregisterFromTargetSystem();
            OnDeath?.Invoke(this);
            DieClientRpc();
        }
        
        [ClientRpc]
        private void DieClientRpc()
        {
            if (_meshRenderer != null)
            {
                _meshRenderer.enabled = false;
            }
            
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }
        
        [ClientRpc]
        private void ShowDamageClientRpc(float damage, Vector3 position)
        {
            CombatEvents.RaiseDamageDealt(position, damage);
        }
        
        public void Heal(float amount)
        {
            if (!IsServer || !_isAlive.Value) return;
            _currentHealth.Value = Mathf.Min(_maxHealth, _currentHealth.Value + amount);
        }
        
        public void SetTargeted(bool targeted)
        {
            _isTargeted = targeted;
            SetTargetIndicator(targeted);
        }
        
        private void SetTargetIndicator(bool visible)
        {
            if (_targetIndicator != null)
            {
                _targetIndicator.SetActive(visible);
            }
        }
        
        private void OnHealthChanged(float oldValue, float newValue)
        {
            // Health bar update logic here
        }
        
        private void OnAliveChanged(bool oldValue, bool newValue)
        {
            if (!newValue)
            {
                SetTargetIndicator(false);
            }
        }
        
        public void Reset()
        {
            if (!IsServer) return;

            _currentHealth.Value = _maxHealth;
            _isAlive.Value = true;
            ClearDamageTracking();
            ResetClientRpc();
        }
        
        [ClientRpc]
        private void ResetClientRpc()
        {
            if (_meshRenderer != null)
            {
                _meshRenderer.enabled = true;
            }
            
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = true;
            }
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_maxHealth <= 0) _maxHealth = 100f;
            if (_level < 1) _level = 1;
        }
#endif
    }
}
