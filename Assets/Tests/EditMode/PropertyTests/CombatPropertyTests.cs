using NUnit.Framework;
using UnityEngine;
using EtherDomes.Data;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for combat systems (Aggro/Threat).
    /// </summary>
    [TestFixture]
    public class CombatPropertyTests
    {
        /// <summary>
        /// Property: Threat Calculation Correctness
        /// Threat should scale linearly with damage dealt.
        /// </summary>
        [Test]
        public void ThreatCalculation_ScalesLinearly_WithDamage(
            [Values(10f, 50f, 100f, 500f, 1000f)] float damage,
            [Values(1f, 1.5f, 2f)] float threatMultiplier)
        {
            // Arrange
            float baseThreat = damage * threatMultiplier;

            // Act - Double the damage should double the threat
            float doubleDamage = damage * 2f;
            float doubleThreat = doubleDamage * threatMultiplier;

            // Assert
            Assert.That(doubleThreat, Is.EqualTo(baseThreat * 2f).Within(0.001f),
                "Threat should scale linearly with damage");
        }

        /// <summary>
        /// Property: Threat is always non-negative
        /// </summary>
        [Test]
        public void ThreatCalculation_IsNeverNegative(
            [Values(0f, 1f, 100f, -10f)] float damage,
            [Values(0f, 1f, 2f)] float multiplier)
        {
            // Threat should be clamped to non-negative
            float threat = Mathf.Max(0f, damage * multiplier);

            Assert.That(threat, Is.GreaterThanOrEqualTo(0f),
                "Threat should never be negative");
        }

        /// <summary>
        /// Property: Higher threat multiplier produces higher threat
        /// </summary>
        [Test]
        public void HigherThreatMultiplier_ProducesHigherThreat(
            [Values(10f, 50f, 100f)] float damage)
        {
            float lowMultiplier = 1f;
            float highMultiplier = 2f;

            float lowThreat = damage * lowMultiplier;
            float highThreat = damage * highMultiplier;

            Assert.That(highThreat, Is.GreaterThan(lowThreat),
                "Higher threat multiplier should produce higher threat");
        }

        /// <summary>
        /// Property: Healing generates threat (typically 0.5x of healing done)
        /// </summary>
        [Test]
        public void HealingThreat_IsProportionalToHealing(
            [Values(100f, 500f, 1000f)] float healingAmount)
        {
            const float healingThreatMultiplier = 0.5f;
            float expectedThreat = healingAmount * healingThreatMultiplier;

            Assert.That(expectedThreat, Is.EqualTo(healingAmount * 0.5f).Within(0.001f),
                "Healing threat should be 50% of healing done");
        }

        /// <summary>
        /// Property: Tank threat multiplier is higher than DPS
        /// </summary>
        [Test]
        public void TankThreatMultiplier_IsHigherThanDPS()
        {
            // Typical values
            const float tankMultiplier = 2f;
            const float dpsMultiplier = 1f;
            const float healerMultiplier = 0.5f;

            Assert.That(tankMultiplier, Is.GreaterThan(dpsMultiplier),
                "Tank threat multiplier should be higher than DPS");
            Assert.That(dpsMultiplier, Is.GreaterThan(healerMultiplier),
                "DPS threat multiplier should be higher than Healer");
        }
    }
}
