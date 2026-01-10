using UnityEngine;
using EtherDomes.Combat;
using EtherDomes.Data;

namespace EtherDomes.Testing
{
    /// <summary>
    /// Simplified player for offline testing without networking.
    /// Green capsule controlled with WASD, can attack with Space.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class TestPlayer : MonoBehaviour, ITargetable
    {
        [Header("Identity")]
        [SerializeField] private string _displayName = "Test Player";
        [SerializeField] private int _level = 10;
        
        [Header("Stats")]
        [SerializeField] private float _maxHealth = 1000f;
        [SerializeField] private float _currentHealth;
        [SerializeField] private float _maxMana = 500f;
        [SerializeField] private float _currentMana;
        
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 7f;
        [SerializeField] private float _rotationSpeed = 720f;
        [SerializeField] private float _gravity = -20f;
        
        [Header("Combat")]
        [SerializeField] private float _attackDamage = 50f;
        [SerializeField] private float _attackRange = 3f;
        [SerializeField] private float _attackCooldown = 1f;
        
        [Header("Camera")]
        [SerializeField] private float _cameraHeight = 15f;
        [SerializeField] private float _cameraAngle = 45f;
        [SerializeField] private float _cameraFollowSpeed = 8f;
        
        private CharacterController _controller;
        private Vector3 _velocity;
        private float _lastAttackTime;
        private bool _isAlive = true;
        private MeshRenderer _renderer;
        private TestEnemy _currentTarget;
        private Camera _camera;
        
        private static ulong _idCounter = 1;
        private ulong _networkId;
        
        // ITargetable
        public ulong NetworkId => _networkId;
        public string DisplayName => _displayName;
        public Vector3 Position => transform.position;
        public bool IsAlive => _isAlive;
        public TargetType Type => TargetType.Friendly;
        public float HealthPercent => _maxHealth > 0 ? _currentHealth / _maxHealth : 0f;
        public int Level => _level;
        public Transform Transform => transform;
        
        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public float CurrentMana => _currentMana;
        public float MaxMana => _maxMana;
        public TestEnemy CurrentTarget => _currentTarget;
        
        private void Awake()
        {
            _networkId = _idCounter++;
            _controller = GetComponent<CharacterController>();
            _renderer = GetComponentInChildren<MeshRenderer>();
            _camera = Camera.main;
        }
        
        private void Start()
        {
            _currentHealth = _maxHealth;
            _currentMana = _maxMana;
            
            // Setup camera
            if (_camera != null)
            {
                UpdateCamera();
            }
        }
        
        private void Update()
        {
            if (!_isAlive) return;
            
            HandleMovement();
            HandleTargeting();
            HandleCombat();
            UpdateCamera();
            RegenerateMana();
        }
        
        private void HandleMovement()
        {
            // WASD movement
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            
            Vector3 inputDir = new Vector3(horizontal, 0, vertical).normalized;
            
            // Camera-relative movement
            if (_camera != null && inputDir.magnitude > 0.1f)
            {
                Vector3 camForward = _camera.transform.forward;
                camForward.y = 0;
                camForward.Normalize();
                
                Vector3 camRight = _camera.transform.right;
                camRight.y = 0;
                camRight.Normalize();
                
                Vector3 moveDir = (camForward * vertical + camRight * horizontal).normalized;
                
                // Move
                _controller.Move(moveDir * _moveSpeed * Time.deltaTime);
                
                // Rotate to face movement direction
                if (moveDir != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(moveDir);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, _rotationSpeed * Time.deltaTime);
                }
            }
            
            // Gravity
            if (_controller.isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }
            _velocity.y += _gravity * Time.deltaTime;
            _controller.Move(_velocity * Time.deltaTime);
        }
        
        private void HandleTargeting()
        {
            // Tab to cycle targets
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                CycleTarget();
            }
            
