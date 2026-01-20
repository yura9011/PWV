using System;
using UnityEngine;
using EtherDomes.Combat;
using EtherDomes.Combat.Abilities;
using EtherDomes.Combat.Visuals;
using EtherDomes.Data;

namespace EtherDomes.Testing
{
    /// <summary>
    /// Offline player controller for testing without networking.
    /// Now uses the new AbilityDefinition system instead of hardcoded attacks.
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
    [RequireComponent(typeof(CombatStateMachine))]
    [RequireComponent(typeof(AbilitySystem))]
    public class OfflinePlayerController : MonoBehaviour, ITargetable
    {
        [Header("Identity")]
        [SerializeField] private string _displayName = "Offline Player";
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
        
        [Header("Abilities")]
        [SerializeField] private AbilityDefinition _basicAttack;
        [SerializeField] private AbilityDefinition _heavyAttack;
        [SerializeField] private AbilityDefinition _heal;
        [SerializeField] private AbilityDefinition _drainLife;
        
        [Header("Combat (Legacy - for reference)")]
        [SerializeField] private float _attackDamage = 50f;
        [SerializeField] private float _attackRange = 15f;
        [SerializeField] private float _attackCooldown = 0.5f;
        [SerializeField] private float _healAmount = 25f;
        [SerializeField] private float _healManaCost = 25f;
        [SerializeField] private float _manaRegenPerSecond = 10f;
        [SerializeField] private float _rangedDamage = 100f;
        [SerializeField] private float _rangedRange = 20f;
        [SerializeField] private float _rangedCooldown = 5f;
        [SerializeField] private float _rangedManaCost = 30f;
        
        [Header("Targeting")]
        [SerializeField] private float _tabTargetRange = 40f;
        [SerializeField] private float _autoSwitchRange = 10f;
        [SerializeField] private LayerMask _enemyLayers = 1 << 8; // Enemy layer
        
        private CharacterController _controller;
        private CombatStateMachine _stateMachine;
        private AbilitySystem _abilitySystem;
        private TargetSystem _targetSystem;
        private Vector3 _velocity;
        private float _lastAttackTime;
        private float _lastRangedTime;
        private bool _isAlive = true;
        private MeshRenderer _renderer;
        private TestEnemy _currentTarget;
        private Camera _camera;
        private bool _isMeditating = false;
        
        // WoW-style input
        private Vector2 _moveInput;
        private float _strafeInput;
        private bool _isRightMouseHeld;
        private bool _isLeftMouseHeld;
        
        private static ulong _idCounter = 1;
        private ulong _networkId;
        
        // Cast bar variables
        private Coroutine _currentCastCoroutine;
        private float _currentCastProgress;
        private string _currentCastName;
        private float _currentCastDuration;
        private bool _isChanneling; // true = channeling (countdown), false = progressive cast
        
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
            _stateMachine = GetComponent<CombatStateMachine>();
            _abilitySystem = GetComponent<AbilitySystem>();
            _targetSystem = GetComponent<TargetSystem>();
            _renderer = GetComponentInChildren<MeshRenderer>();
            _camera = Camera.main;

            if (_targetSystem == null)
                _targetSystem = gameObject.AddComponent<TargetSystem>();
        }
        
        private void Start()
        {
            _currentHealth = _maxHealth;
            _currentMana = _maxMana;

            Debug.Log("[OfflinePlayerController] Start() - Using simplified ability system");
            Debug.Log($"[OfflinePlayerController] TargetSystem: {(_targetSystem != null ? "Found" : "NULL")}");
            Debug.Log($"[OfflinePlayerController] StateMachine: {(_stateMachine != null ? "Found" : "NULL")}");

            // Subscribe to events if available
            if (_stateMachine != null)
            {
                _stateMachine.OnStateChanged += OnCombatStateChanged;
                _stateMachine.OnCastProgress += OnCastProgress;
                _stateMachine.OnCastCompleted += OnCastCompleted;
                _stateMachine.OnCastInterrupted += OnCastInterrupted;
                Debug.Log("[OfflinePlayerController] Subscribed to StateMachine events");
            }

            Debug.Log("[OfflinePlayerController] Initialized with simplified ability system");
        }
        
