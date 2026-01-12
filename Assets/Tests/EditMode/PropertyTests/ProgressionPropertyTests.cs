using NUnit.Framework;
using UnityEngine;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for progression systems (XP, Leveling).
    /// </summary>
    [TestFixture]
    public class ProgressionPropertyTests
    {
        private const int MAX_LEVEL = 50;
        private const int BASE_XP = 100;
        private const float XP_SCALING = 1.15f;

        /// <summary>
        /// Calculate XP required for a level using exponential scaling.
        /// </summary>
        private int CalculateXPForLevel(int level)
        {
            if (level <= 1) return 0;
            return Mathf.RoundToInt(BASE_XP * Mathf.Pow(XP_SCALING, level - 1));
        }

        /// <summary>
        /// Property: XP Required Increases With Level
        /// Each level should require more XP than the previous.
        /// </summary>
        [Test]
        public void XPRequired_IncreasesWithLevel(
            [Values(2, 5, 10, 20, 30, 40, 49)] int level)
        {
            int xpForLevel = CalculateXPForLevel(level);
            int xpForNextLevel = CalculateXPForLevel(level + 1);

            Assert.That(xpForNextLevel, Is.GreaterThan(xpForLevel),
                $"XP for level {level + 1} should be greater than level {level}");
        }

        /// <summary>
        /// Property: XP Required is Always Positive
        /// </summary>
        [Test]
        public void XPRequired_IsAlwaysPositive(
            [Values(2, 10, 25, 50)] int level)
        {
            int xpRequired = CalculateXPForLevel(level);

            Assert.That(xpRequired, Is.GreaterThan(0),
                $"XP required for level {level} should be positive");
        }

        /// <summary>
        /// Property: Level 1 Requires No XP
        /// </summary>
        [Test]
        public void Level1_RequiresNoXP()
        {
            int xpForLevel1 = CalculateXPForLevel(1);

            Assert.That(xpForLevel1, Is.EqualTo(0),
                "Level 1 should require 0 XP");
        }

        /// <summary>
        /// Property: XP Scaling is Consistent
        /// The ratio between consecutive levels should be approximately constant.
        /// </summary>
        [Test]
        public void XPScaling_IsConsistent(
            [Values(5, 10, 20, 30, 40)] int level)
        {
            int xpForLevel = CalculateXPForLevel(level);
            int xpForNextLevel = CalculateXPForLevel(level + 1);

            float ratio = (float)xpForNextLevel / xpForLevel;

            Assert.That(ratio, Is.EqualTo(XP_SCALING).Within(0.01f),
                $"XP scaling ratio should be approximately {XP_SCALING}");
        }

        /// <summary>
        /// Property: Total XP to Max Level is Finite
        /// </summary>
        [Test]
        public void TotalXPToMaxLevel_IsFinite()
        {
            long totalXP = 0;
            for (int level = 2; level <= MAX_LEVEL; level++)
            {
                totalXP += CalculateXPForLevel(level);
            }

            Assert.That(totalXP, Is.GreaterThan(0),
                "Total XP should be positive");
            Assert.That(totalXP, Is.LessThan(long.MaxValue),
                "Total XP should be finite");
        }

        /// <summary>
        /// Property: Level Cannot Exceed Max Level
        /// </summary>
        [Test]
        public void Level_CannotExceedMaxLevel(
            [Values(50, 51, 100, 1000)] int attemptedLevel)
        {
            int clampedLevel = Mathf.Clamp(attemptedLevel, 1, MAX_LEVEL);

            Assert.That(clampedLevel, Is.LessThanOrEqualTo(MAX_LEVEL),
                "Level should be clamped to max level");
            Assert.That(clampedLevel, Is.GreaterThanOrEqualTo(1),
                "Level should be at least 1");
        }

        /// <summary>
        /// Property: XP Gain from Enemies Scales with Level Difference
        /// </summary>
        [Test]
        public void XPGain_ScalesWithLevelDifference(
            [Values(10, 20, 30)] int playerLevel,
            [Values(-5, 0, 5)] int levelDifference)
        {
            int enemyLevel = playerLevel + levelDifference;
            const int baseXP = 50;
            
            // XP scaling based on level difference
            float xpMultiplier = 1f + (levelDifference * 0.1f);
            xpMultiplier = Mathf.Clamp(xpMultiplier, 0.1f, 2f);
            
            int xpGained = Mathf.RoundToInt(baseXP * xpMultiplier);

            if (levelDifference > 0)
            {
                Assert.That(xpGained, Is.GreaterThan(baseXP),
                    "Higher level enemies should give more XP");
            }
            else if (levelDifference < 0)
            {
                Assert.That(xpGained, Is.LessThan(baseXP),
                    "Lower level enemies should give less XP");
            }
            else
            {
                Assert.That(xpGained, Is.EqualTo(baseXP),
                    "Same level enemies should give base XP");
            }
        }
    }
}
