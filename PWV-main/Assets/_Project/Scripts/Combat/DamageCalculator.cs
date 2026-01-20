using System;
using EtherDomes.Data;
using UnityEngine;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Utility class for combat damage calculations.
    /// Implements formulas for physical/spell damage, armor mitigation, and critical hits.
    /// </summary>
    public static class DamageCalculator
    {
        // Constants for damage formulas
        public const float STAT_SCALING_DIVISOR = 100f;
        public const float ARMOR_MITIGATION_DIVISOR = 100f;
        public const float CRIT_DAMAGE_MULTIPLIER = 1.5f;
        public const float MIN_DAMAGE_MULTIPLIER = 0.1f;
        public const float MAX_CRIT_CHANCE = 100f;

        #region Core Damage Calculation

        /// <summary>
        /// Calculates physical damage based on base damage and attack stats.
        /// Formula: Damage = BaseDamage * (1 + (Strength + AttackPower) / 100)
        /// </summary>
        /// <param name="baseDamage">The base damage of the ability or attack</param>
        /// <param name="attackPower">The attacker's attack power stat</param>
        /// <param name="strength">The attacker's strength stat</param>
        /// <returns>Calculated physical damage before mitigation</returns>
        public static float CalculatePhysicalDamage(float baseDamage, int attackPower, int strength)
        {
            if (baseDamage <= 0) return 0f;
            
            float statBonus = (strength + attackPower) / STAT_SCALING_DIVISOR;
            float damage = baseDamage * (1f + statBonus);
            
            return Mathf.Max(damage, baseDamage * MIN_DAMAGE_MULTIPLIER);
        }

        /// <summary>
        /// Calculates spell damage based on base damage and spell stats.
        /// Formula: Damage = BaseDamage * (1 + (Intellect + SpellPower) / 100)
        /// </summary>
        /// <param name="baseDamage">The base damage of the spell</param>
        /// <param name="spellPower">The caster's spell power stat</param>
        /// <param name="intellect">The caster's intellect stat</param>
        /// <returns>Calculated spell damage before mitigation</returns>
        public static float CalculateSpellDamage(float baseDamage, int spellPower, int intellect)
        {
            if (baseDamage <= 0) return 0f;
            
            float statBonus = (intellect + spellPower) / STAT_SCALING_DIVISOR;
            float damage = baseDamage * (1f + statBonus);
            
            return Mathf.Max(damage, baseDamage * MIN_DAMAGE_MULTIPLIER);
        }

        /// <summary>
        /// Calculates healing amount based on base healing and spell stats.
        /// Formula: Healing = BaseHealing * (1 + (Intellect + SpellPower) / 100)
        /// </summary>
        public static float CalculateHealing(float baseHealing, int spellPower, int intellect)
        {
            if (baseHealing <= 0) return 0f;
            
            float statBonus = (intellect + spellPower) / STAT_SCALING_DIVISOR;
            float healing = baseHealing * (1f + statBonus);
            
            return Mathf.Max(healing, baseHealing * MIN_DAMAGE_MULTIPLIER);
        }

        #endregion

        #region Mitigation

        /// <summary>
        /// Calculates damage after armor mitigation.
        /// Formula: FinalDamage = Damage * (100 / (100 + Armor))
        /// </summary>
        /// <param name="damage">The incoming damage before mitigation</param>
        /// <param name="armor">The target's armor value</param>
        /// <returns>Damage after armor reduction</returns>
        public static float ApplyArmorMitigation(float damage, int armor)
        {
            if (damage <= 0) return 0f;
            if (armor <= 0) return damage;
            
            float mitigation = ARMOR_MITIGATION_DIVISOR / (ARMOR_MITIGATION_DIVISOR + armor);
            return damage * mitigation;
        }

        /// <summary>
        /// Gets the damage reduction percentage from armor.
        /// </summary>
        /// <param name="armor">The armor value</param>
        /// <returns>Damage reduction as a percentage (0-100)</returns>
        public static float GetArmorReductionPercent(int armor)
        {
            if (armor <= 0) return 0f;
            return (armor / (ARMOR_MITIGATION_DIVISOR + armor)) * 100f;
        }

        #endregion

        #region Critical Hits

        /// <summary>
        /// Calculates effective crit chance from base chance and crit rating.
        /// </summary>
        /// <param name="baseCritChance">Base crit chance (usually 5%)</param>
        /// <param name="critRating">Additional crit rating from gear/stats</param>
        /// <returns>Total crit chance capped at 100%</returns>
        public static float CalculateCritChance(float baseCritChance, float critRating)
        {
            float totalCrit = baseCritChance + critRating;
            return Mathf.Clamp(totalCrit, 0f, MAX_CRIT_CHANCE);
        }

        /// <summary>
        /// Determines if an attack is a critical hit.
        /// </summary>
        /// <param name="critChance">The crit chance percentage (0-100)</param>
        /// <returns>True if the attack is a critical hit</returns>
        public static bool RollCrit(float critChance)
        {
            if (critChance <= 0) return false;
            return UnityEngine.Random.Range(0f, 100f) < critChance;
        }

        /// <summary>
        /// Applies crit damage multiplier if the attack is a crit.
        /// </summary>
        /// <param name="damage">The base damage</param>
        /// <param name="isCrit">Whether this is a critical hit</param>
        /// <returns>Damage after crit multiplier (if applicable)</returns>
        public static float ApplyCritMultiplier(float damage, bool isCrit)
        {
            return isCrit ? damage * CRIT_DAMAGE_MULTIPLIER : damage;
        }

        /// <summary>
        /// Calculates crit damage (shorthand for damage * CRIT_MULTIPLIER).
        /// </summary>
        public static float CalculateCritDamage(float damage)
        {
            return damage * CRIT_DAMAGE_MULTIPLIER;
        }

        #endregion

        #region Full Damage Pipeline

        /// <summary>
        /// Result of a complete damage calculation.
        /// </summary>
        public struct DamageResult
        {
            public float RawDamage;
            public float MitigatedDamage;
            public float FinalDamage;
            public bool IsCritical;
            
            public override string ToString()
            {
                return $"Raw: {RawDamage:F0}, Mit: {MitigatedDamage:F0}, Final: {FinalDamage:F0}, Crit: {IsCritical}";
            }
        }

        /// <summary>
        /// Performs a complete physical damage calculation pipeline.
        /// </summary>
        /// <param name="baseDamage">Base damage of the attack</param>
        /// <param name="attackerStats">Attacker's character stats</param>
        /// <param name="targetArmor">Target's armor value</param>
        /// <param name="rollCrit">Whether to roll for crit (set false for consistent testing)</param>
        public static DamageResult CalculateFullPhysicalDamage(
            float baseDamage, 
            CharacterStats attackerStats, 
            int targetArmor,
            bool rollCrit = true)
        {
            var result = new DamageResult();
            
            // Step 1: Calculate raw damage from stats
            result.RawDamage = CalculatePhysicalDamage(
                baseDamage, 
                attackerStats.AttackPower, 
                attackerStats.Strength);
            
            // Step 2: Apply armor mitigation
            result.MitigatedDamage = ApplyArmorMitigation(result.RawDamage, targetArmor);
            
            // Step 3: Roll for crit and apply multiplier
            float critChance = CalculateCritChance(
                attackerStats.GetEffectiveCritChance(), 
                0f); // Rating already included in CritChance
            result.IsCritical = rollCrit && RollCrit(critChance);
            result.FinalDamage = ApplyCritMultiplier(result.MitigatedDamage, result.IsCritical);
            
            return result;
        }

        /// <summary>
        /// Performs a complete spell damage calculation pipeline.
        /// Note: Spell damage typically ignores armor unless specified.
        /// </summary>
        public static DamageResult CalculateFullSpellDamage(
            float baseDamage, 
            CharacterStats casterStats,
            bool ignoresArmor = true,
            int targetArmor = 0,
            bool rollCrit = true)
        {
            var result = new DamageResult();
            
            // Step 1: Calculate raw damage from stats
            result.RawDamage = CalculateSpellDamage(
                baseDamage, 
                casterStats.SpellPower, 
                casterStats.Intellect);
            
            // Step 2: Apply armor mitigation (if not ignoring armor)
            result.MitigatedDamage = ignoresArmor ? 
                result.RawDamage : 
                ApplyArmorMitigation(result.RawDamage, targetArmor);
            
            // Step 3: Roll for crit and apply multiplier
            float critChance = CalculateCritChance(
                casterStats.GetEffectiveCritChance(), 
                0f);
            result.IsCritical = rollCrit && RollCrit(critChance);
            result.FinalDamage = ApplyCritMultiplier(result.MitigatedDamage, result.IsCritical);
            
            return result;
        }

        /// <summary>
        /// Performs a complete healing calculation pipeline.
        /// </summary>
        public static DamageResult CalculateFullHealing(
            float baseHealing, 
            CharacterStats casterStats,
            bool rollCrit = true)
        {
            var result = new DamageResult();
            
            // Step 1: Calculate raw healing from stats
            result.RawDamage = CalculateHealing(
                baseHealing, 
                casterStats.SpellPower, 
                casterStats.Intellect);
            result.MitigatedDamage = result.RawDamage; // No mitigation on healing
            
            // Step 2: Roll for crit
            float critChance = CalculateCritChance(
                casterStats.GetEffectiveCritChance(), 
                0f);
            result.IsCritical = rollCrit && RollCrit(critChance);
            result.FinalDamage = ApplyCritMultiplier(result.MitigatedDamage, result.IsCritical);
            
            return result;
        }

        #endregion

        #region Haste Calculations

        /// <summary>
        /// Calculates cast time after haste reduction.
        /// Formula: FinalCastTime = BaseCastTime / (1 + Haste/100)
        /// </summary>
        public static float ApplyHasteToCastTime(float baseCastTime, float haste)
        {
            if (baseCastTime <= 0) return 0f;
            float hasteMultiplier = 1f + (haste / 100f);
            return baseCastTime / hasteMultiplier;
        }

        /// <summary>
        /// Calculates GCD after haste reduction.
        /// GCD cannot go below the minimum (usually 1.0s).
        /// </summary>
        public static float ApplyHasteToGCD(float baseGCD, float haste, float minGCD = 1.0f)
        {
            float hastedGCD = ApplyHasteToCastTime(baseGCD, haste);
            return Mathf.Max(hastedGCD, minGCD);
        }

        #endregion
    }
}
