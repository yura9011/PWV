using System;
using System.Collections.Generic;
using System.Linq;
using EtherDomes.Data;
using EtherDomes.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Tab-Target selection system for combat.
    /// Handles target cycling, selection, and range tracking.
    /// Uses Unity Input System for input handling.
    /// </summary>
    public class TargetSystem : MonoBehaviour, ITargetSystem
    {
        public const float DEFAULT_MAX_RANGE = 40f;

        [SerializeField] private float _maxTargetRange = DEFAULT_MAX_RANGE;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private LayerMask _targetableLayers;

        private readonly List<ITargetable> _registeredTargets = new List<ITargetable>();
        private ITargetable _currentTarget;
        private int _cycleIndex = -1;
        
        // Input System
        private EtherDomesInput _inputActions;

        public event Action<ITargetable> OnTargetChanged;
        public event Action OnTargetLost;

        public ITargetable CurrentTarget => _currentTarget;
        public bool HasTarget => _currentTarget != null;
        public float MaxTargetRange => _maxTargetRange;

        public float TargetDistance
        {
            get
            {
                if (_currentTarget == null || _playerTransform == null)
                    return float.MaxValue;
                return Vector3.Distance(_playerTransform.position, _currentTarget.Position);
            }
        }

        public bool IsTargetInRange => TargetDistance <= _maxTargetRange;

        private void Awake()
        {
            _inputActions = new EtherDomesInput();
        }

        private void OnEnable()
        {
            if (_inputActions == null)
                _inputActions = new EtherDomesInput();

            _inputActions.Player.CycleTarget.performed += OnCycleTargetPerformed;
            _inputActions.Player.ClearTarget.performed += OnClearTargetPerformed;
            _inputActions.Player.Enable();
            
            Debug.Log("[TargetSystem] Input enabled - Tab to cycle targets, Escape to clear");
        }

        private void OnDisable()
        {
            if (_inputActions == null) return;

            _inputActions.Player.CycleTarget.performed -= OnCycleTargetPerformed;
            _inputActions.Player.ClearTarget.performed -= OnClearTargetPerformed;
            _inputActions.Player.Disable();
        }

        private void OnDestroy()
        {
            _inputActions?.Dispose();
            _registeredTargets.Clear();
        }

        private void OnCycleTargetPerformed(InputAction.CallbackContext context)
        {
            CycleTarget();
        }

        private void OnClearTargetPerformed(InputAction.CallbackContext context)
        {
            ClearTarget();
        }

        private void Update()
        {
            // Auto-find player transform if not set
            if (_playerTransform == null)
            {
                var localPlayer = FindLocalPlayer();
                if (localPlayer != null)
                {
                    _playerTransform = localPlayer;
                    Debug.Log("[TargetSystem] Found local player transform");
                }
            }
            
            // Check if current target is still valid
            if (_currentTarget != null)
            {
                if (!_currentTarget.IsAlive)
                {
                    HandleTargetDied();
                }
            }
        }
        
        private Transform FindLocalPlayer()
        {
            // Find local player using NGO's NetworkObject
            var networkObjects = FindObjectsByType<Unity.Netcode.NetworkObject>(FindObjectsSortMode.None);
            foreach (var netObj in networkObjects)
            {
                if (netObj.IsOwner)
                {
                    return netObj.transform;
                }
            }
            return null;
        }

        public void SetPlayerTransform(Transform playerTransform)
        {
            _playerTransform = playerTransform;
        }

        public void CycleTarget()
        {
            Debug.Log($"[TargetSystem] CycleTarget called. Registered targets: {_registeredTargets.Count}, Player: {_playerTransform != null}");
            
            var validTargets = GetValidEnemyTargets();
            
            Debug.Log($"[TargetSystem] Valid targets in range: {validTargets.Count}");
            
            if (validTargets.Count == 0)
            {
                Debug.Log("[TargetSystem] No valid targets found");
                ClearTarget();
                return;
            }

            // Sort by distance for consistent cycling
            validTargets = validTargets.OrderBy(t => 
                Vector3.Distance(_playerTransform.position, t.Position)).ToList();

            _cycleIndex++;
            if (_cycleIndex >= validTargets.Count)
                _cycleIndex = 0;

            SelectTarget(validTargets[_cycleIndex]);
        }


        public void SelectTarget(ITargetable target)
        {
            if (target == null)
            {
                ClearTarget();
                return;
            }

            // Don't re-select same target
            if (_currentTarget == target)
                return;

            // Clear indicator on previous target
            NotifyTargetSelected(_currentTarget, false);

            var previousTarget = _currentTarget;
            _currentTarget = target;

            // Show indicator on new target
            NotifyTargetSelected(_currentTarget, true);

            // Update cycle index to match selected target
            var validTargets = GetValidEnemyTargets();
            _cycleIndex = validTargets.IndexOf(target);

            Debug.Log($"[TargetSystem] Selected: {target.DisplayName} (Distance: {TargetDistance:F1}m)");
            OnTargetChanged?.Invoke(_currentTarget);
        }

        public void ClearTarget()
        {
            if (_currentTarget == null)
                return;

            // Clear indicator on current target
            NotifyTargetSelected(_currentTarget, false);

            var previousTarget = _currentTarget;
            _currentTarget = null;
            _cycleIndex = -1;

            Debug.Log("[TargetSystem] Target cleared");
            OnTargetChanged?.Invoke(null);
        }

        private void NotifyTargetSelected(ITargetable target, bool selected)
        {
            if (target == null) return;
            
            // Try to call SetTargeted on the target's GameObject via interface
            if (target.Transform != null)
            {
                var indicator = target.Transform.GetComponent<ITargetIndicator>();
                if (indicator != null)
                {
                    indicator.SetTargeted(selected);
                }
            }
        }

        public IReadOnlyList<ITargetable> GetTargetsInRange()
        {
            if (_playerTransform == null)
                return new List<ITargetable>();

            return _registeredTargets
                .Where(t => t != null && t.IsAlive)
                .Where(t => Vector3.Distance(_playerTransform.position, t.Position) <= _maxTargetRange)
                .ToList();
        }

        public void RegisterTarget(ITargetable target)
        {
            if (target == null || _registeredTargets.Contains(target))
                return;

            _registeredTargets.Add(target);
            Debug.Log($"[TargetSystem] Registered target: {target.DisplayName}. Total: {_registeredTargets.Count}");
        }

        public void UnregisterTarget(ITargetable target)
        {
            if (target == null)
                return;

            _registeredTargets.Remove(target);

            // Clear if this was our current target
            if (_currentTarget == target)
            {
                HandleTargetLost();
            }
        }

        private List<ITargetable> GetValidEnemyTargets()
        {
            if (_playerTransform == null)
                return new List<ITargetable>();

            return _registeredTargets
                .Where(t => t != null && t.IsAlive && t.Type == TargetType.Enemy)
                .Where(t => Vector3.Distance(_playerTransform.position, t.Position) <= _maxTargetRange)
                .Where(t => IsTargetVisible(t))
                .ToList();
        }

        private bool IsTargetVisible(ITargetable target)
        {
            // Simplified - always visible for now (raycast was blocking targets)
            return true;
        }

        private void HandleTargetDied()
        {
            Debug.Log($"[TargetSystem] Target died: {_currentTarget?.DisplayName}");
            _currentTarget = null;
            _cycleIndex = -1;
            OnTargetLost?.Invoke();
            OnTargetChanged?.Invoke(null);
        }

        private void HandleTargetLost()
        {
            Debug.Log("[TargetSystem] Target lost");
            _currentTarget = null;
            _cycleIndex = -1;
            OnTargetLost?.Invoke();
            OnTargetChanged?.Invoke(null);
        }
    }
}
