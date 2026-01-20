using UnityEngine;
using EtherDomes.Combat;
using EtherDomes.Combat.Abilities;
using EtherDomes.Combat.Visuals;
using EtherDomes.Data;

namespace EtherDomes.Testing
{
    /// <summary>
    /// Test player that uses the new ability system instead of hardcoded attacks.
    /// Replaces the old OfflinePlayerController for testing the new casting system.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(CombatStateMachine))]
    [RequireComponent(typeof(AbilitySystem))]
    public class TestPlayerWithAbilities : MonoBehaviour, ITargetable
    {
        [Header("Identity")]
        [SerializeField] private string _displayName = "Test Player";
        
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 180f;
        [SerializeField] private float _jumpHeight = 2f;
        [SerializeField] private float _gravity = -9.81f;

        [Header("Health")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _currentHealth = 100f;

        [Header("Targeting")]
        [SerializeField] private float _targetRange = 30f;
        [SerializeField] private LayerMask _enemyLayers = 1 << 8; // Enemy layer

        [Header("Abilities")]
        [SerializeField] private AbilityDefinition _basicAttack;
        [SerializeField] private AbilityDefinition _heavyAttack;
        [SerializeField] private AbilityDefinition _heal;
        [SerializeField] private AbilityDefinition _drainLife;

        // Components
        private CharacterController _controller;
        private CombatStateMachine _stateMachine;
        private AbilitySystem _abilitySystem;
        private TargetSystem _targetSystem;

        // State
        private Vector3 _velocity;
        private ITargetable _currentTarget;
        private bool _isAlive = true;

        // ITargetable implementation
        public string DisplayName => _displayName;
        public bool IsAlive => _isAlive;
        public Vector3 Position => transform.position;
        public Transform Transform => transform;
        public ulong NetworkId => 0; // Not networked for testing
        public TargetType Type => TargetType.Friendly;
        public float HealthPercent => _maxHealth > 0 ? _currentHealth / _maxHealth : 0f;
        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public int Level => 1; // Test player is level 1
        
        public event System.Action<ITargetable> OnDeath;

        public float GetThreatTo(ulong playerId)
        {
            // Test player has no threat toward other players
            return 0f;
        }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _stateMachine = GetComponent<CombatStateMachine>();
            _abilitySystem = GetComponent<AbilitySystem>();
            _targetSystem = GetComponent<TargetSystem>();

            if (_targetSystem == null)
                _targetSystem = gameObject.AddComponent<TargetSystem>();
        }

        private void Start()
        {
            // Initialize ability system
            if (_abilitySystem != null)
            {
                _abilitySystem.Initialize(_targetSystem);
                
                // Load abilities into slots
                if (_basicAttack != null) _abilitySystem.SetAbility(0, _basicAttack.ToAbilityData());
                if (_heavyAttack != null) _abilitySystem.SetAbility(1, _heavyAttack.ToAbilityData());
                if (_heal != null) _abilitySystem.SetAbility(2, _heal.ToAbilityData());
                if (_drainLife != null) _abilitySystem.SetAbility(3, _drainLife.ToAbilityData());
            }

            // Subscribe to events
            if (_stateMachine != null)
            {
                _stateMachine.OnStateChanged += OnCombatStateChanged;
                _stateMachine.OnCastProgress += OnCastProgress;
                _stateMachine.OnCastCompleted += OnCastCompleted;
                _stateMachine.OnCastInterrupted += OnCastInterrupted;
            }

            if (_abilitySystem != null)
            {
                _abilitySystem.OnAbilityExecuted += OnAbilityExecuted;
                _abilitySystem.OnAbilityError += OnAbilityError;
            }

            Debug.Log("[TestPlayerWithAbilities] Initialized with new ability system");
        }

        private void Update()
        {
            HandleMovement();
            HandleTargeting();
            HandleAbilities();
        }

        private void HandleMovement()
        {
            if (!_isAlive) return;

            // Get input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            // Movement
            Vector3 move = transform.right * horizontal + transform.forward * vertical;
            
            // Check if we're moving (for cast interruption)
            bool isMoving = move.magnitude > 0.1f;
            if (isMoving && _stateMachine != null && _stateMachine.CurrentState == CombatState.Casting)
            {
                _stateMachine.OnMovementDetected();
            }

            _controller.Move(move * _moveSpeed * Time.deltaTime);

            // Rotation (mouse look)
            if (Input.GetMouseButton(1)) // Right mouse button
            {
                float mouseX = Input.GetAxis("Mouse X");
                transform.Rotate(Vector3.up * mouseX * _rotationSpeed * Time.deltaTime);
            }

            // Jumping
            if (_controller.isGrounded)
            {
                if (Input.GetButtonDown("Jump"))
                {
                    _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
                }
                else
                {
                    _velocity.y = 0f;
                }
            }

            // Apply gravity
            _velocity.y += _gravity * Time.deltaTime;
            _controller.Move(_velocity * Time.deltaTime);
        }

        private void HandleTargeting()
        {
            if (!_isAlive) return;

            // Tab targeting
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                CycleTarget();
            }

            // Clear target
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClearTarget();
            }

