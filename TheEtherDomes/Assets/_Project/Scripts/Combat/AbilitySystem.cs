using System;
using EtherDomes.Data;
using EtherDomes.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Manages ability execution with GCD, cooldowns, and cast times.
    /// Uses Unity Input System for ability hotkeys.
    /// </summary>
    public class AbilitySystem : MonoBehaviour, IAbilitySystem
    {
        public const float DEFAULT_GCD = 1.5f;
        public const int DEFAULT_SLOT_COUNT = 9;

        [SerializeField] private float _globalCooldownDuration = DEFAULT_GCD;
        [SerializeField] private int _slotCount = DEFAULT_SLOT_COUNT;

        private ITargetSystem _targetSystem;
        private AbilityState[] _abilities;
        private float _gcdRemaining;
        private float _castTimeRemaining;
        private float _castTimeTotal;
        private AbilityData _currentCastAbility;
        private Vector3 _castStartPosition;
        
        // Input System
        private EtherDomesInput _inputActions;
        private bool _inputEnabled;

        public event Action OnGCDStarted;
        public event Action OnGCDEnded;
        public event Action<AbilityData> OnCastStarted;
        public event Action<AbilityData> OnCastCompleted;
        public event Action<AbilityData> OnCastInterrupted;
        public event Action<string> OnAbilityError;
        public event Action<AbilityData, ITargetable> OnAbilityExecuted;

        public bool IsOnGCD => _gcdRemaining > 0;
        public float GCDRemaining => _gcdRemaining;
        public bool IsCasting => _currentCastAbility != null;
        public float CastProgress => _castTimeTotal > 0 ? 1f - (_castTimeRemaining / _castTimeTotal) : 0f;
        public AbilityData CurrentCastAbility => _currentCastAbility;
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

        private void Update()
        {
            UpdateGCD();
            UpdateCooldowns();
            UpdateCast();
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
            if (HasMoved())
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

            // Check if already casting
            if (IsCasting)
            {
                OnAbilityError?.Invoke("Already casting");
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

            // Start GCD if affected
            if (ability.AffectedByGCD)
            {
                StartGCD();
            }

            // Start cooldown
            abilityState.StartCooldown();

            // Instant or cast time?
            if (ability.IsInstant)
            {
                ExecuteAbility(ability);
            }
            else
            {
                StartCast(ability);
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
            
            Debug.Log($"[AbilitySystem] Executed: {ability.AbilityName} on {target?.DisplayName ?? "no target"}");
            OnAbilityExecuted?.Invoke(ability, target);
        }

        private bool HasMoved()
        {
            const float movementThreshold = 0.1f;
            return Vector3.Distance(transform.position, _castStartPosition) > movementThreshold;
        }


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
