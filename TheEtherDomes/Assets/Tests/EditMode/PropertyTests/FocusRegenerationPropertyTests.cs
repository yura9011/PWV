using NUnit.Framework;
using EtherDomes.Combat;
using EtherDomes.Data;
using System;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property tests for Focus regeneration rate.
    /// Feature: new-classes-combat, Property 13: Focus Regeneration Rate
    /// Validates: Requirements 6.2
    /// </summary>
    [TestFixture]
    public class FocusRegenerationPropertyTests : PropertyTestBase
    {
        private SecondaryResourceSystem _resourceSystem;
        private const ulong TEST_PLAYER_ID = 1;

        [SetUp]
        public void SetUp()
        {
            var go = new UnityEngine.GameObject("ResourceSystem");
            _resourceSystem = go.AddComponent<SecondaryResourceSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_resourceSystem != null)
            {
                UnityEngine.Object.DestroyImmediate(_resourceSystem.gameObject);
            }
        }

        /// <summary>
        /// Property 13: Focus regenerates at exactly 5 per second.
        /// Requirements 6.2: Focus regenerates at 5/s
        /// </summary>
        [Test]
        public void Property13_FocusRegeneratesAt5PerSecond()
        {
            RunPropertyTest(() =>
            {
                // Arrange
                _resourceSystem.RegisterResource(TEST_PLAYER_ID, SecondaryResourceType.Focus, 100f);
                
                // Start with 0 focus
                float initialFocus = _resourceSystem.GetResource(TEST_PLAYER_ID);
                Assert.AreEqual(0f, initialFocus, "Focus should start at 0");

                // Act - simulate 1 second of regeneration
                float deltaTime = 1f;
                _resourceSystem.ApplyDecay(TEST_PLAYER_ID, deltaTime, inCombat: false);

                // Assert
                float currentFocus = _resourceSystem.GetResource(TEST_PLAYER_ID);
                float expectedRegen = SecondaryResourceSystem.FOCUS_REGEN_RATE * deltaTime;
                
                Assert.AreEqual(expectedRegen, currentFocus, 0.001f,
                    $"Focus should regenerate {expectedRegen} after {deltaTime}s");
            });
        }

        /// <summary>
        /// Property 13b: Focus regeneration is proportional to time elapsed.
        /// </summary>
        [Test]
        public void Property13b_FocusRegenerationIsProportionalToTime()
        {
            RunPropertyTest(() =>
            {
                // Test various time intervals
                float[] timeIntervals = { 0.1f, 0.5f, 1f, 2f, 5f };

                foreach (float deltaTime in timeIntervals)
                {
                    // Reset
                    _resourceSystem.ClearAll();
                    _resourceSystem.RegisterResource(TEST_PLAYER_ID, SecondaryResourceType.Focus, 100f);

                    // Act
                    _resourceSystem.ApplyDecay(TEST_PLAYER_ID, deltaTime, inCombat: false);

                    // Assert
                    float currentFocus = _resourceSystem.GetResource(TEST_PLAYER_ID);
                    float expectedRegen = SecondaryResourceSystem.FOCUS_REGEN_RATE * deltaTime;
                    expectedRegen = Math.Min(expectedRegen, 100f); // Cap at max

                    Assert.AreEqual(expectedRegen, currentFocus, 0.001f,
                        $"Focus should be {expectedRegen} after {deltaTime}s");
                }
            });
        }

        /// <summary>
        /// Property 13c: Focus does not exceed maximum (100).
        /// </summary>
        [Test]
        public void Property13c_FocusDoesNotExceedMaximum()
        {
            RunPropertyTest(() =>
            {
                // Arrange
                _resourceSystem.RegisterResource(TEST_PLAYER_ID, SecondaryResourceType.Focus, 100f);

                // Act - regenerate for a very long time (should cap at 100)
                float longTime = 100f; // Would be 500 focus if uncapped
                _resourceSystem.ApplyDecay(TEST_PLAYER_ID, longTime, inCombat: false);

                // Assert
                float currentFocus = _resourceSystem.GetResource(TEST_PLAYER_ID);
                float maxFocus = _resourceSystem.GetMaxResource(TEST_PLAYER_ID);

                Assert.AreEqual(maxFocus, currentFocus, 0.001f,
                    "Focus should cap at maximum");
                Assert.AreEqual(SecondaryResourceSystem.FOCUS_MAX, maxFocus, 0.001f,
                    "Max focus should be 100");
            });
        }

        /// <summary>
        /// Property 13d: Focus regenerates during combat (unlike some resources).
        /// </summary>
        [Test]
        public void Property13d_FocusRegeneratesDuringCombat()
        {
            RunPropertyTest(() =>
            {
                // Arrange
                _resourceSystem.RegisterResource(TEST_PLAYER_ID, SecondaryResourceType.Focus, 100f);

                // Act - regenerate during combat
                float deltaTime = 2f;
                _resourceSystem.ApplyDecay(TEST_PLAYER_ID, deltaTime, inCombat: true);

                // Assert - Focus should still regenerate in combat
                float currentFocus = _resourceSystem.GetResource(TEST_PLAYER_ID);
                float expectedRegen = SecondaryResourceSystem.FOCUS_REGEN_RATE * deltaTime;

                Assert.AreEqual(expectedRegen, currentFocus, 0.001f,
                    "Focus should regenerate even during combat");
            });
        }

        /// <summary>
        /// Property 13e: Focus can be spent and regenerates back.
        /// </summary>
        [Test]
        public void Property13e_FocusRegeneratesAfterSpending()
        {
            RunPropertyTest(() =>
            {
                // Arrange - start with full focus
                _resourceSystem.RegisterResource(TEST_PLAYER_ID, SecondaryResourceType.Focus, 100f);
                _resourceSystem.AddResource(TEST_PLAYER_ID, 100f);
                
                // Spend 50 focus
                bool spent = _resourceSystem.TrySpendResource(TEST_PLAYER_ID, 50f);
                Assert.IsTrue(spent, "Should be able to spend focus");
                Assert.AreEqual(50f, _resourceSystem.GetResource(TEST_PLAYER_ID), 0.001f);

                // Act - regenerate for 4 seconds (should add 20 focus)
                float deltaTime = 4f;
                _resourceSystem.ApplyDecay(TEST_PLAYER_ID, deltaTime, inCombat: false);

                // Assert
                float currentFocus = _resourceSystem.GetResource(TEST_PLAYER_ID);
                float expectedFocus = 50f + (SecondaryResourceSystem.FOCUS_REGEN_RATE * deltaTime);

                Assert.AreEqual(expectedFocus, currentFocus, 0.001f,
                    $"Focus should be {expectedFocus} after spending 50 and regenerating for {deltaTime}s");
            });
        }

        /// <summary>
        /// Property 13f: Focus regen rate constant is correct.
        /// </summary>
        [Test]
        public void Property13f_FocusRegenRateConstantIsCorrect()
        {
            RunPropertyTest(() =>
            {
                // Assert the constant is set correctly per requirements
                Assert.AreEqual(5f, SecondaryResourceSystem.FOCUS_REGEN_RATE,
                    "FOCUS_REGEN_RATE should be 5 per second (Requirements 6.2)");
                
                Assert.AreEqual(100f, SecondaryResourceSystem.FOCUS_MAX,
                    "FOCUS_MAX should be 100 (Requirements 6.2)");

                // Verify static helper method
                Assert.AreEqual(5f, SecondaryResourceSystem.GetRegenRate(SecondaryResourceType.Focus),
                    "GetRegenRate should return 5 for Focus");

                Assert.IsTrue(SecondaryResourceSystem.DoesResourceRegenerate(SecondaryResourceType.Focus),
                    "Focus should be marked as regenerating resource");
            });
        }

        /// <summary>
        /// Property 13g: Hunter class uses Focus resource.
        /// </summary>
        [Test]
        public void Property13g_HunterClassUsesFocus()
        {
            RunPropertyTest(() =>
            {
                // Assert
                var resourceType = SecondaryResourceSystem.GetResourceTypeForClass(CharacterClass.Hunter);
                Assert.AreEqual(SecondaryResourceType.Focus, resourceType,
                    "Hunter should use Focus as secondary resource");
            });
        }
    }
}
