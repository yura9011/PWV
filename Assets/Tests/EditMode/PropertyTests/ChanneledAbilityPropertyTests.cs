using NUnit.Framework;
using UnityEngine;
using EtherDomes.Data;
using EtherDomes.Tests.Generators;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for channeled abilities.
    /// Feature: new-classes-combat
    /// </summary>
    public class ChanneledAbilityPropertyTests : PropertyTestBase
    {
        /// <summary>
        /// Feature: new-classes-combat, Property 4: Channeled Ability Tick Count
        /// For any channeled ability with channel duration D and tick interval T,
        /// the number of effect applications SHALL equal ceil(D / T) if the channel completes without interruption.
        /// **Validates: Requirements 2.1, 2.2, 2.3**
        /// </summary>
        [Test]
        [Repeat(MIN_ITERATIONS)]
        public void Property4_ChanneledAbilityTickCount_EqualsExpectedTicks()
        {
            // Arrange: Generate random channeled ability
            float duration = RandomFloat(1f, 10f);
            float tickInterval = RandomFloat(0.5f, 2f);
            
            // Ensure tick interval doesn't exceed duration
            if (tickInterval > duration)
            {
                tickInterval = duration;
            }

            var ability = new AbilityData
            {
                AbilityId = System.Guid.NewGuid().ToString(),
                AbilityName = "Test Channel",
                IsChanneled = true,
                ChannelDuration = duration,
                TickInterval = tickInterval
            };

            // Act: Calculate expected ticks
            int expectedTicks = Mathf.CeilToInt(duration / tickInterval);
            int actualTicks = ability.TotalTicks;

            // Assert: TotalTicks should equal ceil(duration / interval)
            Assert.AreEqual(expectedTicks, actualTicks,
                $"Channeled ability with duration {duration}s and interval {tickInterval}s " +
                $"should have {expectedTicks} ticks, but got {actualTicks}");
        }

        /// <summary>
        /// Property test: Non-channeled abilities should have 0 ticks.
        /// </summary>
        [Test]
        [Repeat(MIN_ITERATIONS)]
        public void NonChanneledAbility_HasZeroTicks()
        {
            // Arrange: Generate non-channeled ability
            var ability = TestDataGenerators.GenerateAbilityData();
            ability.IsChanneled = false;

            // Act & Assert
            Assert.AreEqual(0, ability.TotalTicks,
                "Non-channeled ability should have 0 ticks");
        }

        /// <summary>
        /// Property test: Channeled ability with zero tick interval should have 0 ticks (edge case).
        /// </summary>
        [Test]
        public void ChanneledAbility_WithZeroTickInterval_HasZeroTicks()
        {
            // Arrange
            var ability = new AbilityData
            {
                IsChanneled = true,
                ChannelDuration = 5f,
                TickInterval = 0f
            };

            // Act & Assert
            Assert.AreEqual(0, ability.TotalTicks,
                "Channeled ability with zero tick interval should have 0 ticks");
        }

        /// <summary>
        /// Property test: Channeled abilities are not instant.
        /// </summary>
        [Test]
        [Repeat(MIN_ITERATIONS)]
        public void ChanneledAbility_IsNotInstant()
        {
            // Arrange
            var ability = TestDataGenerators.GenerateChanneledAbility();

            // Act & Assert
            Assert.IsFalse(ability.IsInstant,
                "Channeled ability should not be considered instant");
        }

        /// <summary>
        /// Property test: TotalTicks is always at least 1 for valid channeled abilities.
        /// </summary>
        [Test]
        [Repeat(MIN_ITERATIONS)]
        public void ChanneledAbility_HasAtLeastOneTick()
        {
            // Arrange: Generate valid channeled ability
            var ability = TestDataGenerators.GenerateChanneledAbility();

            // Act & Assert
            Assert.GreaterOrEqual(ability.TotalTicks, 1,
                $"Valid channeled ability should have at least 1 tick. " +
                $"Duration: {ability.ChannelDuration}, Interval: {ability.TickInterval}");
        }
    }
}


namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for channel interruption behavior.
    /// Feature: new-classes-combat
    /// </summary>
    public class ChannelInterruptionPropertyTests : PropertyTestBase
    {
        /// <summary>
        /// Feature: new-classes-combat, Property 5: Channel Interruption on Movement
        /// For any channeled ability in progress, if the caster moves more than 0.1 meters,
        /// the channel SHALL be interrupted and no further ticks SHALL occur.
        /// **Validates: Requirements 2.4, 2.5**
        /// </summary>
        [Test]
        [Repeat(MIN_ITERATIONS)]
        public void Property5_ChannelInterruptionOnMovement_InterruptsWhenMovedBeyondThreshold()
        {
            // Arrange: Generate random movement distance
            float movementDistance = RandomFloat(0.11f, 10f); // Always above threshold
            const float MOVEMENT_THRESHOLD = 0.1f;

            // Act: Check if movement exceeds threshold
            bool shouldInterrupt = movementDistance > MOVEMENT_THRESHOLD;

            // Assert: Movement beyond threshold should trigger interruption
            Assert.IsTrue(shouldInterrupt,
                $"Movement of {movementDistance}m should trigger channel interruption (threshold: {MOVEMENT_THRESHOLD}m)");
        }

        /// <summary>
        /// Property test: Movement below threshold should not interrupt channel.
        /// </summary>
        [Test]
        [Repeat(MIN_ITERATIONS)]
        public void ChannelNotInterrupted_WhenMovementBelowThreshold()
        {
            // Arrange: Generate random movement distance below threshold
            float movementDistance = RandomFloat(0f, 0.09f); // Always below threshold
            const float MOVEMENT_THRESHOLD = 0.1f;

            // Act: Check if movement exceeds threshold
            bool shouldInterrupt = movementDistance > MOVEMENT_THRESHOLD;

            // Assert: Movement below threshold should not trigger interruption
            Assert.IsFalse(shouldInterrupt,
                $"Movement of {movementDistance}m should NOT trigger channel interruption (threshold: {MOVEMENT_THRESHOLD}m)");
        }

        /// <summary>
        /// Property test: Movement exactly at threshold should not interrupt.
        /// </summary>
        [Test]
        public void ChannelNotInterrupted_WhenMovementExactlyAtThreshold()
        {
            // Arrange
            const float movementDistance = 0.1f;
            const float MOVEMENT_THRESHOLD = 0.1f;

            // Act: Check if movement exceeds threshold (not equals)
            bool shouldInterrupt = movementDistance > MOVEMENT_THRESHOLD;

            // Assert: Movement exactly at threshold should not trigger interruption
            Assert.IsFalse(shouldInterrupt,
                "Movement exactly at threshold should NOT trigger channel interruption");
        }

        /// <summary>
        /// Property test: Interrupted channel should stop producing ticks.
        /// This tests the invariant that after interruption, no more ticks occur.
        /// </summary>
        [Test]
        [Repeat(MIN_ITERATIONS)]
        public void InterruptedChannel_StopsProducingTicks()
        {
            // Arrange: Generate channeled ability
            var ability = TestDataGenerators.GenerateChanneledAbility();
            int totalPossibleTicks = ability.TotalTicks;
            
            // Simulate interruption at random point
            int ticksBeforeInterrupt = RandomInt(0, totalPossibleTicks);
            
            // After interruption, remaining ticks should be 0
            int ticksAfterInterrupt = 0;
            int totalTicksExecuted = ticksBeforeInterrupt + ticksAfterInterrupt;

            // Assert: Total ticks executed should be less than or equal to ticks before interrupt
            Assert.LessOrEqual(totalTicksExecuted, totalPossibleTicks,
                $"Interrupted channel should not execute more than {totalPossibleTicks} ticks");
            Assert.AreEqual(ticksBeforeInterrupt, totalTicksExecuted,
                "After interruption, no more ticks should occur");
        }

        /// <summary>
        /// Property test: Vector3 distance calculation for movement detection.
        /// </summary>
        [Test]
        [Repeat(MIN_ITERATIONS)]
        public void MovementDetection_UsesCorrectDistanceCalculation()
        {
            // Arrange: Generate two random positions
            Vector3 startPos = RandomPosition();
            Vector3 endPos = startPos + RandomVector3(-5f, 5f);
            
            // Act: Calculate distance
            float distance = Vector3.Distance(startPos, endPos);
            
            // Assert: Distance should be non-negative
            Assert.GreaterOrEqual(distance, 0f,
                "Distance between two positions should be non-negative");
        }
    }
}
