using System;
using NUnit.Framework;
using UnityEngine;
using EtherDomes.Combat;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for the Mana System.
    /// </summary>
    [TestFixture]
    public class ManaPropertyTests
    {
        private ManaSystem _manaSystem;
        private const ulong TEST_PLAYER_ID = 1;
        private const float DEFAULT_MAX_MANA = 1000f;

        [SetUp]
        public void SetUp()
        {
            _manaSystem = new ManaSystem();
        }

        /// <summary>
        /// Feature: mvp-10-features, Property 31: Mana Cost Enforcement
        /// For any ability with ManaCost > CurrentMana, TrySpendMana SHALL return false
        /// and ability SHALL NOT execute.
        /// Validates: Requirements 12.3
        /// </summary>
        [Test]
        [Repeat(100)]
        public void ManaCostEnforcement_InsufficientMana_ReturnsFalse()
        {
            // Arrange
            float maxMana = UnityEngine.Random.Range(100f, 1000f);
            float currentMana = UnityEngine.Random.Range(0f, maxMana * 0.5f); // 0-50% mana
            float manaCost = currentMana + UnityEngine.Random.Range(1f, 100f); // Always more than current
            
            _manaSystem.RegisterPlayer(TEST_PLAYER_ID, maxMana);
            
            // Set current mana to a specific value
            float excessMana = _manaSystem.GetCurrentMana(TEST_PLAYER_ID) - currentMana;
            if (excessMana > 0)
            {
                _manaSystem.TrySpendMana(TEST_PLAYER_ID, excessMana);
            }

            float manaBeforeAttempt = _manaSystem.GetCurrentMana(TEST_PLAYER_ID);

            // Act
            bool result = _manaSystem.TrySpendMana(TEST_PLAYER_ID, manaCost);

            // Assert
            Assert.That(result, Is.False, 
                $"TrySpendMana should return false when cost ({manaCost}) > current ({manaBeforeAttempt})");
            Assert.That(_manaSystem.GetCurrentMana(TEST_PLAYER_ID), Is.EqualTo(manaBeforeAttempt),
                "Mana should not change when spend fails");
        }

        /// <summary>
        /// Feature: mvp-10-features, Property 31: Mana Cost Enforcement (positive case)
        /// For any ability with ManaCost <= CurrentMana, TrySpendMana SHALL return true
        /// and deduct the mana cost.
        /// Validates: Requirements 12.2
        /// </summary>
        [Test]
        [Repeat(100)]
        public void ManaCostEnforcement_SufficientMana_ReturnsTrue()
        {
            // Arrange
            float maxMana = UnityEngine.Random.Range(100f, 1000f);
            _manaSystem.RegisterPlayer(TEST_PLAYER_ID, maxMana);
            
            float currentMana = _manaSystem.GetCurrentMana(TEST_PLAYER_ID);
            float manaCost = UnityEngine.Random.Range(1f, currentMana); // Always <= current

            // Act
            bool result = _manaSystem.TrySpendMana(TEST_PLAYER_ID, manaCost);

            // Assert
            Assert.That(result, Is.True, 
                $"TrySpendMana should return true when cost ({manaCost}) <= current ({currentMana})");
            Assert.That(_manaSystem.GetCurrentMana(TEST_PLAYER_ID), 
                Is.EqualTo(currentMana - manaCost).Within(0.001f),
                "Mana should be reduced by the cost amount");
        }

        /// <summary>
        /// Feature: mvp-10-features, Property 32: Mana Regeneration Rates
        /// For any player out of combat, mana regeneration rate SHALL equal 2% of MaxMana per second.
        /// Validates: Requirements 12.4
        /// </summary>
        [Test]
        public void ManaRegeneration_OutOfCombat_TwoPercentPerSecond()
        {
            // Arrange
            _manaSystem.RegisterPlayer(TEST_PLAYER_ID, DEFAULT_MAX_MANA);
            _manaSystem.TrySpendMana(TEST_PLAYER_ID, DEFAULT_MAX_MANA * 0.5f); // Spend 50%
            
            float manaBeforeRegen = _manaSystem.GetCurrentMana(TEST_PLAYER_ID);
            _manaSystem.StartOutOfCombatRegen(TEST_PLAYER_ID);

            // Act - Simulate 1 second of regen
            _manaSystem.UpdateRegen(1f);

            // Assert
            float expectedRegen = DEFAULT_MAX_MANA * _manaSystem.OutOfCombatRegenRate; // 2%
            float actualRegen = _manaSystem.GetCurrentMana(TEST_PLAYER_ID) - manaBeforeRegen;
            
            Assert.That(actualRegen, Is.EqualTo(expectedRegen).Within(0.01f),
                $"Out of combat regen should be {expectedRegen} (2% of {DEFAULT_MAX_MANA})");
            Assert.That(_manaSystem.OutOfCombatRegenRate, Is.EqualTo(0.02f),
                "OutOfCombatRegenRate should be 0.02 (2%)");
        }

        /// <summary>
        /// Feature: mvp-10-features, Property 32: Mana Regeneration Rates
        /// For any player in combat, mana regeneration rate SHALL equal 0.5% of MaxMana per second.
        /// Validates: Requirements 12.5
        /// </summary>
        [Test]
        public void ManaRegeneration_InCombat_HalfPercentPerSecond()
        {
            // Arrange
            _manaSystem.RegisterPlayer(TEST_PLAYER_ID, DEFAULT_MAX_MANA);
            _manaSystem.TrySpendMana(TEST_PLAYER_ID, DEFAULT_MAX_MANA * 0.5f); // Spend 50%
            
            float manaBeforeRegen = _manaSystem.GetCurrentMana(TEST_PLAYER_ID);
            _manaSystem.StartCombatRegen(TEST_PLAYER_ID);

            // Act - Simulate 1 second of regen
            _manaSystem.UpdateRegen(1f);

            // Assert
            float expectedRegen = DEFAULT_MAX_MANA * _manaSystem.InCombatRegenRate; // 0.5%
            float actualRegen = _manaSystem.GetCurrentMana(TEST_PLAYER_ID) - manaBeforeRegen;
            
            Assert.That(actualRegen, Is.EqualTo(expectedRegen).Within(0.01f),
                $"In combat regen should be {expectedRegen} (0.5% of {DEFAULT_MAX_MANA})");
            Assert.That(_manaSystem.InCombatRegenRate, Is.EqualTo(0.005f),
                "InCombatRegenRate should be 0.005 (0.5%)");
        }

        /// <summary>
        /// Property: Mana cannot exceed MaxMana
        /// </summary>
        [Test]
        [Repeat(100)]
        public void RestoreMana_CannotExceedMax()
        {
            // Arrange
            float maxMana = UnityEngine.Random.Range(100f, 1000f);
            _manaSystem.RegisterPlayer(TEST_PLAYER_ID, maxMana);
            
            float restoreAmount = UnityEngine.Random.Range(1f, maxMana * 2f);

            // Act
            _manaSystem.RestoreMana(TEST_PLAYER_ID, restoreAmount);

            // Assert
            Assert.That(_manaSystem.GetCurrentMana(TEST_PLAYER_ID), Is.LessThanOrEqualTo(maxMana),
                "Mana should never exceed MaxMana");
        }

        /// <summary>
        /// Property: Mana cannot go below zero
        /// </summary>
        [Test]
        [Repeat(100)]
        public void SpendMana_CannotGoBelowZero()
        {
            // Arrange
            float maxMana = UnityEngine.Random.Range(100f, 1000f);
            _manaSystem.RegisterPlayer(TEST_PLAYER_ID, maxMana);
            
            // Spend all mana
            _manaSystem.TrySpendMana(TEST_PLAYER_ID, maxMana);

            // Assert
            Assert.That(_manaSystem.GetCurrentMana(TEST_PLAYER_ID), Is.GreaterThanOrEqualTo(0f),
                "Mana should never go below zero");
        }

        /// <summary>
        /// Property: GetManaPercent returns correct percentage
        /// </summary>
        [Test]
        [Repeat(100)]
        public void GetManaPercent_ReturnsCorrectPercentage()
        {
            // Arrange
            float maxMana = UnityEngine.Random.Range(100f, 1000f);
            _manaSystem.RegisterPlayer(TEST_PLAYER_ID, maxMana);
            
            float spendAmount = UnityEngine.Random.Range(0f, maxMana);
            _manaSystem.TrySpendMana(TEST_PLAYER_ID, spendAmount);

            // Act
            float percent = _manaSystem.GetManaPercent(TEST_PLAYER_ID);
            float currentMana = _manaSystem.GetCurrentMana(TEST_PLAYER_ID);
            float expectedPercent = currentMana / maxMana;

            // Assert
            Assert.That(percent, Is.EqualTo(expectedPercent).Within(0.001f),
                "GetManaPercent should return current/max ratio");
            Assert.That(percent, Is.InRange(0f, 1f),
                "Mana percent should be between 0 and 1");
        }

        /// <summary>
        /// Property: OnManaEmpty fires when mana reaches zero
        /// </summary>
        [Test]
        public void OnManaEmpty_FiresWhenManaReachesZero()
        {
            // Arrange
            _manaSystem.RegisterPlayer(TEST_PLAYER_ID, DEFAULT_MAX_MANA);
            bool eventFired = false;
            ulong eventPlayerId = 0;
            
            _manaSystem.OnManaEmpty += (playerId) =>
            {
                eventFired = true;
                eventPlayerId = playerId;
            };

            // Act - Spend all mana
            _manaSystem.TrySpendMana(TEST_PLAYER_ID, DEFAULT_MAX_MANA);

            // Assert
            Assert.That(eventFired, Is.True, "OnManaEmpty should fire when mana reaches zero");
            Assert.That(eventPlayerId, Is.EqualTo(TEST_PLAYER_ID), "Event should contain correct player ID");
        }

        /// <summary>
        /// Property: OnManaChanged fires on mana changes
        /// </summary>
        [Test]
        public void OnManaChanged_FiresOnManaChanges()
        {
            // Arrange
            int changeCount = 0;
            float lastCurrent = 0;
            float lastMax = 0;
            
            _manaSystem.OnManaChanged += (playerId, current, max) =>
            {
                changeCount++;
                lastCurrent = current;
                lastMax = max;
            };

            // Act
            _manaSystem.RegisterPlayer(TEST_PLAYER_ID, DEFAULT_MAX_MANA); // Should fire
            _manaSystem.TrySpendMana(TEST_PLAYER_ID, 100f); // Should fire
            _manaSystem.RestoreMana(TEST_PLAYER_ID, 50f); // Should fire

            // Assert
            Assert.That(changeCount, Is.EqualTo(3), "OnManaChanged should fire 3 times");
            Assert.That(lastCurrent, Is.EqualTo(DEFAULT_MAX_MANA - 100f + 50f).Within(0.01f));
            Assert.That(lastMax, Is.EqualTo(DEFAULT_MAX_MANA));
        }
    }
}
