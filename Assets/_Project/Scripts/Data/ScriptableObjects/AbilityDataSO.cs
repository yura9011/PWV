using UnityEngine;

namespace EtherDomes.Data
{
    /// <summary>
    /// ScriptableObject for defining abilities in the Unity Editor.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAbility", menuName = "EtherDomes/Ability")]
    public class AbilityDataSO : ScriptableObject
    {
        [Header("Basic Info")]
        public string AbilityId;
        public string AbilityName;
        [TextArea(2, 4)]
        public string Description;
        public Sprite Icon;

        [Header("Timing")]
        [Tooltip("0 for instant cast")]
        public float CastTime = 0f;
        public float Cooldown = 0f;
        public float ManaCost = 0f;

        [Header("Targeting")]
        public float Range = 30f;
        public bool RequiresTarget = true;
        public bool AffectedByGCD = true;

        [Header("Type")]
        public AbilityType Type = AbilityType.Damage;
        public DamageType DamageType = DamageType.Physical;

        [Header("Requirements")]
        public CharacterClass RequiredClass;
        public bool RequiresSpecificSpec = false;
        public Specialization RequiredSpec;
        public int UnlockLevel = 1;

        [Header("Effects")]
        public float BaseDamage = 0f;
        public float BaseHealing = 0f;
        [Tooltip("Multiplier for threat generation (1.0 = normal)")]
        public float ThreatMultiplier = 1f;

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
                ManaCost = ManaCost,
                Range = Range,
                RequiresTarget = RequiresTarget,
                AffectedByGCD = AffectedByGCD,
                Type = Type,
                DamageType = DamageType,
                RequiredClass = RequiredClass,
                RequiredSpec = RequiresSpecificSpec ? RequiredSpec : (Specialization?)null,
                UnlockLevel = UnlockLevel,
                BaseDamage = BaseDamage,
                BaseHealing = BaseHealing,
                ThreatMultiplier = ThreatMultiplier
            };
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(AbilityId))
            {
                AbilityId = System.Guid.NewGuid().ToString();
            }
        }
    }
}