            // Click to target
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f))
                {
                    var enemy = hit.collider.GetComponent<TestEnemy>();
                    if (enemy != null && enemy.IsAlive)
                    {
                        SetTarget(enemy);
                    }
                }
            }
            
            // Escape to clear target
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClearTarget();
            }
        }
        
        private void CycleTarget()
        {
            var enemies = FindObjectsByType<TestEnemy>(FindObjectsSortMode.None);
            TestEnemy nearest = null;
            float nearestDist = float.MaxValue;
            bool foundCurrent = false;
            TestEnemy nextAfterCurrent = null;
            
            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;
                
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = enemy;
                }
                
                if (_currentTarget == enemy)
                {
                    foundCurrent = true;
                }
                else if (foundCurrent && nextAfterCurrent == null)
                {
                    nextAfterCurrent = enemy;
                }
            }
            
            if (nextAfterCurrent != null)
                SetTarget(nextAfterCurrent);
            else if (nearest != null)
                SetTarget(nearest);
        }
        
        private void SetTarget(TestEnemy enemy)
        {
            if (_currentTarget != null)
                _currentTarget.SetTargeted(false);
            
            _currentTarget = enemy;
            
            if (_currentTarget != null)
            {
                _currentTarget.SetTargeted(true);
                Debug.Log($"[TestPlayer] Targeting: {_currentTarget.DisplayName}");
            }
        }
        
        private void ClearTarget()
        {
            if (_currentTarget != null)
            {
                _currentTarget.SetTargeted(false);
                _currentTarget = null;
                Debug.Log("[TestPlayer] Target cleared");
            }
        }
        
        private void HandleCombat()
        {
            // Space or 1 to attack
            if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Alpha1)) && _currentTarget != null)
            {
                if (Time.time - _lastAttackTime >= _attackCooldown)
                {
                    float dist = Vector3.Distance(transform.position, _currentTarget.transform.position);
                    if (dist <= _attackRange)
                    {
                        Attack();
                    }
                    else
                    {
                        Debug.Log("[TestPlayer] Target out of range!");
                    }
                }
            }
            
            // 2 for heavy attack (more damage, more mana)
            if (Input.GetKeyDown(KeyCode.Alpha2) && _currentTarget != null)
            {
                if (Time.time - _lastAttackTime >= _attackCooldown && _currentMana >= 50f)
                {
                    float dist = Vector3.Distance(transform.position, _currentTarget.transform.position);
                    if (dist <= _attackRange)
                    {
                        HeavyAttack();
                    }
                }
            }
        }
        
        private void Attack()
        {
            _lastAttackTime = Time.time;
            _currentMana = Mathf.Max(0, _currentMana - 10f);
            
            _currentTarget.TakeDamage(_attackDamage);
            Debug.Log($"[TestPlayer] Basic attack for {_attackDamage} damage!");
            
            // Check if target died
            if (!_currentTarget.IsAlive)
            {
                ClearTarget();
            }
        }
        
        private void HeavyAttack()
        {
            _lastAttackTime = Time.time;
            _currentMana = Mathf.Max(0, _currentMana - 50f);
            
            float heavyDamage = _attackDamage * 2.5f;
            _currentTarget.TakeDamage(heavyDamage);
            Debug.Log($"[TestPlayer] HEAVY ATTACK for {heavyDamage} damage!");
            
            if (!_currentTarget.IsAlive)
            {
                ClearTarget();
            }
        }
        
        private void RegenerateMana()
        {
            // Regen 2% per second out of combat
            if (Time.time - _lastAttackTime > 3f)
            {
                _currentMana = Mathf.Min(_maxMana, _currentMana + _maxMana * 0.02f * Time.deltaTime);
            }
        }
        
        private void UpdateCamera()
        {
            if (_camera == null) return;
            
            // Fixed top-down camera that follows player position only (no rotation)
            Vector3 targetPos = transform.position + new Vector3(0, _cameraHeight, -_cameraHeight * 0.6f);
            _camera.transform.position = Vector3.Lerp(_camera.transform.position, targetPos, Time.deltaTime * _cameraFollowSpeed);
            
            // Fixed angle looking down at player
            _camera.transform.rotation = Quaternion.Euler(_cameraAngle, 0, 0);
        }
        
        public void TakeDamage(float damage)
        {
            if (!_isAlive) return;
            
            _currentHealth = Mathf.Max(0, _currentHealth - damage);
            Debug.Log($"[TestPlayer] Took {damage} damage! HP: {_currentHealth}/{_maxHealth}");
            
            if (_currentHealth <= 0)
            {
                Die();
            }
        }
        
        public void Heal(float amount)
        {
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
        }
        
        private void Die()
        {
            _isAlive = false;
            Debug.Log("[TestPlayer] YOU DIED!");
            
            if (_renderer != null)
            {
                _renderer.material.color = Color.gray;
            }
        }
        
        public void SetTargeted(bool targeted)
        {
            // Players don't need target indicator in this test
        }
        
        private void OnGUI()
        {
            // HUD
            GUILayout.BeginArea(new Rect(10, Screen.height - 150, 300, 140));
            
            GUI.color = Color.white;
            GUILayout.Label($"HP: {_currentHealth:F0} / {_maxHealth:F0}");
            GUILayout.Label($"Mana: {_currentMana:F0} / {_maxMana:F0}");
            
            if (_currentTarget != null)
            {
                GUILayout.Space(10);
                GUI.color = Color.red;
                GUILayout.Label($"Target: {_currentTarget.DisplayName}");
                GUILayout.Label($"Target HP: {_currentTarget.CurrentHealth:F0} / {_currentTarget.MaxHealth:F0}");
            }
            
            GUILayout.Space(10);
            GUI.color = Color.yellow;
            GUILayout.Label("Controls: WASD=Move, Tab=Target, Space/1=Attack, 2=Heavy");
            
            GUILayout.EndArea();
        }
    }
}
