using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using EtherDomes.Combat;
using EtherDomes.Data;

namespace EtherDomes.Player
{
    /// <summary>
    /// Main networked player component that integrates all player systems.
    /// Uses Unity Netcode for GameObjects (NGO).
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkPlayer : NetworkBehaviour, ITargetable
    {
        [Header("Player Info")]
        [SerializeField] private string _displayName = "Player";
        
        [Header("Combat")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private int _level = 1;

        // NGO NetworkVariables
        // Native types or INetworkSerializable are required. Strings need FixedString.
        // Or we catch the OnValueChanged to update local string.
        
        private NetworkVariable<FixedString64Bytes> _networkName = new NetworkVariable<FixedString64Bytes>(
            "Player", 
            NetworkVariableReadPermission.Everyone, 
            NetworkVariableWritePermission.Server
        );

        private NetworkVariable<float> _networkHealth = new NetworkVariable<float>(100f);
        private NetworkVariable<float> _networkMaxHealth = new NetworkVariable<float>(100f);
        private NetworkVariable<bool> _networkIsAlive = new NetworkVariable<bool>(true);

        // ITargetable implementation
        public ulong NetworkId => NetworkObjectId;
        public string DisplayName => _networkName.Value.ToString();
        public Vector3 Position => transform.position;
        public bool IsAlive => _networkIsAlive.Value;
        public TargetType Type => TargetType.Friendly;
        public int Level => _level;
        public Transform Transform => transform;

        // Properties
        public float Health => _networkHealth.Value;
        public float CurrentHealth => _networkHealth.Value;
        public float MaxHealth => _networkMaxHealth.Value;
        public float HealthPercent => _networkMaxHealth.Value > 0 ? _networkHealth.Value / _networkMaxHealth.Value : 0f;
        
        // ITargetable events
        public event System.Action<ITargetable> OnDeath;
        
        /// <summary>
        /// Gets the threat level this player has toward a specific entity.
        /// Players don't generate threat toward others in this implementation.
        /// </summary>
        public float GetThreatTo(ulong playerId) => 0f;

        public override void OnNetworkSpawn()
        {
            // Subscribe to value changes
            _networkName.OnValueChanged += OnNameChanged;
            _networkHealth.OnValueChanged += OnHealthChanged;
            _networkIsAlive.OnValueChanged += OnAliveChanged;

            if (IsServer)
            {
                _networkHealth.Value = _maxHealth;
                _networkMaxHealth.Value = _maxHealth;
                _networkIsAlive.Value = true;
            }

            if (IsOwner)
            {
                SetupLocalPlayer();
                Debug.Log($"[NetworkPlayer] Local player spawned: {DisplayName}");
            }
        }

        public override void OnNetworkDespawn()
        {
            _networkName.OnValueChanged -= OnNameChanged;
            _networkHealth.OnValueChanged -= OnHealthChanged;
            _networkIsAlive.OnValueChanged -= OnAliveChanged;
        }

        private void SetupLocalPlayer()
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0 && playerLayer <= 31)
            {
                gameObject.layer = playerLayer;
            }
        }

        private void OnNameChanged(FixedString64Bytes oldValue, FixedString64Bytes newValue)
        {
            _displayName = newValue.ToString();
        }

        private void OnHealthChanged(float oldValue, float newValue)
        {
            // Debug.Log($"[NetworkPlayer] {_displayName} health: {oldValue} -> {newValue}");
        }

        private void OnAliveChanged(bool oldValue, bool newValue)
        {
            if (!newValue)
            {
                HandleDeath();
            }
            else if (!oldValue && newValue)
            {
                OnResurrect();
            }
        }

        private void HandleDeath()
        {
            Debug.Log($"[NetworkPlayer] {DisplayName} died");
            OnDeath?.Invoke(this);
        }

        private void OnResurrect()
        {
            Debug.Log($"[NetworkPlayer] {DisplayName} resurrected");
        }

        [ServerRpc]
        public void SetNameServerRpc(string name)
        {
            _networkName.Value = new FixedString64Bytes(name);
            _displayName = name;
        }

        /// <summary>
        /// Public method to apply damage (called by CombatSystem or Local Player).
        /// If client, sends RPC. If Server, applies directly.
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (IsServer)
            {
                ApplyDamageServer(damage);
            }
            else
            {
                TakeDamageServerRpc(damage);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void TakeDamageServerRpc(float damage)
        {
            ApplyDamageServer(damage);
        }

        private void ApplyDamageServer(float damage)
        {
            if (!_networkIsAlive.Value) return;
            
            float newHealth = Mathf.Max(0, _networkHealth.Value - damage);
            _networkHealth.Value = newHealth;
            
            if (newHealth <= 0)
            {
                _networkIsAlive.Value = false;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void HealServerRpc(float amount)
        {
             if (!_networkIsAlive.Value) return;
            _networkHealth.Value = Mathf.Min(_networkMaxHealth.Value, _networkHealth.Value + amount);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ResurrectServerRpc()
        {
            ResurrectWithHealthServerRpc(0.5f);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ResurrectWithHealthServerRpc(float healthPercent)
        {
            if (_networkIsAlive.Value) return;
            _networkHealth.Value = _networkMaxHealth.Value * Mathf.Clamp01(healthPercent);
            _networkIsAlive.Value = true;
        }

        public void InitializeFromCharacterData(CharacterData data)
        {
            if (data == null) return;
            // Initialization logic, typically called by Server immediately after spawn
            if (IsServer)
            {
                _networkName.Value = new FixedString64Bytes(data.Name);
                _maxHealth = data.MaxHP > 0 ? data.MaxHP : 100f;
                _networkMaxHealth.Value = _maxHealth;
                _networkHealth.Value = _maxHealth;
            }
        }
    }
}
