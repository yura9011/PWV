using UnityEngine;
using Unity.Netcode;
using EtherDomes.Combat.Abilities;
using EtherDomes.Combat.Targeting;

namespace EtherDomes.Combat.Visuals
{
    /// <summary>
    /// Controls homing projectile movement toward target.
    /// Visual-only on clients, damage handled by server.
    /// </summary>
    public class ProjectileController : NetworkBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float _speed = 20f;
        [SerializeField] private float _maxLifetime = 10f;
        [SerializeField] private float _hitDistance = 0.5f;

        [Header("Visual")]
        [SerializeField] private GameObject _impactEffectPrefab;
        [SerializeField] private AudioClip _impactSound;
        [SerializeField] private TrailRenderer _trail;

        private NetworkObjectReference _targetRef;
        private Transform _targetTransform;
        private Vector3 _lastKnownPosition;
        private AbilityDefinitionSO _ability;
        private ulong _casterId;
        private float _elapsed;
        private bool _hasReachedTarget;

        #region Initialization

        /// <summary>
        /// Initializes the projectile with target and ability data.
        /// </summary>
        public void Initialize(AbilityDefinitionSO ability, NetworkObjectReference targetRef, ulong casterId)
        {
            _ability = ability;
            _targetRef = targetRef;
            _casterId = casterId;

            if (ability != null)
            {
                _speed = ability.ProjectileSpeed;
                _impactEffectPrefab = ability.ImpactEffectPrefab;
                _impactSound = ability.ImpactSound;
            }

            // Try to get target transform
            if (targetRef.TryGet(out NetworkObject targetObj))
            {
                _targetTransform = targetObj.transform;
                _lastKnownPosition = _targetTransform.position + Vector3.up; // Aim at center mass
            }
        }

        /// <summary>
        /// Initializes for local/non-networked use.
        /// </summary>
        public void InitializeLocal(AbilityDefinitionSO ability, Transform target)
        {
            _ability = ability;
            _targetTransform = target;

            if (ability != null)
            {
                _speed = ability.ProjectileSpeed;
                _impactEffectPrefab = ability.ImpactEffectPrefab;
                _impactSound = ability.ImpactSound;
            }

            if (target != null)
            {
                _lastKnownPosition = target.position + Vector3.up;
            }
        }

        #endregion

        #region Unity Lifecycle

        private void Update()
        {
            if (_hasReachedTarget) return;

            _elapsed += Time.deltaTime;
            if (_elapsed >= _maxLifetime)
            {
                DestroyProjectile();
                return;
            }

            // Update target position if still valid
            if (_targetTransform != null)
            {
                _lastKnownPosition = _targetTransform.position + Vector3.up;
            }

            // Move toward target
            Vector3 direction = (_lastKnownPosition - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, _lastKnownPosition);

            if (distance <= _hitDistance)
            {
                OnReachTarget();
                return;
            }

            // Move
            float moveDistance = _speed * Time.deltaTime;
            if (moveDistance >= distance)
            {
                transform.position = _lastKnownPosition;
                OnReachTarget();
            }
            else
            {
                transform.position += direction * moveDistance;
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        #endregion

        #region Impact

        private void OnReachTarget()
        {
            if (_hasReachedTarget) return;
            _hasReachedTarget = true;

            // Play impact effect
            if (_impactEffectPrefab != null)
            {
                Instantiate(_impactEffectPrefab, transform.position, Quaternion.identity);
            }

            // Play impact sound
            if (_impactSound != null)
            {
                AudioSource.PlayClipAtPoint(_impactSound, transform.position);
            }

            // Notify clients if networked
            if (IsServer)
            {
                PlayImpactEffectClientRpc();
            }

            DestroyProjectile();
        }

        private void DestroyProjectile()
        {
            // Detach trail so it fades naturally
            if (_trail != null)
            {
                _trail.transform.SetParent(null);
                _trail.autodestruct = true;
            }

            if (IsSpawned)
            {
                NetworkObject.Despawn();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        [ClientRpc]
        private void PlayImpactEffectClientRpc()
        {
            if (IsServer) return; // Server already played

            if (_impactEffectPrefab != null)
            {
                Instantiate(_impactEffectPrefab, transform.position, Quaternion.identity);
            }

            if (_impactSound != null)
            {
                AudioSource.PlayClipAtPoint(_impactSound, transform.position);
            }
        }

        #endregion

        #region Static Spawn

        /// <summary>
        /// Spawns a projectile from caster to target.
        /// </summary>
        public static ProjectileController Spawn(
            AbilityDefinitionSO ability,
            Vector3 startPosition,
            Transform target)
        {
            if (ability == null || ability.ProjectilePrefab == null) return null;

            var go = Instantiate(ability.ProjectilePrefab, startPosition, Quaternion.identity);
            var controller = go.GetComponent<ProjectileController>();
            
            if (controller == null)
                controller = go.AddComponent<ProjectileController>();

            controller.InitializeLocal(ability, target);
            return controller;
        }

        #endregion
    }
}
