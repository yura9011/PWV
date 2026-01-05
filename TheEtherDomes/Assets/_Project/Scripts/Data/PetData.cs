using System;
using UnityEngine;

namespace EtherDomes.Data
{
    /// <summary>
    /// Data definition for a pet.
    /// Contains all configuration for the pet including stats, abilities, and behavior.
    /// </summary>
    [Serializable]
    public class PetData
    {
        /// <summary>
        /// Unique identifier for this pet type.
        /// </summary>
        public string PetId;

        /// <summary>
        /// Display name shown in UI.
        /// </summary>
        public string DisplayName;

        /// <summary>
        /// Type of pet (Beast, Demon, etc.).
        /// </summary>
        public PetType PetType;

        /// <summary>
        /// Base health of the pet.
        /// </summary>
        public float BaseHealth;

        /// <summary>
        /// Base damage per attack.
        /// </summary>
        public float BaseDamage;

        /// <summary>
        /// Time between attacks in seconds.
        /// </summary>
        public float AttackSpeed;

        /// <summary>
        /// Range at which pet can attack (in meters).
        /// </summary>
        public float AttackRange;

        /// <summary>
        /// Movement speed of the pet.
        /// </summary>
        public float MoveSpeed;

        /// <summary>
        /// Prefab to spawn for this pet.
        /// </summary>
        public GameObject PetPrefab;

        /// <summary>
        /// Icon to display in UI.
        /// </summary>
        public Sprite Icon;

        /// <summary>
        /// Create a default pet data instance.
        /// </summary>
        public PetData()
        {
            PetId = string.Empty;
            DisplayName = string.Empty;
            PetType = PetType.Beast;
            BaseHealth = 100f;
            BaseDamage = 10f;
            AttackSpeed = 2f;
            AttackRange = 2f;
            MoveSpeed = 5f;
        }

        /// <summary>
        /// Create a copy of this pet data.
        /// </summary>
        public PetData Clone()
        {
            return new PetData
            {
                PetId = PetId,
                DisplayName = DisplayName,
                PetType = PetType,
                BaseHealth = BaseHealth,
                BaseDamage = BaseDamage,
                AttackSpeed = AttackSpeed,
                AttackRange = AttackRange,
                MoveSpeed = MoveSpeed,
                PetPrefab = PetPrefab,
                Icon = Icon
            };
        }
    }

    /// <summary>
    /// Runtime instance of an active pet.
    /// Tracks current state, health, target, and owner information.
    /// </summary>
    [Serializable]
    public class PetInstance
    {
        /// <summary>
        /// The pet data definition.
        /// </summary>
        public PetData Data;

        /// <summary>
        /// Entity ID of the pet owner.
        /// </summary>
        public ulong OwnerId;

        /// <summary>
        /// Entity ID assigned to this pet instance.
        /// </summary>
        public ulong PetEntityId;

        /// <summary>
        /// Current health of the pet.
        /// </summary>
        public float CurrentHealth;

        /// <summary>
        /// Entity ID of the current attack target (0 if none).
        /// </summary>
        public ulong CurrentTargetId;

        /// <summary>
        /// Current state of the pet.
        /// </summary>
        public PetState State;

        /// <summary>
        /// Time when the pet was summoned.
        /// </summary>
        public float SummonedTime;

        /// <summary>
        /// Time of last attack.
        /// </summary>
        public float LastAttackTime;

        /// <summary>
        /// Reference to the spawned GameObject.
        /// </summary>
        [NonSerialized]
        public GameObject PetGameObject;

        /// <summary>
        /// Create a new pet instance from pet data.
        /// </summary>
        /// <param name="data">The pet data definition</param>
        /// <param name="ownerId">Entity ID of the owner</param>
        /// <param name="petEntityId">Entity ID for this pet</param>
        /// <param name="currentTime">Current game time</param>
        public PetInstance(PetData data, ulong ownerId, ulong petEntityId, float currentTime)
        {
            Data = data;
            OwnerId = ownerId;
            PetEntityId = petEntityId;
            CurrentHealth = data.BaseHealth;
            CurrentTargetId = 0;
            State = PetState.Following;
            SummonedTime = currentTime;
            LastAttackTime = 0f;
        }

        /// <summary>
        /// Default constructor for serialization.
        /// </summary>
        public PetInstance()
        {
            State = PetState.Dismissed;
        }

        /// <summary>
        /// Check if the pet is alive.
        /// </summary>
        public bool IsAlive => State != PetState.Dead && State != PetState.Dismissed && CurrentHealth > 0;

        /// <summary>
        /// Check if the pet has a target.
        /// </summary>
        public bool HasTarget => CurrentTargetId != 0;

        /// <summary>
        /// Get the maximum health from pet data.
        /// </summary>
        public float MaxHealth => Data?.BaseHealth ?? 0f;

        /// <summary>
        /// Get the pet ID from the data.
        /// </summary>
        public string PetId => Data?.PetId ?? string.Empty;

        /// <summary>
        /// Get the display name from the data.
        /// </summary>
        public string DisplayName => Data?.DisplayName ?? string.Empty;

        /// <summary>
        /// Check if the pet can attack (based on attack speed).
        /// </summary>
        /// <param name="currentTime">Current game time</param>
        /// <returns>True if enough time has passed since last attack</returns>
        public bool CanAttack(float currentTime)
        {
            if (Data == null) return false;
            return currentTime - LastAttackTime >= Data.AttackSpeed;
        }

        /// <summary>
        /// Record an attack at the current time.
        /// </summary>
        /// <param name="currentTime">Current game time</param>
        public void RecordAttack(float currentTime)
        {
            LastAttackTime = currentTime;
        }

        /// <summary>
        /// Apply damage to the pet.
        /// </summary>
        /// <param name="damage">Amount of damage to apply</param>
        /// <returns>True if pet died from this damage</returns>
        public bool TakeDamage(float damage)
        {
            CurrentHealth -= damage;
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                State = PetState.Dead;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Heal the pet.
        /// </summary>
        /// <param name="amount">Amount to heal</param>
        public void Heal(float amount)
        {
            if (!IsAlive) return;
            CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        }

        /// <summary>
        /// Set the pet's target.
        /// </summary>
        /// <param name="targetId">Entity ID of the target</param>
        public void SetTarget(ulong targetId)
        {
            CurrentTargetId = targetId;
            if (targetId != 0 && IsAlive)
            {
                State = PetState.Attacking;
            }
        }

        /// <summary>
        /// Clear the pet's target and return to following.
        /// </summary>
        public void ClearTarget()
        {
            CurrentTargetId = 0;
            if (IsAlive)
            {
                State = PetState.Following;
            }
        }
    }
}
