using System;
using UnityEngine;
using UnityEngine.AI;
using EtherDomes.Combat;
using EtherDomes.Data;

namespace EtherDomes.Testing
{
    /// <summary>
    /// Simplified enemy for offline testing without networking.
    /// Red cube that can be targeted and takes damage.
    /// </summary>
    public class TestEnemy : MonoBehaviour, ITargetable
    {
        [Header("Identity")]
        [SerializeField] private string _displayName = "Test Enemy";
        [SerializeField] private int _level = 1;
        
        [Header("Stats")]
        [SerializeField] private float _maxHealth = 500f;
        [SerializeField] private float _currentHealth;
        
        [Header("AI")]
        [SerializeField] private float _aggroRange = 10f;
        [SerializeField] private float _attackRange = 5f; // Increased from 2.5f to 5f
        [SerializeField] private float _moveSpeed = 4f;
        [SerializeField] private float _attackCooldown = 1.5f;
        [SerializeField] private float _damage = 25f;
        
        // Components
        private SmartPathfinding3D _pathfinding;
        private NavMeshAgent _navAgent;
        
        private Transform _target;
        private float _lastAttackTime;
        private Vector3 _spawnPosition;
        private bool _isAlive = true;
        private MeshRenderer _renderer;
        private static ulong _idCounter = 1000;
        private ulong _networkId;
        
        // ITargetable
        public ulong NetworkId => _networkId;
        public string DisplayName => _displayName;
        public Vector3 Position => transform.position;
        public bool IsAlive => _isAlive;
        public TargetType Type => TargetType.Enemy;
        public float HealthPercent => _maxHealth > 0 ? _currentHealth / _maxHealth : 0f;
        public int Level => _level;
        public Transform Transform => transform;
        
        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        
        // Current target for auto-switch targeting
        public Transform CurrentTarget => _target;
        
        // ITargetable events
        public event Action<ITargetable> OnDeath;
        
        public float GetThreatTo(ulong playerId) => 0f; // Test enemies don't track threat
        
        private void Awake()
        {
            _networkId = _idCounter++;
            _renderer = GetComponent<MeshRenderer>();
            _pathfinding = GetComponent<SmartPathfinding3D>();
            _navAgent = GetComponent<NavMeshAgent>();
            
            if (_pathfinding == null)
            {
                Debug.LogError($"[TestEnemy] {name} requires SmartPathfinding3D component!");
            }
            
            if (_navAgent == null)
            {
                Debug.LogError($"[TestEnemy] {name} requires NavMeshAgent component!");
            }
        }
        
        private void Start()
        {
            _currentHealth = _maxHealth;
            _spawnPosition = transform.position;
            
            // Configurar NavMeshAgent
            if (_navAgent != null)
            {
                _navAgent.speed = _moveSpeed;
                _navAgent.stoppingDistance = _attackRange;
                _navAgent.updateRotation = false; // Manejamos rotación manualmente
                _navAgent.updateUpAxis = false;
            }
            
            Debug.Log($"[TestEnemy] {_displayName} initialized with NavMesh pathfinding");
        }
        
