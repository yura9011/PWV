using NUnit.Framework;
using EtherDomes.Progression;
using EtherDomes.Data;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for the Soft Cap System.
    /// </summary>
    [TestFixture]
    public class SoftCapPropertyTests
    {
        private SoftCapSystem _softCapSystem;

        [SetUp]
        public void SetUp()
        {
            _softCapSystem = new SoftCapSystem();
        }

        /// <summary>
        /// Feature: mvp-10-features, Property 21: Diminishing Returns Formula
        /// For any raw value > 30%, the effective value SHALL be less than the raw value.
        /// Validates: Requirements 8.2, 8.3
        /// </summary>
        [Test]
        [Repeat(100)]
        public void DiminishingReturns_AboveFirstThreshold_EffectiveIsLessThanRaw()
        {
            // Arrange
            float rawValue = UnityEngine.Random.Range(30.01f, 100f);

            // Act
            float effectiveValue = _softCapSystem.ApplyDiminishingReturns(rawValue);

            // Assert
            Assert.That(effectiveValue, Is.LessThan(rawValue),
                $"Raw value {rawValue}% should result in effective value less than raw");
        }

        /// <summary>
        /// Property 21: Values at or below first threshold have no penalty
        /// </summary>
        [Test]
        [Repeat(100)]
        public void DiminishingReturns_AtOrBelowFirstThreshold_NoPenalty()
        {
            // Arrange
            float rawValue = UnityEngine.Random.Range(0f, 30f);

            // Act
            float effectiveValue = _softCapSystem.ApplyDiminishingReturns(rawValue);

            // Assert
            Assert.That(effectiveValue, Is.EqualTo(rawValue).Within(0.001f),
                $"Raw value {rawValue}% at or below 30% should have no penalty");
        }

        /// <summary>
        /// Property: Hard cap is enforced at 75%
        /// </summary>
        [Test]
        [Repeat(100)]
        public void DiminishingReturns_HardCap_NeverExceeds75Percent()
        {
            // Arrange
            float rawValue = UnityEngine.Random.Range(0f, 200f);

            // Act
            float effectiveValue = _softCapSystem.ApplyDiminishingReturns(rawValue);

            // Assert
            Assert.That(effectiveValue, Is.LessThanOrEqualTo(75f),
                $"Effective value should never exceed 75% hard cap, got {effectiveValue}%");
        }

        /// <summary>
        /// Property: Effective value is monotonically increasing
        /// </summary>
        [Test]
        [Repeat(100)]
        public void DiminishingReturns_MonotonicallyIncreasing()
        {
            // Arrange
            float rawValue1 = UnityEngine.Random.Range(0f, 99f);
            float rawValue2 = rawValue1 + UnityEngine.Random.Range(0.1f, 10f);

            // Act
            float effective1 = _softCapSystem.ApplyDiminishingReturns(rawValue1);
            float effective2 = _softCapSystem.ApplyDiminishingReturns(rawValue2);

            // Assert
            Assert.That(effective2, Is.GreaterThanOrEqualTo(effective1),
                $"Higher raw value ({rawValue2}%) should result in higher or equal effective value");
        }

        /// <summary>
        /// Property: First threshold applies 50% DR
        /// </summary>
        [Test]
        public void DiminishingReturns_FirstThreshold_Applies50PercentDR()
        {
            // Arrange
            float rawValue = 40f; // 10 points above first threshold

            // Act
            float effectiveValue = _softCapSystem.ApplyDiminishingReturns(rawValue);

            // Assert
            // 30 + (10 * 0.5) = 35
            Assert.That(effectiveValue, Is.EqualTo(35f).Within(0.001f),
                "40% raw should equal 35% effective (30 + 10*0.5)");
        }

        /// <summary>
        /// Property: Second threshold applies 75% DR
        /// </summary>
        [Test]
        public void DiminishingReturns_SecondThreshold_Applies75PercentDR()
        {
            // Arrange
            float rawValue = 60f; // 10 points above second threshold

            // Act
            float effectiveValue = _softCapSystem.ApplyDiminishingReturns(rawValue);

            // Assert
            // 30 + (20 * 0.5) + (10 * 0.25) = 30 + 10 + 2.5 = 42.5
            Assert.That(effectiveValue, Is.EqualTo(42.5f).Within(0.001f),
                "60% raw should equal 42.5% effective");
        }

        /// <summary>
        /// Property: Negative values return 0
        /// </summary>
        [Test]
        [Repeat(100)]
        public void DiminishingReturns_NegativeValues_ReturnZero()
        {
            // Arrange
            float rawValue = UnityEngine.Random.Range(-100f, -0.01f);

            // Act
            float effectiveValue = _softCapSystem.ApplyDiminishingReturns(rawValue);

            // Assert
            Assert.That(effectiveValue, Is.EqualTo(0f),
                "Negative raw values should return 0");
        }

        /// <summary>
        /// Property: GetSoftCapInfo returns correct thresholds
        /// </summary>
        [Test]
        public void GetSoftCapInfo_ReturnsCorrectThresholds()
        {
            // Act
            var info = _softCapSystem.GetSoftCapInfo();

            // Assert
            Assert.That(info.FirstThreshold, Is.EqualTo(30f), "FirstThreshold should be 30%");
            Assert.That(info.FirstPenalty, Is.EqualTo(0.5f), "FirstPenalty should be 50%");
            Assert.That(info.SecondThreshold, Is.EqualTo(50f), "SecondThreshold should be 50%");
            Assert.That(info.SecondPenalty, Is.EqualTo(0.75f), "SecondPenalty should be 75%");
            Assert.That(info.HardCap, Is.EqualTo(75f), "HardCap should be 75%");
        }

        /// <summary>
        /// Property: CharacterStats applies soft caps to secondary stats
        /// </summary>
        [Test]
        [Repeat(100)]
        public void CharacterStats_GetEffectiveStats_AppliesSoftCaps()
        {
            // Arrange
            var stats = new CharacterStats();
            float rawCrit = UnityEngine.Random.Range(30.01f, 100f);
            float rawHaste = UnityEngine.Random.Range(30.01f, 100f);
            float rawMastery = UnityEngine.Random.Range(30.01f, 100f);
            
            stats.CritChance = rawCrit;
            stats.Haste = rawHaste;
            stats.Mastery = rawMastery;

            // Act
            float effectiveCrit = stats.GetEffectiveCritChance();
            float effectiveHaste = stats.GetEffectiveHaste();
            float effectiveMastery = stats.GetEffectiveMastery();

            // Assert
            Assert.That(effectiveCrit, Is.LessThan(rawCrit),
                "Effective crit should be less than raw when above threshold");
            Assert.That(effectiveHaste, Is.LessThan(rawHaste),
                "Effective haste should be less than raw when above threshold");
            Assert.That(effectiveMastery, Is.LessThan(rawMastery),
                "Effective mastery should be less than raw when above threshold");
        }

        /// <summary>
        /// Property: CharacterStats effective values never exceed hard cap
        /// </summary>
        [Test]
        [Repeat(100)]
        public void CharacterStats_EffectiveValues_NeverExceedHardCap()
        {
            // Arrange
            var stats = new CharacterStats();
            stats.CritChance = UnityEngine.Random.Range(0f, 200f);
            stats.Haste = UnityEngine.Random.Range(0f, 200f);
            stats.Mastery = UnityEngine.Random.Range(0f, 200f);

            // Act & Assert
            Assert.That(stats.GetEffectiveCritChance(), Is.LessThanOrEqualTo(75f),
                "Effective crit should never exceed 75%");
            Assert.That(stats.GetEffectiveHaste(), Is.LessThanOrEqualTo(75f),
                "Effective haste should never exceed 75%");
            Assert.That(stats.GetEffectiveMastery(), Is.LessThanOrEqualTo(75f),
                "Effective mastery should never exceed 75%");
        }

        /// <summary>
        /// Property: GetEffectiveValue is alias for ApplyDiminishingReturns
        /// </summary>
        [Test]
        [Repeat(100)]
        public void GetEffectiveValue_EqualsApplyDiminishingReturns()
        {
            // Arrange
            float rawValue = UnityEngine.Random.Range(0f, 100f);

            // Act
            float fromApply = _softCapSystem.ApplyDiminishingReturns(rawValue);
            float fromGet = _softCapSystem.GetEffectiveValue(rawValue);

            // Assert
            Assert.That(fromGet, Is.EqualTo(fromApply),
                "GetEffectiveValue should return same result as ApplyDiminishingReturns");
        }
    }
}