            // Click targeting (left mouse button)
            if (Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1))
            {
                TryClickTarget();
            }
        }

        private void HandleAbilities()
        {
            if (!_isAlive || _abilitySystem == null) return;

            // Ability hotkeys (1-4)
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _abilitySystem.TryExecuteAbility(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _abilitySystem.TryExecuteAbility(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                _abilitySystem.TryExecuteAbility(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                _abilitySystem.TryExecuteAbility(3);
            }
        }

        private void CycleTarget()
        {
            var enemies = FindObjectsByType<TestEnemy>(FindObjectsSortMode.None);
            if (enemies.Length == 0) return;

            // Find current target index
            int currentIndex = -1;
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] == _currentTarget)
                {
                    currentIndex = i;
                    break;
                }
            }

            // Get next target
            int nextIndex = (currentIndex + 1) % enemies.Length;
            SetTarget(enemies[nextIndex]);
        }

        private void TryClickTarget()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, _targetRange, _enemyLayers))
            {
                var targetable = hit.collider.GetComponent<ITargetable>();
                if (targetable != null)
                {
                    SetTarget(targetable);
                }
            }
        }

        private void SetTarget(ITargetable target)
        {
            // Clear old target
            if (_currentTarget != null && _currentTarget is ITargetIndicator oldIndicator)
            {
                oldIndicator.SetTargeted(false);
            }

            _currentTarget = target;

            // Set new target
            if (_currentTarget != null && _currentTarget is ITargetIndicator newIndicator)
            {
                newIndicator.SetTargeted(true);
            }

            // Update target system
            if (_targetSystem != null)
            {
                _targetSystem.SelectTarget(_currentTarget);
            }

            Debug.Log($"[TestPlayerWithAbilities] Target set: {_currentTarget?.DisplayName ?? "None"}");
        }

        private void ClearTarget()
        {
            SetTarget(null);
        }

        #region Event Handlers

        private void OnCombatStateChanged(CombatState previousState, CombatState newState)
        {
            Debug.Log($"[TestPlayerWithAbilities] Combat state: {previousState} → {newState}");
        }

        private void OnCastProgress(float progress, float totalDuration)
        {
            // Log every 25% to avoid spam
            int progressPercent = Mathf.RoundToInt(progress * 100);
            if (progressPercent % 25 == 0)
            {
                Debug.Log($"[TestPlayerWithAbilities] Casting: {progressPercent}%");
            }
        }

        private void OnCastCompleted(ScriptableObject ability)
        {
            var abilityDef = ability as AbilityDefinition;
            Debug.Log($"[TestPlayerWithAbilities] ✅ Cast completed: {abilityDef?.AbilityName ?? ability?.name}");
        }

        private void OnCastInterrupted(ScriptableObject ability)
        {
            var abilityDef = ability as AbilityDefinition;
            Debug.Log($"[TestPlayerWithAbilities] ❌ Cast interrupted: {abilityDef?.AbilityName ?? ability?.name}");
        }

        private void OnAbilityExecuted(AbilityData ability, ITargetable target)
        {
            Debug.Log($"[TestPlayerWithAbilities] Executed: {ability.AbilityName} on {target?.DisplayName ?? "no target"}");
            
            // Apply damage/healing
            if (ability.BaseDamage > 0 && target is IDamageable damageable)
            {
                damageable.TakeDamage(ability.BaseDamage, 0);
                
                // Spawn floating combat text
                FloatingCombatText.SpawnDamage(target.Position + Vector3.up * 1.5f, ability.BaseDamage, ability.DamageType);
            }
            
            if (ability.BaseHealing > 0)
            {
                _currentHealth = Mathf.Min(_maxHealth, _currentHealth + ability.BaseHealing);
                FloatingCombatText.SpawnHeal(transform.position + Vector3.up * 1.5f, ability.BaseHealing);
                Debug.Log($"[TestPlayerWithAbilities] Healed for {ability.BaseHealing}! HP: {_currentHealth}/{_maxHealth}");
            }
        }

        private void OnAbilityError(string error)
        {
            Debug.LogWarning($"[TestPlayerWithAbilities] Ability error: {error}");
        }

        #endregion

        #region ITargetable Implementation

        public void TakeDamage(float damage, ulong attackerId)
        {
            if (!_isAlive) return;

            _currentHealth = Mathf.Max(0, _currentHealth - damage);
            Debug.Log($"[TestPlayerWithAbilities] Took {damage} damage! HP: {_currentHealth}/{_maxHealth}");

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            _isAlive = false;
            Debug.Log("[TestPlayerWithAbilities] YOU DIED!");
            
            // Fire death event
            OnDeath?.Invoke(this);
            
            // Disable movement
            _controller.enabled = false;
        }

        public void SetTargeted(bool targeted)
        {
            // Visual feedback for being targeted (optional)
        }

        #endregion

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

            if (_abilitySystem != null)
            {
                _abilitySystem.OnAbilityExecuted -= OnAbilityExecuted;
                _abilitySystem.OnAbilityError -= OnAbilityError;
            }
        }

        private void OnGUI()
        {
            if (!_isAlive) return;

            GUILayout.BeginArea(new Rect(Screen.width - 250, 10, 240, 150));
            GUILayout.BeginVertical("box");

            GUILayout.Label("Test Player (New Abilities)");
            GUILayout.Label($"HP: {_currentHealth:F0}/{_maxHealth:F0}");
            GUILayout.Label($"Target: {_currentTarget?.DisplayName ?? "None"}");
            
            if (_stateMachine != null)
            {
                GUILayout.Label($"State: {_stateMachine.CurrentState}");
                if (_stateMachine.CurrentState == CombatState.Casting)
                {
                    GUILayout.Label($"Cast: {_stateMachine.CastProgress:P0}");
                }
            }

            GUILayout.Space(5);
            GUILayout.Label("Controls:");
            GUILayout.Label("1-4: Abilities");
            GUILayout.Label("Tab: Cycle target");
            GUILayout.Label("Esc: Clear target");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}