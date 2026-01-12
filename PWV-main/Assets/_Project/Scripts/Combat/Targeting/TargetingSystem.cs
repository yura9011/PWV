using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace EtherDomes.Combat.Targeting
{
    // Use ITargetable from Combat namespace
    using EtherDomes.Combat;

    /// <summary>
    /// Handles target selection with Tab-targeting algorithm.
    /// Implements vision cone priority, Line of Sight checks, and auto-switch on death.
    /// </summary>
    public class TargetingSystem : NetworkBehaviour
    {
        #region Configuration

        [Header("Configuration")]
        [SerializeField] private CombatConfigSO _config;

        [Header("References")]
        [SerializeField] private Transform _cameraTransform;

        #endregion

        #region State

        private ITargetable _currentTarget;
        private List<ITargetable> _validTargets = new List<ITargetable>();
        private List<ITargetable> _cycleHistory = new List<ITargetable>();
        private int _cycleIndex = -1;

        /// <summary>Currently selected target.</summary>
        public ITargetable CurrentTarget => _currentTarget;

        /// <summary>Returns true if there is a valid target selected.</summary>
        public bool HasTarget => _currentTarget != null && _currentTarget.IsAlive;

        #endregion

        #region Events

        /// <summary>Fired when target changes (includes null for cleared).</summary>
        public event Action<ITargetable> OnTargetChanged;

        /// <summary>Fired when target is cleared.</summary>
        public event Action OnTargetCleared;

        /// <summary>Fired when auto-switch occurs.</summary>
        public event Action<ITargetable, ITargetable> OnAutoSwitch;

        #endregion

        #region Configuration Properties

        public float MaxTargetRange => _config != null ? _config.MaxTargetRange : 40f;
        public float AutoSwitchRange => _config != null ? _config.AutoSwitchRange : 10f;
        public float TabConeAngle => _config != null ? _config.TabConeAngle : 90f;
        public LayerMask EnemyLayer => _config != null ? _config.EnemyLayer : default;
        public LayerMask ObstacleLayer => _config != null ? _config.ObstacleLayer : default;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_config == null)
                _config = CombatConfigSO.Instance;

            if (_cameraTransform == null && Camera.main != null)
                _cameraTransform = Camera.main.transform;
        }

        private void OnDisable()
        {
            // Unsubscribe from current target death
            if (_currentTarget != null)
                _currentTarget.OnDeath -= HandleTargetDeath;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Cycles to the next valid target using Tab-targeting algorithm.
        /// </summary>
        public void TabTarget()
        {
            if (!IsOwner) return;

            // Get all valid targets
            RefreshValidTargets();

            if (_validTargets.Count == 0)
            {
                ClearTarget();
                return;
            }

            // Sort by priority (dot product)
            SortTargetsByPriority();

            // Cycle through targets
            _cycleIndex++;
            if (_cycleIndex >= _validTargets.Count)
            {
                _cycleIndex = 0;
                _cycleHistory.Clear();
            }

            // Skip already visited targets in this cycle
            while (_cycleHistory.Contains(_validTargets[_cycleIndex]) && _cycleHistory.Count < _validTargets.Count)
            {
                _cycleIndex++;
                if (_cycleIndex >= _validTargets.Count)
                    _cycleIndex = 0;
            }

            SetTarget(_validTargets[_cycleIndex]);
            _cycleHistory.Add(_validTargets[_cycleIndex]);
        }

        /// <summary>
        /// Sets a specific target.
        /// </summary>
        public void SetTarget(ITargetable target)
        {
            if (target == _currentTarget) return;

            // Unsubscribe from old target
            if (_currentTarget != null)
                _currentTarget.OnDeath -= HandleTargetDeath;

            var previousTarget = _currentTarget;
            _currentTarget = target;

            // Subscribe to new target
            if (_currentTarget != null)
                _currentTarget.OnDeath += HandleTargetDeath;

            // Reset cycle if manually setting target
            if (target != null && !_validTargets.Contains(target))
            {
                _cycleHistory.Clear();
                _cycleIndex = -1;
            }

            OnTargetChanged?.Invoke(_currentTarget);
        }

        /// <summary>
        /// Clears the current target.
        /// </summary>
        public void ClearTarget()
        {
            if (_currentTarget == null) return;

            _currentTarget.OnDeath -= HandleTargetDeath;
            _currentTarget = null;
            _cycleHistory.Clear();
            _cycleIndex = -1;

            OnTargetCleared?.Invoke();
            OnTargetChanged?.Invoke(null);
        }

        /// <summary>
        /// Returns true if current target is valid and selected.
        /// </summary>
        public bool HasValidTarget()
        {
            return _currentTarget != null && _currentTarget.IsAlive;
        }

        /// <summary>
        /// Returns true if current target is within specified range.
        /// </summary>
        public bool IsTargetInRange(float range)
        {
            if (_currentTarget == null) return false;
            
            float distance = Vector3.Distance(transform.position, _currentTarget.Position);
            return distance <= range;
        }

        /// <summary>
        /// Returns true if there is Line of Sight to current target.
        /// </summary>
        public bool HasLineOfSight()
        {
            if (_currentTarget == null) return false;
            return HasLineOfSightTo(_currentTarget);
        }

        /// <summary>
        /// Returns true if there is Line of Sight to a specific target.
        /// </summary>
        public bool HasLineOfSightTo(ITargetable target)
        {
            if (target == null) return false;

            Vector3 origin = transform.position + Vector3.up; // Eye level
            Vector3 targetPos = target.Position + Vector3.up;
            Vector3 direction = targetPos - origin;
            float distance = direction.magnitude;

            return !Physics.Raycast(origin, direction.normalized, distance, ObstacleLayer);
        }

        /// <summary>
        /// Gets all currently valid targets within range.
        /// </summary>
        public List<ITargetable> GetValidTargets()
        {
            RefreshValidTargets();
            return new List<ITargetable>(_validTargets);
        }

        #endregion

        #region Internal Methods

        private void RefreshValidTargets()
        {
            _validTargets.Clear();

            // Get all colliders in range
            Collider[] colliders = Physics.OverlapSphere(transform.position, MaxTargetRange, EnemyLayer);

            foreach (var collider in colliders)
            {
                var targetable = collider.GetComponent<ITargetable>();
                if (targetable == null) continue;
                if (!targetable.IsAlive) continue;

                // Frustum culling - check if in camera view
                if (!IsInCameraFrustum(targetable.Position)) continue;

                // Line of Sight check
                if (!HasLineOfSightTo(targetable)) continue;

                _validTargets.Add(targetable);
            }
        }

        private void SortTargetsByPriority()
        {
            if (_cameraTransform == null) return;

            _validTargets.Sort((a, b) =>
            {
                float priorityA = CalculateTargetPriority(a);
                float priorityB = CalculateTargetPriority(b);
                return priorityB.CompareTo(priorityA); // Descending order
            });
        }

        /// <summary>
        /// Calculates priority based on dot product (how centered on screen).
        /// </summary>
        public float CalculateTargetPriority(ITargetable target)
        {
            if (target == null || _cameraTransform == null) return -1f;

            Vector3 directionToTarget = (target.Position - _cameraTransform.position).normalized;
            float dotProduct = Vector3.Dot(_cameraTransform.forward, directionToTarget);

            // Filter by cone angle
            float coneThreshold = Mathf.Cos(TabConeAngle * 0.5f * Mathf.Deg2Rad);
            if (dotProduct < coneThreshold)
                return -1f; // Outside cone

            return dotProduct;
        }

        private bool IsInCameraFrustum(Vector3 position)
        {
            if (Camera.main == null) return true; // Assume visible if no camera

            Vector3 viewportPoint = Camera.main.WorldToViewportPoint(position);
            return viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
                   viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
                   viewportPoint.z > 0;
        }

        private void HandleTargetDeath(ITargetable deadTarget)
        {
            if (deadTarget != _currentTarget) return;

            // Auto-switch to new target
            var newTarget = FindAutoSwitchTarget(deadTarget.Position);
            var previousTarget = _currentTarget;

            if (newTarget != null)
            {
                SetTarget(newTarget);
                OnAutoSwitch?.Invoke(previousTarget, newTarget);
            }
            else
            {
                ClearTarget();
            }
        }

        private ITargetable FindAutoSwitchTarget(Vector3 deathPosition)
        {
            // Get enemies within auto-switch range of death position
            Collider[] colliders = Physics.OverlapSphere(deathPosition, AutoSwitchRange, EnemyLayer);
            
            ITargetable bestTarget = null;
            float bestScore = float.MinValue;
            ulong myClientId = NetworkManager.Singleton?.LocalClientId ?? 0;

            foreach (var collider in colliders)
            {
                var targetable = collider.GetComponent<ITargetable>();
                if (targetable == null) continue;
                if (!targetable.IsAlive) continue;
                if (!HasLineOfSightTo(targetable)) continue;

                // Calculate score: threat first, then distance
                float threat = targetable.GetThreatTo(myClientId);
                float distance = Vector3.Distance(deathPosition, targetable.Position);
                
                // Score: high threat = high score, close distance = high score
                float score = threat * 1000f - distance;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = targetable;
                }
            }

            return bestTarget;
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
        /// Sets the camera transform for testing purposes.
        /// </summary>
        public void SetCameraTransform(Transform cameraTransform)
        {
            _cameraTransform = cameraTransform;
        }

        /// <summary>
        /// Forces valid targets list for testing purposes.
        /// </summary>
        public void ForceValidTargets(List<ITargetable> targets)
        {
            _validTargets = new List<ITargetable>(targets);
        }

        #endregion
    }
}