        private void Update()
        {
            if (!_isAlive) return;
            
            ReadInput();
            HandleCameraRotation();
            HandleMovement();
            HandleTargeting();
            HandleAbilities(); // Changed from HandleCombat
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
                }
            }
            _velocity.y += _gravity * Time.deltaTime;
            
            // Check if moving forward (W pressed)
            bool isMovingForward = _moveInput.y > 0.1f;
            
            // A/D behavior:
            // - If W is held (moving forward): A/D = strafe
            // - If right mouse held: A/D = strafe
            // - Otherwise: A/D = rotate
            bool shouldStrafe = isMovingForward || _isRightMouseHeld;
            
            if (!shouldStrafe && Mathf.Abs(_moveInput.x) > 0.1f)
            {
                // Only rotate when standing still without right mouse
                transform.Rotate(0, _moveInput.x * _turnSpeed * Time.deltaTime, 0);
            }
            
            // Movement direction
            Vector3 moveDirection = Vector3.zero;
            
            // W/S forward/back
            if (Mathf.Abs(_moveInput.y) > 0.01f)
                moveDirection += transform.forward * _moveInput.y;
            
            // Strafe: Q/E always, A/D when moving forward or right mouse held
            float totalStrafe = _strafeInput;
            if (shouldStrafe)
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
            // Algoritmo Tab mejorado con cono de visión
            var bestTarget = FindBestTabTarget();
            if (bestTarget != null)
            {
                SetTarget(bestTarget);
            }
        }
        
        private TestEnemy FindBestTabTarget()
        {
            // 1. Physics.OverlapSphere: Obtener todos los enemigos en radio
            Collider[] colliders = Physics.OverlapSphere(transform.position, _tabTargetRange, _enemyLayers);
            
            TestEnemy bestTarget = null;
            float bestScore = -1f;
            
            foreach (var collider in colliders)
            {
                var enemy = collider.GetComponent<TestEnemy>();
                if (enemy == null || !enemy.IsAlive) continue;
                
                // Skip current target for cycling
                if (enemy == _currentTarget) continue;
                
                // 2. Culling de Pantalla: Verificar si está en el frustum de la cámara
                if (!IsInCameraFrustum(enemy.transform.position)) continue;
                
                // 3. Line of Sight: Raycast para verificar que no hay paredes
                if (!HasLineOfSight(enemy.transform.position)) continue;
                
                // 4. Prioridad Central: Calcular score basado en posición en pantalla
                float score = CalculateTargetScore(enemy.transform.position);
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = enemy;
                }
            }
            
            // Si no encontramos nada mejor, buscar el más cercano como fallback (también con line of sight)
            if (bestTarget == null)
            {
                bestTarget = FindNearestEnemy();
            }
            
            return bestTarget;
        }
        
        private bool IsInCameraFrustum(Vector3 worldPosition)
        {
            if (_camera == null) return true; // Fallback si no hay cámara
            
            // Convertir posición mundial a viewport
            Vector3 viewportPoint = _camera.WorldToViewportPoint(worldPosition);
            
            // Verificar si está dentro del frustum (0-1 en X e Y, Z > 0)
            return viewportPoint.x >= 0f && viewportPoint.x <= 1f &&
                   viewportPoint.y >= 0f && viewportPoint.y <= 1f &&
                   viewportPoint.z > 0f;
        }
        
        private bool HasLineOfSight(Vector3 targetPosition)
        {
            Vector3 origin = transform.position + Vector3.up * 1.6f; // Eye level
            Vector3 direction = (targetPosition + Vector3.up * 1.0f) - origin; // Apuntar al centro del enemigo
            float distance = direction.magnitude;
            
            // Raycast para verificar obstáculos (solo paredes/estructuras, no enemigos ni jugadores)
            int wallLayerMask = LayerMask.GetMask("Default");
            
            bool hasLineOfSight = !Physics.Raycast(origin, direction.normalized, distance - 0.1f, wallLayerMask);
            
            return hasLineOfSight;
        }
        
        private bool CanAttackTarget()
        {
            if (_currentTarget == null) return false;
            
            // Verificar rango
            float dist = Vector3.Distance(transform.position, _currentTarget.transform.position);
            if (dist > _attackRange) 
            {
                Debug.Log("[OfflinePlayerController] Target out of range!");
                return false;
            }
            
            // Verificar ángulo (target debe estar en frente)
            Vector3 dirToTarget = (_currentTarget.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToTarget);
            if (angle > 90f) 
            {
                Debug.Log("[OfflinePlayerController] Target is behind you! Turn to face the enemy.");
                return false;
            }
            
            // Verificar line-of-sight (no hay paredes en el medio)
            if (!HasLineOfSight(_currentTarget.transform.position))
            {
                Debug.Log("[OfflinePlayerController] Cannot attack through walls!");
                return false;
            }
            
            return true;
        }
        
        private bool CanRangedAttackTarget()
        {
            if (_currentTarget == null) return false;
            
            // Verificar rango ranged
            float dist = Vector3.Distance(transform.position, _currentTarget.transform.position);
            if (dist > _rangedRange) 
            {
                Debug.Log($"[OfflinePlayerController] Target out of ranged range! ({dist:F1}m > {_rangedRange}m)");
                return false;
            }
            
            // Verificar ángulo (target debe estar en frente)
            Vector3 dirToTarget = (_currentTarget.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToTarget);
            if (angle > 90f) 
            {
                Debug.Log("[OfflinePlayerController] Target is behind you! Turn to face the enemy.");
                return false;
            }
            
            // Verificar line-of-sight (no hay paredes en el medio)
            if (!HasLineOfSight(_currentTarget.transform.position))
            {
                Debug.Log("[OfflinePlayerController] Cannot attack through walls!");
                return false;
            }
            
            return true;
        }
        
        private float CalculateTargetScore(Vector3 targetPosition)
        {
            if (_camera == null) return 0f;
            
            // Vector desde cámara hacia el objetivo
            Vector3 cameraToTarget = (targetPosition - _camera.transform.position).normalized;
            
            // Producto punto con la dirección forward de la cámara
            float dotProduct = Vector3.Dot(_camera.transform.forward, cameraToTarget);
            
            // Convertir a score (0-1, donde 1 es centro de pantalla)
            return Mathf.Max(0f, dotProduct);
        }
        
        private TestEnemy FindNearestEnemy()
        {
            var enemies = FindObjectsByType<TestEnemy>(FindObjectsSortMode.None);
            TestEnemy nearest = null;
            float nearestDist = float.MaxValue;
            
            Debug.Log("[OfflinePlayerController] FindNearestEnemy() fallback - checking line of sight for all enemies");
            
            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive || enemy == _currentTarget) continue;
                
                // IMPORTANT: Also check line of sight in fallback
                if (!HasLineOfSight(enemy.transform.position))
                {
                    Debug.Log($"[OfflinePlayerController] FindNearestEnemy() skipping {enemy.name} - no line of sight");
                    continue;
                }
                
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = enemy;
                }
            }
            
            Debug.Log($"[OfflinePlayerController] FindNearestEnemy() result: {(nearest != null ? nearest.name : "null")}");
            return nearest;
        }
        
        private void SetTarget(TestEnemy enemy)
        {
            // Desuscribirse del target anterior
            if (_currentTarget != null)
            {
                _currentTarget.SetTargeted(false);
                _currentTarget.OnDeath -= HandleTargetDeath;
            }
            
            _currentTarget = enemy;
            
            // Suscribirse al nuevo target
            if (_currentTarget != null)
            {
                _currentTarget.SetTargeted(true);
                _currentTarget.OnDeath += HandleTargetDeath;
                Debug.Log($"[OfflinePlayer] Target set to: {_currentTarget.DisplayName}");
            }

            // Update target system
            if (_targetSystem != null)
            {
                _targetSystem.SelectTarget(_currentTarget);
            }
        }
        
        private void HandleTargetDeath(ITargetable deadTarget)
        {
            var deadEnemy = deadTarget as TestEnemy;
            if (deadEnemy == null) return;
            
            Debug.Log($"[OfflinePlayer] Target {deadEnemy.DisplayName} died, searching for auto-switch target");
            
            // Auto-Switch: Buscar nuevo target cerca del cadáver
            var newTarget = FindAutoSwitchTarget(deadEnemy.transform.position);
            
            if (newTarget != null)
            {
                SetTarget(newTarget);
                Debug.Log($"[OfflinePlayer] Auto-switched to: {newTarget.DisplayName}");
            }
            else
            {
                // Limpiar target si no hay nadie cerca
                SetTarget(null);
                Debug.Log("[OfflinePlayer] No auto-switch target found, clearing target");
            }
        }
        
        private TestEnemy FindAutoSwitchTarget(Vector3 deathPosition)
        {
            // Buscar enemigos en radio corto alrededor del cadáver
            Collider[] colliders = Physics.OverlapSphere(deathPosition, _autoSwitchRange, _enemyLayers);
            
            TestEnemy bestTarget = null;
            TestEnemy closestTarget = null;
            float closestDistance = float.MaxValue;
            
            foreach (var collider in colliders)
            {
                var enemy = collider.GetComponent<TestEnemy>();
                if (enemy == null || !enemy.IsAlive) continue;
                
                float distance = Vector3.Distance(deathPosition, enemy.transform.position);
                
                // Prioridad 1: Enemigo que ya me esté atacando
                if (enemy.CurrentTarget == transform)
                {
                    bestTarget = enemy;
                    break; // Máxima prioridad, no seguir buscando
                }
                
                // Prioridad 2: Enemigo más cercano
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = enemy;
                }
            }
            
            return bestTarget ?? closestTarget;
        }
        
        private void ClearTarget()
        {
            if (_currentTarget != null)
            {
                _currentTarget.SetTargeted(false);
                _currentTarget = null;
                Debug.Log("[OfflinePlayerController] Target cleared");
            }
        }
        
        private void HandleAbilities()
        {
            if (!_isAlive) return;

            // Check if we're moving (for cast interruption)
            bool isMoving = _moveInput.sqrMagnitude > 0.01f || _strafeInput != 0;
            
            // Interrupt casting if moving
            if (isMoving && _currentCastCoroutine != null)
            {
                Debug.Log("[OfflinePlayerController] Cast interrupted by movement");
                StopCoroutine(_currentCastCoroutine);
                _currentCastCoroutine = null;
                _currentCastName = null;
                _currentCastProgress = 0f;
                _isChanneling = false;
            }

            // Stop meditating if moving
            if (isMoving)
            {
                _isMeditating = false;
            }

            // Don't allow new abilities while casting
            if (_currentCastCoroutine != null)
            {
                return;
            }

            // Ability hotkeys (1-4) - simplified approach
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log("[OfflinePlayerController] Key 1 pressed - trying Basic Attack");
                AutoTargetIfNeeded();
                TrySimpleAbility("Basic Attack", 50f, 0f, 15f); // damage, castTime, range
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Debug.Log("[OfflinePlayerController] Key 2 pressed - trying Heavy Attack (Progressive Cast)");
                AutoTargetIfNeeded();
                TrySimpleAbility("Heavy Attack", 125f, 1.5f, 15f); // Progressive cast - 1.5s cast time
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Debug.Log("[OfflinePlayerController] Key 3 pressed - trying Heal");
                TrySimpleHeal(50f); // healing amount
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                Debug.Log("[OfflinePlayerController] Key 4 pressed - trying Drain Life (Channeling)");
                AutoTargetIfNeeded();
                TryChannelingAbility("Drain Life", 25f, 5f, 30f, 0.5f); // Channeling - 5s duration, 25 damage every 0.5s
            }
            
            // Legacy: 5 for meditate (keep this as is)
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                _isMeditating = true;
                Debug.Log("[OfflinePlayerController] Meditating... stay still to recover mana");
            }
            
            // Meditar: recuperar mana si está quieto
            if (_isMeditating && !isMoving)
            {
                _currentMana = Mathf.Min(_maxMana, _currentMana + _manaRegenPerSecond * Time.deltaTime);
            }
        }

        private void TrySimpleAbility(string abilityName, float damage, float castTime, float range)
        {
            // Check if already casting
            if (_currentCastCoroutine != null)
            {
                Debug.Log($"[OfflinePlayerController] {abilityName} failed: Already casting");
                return;
            }

            // Check if we have a target
            if (_currentTarget == null || !_currentTarget.IsAlive)
            {
                Debug.Log($"[OfflinePlayerController] {abilityName} failed: No valid target");
                return;
            }

            // Check range
            float distance = Vector3.Distance(transform.position, _currentTarget.transform.position);
            if (distance > range)
            {
                Debug.Log($"[OfflinePlayerController] {abilityName} failed: Out of range ({distance:F1}m > {range}m)");
                return;
            }

            // Check facing
            Vector3 dirToTarget = (_currentTarget.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToTarget);
            if (angle > 90f)
            {
                Debug.Log($"[OfflinePlayerController] {abilityName} failed: Target not in front");
                return;
            }

            Debug.Log($"[OfflinePlayerController] {abilityName} SUCCESS - Starting cast");

            // If instant cast
            if (castTime <= 0f)
            {
                ExecuteAbilityDamage(abilityName, damage);
            }
            else
            {
                // Start casting coroutine
                _currentCastCoroutine = StartCoroutine(CastAbility(abilityName, damage, castTime));
            }
        }

        private void TrySimpleHeal(float healAmount)
        {
            if (_currentHealth >= _maxHealth)
            {
                Debug.Log("[OfflinePlayerController] Heal failed: Already at full health");
                return;
            }

            Debug.Log("[OfflinePlayerController] Heal SUCCESS");
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + healAmount);
            FloatingCombatText.SpawnHeal(transform.position + Vector3.up * 1.5f, healAmount);
            Debug.Log($"[OfflinePlayerController] Healed for {healAmount}! HP: {_currentHealth}/{_maxHealth}");
        }

        private System.Collections.IEnumerator CastAbility(string abilityName, float damage, float castTime)
        {
            Debug.Log($"[OfflinePlayerController] Starting cast: {abilityName} ({castTime}s)");
            
            // Initialize cast bar variables - PROGRESSIVE CAST
            _currentCastName = abilityName;
            _currentCastDuration = castTime;
            _currentCastProgress = 0f;
            _isChanneling = false; // Progressive cast
            
            float elapsed = 0f;
            Vector3 startPos = transform.position;

            while (elapsed < castTime)
            {
                elapsed += Time.deltaTime;
                _currentCastProgress = elapsed / castTime;

                // Check for movement interruption
                if (Vector3.Distance(transform.position, startPos) > 0.1f)
                {
                    Debug.Log($"[OfflinePlayerController] {abilityName} INTERRUPTED by movement");
                    // Clear cast bar
                    _currentCastName = null;
                    _currentCastProgress = 0f;
                    yield break;
                }

                // Log progress every 25%
                int progressPercent = Mathf.RoundToInt(_currentCastProgress * 100);
                if (progressPercent % 25 == 0 && progressPercent > 0)
                {
                    Debug.Log($"[OfflinePlayerController] Casting {abilityName}: {progressPercent}%");
                }

                yield return null;
            }

            Debug.Log($"[OfflinePlayerController] {abilityName} cast COMPLETED");
            
            // Clear cast bar
            _currentCastName = null;
            _currentCastProgress = 0f;
            
            ExecuteAbilityDamage(abilityName, damage);
        }

        private void ExecuteAbilityDamage(string abilityName, float damage)
        {
            if (_currentTarget == null || !_currentTarget.IsAlive)
            {
                Debug.Log($"[OfflinePlayerController] {abilityName} execution failed: Target lost");
                return;
            }

            // Apply damage
            _currentTarget.TakeDamage(damage);
            
            // Spawn floating combat text
            FloatingCombatText.SpawnDamage(_currentTarget.transform.position + Vector3.up * 1.5f, damage, DamageType.Physical);
            
            Debug.Log($"[OfflinePlayerController] {abilityName} dealt {damage} damage to {_currentTarget.DisplayName}");

            // Check if target died
            if (!_currentTarget.IsAlive)
            {
                ClearTarget();
            }
        }
        
        private void TryChannelingAbility(string abilityName, float damagePerTick, float duration, float range, float tickInterval)
        {
            // Check if already casting
            if (_currentCastCoroutine != null)
            {
                Debug.Log($"[OfflinePlayerController] {abilityName} failed: Already casting");
                return;
            }

            // Check if we have a target
            if (_currentTarget == null || !_currentTarget.IsAlive)
            {
                Debug.Log($"[OfflinePlayerController] {abilityName} failed: No valid target");
                return;
            }

            // Check range
            float distance = Vector3.Distance(transform.position, _currentTarget.transform.position);
            if (distance > range)
            {
                Debug.Log($"[OfflinePlayerController] {abilityName} failed: Out of range ({distance:F1}m > {range}m)");
                return;
            }

            // Check facing
            Vector3 dirToTarget = (_currentTarget.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToTarget);
            if (angle > 90f)
            {
                Debug.Log($"[OfflinePlayerController] {abilityName} failed: Target not in front");
                return;
            }

            Debug.Log($"[OfflinePlayerController] {abilityName} SUCCESS - Starting channel");

            // Start channeling coroutine
            _currentCastCoroutine = StartCoroutine(ChannelAbility(abilityName, damagePerTick, duration, tickInterval));
        }

        private System.Collections.IEnumerator ChannelAbility(string abilityName, float damagePerTick, float duration, float tickInterval)
        {
            Debug.Log($"[OfflinePlayerController] Starting channel: {abilityName} ({duration}s, {damagePerTick} damage every {tickInterval}s)");
            
            // Initialize cast bar variables - CHANNELING
            _currentCastName = abilityName;
            _currentCastDuration = duration;
            _currentCastProgress = 0f;
            _isChanneling = true; // Channeling with countdown
            
            float elapsed = 0f;
            float nextTickTime = tickInterval;
            Vector3 startPos = transform.position;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _currentCastProgress = elapsed / duration;

                // Check for movement interruption
                if (Vector3.Distance(transform.position, startPos) > 0.1f)
                {
                    Debug.Log($"[OfflinePlayerController] {abilityName} INTERRUPTED by movement");
                    // Clear cast bar
                    _currentCastName = null;
                    _currentCastProgress = 0f;
                    yield break;
                }

                // Check if target is still valid
                if (_currentTarget == null || !_currentTarget.IsAlive)
                {
                    Debug.Log($"[OfflinePlayerController] {abilityName} INTERRUPTED - target lost");
                    _currentCastName = null;
                    _currentCastProgress = 0f;
                    yield break;
                }

                // Damage tick
                if (elapsed >= nextTickTime)
                {
                    ExecuteAbilityDamage($"{abilityName} (tick)", damagePerTick);
                    nextTickTime += tickInterval;
                    Debug.Log($"[OfflinePlayerController] {abilityName} tick! Next tick in {tickInterval}s");
                }

                // Log progress every 25%
                int progressPercent = Mathf.RoundToInt(_currentCastProgress * 100);
                if (progressPercent % 25 == 0 && progressPercent > 0)
                {
                    Debug.Log($"[OfflinePlayerController] Channeling {abilityName}: {progressPercent}%");
                }

                yield return null;
            }

            Debug.Log($"[OfflinePlayerController] {abilityName} channel COMPLETED");
            
            // Clear cast bar
            _currentCastName = null;
            _currentCastProgress = 0f;
        }
        
        private void AutoTargetIfNeeded()
        {
            if (_currentTarget != null && _currentTarget.IsAlive) return;
            
            // Find nearest enemy within 20m (ranged attack range)
            var enemies = FindObjectsByType<TestEnemy>(FindObjectsSortMode.None);
            TestEnemy nearest = null;
            float nearestDist = float.MaxValue;
            
            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < nearestDist && dist <= 20f) // 20m range for auto-target
                {
                    nearestDist = dist;
                    nearest = enemy;
                }
            }
            
            if (nearest != null)
            {
                SetTarget(nearest);
                Debug.Log($"[OfflinePlayerController] Auto-targeted: {nearest.DisplayName}");
            }
        }
        
        #region Combat State Event Handlers

        private void OnCombatStateChanged(CombatState previousState, CombatState newState)
        {
            Debug.Log($"[OfflinePlayerController] Combat state: {previousState} → {newState}");
        }

        private void OnCastProgress(float progress, float totalDuration)
        {
            // Log every 25% to avoid spam
            int progressPercent = Mathf.RoundToInt(progress * 100);
            if (progressPercent % 25 == 0)
            {
                Debug.Log($"[OfflinePlayerController] Casting: {progressPercent}%");
            }
        }

        private void OnCastCompleted(ScriptableObject ability)
        {
            Debug.Log($"[OfflinePlayerController] ✅ Cast completed");
        }

        private void OnCastInterrupted(ScriptableObject ability)
        {
            Debug.Log($"[OfflinePlayerController] ❌ Cast interrupted");
        }

        #endregion
        
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
            Debug.Log($"[OfflinePlayerController] Took {damage} damage! HP: {_currentHealth}/{_maxHealth}");
            
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
            Debug.Log("[OfflinePlayerController] YOU DIED!");
            
            if (_renderer != null)
                _renderer.material.color = Color.gray;
            
            OnDeath?.Invoke(this);
        }
        
        public void SetTargeted(bool targeted) { }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (_stateMachine != null)
            {
                _stateMachine.OnStateChanged -= OnCombatStateChanged;
                _stateMachine.OnCastProgress -= OnCastProgress;
                _stateMachine.OnCastCompleted -= OnCastCompleted;
                _stateMachine.OnCastInterrupted -= OnCastInterrupted;
            }
        }
        
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, Screen.height - 200, 400, 190));
            
            GUI.color = Color.white;
            GUILayout.Label($"HP: {_currentHealth:F0} / {_maxHealth:F0}");
            GUILayout.Label($"Mana: {_currentMana:F0} / {_maxMana:F0}");
            
            // Show combat state
            if (_stateMachine != null)
            {
                GUI.color = Color.cyan;
                GUILayout.Label($"State: {_stateMachine.CurrentState}");
                if (_stateMachine.CurrentState == CombatState.Casting)
                {
                    GUILayout.Label($"Cast: {_stateMachine.CastProgress:P0}");
                }
            }
            
            // Show cast bar (our simplified version)
            if (!string.IsNullOrEmpty(_currentCastName))
            {
                GUI.color = Color.yellow;
                GUILayout.Label($"Casting: {_currentCastName}");
                
                // Draw cast bar
                Rect castBarRect = GUILayoutUtility.GetRect(300, 20);
                GUI.color = Color.black;
                GUI.DrawTexture(castBarRect, Texture2D.whiteTexture);
                
                // Fill bar - Different behavior based on cast type
                float fillAmount;
                if (_isChanneling)
                {
                    // CHANNELING: Bar empties (100% to 0%)
                    fillAmount = 1f - _currentCastProgress;
                }
                else
                {
                    // PROGRESSIVE CAST: Bar fills (0% to 100%)
                    fillAmount = _currentCastProgress;
                }
                
                Rect fillRect = new Rect(castBarRect.x + 2, castBarRect.y + 2, 
                    (castBarRect.width - 4) * fillAmount, castBarRect.height - 4);
                GUI.color = Color.yellow;
                GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
                
                // Cast time text - Different display based on cast type
                GUI.color = Color.white;
                string timeText;
                
                if (_isChanneling)
                {
                    // CHANNELING: Show countdown (remaining time)
                    float remainingTime = _currentCastDuration - (_currentCastProgress * _currentCastDuration);
                    timeText = $"{remainingTime:F1}s";
                }
                else
                {
                    // PROGRESSIVE CAST: Show progress (elapsed / total)
                    float elapsedTime = _currentCastProgress * _currentCastDuration;
                    timeText = $"{elapsedTime:F1}s / {_currentCastDuration:F1}s";
                }
                
                GUI.Label(castBarRect, timeText, new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
            }
            
            if (_isMeditating && _moveInput.sqrMagnitude < 0.01f)
            {
                GUI.color = Color.cyan;
                GUILayout.Label("** MEDITATING **");
            }
            
            if (_currentTarget != null)
            {
                float dist = Vector3.Distance(transform.position, _currentTarget.transform.position);
                GUILayout.Space(5);
                GUI.color = Color.red;
                GUILayout.Label($"Target: {_currentTarget.DisplayName} ({dist:F1}m)");
                GUILayout.Label($"Target HP: {_currentTarget.CurrentHealth:F0} / {_currentTarget.MaxHealth:F0}");
            }
            
            GUILayout.Space(5);
            GUI.color = Color.yellow;
            GUILayout.Label("NEW ABILITY SYSTEM:");
            GUILayout.Label("[1] Basic Attack | [2] Heavy Attack");
            GUILayout.Label("[3] Heal | [4] Drain Life | [5] Meditate");
            
            GUILayout.EndArea();
        }
    }
}
