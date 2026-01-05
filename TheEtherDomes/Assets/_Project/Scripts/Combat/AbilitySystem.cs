using System;
using EtherDomes.Data;
using EtherDomes.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Manages ability execution with GCD, cooldowns, cast times, channeled abilities, and mana costs.
    /// Uses Unity Input System for ability hotkeys.
    /// </summary>
    public class AbilitySystem : MonoBehaviour, IAbilitySystem
    {
        public const float DEFAULT_GCD = 1.5f;
        public const int DEFAULT_SLOT_COUNT = 9;
        public const float MOVEMENT_THRESHOLD = 0.1f;

        [SerializeField] private float _globalCooldownDuration = DEFAULT_GCD;
        [SerializeField] private int _slotCount = DEFAULT_SLOT_COUNT;

        private ITargetSystem _targetSystem;
        private IManaSystem _manaSystem;
        private IStealthSystem _stealthSystem;
        private ISecondaryResourceSystem _secondaryResourceSystem;
        private IDiminishingReturnsSystem _diminishingReturnsSystem;
        private ulong _localPlayerId;
        private AbilityState[] _abilities;
        private float _gcdRemaining;
        
        // Casting state
        private float _castTimeRemaining;
        private float _castTimeTotal;
        private AbilityData _currentCastAbility;
        private Vector3 _castStartPosition;
        
        // Channeling state (Requirements 2.2, 2.4, 2.5, 2.7)
        private AbilityData _currentChannelAbility;
        private float _channelTimeRemaining;
        private float _channelTimeTotal;
        private float _nextTickTime;
        private int _ticksCompleted;
        private Vector3 _channelStartPosition;
        
        // Input System
        private EtherDomesInput _inputActions;
        private bool _inputEnabled;

        public event Action OnGCDStarted;
        public event Action OnGCDEnded;
        public event Action<AbilityData> OnCastStarted;
        public event Action<AbilityData> OnCastCompleted;
        public event Action<AbilityData> OnCastInterrupted;
        public event Action<AbilityData> OnChannelStarted;
        public event Action<AbilityData, int> OnChannelTick;
        public event Action<AbilityData> OnChannelCompleted;
        public event Action<AbilityData> OnChannelInterrupted;
        public event Action<string> OnAbilityError;
        public event Action<AbilityData, ITargetable> OnAbilityExecuted;
        public event Action<AbilityData, ITargetable, int, float> OnComboPointAbilityExecuted; // ability, target, comboPointsConsumed, damageMultiplier
        public event Action<ulong, float, AbilityData> OnDrainLifeHealing; // casterId, healAmount, ability (Requirements 7.7)
        public event Action<ulong, ulong, Vector3> OnPullEffect; // casterId, targetId, casterPosition (Requirements 11.4)
        public event Action<ulong, Vector3, float> OnKnockbackSelf; // casterId, direction, distance (Requirements 11.5)
        public event Action<ulong, CCType, float> OnCCApplied; // targetId, ccType, effectiveDuration (Requirements 11.6, 11.7)

        public bool IsOnGCD => _gcdRemaining > 0;
        public float GCDRemaining => _gcdRemaining;
        public bool IsCasting => _currentCastAbility != null;
        public bool IsChanneling => _currentChannelAbility != null;
        public float CastProgress => _castTimeTotal > 0 ? 1f - (_castTimeRemaining / _castTimeTotal) : 0f;
        public float ChannelProgress => _channelTimeTotal > 0 ? _channelTimeRemaining / _channelTimeTotal : 0f;
        public int ChannelTicksCompleted => _ticksCompleted;
        public AbilityData CurrentCastAbility => _currentCastAbility;
        public AbilityData CurrentChannelAbility => _currentChannelAbility;
        public float GlobalCooldownDuration => _globalCooldownDuration;
        public int SlotCount => _slotCount;

        private void Awake()
        {
            _abilities = new AbilityState[_slotCount];
            Debug.Log($"[AbilitySystem] Awake - initialized {_slotCount} ability slots");
        }

        private void Start()
        {
            // Initialize input in Start to avoid conflicts with other systems
            _inputActions = new EtherDomesInput();
            SetupInputBindings();
            _inputActions.Player.Enable();
            _inputEnabled = true;
            Debug.Log("[AbilitySystem] Start - Input enabled for keys 1-9");
        }

        private void SetupInputBindings()
        {
            _inputActions.Player.Ability1.performed += OnAbility1;
            _inputActions.Player.Ability2.performed += OnAbility2;
            _inputActions.Player.Ability3.performed += OnAbility3;
            _inputActions.Player.Ability4.performed += OnAbility4;
            _inputActions.Player.Ability5.performed += ctx => TryExecuteAbility(4);
            _inputActions.Player.Ability6.performed += ctx => TryExecuteAbility(5);
            _inputActions.Player.Ability7.performed += ctx => TryExecuteAbility(6);
            _inputActions.Player.Ability8.performed += ctx => TryExecuteAbility(7);
            _inputActions.Player.Ability9.performed += ctx => TryExecuteAbility(8);
        }

        private void OnAbility1(InputAction.CallbackContext ctx) { Debug.Log("[AbilitySystem] Key 1 pressed"); TryExecuteAbility(0); }
        private void OnAbility2(InputAction.CallbackContext ctx) { Debug.Log("[AbilitySystem] Key 2 pressed"); TryExecuteAbility(1); }
        private void OnAbility3(InputAction.CallbackContext ctx) { Debug.Log("[AbilitySystem] Key 3 pressed"); TryExecuteAbility(2); }
        private void OnAbility4(InputAction.CallbackContext ctx) { Debug.Log("[AbilitySystem] Key 4 pressed"); TryExecuteAbility(3); }

        private void OnDisable()
        {
            if (_inputActions != null && _inputEnabled)
            {
                _inputActions.Player.Disable();
                _inputEnabled = false;
            }
        }

        private void OnEnable()
        {
            if (_inputActions != null && !_inputEnabled)
            {
                _inputActions.Player.Enable();
                _inputEnabled = true;
            }
        }

        private void OnDestroy()
        {
            if (_inputActions != null)
            {
                _inputActions.Player.Ability1.performed -= OnAbility1;
                _inputActions.Player.Ability2.performed -= OnAbility2;
                _inputActions.Player.Ability3.performed -= OnAbility3;
                _inputActions.Player.Ability4.performed -= OnAbility4;
                _inputActions.Dispose();
            }
        }

        public void Initialize(ITargetSystem targetSystem)
        {
            _targetSystem = targetSystem;
        }

        /// <summary>
        /// Initializes the ability system with target and mana systems.
        /// </summary>
        public void Initialize(ITargetSystem targetSystem, IManaSystem manaSystem, ulong localPlayerId)
        {
            _targetSystem = targetSystem;
            _manaSystem = manaSystem;
            _localPlayerId = localPlayerId;
        }

        /// <summary>
        /// Initializes the ability system with target, mana, and stealth systems.
        /// Requirements: 4.5
        /// </summary>
        public void Initialize(ITargetSystem targetSystem, IManaSystem manaSystem, IStealthSystem stealthSystem, ulong localPlayerId)
        {
            _targetSystem = targetSystem;
            _manaSystem = manaSystem;
            _stealthSystem = stealthSystem;
            _localPlayerId = localPlayerId;
        }

        /// <summary>
        /// Sets the stealth system for stealth ability verification.
        /// Requirements: 4.5
        /// </summary>
        public void SetStealthSystem(IStealthSystem stealthSystem)
        {
            _stealthSystem = stealthSystem;
        }

        /// <summary>
        /// Sets the mana system for mana cost verification.
        /// </summary>
        public void SetManaSystem(IManaSystem manaSystem, ulong playerId)
        {
            _manaSystem = manaSystem;
            _localPlayerId = playerId;
        }

        /// <summary>
        /// Sets the secondary resource system for combo points and other resources.
        /// Requirements: 5.4, 5.5
        /// </summary>
        public void SetSecondaryResourceSystem(ISecondaryResourceSystem secondaryResourceSystem)
        {
            _secondaryResourceSystem = secondaryResourceSystem;
        }

        /// <summary>
        /// Sets the diminishing returns system for CC effects.
        /// Requirements: 11.6, 11.7
        /// </summary>
        public void SetDiminishingReturnsSystem(IDiminishingReturnsSystem diminishingReturnsSystem)
        {
            _diminishingReturnsSystem = diminishingReturnsSystem;
        }

        private void Update()
        {
            UpdateGCD();
            UpdateCooldowns();
            UpdateCast();
            UpdateChannel();
        }

        private void UpdateGCD()
        {
            if (_gcdRemaining > 0)
            {
                _gcdRemaining -= Time.deltaTime;
                if (_gcdRemaining <= 0)
                {
                    _gcdRemaining = 0;
                    OnGCDEnded?.Invoke();
                }
            }
        }

        private void UpdateCooldowns()
        {
            if (_abilities == null) return;
            
            for (int i = 0; i < _abilities.Length; i++)
            {
                _abilities[i]?.UpdateCooldown(Time.deltaTime);
            }
        }

        private void UpdateCast()
        {
            if (_currentCastAbility == null)
                return;

            // Check for movement interruption
            if (HasMovedFrom(_castStartPosition))
            {
                InterruptCast();
                return;
            }

            _castTimeRemaining -= Time.deltaTime;
            if (_castTimeRemaining <= 0)
            {
                CompleteCast();
            }
        }

        /// <summary>
        /// Updates channeling state, processes ticks, and checks for interruption.
        /// Requirements: 2.2, 2.4, 2.5, 2.7
        /// </summary>
        private void UpdateChannel()
        {
            if (_currentChannelAbility == null)
                return;

            // Check for movement interruption (threshold 0.1m)
            if (HasMovedFrom(_channelStartPosition))
            {
                InterruptChannel();
                return;
            }

            // Process ticks
            float elapsed = _channelTimeTotal - _channelTimeRemaining;
            while (_nextTickTime <= elapsed && _ticksCompleted < _currentChannelAbility.TotalTicks)
            {
                ProcessChannelTick();
            }

            _channelTimeRemaining -= Time.deltaTime;
            
            // Check if channel completed
            if (_channelTimeRemaining <= 0)
            {
                CompleteChannel();
            }
        }


        public bool TryExecuteAbility(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slotCount)
            {
                OnAbilityError?.Invoke("Invalid ability slot");
                return false;
            }

            var abilityState = _abilities[slotIndex];
            if (abilityState == null || abilityState.Data == null)
            {
                OnAbilityError?.Invoke("No ability in slot");
                return false;
            }

            var ability = abilityState.Data;

            // Check GCD
            if (ability.AffectedByGCD && IsOnGCD)
            {
                OnAbilityError?.Invoke("On cooldown");
                return false;
            }

            // Check ability cooldown
            if (abilityState.IsOnCooldown)
            {
                OnAbilityError?.Invoke($"Ability on cooldown ({abilityState.CooldownRemaining:F1}s)");
                return false;
            }

            // Check if already casting or channeling
            if (IsCasting)
            {
                OnAbilityError?.Invoke("Already casting");
                return false;
            }

            if (IsChanneling)
            {
                OnAbilityError?.Invoke("Already channeling");
                return false;
            }

            // Check target requirement
            if (ability.RequiresTarget && !_targetSystem.HasTarget)
            {
                OnAbilityError?.Invoke("No target");
                return false;
            }

            // Check range
            if (ability.RequiresTarget && _targetSystem.TargetDistance > ability.Range)
            {
                OnAbilityError?.Invoke("Out of range");
                return false;
            }

            // Check minimum range (Hunter dead zone) - Requirements 6.8
            if (ability.RequiresTarget && ability.MinRange > 0 && _targetSystem.TargetDistance < ability.MinRange)
            {
                OnAbilityError?.Invoke("Too close");
                return false;
            }

            // Check mana cost (Requirement 12.2, 12.3)
            // For Energy/Focus, use SecondaryResourceSystem instead of ManaSystem
            if (ability.ManaCost > 0)
            {
                if (ability.ResourceType == SecondaryResourceType.Energy || 
                    ability.ResourceType == SecondaryResourceType.Focus)
                {
                    // Use SecondaryResourceSystem for Energy/Focus
                    if (_secondaryResourceSystem != null)
                    {
                        float currentResource = _secondaryResourceSystem.GetResource(_localPlayerId);
                        if (currentResource < ability.ManaCost)
                        {
                            string resourceName = ability.ResourceType == SecondaryResourceType.Energy ? "energy" : "focus";
                            OnAbilityError?.Invoke($"Not enough {resourceName}");
                            return false;
                        }
                    }
                }
                else if (_manaSystem != null)
                {
                    // Use ManaSystem for Mana
                    float currentMana = _manaSystem.GetCurrentMana(_localPlayerId);
                    if (currentMana < ability.ManaCost)
                    {
                        OnAbilityError?.Invoke("Not enough mana");
                        return false;
                    }
                }
            }

            // Check stealth requirement (Requirement 4.5)
            if (ability.RequiresStealth && _stealthSystem != null)
            {
                if (!_stealthSystem.IsInStealth(_localPlayerId))
                {
                    OnAbilityError?.Invoke("Requires stealth");
                    return false;
                }
            }

            // Check combo points requirement for finishers (Requirements 5.5)
            if (ability.ConsumesComboPoints && _secondaryResourceSystem != null)
            {
                if (!_secondaryResourceSystem.HasComboPoints(_localPlayerId))
                {
                    OnAbilityError?.Invoke("Requires combo points");
                    return false;
                }
            }

            // Start GCD if affected
            if (ability.AffectedByGCD)
            {
                StartGCD();
            }

            // Start cooldown
            abilityState.StartCooldown();

            // Determine ability type: channeled, cast time, or instant
            if (ability.IsChanneled)
            {
                StartChannel(ability);
            }
            else if (!ability.IsInstant)
            {
                StartCast(ability);
            }
            else
            {
                ExecuteAbility(ability);
            }

            return true;
        }

        private void StartGCD()
        {
            _gcdRemaining = _globalCooldownDuration;
            OnGCDStarted?.Invoke();
        }

        private void StartCast(AbilityData ability)
        {
            _currentCastAbility = ability;
            _castTimeTotal = ability.CastTime;
            _castTimeRemaining = ability.CastTime;
            _castStartPosition = transform.position;

            Debug.Log($"[AbilitySystem] Casting: {ability.AbilityName} ({ability.CastTime}s)");
            OnCastStarted?.Invoke(ability);
        }

        private void CompleteCast()
        {
            var ability = _currentCastAbility;
            _currentCastAbility = null;
            _castTimeRemaining = 0;
            _castTimeTotal = 0;

            ExecuteAbility(ability);
            OnCastCompleted?.Invoke(ability);
        }

        public void InterruptCast()
        {
            if (_currentCastAbility == null)
                return;

            var ability = _currentCastAbility;
            _currentCastAbility = null;
            _castTimeRemaining = 0;
            _castTimeTotal = 0;

            Debug.Log($"[AbilitySystem] Cast interrupted: {ability.AbilityName}");
            OnCastInterrupted?.Invoke(ability);
        }

        private void ExecuteAbility(AbilityData ability)
        {
            var target = _targetSystem?.CurrentTarget;
            
            // Deduct mana/energy/focus cost (Requirement 12.2)
            if (ability.ManaCost > 0)
            {
                if (ability.ResourceType == SecondaryResourceType.Energy || 
                    ability.ResourceType == SecondaryResourceType.Focus)
                {
                    // Use SecondaryResourceSystem for Energy/Focus
                    _secondaryResourceSystem?.TrySpendResource(_localPlayerId, ability.ManaCost);
                }
                else if (_manaSystem != null)
                {
                    // Use ManaSystem for Mana
                    _manaSystem.TrySpendMana(_localPlayerId, ability.ManaCost);
                }
            }
            
            // Break stealth if ability breaks stealth (Requirement 4.5)
            if (ability.BreaksStealth && _stealthSystem != null && _stealthSystem.IsInStealth(_localPlayerId))
            {
                _stealthSystem.BreakStealth(_localPlayerId, StealthBreakReason.AbilityUsed);
            }

            // Handle combo points (Requirements 5.4, 5.5)
            int comboPointsConsumed = 0;
            float damageMultiplier = 1f;
            
            if (_secondaryResourceSystem != null)
            {
                // Generate combo points if ability is a generator
                if (ability.GeneratesComboPoint)
                {
                    int pointsToGenerate = ability.ComboPointsGenerated > 0 ? ability.ComboPointsGenerated : 1;
                    _secondaryResourceSystem.AddComboPoint(_localPlayerId, pointsToGenerate);
                    Debug.Log($"[AbilitySystem] Generated {pointsToGenerate} combo point(s) from {ability.AbilityName}");
                }
                
                // Consume combo points if ability is a finisher
                if (ability.ConsumesComboPoints)
                {
                    comboPointsConsumed = _secondaryResourceSystem.ConsumeAllComboPoints(_localPlayerId);
                    damageMultiplier = _secondaryResourceSystem.CalculateComboPointDamageMultiplier(comboPointsConsumed);
                    Debug.Log($"[AbilitySystem] Consumed {comboPointsConsumed} combo points, damage multiplier: {damageMultiplier:F2}x");
                }
            }
            
            Debug.Log($"[AbilitySystem] Executed: {ability.AbilityName} on {target?.DisplayName ?? "no target"}");
            OnAbilityExecuted?.Invoke(ability, target);
            
            // Fire combo point event if combo points were consumed
            if (comboPointsConsumed > 0)
            {
                OnComboPointAbilityExecuted?.Invoke(ability, target, comboPointsConsumed, damageMultiplier);
            }

            // Handle pull effect (Death Grip) - Requirements 11.4
            if (ability.IsPullEffect && target != null)
            {
                ulong targetId = 0;
                // Try to get target ID from ITargetable
                if (target is UnityEngine.Component component)
                {
                    var networkIdentity = component.GetComponent<Unity.Netcode.NetworkObject>();
                    if (networkIdentity != null)
                    {
                        targetId = networkIdentity.NetworkObjectId;
                    }
                }
                
                Debug.Log($"[AbilitySystem] Death Grip: Pulling {target.DisplayName} to caster position");
                OnPullEffect?.Invoke(_localPlayerId, targetId, transform.position);
            }

            // Handle knockback self effect (Disengage) - Requirements 11.5
            if (ability.IsKnockbackSelf)
            {
                Vector3 knockbackDirection;
                if (target != null && target is UnityEngine.Component targetComponent)
                {
                    // Move away from target
                    knockbackDirection = (transform.position - targetComponent.transform.position).normalized;
                }
                else
                {
                    // Move backward if no target
                    knockbackDirection = -transform.forward;
                }
                
                Debug.Log($"[AbilitySystem] Disengage: Moving caster {ability.KnockbackDistance}m away");
                OnKnockbackSelf?.Invoke(_localPlayerId, knockbackDirection, ability.KnockbackDistance);
            }

            // Handle CC effects with Diminishing Returns - Requirements 11.6, 11.7
            if (ability.CCType != CCType.None && target != null)
            {
                ulong targetId = 0;
                if (target is UnityEngine.Component ccTargetComponent)
                {
                    var networkIdentity = ccTargetComponent.GetComponent<Unity.Netcode.NetworkObject>();
                    if (networkIdentity != null)
                    {
                        targetId = networkIdentity.NetworkObjectId;
                    }
                }

                if (targetId != 0)
                {
                    // Apply diminishing returns to CC duration
                    float baseDuration = GetCCBaseDuration(ability);
                    float effectiveDuration = baseDuration;
                    
                    if (_diminishingReturnsSystem != null)
                    {
                        effectiveDuration = _diminishingReturnsSystem.ApplyDiminishingReturns(targetId, ability.CCType, baseDuration);
                    }

                    if (effectiveDuration > 0)
                    {
                        Debug.Log($"[AbilitySystem] CC Applied: {ability.CCType} to {target.DisplayName} for {effectiveDuration}s (base: {baseDuration}s)");
                        OnCCApplied?.Invoke(targetId, ability.CCType, effectiveDuration);
                    }
                    else
                    {
                        Debug.Log($"[AbilitySystem] CC Immune: {target.DisplayName} is immune to {ability.CCType}");
                    }
                }
            }
        }

        /// <summary>
        /// Gets the base duration for a CC effect based on ability type.
        /// </summary>
        private float GetCCBaseDuration(AbilityData ability)
        {
            // Default durations based on CC type
            return ability.CCType switch
            {
                CCType.Stun => 4f,   // Cheap Shot, Shadowfury, Freezing Trap
                CCType.Fear => 8f,   // Fear (up to 20s but breaks on damage)
                CCType.Slow => 6f,   // Concussive Shot, Chains of Ice
                CCType.Root => 8f,   // Generic root duration
                _ => 0f
            };
        }

        /// <summary>
        /// Checks if the player has moved more than the threshold from a starting position.
        /// </summary>
        private bool HasMovedFrom(Vector3 startPosition)
        {
            return Vector3.Distance(transform.position, startPosition) > MOVEMENT_THRESHOLD;
        }

        #region Channeling Methods

        /// <summary>
        /// Starts channeling an ability.
        /// Requirements: 2.2
        /// </summary>
        private void StartChannel(AbilityData ability)
        {
            _currentChannelAbility = ability;
            _channelTimeTotal = ability.ChannelDuration;
            _channelTimeRemaining = ability.ChannelDuration;
            _channelStartPosition = transform.position;
            _ticksCompleted = 0;
            _nextTickTime = 0f; // First tick happens immediately

            // Deduct mana cost at start of channel
            if (_manaSystem != null && ability.ManaCost > 0)
            {
                _manaSystem.TrySpendMana(_localPlayerId, ability.ManaCost);
            }

            Debug.Log($"[AbilitySystem] Channeling: {ability.AbilityName} ({ability.ChannelDuration}s, {ability.TotalTicks} ticks)");
            OnChannelStarted?.Invoke(ability);
            
            // Process first tick immediately
            ProcessChannelTick();
        }

        /// <summary>
        /// Processes a single channel tick, applying effects.
        /// Requirements: 2.2, 7.7 (Drain Life healing)
        /// </summary>
        private void ProcessChannelTick()
        {
            if (_currentChannelAbility == null)
                return;

            _ticksCompleted++;
            _nextTickTime += _currentChannelAbility.TickInterval;

            var target = _targetSystem?.CurrentTarget;
            Debug.Log($"[AbilitySystem] Channel tick {_ticksCompleted}/{_currentChannelAbility.TotalTicks}: {_currentChannelAbility.AbilityName}");
            
            // Handle Drain Life healing (Requirements 7.7)
            if (_currentChannelAbility.HealsOnDamage && _currentChannelAbility.HealOnDamagePercent > 0)
            {
                float damageDealt = _currentChannelAbility.BaseDamage;
                float healAmount = damageDealt * _currentChannelAbility.HealOnDamagePercent;
                Debug.Log($"[AbilitySystem] Drain Life healing: {healAmount} (50% of {damageDealt} damage)");
                OnDrainLifeHealing?.Invoke(_localPlayerId, healAmount, _currentChannelAbility);
            }
            
            OnChannelTick?.Invoke(_currentChannelAbility, _ticksCompleted);
            OnAbilityExecuted?.Invoke(_currentChannelAbility, target);
        }

        /// <summary>
        /// Completes the channel after all ticks.
        /// Requirements: 2.7
        /// </summary>
        private void CompleteChannel()
        {
            var ability = _currentChannelAbility;
            _currentChannelAbility = null;
            _channelTimeRemaining = 0;
            _channelTimeTotal = 0;
            _ticksCompleted = 0;

            Debug.Log($"[AbilitySystem] Channel completed: {ability.AbilityName}");
            OnChannelCompleted?.Invoke(ability);
        }

        /// <summary>
        /// Interrupts the current channel.
        /// Requirements: 2.4, 2.5
        /// </summary>
        public void InterruptChannel()
        {
            if (_currentChannelAbility == null)
                return;

            var ability = _currentChannelAbility;
            int ticksBeforeInterrupt = _ticksCompleted;
            
            _currentChannelAbility = null;
            _channelTimeRemaining = 0;
            _channelTimeTotal = 0;
            _ticksCompleted = 0;

            Debug.Log($"[AbilitySystem] Channel interrupted: {ability.AbilityName} (completed {ticksBeforeInterrupt} ticks)");
            OnChannelInterrupted?.Invoke(ability);
        }

        #endregion


        public AbilityState[] GetAbilities()
        {
            return _abilities;
        }

        public AbilityState GetAbility(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slotCount)
                return null;
            return _abilities[slotIndex];
        }

        public void SetAbility(int slotIndex, AbilityData ability)
        {
            if (slotIndex < 0 || slotIndex >= _slotCount)
                return;

            _abilities[slotIndex] = ability != null ? new AbilityState(ability) : null;
        }

        public float GetCooldownRemaining(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slotCount)
                return 0;

            return _abilities[slotIndex]?.CooldownRemaining ?? 0;
        }

        /// <summary>
        /// Load abilities from an array (e.g., from class system).
        /// </summary>
        public void LoadAbilities(AbilityData[] abilities)
        {
            for (int i = 0; i < _slotCount; i++)
            {
                if (i < abilities.Length && abilities[i] != null)
                {
                    _abilities[i] = new AbilityState(abilities[i]);
                }
                else
                {
                    _abilities[i] = null;
                }
            }
        }

        /// <summary>
        /// Reset all cooldowns (for testing/debug).
        /// </summary>
        public void ResetAllCooldowns()
        {
            _gcdRemaining = 0;
            for (int i = 0; i < _abilities.Length; i++)
            {
                if (_abilities[i] != null)
                    _abilities[i].CooldownRemaining = 0;
            }
        }
    }
}
