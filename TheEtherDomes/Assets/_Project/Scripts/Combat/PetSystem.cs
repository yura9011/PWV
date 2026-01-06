using System;
using System.Collections.Generic;
using System.Linq;
using EtherDomes.Data;
using UnityEngine;
using UnityEngine.AI;

namespace EtherDomes.Combat
{
    /// <summary>
    /// System for managing pets including summoning, commands, and AI behavior.
    /// Handles pet following, attacking, teleportation, and death/resummon cooldowns.
    /// </summary>
    public class PetSystem : MonoBehaviour, IPetSystem
    {
        #region Constants
        
        public const float DEFAULT_FOLLOW_DISTANCE = 3f;
        public const float DEFAULT_TELEPORT_DISTANCE = 40f;
        public const float DEFAULT_RESUMMON_COOLDOWN = 10f;
        public const float FOLLOW_DISTANCE_TOLERANCE = 0.5f;

        public float FollowDistance => DEFAULT_FOLLOW_DISTANCE;
        public float TeleportDistance => DEFAULT_TELEPORT_DISTANCE;
        public float ResummonCooldown => DEFAULT_RESUMMON_COOLDOWN;
        
        #endregion

        #region Private Fields
        
        // Active pets by owner ID
        private readonly Dictionary<ulong, PetInstance> _activePets = new Dictionary<ulong, PetInstance>();
        
        // Resummon cooldowns by owner ID (time when cooldown expires)
        private readonly Dictionary<ulong, float> _resummonCooldowns = new Dictionary<ulong, float>();
        
        // Owner positions for following (in real implementation, would query from entity system)
        private readonly Dictionary<ulong, Vector3> _ownerPositions = new Dictionary<ulong, Vector3>();
        
        // Owner targets for auto-attack (in real implementation, would query from target system)
        private readonly Dictionary<ulong, ulong> _ownerTargets = new Dictionary<ulong, ulong>();
        
        // Target positions for attacking
        private readonly Dictionary<ulong, Vector3> _targetPositions = new Dictionary<ulong, Vector3>();
        
        // Next pet entity ID
        private ulong _nextPetEntityId = 1000000;
        
        // Reference to combat system for damage
        private ICombatSystem _combatSystem;
        
        #endregion

        #region Events
        
        public event Action<ulong, PetInstance> OnPetSummoned;
        public event Action<ulong> OnPetDismissed;
        public event Action<ulong> OnPetDied;
        public event Action<ulong, ulong, float> OnPetDamageDealt;
        public event Action<ulong, PetState> OnPetStateChanged;
        
        #endregion

        #region Initialization
        
        /// <summary>
        /// Initialize the pet system with required dependencies.
        /// </summary>
        public void Initialize(ICombatSystem combatSystem)
        {
            _combatSystem = combatSystem;
        }
        
        #endregion

        #region Unity Lifecycle
        
        private void Update()
        {
            float currentTime = Time.time;
            float deltaTime = Time.deltaTime;
            
            UpdateAllPets(currentTime, deltaTime);
        }
        
        #endregion

        #region Core Operations
        
        public bool SummonPet(ulong ownerId, PetData petData)
        {
            if (petData == null)
            {
                Debug.LogWarning("[PetSystem] Attempted to summon null pet data");
                return false;
            }

            // Check resummon cooldown
            if (!CanSummonPet(ownerId))
            {
                float remaining = GetResummonCooldown(ownerId);
                Debug.Log($"[PetSystem] Cannot summon pet for {ownerId}, cooldown remaining: {remaining:F1}s");
                return false;
            }

            // Dismiss existing pet if any
            if (HasPet(ownerId))
            {
                DismissPet(ownerId);
            }

            // Generate pet entity ID
            ulong petEntityId = _nextPetEntityId++;

            // Create pet instance
            var petInstance = new PetInstance(petData, ownerId, petEntityId, Time.time);

            // Spawn pet GameObject if prefab exists
            if (petData.PetPrefab != null)
            {
                Vector3 spawnPosition = GetOwnerPosition(ownerId) + Vector3.right * FollowDistance;
                petInstance.PetGameObject = Instantiate(petData.PetPrefab, spawnPosition, Quaternion.identity);
                
                // Setup NavMeshAgent if present
                var navAgent = petInstance.PetGameObject.GetComponent<NavMeshAgent>();
                if (navAgent != null)
                {
                    navAgent.speed = petData.MoveSpeed;
                    navAgent.stoppingDistance = FollowDistance - FOLLOW_DISTANCE_TOLERANCE;
                }
            }

            // Register pet
            _activePets[ownerId] = petInstance;

            Debug.Log($"[PetSystem] Summoned pet '{petData.DisplayName}' for owner {ownerId} (PetEntityId: {petEntityId})");
            OnPetSummoned?.Invoke(ownerId, petInstance);

            return true;
        }

