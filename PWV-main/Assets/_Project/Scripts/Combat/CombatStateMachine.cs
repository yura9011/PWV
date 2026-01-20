using System;
using UnityEngine;
using Unity.Netcode;
using EtherDomes.Data;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Manages combat state transitions for ability casting.
    /// Handles Idle, Casting, GlobalCooldown, and Locked states.
    /// </summary>
    public class CombatStateMachine : NetworkBehaviour
    {
        #region Configuration

        [Header("Configuration")]
        [SerializeField] private CombatConfigSO _config;

        #endregion

        #region State

        private CombatState _currentState = CombatState.Idle;
        private float _stateTimeRemaining;
        private float _castDuration;
        private ScriptableObject _currentCastingAbility;
        private float _lockDuration;

        /// <summary>Current combat state.</summary>
        public CombatState CurrentState => _currentState;

        /// <summary>Time remaining in current state (for Casting, GCD, Locked).</summary>
        public float StateTimeRemaining => _stateTimeRemaining;

        /// <summary>Total cast duration for current cast.</summary>
        public float CastDuration => _castDuration;

        /// <summary>Ability currently being cast (null if not casting).</summary>
        public ScriptableObject CurrentCastingAbility => _currentCastingAbility;

        /// <summary>Progress of current cast (0-1).</summary>
        public float CastProgress => _castDuration > 0 ? 1f - (_stateTimeRemaining / _castDuration) : 1f;

        /// <summary>Global Cooldown duration from config.</summary>
        public float GlobalCooldownDuration => _config != null ? _config.GlobalCooldownDuration : 1.5f;

        #endregion

        #region Events

        /// <summary>Fired when combat state changes.</summary>
        public event Action<CombatState, CombatState> OnStateChanged;

        /// <summary>Fired every frame during casting with (current progress, total duration).</summary>
        public event Action<float, float> OnCastProgress;

        /// <summary>Fired when a cast is interrupted (movement, stun, etc).</summary>
        public event Action<ScriptableObject> OnCastInterrupted;

        /// <summary>Fired when a cast completes successfully.</summary>
        public event Action<ScriptableObject> OnCastCompleted;

        /// <summary>Fired when GCD starts.</summary>
        public event Action<float> OnGCDStarted;

        /// <summary>Fired when locked state starts.</summary>
        public event Action<float> OnLocked;

        /// <summary>Fired when unlocked from locked state.</summary>
        public event Action OnUnlocked;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_config == null)
                _config = CombatConfigSO.Instance;
        }

        private void Update()
        {
            if (!IsOwner && !IsServer) return;
            UpdateState(Time.deltaTime);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Checks if an ability can be used in the current state.
        /// </summary>
        /// <param name="isOffGCD">Whether the ability is off-GCD</param>
        public bool CanUseAbility(bool isOffGCD = false)
        {
            switch (_currentState)
            {
                case CombatState.Idle:
                    return true;

                case CombatState.Casting:
                    return false; // Cannot use abilities while casting

                case CombatState.GlobalCooldown:
                    // Off-GCD abilities can be used during GCD
                    return isOffGCD;

                case CombatState.Locked:
                    return false; // Cannot use any abilities while locked

                default:
                    return false;
            }
        }

        /// <summary>
        /// Starts casting an ability.
        /// </summary>
        /// <param name="ability">The ability ScriptableObject</param>
        /// <param name="castTime">Cast time in seconds (0 = instant)</param>
        /// <param name="triggersGCD">Whether this ability triggers GCD</param>
        /// <param name="requiresStationary">Whether movement interrupts this cast</param>
        public void StartCasting(ScriptableObject ability, float castTime, bool triggersGCD, bool requiresStationary)
        {
            if (ability == null) return;

            if (castTime <= 0)
            {
                // Instant cast - complete immediately
                OnCastCompleted?.Invoke(ability);
                
                // Start GCD if ability triggers it
                if (triggersGCD)
                    StartGCD();
            }
            else
            {
                // Start casting
                _currentCastingAbility = ability;
                _castDuration = castTime;
                _stateTimeRemaining = castTime;
                TransitionTo(CombatState.Casting);
            }
        }

        /// <summary>
        /// Interrupts the current cast (e.g., due to movement or CC).
        /// </summary>
        public void InterruptCast()
        {
            if (_currentState != CombatState.Casting) return;

            var interruptedAbility = _currentCastingAbility;
            _currentCastingAbility = null;
            _castDuration = 0f;
            _stateTimeRemaining = 0f;
            
            TransitionTo(CombatState.Idle);
            OnCastInterrupted?.Invoke(interruptedAbility);
        }

        /// <summary>
        /// Starts the Global Cooldown period.
        /// </summary>
        public void StartGCD()
        {
            if (_currentState == CombatState.Locked) return;
            
            _stateTimeRemaining = GlobalCooldownDuration;
            TransitionTo(CombatState.GlobalCooldown);
            OnGCDStarted?.Invoke(GlobalCooldownDuration);
        }

        /// <summary>
        /// Locks the player (stun, silence, etc) for a duration.
        /// </summary>
        public void Lock(float duration)
        {
            if (duration <= 0) return;

            // If casting, interrupt first
            if (_currentState == CombatState.Casting)
            {
                var interruptedAbility = _currentCastingAbility;
                _currentCastingAbility = null;
                OnCastInterrupted?.Invoke(interruptedAbility);
            }

            _lockDuration = duration;
            _stateTimeRemaining = duration;
            TransitionTo(CombatState.Locked);
            OnLocked?.Invoke(duration);
        }

        /// <summary>
        /// Immediately unlocks from locked state.
        /// </summary>
        public void Unlock()
        {
            if (_currentState != CombatState.Locked) return;

            _stateTimeRemaining = 0f;
            TransitionTo(CombatState.Idle);
            OnUnlocked?.Invoke();
        }

        /// <summary>
        /// Called when player movement is detected.
        /// Interrupts stationary casts.
        /// </summary>
        public void OnMovementDetected()
        {
            if (_currentState != CombatState.Casting) return;
            // Note: The caller should check if the ability requires stationary
            InterruptCast();
        }

        /// <summary>
        /// Resets the state machine to Idle.
        /// </summary>
        public void Reset()
        {
            _currentCastingAbility = null;
            _castDuration = 0f;
            _stateTimeRemaining = 0f;
            _lockDuration = 0f;
            TransitionTo(CombatState.Idle);
        }

        #endregion

        #region Internal Methods

        private void UpdateState(float deltaTime)
        {
            switch (_currentState)
            {
                case CombatState.Idle:
                    // Nothing to update
                    break;

                case CombatState.Casting:
                    UpdateCasting(deltaTime);
                    break;

                case CombatState.GlobalCooldown:
                    UpdateGCD(deltaTime);
                    break;

                case CombatState.Locked:
                    UpdateLocked(deltaTime);
                    break;
            }
        }

        private void UpdateCasting(float deltaTime)
        {
            _stateTimeRemaining -= deltaTime;
            
            // Fire progress event
            float progress = _castDuration > 0 ? 1f - (_stateTimeRemaining / _castDuration) : 1f;
            OnCastProgress?.Invoke(progress, _castDuration);

            if (_stateTimeRemaining <= 0f)
            {
                // Cast completed
                var completedAbility = _currentCastingAbility;
                _currentCastingAbility = null;
                _castDuration = 0f;
                
                OnCastCompleted?.Invoke(completedAbility);
                TransitionTo(CombatState.Idle);
            }
        }

        private void UpdateGCD(float deltaTime)
        {
            _stateTimeRemaining -= deltaTime;

            if (_stateTimeRemaining <= 0f)
            {
                _stateTimeRemaining = 0f;
                TransitionTo(CombatState.Idle);
            }
        }

        private void UpdateLocked(float deltaTime)
        {
            _stateTimeRemaining -= deltaTime;

            if (_stateTimeRemaining <= 0f)
            {
                _stateTimeRemaining = 0f;
                TransitionTo(CombatState.Idle);
                OnUnlocked?.Invoke();
            }
        }

        private void TransitionTo(CombatState newState)
        {
            if (_currentState == newState) return;

            var previousState = _currentState;
            _currentState = newState;
            OnStateChanged?.Invoke(previousState, newState);
        }

        #endregion

        #region Testing Support

        /// <summary>
        /// Sets the config for testing purposes.
        /// </summary>
        public void SetConfig(CombatConfigSO config)
        {
            _config = config;
        }

        /// <summary>
        /// Forces a state transition for testing purposes.
        /// </summary>
        public void ForceState(CombatState state, float timeRemaining = 0f)
        {
            _stateTimeRemaining = timeRemaining;
            TransitionTo(state);
        }

        #endregion
    }
}
