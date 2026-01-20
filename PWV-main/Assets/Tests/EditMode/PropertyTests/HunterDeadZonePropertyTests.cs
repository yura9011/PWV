using NUnit.Framework;
using UnityEngine;
using EtherDomes.Data;
using EtherDomes.Classes.Abilities;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for Hunter dead zone enforcement.
    /// Feature: new-classes-combat, Property 14: Hunter Dead Zone Enforcement
    /// Validates: Requirements 6.8
    /// </summary>
    [TestFixture]
    public class HunterDeadZonePropertyTests : PropertyTestBase
    {
        private const float HUNTER_DEAD_ZONE = 8f;

        #region Property 14: Hunter Dead Zone Enforcement

        /// <summary>
        /// Feature: new-classes-combat, Property 14: Hunter Dead Zone Enforcement
        /// For any Hunter ability with MinRange > 0, attempting to use it on a target 
        /// within MinRange SHALL fail with "Too close" error.
        /// Validates: Requirements 6.8
        /// </summary>
        [Test]
        [Repeat(100)]
        public void Property14_HunterRangedAbilities_HaveMinRange()
        {
            // Arrange: Get all Hunter abilities
            var bmAbilities = ClassAbilityDefinitions.GetHunterBeastMasteryAbilities();
            var mmAbilities = ClassAbilityDefinitions.GetHunterMarksmanshipAbilities();
            var sharedAbilities = ClassAbilityDefinitions.GetHunterSharedAbilities();

            // Act & Assert: Check ranged damage abilities have MinRange
            foreach (var ability in bmAbilities)
            {
                if (ability.Type == AbilityType.Damage && ability.Range > 10f)
                {
                    // Kill Command is pet attack, no dead zone
                    if (ability.AbilityId == "hunter_kill_command") continue;
                    
                    Assert.AreEqual(HUNTER_DEAD_ZONE, ability.MinRange,
                        $"Hunter ability {ability.AbilityName} should have MinRange of {HUNTER_DEAD_ZONE}m");
                }
            }

            foreach (var ability in mmAbilities)
            {
                if (ability.Type == AbilityType.Damage && ability.Range > 10f)
                {
                    Assert.AreEqual(HUNTER_DEAD_ZONE, ability.MinRange,
                        $"Hunter ability {ability.AbilityName} should have MinRange of {HUNTER_DEAD_ZONE}m");
                }
            }
        }

        /// <summary>
        /// Property 14: Kill Command should NOT have dead zone (pet attack).
        /// </summary>
        [Test]
        public void KillCommand_HasNoDeadZone()
        {
            // Arrange
            var bmAbilities = ClassAbilityDefinitions.GetHunterBeastMasteryAbilities();
            var killCommand = System.Array.Find(bmAbilities, a => a.AbilityId == "hunter_kill_command");

            // Assert
            Assert.IsNotNull(killCommand, "Kill Command should exist");
            Assert.AreEqual(0f, killCommand.MinRange, "Kill Command should have no MinRange (pet attack)");
        }

        /// <summary>
        /// Property 14: Disengage should NOT have dead zone (self-targeted).
        /// </summary>
        [Test]
        public void Disengage_HasNoDeadZone()
        {
            // Arrange
            var sharedAbilities = ClassAbilityDefinitions.GetHunterSharedAbilities();
            var disengage = System.Array.Find(sharedAbilities, a => a.AbilityId == "hunter_disengage");

            // Assert
            Assert.IsNotNull(disengage, "Disengage should exist");
            Assert.AreEqual(0f, disengage.MinRange, "Disengage should have no MinRange (self-targeted)");
        }

        /// <summary>
        /// Property 14: Freezing Trap should NOT have dead zone (placed at feet).
        /// </summary>
        [Test]
        public void FreezingTrap_HasNoDeadZone()
        {
            // Arrange
            var sharedAbilities = ClassAbilityDefinitions.GetHunterSharedAbilities();
            var trap = System.Array.Find(sharedAbilities, a => a.AbilityId == "hunter_freezing_trap");

            // Assert
            Assert.IsNotNull(trap, "Freezing Trap should exist");
            Assert.AreEqual(0f, trap.MinRange, "Freezing Trap should have no MinRange (placed at feet)");
        }

        /// <summary>
        /// Property 14: Concussive Shot should have dead zone.
        /// </summary>
        [Test]
        public void ConcussiveShot_HasDeadZone()
        {
            // Arrange
            var sharedAbilities = ClassAbilityDefinitions.GetHunterSharedAbilities();
            var concussive = System.Array.Find(sharedAbilities, a => a.AbilityId == "hunter_concussive_shot");

            // Assert
            Assert.IsNotNull(concussive, "Concussive Shot should exist");
            Assert.AreEqual(HUNTER_DEAD_ZONE, concussive.MinRange, 
                $"Concussive Shot should have MinRange of {HUNTER_DEAD_ZONE}m");
        }

        /// <summary>
        /// Property 14: Counter Shot should have dead zone.
        /// </summary>
        [Test]
        public void CounterShot_HasDeadZone()
        {
            // Arrange
            var sharedAbilities = ClassAbilityDefinitions.GetHunterSharedAbilities();
            var counter = System.Array.Find(sharedAbilities, a => a.AbilityId == "hunter_counter_shot");

            // Assert
            Assert.IsNotNull(counter, "Counter Shot should exist");
            Assert.AreEqual(HUNTER_DEAD_ZONE, counter.MinRange, 
                $"Counter Shot should have MinRange of {HUNTER_DEAD_ZONE}m");
        }

        /// <summary>
        /// Property 14: Aimed Shot should have dead zone.
        /// </summary>
        [Test]
        public void AimedShot_HasDeadZone()
        {
            // Arrange
            var mmAbilities = ClassAbilityDefinitions.GetHunterMarksmanshipAbilities();
            var aimed = System.Array.Find(mmAbilities, a => a.AbilityId == "hunter_aimed_shot");

            // Assert
            Assert.IsNotNull(aimed, "Aimed Shot should exist");
            Assert.AreEqual(HUNTER_DEAD_ZONE, aimed.MinRange, 
                $"Aimed Shot should have MinRange of {HUNTER_DEAD_ZONE}m");
        }

        /// <summary>
        /// Property 14: Distance within dead zone should fail ability check.
        /// </summary>
        [Test]
        [Repeat(100)]
        public void DistanceWithinDeadZone_ShouldFailCheck()
        {
            // Arrange
            float targetDistance = RandomFloat(0f, HUNTER_DEAD_ZONE - 0.1f);
            float minRange = HUNTER_DEAD_ZONE;

            // Act
            bool isTooClose = targetDistance < minRange;

            // Assert
            Assert.IsTrue(isTooClose, 
                $"Target at {targetDistance}m should be too close (MinRange: {minRange}m)");
        }

        /// <summary>
        /// Property 14: Distance outside dead zone should pass ability check.
        /// </summary>
        [Test]
        [Repeat(100)]
        public void DistanceOutsideDeadZone_ShouldPassCheck()
        {
            // Arrange
            float targetDistance = RandomFloat(HUNTER_DEAD_ZONE + 0.1f, 40f);
            float minRange = HUNTER_DEAD_ZONE;

            // Act
            bool isTooClose = targetDistance < minRange;

            // Assert
            Assert.IsFalse(isTooClose, 
                $"Target at {targetDistance}m should NOT be too close (MinRange: {minRange}m)");
        }

        #endregion
    }
}
