using NUnit.Framework;
using EtherDomes.World;
using System;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for the Weekly Lockout System.
    /// </summary>
    [TestFixture]
    public class WeeklyLockoutPropertyTests
    {
        private WeeklyLockoutSystem _lockoutSystem;

        [SetUp]
        public void SetUp()
        {
            _lockoutSystem = new WeeklyLockoutSystem();
        }

        /// <summary>
        /// Feature: mvp-10-features, Property 18: Weekly Lockout Enforcement
        /// After recording a kill, IsLockedOut SHALL return true for that boss.
        /// Validates: Requirements 5.7
        /// </summary>
        [Test]
        public void WeeklyLockoutEnforcement_AfterKill_IsLockedOutReturnsTrue()
        {
            // Arrange
            ulong playerId = 1;
            string bossId = "crypt-lord";

            // Act
            _lockoutSystem.RecordKill(playerId, bossId);

            // Assert
            Assert.That(_lockoutSystem.IsLockedOut(playerId, bossId), Is.True,
                "IsLockedOut should return true after recording kill");
        }

        /// <summary>
        /// Property 18: Before kill, IsLockedOut returns false
        /// </summary>
        [Test]
        [Repeat(100)]
        public void WeeklyLockout_BeforeKill_IsLockedOutReturnsFalse()
        {
            // Arrange
            ulong playerId = (ulong)UnityEngine.Random.Range(1, 1000);
            string bossId = $"boss-{UnityEngine.Random.Range(1, 100)}";

            // Assert
            Assert.That(_lockoutSystem.IsLockedOut(playerId, bossId), Is.False,
                "IsLockedOut should return false before any kill");
        }

        /// <summary>
        /// Property: Lockouts are per-player
        /// </summary>
        [Test]
        public void Lockouts_ArePerPlayer()
        {
            // Arrange
            ulong player1 = 1;
            ulong player2 = 2;
            string bossId = "test-boss";

            // Act
            _lockoutSystem.RecordKill(player1, bossId);

            // Assert
            Assert.That(_lockoutSystem.IsLockedOut(player1, bossId), Is.True,
                "Player 1 should be locked");
            Assert.That(_lockoutSystem.IsLockedOut(player2, bossId), Is.False,
                "Player 2 should not be locked");
        }

        /// <summary>
        /// Property: Lockouts are per-boss
        /// </summary>
        [Test]
        public void Lockouts_ArePerBoss()
        {
            // Arrange
            ulong playerId = 1;
            string boss1 = "boss-1";
            string boss2 = "boss-2";

            // Act
            _lockoutSystem.RecordKill(playerId, boss1);

            // Assert
            Assert.That(_lockoutSystem.IsLockedOut(playerId, boss1), Is.True,
                "Should be locked to boss 1");
            Assert.That(_lockoutSystem.IsLockedOut(playerId, boss2), Is.False,
                "Should not be locked to boss 2");
        }

        /// <summary>
        /// Property: GetLockedBosses returns all locked bosses
        /// </summary>
        [Test]
        public void GetLockedBosses_ReturnsAllLockedBosses()
        {
            // Arrange
            ulong playerId = 1;
            string[] bosses = { "boss-1", "boss-2", "boss-3" };

            // Act
            foreach (var boss in bosses)
            {
                _lockoutSystem.RecordKill(playerId, boss);
            }

            var lockedBosses = _lockoutSystem.GetLockedBosses(playerId);

            // Assert
            Assert.That(lockedBosses.Count, Is.EqualTo(3),
                "Should have 3 locked bosses");
            foreach (var boss in bosses)
            {
                Assert.That(lockedBosses, Contains.Item(boss),
                    $"Should contain {boss}");
            }
        }

        /// <summary>
        /// Property: OnBossKillRecorded fires on kill
        /// </summary>
        [Test]
        public void OnBossKillRecorded_FiresOnKill()
        {
            // Arrange
            ulong playerId = 1;
            string bossId = "test-boss";
            bool eventFired = false;
            ulong reportedPlayerId = 0;
            string reportedBossId = null;

            _lockoutSystem.OnBossKillRecorded += (pid, bid) =>
            {
                eventFired = true;
                reportedPlayerId = pid;
                reportedBossId = bid;
            };

            // Act
            _lockoutSystem.RecordKill(playerId, bossId);

            // Assert
            Assert.That(eventFired, Is.True, "OnBossKillRecorded should fire");
            Assert.That(reportedPlayerId, Is.EqualTo(playerId), "Should report correct player");
            Assert.That(reportedBossId, Is.EqualTo(bossId), "Should report correct boss");
        }

        /// <summary>
        /// Property: Duplicate kills don't fire event
        /// </summary>
        [Test]
        public void DuplicateKills_DontFireEvent()
        {
            // Arrange
            ulong playerId = 1;
            string bossId = "test-boss";
            int eventCount = 0;

            _lockoutSystem.OnBossKillRecorded += (pid, bid) => eventCount++;

            // Act
            _lockoutSystem.RecordKill(playerId, bossId);
            _lockoutSystem.RecordKill(playerId, bossId);

            // Assert
            Assert.That(eventCount, Is.EqualTo(1),
                "Event should only fire once for duplicate kills");
        }

        /// <summary>
        /// Property: GetResetTime returns a Monday
        /// </summary>
        [Test]
        public void GetResetTime_ReturnsMonday()
        {
            // Act
            var resetTime = _lockoutSystem.GetResetTime();

            // Assert
            Assert.That(resetTime.DayOfWeek, Is.EqualTo(DayOfWeek.Monday),
                "Reset time should be on Monday");
            Assert.That(resetTime.Hour, Is.EqualTo(0), "Reset should be at midnight");
            Assert.That(resetTime.Minute, Is.EqualTo(0), "Reset should be at midnight");
        }

        /// <summary>
        /// Property: GetResetTime is in the future
        /// </summary>
        [Test]
        public void GetResetTime_IsInFuture()
        {
            // Act
            var resetTime = _lockoutSystem.GetResetTime();

            // Assert
            Assert.That(resetTime, Is.GreaterThan(DateTime.UtcNow),
                "Reset time should be in the future");
        }

        /// <summary>
        /// Property: ForceReset clears all lockouts
        /// </summary>
        [Test]
        public void ForceReset_ClearsAllLockouts()
        {
            // Arrange
            ulong playerId = 1;
            _lockoutSystem.RecordKill(playerId, "boss-1");
            _lockoutSystem.RecordKill(playerId, "boss-2");

            // Act
            _lockoutSystem.ForceReset();

            // Assert
            Assert.That(_lockoutSystem.IsLockedOut(playerId, "boss-1"), Is.False,
                "Should not be locked after reset");
            Assert.That(_lockoutSystem.GetLockedBosses(playerId).Count, Is.EqualTo(0),
                "Should have no locked bosses after reset");
        }

        /// <summary>
        /// Property: OnLockoutsReset fires on reset
        /// </summary>
        [Test]
        public void OnLockoutsReset_FiresOnReset()
        {
            // Arrange
            bool eventFired = false;
            _lockoutSystem.OnLockoutsReset += () => eventFired = true;

            // Act
            _lockoutSystem.ForceReset();

            // Assert
            Assert.That(eventFired, Is.True, "OnLockoutsReset should fire");
        }

        /// <summary>
        /// Property: LoadPlayerLockouts restores lockouts
        /// </summary>
        [Test]
        public void LoadPlayerLockouts_RestoresLockouts()
        {
            // Arrange
            ulong playerId = 1;
            var savedLockouts = new System.Collections.Generic.List<string> { "boss-1", "boss-2" };

            // Act
            _lockoutSystem.LoadPlayerLockouts(playerId, savedLockouts);

            // Assert
            Assert.That(_lockoutSystem.IsLockedOut(playerId, "boss-1"), Is.True);
            Assert.That(_lockoutSystem.IsLockedOut(playerId, "boss-2"), Is.True);
            Assert.That(_lockoutSystem.IsLockedOut(playerId, "boss-3"), Is.False);
        }
    }
}
