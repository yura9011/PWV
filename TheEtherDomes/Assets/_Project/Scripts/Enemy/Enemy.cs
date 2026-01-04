using Unity.Netcode;
using UnityEngine;
using EtherDomes.Combat;
using EtherDomes.Data;

namespace EtherDomes.Enemy
{
    /// <summary>
    /// Basic enemy entity that implements ITargetable for Tab-Target combat.
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
        
        // Network synced state
        private NetworkVariable<float> _currentHealth = new NetworkVariable<float>(
            100f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        
        private NetworkVariable<bool> _isAlive = new NetworkVariable<bool>(
            true,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        
        private bool _isTargeted;
        
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
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if (IsServer)
            {
                _currentHealth.Value = _maxHealth;
                _isAlive.Value = true;
            }
            
            // Subscribe to health changes for visual updates
            _currentHealth.OnValueChanged += OnHealthChanged;
            _isAlive.OnValueChanged += OnAliveChanged;
            
            // Hide target indicator initially
            SetTargetIndicator(false);
            
            // Register with TargetSystem
            RegisterWithTargetSystem();
            
            Debug.Log($"[Enemy] Spawned: {_displayName} (Level {_level})");
        }
        
        public override void OnNetworkDespawn()
        {
            // Unregister from TargetSystem
            UnregisterFromTargetSystem();
            
            _currentHealth.OnValueChanged -= OnHealthChanged;
            _isAlive.OnValueChanged -= OnAliveChanged;
            base.OnNetworkDespawn();
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
        /// Apply damage to this enemy. Can be called from any client.
        /// Routes to server for authoritative damage application.
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (!_isAlive.Value) return;
            
            if (IsServer)
            {
                // Server can apply damage directly
                ApplyDamageInternal(damage);
            }
            else
            {
                // Client requests damage via ServerRpc
                TakeDamageServerRpc(damage);
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void TakeDamageServerRpc(float damage)
        {
            ApplyDamageInternal(damage);
        }
        
        private void ApplyDamageInternal(float damage)
        {
            if (!_isAlive.Value) return;
            
            _currentHealth.Value = Mathf.Max(0, _currentHealth.Value - damage);
            Debug.Log($"[Enemy] {_displayName} took {damage} damage. Health: {_currentHealth.Value}/{_maxHealth}");
            
            if (_currentHealth.Value <= 0)
            {
                Die();
            }
        }
        
        /// <summary>
        /// Kill this enemy. Server authoritative.
        /// </summary>
        public void Die()
        {
            if (!IsServer) return;
            
            _isAlive.Value = false;
            _currentHealth.Value = 0;
            
            // Unregister from TargetSystem when dead
            UnregisterFromTargetSystem();
            
            // Notify clients to handle death visuals
            DieClientRpc();
        }
        
        [ClientRpc]
        private void DieClientRpc()
        {
            // Disable mesh or play death animation
            if (_meshRenderer != null)
            {
                _meshRenderer.enabled = false;
            }
            
            // Disable collider so it can't be targeted
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }
        
        /// <summary>
        /// Heal this enemy. Server authoritative.
        /// </summary>
        public void Heal(float amount)
        {
            if (!IsServer || !_isAlive.Value) return;
            
            _currentHealth.Value = Mathf.Min(_maxHealth, _currentHealth.Value + amount);
        }
        
        /// <summary>
        /// Set whether this enemy is currently targeted.
        /// </summary>
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
        
        private void OnHealthChanged(float previousValue, float newValue)
        {
            // Could trigger health bar update here
        }
        
        private void OnAliveChanged(bool previousValue, bool newValue)
        {
            if (!newValue)
            {
                // Enemy died - clear target indicator
                SetTargetIndicator(false);
            }
        }
        
        /// <summary>
        /// Reset enemy to full health. Server authoritative.
        /// </summary>
        public void Reset()
        {
            if (!IsServer) return;
            
            _currentHealth.Value = _maxHealth;
            _isAlive.Value = true;
            
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
            if (_maxHealth <= 0)
            {
                _maxHealth = 100f;
            }
            
            if (_level < 1)
            {
                _level = 1;
            }
        }
#endif
    }
}
