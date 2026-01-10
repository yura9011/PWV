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

        #region Energy Regeneration Tests (Requirements 5.2)

        /// <summary>
        /// Feature: new-classes-combat, Property 11: Energy Regeneration Rate
        /// *For any* Rogue, Energy SHALL regenerate at 10 per second, capped at 100 maximum.
        /// Validates: Requirements 5.2
        /// </summary>
        [Test]
        [Repeat(100)]
        public void EnergyRegeneration_RegeneratesAt10PerSecond()
        {
            // Arrange
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterResource(playerId, SecondaryResourceType.Energy, SecondaryResourceSystem.ENERGY_MAX);
            
            // Start with random energy between 0 and 90
            float startEnergy = Random.Range(0f, 90f);
            resourceSystem.AddResource(playerId, startEnergy);
            float initialEnergy = resourceSystem.GetResource(playerId);

            // Act - simulate 1 second
            resourceSystem.ApplyDecay(playerId, 1f, inCombat: false);

            // Assert
            float currentEnergy = resourceSystem.GetResource(playerId);
            float expectedEnergy = Mathf.Min(initialEnergy + SecondaryResourceSystem.ENERGY_REGEN_RATE, SecondaryResourceSystem.ENERGY_MAX);
            
            Assert.That(currentEnergy, Is.EqualTo(expectedEnergy).Within(0.01f),
                $"Energy should regenerate at {SecondaryResourceSystem.ENERGY_REGEN_RATE}/s (was {initialEnergy}, expected {expectedEnergy}, got {currentEnergy})");

            // Cleanup
            Object.DestroyImmediate(resourceGO);
        }

        /// <summary>
        /// Property: Energy regenerates even in combat
        /// </summary>
        [Test]
        public void EnergyRegeneration_RegeneratesInCombat()
        {
            // Arrange
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterResource(playerId, SecondaryResourceType.Energy, SecondaryResourceSystem.ENERGY_MAX);
            resourceSystem.AddResource(playerId, 50f);
            float initialEnergy = resourceSystem.GetResource(playerId);

            // Act - simulate 1 second in combat
            resourceSystem.ApplyDecay(playerId, 1f, inCombat: true);

            // Assert
            float currentEnergy = resourceSystem.GetResource(playerId);
            Assert.That(currentEnergy, Is.EqualTo(initialEnergy + SecondaryResourceSystem.ENERGY_REGEN_RATE).Within(0.01f),
                "Energy should regenerate even in combat");

            // Cleanup
            Object.DestroyImmediate(resourceGO);
        }

        /// <summary>
        /// Property: Energy caps at 100
        /// </summary>
        [Test]
        public void EnergyRegeneration_CapsAt100()
        {
            // Arrange
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterResource(playerId, SecondaryResourceType.Energy, SecondaryResourceSystem.ENERGY_MAX);
            resourceSystem.AddResource(playerId, 95f);

            // Act - simulate 1 second (would add 10, but should cap at 100)
            resourceSystem.ApplyDecay(playerId, 1f, inCombat: false);

            // Assert
            float currentEnergy = resourceSystem.GetResource(playerId);
            Assert.That(currentEnergy, Is.EqualTo(SecondaryResourceSystem.ENERGY_MAX),
                "Energy should cap at 100");

            // Cleanup
            Object.DestroyImmediate(resourceGO);
        }

        /// <summary>
        /// Property: Energy default max is 100
        /// </summary>
        [Test]
        public void EnergyDefaultMax_Is100()
        {
            Assert.That(SecondaryResourceSystem.GetDefaultMax(SecondaryResourceType.Energy), 
                Is.EqualTo(100f), "Energy max should be 100");
        }

        /// <summary>
        /// Property: Energy regeneration rate is 10/s
        /// </summary>
        [Test]
        public void EnergyRegenRate_Is10PerSecond()
        {
            Assert.That(SecondaryResourceSystem.GetRegenRate(SecondaryResourceType.Energy), 
                Is.EqualTo(10f), "Energy regen rate should be 10/s");
        }

        #endregion

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

        #region Combo Points Tests (Requirements 5.3, 5.4, 5.5)

        /// <summary>
        /// Feature: new-classes-combat, Property 12: Combo Point Generation and Consumption
        /// *For any* Rogue using a generator ability, combo points SHALL increase by 1 (max 5).
        /// *For any* Rogue using a finisher, all combo points SHALL be consumed and damage SHALL scale.
        /// Validates: Requirements 5.3, 5.4, 5.5
        /// </summary>
        [Test]
        [Repeat(100)]
        public void ComboPoints_GenerationAndConsumption()
        {
            // Arrange
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterComboPoints(playerId);

            // Generate random number of combo points (1-5)
            int pointsToGenerate = Random.Range(1, 6);
            for (int i = 0; i < pointsToGenerate; i++)
            {
                resourceSystem.AddComboPoint(playerId);
            }

            int currentPoints = resourceSystem.GetComboPoints(playerId);
            Assert.That(currentPoints, Is.EqualTo(pointsToGenerate),
                $"Should have {pointsToGenerate} combo points after generating");

            // Act - consume all combo points
            int consumed = resourceSystem.ConsumeAllComboPoints(playerId);
            float damageMultiplier = resourceSystem.CalculateComboPointDamageMultiplier(consumed);

            // Assert
            Assert.That(consumed, Is.EqualTo(pointsToGenerate),
                "Should consume all generated combo points");
            Assert.That(resourceSystem.GetComboPoints(playerId), Is.EqualTo(0),
                "Combo points should be 0 after consumption");
            
            float expectedMultiplier = 1f + (consumed * SecondaryResourceSystem.COMBO_POINT_DAMAGE_MULTIPLIER);
            Assert.That(damageMultiplier, Is.EqualTo(expectedMultiplier).Within(0.001f),
                $"Damage multiplier should be {expectedMultiplier} for {consumed} combo points");

            // Cleanup
            Object.DestroyImmediate(resourceGO);
        }

        /// <summary>
        /// Property: Combo points cap at 5
        /// </summary>
        [Test]
        public void ComboPoints_CapsAt5()
        {
            // Arrange
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterComboPoints(playerId);

            // Act - try to add more than 5 combo points
            for (int i = 0; i < 10; i++)
            {
                resourceSystem.AddComboPoint(playerId);
            }

            // Assert
            Assert.That(resourceSystem.GetComboPoints(playerId), Is.EqualTo(5),
                "Combo points should cap at 5");

            // Cleanup
            Object.DestroyImmediate(resourceGO);
        }

        /// <summary>
        /// Property: Combo points do not regenerate
        /// </summary>
        [Test]
        public void ComboPoints_DoNotRegenerate()
        {
            // Arrange
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterComboPoints(playerId);
            resourceSystem.AddComboPoint(playerId);
            resourceSystem.AddComboPoint(playerId);
            
            int initialPoints = resourceSystem.GetComboPoints(playerId);

            // Act - simulate time passing (combo points should not change)
            resourceSystem.ApplyDecay(playerId, 10f, inCombat: false);

            // Assert
            Assert.That(resourceSystem.GetComboPoints(playerId), Is.EqualTo(initialPoints),
                "Combo points should not change over time");

            // Cleanup
            Object.DestroyImmediate(resourceGO);
        }

        /// <summary>
        /// Property: HasComboPoints returns correct value
        /// </summary>
        [Test]
        public void HasComboPoints_ReturnsCorrectValue()
        {
            // Arrange
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterComboPoints(playerId);

            // Assert - no combo points initially
            Assert.That(resourceSystem.HasComboPoints(playerId), Is.False,
                "Should not have combo points initially");

            // Add a combo point
            resourceSystem.AddComboPoint(playerId);

            // Assert - has combo points now
            Assert.That(resourceSystem.HasComboPoints(playerId), Is.True,
                "Should have combo points after adding one");

            // Consume all
            resourceSystem.ConsumeAllComboPoints(playerId);

            // Assert - no combo points after consumption
            Assert.That(resourceSystem.HasComboPoints(playerId), Is.False,
                "Should not have combo points after consumption");

            // Cleanup
            Object.DestroyImmediate(resourceGO);
        }

        /// <summary>
        /// Property: Damage multiplier scales correctly with combo points
        /// </summary>
        [Test]
        [Repeat(100)]
        public void ComboPoints_DamageMultiplierScalesCorrectly()
        {
            // Arrange
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            int comboPoints = Random.Range(0, 6);

            // Act
            float multiplier = resourceSystem.CalculateComboPointDamageMultiplier(comboPoints);

            // Assert
            float expectedMultiplier = 1f + (comboPoints * SecondaryResourceSystem.COMBO_POINT_DAMAGE_MULTIPLIER);
            Assert.That(multiplier, Is.EqualTo(expectedMultiplier).Within(0.001f),
                $"Multiplier for {comboPoints} combo points should be {expectedMultiplier}");

            // Cleanup
            Object.DestroyImmediate(resourceGO);
        }

        /// <summary>
        /// Property: OnComboPointsChanged fires when combo points change
        /// </summary>
        [Test]
        public void OnComboPointsChanged_FiresOnChange()
        {
            // Arrange
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            bool eventFired = false;
            int receivedCurrent = 0;
            int receivedMax = 0;

            resourceSystem.OnComboPointsChanged += (id, current, max) =>
            {
                eventFired = true;
                receivedCurrent = current;
                receivedMax = max;
            };

            resourceSystem.RegisterComboPoints(playerId);

            // Act
            resourceSystem.AddComboPoint(playerId);

            // Assert
            Assert.That(eventFired, Is.True, "OnComboPointsChanged should fire");
            Assert.That(receivedCurrent, Is.EqualTo(1), "Current should be 1");
            Assert.That(receivedMax, Is.EqualTo(5), "Max should be 5");

            // Cleanup
            Object.DestroyImmediate(resourceGO);
        }

        /// <summary>
        /// Property: Combo points default max is 5
        /// </summary>
        [Test]
        public void ComboPointsDefaultMax_Is5()
        {
            Assert.That(SecondaryResourceSystem.GetDefaultMax(SecondaryResourceType.ComboPoints), 
                Is.EqualTo(5f), "Combo Points max should be 5");
        }

        #endregion
    }
}
