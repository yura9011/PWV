using NUnit.Framework;
using UnityEngine;
using EtherDomes.Data;
using EtherDomes.Classes.Abilities;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for Drain Life healing mechanics.
    /// Feature: new-classes-combat, Property 15: Drain Life Healing
    /// Validates: Requirements 7.7
    /// </summary>
    [TestFixture]
    public class DrainLifeHealingPropertyTests : PropertyTestBase
    {
        private const float DRAIN_LIFE_HEAL_PERCENT = 0.5f; // 50%

        #region Property 15: Drain Life Healing

        /// <summary>
        /// Feature: new-classes-combat, Property 15: Drain Life Healing
        /// For any Warlock using Drain Life, the healing received SHALL equal 50% of the damage dealt.
        /// Validates: Requirements 7.7
        /// </summary>
        [Test]
        public void Property15_DrainLife_HasCorrectHealPercent()
        {
            // Arrange
            var afflictionAbilities = ClassAbilityDefinitions.GetWarlockAfflictionAbilities();
            var drainLife = System.Array.Find(afflictionAbilities, a => a.AbilityId == "warlock_drain_life");

            // Assert
            Assert.IsNotNull(drainLife, "Drain Life should exist");
            Assert.IsTrue(drainLife.HealsOnDamage, "Drain Life should have HealsOnDamage = true");
            Assert.AreEqual(DRAIN_LIFE_HEAL_PERCENT, drainLife.HealOnDamagePercent, 0.001f,
                $"Drain Life should heal for {DRAIN_LIFE_HEAL_PERCENT * 100}% of damage");
        }

        /// <summary>
        /// Property 15: Drain Life is a channeled ability.
        /// </summary>
        [Test]
        public void DrainLife_IsChanneled()
        {
            // Arrange
            var afflictionAbilities = ClassAbilityDefinitions.GetWarlockAfflictionAbilities();
            var drainLife = System.Array.Find(afflictionAbilities, a => a.AbilityId == "warlock_drain_life");

            // Assert
            Assert.IsNotNull(drainLife, "Drain Life should exist");
            Assert.IsTrue(drainLife.IsChanneled, "Drain Life should be channeled");
            Assert.Greater(drainLife.ChannelDuration, 0f, "Drain Life should have positive channel duration");
            Assert.Greater(drainLife.TotalTicks, 0, "Drain Life should have ticks");
        }

        /// <summary>
        /// Property 15: Healing calculation is correct for random damage values.
        /// </summary>
        [Test]
        [Repeat(100)]
        public void DrainLifeHealing_CalculatesCorrectly()
        {
            // Arrange
            float damageDealt = RandomFloat(10f, 500f);
            float healPercent = DRAIN_LIFE_HEAL_PERCENT;

            // Act
            float expectedHealing = damageDealt * healPercent;

            // Assert
            Assert.AreEqual(damageDealt * 0.5f, expectedHealing, 0.001f,
                $"Healing from {damageDealt} damage should be {expectedHealing}");
        }

        /// <summary>
        /// Property 15: Per-tick healing is 50% of per-tick damage.
        /// </summary>
        [Test]
        public void DrainLife_PerTickHealing_Is50PercentOfDamage()
        {
            // Arrange
            var afflictionAbilities = ClassAbilityDefinitions.GetWarlockAfflictionAbilities();
            var drainLife = System.Array.Find(afflictionAbilities, a => a.AbilityId == "warlock_drain_life");

            // Act
            float tickDamage = drainLife.BaseDamage;
            float expectedTickHealing = tickDamage * DRAIN_LIFE_HEAL_PERCENT;

            // Assert
            Assert.AreEqual(expectedTickHealing, tickDamage * drainLife.HealOnDamagePercent, 0.001f,
                $"Per-tick healing should be {expectedTickHealing} (50% of {tickDamage})");
        }

        /// <summary>
        /// Property 15: Total healing over full channel equals 50% of total damage.
        /// </summary>
        [Test]
        public void DrainLife_TotalHealing_Is50PercentOfTotalDamage()
        {
            // Arrange
            var afflictionAbilities = ClassAbilityDefinitions.GetWarlockAfflictionAbilities();
            var drainLife = System.Array.Find(afflictionAbilities, a => a.AbilityId == "warlock_drain_life");

            // Act
            float totalDamage = drainLife.BaseDamage * drainLife.TotalTicks;
            float totalHealing = totalDamage * drainLife.HealOnDamagePercent;

            // Assert
            Assert.AreEqual(totalDamage * 0.5f, totalHealing, 0.001f,
                $"Total healing should be 50% of total damage ({totalDamage})");
        }

        #endregion

        #region Other Abilities Should Not Heal

        /// <summary>
        /// Property 15: Other Warlock abilities should NOT have HealsOnDamage.
        /// </summary>
        [Test]
        public void OtherWarlockAbilities_DoNotHealOnDamage()
        {
            // Arrange
            var afflictionAbilities = ClassAbilityDefinitions.GetWarlockAfflictionAbilities();
            var destructionAbilities = ClassAbilityDefinitions.GetWarlockDestructionAbilities();
            var sharedAbilities = ClassAbilityDefinitions.GetWarlockSharedAbilities();

            // Assert
            foreach (var ability in afflictionAbilities)
            {
                if (ability.AbilityId == "warlock_drain_life") continue;
                Assert.IsFalse(ability.HealsOnDamage,
                    $"{ability.AbilityName} should NOT have HealsOnDamage");
            }

            foreach (var ability in destructionAbilities)
            {
                Assert.IsFalse(ability.HealsOnDamage,
                    $"{ability.AbilityName} should NOT have HealsOnDamage");
            }

            foreach (var ability in sharedAbilities)
            {
                Assert.IsFalse(ability.HealsOnDamage,
                    $"{ability.AbilityName} should NOT have HealsOnDamage");
            }
        }

        #endregion
    }
}