        public void DismissPet(ulong ownerId)
        {
            if (!_activePets.TryGetValue(ownerId, out var pet))
            {
                return;
            }

            // Destroy GameObject
            if (pet.PetGameObject != null)
            {
                Destroy(pet.PetGameObject);
            }

            // Update state
            pet.State = PetState.Dismissed;

            // Remove from active pets
            _activePets.Remove(ownerId);

            Debug.Log($"[PetSystem] Dismissed pet for owner {ownerId}");
            OnPetDismissed?.Invoke(ownerId);
        }

        public void CommandAttack(ulong ownerId, ulong targetId)
        {
            if (!_activePets.TryGetValue(ownerId, out var pet))
            {
                Debug.LogWarning($"[PetSystem] No pet found for owner {ownerId}");
                return;
            }

            if (!pet.IsAlive)
            {
                Debug.LogWarning($"[PetSystem] Pet for owner {ownerId} is not alive");
                return;
            }

            var previousState = pet.State;
            pet.SetTarget(targetId);

            Debug.Log($"[PetSystem] Pet for owner {ownerId} commanded to attack target {targetId}");
            
            if (previousState != pet.State)
            {
                OnPetStateChanged?.Invoke(ownerId, pet.State);
            }
        }

        public void CommandFollow(ulong ownerId)
        {
            if (!_activePets.TryGetValue(ownerId, out var pet))
            {
                Debug.LogWarning($"[PetSystem] No pet found for owner {ownerId}");
                return;
            }

            if (!pet.IsAlive)
            {
                Debug.LogWarning($"[PetSystem] Pet for owner {ownerId} is not alive");
                return;
            }

            var previousState = pet.State;
            pet.ClearTarget();

            Debug.Log($"[PetSystem] Pet for owner {ownerId} commanded to follow");
            
            if (previousState != pet.State)
            {
                OnPetStateChanged?.Invoke(ownerId, pet.State);
            }
        }
        
        #endregion

        #region Queries
        
        public bool HasPet(ulong ownerId)
        {
            return _activePets.ContainsKey(ownerId) && _activePets[ownerId].IsAlive;
        }

        public PetInstance GetPet(ulong ownerId)
        {
            _activePets.TryGetValue(ownerId, out var pet);
            return pet;
        }

        public float GetPetHealth(ulong ownerId)
        {
            if (_activePets.TryGetValue(ownerId, out var pet))
            {
                return pet.CurrentHealth;
            }
            return 0f;
        }

        public float GetPetMaxHealth(ulong ownerId)
        {
            if (_activePets.TryGetValue(ownerId, out var pet))
            {
                return pet.MaxHealth;
            }
            return 0f;
        }

        public bool IsPetAlive(ulong ownerId)
        {
            if (_activePets.TryGetValue(ownerId, out var pet))
            {
                return pet.IsAlive;
            }
            return false;
        }

        public float GetResummonCooldown(ulong ownerId)
        {
            if (_resummonCooldowns.TryGetValue(ownerId, out float cooldownExpireTime))
            {
                float remaining = cooldownExpireTime - Time.time;
                return Mathf.Max(0f, remaining);
            }
            return 0f;
        }

