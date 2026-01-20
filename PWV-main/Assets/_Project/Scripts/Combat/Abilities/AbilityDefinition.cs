using UnityEngine;
using EtherDomes.Data;

namespace EtherDomes.Combat.Abilities
{
    /// <summary>
    /// Unified ScriptableObject for ability definitions.
    /// Combines the best of AbilityData and AbilityDefinitionSO.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAbility", menuName = "EtherDomes/Combat/Ability Definition")]
    public class AbilityDefinition : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique identifier for this ability")]
        public string AbilityId;
        
        [Tooltip("Display name of the ability")]
        public string AbilityName;
        
        [TextArea(2, 4)]
        [Tooltip("Description shown in tooltips")]
        public string Description;
        
        [Tooltip("Icon displayed in action bar")]
        public Sprite Icon;

        [Header("Costs and Timing")]
        [Tooltip("Time to cast in seconds. 0 = instant cast")]
        [Min(0f)]
        public float CastTime;
        
        [Tooltip("Cooldown in seconds before ability can be used again")]
        [Min(0f)]
        public float Cooldown;
        
        [Tooltip("Maximum range to target in meters")]
        [Min(0f)]
        public float Range = 30f;
        
        [Tooltip("Minimum range (Hunter dead zone)")]
        [Min(0f)]
        public float MinRange = 0f;
        
        [Tooltip("Resource cost to use this ability")]
        [Min(0)]
        public float ResourceCost;
        
        [Tooltip("Type of resource consumed")]
        public SecondaryResourceType ResourceType = SecondaryResourceType.Mana;

        [Header("Behavior")]
        [Tooltip("TRUE: Cast is interrupted if player moves. FALSE: Can move while casting (Arquero)")]
        public bool RequiresStationary = true;
        
        [Tooltip("TRUE: Activates Global Cooldown. FALSE: Can be used during GCD (defensive abilities)")]
        public bool TriggersGCD = true;
        
        [Tooltip("TRUE: Requires enemy target. FALSE: Self-cast or friendly target")]
        public bool RequiresTarget = true;
        
        [Tooltip("TRUE: Targets self automatically")]
        public bool IsSelfCast;
        
        [Tooltip("TRUE: Requires target to be in front of caster")]
        public bool RequiresFacing = true;

        [Header("Damage/Healing")]
        [Tooltip("Base damage dealt (before stats and mitigation)")]
        [Min(0f)]
        public float BaseDamage;
        
        [Tooltip("Base healing done (before stats)")]
        [Min(0f)]
        public float BaseHealing;
        
        [Tooltip("Type of damage dealt")]
        public DamageType DamageType = DamageType.Physical;
        
        [Tooltip("Stat multiplier for damage/healing scaling")]
        [Min(0f)]
        public float StatMultiplier = 1f;
        
        [Tooltip("Threat multiplier (1.0 = normal)")]
        [Min(0f)]
        public float ThreatMultiplier = 1f;

        [Header("Channeling")]
        [Tooltip("TRUE: This is a channeled ability")]
        public bool IsChanneled;
        
        [Tooltip("Duration of channeling in seconds")]
        [Min(0f)]
        public float ChannelDuration;
        
        [Tooltip("Time between ticks in seconds")]
        [Min(0.1f)]
        public float TickInterval = 1f;

        [Header("Stealth")]
        [Tooltip("Requires stealth to use")]
        public bool RequiresStealth;
        
        [Tooltip("Breaks stealth when used")]
        public bool BreaksStealth = true;

        [Header("Combo Points")]
        [Tooltip("Generates combo points when used")]
        public bool GeneratesComboPoint;
        
        [Tooltip("Number of combo points generated")]
        [Min(1)]
        public int ComboPointsGenerated = 1;
        
        [Tooltip("Consumes all combo points (finisher)")]
        public bool ConsumesComboPoints;
        
        [Tooltip("Damage multiplier per combo point (0.2 = 20% per point)")]
        [Min(0f)]
        public float ComboPointDamageMultiplier = 0.2f;

        [Header("Special Effects")]
        [Tooltip("Heals caster for percentage of damage dealt")]
        public bool HealsOnDamage;
        
        [Tooltip("Percentage of damage that heals caster (0.5 = 50%)")]
        [Range(0f, 1f)]
        public float HealOnDamagePercent;
        
        [Tooltip("Crowd control type applied")]
        public CCType CCType = CCType.None;
        
        [Tooltip("Pulls target to caster (Death Grip)")]
        public bool IsPullEffect;
        
        [Tooltip("Pushes caster away from target (Disengage)")]
        public bool IsKnockbackSelf;
        
        [Tooltip("Distance for knockback effects")]
        [Min(0f)]
        public float KnockbackDistance = 15f;

        [Header("Requirements")]
        [Tooltip("Required class to use this ability")]
        public CharacterClass RequiredClass;
        
        [Tooltip("Required specialization (optional)")]
        public Specialization? RequiredSpec;
        
        [Tooltip("Minimum level to unlock")]
        [Min(1)]
        public int UnlockLevel = 1;

