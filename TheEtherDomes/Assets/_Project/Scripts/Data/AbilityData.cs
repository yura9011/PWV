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
        }

        public bool IsInstant => CastTime <= 0f;
        
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
