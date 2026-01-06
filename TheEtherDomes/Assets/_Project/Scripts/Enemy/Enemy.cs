using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using EtherDomes.Combat;
using EtherDomes.Data;

namespace EtherDomes.Enemy
{
    /// <summary>
    /// Basic enemy entity that implements ITargetable for Tab-Target combat.
    /// Includes damage tracking for target selection based on highest damage dealer.
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
        [SyncVar(hook = nameof(OnHealthChanged))]
        private float _currentHealth = 100f;
        
        [SyncVar(hook = nameof(OnAliveChanged))]
        private bool _isAlive = true;
        
        private bool _isTargeted;
        
        // Damage tracking for target selection
        private readonly Dictionary<uint, float> _damageByPlayer = new();
        
        // ITargetable implementation
        public ulong NetworkId => netId;
        public string DisplayName => _displayName;
        public Vector3 Position => transform.position;
        public bool IsAlive => _isAlive;
        public TargetType Type => TargetType.Enemy;
        public float HealthPercent => _maxHealth > 0 ? _currentHealth / _maxHealth : 0f;
        public int Level => _level;
        public Transform Transform => transform;
        
        // Public properties
        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            _currentHealth = _maxHealth;
            _isAlive = true;
        }
        
        public override void OnStartClient()
        {
            base.OnStartClient();
            SetTargetIndicator(false);
            RegisterWithTargetSystem();
            Debug.Log($"[Enemy] Spawned: {_displayName} (Level {_level})");
        }
        
        public override void OnStopClient()
        {
            UnregisterFromTargetSystem();
            base.OnStopClient();
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
        public void TakeDamage(float damage, uint sourcePlayerId)
        {
            if (!_isAlive) return;
            
            if (isServer)
            {
                ApplyDamageInternal(damage, sourcePlayerId);
            }
            else
            {
                CmdTakeDamage(damage, sourcePlayerId);
            }
        }
        
        [Command(requiresAuthority = false)]
        private void CmdTakeDamage(float damage, uint sourcePlayerId)
        {
            ApplyDamageInternal(damage, sourcePlayerId);
        }
        
        private void ApplyDamageInternal(float damage, uint sourcePlayerId)
        {
            if (!_isAlive) return;
            
            if (sourcePlayerId != 0)
            {
                RecordDamage(sourcePlayerId, damage);
            }
            
            _currentHealth = Mathf.Max(0, _currentHealth - damage);
            Debug.Log($"[Enemy] {_displayName} took {damage} damage. Health: {_currentHealth}/{_maxHealth}");
            
            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        public void RecordDamage(uint playerId, float damage)
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

        public float GetTotalDamage(uint playerId)
        {
            return _damageByPlayer.TryGetValue(playerId, out float damage) ? damage : 0f;
        }

        public uint GetHighestDamageDealer()
        {
            if (_damageByPlayer.Count == 0)
                return 0;

            return _damageByPlayer.OrderByDescending(kvp => kvp.Value).First().Key;
        }

        public void ClearDamageTracking()
        {
            _damageByPlayer.Clear();
        }
        
        [Server]
        public void Die()
        {
            _isAlive = false;
            _currentHealth = 0;
            UnregisterFromTargetSystem();
            RpcDie();
        }
        
        [ClientRpc]
        private void RpcDie()
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
        
        [Server]
        public void Heal(float amount)
        {
            if (!_isAlive) return;
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
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
            // Health bar update
        }
        
        private void OnAliveChanged(bool oldValue, bool newValue)
        {
            if (!newValue)
            {
                SetTargetIndicator(false);
            }
        }
        
        [Server]
        public void Reset()
        {
            _currentHealth = _maxHealth;
            _isAlive = true;
            ClearDamageTracking();
            RpcReset();
        }
        
        [ClientRpc]
        private void RpcReset()
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
        private new void OnValidate()
        {
            if (_maxHealth <= 0) _maxHealth = 100f;
            if (_level < 1) _level = 1;
        }
#endif
    }
}
