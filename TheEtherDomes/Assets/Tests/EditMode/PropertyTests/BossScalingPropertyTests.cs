using NUnit.Framework;
using EtherDomes.Enemy;
using UnityEngine;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for boss HP scaling.
    /// </summary>
    [TestFixture]
    public class BossScalingPropertyTests
    {
        /// <summary>
        /// Feature: mvp-10-features, Property 30: HP Scaling Per Player
        /// Boss HP SHALL scale as: HP = BaseHealth * (1 + (N-1) * 0.2)
        /// Validates: Requirements 10.7
        /// </summary>
        [Test]
        [Repeat(100)]
        public void HPScaling_FollowsFormula()
        {
            // Arrange
            int playerCount = Random.Range(1, 11); // 1-10 players

            // Act
            float multiplier = BossAI.CalculateHPScaling(playerCount);

            // Assert
            float expected = 1f + (playerCount - 1) * 0.2f;
            Assert.That(multiplier, Is.EqualTo(expected).Within(0.001f),
                $"HP scaling for {playerCount} players should be {expected}");
        }

        /// <summary>
        /// Property: 1 player = 1.0x multiplier
        /// </summary>
        [Test]
        public void HPScaling_OnePlayer_NoScaling()
        {
            // Act
            float multiplier = BossAI.CalculateHPScaling(1);

            // Assert
            Assert.That(multiplier, Is.EqualTo(1.0f),
                "1 player should have 1.0x HP multiplier");
        }

        /// <summary>
        /// Property: 5 players = 1.8x multiplier
        /// </summary>
        [Test]
        public void HPScaling_FivePlayers_CorrectMultiplier()
        {
            // Act
            float multiplier = BossAI.CalculateHPScaling(5);

            // Assert
            // 1 + (5-1) * 0.2 = 1 + 0.8 = 1.8
            Assert.That(multiplier, Is.EqualTo(1.8f).Within(0.001f),
                "5 players should have 1.8x HP multiplier");
        }

        /// <summary>
        /// Property: 10 players = 2.8x multiplier
        /// </summary>
        [Test]
        public void HPScaling_TenPlayers_CorrectMultiplier()
        {
            // Act
            float multiplier = BossAI.CalculateHPScaling(10);

            // Assert
            // 1 + (10-1) * 0.2 = 1 + 1.8 = 2.8
            Assert.That(multiplier, Is.EqualTo(2.8f).Within(0.001f),
                "10 players should have 2.8x HP multiplier");
        }

        /// <summary>
        /// Property: Scaling is linear with player count
        /// </summary>
        [Test]
        public void HPScaling_IsLinear()
        {
            // Arrange
            float[] multipliers = new float[10];
            for (int i = 1; i <= 10; i++)
            {
                multipliers[i - 1] = BossAI.CalculateHPScaling(i);
            }

            // Assert - each step should increase by 0.2
            for (int i = 1; i < 10; i++)
            {
                float diff = multipliers[i] - multipliers[i - 1];
                Assert.That(diff, Is.EqualTo(0.2f).Within(0.001f),
                    $"Scaling should increase by 0.2 per player (step {i} to {i + 1})");
            }
        }

        /// <summary>
        /// Property: BossAI scales correctly when initialized
        /// </summary>
        [Test]
        public void BossAI_ScaleForGroupSize_AppliesCorrectly()
        {
            // Arrange
            var bossGO = new GameObject("Boss");
            var bossAI = bossGO.AddComponent<BossAI>();
            
            // Use reflection to set base health since it's serialized
            var baseHealthField = typeof(BossAI).GetField("_baseHealth", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            baseHealthField.SetValue(bossAI, 10000f);

            bossAI.Initialize();

            // Act
            bossAI.ScaleForGroupSize(5);

            // Assert
            // 10000 * 1.8 = 18000
            Assert.That(bossAI.MaxHealth, Is.EqualTo(18000f).Within(1f),
                "Boss HP should be scaled for 5 players");

            // Cleanup
            Object.DestroyImmediate(bossGO);
        }

        /// <summary>
        /// Property: Multiplier is always >= 1.0
        /// </summary>
        [Test]
        [Repeat(100)]
        public void HPScaling_AlwaysAtLeastOne()
        {
            // Arrange
            int playerCount = Random.Range(1, 100);

            // Act
            float multiplier = BossAI.CalculateHPScaling(playerCount);

            // Assert
            Assert.That(multiplier, Is.GreaterThanOrEqualTo(1.0f),
                "HP multiplier should always be >= 1.0");
        }
    }
}
