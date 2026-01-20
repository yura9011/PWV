using System;
using UnityEngine;
using EtherDomes.Data;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Implements input buffering (spell queue) for responsive combat.
    /// Allows queuing abilities during Casting or GCD states within a buffer window.
    /// </summary>
    public class SpellQueue : MonoBehaviour
    {
        #region Configuration

        [Header("Configuration")]
        [SerializeField] private CombatConfigSO _config;
        
        [SerializeField] private CombatStateMachine _stateMachine;

        #endregion

        #region State

        private ScriptableObject _queuedAbility;
        private int _queuedAbilitySlot = -1;
        private float _queueTime;

        /// <summary>Currently queued ability (null if none).</summary>
        public ScriptableObject QueuedAbility => _queuedAbility;

        /// <summary>Slot index of queued ability (-1 if none).</summary>
        public int QueuedAbilitySlot => _queuedAbilitySlot;

        /// <summary>Buffer window duration from config.</summary>
        public float BufferWindow => _config != null ? _config.SpellQueueWindow : 0.4f;

        #endregion

        #region Events

        /// <summary>Fired when an ability is queued.</summary>
        public event Action<ScriptableObject, int> OnAbilityQueued;

        /// <summary>Fired when queued ability is consumed.</summary>
        public event Action<ScriptableObject, int> OnQueueConsumed;

        /// <summary>Fired when queue is cleared.</summary>
        public event Action OnQueueCleared;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_config == null)
                _config = CombatConfigSO.Instance;
        }

        private void OnEnable()
        {
            if (_stateMachine != null)
            {
                _stateMachine.OnStateChanged += HandleStateChanged;
            }
        }

        private void OnDisable()
        {
            if (_stateMachine != null)
            {
                _stateMachine.OnStateChanged -= HandleStateChanged;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Attempts to queue an ability for execution when current state ends.
        /// Only succeeds if within buffer window of state ending.
        /// </summary>
        /// <param name="ability">Ability to queue</param>
        /// <param name="slot">Slot index of the ability</param>
        /// <returns>True if ability was queued</returns>
        public bool TryQueueAbility(ScriptableObject ability, int slot)
        {
            if (ability == null) return false;
            if (_stateMachine == null) return false;

            // Can only queue during Casting or GCD
            var state = _stateMachine.CurrentState;
            if (state != CombatState.Casting && state != CombatState.GlobalCooldown)
                return false;

            // Check if within buffer window
            if (!IsWithinBufferWindow())
                return false;

            // Queue the ability (replaces any existing queued ability)
            _queuedAbility = ability;
            _queuedAbilitySlot = slot;
            _queueTime = Time.time;
            
            OnAbilityQueued?.Invoke(ability, slot);
            return true;
        }

        /// <summary>
        /// Consumes and returns the queued ability, clearing the queue.
        /// </summary>
        /// <returns>The queued ability, or null if none</returns>
        public ScriptableObject ConsumeQueuedAbility()
        {
            if (_queuedAbility == null) return null;

            var ability = _queuedAbility;
            var slot = _queuedAbilitySlot;
            
            _queuedAbility = null;
            _queuedAbilitySlot = -1;
            
            OnQueueConsumed?.Invoke(ability, slot);
            return ability;
        }

        /// <summary>
        /// Consumes and returns the queued ability with its slot.
        /// </summary>
        /// <param name="slot">Output slot index</param>
        /// <returns>The queued ability, or null if none</returns>
        public ScriptableObject ConsumeQueuedAbility(out int slot)
        {
            slot = _queuedAbilitySlot;
            return ConsumeQueuedAbility();
        }

        /// <summary>
        /// Clears any queued ability.
        /// </summary>
        public void ClearQueue()
        {
            if (_queuedAbility == null) return;

            _queuedAbility = null;
            _queuedAbilitySlot = -1;
            OnQueueCleared?.Invoke();
        }

        /// <summary>
        /// Returns true if there is a queued ability.
        /// </summary>
        public bool HasQueuedAbility()
        {
            return _queuedAbility != null;
        }

        /// <summary>
        /// Returns true if currently within the buffer window.
        /// </summary>
        public bool IsWithinBufferWindow()
        {
            if (_stateMachine == null) return false;

            var state = _stateMachine.CurrentState;
            if (state != CombatState.Casting && state != CombatState.GlobalCooldown)
                return false;

            return _stateMachine.StateTimeRemaining <= BufferWindow;
        }

        #endregion

        #region Internal Methods

        private void HandleStateChanged(CombatState previousState, CombatState newState)
        {
            // When transitioning to Idle, check if we have a queued ability
            if (newState == CombatState.Idle && HasQueuedAbility())
            {
                // The AbilityExecutor should consume this
                // We just notify that it's ready
            }
            
            // Clear queue if entering Locked state
            if (newState == CombatState.Locked)
            {
                ClearQueue();
            }
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
        /// Sets the state machine reference for testing purposes.
        /// </summary>
        public void SetStateMachine(CombatStateMachine stateMachine)
        {
            // Unsubscribe from old
            if (_stateMachine != null)
                _stateMachine.OnStateChanged -= HandleStateChanged;
            
            _stateMachine = stateMachine;
            
            // Subscribe to new
            if (_stateMachine != null)
                _stateMachine.OnStateChanged += HandleStateChanged;
        }

        /// <summary>
        /// Forces a queued ability for testing purposes.
        /// </summary>
        public void ForceQueue(ScriptableObject ability, int slot)
        {
            _queuedAbility = ability;
            _queuedAbilitySlot = slot;
            _queueTime = Time.time;
        }

        #endregion
    }
}
