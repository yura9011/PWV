using System;
using UnityEngine;
using Unity.Netcode;
using EtherDomes.Data;
using EtherDomes.Combat.Targeting;

namespace EtherDomes.Combat.Abilities
{
    // Import SpellQueue from parent namespace
    using EtherDomes.Combat;

    /// <summary>
    /// Handles ability execution with server-authoritative validation.
    /// Manages ability slots, cooldowns, and network RPCs.
    /// </summary>
    public class AbilityExecutor : NetworkBehaviour
    {
        #region Configuration

        [Header("Configuration")]
        [SerializeField] private CombatConfigSO _config;

        [Header("Ability Slots")]
        [SerializeField] private AbilityDefinitionSO[] _abilitySlots = new AbilityDefinitionSO[10];

        [Header("Dependencies")]
        [SerializeField] private TargetingSystem _targeting;
        [SerializeField] private CombatStateMachine _stateMachine;
        [SerializeField] private SpellQueue _spellQueue;
        [SerializeField] private SecondaryResourceSystem _resourceSystem;
        [SerializeField] private HealthSystem _healthSystem;

        #endregion

        #region State

        private float[] _cooldowns = new float[10];
        private bool _initialized;

        /// <summary>Ability slots array.</summary>
        public AbilityDefinitionSO[] AbilitySlots => _abilitySlots;

        #endregion

        #region Events

        /// <summary>Fired when cooldown starts (slot, duration).</summary>
        public event Action<int, float> OnCooldownStarted;

        /// <summary>Fired when ability executes successfully.</summary>
        public event Action<AbilityDefinitionSO, ITargetable> OnAbilityExecuted;

        /// <summary>Fired when ability fails with error message.</summary>
        public event Action<string> OnAbilityError;

        /// <summary>Fired when cast starts (for UI).</summary>
        public event Action<AbilityDefinitionSO, ITargetable> OnCastStarted;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
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
            UpdateCooldowns(Time.deltaTime);
            CheckQueuedAbility();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Initialize();
        }

        #endregion

        #region Initialization

