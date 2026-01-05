using NUnit.Framework;
using EtherDomes.Combat;
using EtherDomes.Data;
using UnityEngine;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for the Secondary Resource system.
    /// </summary>
    [TestFixture]
    public class SecondaryResourcePropertyTests
    {
        /// <summary>
        /// Feature: mvp-10-features, Property 26: Class Resource Assignment
        /// Warrior SHALL have Rage, Paladin SHALL have HolyPower.
        /// Validates: Requirements 9.3
        /// </summary>
        [Test]
        public void ClassResourceAssignment_WarriorHasRage()
        {
            // Act
            var resourceType = SecondaryResourceSystem.GetResourceTypeForClass(CharacterClass.Warrior);

            // Assert
            Assert.That(resourceType, Is.EqualTo(SecondaryResourceType.Rage),
                "Warrior should have Rage as secondary resource");
        }

        [Test]
        public void ClassResourceAssignment_PaladinHasHolyPower()
        {
            // Act
            var resourceType = SecondaryResourceSystem.GetResourceTypeForClass(CharacterClass.Paladin);

            // Assert
            Assert.That(resourceType, Is.EqualTo(SecondaryResourceType.HolyPower),
                "Paladin should have HolyPower as secondary resource");
        }

        [Test]
        public void ClassResourceAssignment_MageHasNone()
        {
            // Act
            var resourceType = SecondaryResourceSystem.GetResourceTypeForClass(CharacterClass.Mage);

            // Assert
            Assert.That(resourceType, Is.EqualTo(SecondaryResourceType.None),
                "Mage should have no secondary resource");
        }

        [Test]
        public void ClassResourceAssignment_PriestHasNone()
        {
            // Act
            var resourceType = SecondaryResourceSystem.GetResourceTypeForClass(CharacterClass.Priest);

            // Assert
            Assert.That(resourceType, Is.EqualTo(SecondaryResourceType.None),
                "Priest should have no secondary resource");
        }

        /// <summary>
        /// Property: Rage decays out of combat
        /// </summary>
        [Test]
        public void RageDecay_OutOfCombat_Decays()
        {
            // Arrange
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterResource(playerId, SecondaryResourceType.Rage, 100f);
            resourceSystem.AddResource(playerId, 50f);

            float initialRage = resourceSystem.GetResource(playerId);

            // Act - simulate 1 second out of combat
            resourceSystem.ApplyDecay(playerId, 1f, inCombat: false);

            // Assert
            float currentRage = resourceSystem.GetResource(playerId);
            Assert.That(currentRage, Is.LessThan(initialRage),
                "Rage should decay out of combat");
            Assert.That(currentRage, Is.EqualTo(initialRage - SecondaryResourceSystem.RAGE_DECAY_RATE).Within(0.01f),
                "Rage should decay at correct rate");

            // Cleanup
            Object.DestroyImmediate(resourceGO);
        }

        /// <summary>
        /// Property: Rage does not decay in combat
        /// </summary>
        [Test]
        public void RageDecay_InCombat_DoesNotDecay()
        {
            // Arrange
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterResource(playerId, SecondaryResourceType.Rage, 100f);
            resourceSystem.AddResource(playerId, 50f);

            float initialRage = resourceSystem.GetResource(playerId);

            // Act - simulate 1 second in combat
            resourceSystem.ApplyDecay(playerId, 1f, inCombat: true);

            // Assert
            float currentRage = resourceSystem.GetResource(playerId);
            Assert.That(currentRage, Is.EqualTo(initialRage),
                "Rage should not decay in combat");

            // Cleanup
            Object.DestroyImmediate(resourceGO);
        }

        /// <summary>
        /// Property: Holy Power does not decay
        /// </summary>
        [Test]
        public void HolyPowerDecay_NeverDecays()
        {
            // Arrange
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterResource(playerId, SecondaryResourceType.HolyPower, 5f);
            resourceSystem.AddResource(playerId, 3f);

            float initialPower = resourceSystem.GetResource(playerId);

            // Act - simulate decay out of combat
            resourceSystem.ApplyDecay(playerId, 10f, inCombat: false);

            // Assert
            float currentPower = resourceSystem.GetResource(playerId);
            Assert.That(currentPower, Is.EqualTo(initialPower),
                "Holy Power should never decay");

            // Cleanup
            Object.DestroyImmediate(resourceGO);
        }

        /// <summary>
        /// Property: Resource cannot exceed max
        /// </summary>
        [Test]
        [Repeat(100)]
        public void AddResource_CannotExceedMax()
        {
            // Arrange
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            float maxValue = Random.Range(50f, 200f);
            resourceSystem.RegisterResource(playerId, SecondaryResourceType.Rage, maxValue);

            // Act - try to add more than max
            float addAmount = maxValue * 2;
            resourceSystem.AddResource(playerId, addAmount);

            // Assert
            float current = resourceSystem.GetResource(playerId);
            Assert.That(current, Is.EqualTo(maxValue),
                "Resource should be capped at max value");

            // Cleanup
            Object.DestroyImmediate(resourceGO);
        }

        /// <summary>
        /// Property: Resource cannot go below zero
        /// </summary>
        [Test]
        [Repeat(100)]
        public void SpendResource_CannotGoBelowZero()
        {
            // Arrange
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterResource(playerId, SecondaryResourceType.Rage, 100f);
            
            float initialAmount = Random.Range(10f, 50f);
            resourceSystem.AddResource(playerId, initialAmount);

            // Act - try to spend more than available
            float spendAmount = initialAmount + 10f;
            bool success = resourceSystem.TrySpendResource(playerId, spendAmount);

            // Assert
            Assert.That(success, Is.False, "Should fail to spend more than available");
            Assert.That(resourceSystem.GetResource(playerId), Is.EqualTo(initialAmount),
                "Resource should remain unchanged after failed spend");

            // Cleanup
            Object.DestroyImmediate(resourceGO);
        }

        /// <summary>
        /// Property: TrySpendResource returns true when sufficient
        /// </summary>
        [Test]
        [Repeat(100)]
        public void TrySpendResource_SufficientResource_ReturnsTrue()
        {
            // Arrange
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterResource(playerId, SecondaryResourceType.Rage, 100f);
            
            float initialAmount = Random.Range(50f, 100f);
            resourceSystem.AddResource(playerId, initialAmount);

            float spendAmount = Random.Range(1f, initialAmount);

            // Act
            bool success = resourceSystem.TrySpendResource(playerId, spendAmount);

            // Assert
            Assert.That(success, Is.True, "Should succeed when sufficient resource");
            Assert.That(resourceSystem.GetResource(playerId), 
                Is.EqualTo(initialAmount - spendAmount).Within(0.01f),
                "Resource should be reduced by spend amount");

            // Cleanup
            Object.DestroyImmediate(resourceGO);
        }

        /// <summary>
        /// Property: Default max values are correct
        /// </summary>
        [Test]
        public void DefaultMaxValues_AreCorrect()
        {
            Assert.That(SecondaryResourceSystem.GetDefaultMax(SecondaryResourceType.Rage), 
                Is.EqualTo(100f), "Rage max should be 100");
            Assert.That(SecondaryResourceSystem.GetDefaultMax(SecondaryResourceType.HolyPower), 
                Is.EqualTo(5f), "Holy Power max should be 5");
            Assert.That(SecondaryResourceSystem.GetDefaultMax(SecondaryResourceType.None), 
                Is.EqualTo(0f), "None should have 0 max");
        }

        /// <summary>
        /// Property: DoesResourceDecay returns correct values
        /// </summary>
        [Test]
        public void DoesResourceDecay_ReturnsCorrectValues()
        {
            Assert.That(SecondaryResourceSystem.DoesResourceDecay(SecondaryResourceType.Rage), 
                Is.True, "Rage should decay");
            Assert.That(SecondaryResourceSystem.DoesResourceDecay(SecondaryResourceType.HolyPower), 
                Is.False, "Holy Power should not decay");
            Assert.That(SecondaryResourceSystem.DoesResourceDecay(SecondaryResourceType.None), 
                Is.False, "None should not decay");
        }

        /// <summary>
        /// Property: OnResourceChanged fires when resource changes
        /// </summary>
        [Test]
        public void OnResourceChanged_FiresOnChange()
        {
            // Arrange
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            bool eventFired = false;
            float receivedCurrent = 0;
            float receivedMax = 0;

            resourceSystem.OnResourceChanged += (id, current, max) =>
            {
                eventFired = true;
                receivedCurrent = current;
                receivedMax = max;
            };

            resourceSystem.RegisterResource(playerId, SecondaryResourceType.Rage, 100f);

            // Act
            resourceSystem.AddResource(playerId, 25f);

            // Assert
            Assert.That(eventFired, Is.True, "OnResourceChanged should fire");
            Assert.That(receivedCurrent, Is.EqualTo(25f), "Current should be 25");
            Assert.That(receivedMax, Is.EqualTo(100f), "Max should be 100");

            // Cleanup
            Object.DestroyImmediate(resourceGO);
        }

        /// <summary>
        /// Property: GenerateRageFromDamageDealt adds correct amount
        /// </summary>
        [Test]
        public void GenerateRageFromDamageDealt_AddsCorrectAmount()
        {
            // Arrange
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterResource(playerId, SecondaryResourceType.Rage, 100f);

            // Act
            resourceSystem.GenerateRageFromDamageDealt(playerId, 100f);

            // Assert
            Assert.That(resourceSystem.GetResource(playerId), 
                Is.EqualTo(SecondaryResourceSystem.RAGE_PER_DAMAGE_DEALT),
                "Should add correct rage amount for damage dealt");

            // Cleanup
            Object.DestroyImmediate(resourceGO);
        }

        /// <summary>
        /// Property: GenerateRageFromDamageTaken adds correct amount
        /// </summary>
        [Test]
        public void GenerateRageFromDamageTaken_AddsCorrectAmount()
        {
            // Arrange
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterResource(playerId, SecondaryResourceType.Rage, 100f);

            // Act
            resourceSystem.GenerateRageFromDamageTaken(playerId, 50f);

            // Assert
            Assert.That(resourceSystem.GetResource(playerId), 
                Is.EqualTo(SecondaryResourceSystem.RAGE_PER_DAMAGE_TAKEN),
                "Should add correct rage amount for damage taken");

            // Cleanup
            Object.DestroyImmediate(resourceGO);
        }
    }
}
