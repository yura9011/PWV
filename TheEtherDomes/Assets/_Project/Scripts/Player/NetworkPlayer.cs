using Mirror;
using EtherDomes.Combat;
using EtherDomes.Data;
using UnityEngine;

namespace EtherDomes.Player
{
    /// <summary>
    /// Main networked player component that integrates all player systems.
    /// Uses Mirror networking.
    /// </summary>
    [RequireComponent(typeof(NetworkIdentity))]
    public class NetworkPlayer : NetworkBehaviour, ITargetable
    {
        [Header("Player Info")]
        [SerializeField] private string _displayName = "Player";
        
        [Header("Combat")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private int _level = 1;

        // Network synced variables
        [SyncVar(hook = nameof(OnNameChanged))]
        private string _networkName = "Player";

        [SyncVar(hook = nameof(OnHealthChanged))]
        private float _networkHealth = 100f;

        [SyncVar]
        private float _networkMaxHealth = 100f;

        [SyncVar(hook = nameof(OnAliveChanged))]
        private bool _networkIsAlive = true;

        // ITargetable implementation
        public ulong NetworkId => netId;
        public string DisplayName => _networkName;
        public Vector3 Position => transform.position;
        public bool IsAlive => _networkIsAlive;
        public TargetType Type => TargetType.Friendly;
        public int Level => _level;
        public Transform Transform => transform;

        // Properties
        public float Health => _networkHealth;
        public float CurrentHealth => _networkHealth;
        public float MaxHealth => _networkMaxHealth;
        public float HealthPercent => _networkMaxHealth > 0 ? _networkHealth / _networkMaxHealth : 0f;

        public override void OnStartServer()
        {
            base.OnStartServer();
            _networkHealth = _maxHealth;
            _networkMaxHealth = _maxHealth;
            _networkIsAlive = true;
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            SetupLocalPlayer();
            Debug.Log($"[NetworkPlayer] Local player spawned: {_displayName}");
        }

        private void SetupLocalPlayer()
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0 && playerLayer <= 31)
            {
                gameObject.layer = playerLayer;
            }
        }

        private void OnNameChanged(string oldValue, string newValue)
        {
            _displayName = newValue;
        }

        private void OnHealthChanged(float oldValue, float newValue)
        {
            Debug.Log($"[NetworkPlayer] {_displayName} health: {oldValue} -> {newValue}");
        }

        private void OnAliveChanged(bool oldValue, bool newValue)
        {
            if (!newValue)
            {
                OnDeath();
            }
            else if (!oldValue && newValue)
            {
                OnResurrect();
            }
        }

        private void OnDeath()
        {
            Debug.Log($"[NetworkPlayer] {_displayName} died");
        }

        private void OnResurrect()
        {
            Debug.Log($"[NetworkPlayer] {_displayName} resurrected");
        }

        [Command]
        public void CmdSetName(string name)
        {
            _networkName = name;
            _displayName = name;
        }

        [Command(requiresAuthority = false)]
        public void CmdTakeDamage(float damage)
        {
            if (!_networkIsAlive) return;
            _networkHealth = Mathf.Max(0, _networkHealth - damage);
            if (_networkHealth <= 0)
            {
                _networkIsAlive = false;
            }
        }

        [Command(requiresAuthority = false)]
        public void CmdHeal(float amount)
        {
            if (!_networkIsAlive) return;
            _networkHealth = Mathf.Min(_networkMaxHealth, _networkHealth + amount);
        }

        [Command(requiresAuthority = false)]
        public void CmdResurrect()
        {
            CmdResurrectWithHealth(0.5f);
        }

        [Command(requiresAuthority = false)]
        public void CmdResurrectWithHealth(float healthPercent)
        {
            if (_networkIsAlive) return;
            _networkHealth = _networkMaxHealth * Mathf.Clamp01(healthPercent);
            _networkIsAlive = true;
        }

        public void InitializeFromCharacterData(CharacterData data)
        {
            if (data == null) return;
            _displayName = data.CharacterName;
            _maxHealth = data.BaseStats?.MaxHealth ?? 100f;
            
            if (isServer)
            {
                _networkName = data.CharacterName;
                _networkMaxHealth = _maxHealth;
                _networkHealth = _maxHealth;
            }
        }
    }
}