        public bool CanSummonPet(ulong ownerId)
        {
            return GetResummonCooldown(ownerId) <= 0f;
        }

        public IReadOnlyList<PetInstance> GetAllActivePets()
        {
            return _activePets.Values.Where(p => p.IsAlive).ToList().AsReadOnly();
        }
        
        #endregion

        #region External Updates
        
        /// <summary>
        /// Update owner position for pet following behavior.
        /// </summary>
        public void UpdateOwnerPosition(ulong ownerId, Vector3 position)
        {
            _ownerPositions[ownerId] = position;
        }

        /// <summary>
        /// Update owner's current target for auto-attack.
        /// </summary>
        public void UpdateOwnerTarget(ulong ownerId, ulong targetId)
        {
            _ownerTargets[ownerId] = targetId;
        }

        /// <summary>
        /// Update a target's position for pet attacking.
        /// </summary>
        public void UpdateTargetPosition(ulong targetId, Vector3 position)
        {
            _targetPositions[targetId] = position;
        }

        /// <summary>
        /// Apply damage to a pet.
        /// </summary>
        public void ApplyDamageToPet(ulong ownerId, float damage)
        {
            if (!_activePets.TryGetValue(ownerId, out var pet))
            {
                return;
            }

            if (pet.TakeDamage(damage))
            {
                HandlePetDeath(ownerId, pet);
            }
        }
        
        #endregion

        #region Private Methods
        
        private Vector3 GetOwnerPosition(ulong ownerId)
        {
            if (_ownerPositions.TryGetValue(ownerId, out var position))
            {
                return position;
            }
            return Vector3.zero;
        }

        private Vector3 GetTargetPosition(ulong targetId)
        {
            if (_targetPositions.TryGetValue(targetId, out var position))
            {
                return position;
            }
            return Vector3.zero;
        }

        private void UpdateAllPets(float currentTime, float deltaTime)
        {
            foreach (var kvp in _activePets.ToList())
            {
                ulong ownerId = kvp.Key;
                var pet = kvp.Value;

                if (!pet.IsAlive) continue;

                // Auto-target owner's target if pet has no target
                if (!pet.HasTarget && _ownerTargets.TryGetValue(ownerId, out ulong ownerTarget) && ownerTarget != 0)
                {
                    var previousState = pet.State;
                    pet.SetTarget(ownerTarget);
                    if (previousState != pet.State)
                    {
                        OnPetStateChanged?.Invoke(ownerId, pet.State);
                    }
                }

                // Update pet behavior based on state
                switch (pet.State)
                {
                    case PetState.Following:
                        UpdateFollowingBehavior(ownerId, pet);
                        break;
                    case PetState.Attacking:
                        UpdateAttackingBehavior(ownerId, pet, currentTime);
                        break;
                }

                // Check for teleport
                CheckTeleport(ownerId, pet);
            }
        }

        private void UpdateFollowingBehavior(ulong ownerId, PetInstance pet)
        {
            if (pet.PetGameObject == null) return;

            Vector3 ownerPos = GetOwnerPosition(ownerId);
            Vector3 petPos = pet.PetGameObject.transform.position;
            float distance = Vector3.Distance(ownerPos, petPos);

            // Only move if outside follow distance tolerance
            if (distance > FollowDistance + FOLLOW_DISTANCE_TOLERANCE)
            {
                var navAgent = pet.PetGameObject.GetComponent<NavMeshAgent>();
                if (navAgent != null && navAgent.isOnNavMesh)
                {
                    // Calculate position at follow distance from owner
                    Vector3 direction = (petPos - ownerPos).normalized;
                    if (direction == Vector3.zero) direction = Vector3.back;
                    Vector3 targetPos = ownerPos + direction * FollowDistance;
                    navAgent.SetDestination(targetPos);
                }
            }
        }

