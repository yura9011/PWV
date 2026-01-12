using System;
using UnityEngine;
using EtherDomes.Combat;
using EtherDomes.Data;

namespace EtherDomes.Testing
{
    /// <summary>
    /// Simplified player for offline testing without networking.
    /// WoW-style controls:
    /// - W/S: Move forward/backward
    /// - A/D: Rotate player (turn left/right)
    /// - Q/E: Strafe (sidestep left/right)
    /// - Space: Jump
    /// - Left-click hold: Rotate camera only (free look)
    /// - Right-click hold: Rotate camera AND player direction, A/D become strafe
    /// - A+W or D+W with right mouse: Diagonal strafe movement
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
        [SerializeField] private float _turnSpeed = 150f;
        [SerializeField] private float _gravity = -20f;
        [SerializeField] private float _jumpHeight = 1.5f;
        
        [Header("Combat")]
        [SerializeField] private float _attackDamage = 50f;
        [SerializeField] private float _attackRange = 15f;
        [SerializeField] private float _attackCooldown = 0.5f;
        
        private CharacterController _controller;
        private Vector3 _velocity;
        private float _lastAttackTime;
        private bool _isAlive = true;
        private MeshRenderer _renderer;
        private TestEnemy _currentTarget;
        private Camera _camera;
        
        // WoW-style input
        private Vector2 _moveInput;
        private float _strafeInput;
        private bool _isRightMouseHeld;
        private bool _isLeftMouseHeld;
        
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
        
        // ITargetable events
        public event Action<ITargetable> OnDeath;
        
        public float GetThreatTo(ulong playerId) => 0f;
        
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
        }
        
        private void Update()
        {
            if (!_isAlive) return;
            
            ReadInput();
            HandleCameraRotation();
            HandleMovement();
            HandleTargeting();
            HandleCombat();
            RegenerateMana();
        }
        
        private void ReadInput()
        {
            _moveInput = Vector2.zero;
            _strafeInput = 0f;
            
            // WASD input
            _moveInput.x = Input.GetAxis("Horizontal"); // A/D
            _moveInput.y = Input.GetAxis("Vertical");   // W/S
            
            // Q/E strafe
            if (Input.GetKey(KeyCode.Q)) _strafeInput -= 1f;
            if (Input.GetKey(KeyCode.E)) _strafeInput += 1f;
            
            // Mouse buttons
            _isRightMouseHeld = Input.GetMouseButton(1);
            _isLeftMouseHeld = Input.GetMouseButton(0);
        }
        
        private void HandleCameraRotation()
        {
            // Right mouse: rotate player to match camera direction
            if (_isRightMouseHeld && _camera != null)
            {
                float cameraYaw = _camera.transform.eulerAngles.y;
                transform.rotation = Quaternion.Euler(0, cameraYaw, 0);
            }
        }
        
        private void HandleMovement()
        {
            if (_controller == null || !_controller.enabled) return;
            
            // Gravity & Jump
            if (_controller.isGrounded)
            {
                _velocity.y = -2f;
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
                    Debug.Log("[TestPlayer] Jump!");
                }
            }
            _velocity.y += _gravity * Time.deltaTime;
            
            // A/D: rotate when no right mouse held, strafe when right mouse held
            if (!_isRightMouseHeld && Mathf.Abs(_moveInput.x) > 0.1f)
            {
                transform.Rotate(0, _moveInput.x * _turnSpeed * Time.deltaTime, 0);
            }
            
            // Movement direction
            Vector3 moveDirection = Vector3.zero;
            
            // W/S forward/back
            if (Mathf.Abs(_moveInput.y) > 0.01f)
                moveDirection += transform.forward * _moveInput.y;
            
            // Strafe: Q/E always, A/D when right mouse held
            float totalStrafe = _strafeInput;
            if (_isRightMouseHeld)
                totalStrafe += _moveInput.x;
            
            if (Mathf.Abs(totalStrafe) > 0.01f)
                moveDirection += transform.right * Mathf.Clamp(totalStrafe, -1f, 1f);
            
            // Normalize diagonal movement
            if (moveDirection.sqrMagnitude > 1f)
                moveDirection.Normalize();
            
            // Apply movement
            Vector3 finalMove = moveDirection * _moveSpeed + Vector3.up * _velocity.y;
            _controller.Move(finalMove * Time.deltaTime);
        }
        
        private void HandleTargeting()
        {
            // Tab to cycle targets
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                CycleTarget();
            }
            
            // Click izquierdo para seleccionar target
            if (Input.GetMouseButtonDown(0))
            {
                TrySelectTarget();
            }
            
            // Escape to clear target
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClearTarget();
            }
        }
        
        private void TrySelectTarget()
        {
            if (_camera == null) return;
            
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
            // 1 para ataque básico (solo tecla 1, no Space)
            if (Input.GetKeyDown(KeyCode.Alpha1) && _currentTarget != null)
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
            
            // 2 for heavy attack
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
            
            if (!_currentTarget.IsAlive)
                ClearTarget();
        }
        
        private void HeavyAttack()
        {
            _lastAttackTime = Time.time;
            _currentMana = Mathf.Max(0, _currentMana - 50f);
            
            float heavyDamage = _attackDamage * 2.5f;
            _currentTarget.TakeDamage(heavyDamage);
            Debug.Log($"[TestPlayer] HEAVY ATTACK for {heavyDamage} damage!");
            
            if (!_currentTarget.IsAlive)
                ClearTarget();
        }
        
        private void RegenerateMana()
        {
            if (Time.time - _lastAttackTime > 3f)
            {
                _currentMana = Mathf.Min(_maxMana, _currentMana + _maxMana * 0.02f * Time.deltaTime);
            }
        }
        
        public void TakeDamage(float damage)
        {
            if (!_isAlive) return;
            
            _currentHealth = Mathf.Max(0, _currentHealth - damage);
            Debug.Log($"[TestPlayer] Took {damage} damage! HP: {_currentHealth}/{_maxHealth}");
            
            if (_currentHealth <= 0)
                Die();
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
                _renderer.material.color = Color.gray;
            
            OnDeath?.Invoke(this);
        }
        
        public void SetTargeted(bool targeted) { }
        
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, Screen.height - 120, 300, 110));
            
            GUI.color = Color.white;
            GUILayout.Label($"HP: {_currentHealth:F0} / {_maxHealth:F0}");
            GUILayout.Label($"Mana: {_currentMana:F0} / {_maxMana:F0}");
            
            if (_currentTarget != null)
            {
                GUILayout.Space(5);
                GUI.color = Color.red;
                GUILayout.Label($"Target: {_currentTarget.DisplayName}");
                GUILayout.Label($"Target HP: {_currentTarget.CurrentHealth:F0} / {_currentTarget.MaxHealth:F0}");
            }
            
            GUILayout.Space(5);
            GUI.color = Color.yellow;
            GUILayout.Label("[1] Ataque Basico | [2] Ataque Pesado");
            
            GUILayout.EndArea();
        }
    }
}