        private void Initialize()
        {
            if (_initialized) return;

            // Auto-find dependencies if not set
            if (_targeting == null) _targeting = GetComponent<TargetingSystem>();
            if (_stateMachine == null) _stateMachine = GetComponent<CombatStateMachine>();
            if (_spellQueue == null) _spellQueue = GetComponent<SpellQueue>();
            if (_resourceSystem == null) _resourceSystem = GetComponent<SecondaryResourceSystem>();
            if (_healthSystem == null) _healthSystem = GetComponent<HealthSystem>();

            // Subscribe to cast completion
            if (_stateMachine != null)
            {
                _stateMachine.OnCastCompleted += HandleCastCompleted;
            }

            _initialized = true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Attempts to use an ability from a slot.
        /// </summary>
        public void TryUseAbility(int slot)
        {
            if (!IsOwner) return;
            if (slot < 0 || slot >= _abilitySlots.Length) return;

            var ability = _abilitySlots[slot];
            if (ability == null) return;

            // Check if we should queue instead
            if (_stateMachine != null && !_stateMachine.CanUseAbility(ability.IsOffGCD))
            {
                // Try to queue
                if (_spellQueue != null && _spellQueue.TryQueueAbility(ability, slot))
                {
                    return; // Successfully queued
                }
                
                OnAbilityError?.Invoke("No puedes usar esa habilidad ahora");
                return;
            }

            // Local validation (client-side prediction)
            string error;
            if (!ValidateAbilityLocal(slot, out error))
            {
                OnAbilityError?.Invoke(error);
                return;
            }

            // Get target reference
            NetworkObjectReference targetRef = default;
            if (ability.IsOffensive && _targeting != null && _targeting.CurrentTarget != null)
            {
                // Get NetworkObject from target's Transform (ITargetable has NetworkId, not NetworkObject)
                var targetNetObj = _targeting.CurrentTarget.Transform?.GetComponent<NetworkObject>();
                if (targetNetObj != null)
                    targetRef = targetNetObj;
            }

            // Send to server
            RequestCastServerRpc(slot, targetRef);
        }

        /// <summary>
        /// Returns true if ability in slot is ready (off cooldown).
        /// </summary>
        public bool IsAbilityReady(int slot)
        {
            if (slot < 0 || slot >= _cooldowns.Length) return false;
            return _cooldowns[slot] <= 0f;
        }

        /// <summary>
        /// Gets remaining cooldown for a slot.
        /// </summary>
        public float GetCooldownRemaining(int slot)
        {
            if (slot < 0 || slot >= _cooldowns.Length) return 0f;
            return Mathf.Max(0f, _cooldowns[slot]);
        }

        /// <summary>
        /// Gets cooldown progress (0 = ready, 1 = just started).
        /// </summary>
        public float GetCooldownProgress(int slot)
        {
            if (slot < 0 || slot >= _abilitySlots.Length) return 0f;
            var ability = _abilitySlots[slot];
            if (ability == null || ability.Cooldown <= 0) return 0f;
            return Mathf.Clamp01(_cooldowns[slot] / ability.Cooldown);
        }

        /// <summary>
        /// Sets an ability in a slot.
        /// </summary>
        public void SetAbilitySlot(int slot, AbilityDefinitionSO ability)
        {
            if (slot < 0 || slot >= _abilitySlots.Length) return;
            _abilitySlots[slot] = ability;
        }

        #endregion

        #region Validation

        private bool ValidateAbilityLocal(int slot, out string error)
        {
            error = null;
            var ability = _abilitySlots[slot];

            // Cooldown check
            if (!IsAbilityReady(slot))
            {
                error = "Habilidad en enfriamiento";
                return false;
            }

            // Resource check
            if (_resourceSystem != null && ability.ResourceCost > 0)
            {
                if (_resourceSystem.GetResource(OwnerClientId) < ability.ResourceCost)
                {
                    error = $"No tienes suficiente {GetResourceName(ability.ResourceType)}";
                    return false;
                }
            }

            // Target check for offensive abilities
            if (ability.IsOffensive && !ability.IsSelfCast)
            {
                if (_targeting == null || !_targeting.HasValidTarget())
                {
                    error = "Necesitas un objetivo";
                    return false;
                }

                if (!_targeting.IsTargetInRange(ability.Range))
                {
                    error = "Objetivo fuera de rango";
                    return false;
                }

                if (!_targeting.HasLineOfSight())
                {
                    error = "Sin línea de visión";
                    return false;
                }
            }

            return true;
        }

        private bool ValidateAbilityServer(int slot, ITargetable target, out string error)
        {
            error = null;
            
            if (slot < 0 || slot >= _abilitySlots.Length)
            {
                error = "Slot inválido";
                return false;
            }

            var ability = _abilitySlots[slot];
            if (ability == null)
            {
                error = "No hay habilidad en ese slot";
                return false;
            }

            // Resource check
            if (_resourceSystem != null && ability.ResourceCost > 0)
            {
                if (_resourceSystem.GetResource(OwnerClientId) < ability.ResourceCost)
                {
                    error = $"No tienes suficiente {GetResourceName(ability.ResourceType)}";
                    return false;
                }
            }

            // Target validation for offensive abilities
            if (ability.IsOffensive && !ability.IsSelfCast)
            {
                if (target == null)
                {
                    error = "Objetivo inválido";
                    return false;
                }

                if (!target.IsAlive)
                {
                    error = "El objetivo está muerto";
                    return false;
                }

                float distance = Vector3.Distance(transform.position, target.Position);
                if (distance > ability.Range)
                {
                    error = "Objetivo fuera de rango";
                    return false;
                }
            }

            return true;
        }

        private string GetResourceName(SecondaryResourceType type)
        {
            return type switch
            {
                SecondaryResourceType.Mana => "Maná",
                SecondaryResourceType.Colera => "Cólera",
                SecondaryResourceType.Energia => "Energía",
                SecondaryResourceType.EnergiaRunica => "Energía Rúnica",
                SecondaryResourceType.Runas => "Runas",
                _ => "recurso"
            };
        }

        #endregion

        #region RPCs

        [ServerRpc]
        private void RequestCastServerRpc(int slot, NetworkObjectReference targetRef)
        {
            // Resolve target
            ITargetable target = null;
            if (targetRef.TryGet(out NetworkObject targetObj))
            {
                target = targetObj.GetComponent<ITargetable>();
            }

            // Server validation
            if (!ValidateAbilityServer(slot, target, out string error))
            {
                AbilityErrorClientRpc(error);
                return;
            }

            var ability = _abilitySlots[slot];

            // Consume resources
            if (_resourceSystem != null && ability.ResourceCost > 0)
            {
                _resourceSystem.TrySpendResource(OwnerClientId, ability.ResourceCost);
            }

            // Start cast or execute immediately
            if (ability.IsInstant)
            {
                ExecuteAbility(ability, target);
                StartCooldown(slot);
                
                if (ability.TriggersGCD)
                    StartGCDClientRpc();
            }
            else
            {
                // Start casting
                StartCastClientRpc(slot, targetRef);
            }

            // Broadcast visuals to all clients
            BroadcastVisualsClientRpc(slot, targetRef);
        }

        [ClientRpc]
        private void StartCastClientRpc(int slot, NetworkObjectReference targetRef)
        {
            if (!IsOwner) return;

            var ability = _abilitySlots[slot];
            if (ability == null) return;

            // Resolve target for event
            ITargetable target = null;
            if (targetRef.TryGet(out NetworkObject targetObj))
            {
                target = targetObj.GetComponent<ITargetable>();
            }

            // Start casting state
            if (_stateMachine != null)
            {
                _stateMachine.StartCasting(ability, ability.CastTime, ability.TriggersGCD, ability.RequiresStationary);
            }

            OnCastStarted?.Invoke(ability, target);
        }

        [ClientRpc]
        private void StartGCDClientRpc()
        {
            if (!IsOwner) return;
            _stateMachine?.StartGCD();
        }

        [ClientRpc]
        private void BroadcastVisualsClientRpc(int slot, NetworkObjectReference targetRef)
        {
            var ability = _abilitySlots[slot];
            if (ability == null) return;

            // Play cast sound
            if (ability.CastSound != null)
            {
                AudioSource.PlayClipAtPoint(ability.CastSound, transform.position);
            }

            // Spawn cast effect
            if (ability.CastEffectPrefab != null)
            {
                Instantiate(ability.CastEffectPrefab, transform.position, Quaternion.identity);
            }

            // TODO: Play animation
        }

        [ClientRpc]
        private void AbilityErrorClientRpc(string errorMessage)
        {
            if (!IsOwner) return;
            OnAbilityError?.Invoke(errorMessage);
        }

        [ClientRpc]
        private void AbilityExecutedClientRpc(int slot, NetworkObjectReference targetRef)
        {
            var ability = _abilitySlots[slot];
            if (ability == null) return;

            ITargetable target = null;
            if (targetRef.TryGet(out NetworkObject targetObj))
            {
                target = targetObj.GetComponent<ITargetable>();
            }

            OnAbilityExecuted?.Invoke(ability, target);
        }

        #endregion

        #region Ability Execution

        private void HandleCastCompleted(ScriptableObject abilityObj)
        {
            if (!IsServer) return;

            var ability = abilityObj as AbilityDefinitionSO;
            if (ability == null) return;

            // Find slot for this ability
            int slot = -1;
            for (int i = 0; i < _abilitySlots.Length; i++)
            {
                if (_abilitySlots[i] == ability)
                {
                    slot = i;
                    break;
                }
            }

            if (slot < 0) return;

            // Get current target
            ITargetable target = _targeting?.CurrentTarget;

            // Execute the ability
            ExecuteAbility(ability, target);
            StartCooldown(slot);

            // Start GCD if ability triggers it
            if (ability.TriggersGCD && _stateMachine != null)
                _stateMachine.StartGCD();

            // Notify clients
            NetworkObjectReference targetRef = default;
            if (target != null)
            {
                var targetNetObj = target.Transform?.GetComponent<NetworkObject>();
                if (targetNetObj != null)
                    targetRef = targetNetObj;
            }
            
            AbilityExecutedClientRpc(slot, targetRef);
        }

        private void ExecuteAbility(AbilityDefinitionSO ability, ITargetable target)
        {
            if (!IsServer) return;

            // Calculate damage
            if (ability.DealsDamage && target != null)
            {
                float damage = CalculateDamage(ability, target);
                
                if (target is IDamageable damageable)
                {
                    damageable.TakeDamage(damage, OwnerClientId);
                }
            }

            // Calculate healing
            if (ability.DoesHealing)
            {
                float healing = CalculateHealing(ability);
                
                if (ability.IsSelfCast && _healthSystem != null)
                {
                    _healthSystem.Heal(healing, OwnerClientId);
                }
                else if (target is HealthSystem targetHealth)
                {
                    targetHealth.Heal(healing, OwnerClientId);
                }
            }

            // Spawn projectile if needed
            if (ability.IsProjectile && target != null)
            {
                SpawnProjectile(ability, target);
            }
        }

        private float CalculateDamage(AbilityDefinitionSO ability, ITargetable target)
        {
            // Formula: (BaseDamage * StatMultiplier) - Mitigation
            float baseDamage = ability.BaseDamage * ability.StatMultiplier;
            
            // TODO: Get actual stats from character
            // TODO: Apply mitigation from target
            float mitigation = 0f;

            return Mathf.Max(0f, baseDamage - mitigation);
        }

        private float CalculateHealing(AbilityDefinitionSO ability)
        {
            return ability.BaseHealing * ability.StatMultiplier;
        }

        private void SpawnProjectile(AbilityDefinitionSO ability, ITargetable target)
        {
            // TODO: Implement projectile spawning
            // For now, damage is applied instantly
        }

        #endregion

        #region Cooldowns

        private void UpdateCooldowns(float deltaTime)
        {
            for (int i = 0; i < _cooldowns.Length; i++)
            {
                if (_cooldowns[i] > 0)
                {
                    _cooldowns[i] -= deltaTime;
                }
            }
        }

        private void StartCooldown(int slot)
        {
            if (slot < 0 || slot >= _abilitySlots.Length) return;
            
            var ability = _abilitySlots[slot];
            if (ability == null) return;

            _cooldowns[slot] = ability.Cooldown;
            
            // Notify on owner client
            if (IsOwner)
            {
                OnCooldownStarted?.Invoke(slot, ability.Cooldown);
            }
        }

        private void CheckQueuedAbility()
        {
            if (_spellQueue == null) return;
            if (_stateMachine == null) return;
            if (_stateMachine.CurrentState != CombatState.Idle) return;
            if (!_spellQueue.HasQueuedAbility()) return;

            var ability = _spellQueue.ConsumeQueuedAbility(out int slot);
            if (ability != null && slot >= 0)
            {
                TryUseAbility(slot);
            }
        }

        #endregion

        #region Testing Support

        public void SetConfig(CombatConfigSO config) => _config = config;
        public void SetTargeting(TargetingSystem targeting) => _targeting = targeting;
        public void SetStateMachine(CombatStateMachine stateMachine) => _stateMachine = stateMachine;
        public void SetSpellQueue(SpellQueue spellQueue) => _spellQueue = spellQueue;
        public void SetResourceSystem(SecondaryResourceSystem resourceSystem) => _resourceSystem = resourceSystem;
        
        public void ForceCooldown(int slot, float remaining)
        {
            if (slot >= 0 && slot < _cooldowns.Length)
                _cooldowns[slot] = remaining;
        }

        #endregion
    }
}
