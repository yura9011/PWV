using System;
using NUnit.Framework;
using UnityEngine;
using EtherDomes.Combat;
using EtherDomes.Data;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for the Diminishing Returns System.
    /// Feature: new-classes-combat, Property 17: Diminishing Returns on CC
    /// Validates: Requirements 11.6, 11.7
    /// </summary>
    [TestFixture]
    public class DiminishingReturnsPropertyTests : PropertyTestBase
    {
        private DiminishingReturnsSystem _drSystem;
        private const ulong TEST_TARGET_ID = 1;

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("DiminishingReturnsSystem");
            _drSystem = go.AddComponent<DiminishingReturnsSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_drSystem != null)
            {
                UnityEngine.Object.DestroyImmediate(_drSystem.gameObject);
            }
        }

        #region Helper Methods

        private CCType GetRandomCCType()
        {
            // Exclude None from random selection
            CCType[] ccTypes = { CCType.Slow, CCType.Stun, CCType.Fear, CCType.Root };
            return ccTypes[UnityEngine.Random.Range(0, ccTypes.Length)];
        }

        #endregion

        #region Property 17: Diminishing Returns on CC

        /// <summary>
        /// Feature: new-classes-combat, Property 17: Diminishing Returns on CC
        /// For any CC effect applied to a target, the effective duration SHALL be:
        /// - 100% for 1st application
        /// - 50% for 2nd application
        /// - 25% for 3rd application
        /// After 3 applications of the same CC type within 15 seconds, 
        /// the target SHALL be immune for 15 seconds.
        /// Validates: Requirements 11.6, 11.7
        /// </summary>
        [Test]
        [Repeat(100)]
        public void DiminishingReturns_FirstApplication_Returns100Percent()
        {
            // Arrange
            ulong targetId = RandomULong(1, 1000);
            CCType ccType = GetRandomCCType();
            float baseDuration = RandomFloat(2f, 10f);
            
            // Clear any existing DR
            _drSystem.ClearDR(targetId);

            // Act
            float effectiveDuration = _drSystem.ApplyDiminishingReturns(targetId, ccType, baseDuration);

            // Assert - First application should be 100%
            Assert.That(effectiveDuration, Is.EqualTo(baseDuration).Within(0.001f),
                $"First application of {ccType} should have 100% duration ({baseDuration}s), got {effectiveDuration}s");
        }

        /// <summary>
        /// Feature: new-classes-combat, Property 17: Diminishing Returns on CC
        /// Second application should be 50% duration.
        /// Validates: Requirements 11.6
        /// </summary>
        [Test]
        [Repeat(100)]
        public void DiminishingReturns_SecondApplication_Returns50Percent()
        {
            // Arrange
            ulong targetId = RandomULong(1, 1000);
            CCType ccType = GetRandomCCType();
            float baseDuration = RandomFloat(2f, 10f);
            
            // Clear any existing DR
            _drSystem.ClearDR(targetId);

            // Apply first CC
            _drSystem.ApplyDiminishingReturns(targetId, ccType, baseDuration);

            // Act - Apply second CC
            float effectiveDuration = _drSystem.ApplyDiminishingReturns(targetId, ccType, baseDuration);

            // Assert - Second application should be 50%
            float expectedDuration = baseDuration * 0.5f;
            Assert.That(effectiveDuration, Is.EqualTo(expectedDuration).Within(0.001f),
                $"Second application of {ccType} should have 50% duration ({expectedDuration}s), got {effectiveDuration}s");
        }

        /// <summary>
        /// Feature: new-classes-combat, Property 17: Diminishing Returns on CC
        /// Third application should be 25% duration.
        /// Validates: Requirements 11.6
        /// </summary>
        [Test]
        [Repeat(100)]
        public void DiminishingReturns_ThirdApplication_Returns25Percent()
        {
            // Arrange
            ulong targetId = RandomULong(1, 1000);
            CCType ccType = GetRandomCCType();
            float baseDuration = RandomFloat(2f, 10f);
            
            // Clear any existing DR
            _drSystem.ClearDR(targetId);

            // Apply first and second CC
            _drSystem.ApplyDiminishingReturns(targetId, ccType, baseDuration);
            _drSystem.ApplyDiminishingReturns(targetId, ccType, baseDuration);

            // Act - Apply third CC
            float effectiveDuration = _drSystem.ApplyDiminishingReturns(targetId, ccType, baseDuration);

            // Assert - Third application should be 25%
            float expectedDuration = baseDuration * 0.25f;
            Assert.That(effectiveDuration, Is.EqualTo(expectedDuration).Within(0.001f),
                $"Third application of {ccType} should have 25% duration ({expectedDuration}s), got {effectiveDuration}s");
        }

        /// <summary>
        /// Feature: new-classes-combat, Property 17: Diminishing Returns on CC
        /// After 3 applications, target becomes immune (0% duration).
        /// Validates: Requirements 11.7
        /// </summary>
        [Test]
        [Repeat(100)]
        public void DiminishingReturns_AfterThreeApplications_TargetIsImmune()
        {
            // Arrange
            ulong targetId = RandomULong(1, 1000);
            CCType ccType = GetRandomCCType();
            float baseDuration = RandomFloat(2f, 10f);
            
            // Clear any existing DR
            _drSystem.ClearDR(targetId);

            // Apply three CCs to trigger immunity
            _drSystem.ApplyDiminishingReturns(targetId, ccType, baseDuration);
            _drSystem.ApplyDiminishingReturns(targetId, ccType, baseDuration);
            _drSystem.ApplyDiminishingReturns(targetId, ccType, baseDuration);

            // Act - Try to apply fourth CC
            float effectiveDuration = _drSystem.ApplyDiminishingReturns(targetId, ccType, baseDuration);

            // Assert - Should be immune (0 duration)
            Assert.That(effectiveDuration, Is.EqualTo(0f),
                $"After 3 applications, {ccType} should have 0% duration (immune), got {effectiveDuration}s");
            Assert.That(_drSystem.IsImmune(targetId, ccType), Is.True,
                $"Target should be immune to {ccType} after 3 applications");
        }

        /// <summary>
        /// Feature: new-classes-combat, Property 17: Diminishing Returns on CC
        /// Immunity should last 15 seconds.
        /// Validates: Requirements 11.7
        /// </summary>
        [Test]
        public void DiminishingReturns_ImmunityLasts15Seconds()
        {
            // Arrange
            ulong targetId = 1;
            CCType ccType = CCType.Stun;
            float baseDuration = 5f;
            
            // Clear any existing DR
            _drSystem.ClearDR(targetId);

            // Apply three CCs to trigger immunity
            _drSystem.ApplyDiminishingReturns(targetId, ccType, baseDuration);
            _drSystem.ApplyDiminishingReturns(targetId, ccType, baseDuration);
            _drSystem.ApplyDiminishingReturns(targetId, ccType, baseDuration);

            // Assert - Should be immune
            Assert.That(_drSystem.IsImmune(targetId, ccType), Is.True);
            Assert.That(_drSystem.GetImmunityRemaining(targetId, ccType), Is.EqualTo(15f).Within(0.001f),
                "Immunity should last 15 seconds");
        }

        /// <summary>
        /// Feature: new-classes-combat, Property 17: Diminishing Returns on CC
        /// Different CC types should have independent DR tracking.
        /// Validates: Requirements 11.6
        /// </summary>
        [Test]
        [Repeat(100)]
        public void DiminishingReturns_DifferentCCTypes_HaveIndependentDR()
        {
            // Arrange
            ulong targetId = RandomULong(1, 1000);
            CCType ccType1 = CCType.Stun;
            CCType ccType2 = CCType.Fear;
            float baseDuration = RandomFloat(2f, 10f);
            
            // Clear any existing DR
            _drSystem.ClearDR(targetId);

            // Apply stun twice (should be at 50% DR)
            _drSystem.ApplyDiminishingReturns(targetId, ccType1, baseDuration);
            _drSystem.ApplyDiminishingReturns(targetId, ccType1, baseDuration);

            // Act - Apply fear (should be at 100% - no DR)
            float fearDuration = _drSystem.ApplyDiminishingReturns(targetId, ccType2, baseDuration);

            // Assert - Fear should have full duration (independent DR)
            Assert.That(fearDuration, Is.EqualTo(baseDuration).Within(0.001f),
                $"Fear should have 100% duration (independent from Stun DR), got {fearDuration}s");
        }

        /// <summary>
        /// Feature: new-classes-combat, Property 17: Diminishing Returns on CC
        /// DR should reset after 15 seconds without that CC type.
        /// Validates: Requirements 11.6
        /// </summary>
        [Test]
        public void DiminishingReturns_ResetsAfter15SecondsWithoutCC()
        {
            // Arrange
            ulong targetId = 1;
            CCType ccType = CCType.Stun;
            float baseDuration = 5f;
            
            // Clear any existing DR
            _drSystem.ClearDR(targetId);

            // Apply one CC
            _drSystem.ApplyDiminishingReturns(targetId, ccType, baseDuration);
            
            // Verify DR level is 1
            Assert.That(_drSystem.GetDRLevel(targetId, ccType), Is.EqualTo(1));

            // Simulate 15 seconds passing
            _drSystem.Update(15.1f);

            // Act - Apply CC again
            float effectiveDuration = _drSystem.ApplyDiminishingReturns(targetId, ccType, baseDuration);

            // Assert - Should be back to 100% (DR reset)
            Assert.That(effectiveDuration, Is.EqualTo(baseDuration).Within(0.001f),
                $"After 15s without CC, duration should reset to 100%, got {effectiveDuration}s");
        }

        /// <summary>
        /// Feature: new-classes-combat, Property 17: Diminishing Returns on CC
        /// Immunity should expire after 15 seconds.
        /// Validates: Requirements 11.7
        /// </summary>
        [Test]
        public void DiminishingReturns_ImmunityExpiresAfter15Seconds()
        {
            // Arrange
            ulong targetId = 1;
            CCType ccType = CCType.Stun;
            float baseDuration = 5f;
            
            // Clear any existing DR
            _drSystem.ClearDR(targetId);

            // Apply three CCs to trigger immunity
            _drSystem.ApplyDiminishingReturns(targetId, ccType, baseDuration);
            _drSystem.ApplyDiminishingReturns(targetId, ccType, baseDuration);
            _drSystem.ApplyDiminishingReturns(targetId, ccType, baseDuration);

            // Verify immunity
            Assert.That(_drSystem.IsImmune(targetId, ccType), Is.True);

            // Simulate 15 seconds passing
            _drSystem.Update(15.1f);

            // Assert - Immunity should have expired
            Assert.That(_drSystem.IsImmune(targetId, ccType), Is.False,
                "Immunity should expire after 15 seconds");

            // Act - Apply CC again (should work at 100%)
            float effectiveDuration = _drSystem.ApplyDiminishingReturns(targetId, ccType, baseDuration);
            Assert.That(effectiveDuration, Is.EqualTo(baseDuration).Within(0.001f),
                "After immunity expires, CC should work at 100% again");
        }

        /// <summary>
        /// Property 17: DR level query returns correct values.
        /// </summary>
        [Test]
        public void DiminishingReturns_GetDRLevel_ReturnsCorrectValues()
        {
            // Arrange
            ulong targetId = 1;
            CCType ccType = CCType.Stun;
            float baseDuration = 5f;
            
            // Clear any existing DR
            _drSystem.ClearDR(targetId);

            // Assert initial state
            Assert.That(_drSystem.GetDRLevel(targetId, ccType), Is.EqualTo(0), "Initial DR level should be 0");

            // Apply first CC
            _drSystem.ApplyDiminishingReturns(targetId, ccType, baseDuration);
            Assert.That(_drSystem.GetDRLevel(targetId, ccType), Is.EqualTo(1), "After 1st CC, DR level should be 1");

            // Apply second CC
            _drSystem.ApplyDiminishingReturns(targetId, ccType, baseDuration);
            Assert.That(_drSystem.GetDRLevel(targetId, ccType), Is.EqualTo(2), "After 2nd CC, DR level should be 2");

            // Apply third CC (triggers immunity)
            _drSystem.ApplyDiminishingReturns(targetId, ccType, baseDuration);
            Assert.That(_drSystem.GetDRLevel(targetId, ccType), Is.EqualTo(3), "After 3rd CC, DR level should be 3 (immune)");
        }

        /// <summary>
        /// Property 17: Duration multiplier returns correct values.
        /// </summary>
        [Test]
        public void DiminishingReturns_GetDurationMultiplier_ReturnsCorrectValues()
        {
            // Arrange
            ulong targetId = 1;
            CCType ccType = CCType.Stun;
            float baseDuration = 5f;
            
            // Clear any existing DR
            _drSystem.ClearDR(targetId);

            // Assert initial state
            Assert.That(_drSystem.GetDurationMultiplier(targetId, ccType), Is.EqualTo(1.0f), "Initial multiplier should be 1.0");

            // Apply first CC
            _drSystem.ApplyDiminishingReturns(targetId, ccType, baseDuration);
            Assert.That(_drSystem.GetDurationMultiplier(targetId, ccType), Is.EqualTo(0.5f), "After 1st CC, multiplier should be 0.5");

            // Apply second CC
            _drSystem.ApplyDiminishingReturns(targetId, ccType, baseDuration);
            Assert.That(_drSystem.GetDurationMultiplier(targetId, ccType), Is.EqualTo(0.25f), "After 2nd CC, multiplier should be 0.25");

            // Apply third CC (triggers immunity)
            _drSystem.ApplyDiminishingReturns(targetId, ccType, baseDuration);
            Assert.That(_drSystem.GetDurationMultiplier(targetId, ccType), Is.EqualTo(0f), "After 3rd CC, multiplier should be 0 (immune)");
        }

        /// <summary>
        /// Property 17: ClearDR removes all tracking for an entity.
        /// </summary>
        [Test]
        public void DiminishingReturns_ClearDR_RemovesAllTracking()
        {
            // Arrange
            ulong targetId = 1;
            CCType ccType = CCType.Stun;
            float baseDuration = 5f;
            
            // Apply some CCs
            _drSystem.ApplyDiminishingReturns(targetId, ccType, baseDuration);
            _drSystem.ApplyDiminishingReturns(targetId, ccType, baseDuration);
            Assert.That(_drSystem.GetDRLevel(targetId, ccType), Is.EqualTo(2));

            // Act
            _drSystem.ClearDR(targetId);

            // Assert
            Assert.That(_drSystem.GetDRLevel(targetId, ccType), Is.EqualTo(0), "DR level should be 0 after clear");
            Assert.That(_drSystem.IsImmune(targetId, ccType), Is.False, "Should not be immune after clear");
        }

        #endregion
    }
}
