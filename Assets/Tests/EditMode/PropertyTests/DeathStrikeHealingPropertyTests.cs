using NUnit.Framework;
using UnityEngine;
using EtherDomes.Combat;
using EtherDomes.Data;
using EtherDomes.Classes.Abilities;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for Death Strike healing mechanics.
    /// Feature: new-classes-combat, Property 16: Death Strike Healing
    /// Validates: Requirements 8.8
    /// </summary>
    [TestFixture]
    public class DeathStrikeHealingPropertyTests : PropertyTestBase
    {
        private CombatSystem _combatSystem;
        private const ulong TEST_PLAYER_ID = 1;
        private const float MAX_HEALTH = 1000f;
        private const float DEATH_STRIKE_HEAL_PERCENT = 0.25f; // 25%
        private const float DEATH_STRIKE_MIN_HEAL_PERCENT = 0.10f; // 10%

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("CombatSystem");
            _combatSystem = go.AddComponent<CombatSystem>();
            _combatSystem.Initialize(null);
            _combatSystem.RegisterPlayer(TEST_PLAYER_ID, MAX_HEALTH);
        }

        [TearDown]
        public void TearDown()
        {
            if (_combatSystem != null)
            {
                Object.DestroyImmediate(_combatSystem.gameObject);
            }
        }

        #region Property 16: Death Strike Healing

        /// <summary>
        /// Feature: new-classes-combat, Property 16: Death Strike Healing
        /// For any Death Knight using Death Strike, the healing received SHALL equal 
        /// 25% of damage taken in the last 5 seconds (minimum 10% of max health).
        /// Validates: Requirements 8.8
        /// </summary>
        [Test]
        public void Property16_DeathStrike_HasCorrectHealPercent()
        {
            // Arrange
            var bloodAbilities = ClassAbilityDefinitions.GetDeathKnightBloodAbilities();
            var deathStrike = System.Array.Find(bloodAbilities, a => a.AbilityId == "dk_death_strike");

            // Assert
            Assert.IsNotNull(deathStrike, "Death Strike should exist");
            Assert.IsTrue(deathStrike.HealsOnDamage, "Death Strike should have HealsOnDamage = true");
            Assert.AreEqual(DEATH_STRIKE_HEAL_PERCENT, deathStrike.HealOnDamagePercent, 0.001f,
                $"Death Strike should heal for {DEATH_STRIKE_HEAL_PERCENT * 100}% of recent damage");
        }

        /// <summary>
        /// Property 16: Death Strike healing is based on recent damage taken.
        /// </summary>
        [Test]
        [Repeat(100)]
        public void DeathStrikeHealing_BasedOnRecentDamage()
        {
            // Arrange
            float damageTaken = RandomFloat(100f, 800f);
            
            // Apply damage to track
            _combatSystem.ApplyDamage(TEST_PLAYER_ID, damageTaken, DamageType.Physical, 999);

            // Act
            float recentDamage = _combatSystem.GetRecentDamageTaken(TEST_PLAYER_ID);
            float healing = _combatSystem.CalculateDeathStrikeHealing(TEST_PLAYER_ID);

            // Assert
            Assert.AreEqual(damageTaken, recentDamage, 0.001f,
                "Recent damage should match applied damage");
            
            float expectedHealing = damageTaken * DEATH_STRIKE_HEAL_PERCENT;
            float minHealing = MAX_HEALTH * DEATH_STRIKE_MIN_HEAL_PERCENT;
            float actualExpected = Mathf.Max(expectedHealing, minHealing);
            
            Assert.AreEqual(actualExpected, healing, 0.001f,
                $"Healing should be max of {expectedHealing} (25% of {damageTaken}) or {minHealing} (10% of {MAX_HEALTH})");
        }

        /// <summary>
        /// Property 16: Death Strike has minimum healing of 10% max health.
        /// </summary>
        [Test]
        public void DeathStrikeHealing_HasMinimum10PercentMaxHealth()
        {
            // Arrange: Apply very little damage
            float smallDamage = 10f;
            _combatSystem.ApplyDamage(TEST_PLAYER_ID, smallDamage, DamageType.Physical, 999);

            // Act
            float healing = _combatSystem.CalculateDeathStrikeHealing(TEST_PLAYER_ID);

            // Assert
            float minHealing = MAX_HEALTH * DEATH_STRIKE_MIN_HEAL_PERCENT;
            Assert.AreEqual(minHealing, healing, 0.001f,
                $"Healing should be minimum {minHealing} (10% of {MAX_HEALTH})");
        }

        /// <summary>
        /// Property 16: Death Strike healing with no recent damage uses minimum.
        /// </summary>
        [Test]
        public void DeathStrikeHealing_WithNoRecentDamage_UsesMinimum()
        {
            // Arrange: No damage applied

            // Act
            float healing = _combatSystem.CalculateDeathStrikeHealing(TEST_PLAYER_ID);

            // Assert
            float minHealing = MAX_HEALTH * DEATH_STRIKE_MIN_HEAL_PERCENT;
            Assert.AreEqual(minHealing, healing, 0.001f,
                $"Healing with no recent damage should be minimum {minHealing}");
        }

        /// <summary>
        /// Property 16: Death Strike healing with high damage uses 25%.
        /// </summary>
        [Test]
        public void DeathStrikeHealing_WithHighDamage_Uses25Percent()
        {
            // Arrange: Apply high damage
            float highDamage = 600f;
            _combatSystem.ApplyDamage(TEST_PLAYER_ID, highDamage, DamageType.Physical, 999);

            // Act
            float healing = _combatSystem.CalculateDeathStrikeHealing(TEST_PLAYER_ID);

            // Assert
            float expectedHealing = highDamage * DEATH_STRIKE_HEAL_PERCENT;
            Assert.AreEqual(expectedHealing, healing, 0.001f,
                $"Healing should be {expectedHealing} (25% of {highDamage})");
        }

        /// <summary>
        /// Property 16: Multiple damage instances are summed.
        /// </summary>
        [Test]
        public void DeathStrikeHealing_SumsMultipleDamageInstances()
        {
            // Arrange: Apply multiple damage instances
            float damage1 = 100f;
            float damage2 = 150f;
            float damage3 = 200f;
            
            _combatSystem.ApplyDamage(TEST_PLAYER_ID, damage1, DamageType.Physical, 999);
            _combatSystem.ApplyDamage(TEST_PLAYER_ID, damage2, DamageType.Fire, 999);
            _combatSystem.ApplyDamage(TEST_PLAYER_ID, damage3, DamageType.Frost, 999);

            // Act
            float recentDamage = _combatSystem.GetRecentDamageTaken(TEST_PLAYER_ID);
            float healing = _combatSystem.CalculateDeathStrikeHealing(TEST_PLAYER_ID);

            // Assert
            float totalDamage = damage1 + damage2 + damage3;
            Assert.AreEqual(totalDamage, recentDamage, 0.001f,
                "Recent damage should sum all instances");
            
            float expectedHealing = totalDamage * DEATH_STRIKE_HEAL_PERCENT;
            Assert.AreEqual(expectedHealing, healing, 0.001f,
                $"Healing should be 25% of total damage ({totalDamage})");
        }

        #endregion

        #region Constants Verification

        /// <summary>
        /// Property 16: Verify CombatSystem constants match requirements.
        /// </summary>
        [Test]
        public void CombatSystemConstants_MatchRequirements()
        {
            Assert.AreEqual(5f, CombatSystem.DEATH_STRIKE_DAMAGE_WINDOW,
                "Damage window should be 5 seconds");
            Assert.AreEqual(0.25f, CombatSystem.DEATH_STRIKE_HEAL_PERCENT,
                "Heal percent should be 25%");
            Assert.AreEqual(0.10f, CombatSystem.DEATH_STRIKE_MIN_HEAL_PERCENT,
                "Min heal percent should be 10%");
        }

        #endregion
    }
}
