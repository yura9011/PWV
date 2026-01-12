using System;
using UnityEngine;
using Unity.Netcode;
using EtherDomes.Combat.Abilities;
using EtherDomes.Combat.Targeting;
using EtherDomes.Data;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Facade providing unified API for combat operations.
    /// Coordinates all combat subsystems.
    /// </summary>
    public class CombatManager : NetworkBehaviour
    {
        #region Singleton

        private static CombatManager _instance;
        public static CombatManager Instance => _instance;

        #endregion

        #region Configuration

        [Header("Configuration")]
        [SerializeField] private CombatConfigSO _config;

        #endregion

        #region Components

        [Header("Components")]
        [SerializeField] private TargetingSystem _targeting;
        [SerializeField] private CombatStateMachine _stateMachine;
        [SerializeField] private SpellQueue _spellQueue;
        [SerializeField] private AbilityExecutor _abilityExecutor;
        [SerializeField] private HealthSystem _healthSystem;
        [SerializeField] private SecondaryResourceSystem _resourceSystem;

        // Public accessors
        public TargetingSystem Targeting => _targeting;
        public CombatStateMachine StateMachine => _stateMachine;
        public SpellQueue SpellQueue => _spellQueue;
        public AbilityExecutor AbilityExecutor => _abilityExecutor;
        public HealthSystem Health => _healthSystem;
        public SecondaryResourceSystem Resources => _resourceSystem;

        #endregion

        #region State

        private bool _initialized;
        private bool _inCombat;
        private float _combatDropoffTimer;

        /// <summary>Returns true if player is in combat.</summary>
        public bool InCombat => _inCombat;

        /// <summary>Current combat state.</summary>
        public CombatState CurrentState => _stateMachine?.CurrentState ?? CombatState.Idle;

        /// <summary>Current target.</summary>
        public ITargetable CurrentTarget => _targeting?.CurrentTarget;

        #endregion

        #region Events

        /// <summary>Fired when entering combat.</summary>
        public event Action OnEnterCombat;

        /// <summary>Fired when leaving combat.</summary>
        public event Action OnLeaveCombat;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            if (_config == null)
                _config = CombatConfigSO.Instance;
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            if (!IsOwner) return;

            UpdateCombatState();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Initialize();
        }

        public override void OnNetworkDespawn()
        {
            Cleanup();
            base.OnNetworkDespawn();
        }

        public override void OnDestroy()
        {
            if (_instance == this)
                _instance = null;

            Cleanup();
            base.OnDestroy();
        }

        #endregion

        #region Initialization

        private void Initialize()
        {
            if (_initialized) return;

            // Auto-find components if not set
            if (_targeting == null) _targeting = GetComponent<TargetingSystem>();
            if (_stateMachine == null) _stateMachine = GetComponent<CombatStateMachine>();
            if (_spellQueue == null) _spellQueue = GetComponent<SpellQueue>();
            if (_abilityExecutor == null) _abilityExecutor = GetComponent<AbilityExecutor>();
            if (_healthSystem == null) _healthSystem = GetComponent<HealthSystem>();
            if (_resourceSystem == null) _resourceSystem = GetComponent<SecondaryResourceSystem>();

            // Wire up events
            if (_abilityExecutor != null)
            {
                _abilityExecutor.OnAbilityExecuted += HandleAbilityExecuted;
            }

            if (_healthSystem != null)
            {
                _healthSystem.OnDamageTaken += HandleDamageTaken;
            }

            if (_targeting != null)
            {
                _targeting.OnTargetChanged += HandleTargetChanged;
            }

            _initialized = true;
        }

        private void Cleanup()
        {
            if (_abilityExecutor != null)
            {
                _abilityExecutor.OnAbilityExecuted -= HandleAbilityExecuted;
            }

            if (_healthSystem != null)
            {
                _healthSystem.OnDamageTaken -= HandleDamageTaken;
            }

            if (_targeting != null)
            {
                _targeting.OnTargetChanged -= HandleTargetChanged;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Cycles to next target using Tab algorithm.
        /// </summary>
        public void TabTarget()
        {
            _targeting?.TabTarget();
        }

        /// <summary>
        /// Sets a specific target.
        /// </summary>
        public void SetTarget(ITargetable target)
        {
            _targeting?.SetTarget(target);
        }

        /// <summary>
        /// Clears current target.
        /// </summary>
        public void ClearTarget()
        {
            _targeting?.ClearTarget();
        }

        /// <summary>
        /// Uses ability from slot.
        /// </summary>
        public void UseAbility(int slot)
        {
            _abilityExecutor?.TryUseAbility(slot);
        }

        /// <summary>
        /// Interrupts current cast.
        /// </summary>
        public void InterruptCast()
        {
            _stateMachine?.InterruptCast();
        }

        /// <summary>
        /// Called when player moves. Interrupts stationary casts.
        /// </summary>
        public void OnPlayerMove()
        {
            _stateMachine?.OnMovementDetected();
        }

        /// <summary>
        /// Checks if ability in slot can be used.
        /// </summary>
        public bool CanUseAbility(int slot)
        {
            if (_abilityExecutor == null) return false;
            if (slot < 0 || slot >= _abilityExecutor.AbilitySlots.Length) return false;

            var ability = _abilityExecutor.AbilitySlots[slot];
            if (ability == null) return false;

            // Check state machine
            if (_stateMachine != null && !_stateMachine.CanUseAbility(ability.IsOffGCD))
                return false;

            // Check cooldown
            if (!_abilityExecutor.IsAbilityReady(slot))
                return false;

            // Check resources
            if (_resourceSystem != null && ability.ResourceCost > 0)
            {
                if (_resourceSystem.GetResource(OwnerClientId) < ability.ResourceCost)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Gets ability definition from slot.
        /// </summary>
        public AbilityDefinitionSO GetAbility(int slot)
        {
            if (_abilityExecutor == null) return null;
            if (slot < 0 || slot >= _abilityExecutor.AbilitySlots.Length) return null;
            return _abilityExecutor.AbilitySlots[slot];
        }

        /// <summary>
        /// Gets cooldown remaining for slot.
        /// </summary>
        public float GetCooldown(int slot)
        {
            return _abilityExecutor?.GetCooldownRemaining(slot) ?? 0f;
        }

        #endregion

        #region Combat State

        private void UpdateCombatState()
        {
            if (_inCombat)
            {
                _combatDropoffTimer -= Time.deltaTime;
                if (_combatDropoffTimer <= 0f)
                {
                    LeaveCombat();
                }
            }
        }

        private void EnterCombat()
        {
            if (_inCombat) return;

            _inCombat = true;
            _combatDropoffTimer = _config?.CombatDropoffTime ?? 5f;
            OnEnterCombat?.Invoke();
        }

        private void LeaveCombat()
        {
            if (!_inCombat) return;

            _inCombat = false;
            OnLeaveCombat?.Invoke();
        }

        private void ResetCombatTimer()
        {
            _combatDropoffTimer = _config?.CombatDropoffTime ?? 5f;
            if (!_inCombat)
            {
                EnterCombat();
            }
        }

        #endregion

        #region Event Handlers

        private void HandleAbilityExecuted(AbilityDefinitionSO ability, ITargetable target)
        {
            if (ability.IsOffensive)
            {
                ResetCombatTimer();
            }
        }

        private void HandleDamageTaken(float amount, DamageType type, ulong attackerId)
        {
            ResetCombatTimer();
        }

        private void HandleTargetChanged(ITargetable target)
        {
            // Could trigger combat if targeting enemy
        }

        #endregion

        #region Testing Support

        public void SetConfig(CombatConfigSO config) => _config = config;
        public void SetTargeting(TargetingSystem targeting) => _targeting = targeting;
        public void SetStateMachine(CombatStateMachine stateMachine) => _stateMachine = stateMachine;
        public void SetSpellQueue(SpellQueue spellQueue) => _spellQueue = spellQueue;
        public void SetAbilityExecutor(AbilityExecutor executor) => _abilityExecutor = executor;
        public void SetHealthSystem(HealthSystem health) => _healthSystem = health;
        public void SetResourceSystem(SecondaryResourceSystem resources) => _resourceSystem = resources;

        #endregion
    }
}
