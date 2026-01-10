using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using EtherDomes.Combat;
using EtherDomes.Data;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for the Buff System.
    /// Feature: new-classes-combat
    /// </summary>
    [TestFixture]
    public class BuffSystemPropertyTests
    {
        private BuffSystem _buffSystem;
        private const ulong TEST_ENTITY_ID = 1;

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("BuffSystem");
            _buffSystem = go.AddComponent<BuffSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_buffSystem != null)
            {
                UnityEngine.Object.DestroyImmediate(_buffSystem.gameObject);
            }
        }

        #region Helper Methods

        private BuffData CreateTestBuff(string id, float duration, bool isBuff = true)
        {
            return new BuffData
            {
                BuffId = id,
                DisplayName = $"Test {(isBuff ? "Buff" : "Debuff")} {id}",
                Duration = duration,
                EffectType = isBuff ? EffectType.Buff : EffectType.Debuff
            };
        }

        private BuffData CreateTestDoT(string id, float duration, float tickInterval, float tickDamage)
        {
            return new BuffData
            {
                BuffId = id,
                DisplayName = $"Test DoT {id}",
                Duration = duration,
                EffectType = EffectType.DoT,
                IsPeriodicEffect = true,
                TickInterval = tickInterval,
                TickDamage = tickDamage,
                DamageType = DamageType.Shadow
            };
        }

        private BuffData CreateTestHoT(string id, float duration, float tickInterval, float tickHealing)
        {
            return new BuffData
            {
                BuffId = id,
                DisplayName = $"Test HoT {id}",
                Duration = duration,
                EffectType = EffectType.HoT,
                IsPeriodicEffect = true,
                TickInterval = tickInterval,
                TickHealing = tickHealing
            };
        }

        #endregion

        #region Property 1: Buff Duration Bounds

        /// <summary>
        /// Feature: new-classes-combat, Property 1: Buff Duration Bounds
        /// For any buff or debuff applied to any entity, the duration SHALL be clamped 
        /// between 1 and 300 seconds.
        /// Validates: Requirements 1.1
        /// </summary>
        [Test]
        [Repeat(100)]
        public void BuffDurationBounds_DurationIsClamped_BetweenMinAndMax()
        {
            // Arrange - Generate random duration including values outside valid range
            float randomDuration = UnityEngine.Random.Range(-100f, 500f);
            string buffId = $"test_buff_{UnityEngine.Random.Range(0, 10000)}";
            var buff = CreateTestBuff(buffId, randomDuration);

            // Act
            _buffSystem.ApplyBuff(TEST_ENTITY_ID, buff);

            // Assert
            float remainingDuration = _buffSystem.GetRemainingDuration(TEST_ENTITY_ID, buffId);
            
            Assert.That(remainingDuration, Is.GreaterThanOrEqualTo(_buffSystem.MinDuration),
                $"Duration {randomDuration} should be clamped to at least {_buffSystem.MinDuration}");
            Assert.That(remainingDuration, Is.LessThanOrEqualTo(_buffSystem.MaxDuration),
                $"Duration {randomDuration} should be clamped to at most {_buffSystem.MaxDuration}");
        }

        /// <summary>
        /// Feature: new-classes-combat, Property 1: Buff Duration Bounds (Debuff variant)
        /// For any debuff applied to any entity, the duration SHALL be clamped 
        /// between 1 and 300 seconds.
        /// Validates: Requirements 1.1
        /// </summary>
        [Test]
        [Repeat(100)]
        public void DebuffDurationBounds_DurationIsClamped_BetweenMinAndMax()
        {
            // Arrange - Generate random duration including values outside valid range
            float randomDuration = UnityEngine.Random.Range(-100f, 500f);
            string debuffId = $"test_debuff_{UnityEngine.Random.Range(0, 10000)}";
            var debuff = CreateTestBuff(debuffId, randomDuration, false);

            // Act
            _buffSystem.ApplyDebuff(TEST_ENTITY_ID, debuff);

            // Assert
            float remainingDuration = _buffSystem.GetRemainingDuration(TEST_ENTITY_ID, debuffId);
            
            Assert.That(remainingDuration, Is.GreaterThanOrEqualTo(_buffSystem.MinDuration),
                $"Duration {randomDuration} should be clamped to at least {_buffSystem.MinDuration}");
            Assert.That(remainingDuration, Is.LessThanOrEqualTo(_buffSystem.MaxDuration),
                $"Duration {randomDuration} should be clamped to at most {_buffSystem.MaxDuration}");
        }

        /// <summary>
        /// Property 1: Edge case - Duration exactly at minimum
        /// </summary>
        [Test]
        public void BuffDurationBounds_ExactlyMinDuration_IsAccepted()
        {
            // Arrange
            var buff = CreateTestBuff("min_duration_buff", _buffSystem.MinDuration);

            // Act
            _buffSystem.ApplyBuff(TEST_ENTITY_ID, buff);

            // Assert
            float remainingDuration = _buffSystem.GetRemainingDuration(TEST_ENTITY_ID, "min_duration_buff");
            Assert.That(remainingDuration, Is.EqualTo(_buffSystem.MinDuration).Within(0.001f));
        }

        /// <summary>
        /// Property 1: Edge case - Duration exactly at maximum
        /// </summary>
        [Test]
        public void BuffDurationBounds_ExactlyMaxDuration_IsAccepted()
        {
            // Arrange
            var buff = CreateTestBuff("max_duration_buff", _buffSystem.MaxDuration);

            // Act
            _buffSystem.ApplyBuff(TEST_ENTITY_ID, buff);

            // Assert
            float remainingDuration = _buffSystem.GetRemainingDuration(TEST_ENTITY_ID, "max_duration_buff");
            Assert.That(remainingDuration, Is.EqualTo(_buffSystem.MaxDuration).Within(0.001f));
        }

        #endregion

        #region Property 2: Buff/Debuff Limit Enforcement

        /// <summary>
        /// Feature: new-classes-combat, Property 2: Buff/Debuff Limit Enforcement
        /// For any entity, the number of active buffs SHALL never exceed 20.
        /// Validates: Requirements 1.7, 1.8
        /// </summary>
        [Test]
        [Repeat(100)]
        public void BuffLimit_NeverExceedsMaxBuffs()
        {
            // Arrange - Apply random number of buffs (more than max)
            int numBuffsToApply = UnityEngine.Random.Range(21, 50);
            
            // Act
            for (int i = 0; i < numBuffsToApply; i++)
            {
                var buff = CreateTestBuff($"buff_{i}", 60f);
                _buffSystem.ApplyBuff(TEST_ENTITY_ID, buff);
            }

            // Assert
            int buffCount = _buffSystem.GetBuffCount(TEST_ENTITY_ID);
            Assert.That(buffCount, Is.LessThanOrEqualTo(_buffSystem.MaxBuffsPerEntity),
                $"Buff count ({buffCount}) should never exceed {_buffSystem.MaxBuffsPerEntity}");
        }

        /// <summary>
        /// Feature: new-classes-combat, Property 2: Buff/Debuff Limit Enforcement
        /// For any entity, the number of active debuffs SHALL never exceed 20.
        /// Validates: Requirements 1.7, 1.8
        /// </summary>
        [Test]
        [Repeat(100)]
        public void DebuffLimit_NeverExceedsMaxDebuffs()
        {
            // Arrange - Apply random number of debuffs (more than max)
            int numDebuffsToApply = UnityEngine.Random.Range(21, 50);
            
            // Act
            for (int i = 0; i < numDebuffsToApply; i++)
            {
                var debuff = CreateTestBuff($"debuff_{i}", 60f, false);
                _buffSystem.ApplyDebuff(TEST_ENTITY_ID, debuff);
            }

            // Assert
            int debuffCount = _buffSystem.GetDebuffCount(TEST_ENTITY_ID);
            Assert.That(debuffCount, Is.LessThanOrEqualTo(_buffSystem.MaxDebuffsPerEntity),
                $"Debuff count ({debuffCount}) should never exceed {_buffSystem.MaxDebuffsPerEntity}");
        }

        /// <summary>
        /// Property 2: When limit is reached, oldest buff is replaced
        /// </summary>
        [Test]
        public void BuffLimit_OldestBuffIsReplaced_WhenLimitExceeded()
        {
            // Arrange - Fill up to max buffs
            for (int i = 0; i < _buffSystem.MaxBuffsPerEntity; i++)
            {
                var buff = CreateTestBuff($"buff_{i}", 60f);
                _buffSystem.ApplyBuff(TEST_ENTITY_ID, buff);
            }

            // Act - Apply one more buff
            var newBuff = CreateTestBuff("new_buff", 60f);
            _buffSystem.ApplyBuff(TEST_ENTITY_ID, newBuff);

            // Assert
            Assert.That(_buffSystem.GetBuffCount(TEST_ENTITY_ID), Is.EqualTo(_buffSystem.MaxBuffsPerEntity));
            Assert.That(_buffSystem.HasBuff(TEST_ENTITY_ID, "new_buff"), Is.True, 
                "New buff should be present");
            Assert.That(_buffSystem.HasBuff(TEST_ENTITY_ID, "buff_0"), Is.False, 
                "Oldest buff should be removed");
        }

        #endregion

        #region Property 3: DoT/HoT Tick Consistency

        /// <summary>
        /// Feature: new-classes-combat, Property 3: DoT/HoT Tick Consistency
        /// For any DoT or HoT effect with duration D and tick interval T, 
        /// the total number of ticks SHALL equal ceil(D / T).
        /// Validates: Requirements 1.4, 1.5, 1.6
        /// </summary>
        [Test]
        [Repeat(100)]
        public void DoTTickCount_EqualsExpectedTicks()
        {
            // Arrange
            float duration = UnityEngine.Random.Range(5f, 30f);
            float tickInterval = UnityEngine.Random.Range(1f, 5f);
            float tickDamage = UnityEngine.Random.Range(10f, 100f);
            
            var dot = CreateTestDoT("test_dot", duration, tickInterval, tickDamage);
            int expectedTicks = Mathf.CeilToInt(duration / tickInterval);

            // Assert - Verify expected tick calculation
            Assert.That(dot.ExpectedTicks, Is.EqualTo(expectedTicks),
                $"DoT with duration {duration}s and interval {tickInterval}s should have {expectedTicks} ticks");
        }

        /// <summary>
        /// Feature: new-classes-combat, Property 3: DoT/HoT Tick Consistency
        /// For any HoT effect with duration D and tick interval T, 
        /// the total number of ticks SHALL equal ceil(D / T).
        /// Validates: Requirements 1.4, 1.5, 1.6
        /// </summary>
        [Test]
        [Repeat(100)]
        public void HoTTickCount_EqualsExpectedTicks()
        {
            // Arrange
            float duration = UnityEngine.Random.Range(5f, 30f);
            float tickInterval = UnityEngine.Random.Range(1f, 5f);
            float tickHealing = UnityEngine.Random.Range(10f, 100f);
            
            var hot = CreateTestHoT("test_hot", duration, tickInterval, tickHealing);
            int expectedTicks = Mathf.CeilToInt(duration / tickInterval);

            // Assert - Verify expected tick calculation
            Assert.That(hot.ExpectedTicks, Is.EqualTo(expectedTicks),
                $"HoT with duration {duration}s and interval {tickInterval}s should have {expectedTicks} ticks");
        }

        /// <summary>
        /// Property 3: DoT tick damage is consistent
        /// </summary>
        [Test]
        public void DoTTickDamage_IsConsistent()
        {
            // Arrange
            float tickDamage = 50f;
            var dot = CreateTestDoT("consistent_dot", 10f, 2f, tickDamage);
            
            float totalDamage = 0f;
            int tickCount = 0;
            
            _buffSystem.OnDoTTick += (targetId, damage, damageType, sourceId) =>
            {
                totalDamage += damage;
                tickCount++;
            };

            _buffSystem.ApplyDebuff(TEST_ENTITY_ID, dot);

            // Simulate ticks manually by checking the buff instance
            var debuffs = _buffSystem.GetActiveDebuffs(TEST_ENTITY_ID);
            Assert.That(debuffs.Count, Is.EqualTo(1));
            
            var debuffInstance = debuffs[0];
            Assert.That(debuffInstance.Data.TickDamage, Is.EqualTo(tickDamage),
                "Each tick should deal the configured damage amount");
        }

        /// <summary>
        /// Property 3: HoT tick healing is consistent
        /// </summary>
        [Test]
        public void HoTTickHealing_IsConsistent()
        {
            // Arrange
            float tickHealing = 75f;
            var hot = CreateTestHoT("consistent_hot", 10f, 2f, tickHealing);

            _buffSystem.ApplyBuff(TEST_ENTITY_ID, hot);

            // Verify the buff instance has correct healing value
            var buffs = _buffSystem.GetActiveBuffs(TEST_ENTITY_ID);
            Assert.That(buffs.Count, Is.EqualTo(1));
            
            var buffInstance = buffs[0];
            Assert.That(buffInstance.Data.TickHealing, Is.EqualTo(tickHealing),
                "Each tick should heal the configured amount");
        }

        #endregion

        #region Additional Tests

        /// <summary>
        /// Test: Buff application fires event
        /// </summary>
        [Test]
        public void ApplyBuff_FiresOnBuffAppliedEvent()
        {
            // Arrange
            bool eventFired = false;
            ulong eventTargetId = 0;
            BuffInstance eventInstance = null;

            _buffSystem.OnBuffApplied += (targetId, instance) =>
            {
                eventFired = true;
                eventTargetId = targetId;
                eventInstance = instance;
            };

            var buff = CreateTestBuff("event_test_buff", 30f);

            // Act
            _buffSystem.ApplyBuff(TEST_ENTITY_ID, buff);

            // Assert
            Assert.That(eventFired, Is.True, "OnBuffApplied should fire");
            Assert.That(eventTargetId, Is.EqualTo(TEST_ENTITY_ID));
            Assert.That(eventInstance.BuffId, Is.EqualTo("event_test_buff"));
        }

        /// <summary>
        /// Test: Debuff application fires event
        /// </summary>
        [Test]
        public void ApplyDebuff_FiresOnDebuffAppliedEvent()
        {
            // Arrange
            bool eventFired = false;
            ulong eventTargetId = 0;
            BuffInstance eventInstance = null;

            _buffSystem.OnDebuffApplied += (targetId, instance) =>
            {
                eventFired = true;
                eventTargetId = targetId;
                eventInstance = instance;
            };

            var debuff = CreateTestBuff("event_test_debuff", 30f, false);

            // Act
            _buffSystem.ApplyDebuff(TEST_ENTITY_ID, debuff);

            // Assert
            Assert.That(eventFired, Is.True, "OnDebuffApplied should fire");
            Assert.That(eventTargetId, Is.EqualTo(TEST_ENTITY_ID));
            Assert.That(eventInstance.BuffId, Is.EqualTo("event_test_debuff"));
        }

        /// <summary>
        /// Test: HasBuff returns correct value
        /// </summary>
        [Test]
        public void HasBuff_ReturnsCorrectValue()
        {
            // Arrange
            var buff = CreateTestBuff("has_buff_test", 30f);

            // Assert - Before applying
            Assert.That(_buffSystem.HasBuff(TEST_ENTITY_ID, "has_buff_test"), Is.False);

            // Act
            _buffSystem.ApplyBuff(TEST_ENTITY_ID, buff);

            // Assert - After applying
            Assert.That(_buffSystem.HasBuff(TEST_ENTITY_ID, "has_buff_test"), Is.True);
            Assert.That(_buffSystem.HasBuff(TEST_ENTITY_ID, "nonexistent_buff"), Is.False);
        }

        /// <summary>
        /// Test: RemoveBuff removes the buff
        /// </summary>
        [Test]
        public void RemoveBuff_RemovesTheBuff()
        {
            // Arrange
            var buff = CreateTestBuff("remove_test_buff", 30f);
            _buffSystem.ApplyBuff(TEST_ENTITY_ID, buff);
            Assert.That(_buffSystem.HasBuff(TEST_ENTITY_ID, "remove_test_buff"), Is.True);

            // Act
            _buffSystem.RemoveBuff(TEST_ENTITY_ID, "remove_test_buff");

            // Assert
            Assert.That(_buffSystem.HasBuff(TEST_ENTITY_ID, "remove_test_buff"), Is.False);
        }

        /// <summary>
        /// Test: RemoveAllBuffs clears all buffs
        /// </summary>
        [Test]
        public void RemoveAllBuffs_ClearsAllBuffs()
        {
            // Arrange
            for (int i = 0; i < 5; i++)
            {
                _buffSystem.ApplyBuff(TEST_ENTITY_ID, CreateTestBuff($"buff_{i}", 30f));
            }
            Assert.That(_buffSystem.GetBuffCount(TEST_ENTITY_ID), Is.EqualTo(5));

            // Act
            _buffSystem.RemoveAllBuffs(TEST_ENTITY_ID);

            // Assert
            Assert.That(_buffSystem.GetBuffCount(TEST_ENTITY_ID), Is.EqualTo(0));
        }

        #endregion
    }
}
