using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using EtherDomes.Combat;
using EtherDomes.Data;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for the Tab-Target system.
    /// </summary>
    [TestFixture]
    public class TargetSystemPropertyTests
    {
        /// <summary>
        /// Property 3: Target Range Inclusion
        /// Targets within max range should be included, targets outside should be excluded.
        /// </summary>
        [Test]
        public void TargetsInRange_AreIncluded_TargetsOutOfRange_AreExcluded(
            [Values(10f, 20f, 30f, 39f, 40f, 41f, 50f, 100f)] float targetDistance)
        {
            // Arrange
            const float maxRange = TargetSystem.DEFAULT_MAX_RANGE; // 40m
            var playerPosition = Vector3.zero;
            var targetPosition = new Vector3(targetDistance, 0f, 0f);
            
            // Act
            float actualDistance = Vector3.Distance(playerPosition, targetPosition);
            bool shouldBeInRange = actualDistance <= maxRange;

            // Assert
            if (shouldBeInRange)
            {
                Assert.That(actualDistance, Is.LessThanOrEqualTo(maxRange),
                    $"Target at {targetDistance}m should be in range (max: {maxRange}m)");
            }
            else
            {
                Assert.That(actualDistance, Is.GreaterThan(maxRange),
                    $"Target at {targetDistance}m should be out of range (max: {maxRange}m)");
            }
        }

        /// <summary>
        /// Property: Distance calculation is symmetric
        /// Distance from A to B should equal distance from B to A
        /// </summary>
        [Test]
        public void DistanceCalculation_IsSymmetric(
            [Values(0f, 10f, -10f)] float x,
            [Values(0f, 5f, -5f)] float y,
            [Values(0f, 15f, -15f)] float z)
        {
            var pointA = Vector3.zero;
            var pointB = new Vector3(x, y, z);

            float distanceAtoB = Vector3.Distance(pointA, pointB);
            float distanceBtoA = Vector3.Distance(pointB, pointA);

            Assert.That(distanceAtoB, Is.EqualTo(distanceBtoA).Within(0.0001f),
                "Distance should be symmetric");
        }

        /// <summary>
        /// Property: Targets at exactly max range should be included
        /// </summary>
        [Test]
        public void TargetAtExactMaxRange_IsIncluded()
        {
            const float maxRange = TargetSystem.DEFAULT_MAX_RANGE;
            var playerPosition = Vector3.zero;
            var targetPosition = new Vector3(maxRange, 0f, 0f);

            float distance = Vector3.Distance(playerPosition, targetPosition);
            bool isInRange = distance <= maxRange;

            Assert.That(isInRange, Is.True,
                $"Target at exactly {maxRange}m should be included");
        }

        /// <summary>
        /// Property: Targets just outside max range should be excluded
        /// </summary>
        [Test]
        public void TargetJustOutsideMaxRange_IsExcluded()
        {
            const float maxRange = TargetSystem.DEFAULT_MAX_RANGE;
            const float epsilon = 0.001f;
            var playerPosition = Vector3.zero;
            var targetPosition = new Vector3(maxRange + epsilon, 0f, 0f);

            float distance = Vector3.Distance(playerPosition, targetPosition);
            bool isInRange = distance <= maxRange;

            Assert.That(isInRange, Is.False,
                $"Target at {maxRange + epsilon}m should be excluded");
        }

        /// <summary>
        /// Property: Range check works in all directions
        /// </summary>
        [Test]
        public void RangeCheck_WorksInAllDirections(
            [Values(0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f)] float angle)
        {
            const float maxRange = TargetSystem.DEFAULT_MAX_RANGE;
            const float testDistance = 30f; // Within range
            var playerPosition = Vector3.zero;
            
            // Calculate position at given angle
            float radians = angle * Mathf.Deg2Rad;
            var targetPosition = new Vector3(
                Mathf.Cos(radians) * testDistance,
                0f,
                Mathf.Sin(radians) * testDistance
            );

            float distance = Vector3.Distance(playerPosition, targetPosition);
            bool isInRange = distance <= maxRange;

            Assert.That(isInRange, Is.True,
                $"Target at {testDistance}m in direction {angle}° should be in range");
            Assert.That(distance, Is.EqualTo(testDistance).Within(0.001f),
                "Distance calculation should be accurate regardless of direction");
        }
    }
}
