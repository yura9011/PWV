using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using EtherDomes.Combat;

namespace EtherDomes.Enemy
{
    /// <summary>
    /// AI state machine for enemy entities.
    /// States: Idle, Patrol, Aggro, Combat, Return, Dead
    /// Server-authoritative - only runs on server.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class EnemyAI : NetworkBehaviour
    {
        #region Constants
        
        public const float DEFAULT_AGGRO_RANGE = 10f;
        public const float DEFAULT_LEASH_RANGE = 40f;
        public const float DEFAULT_ATTACK_RANGE = 2f;
        public const float DEFAULT_ATTACK_COOLDOWN = 2f;
        public const float DEFAULT_COMBAT_TIMEOUT = 5f;
        public const float DEFAULT_MOVE_SPEED = 3.5f;
        
        #endregion

        #region Serialized Fields
        
        [Header("Detection")]
        [SerializeField] private float _aggroRange = DEFAULT_AGGRO_RANGE;
        [SerializeField] private LayerMask _playerLayer;
        
        [Header("Combat")]
        [SerializeField] private float _attackRange = DEFAULT_ATTACK_RANGE;
        [SerializeField] private float _attackCooldown = DEFAULT_ATTACK_COOLDOWN;
        [SerializeField] private float _attackDamage = 10f;
        
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = DEFAULT_MOVE_SPEED;
        [SerializeField] private float _leashRange = DEFAULT_LEASH_RANGE;
        
        [Header("Timings")]
        [SerializeField] private float _combatTimeout = DEFAULT_COMBAT_TIMEOUT;
        
        #endregion

        #region State
        
        public enum AIState
        {
            Idle,
            Patrol,
            Aggro,
            Combat,
            Return,
            Dead
        }
        
        private NetworkVariable<AIState> _currentState = new NetworkVariable<AIState>(AIState.Idle);
        private NetworkVariable<ulong> _currentTargetNetId = new NetworkVariable<ulong>(0);
        
        private Enemy _enemy;
        private AggroSystem _aggroSystem;
        private CombatSystem _combatSystem;
        
        private Vector3 _spawnPosition;
        private float _lastAttackTime;
        private float _lastCombatTime;
        private Transform _currentTarget;
        
        // Cached players in range
        private readonly List<Transform> _playersInRange = new();
        private readonly Collider[] _overlapResults = new Collider[10];
        
        #endregion

        #region Properties
        
        public AIState CurrentState => _currentState.Value;
        public float AggroRange => _aggroRange;
        public float AttackRange => _attackRange;
        public bool IsInCombat => _currentState.Value == AIState.Combat || _currentState.Value == AIState.Aggro;
        public Transform CurrentTarget => _currentTarget;
        
        #endregion

        #region Unity Lifecycle
        
        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
        }
        
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                _spawnPosition = transform.position;
                _lastAttackTime = -_attackCooldown;
                
                // Find systems
                _aggroSystem = FindFirstObjectByType<AggroSystem>();
                _combatSystem = FindFirstObjectByType<CombatSystem>();
                
                // Register this enemy with combat system
                if (_combatSystem != null)
                {
                    _combatSystem.RegisterEnemy(NetworkObjectId, _enemy.MaxHealth);
                }
                
                // Subscribe to aggro changes
                if (_aggroSystem != null)
                {
                    _aggroSystem.OnAggroChanged += OnAggroChanged;
                }
                
                // [FIX] Ensure Player Layer is set
                if (_playerLayer.value == 0)
                {
                    int layerIndex = LayerMask.NameToLayer("Player");
                    if (layerIndex != -1)
                    {
                        _playerLayer = 1 << layerIndex;
                        Debug.Log($"[EnemyAI] Auto-assigned Player Layer Mask: {_playerLayer.value} (Index {layerIndex})");
                    }
                    else
                    {
                        Debug.LogError("[EnemyAI] 'Player' Layer not found in Project Settings!");
                    }
                }
                else
                {
                    Debug.Log($"[EnemyAI] Using configured Player Layer Mask: {_playerLayer.value}");
                }
                
