using System;
using UnityEngine;

namespace EtherDomes.Data
{
    /// <summary>
    /// Data defining an ability's properties.
    /// </summary>
    [Serializable]
    public class AbilityData
    {
        public string AbilityId;
        public string AbilityName;
        public string Description;
        public Sprite Icon;
        public float CastTime;        // 0 for instant
        public float Cooldown;
        public float ManaCost;
        public float Range;
        public bool RequiresTarget;
        public bool AffectedByGCD;
        public AbilityType Type;
        public DamageType DamageType;
        public CharacterClass RequiredClass;
        public Specialization? RequiredSpec;
        public int UnlockLevel;
        
        // Effect values
        public float BaseDamage;
        public float BaseHealing;
        public float ThreatMultiplier;

        // Channeled ability support (Requirements 2.1, 2.3)
        public bool IsChanneled;
        public float ChannelDuration;
        public float TickInterval;    // Default 1.0 second
        
        /// <summary>
        /// Total number of ticks for channeled abilities.
        /// Calculated as ceil(ChannelDuration / TickInterval).
        /// </summary>
        public int TotalTicks => IsChanneled && TickInterval > 0 
            ? Mathf.CeilToInt(ChannelDuration / TickInterval) 
            : 0;

        // Stealth ability support (Requirements 4.5)
        /// <summary>
        /// Whether this ability requires the player to be in stealth to use.
        /// </summary>
        public bool RequiresStealth;
        
        /// <summary>
        /// Whether using this ability breaks stealth.
        /// </summary>
        public bool BreaksStealth;

        // Combo Point mechanics (Requirements 5.4, 5.5)
        /// <summary>
        /// Whether this ability generates combo points when used.
        /// </summary>
        public bool GeneratesComboPoint;

        /// <summary>
        /// Number of combo points generated (default 1, Ambush generates 2).
        /// </summary>
        public int ComboPointsGenerated;
        
        /// <summary>
        /// Whether this ability consumes all combo points (finisher).
        /// </summary>
        public bool ConsumesComboPoints;
        
        /// <summary>
        /// Damage multiplier per combo point consumed (default 0.2 = 20% per point).
        /// </summary>
        public float ComboPointDamageMultiplier;

        // Resource type override
        /// <summary>
        /// The resource type this ability uses (Energy, Focus, etc.).
        /// </summary>
        public SecondaryResourceType ResourceType;

        /// <summary>
        /// Cost of the secondary resource (Energy, Focus, etc.) for this ability.
        /// </summary>
        public float ResourceCost;

        // Range constraints (Requirements 6.8)
        /// <summary>
        /// Minimum range for the ability (for Hunter dead zone).
        /// </summary>
        public float MinRange;

        // Drain Life healing (Requirements 7.7)
        /// <summary>
        /// Whether this ability heals the caster for a percentage of damage dealt.
        /// </summary>
        public bool HealsOnDamage;
        
        /// <summary>
        /// Percentage of damage dealt that heals the caster (0.5 = 50%).
        /// </summary>
        public float HealOnDamagePercent;

        /// <summary>
        /// Type of crowd control effect this ability applies.
        /// Used for Diminishing Returns tracking.
        /// Requirements: 11.6, 11.7
        /// </summary>
        public CCType CCType;

        // Movement effects (Requirements 11.4, 11.5)
        /// <summary>
        /// Whether this ability pulls the target to the caster (Death Grip).
        /// Requirements: 11.4
        /// </summary>
        public bool IsPullEffect;

        /// <summary>
        /// Whether this ability pushes the caster away from the target (Disengage).
        /// Requirements: 11.5
        /// </summary>
        public bool IsKnockbackSelf;

        /// <summary>
        /// Distance for knockback/disengage effects in meters.
        /// </summary>
        public float KnockbackDistance;

        public AbilityData()
        {
            AbilityId = Guid.NewGuid().ToString();
            CastTime = 0f;
            Cooldown = 0f;
            ManaCost = 0f;
            Range = 30f;
            RequiresTarget = true;
            AffectedByGCD = true;
            Type = AbilityType.Damage;
            DamageType = DamageType.Physical;
            UnlockLevel = 1;
            BaseDamage = 0f;
            BaseHealing = 0f;
            ThreatMultiplier = 1f;
            
            // Channeled defaults
            IsChanneled = false;
            ChannelDuration = 0f;
            TickInterval = 1.0f;
            
            // Stealth defaults
            RequiresStealth = false;
            BreaksStealth = true; // Most abilities break stealth by default
            
            // Combo Point defaults (Requirements 5.4, 5.5)
            GeneratesComboPoint = false;
            ComboPointsGenerated = 1; // Default 1, Ambush generates 2
            ConsumesComboPoints = false;
            ComboPointDamageMultiplier = 0.2f; // 20% per combo point
            
            // Resource type default
            ResourceType = SecondaryResourceType.None;
            ResourceCost = 0f;
            
            // Range defaults
            MinRange = 0f;
            
            // Drain Life healing defaults (Requirements 7.7)
            HealsOnDamage = false;
            HealOnDamagePercent = 0f;

            // CC type default (Requirements 11.6, 11.7)
            CCType = CCType.None;

            // Movement effect defaults (Requirements 11.4, 11.5)
            IsPullEffect = false;
            IsKnockbackSelf = false;
            KnockbackDistance = 15f; // Default 15 meters for Disengage
        }

        public bool IsInstant => CastTime <= 0f && !IsChanneled;
        
        public bool CanUse(int playerLevel, float currentMana, bool hasTarget)
        {
            if (playerLevel < UnlockLevel) return false;
            if (currentMana < ManaCost) return false;
            if (RequiresTarget && !hasTarget) return false;
            return true;
        }
    }

    /// <summary>
    /// Runtime state of an ability (cooldowns, etc.)
    /// </summary>
    public class AbilityState
    {
        public AbilityData Data;
        public float CooldownRemaining;
        public bool IsOnCooldown => CooldownRemaining > 0f;

        public AbilityState(AbilityData data)
        {
            Data = data;
            CooldownRemaining = 0f;
        }

        public void StartCooldown()
        {
            CooldownRemaining = Data.Cooldown;
        }

        public void UpdateCooldown(float deltaTime)
        {
            if (CooldownRemaining > 0f)
            {
                CooldownRemaining -= deltaTime;
                if (CooldownRemaining < 0f)
                    CooldownRemaining = 0f;
            }
        }
    }
}