        private void Update()
        {
            if (!_isAlive) return;
            
            // Simple AI: detect player, move towards, attack
            if (_target == null)
            {
                DetectPlayer();
            }
            
            if (_target != null)
            {
                // Check if target is still valid
                var targetable = _target.GetComponent<ITargetable>();
                if (targetable == null || !targetable.IsAlive)
                {
                    _target = null;
                    _pathfinding?.StopPathfinding();
                    return;
                }
                
                float distance = Vector3.Distance(transform.position, _target.position);
                
                if (distance > _attackRange)
                {
                    // NUEVO: Usar SmartPathfinding3D
                    if (_pathfinding != null)
                    {
                        _pathfinding.SetTarget(_target);
                        
                        // Rotar hacia la dirección de movimiento
                        Vector3 moveDirection = _pathfinding.GetMovementDirection();
                        if (moveDirection.sqrMagnitude > 0.01f)
                        {
                            Vector3 lookDirection = moveDirection;
                            lookDirection.y = 0; // Mantener en plano horizontal
                            if (lookDirection.sqrMagnitude > 0.01f)
                            {
                                transform.forward = lookDirection.normalized;
                            }
                        }
                        
                        // Debug info
                        if (_pathfinding.HasPath)
                        {
                            Debug.Log($"[TestEnemy] {_displayName} following NavMesh path. Remaining distance: {_pathfinding.RemainingDistance:F1}m");
                        }
                        else if (_pathfinding.IsPathfinding)
                        {
                            Debug.Log($"[TestEnemy] {_displayName} calculating path...");
                        }
                        else
                        {
                            Debug.LogWarning($"[TestEnemy] {_displayName} no path available to target!");
                        }
                    }
                    else
                    {
                        // Fallback: movimiento directo (sin pathfinding)
                        Vector3 direction = (_target.position - transform.position).normalized;
                        direction.y = 0;
                        
                        if (direction.sqrMagnitude > 0.01f)
                        {
                            transform.forward = direction;
                            transform.position += direction * _moveSpeed * Time.deltaTime;
                        }
                        
                        Debug.LogWarning($"[TestEnemy] {_displayName} using fallback movement (no pathfinding component)");
                    }
                }
                else
                {
                    // In range - stop pathfinding and face target for attack
                    _pathfinding?.StopPathfinding();
                    
                    Vector3 direction = (_target.position - transform.position).normalized;
                    direction.y = 0;
                    if (direction.sqrMagnitude > 0.01f)
                    {
                        transform.forward = direction;
                    }
                    
                    // Attack if we have line of sight
                    if (Time.time - _lastAttackTime >= _attackCooldown)
                    {
                        // Simplified: always attack if in range (remove line of sight check for testing)
                        _lastAttackTime = Time.time;
                        Attack();
                        Debug.Log($"[TestEnemy] {_displayName} attacking player at {distance:F1}m range");
                    }
                }
            }
            else
            {
                // No target - stop pathfinding
                _pathfinding?.StopPathfinding();
            }
        }
        
        private void DetectPlayer()
        {
            var players = FindObjectsByType<OfflinePlayerController>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                if (!player.IsAlive) continue;
                
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance <= _aggroRange)
                {
                    // Aggro immediately - no line of sight check for simplicity
                    _target = player.transform;
                    Debug.Log($"[TestEnemy] {_displayName} AGGRO on player! Distance: {distance:F1}");
                    break;
                }
            }
        }
        
        private void Attack()
        {
            if (_target == null) return;
            
            var player = _target.GetComponent<OfflinePlayerController>();
            if (player != null)
            {
                player.TakeDamage(_damage);
                Debug.Log($"[TestEnemy] {_displayName} attacks for {_damage} damage!");
            }
        }
        
        public void TakeDamage(float damage)
        {
            if (!_isAlive) return;
            
            _currentHealth = Mathf.Max(0, _currentHealth - damage);
            Debug.Log($"[TestEnemy] {_displayName} took {damage} damage. HP: {_currentHealth}/{_maxHealth}");
            
            // Aggro on damage - find who attacked us
            if (_target == null)
            {
                var players = FindObjectsByType<OfflinePlayerController>(FindObjectsSortMode.None);
                foreach (var player in players)
                {
                    if (player.IsAlive)
                    {
                        _target = player.transform;
                        Debug.Log($"[TestEnemy] {_displayName} AGGRO from damage!");
                        break;
                    }
                }
            }
            
            // Flash red
            if (_renderer != null)
            {
                StartCoroutine(FlashDamage());
            }
            
            if (_currentHealth <= 0)
            {
                Die();
            }
        }
        
        private System.Collections.IEnumerator FlashDamage()
        {
            var originalColor = _renderer.material.color;
            _renderer.material.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            if (_renderer != null)
                _renderer.material.color = originalColor;
        }
        
        private void Die()
        {
            _isAlive = false;
            Debug.Log($"[TestEnemy] {_displayName} died!");
            
            // CRÍTICO: Detener pathfinding al morir
            _pathfinding?.StopPathfinding();
            if (_navAgent != null)
            {
                _navAgent.enabled = false; // Desactivar NavMeshAgent
            }
            
            if (_renderer != null)
            {
                _renderer.material.color = Color.gray;
            }
            
            // Disable collider
            var collider = GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;
            
            OnDeath?.Invoke(this);
        }
        
        public void SetTargeted(bool targeted)
        {
            // Visual feedback for targeting
            if (_renderer != null && _isAlive)
            {
                _renderer.material.color = targeted ? new Color(1f, 0.3f, 0.3f) : Color.red;
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _aggroRange);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRange);
        }
        
        private void OnDrawGizmos()
        {
            // Always show aggro range in red (semi-transparent)
            if (_isAlive)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
                Gizmos.DrawSphere(transform.position, _aggroRange);
                Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
                Gizmos.DrawWireSphere(transform.position, _aggroRange);
            }
        }
    }
}