                Debug.Log($"[EnemyAI] Initialized: {_enemy.DisplayName} at {_spawnPosition}");
            }
            
            _currentState.OnValueChanged += OnStateChanged;
        }
        
        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                if (_aggroSystem != null)
                {
                    _aggroSystem.OnAggroChanged -= OnAggroChanged;
                }
            }
            _currentState.OnValueChanged -= OnStateChanged;
        }
        
        private void Update()
        {
            if (!IsServer) return;

            if (!_enemy.IsAlive)
            {
                if (_currentState.Value != AIState.Dead)
                {
                    SetState(AIState.Dead);
                }
                return;
            }
            
            UpdateStateMachine();
        }
        
        #endregion

        #region State Machine
        
        private void UpdateStateMachine()
        {
            switch (_currentState.Value)
            {
                case AIState.Idle:
                    UpdateIdle();
                    break;
                case AIState.Patrol:
                    UpdatePatrol();
                    break;
                case AIState.Aggro:
                    UpdateAggro();
                    break;
                case AIState.Combat:
                    UpdateCombat();
                    break;
                case AIState.Return:
                    UpdateReturn();
                    break;
                case AIState.Dead:
                    // Do nothing
                    break;
            }
        }
        
        private void SetState(AIState newState)
        {
            if (_currentState.Value == newState) return;
            
            var oldState = _currentState.Value;
            _currentState.Value = newState;
            
             Debug.Log($"[EnemyAI] {_enemy.DisplayName}: {oldState} -> {newState}");
            
            // State enter actions
            switch (newState)
            {
                case AIState.Return:
                    _currentTarget = null;
                    _currentTargetNetId.Value = 0;
                    _aggroSystem?.ResetThreat(NetworkObjectId);
                    break;
                case AIState.Dead:
                    _currentTarget = null;
                    _currentTargetNetId.Value = 0;
                    break;
            }
        }
        
        #endregion

        #region State Updates
        
        private void UpdateIdle()
        {
            if (DetectPlayersInRange())
            {
                SetState(AIState.Aggro);
            }
        }
        
        private void UpdatePatrol()
        {
            if (DetectPlayersInRange())
            {
                SetState(AIState.Aggro);
            }
        }
        
        private void UpdateAggro()
        {
            UpdateTarget();
            
            if (_currentTarget == null)
            {
                SetState(AIState.Idle);
                return;
            }
            
            float distanceToTarget = Vector3.Distance(transform.position, _currentTarget.position);
            float distanceFromSpawn = Vector3.Distance(transform.position, _spawnPosition);
            
            if (distanceFromSpawn > _leashRange)
            {
                SetState(AIState.Return);
                return;
            }
            
            if (distanceToTarget > _attackRange)
            {
                MoveTowards(_currentTarget.position);
            }
            else
            {
                SetState(AIState.Combat);
            }
        }
        
        private void UpdateCombat()
        {
            UpdateTarget();
            
            if (_currentTarget == null)
            {
                if (Time.time - _lastCombatTime > _combatTimeout)
                {
                    SetState(AIState.Return);
                }
                return;
            }
            
            _lastCombatTime = Time.time;
            
            float distanceToTarget = Vector3.Distance(transform.position, _currentTarget.position);
            float distanceFromSpawn = Vector3.Distance(transform.position, _spawnPosition);
            
            if (distanceFromSpawn > _leashRange)
            {
                SetState(AIState.Return);
                return;
            }
            
            if (distanceToTarget > _attackRange * 1.2f)
            {
                SetState(AIState.Aggro);
                return;
            }
            
            FaceTarget(_currentTarget.position);
            
            if (Time.time - _lastAttackTime >= _attackCooldown)
            {
                PerformAttack();
            }
        }
        
        private void UpdateReturn()
        {
            float distanceToSpawn = Vector3.Distance(transform.position, _spawnPosition);
            
            if (distanceToSpawn < 0.5f)
            {
                transform.position = _spawnPosition;
                _enemy.Reset();
                SetState(AIState.Idle);
                return;
            }
            
            if (DetectPlayersInRange())
            {
                SetState(AIState.Aggro);
                // Note: Logic allows re-aggro during return if close
                return;
            }
            
            MoveTowards(_spawnPosition);
        }
        
        #endregion

        #region Detection & Targeting
        
        private bool DetectPlayersInRange()
        {
            _playersInRange.Clear();
            
            int count = Physics.OverlapSphereNonAlloc(
                transform.position, 
                _aggroRange, 
                _overlapResults, 
                _playerLayer);
            
            // [DEBUG] Log detection if we suspect issues
            if (Time.frameCount % 120 == 0) // Log every 2 seconds roughly
            {
                 Debug.Log($"[EnemyAI] Scanning for players... Radius: {_aggroRange}, Mask: {_playerLayer.value}. Hits: {count}");
            }

            for (int i = 0; i < count; i++)
            {
                var netIdentity = _overlapResults[i].GetComponent<NetworkObject>();
                if (netIdentity != null)
                {
                    _playersInRange.Add(_overlapResults[i].transform);
                    Debug.Log($"[EnemyAI] DETECTED PLAYER: {_overlapResults[i].name}");
                }
                else
                {
                    Debug.LogWarning($"[EnemyAI] Hit object {_overlapResults[i].name} on Player layer but no NetworkObject!");
                }
            }
            
            return _playersInRange.Count > 0;
        }
        
        private void UpdateTarget()
        {
            if (_aggroSystem != null)
            {
                // Aggro system uses ulong (ClientId/NetworkId)
                ulong highestThreatPlayer = _aggroSystem.GetCurrentTarget(NetworkObjectId);
                if (highestThreatPlayer != 0)
                {
                    var player = FindPlayerByNetId(highestThreatPlayer);
                    if (player != null && player.gameObject.activeInHierarchy)
                    {
                        SetTarget(player);
                        return;
                    }
                }
            }
            
            if (_playersInRange.Count > 0 || DetectPlayersInRange())
            {
                Transform nearest = GetNearestPlayer();
                if (nearest != null)
                {
                    SetTarget(nearest);
                    return;
                }
            }
            
            _currentTarget = null;
            _currentTargetNetId.Value = 0;
        }
        
        private void SetTarget(Transform target)
        {
            if (_currentTarget == target) return;
            
            _currentTarget = target;
            var netIdentity = target?.GetComponent<NetworkObject>();
            _currentTargetNetId.Value = netIdentity != null ? netIdentity.NetworkObjectId : 0;
            
            // Debug.Log($"[EnemyAI] {_enemy.DisplayName} targeting: {target?.name ?? "none"}");
        }
        
        private Transform GetNearestPlayer()
        {
            Transform nearest = null;
            float nearestDistance = float.MaxValue;
            
            foreach (var player in _playersInRange)
            {
                if (player == null || !player.gameObject.activeInHierarchy) continue;
                
                float distance = Vector3.Distance(transform.position, player.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = player;
                }
            }
            
            return nearest;
        }
        
        private Transform FindPlayerByNetId(ulong netId)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netId, out NetworkObject obj))
            {
                return obj.transform;
            }
            return null;
        }
        
        #endregion

        #region Movement
        
        private void MoveTowards(Vector3 target)
        {
            Vector3 direction = (target - transform.position).normalized;
            direction.y = 0;
            
            transform.position += direction * _moveSpeed * Time.deltaTime;
            FaceTarget(target);
        }
        
        private void FaceTarget(Vector3 target)
        {
            Vector3 direction = (target - transform.position).normalized;
            direction.y = 0;
            
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
        
        #endregion

        #region Combat
        
        private void PerformAttack()
        {
            if (_currentTarget == null) return;
            
            _lastAttackTime = Time.time;
            _lastCombatTime = Time.time;
            
            ulong targetNetId = 0;
             var netObj = _currentTarget.GetComponent<NetworkObject>();
             if (netObj != null) targetNetId = netObj.NetworkObjectId;

            if (targetNetId != 0 && _combatSystem != null)
            {
                _combatSystem.ApplyDamage(
                    targetNetId, 
                    _attackDamage, 
                    Data.DamageType.Physical, 
                    NetworkObjectId);
            }
            
            Debug.Log($"[EnemyAI] {_enemy.DisplayName} attacks for {_attackDamage} damage");
            AttackClientRpc();
        }
        
        [ClientRpc]
        private void AttackClientRpc()
        {
            // Attack VFX/SFX placeholder
        }
        
        #endregion

        #region Event Handlers
        
        private void OnAggroChanged(ulong enemyId, ulong newTargetPlayerId)
        {
            if (enemyId != NetworkObjectId) return;
            // Server check redundant as we update only on server, but safe
            if (!IsServer) return;
            
            var player = FindPlayerByNetId(newTargetPlayerId);
            if (player != null)
            {
                SetTarget(player);
            }
        }
        
        private void OnStateChanged(AIState oldState, AIState newState)
        {
            // Client-side visual reactions to state change (animation trigger etc)
        }
        
        #endregion

        #region Public API
        
        public void EnterCombat(Transform target)
        {
            if (!IsServer) return;
            if (!_enemy.IsAlive || _currentState.Value == AIState.Dead) return;
            SetTarget(target);
            SetState(AIState.Aggro);
        }
        
        public void ForceReturn()
        {
            if (!IsServer) return;
            if (_currentState.Value == AIState.Dead) return;
            SetState(AIState.Return);
        }
        
        public void ResetAI()
        {
            if (!IsServer) return;
            _currentTarget = null;
            _currentTargetNetId.Value = 0;
            transform.position = _spawnPosition;
            SetState(AIState.Idle);
        }
        
        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _aggroRange);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRange);
            
            Gizmos.color = Color.blue;
            Vector3 spawnPos = Application.isPlaying ? _spawnPosition : transform.position;
            Gizmos.DrawWireSphere(spawnPos, _leashRange);
            
            if (_currentTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, _currentTarget.position);
            }
        }
#endif
    }
}
