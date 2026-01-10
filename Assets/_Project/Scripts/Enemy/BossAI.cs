using System;
using System.Collections.Generic;
using UnityEngine;

namespace EtherDomes.Enemy
{
    /// <summary>
    /// AI controller for boss enemies with mechanics.
    /// 
    /// Features:
    /// - Phase-based mechanics
    /// - Visual tells before attacks
    /// - Timed ability rotations
    /// 
    /// Requirements: 10.3
    /// </summary>
    public class BossAI : MonoBehaviour
    {
        [Header("Boss Settings")]
        [SerializeField] private string _bossId;
        [SerializeField] private float _baseHealth = 10000f;
        [SerializeField] private float _baseDamage = 50f;

        [Header("Mechanics")]
        [SerializeField] private List<BossMechanic> _mechanics = new();

        [Header("Visual Tells")]
        [SerializeField] private GameObject _groundAoEIndicatorPrefab;
        [SerializeField] private GameObject _cleaveIndicatorPrefab;
        [SerializeField] private Color _dangerColor = Color.red;

        private IEnemyAI _enemyAI;
        private float _currentHealth;
        private float _maxHealth;
        private int _currentPhase = 1;
        private readonly Dictionary<string, float> _mechanicCooldowns = new();
        private BossMechanic _currentMechanic;
        private float _mechanicTimer;
        private GameObject _currentIndicator;

        public event Action<int> OnPhaseChanged;
        public event Action<BossMechanic> OnMechanicStarted;
        public event Action<BossMechanic> OnMechanicExecuted;

        public string BossId => _bossId;
        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public int CurrentPhase => _currentPhase;
        public bool IsExecutingMechanic => _currentMechanic != null;

        private void Awake()
        {
            _enemyAI = GetComponent<IEnemyAI>();
        }

        public void Initialize(float healthMultiplier = 1f, float damageMultiplier = 1f)
        {
            _maxHealth = _baseHealth * healthMultiplier;
            _currentHealth = _maxHealth;

            // Initialize mechanic cooldowns
            foreach (var mechanic in _mechanics)
            {
                _mechanicCooldowns[mechanic.MechanicId] = 0f;
            }

            Debug.Log($"[BossAI] Initialized {_bossId} with {_maxHealth} HP");
        }

        /// <summary>
        /// Scale boss for group size.
        /// HP = BaseHealth * (1 + (N-1) * 0.2)
        /// </summary>
        public void ScaleForGroupSize(int playerCount)
        {
            float multiplier = 1f + (playerCount - 1) * 0.2f;
            _maxHealth = _baseHealth * multiplier;
            _currentHealth = _maxHealth;

            Debug.Log($"[BossAI] Scaled for {playerCount} players: {_maxHealth} HP (x{multiplier:F1})");
        }

        private void Update()
        {
            if (_enemyAI == null || _enemyAI.CurrentState != EnemyAIState.Combat)
                return;

            UpdateMechanics();
            UpdatePhase();
        }

        private void UpdateMechanics()
        {
            // Update cooldowns
            foreach (var mechanic in _mechanics)
            {
                if (_mechanicCooldowns.ContainsKey(mechanic.MechanicId))
                {
                    _mechanicCooldowns[mechanic.MechanicId] -= Time.deltaTime;
                }
            }

            // If executing a mechanic, update timer
            if (_currentMechanic != null)
            {
                _mechanicTimer -= Time.deltaTime;
                if (_mechanicTimer <= 0)
                {
                    ExecuteMechanic(_currentMechanic);
                    _currentMechanic = null;
                }
                return;
            }

            // Check for mechanics to start
            foreach (var mechanic in _mechanics)
            {
                if (!CanUseMechanic(mechanic))
                    continue;

                if (_mechanicCooldowns[mechanic.MechanicId] <= 0)
                {
                    StartMechanic(mechanic);
                    break;
                }
            }
        }

        private bool CanUseMechanic(BossMechanic mechanic)
        {
            // Check phase requirement
            if (mechanic.RequiredPhase > _currentPhase)
                return false;

            // Check health threshold
            float healthPercent = _currentHealth / _maxHealth;
            if (healthPercent > mechanic.HealthThreshold)
                return false;

            return true;
        }

        private void StartMechanic(BossMechanic mechanic)
        {
            _currentMechanic = mechanic;
            _mechanicTimer = mechanic.TellDuration;
            _mechanicCooldowns[mechanic.MechanicId] = mechanic.Cooldown;

            // Show visual tell
            ShowMechanicTell(mechanic);

            Debug.Log($"[BossAI] Starting mechanic: {mechanic.MechanicName} (Tell: {mechanic.TellDuration}s)");
            OnMechanicStarted?.Invoke(mechanic);
        }