        [Header("Visual Effects")]
        [Tooltip("Projectile prefab. If null, damage is instant (hitscan)")]
        public GameObject ProjectilePrefab;
        
        [Tooltip("Speed of projectile in m/s")]
        [Min(1f)]
        public float ProjectileSpeed = 20f;
        
        [Tooltip("Effect spawned on impact")]
        public GameObject ImpactEffectPrefab;
        
        [Tooltip("Effect spawned on caster during cast")]
        public GameObject CastEffectPrefab;

        [Header("Audio")]
        [Tooltip("Sound played when cast starts")]
        public AudioClip CastSound;
        
        [Tooltip("Sound played on impact")]
        public AudioClip ImpactSound;

        [Header("Animation")]
        [Tooltip("Animation trigger name for casting")]
        public string CastAnimationTrigger = "Cast";
        
        [Tooltip("Animation trigger name for attack")]
        public string AttackAnimationTrigger = "Attack";

        #region Computed Properties

        /// <summary>Returns true if this ability has no cast time and is not channeled.</summary>
        public bool IsInstant => CastTime <= 0f && !IsChanneled;

        /// <summary>Returns true if this ability spawns a projectile.</summary>
        public bool IsProjectile => ProjectilePrefab != null;

        /// <summary>Returns true if this ability deals damage.</summary>
        public bool DealsDamage => BaseDamage > 0f;

        /// <summary>Returns true if this ability heals.</summary>
        public bool DoesHealing => BaseHealing > 0f;

        /// <summary>Returns true if this ability can be used during GCD.</summary>
        public bool IsOffGCD => !TriggersGCD;

        /// <summary>Total number of ticks for channeled abilities.</summary>
        public int TotalTicks => IsChanneled && TickInterval > 0 
            ? Mathf.CeilToInt(ChannelDuration / TickInterval) 
            : 0;

        #endregion

        #region Conversion

        /// <summary>
        /// Converts this ScriptableObject to AbilityData for runtime use.
        /// </summary>
        public AbilityData ToAbilityData()
        {
            return new AbilityData
            {
                AbilityId = AbilityId,
                AbilityName = AbilityName,
                Description = Description,
                Icon = Icon,
                CastTime = CastTime,
                Cooldown = Cooldown,
                ManaCost = ResourceCost, // Legacy compatibility
                Range = Range,
                MinRange = MinRange,
                RequiresTarget = RequiresTarget,
                AffectedByGCD = TriggersGCD,
                Type = DealsDamage ? AbilityType.Damage : (DoesHealing ? AbilityType.Healing : AbilityType.Utility),
                DamageType = DamageType,
                RequiredClass = RequiredClass,
                RequiredSpec = RequiredSpec,
                UnlockLevel = UnlockLevel,
                BaseDamage = BaseDamage,
                BaseHealing = BaseHealing,
                ThreatMultiplier = ThreatMultiplier,
                IsChanneled = IsChanneled,
                ChannelDuration = ChannelDuration,
                TickInterval = TickInterval,
                RequiresStealth = RequiresStealth,
                BreaksStealth = BreaksStealth,
                GeneratesComboPoint = GeneratesComboPoint,
                ComboPointsGenerated = ComboPointsGenerated,
                ConsumesComboPoints = ConsumesComboPoints,
                ComboPointDamageMultiplier = ComboPointDamageMultiplier,
                ResourceType = ResourceType,
                ResourceCost = ResourceCost,
                HealsOnDamage = HealsOnDamage,
                HealOnDamagePercent = HealOnDamagePercent,
                CCType = CCType,
                IsPullEffect = IsPullEffect,
                IsKnockbackSelf = IsKnockbackSelf,
                KnockbackDistance = KnockbackDistance
            };
        }

        #endregion

        #region Validation

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(AbilityId))
                AbilityId = System.Guid.NewGuid().ToString();
                
            if (string.IsNullOrEmpty(AbilityName))
                AbilityName = name;
                
            if (IsSelfCast)
                RequiresTarget = false;
                
            if (IsChanneled && ChannelDuration <= 0)
                ChannelDuration = 3f;
                
            if (IsChanneled && TickInterval <= 0)
                TickInterval = 1f;
        }

        /// <summary>
        /// Validates that all required fields are set.
        /// </summary>
        public bool IsValid(out string error)
        {
            if (string.IsNullOrEmpty(AbilityId))
            {
                error = "AbilityId is required";
                return false;
            }

            if (string.IsNullOrEmpty(AbilityName))
            {
                error = "AbilityName is required";
                return false;
            }

            if (RequiresTarget && Range <= 0)
            {
                error = "Abilities that require target must have Range > 0";
                return false;
            }

            if (IsChanneled && ChannelDuration <= 0)
            {
                error = "Channeled abilities must have ChannelDuration > 0";
                return false;
            }

            if (IsChanneled && TickInterval <= 0)
            {
                error = "Channeled abilities must have TickInterval > 0";
                return false;
            }

            error = null;
            return true;
        }

        #endregion
    }
}