        private void UpdateAttackingBehavior(ulong ownerId, PetInstance pet, float currentTime)
        {
            if (!pet.HasTarget)
            {
                var previousState = pet.State;
                pet.ClearTarget();
                if (previousState != pet.State)
                {
                    OnPetStateChanged?.Invoke(ownerId, pet.State);
                }
                return;
            }

            Vector3 targetPos = GetTargetPosition(pet.CurrentTargetId);
            
            if (pet.PetGameObject != null)
            {
                Vector3 petPos = pet.PetGameObject.transform.position;
                float distance = Vector3.Distance(targetPos, petPos);

                var navAgent = pet.PetGameObject.GetComponent<NavMeshAgent>();
                
                // Move towards target if out of attack range
                if (distance > pet.Data.AttackRange)
                {
                    if (navAgent != null && navAgent.isOnNavMesh)
                    {
                        navAgent.SetDestination(targetPos);
                    }
                }
                else
                {
                    // Stop moving and attack
                    if (navAgent != null)
                    {
                        navAgent.ResetPath();
                    }

                    // Attack if cooldown is ready
                    if (pet.CanAttack(currentTime))
                    {
                        PerformAttack(ownerId, pet, currentTime);
                    }
                }
            }
            else
            {
                // No GameObject, just perform attack logic
                if (pet.CanAttack(currentTime))
                {
                    PerformAttack(ownerId, pet, currentTime);
                }
            }
        }

        private void PerformAttack(ulong ownerId, PetInstance pet, float currentTime)
        {
            float damage = pet.Data.BaseDamage;
            ulong targetId = pet.CurrentTargetId;

            // Apply damage through combat system
            _combatSystem?.ApplyDamage(targetId, damage, DamageType.Physical, pet.PetEntityId);

            // Record attack time
            pet.RecordAttack(currentTime);

            Debug.Log($"[PetSystem] Pet for owner {ownerId} attacked target {targetId} for {damage:F0} damage");
            OnPetDamageDealt?.Invoke(ownerId, targetId, damage);
        }

        private void CheckTeleport(ulong ownerId, PetInstance pet)
        {
            if (pet.PetGameObject == null) return;

            Vector3 ownerPos = GetOwnerPosition(ownerId);
            Vector3 petPos = pet.PetGameObject.transform.position;
            float distance = Vector3.Distance(ownerPos, petPos);

            if (distance > TeleportDistance)
            {
                // Teleport pet to owner
                Vector3 teleportPos = ownerPos + Vector3.back * FollowDistance;
                
                var navAgent = pet.PetGameObject.GetComponent<NavMeshAgent>();
                if (navAgent != null)
                {
                    navAgent.Warp(teleportPos);
                }
                else
                {
                    pet.PetGameObject.transform.position = teleportPos;
                }

                Debug.Log($"[PetSystem] Teleported pet for owner {ownerId} (distance was {distance:F0}m)");
            }
        }

        private void HandlePetDeath(ulong ownerId, PetInstance pet)
        {
            // Destroy GameObject
            if (pet.PetGameObject != null)
            {
                Destroy(pet.PetGameObject);
            }

            // Set resummon cooldown
            _resummonCooldowns[ownerId] = Time.time + ResummonCooldown;

            // Remove from active pets
            _activePets.Remove(ownerId);

            Debug.Log($"[PetSystem] Pet for owner {ownerId} died. Resummon available in {ResummonCooldown}s");
            OnPetDied?.Invoke(ownerId);
        }

        /// <summary>
        /// Unregister an owner, removing their pet and cooldown data.
        /// </summary>
        public void UnregisterOwner(ulong ownerId)
        {
            if (_activePets.TryGetValue(ownerId, out var pet))
            {
                if (pet.PetGameObject != null)
                {
                    Destroy(pet.PetGameObject);
                }
                _activePets.Remove(ownerId);
            }
            _resummonCooldowns.Remove(ownerId);
            _ownerPositions.Remove(ownerId);
            _ownerTargets.Remove(ownerId);
        }
        
        #endregion
    }
}