        private void ShowMechanicTell(BossMechanic mechanic)
        {
            // Clean up previous indicator
            if (_currentIndicator != null)
            {
                Destroy(_currentIndicator);
            }

            switch (mechanic.Type)
            {
                case MechanicType.GroundAoE:
                    if (_groundAoEIndicatorPrefab != null)
                    {
                        _currentIndicator = Instantiate(_groundAoEIndicatorPrefab, 
                            mechanic.TargetPosition != Vector3.zero ? mechanic.TargetPosition : transform.position,
                            Quaternion.identity);
                        _currentIndicator.transform.localScale = Vector3.one * mechanic.Radius * 2f;
                    }
                    break;

                case MechanicType.FrontalCleave:
                    if (_cleaveIndicatorPrefab != null)
                    {
                        _currentIndicator = Instantiate(_cleaveIndicatorPrefab, 
                            transform.position, transform.rotation);
                        // Scale for cone angle
                        _currentIndicator.transform.localScale = new Vector3(
                            mechanic.Radius, 1f, mechanic.Radius);
                    }
                    break;
            }
        }

        private void ExecuteMechanic(BossMechanic mechanic)
        {
            // Clean up indicator
            if (_currentIndicator != null)
            {
                Destroy(_currentIndicator);
                _currentIndicator = null;
            }

            Debug.Log($"[BossAI] Executing mechanic: {mechanic.MechanicName}");
            OnMechanicExecuted?.Invoke(mechanic);

            // In a real implementation, this would deal damage to players in the area
            // For now, we just log the execution
        }

        private void UpdatePhase()
        {
            float healthPercent = _currentHealth / _maxHealth;
            int newPhase = 1;

            if (healthPercent <= 0.25f)
                newPhase = 4;
            else if (healthPercent <= 0.5f)
                newPhase = 3;
            else if (healthPercent <= 0.75f)
                newPhase = 2;

            if (newPhase != _currentPhase)
            {
                _currentPhase = newPhase;
                Debug.Log($"[BossAI] Phase changed to {_currentPhase}");
                OnPhaseChanged?.Invoke(_currentPhase);
            }
        }

        public void TakeDamage(float damage)
        {
            _currentHealth = Mathf.Max(0, _currentHealth - damage);
            
            if (_currentHealth <= 0)
            {
                OnDeath();
            }
        }

        private void OnDeath()
        {
            // Clean up
            if (_currentIndicator != null)
            {
                Destroy(_currentIndicator);
            }

            Debug.Log($"[BossAI] {_bossId} defeated!");
        }

        /// <summary>
        /// Add a mechanic to the boss.
        /// </summary>
        public void AddMechanic(BossMechanic mechanic)
        {
            _mechanics.Add(mechanic);
            _mechanicCooldowns[mechanic.MechanicId] = mechanic.InitialDelay;
        }

        /// <summary>
        /// Get all mechanics for this boss.
        /// </summary>
        public List<BossMechanic> GetMechanics()
        {
            return new List<BossMechanic>(_mechanics);
        }

        /// <summary>
        /// Calculate HP scaling multiplier for group size.
        /// </summary>
        public static float CalculateHPScaling(int playerCount)
        {
            return 1f + (playerCount - 1) * 0.2f;
        }
    }

    /// <summary>
    /// Definition of a boss mechanic.
    /// </summary>
    [Serializable]
    public class BossMechanic
    {
        public string MechanicId;
        public string MechanicName;
        public MechanicType Type;
        public float Damage;
        public float Radius;
        public float TellDuration;      // Time to show warning before execution
        public float Cooldown;          // Time between uses
        public float InitialDelay;      // Delay before first use
        public int RequiredPhase;       // Minimum phase to use this mechanic
        public float HealthThreshold;   // Health % below which this activates (1.0 = always)
        public Vector3 TargetPosition;  // For targeted AoE
    }

    /// <summary>
    /// Types of boss mechanics.
    /// </summary>
    public enum MechanicType
    {
        GroundAoE,      // Circle on ground
        FrontalCleave,  // Cone in front
        Charge,         // Rush to target
        Summon,         // Spawn adds
        Enrage,         // Damage buff
        Shield,         // Damage reduction
        Heal            // Self heal
    }
}
