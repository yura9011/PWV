using Unity.Collections;
using Unity.Netcode;
using EtherDomes.Combat;
using EtherDomes.Data;
using UnityEngine;

namespace EtherDomes.Player
{
    /// <summary>
    /// Main networked player component that integrates all player systems.
    /// Attached to the Player Prefab with NetworkObject.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(PlayerController))]
    public class NetworkPlayer : NetworkBehaviour, ITargetable
    {
        [Header("Player Info")]
        [SerializeField] private string _displayName = "Player";
        
        [Header("Combat")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _currentHealth = 100f;
        [SerializeField] private int _level = 1;

        // Network synced variables
        private NetworkVariable<FixedString64Bytes> _networkName = new NetworkVariable<FixedString64Bytes>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private NetworkVariable<float> _networkHealth = new NetworkVariable<float>(
            100f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private NetworkVariable<float> _networkMaxHealth = new NetworkVariable<float>(
            100f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private NetworkVariable<bool> _networkIsAlive = new NetworkVariable<bool>(
            true,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        // Components
        private PlayerController _playerController;

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
        public PlayerController Controller => _playerController;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Subscribe to health changes
            _networkHealth.OnValueChanged += OnHealthChanged;
            _networkIsAlive.OnValueChanged += OnAliveChanged;

            if (IsServer)
            {
                // Initialize server-side values
                _networkHealth.Value = _maxHealth;
                _networkMaxHealth.Value = _maxHealth;
                _networkIsAlive.Value = true;
            }

            if (IsOwner)
            {
                // Setup local player
                SetupLocalPlayer();
            }

            Debug.Log($"[NetworkPlayer] Spawned: {_displayName} (Owner: {IsOwner}, Server: {IsServer})");
        }

        public override void OnNetworkDespawn()
        {
            _networkHealth.OnValueChanged -= OnHealthChanged;
            _networkIsAlive.OnValueChanged -= OnAliveChanged;
            base.OnNetworkDespawn();
        }

        private void SetupLocalPlayer()
        {
            // Configure camera for local player
            if (Camera.main != null)
            {
                _playerController.SetCameraTransform(Camera.main.transform);
            }

            // Configure physics layer (only if "Player" layer exists)
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0 && playerLayer <= 31)
            {
                gameObject.layer = playerLayer;
            }
            else
            {
                Debug.LogWarning("[NetworkPlayer] 'Player' layer not found. Create it in Edit > Project Settings > Tags and Layers");
            }
        }


        private void OnHealthChanged(float previousValue, float newValue)
        {
            Debug.Log($"[NetworkPlayer] {_displayName} health: {previousValue} -> {newValue}");
        }

        private void OnAliveChanged(bool previousValue, bool newValue)
        {
            if (!newValue)
            {
                OnDeath();
            }
            else if (previousValue == false && newValue == true)
            {
                OnResurrect();
            }
        }

        private void OnDeath()
        {
            Debug.Log($"[NetworkPlayer] {_displayName} died");
            _playerController?.StopMovement();
        }

        private void OnResurrect()
        {
            Debug.Log($"[NetworkPlayer] {_displayName} resurrected");
        }

        /// <summary>
        /// Set player name (server only).
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void SetNameServerRpc(string name)
        {
            _networkName.Value = name;
            _displayName = name;
        }

        /// <summary>
        /// Apply damage to this player (server only).
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void TakeDamageServerRpc(float damage)
        {
            if (!_networkIsAlive.Value) return;

            _networkHealth.Value = Mathf.Max(0, _networkHealth.Value - damage);

            if (_networkHealth.Value <= 0)
            {
                _networkIsAlive.Value = false;
            }
        }

        /// <summary>
        /// Heal this player (server only).
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void HealServerRpc(float amount)
        {
            if (!_networkIsAlive.Value) return;

            _networkHealth.Value = Mathf.Min(_networkMaxHealth.Value, _networkHealth.Value + amount);
        }

        /// <summary>
        /// Resurrect this player (server only).
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ResurrectServerRpc(float healthPercent = 0.5f)
        {
            if (_networkIsAlive.Value) return;

            _networkHealth.Value = _networkMaxHealth.Value * Mathf.Clamp01(healthPercent);
            _networkIsAlive.Value = true;
        }

        /// <summary>
        /// Set max health and optionally heal to full (server only).
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void SetMaxHealthServerRpc(float maxHealth, bool healToFull = false)
        {
            _networkMaxHealth.Value = Mathf.Max(1, maxHealth);
            
            if (healToFull)
            {
                _networkHealth.Value = _networkMaxHealth.Value;
            }
            else
            {
                // Clamp current health to new max
                _networkHealth.Value = Mathf.Min(_networkHealth.Value, _networkMaxHealth.Value);
            }
        }

        /// <summary>
        /// Initialize player with character data.
        /// </summary>
        public void InitializeFromCharacterData(CharacterData data)
        {
            if (data == null) return;

            _displayName = data.CharacterName;
            _maxHealth = data.BaseStats?.MaxHealth ?? 100f;
            _currentHealth = data.BaseStats?.Health ?? _maxHealth;

            if (IsServer)
            {
                _networkName.Value = data.CharacterName;
                _networkMaxHealth.Value = _maxHealth;
                _networkHealth.Value = _currentHealth;
            }
        }
    }
}
