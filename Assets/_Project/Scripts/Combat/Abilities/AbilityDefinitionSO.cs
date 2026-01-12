using UnityEngine;
using EtherDomes.Data;

namespace EtherDomes.Combat.Abilities
{
    /// <summary>
    /// ScriptableObject defining an ability's properties.
    /// Supports the hybrid combat system (stationary casters vs mobile archers).
    /// </summary>
    [CreateAssetMenu(fileName = "NewAbility", menuName = "EtherDomes/Combat/Ability Definition")]
    public class AbilityDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique identifier for this ability")]
        public int AbilityID;
        
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
        
        [Tooltip("Resource cost to use this ability")]
        [Min(0)]
        public int ResourceCost;
        
        [Tooltip("Type of resource consumed")]
        public SecondaryResourceType ResourceType = SecondaryResourceType.Mana;

        [Header("Behavior")]
        [Tooltip("TRUE: Cast is interrupted if player moves. FALSE: Can move while casting (Arquero)")]
        public bool RequiresStationary = true;
        
        [Tooltip("TRUE: Activates Global Cooldown. FALSE: Can be used during GCD (defensive abilities)")]
        public bool TriggersGCD = true;
        
        [Tooltip("TRUE: Requires enemy target. FALSE: Self-cast or friendly target")]
        public bool IsOffensive = true;
        
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

        /// <summary>
        /// Returns true if this ability has no cast time.
        /// </summary>
        public bool IsInstant => CastTime <= 0f;

        /// <summary>
        /// Returns true if this ability spawns a projectile.
        /// </summary>
        public bool IsProjectile => ProjectilePrefab != null;

        /// <summary>
        /// Returns true if this ability deals damage.
        /// </summary>
        public bool DealsDamage => BaseDamage > 0f;

        /// <summary>
        /// Returns true if this ability heals.
        /// </summary>
        public bool DoesHealing => BaseHealing > 0f;

        /// <summary>
        /// Returns true if this ability can be used during GCD.
        /// </summary>
        public bool IsOffGCD => !TriggersGCD;

        #endregion

        #region Validation

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(AbilityName))
                AbilityName = name;
                
            if (IsSelfCast)
                IsOffensive = false;
        }

        /// <summary>
        /// Validates that all required fields are set.
        /// </summary>
        public bool IsValid(out string error)
        {
            if (AbilityID <= 0)
            {
                error = "AbilityID must be greater than 0";
                return false;
            }

            if (string.IsNullOrEmpty(AbilityName))
            {
                error = "AbilityName is required";
                return false;
            }

            if (IsOffensive && Range <= 0)
            {
                error = "Offensive abilities must have Range > 0";
                return false;
            }

            error = null;
            return true;
        }

        #endregion
    }
}
