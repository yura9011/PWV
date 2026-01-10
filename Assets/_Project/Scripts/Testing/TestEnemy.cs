using UnityEngine;
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
        [SerializeField] private float _aggroRange = 15f;
        [SerializeField] private float _attackRange = 2f;
        [SerializeField] private float _moveSpeed = 3.5f;
        [SerializeField] private float _attackCooldown = 2f;
        [SerializeField] private float _damage = 20f;
        
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
        
        private void Awake()
        {
            _networkId = _idCounter++;
            _renderer = GetComponent<MeshRenderer>();
        }
        
        private void Start()
        {
            _currentHealth = _maxHealth;
            _spawnPosition = transform.position;
        }
        
        private void Update()
        {
            if (!_isAlive) return;
            
            // Simple AI: detect player, move towards, attack
            if (_target == null)
            {
                DetectPlayer();
            }
            else
            {
                float distance = Vector3.Distance(transform.position, _target.position);
                
                if (distance > _attackRange)
                {
                    // Move towards target
                    Vector3 direction = (_target.position - transform.position).normalized;
                    direction.y = 0;
                    transform.position += direction * _moveSpeed * Time.deltaTime;
                    transform.forward = direction;
                }
                else
                {
                    // Attack
                    if (Time.time - _lastAttackTime >= _attackCooldown)
                    {
                        _lastAttackTime = Time.time;
                        Attack();
                    }
                }
                
                // Check if target is still valid
                var targetable = _target.GetComponent<ITargetable>();
                if (targetable == null || !targetable.IsAlive)
                {
                    _target = null;
                }
            }
        }
        
        private void DetectPlayer()
        {
            var players = FindObjectsByType<TestPlayer>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                if (!player.IsAlive) continue;
                
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance <= _aggroRange)
                {
                    _target = player.transform;
                    Debug.Log($"[TestEnemy] {_displayName} detected player!");
                    break;
                }
            }
        }
        
        private void Attack()
        {
            if (_target == null) return;
            
            var player = _target.GetComponent<TestPlayer>();
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
            
            if (_renderer != null)
            {
                _renderer.material.color = Color.gray;
            }
            
            // Disable collider
            var collider = GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;
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
    }
}
