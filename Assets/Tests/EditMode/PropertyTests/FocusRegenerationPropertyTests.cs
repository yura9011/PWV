using NUnit.Framework;
using EtherDomes.Combat;
using EtherDomes.Data;
using System;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property tests for Energia regeneration rate (Arquero).
    /// Updated for Phase 2 - 8 Classes system.
    /// Energia regenerates at 10/s for Arquero.
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
        /// Energia regenerates at 10 per second for Arquero.
        /// </summary>
        [Test]
        public void Property13_FocusRegeneratesAt5PerSecond()
        {
            RunPropertyTest(() =>
            {
                // Arrange - Energia starts full, spend some first
                _resourceSystem.RegisterResource(TEST_PLAYER_ID, SecondaryResourceType.Energia, 100f);
                _resourceSystem.TrySpendResource(TEST_PLAYER_ID, 100f); // Empty it
                
                float initialEnergia = _resourceSystem.GetResource(TEST_PLAYER_ID);
                Assert.AreEqual(0f, initialEnergia, "Energia should be 0 after spending all");

                // Act - simulate 1 second of regeneration
                float deltaTime = 1f;
                _resourceSystem.ApplyDecay(TEST_PLAYER_ID, deltaTime, inCombat: false);

                // Assert
                float currentEnergia = _resourceSystem.GetResource(TEST_PLAYER_ID);
                float expectedRegen = SecondaryResourceSystem.ENERGIA_REGEN_RATE * deltaTime;
                
                Assert.AreEqual(expectedRegen, currentEnergia, 0.001f,
                    $"Energia should regenerate {expectedRegen} after {deltaTime}s");
            });
        }

        /// <summary>
        /// Energia regeneration is proportional to time elapsed.
        /// </summary>
        [Test]
        public void Property13b_FocusRegenerationIsProportionalToTime()
        {
            RunPropertyTest(() =>
            {
                float[] timeIntervals = { 0.1f, 0.5f, 1f, 2f, 5f };

                foreach (float deltaTime in timeIntervals)
                {
                    // Reset
                    _resourceSystem.ClearAll();
                    _resourceSystem.RegisterResource(TEST_PLAYER_ID, SecondaryResourceType.Energia, 100f);
                    _resourceSystem.TrySpendResource(TEST_PLAYER_ID, 100f); // Empty it

                    // Act
                    _resourceSystem.ApplyDecay(TEST_PLAYER_ID, deltaTime, inCombat: false);

                    // Assert
                    float currentEnergia = _resourceSystem.GetResource(TEST_PLAYER_ID);
                    float expectedRegen = SecondaryResourceSystem.ENERGIA_REGEN_RATE * deltaTime;
                    expectedRegen = Math.Min(expectedRegen, 100f);

                    Assert.AreEqual(expectedRegen, currentEnergia, 0.001f,
                        $"Energia should be {expectedRegen} after {deltaTime}s");
                }
            });
        }

        /// <summary>
        /// Energia does not exceed maximum (100).
        /// </summary>
        [Test]
        public void Property13c_FocusDoesNotExceedMaximum()
        {
            RunPropertyTest(() =>
            {
                // Arrange - starts full
                _resourceSystem.RegisterResource(TEST_PLAYER_ID, SecondaryResourceType.Energia, 100f);

                // Act - regenerate for a very long time (should stay at 100)
                float longTime = 100f;
                _resourceSystem.ApplyDecay(TEST_PLAYER_ID, longTime, inCombat: false);

                // Assert
                float currentEnergia = _resourceSystem.GetResource(TEST_PLAYER_ID);
                float maxEnergia = _resourceSystem.GetMaxResource(TEST_PLAYER_ID);

                Assert.AreEqual(maxEnergia, currentEnergia, 0.001f,
                    "Energia should cap at maximum");
                Assert.AreEqual(SecondaryResourceSystem.ENERGIA_MAX, maxEnergia, 0.001f,
                    "Max energia should be 100");
            });
        }

        /// <summary>
        /// Energia regenerates during combat.
        /// </summary>
        [Test]
        public void Property13d_FocusRegeneratesDuringCombat()
        {
            RunPropertyTest(() =>
            {
                // Arrange
                _resourceSystem.RegisterResource(TEST_PLAYER_ID, SecondaryResourceType.Energia, 100f);
                _resourceSystem.TrySpendResource(TEST_PLAYER_ID, 100f); // Empty it

                // Act - regenerate during combat
                float deltaTime = 2f;
                _resourceSystem.ApplyDecay(TEST_PLAYER_ID, deltaTime, inCombat: true);

                // Assert - Energia should still regenerate in combat
                float currentEnergia = _resourceSystem.GetResource(TEST_PLAYER_ID);
                float expectedRegen = SecondaryResourceSystem.ENERGIA_REGEN_RATE * deltaTime;

                Assert.AreEqual(expectedRegen, currentEnergia, 0.001f,
                    "Energia should regenerate even during combat");
            });
        }

        /// <summary>
        /// Energia can be spent and regenerates back.
        /// </summary>
        [Test]
        public void Property13e_FocusRegeneratesAfterSpending()
        {
            RunPropertyTest(() =>
            {
                // Arrange - starts full
                _resourceSystem.RegisterResource(TEST_PLAYER_ID, SecondaryResourceType.Energia, 100f);
                
                // Spend 50 energia
                bool spent = _resourceSystem.TrySpendResource(TEST_PLAYER_ID, 50f);
                Assert.IsTrue(spent, "Should be able to spend energia");
                Assert.AreEqual(50f, _resourceSystem.GetResource(TEST_PLAYER_ID), 0.001f);

                // Act - regenerate for 2 seconds (should add 20 energia)
                float deltaTime = 2f;
                _resourceSystem.ApplyDecay(TEST_PLAYER_ID, deltaTime, inCombat: false);

                // Assert
                float currentEnergia = _resourceSystem.GetResource(TEST_PLAYER_ID);
                float expectedEnergia = 50f + (SecondaryResourceSystem.ENERGIA_REGEN_RATE * deltaTime);

                Assert.AreEqual(expectedEnergia, currentEnergia, 0.001f,
                    $"Energia should be {expectedEnergia} after spending 50 and regenerating for {deltaTime}s");
            });
        }

        /// <summary>
        /// Energia regen rate constant is correct (10/s).
        /// </summary>
        [Test]
        public void Property13f_FocusRegenRateConstantIsCorrect()
        {
            RunPropertyTest(() =>
            {
                // Assert the constant is set correctly
                Assert.AreEqual(10f, SecondaryResourceSystem.ENERGIA_REGEN_RATE,
                    "ENERGIA_REGEN_RATE should be 10 per second");
                
                Assert.AreEqual(100f, SecondaryResourceSystem.ENERGIA_MAX,
                    "ENERGIA_MAX should be 100");

                // Verify static helper method
                Assert.AreEqual(10f, SecondaryResourceSystem.GetRegenRate(SecondaryResourceType.Energia),
                    "GetRegenRate should return 10 for Energia");

                Assert.IsTrue(SecondaryResourceSystem.DoesResourceRegenerate(SecondaryResourceType.Energia),
                    "Energia should be marked as regenerating resource");
            });
        }

        /// <summary>
        /// Arquero class uses Energia resource.
        /// </summary>
        [Test]
        public void Property13g_HunterClassUsesFocus()
        {
            RunPropertyTest(() =>
            {
                // Assert
                var resourceType = SecondaryResourceSystem.GetResourceTypeForClass(CharacterClass.Arquero);
                Assert.AreEqual(SecondaryResourceType.Energia, resourceType,
                    "Arquero should use Energia as secondary resource");
            });
        }
    }
}
