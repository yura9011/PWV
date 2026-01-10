using NUnit.Framework;
using EtherDomes.Progression;
using EtherDomes.Data;
using System.Collections.Generic;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for the Loot Distribution System.
    /// </summary>
    [TestFixture]
    public class LootDistributionPropertyTests
    {
        private LootDistributionSystem _lootSystem;

        [SetUp]
        public void SetUp()
        {
            _lootSystem = new LootDistributionSystem();
        }

        /// <summary>
        /// Feature: mvp-10-features, Property 13: Bad Luck Protection Scaling
        /// For any N > 10 failed attempts, bonus SHALL equal (N-10) * 0.05.
        /// Validates: Requirements 4.5
        /// </summary>
        [Test]
        [Repeat(100)]
        public void BadLuckProtectionScaling_AboveThreshold_CorrectBonus()
        {
            // Arrange
            ulong playerId = (ulong)UnityEngine.Random.Range(1, 1000);
            ItemRarity rarity = (ItemRarity)UnityEngine.Random.Range(0, 5);
            int attempts = UnityEngine.Random.Range(11, 50);

            // Record failed attempts
            for (int i = 0; i < attempts; i++)
            {
                _lootSystem.RecordLootAttempt(playerId, rarity, false);
            }

            // Act
            float bonus = _lootSystem.GetBadLuckProtectionBonus(playerId, rarity);

            // Assert
            float expectedBonus = (attempts - 10) * 0.05f;
            Assert.That(bonus, Is.EqualTo(expectedBonus).Within(0.001f),
                $"For {attempts} attempts, bonus should be {expectedBonus}");
        }

        /// <summary>
        /// Property 13: No bonus at or below threshold
        /// </summary>
        [Test]
        [Repeat(100)]
        public void BadLuckProtection_AtOrBelowThreshold_NoBonus()
        {
            // Arrange
            ulong playerId = (ulong)UnityEngine.Random.Range(1, 1000);
            ItemRarity rarity = (ItemRarity)UnityEngine.Random.Range(0, 5);
            int attempts = UnityEngine.Random.Range(0, 11);

            // Record failed attempts
            for (int i = 0; i < attempts; i++)
            {
                _lootSystem.RecordLootAttempt(playerId, rarity, false);
            }

            // Act
            float bonus = _lootSystem.GetBadLuckProtectionBonus(playerId, rarity);

            // Assert
            Assert.That(bonus, Is.EqualTo(0f),
                $"For {attempts} attempts (<=10), bonus should be 0");
        }

        /// <summary>
        /// Property: Winning resets bad luck protection
        /// </summary>
        [Test]
        public void BadLuckProtection_WinningResetsCounter()
        {
            // Arrange
            ulong playerId = 1;
            ItemRarity rarity = ItemRarity.Epic;

            // Record 15 failed attempts
            for (int i = 0; i < 15; i++)
            {
                _lootSystem.RecordLootAttempt(playerId, rarity, false);
            }

            // Verify bonus exists
            Assert.That(_lootSystem.GetBadLuckProtectionBonus(playerId, rarity), Is.GreaterThan(0));

            // Act - Win an item
            _lootSystem.RecordLootAttempt(playerId, rarity, true);

            // Assert
            Assert.That(_lootSystem.GetBadLuckProtectionBonus(playerId, rarity), Is.EqualTo(0f),
                "Winning should reset bad luck protection");
        }

        /// <summary>
        /// Property: Need beats Greed
        /// </summary>
        [Test]
        [Repeat(100)]
        public void RollPriority_NeedBeatsGreed()
        {
            // Arrange
            var item = CreateRandomItem();
            var players = new List<ulong> { 1, 2 };
            
            string sessionId = _lootSystem.StartNeedGreedRoll(item, players);

            // Player 1 rolls Greed with high value
            // Player 2 rolls Need with low value
            _lootSystem.SubmitRoll(sessionId, 1, LootRollType.Greed);
            _lootSystem.SubmitRoll(sessionId, 2, LootRollType.Need);

            // Act
            var session = _lootSystem.GetSession(sessionId);

            // Assert
            Assert.That(session.WinnerId, Is.EqualTo(2UL),
                "Need should always beat Greed regardless of roll value");
        }

        /// <summary>
        /// Property: Greed beats Pass
        /// </summary>
        [Test]
        public void RollPriority_GreedBeatsPass()
        {
            // Arrange
            var item = CreateRandomItem();
            var players = new List<ulong> { 1, 2 };
            
            string sessionId = _lootSystem.StartNeedGreedRoll(item, players);

            _lootSystem.SubmitRoll(sessionId, 1, LootRollType.Pass);
            _lootSystem.SubmitRoll(sessionId, 2, LootRollType.Greed);

            // Act
            var session = _lootSystem.GetSession(sessionId);

            // Assert
            Assert.That(session.WinnerId, Is.EqualTo(2UL),
                "Greed should beat Pass");
        }

        /// <summary>
        /// Property: All Pass results in no winner
        /// </summary>
        [Test]
        public void RollPriority_AllPass_NoWinner()
        {
            // Arrange
            var item = CreateRandomItem();
            var players = new List<ulong> { 1, 2, 3 };
            
            string sessionId = _lootSystem.StartNeedGreedRoll(item, players);

            _lootSystem.SubmitRoll(sessionId, 1, LootRollType.Pass);
            _lootSystem.SubmitRoll(sessionId, 2, LootRollType.Pass);
            _lootSystem.SubmitRoll(sessionId, 3, LootRollType.Pass);

            // Act
            var session = _lootSystem.GetSession(sessionId);

            // Assert
            Assert.That(session.WinnerId, Is.Null,
                "All Pass should result in no winner");
        }

        /// <summary>
        /// Property: Roll values are between 1 and 100
        /// </summary>
        [Test]
        [Repeat(100)]
        public void RollValues_BetweenOneAndHundred()
        {
            // Arrange
            var item = CreateRandomItem();
            var players = new List<ulong> { 1 };
            
            string sessionId = _lootSystem.StartNeedGreedRoll(item, players);
            _lootSystem.SubmitRoll(sessionId, 1, LootRollType.Need);

            // Act
            var session = _lootSystem.GetSession(sessionId);
            var roll = session.Rolls[1];

            // Assert
            Assert.That(roll.RollValue, Is.InRange(1, 100),
                "Roll value should be between 1 and 100");
        }

        /// <summary>
        /// Property: Auto-finalize when all players roll
        /// </summary>
        [Test]
        public void AutoFinalize_WhenAllPlayersRoll()
        {
            // Arrange
            var item = CreateRandomItem();
            var players = new List<ulong> { 1, 2 };
            
            string sessionId = _lootSystem.StartNeedGreedRoll(item, players);

            // Act
            _lootSystem.SubmitRoll(sessionId, 1, LootRollType.Greed);
            _lootSystem.SubmitRoll(sessionId, 2, LootRollType.Greed);

            // Assert
            var session = _lootSystem.GetSession(sessionId);
            Assert.That(session.IsFinalized, Is.True,
                "Session should auto-finalize when all players roll");
        }

        /// <summary>
        /// Property: Cannot roll twice
        /// </summary>
        [Test]
        public void CannotRollTwice()
        {
            // Arrange
            var item = CreateRandomItem();
            var players = new List<ulong> { 1, 2 };
            
            string sessionId = _lootSystem.StartNeedGreedRoll(item, players);
            _lootSystem.SubmitRoll(sessionId, 1, LootRollType.Greed);

            // Get first roll value
            var session = _lootSystem.GetSession(sessionId);
            var firstRoll = session.Rolls[1];

            // Act - Try to roll again
            _lootSystem.SubmitRoll(sessionId, 1, LootRollType.Need);

            // Assert - Roll should not change
            var secondRoll = session.Rolls[1];
            Assert.That(secondRoll.RollType, Is.EqualTo(firstRoll.RollType),
                "Second roll should be ignored");
        }

        /// <summary>
        /// Property: Timeout is 30 seconds
        /// </summary>
        [Test]
        public void RollTimeout_Is30Seconds()
        {
            Assert.That(_lootSystem.RollTimeout, Is.EqualTo(30f),
                "Roll timeout should be 30 seconds");
        }

        /// <summary>
        /// Property: OnRollStarted fires when roll starts
        /// </summary>
        [Test]
        public void OnRollStarted_FiresWhenRollStarts()
        {
            // Arrange
            var item = CreateRandomItem();
            var players = new List<ulong> { 1 };
            
            bool eventFired = false;
            _lootSystem.OnRollStarted += (session) => eventFired = true;

            // Act
            _lootSystem.StartNeedGreedRoll(item, players);

            // Assert
            Assert.That(eventFired, Is.True, "OnRollStarted should fire");
        }

        /// <summary>
        /// Property: OnRollFinalized fires when rolls finalize
        /// </summary>
        [Test]
        public void OnRollFinalized_FiresWhenFinalized()
        {
            // Arrange
            var item = CreateRandomItem();
            var players = new List<ulong> { 1 };
            
            bool eventFired = false;
            ulong? reportedWinner = null;
            _lootSystem.OnRollFinalized += (sessionId, winner, i) =>
            {
                eventFired = true;
                reportedWinner = winner;
            };

            string sessionId = _lootSystem.StartNeedGreedRoll(item, players);

            // Act
            _lootSystem.SubmitRoll(sessionId, 1, LootRollType.Need);

            // Assert
            Assert.That(eventFired, Is.True, "OnRollFinalized should fire");
            Assert.That(reportedWinner, Is.EqualTo(1UL), "Winner should be reported");
        }

        /// <summary>
        /// Property: Bad luck protection is per-rarity
        /// </summary>
        [Test]
        public void BadLuckProtection_IsPerRarity()
        {
            // Arrange
            ulong playerId = 1;

            // Record 15 failed attempts for Epic
            for (int i = 0; i < 15; i++)
            {
                _lootSystem.RecordLootAttempt(playerId, ItemRarity.Epic, false);
            }

            // Act
            float epicBonus = _lootSystem.GetBadLuckProtectionBonus(playerId, ItemRarity.Epic);
            float rareBonus = _lootSystem.GetBadLuckProtectionBonus(playerId, ItemRarity.Rare);

            // Assert
            Assert.That(epicBonus, Is.GreaterThan(0), "Epic should have bonus");
            Assert.That(rareBonus, Is.EqualTo(0f), "Rare should have no bonus");
        }

        private ItemData CreateRandomItem()
        {
            return new ItemData
            {
                ItemId = System.Guid.NewGuid().ToString(),
                ItemName = "Test Loot",
                ItemLevel = UnityEngine.Random.Range(1, 60),
                Rarity = (ItemRarity)UnityEngine.Random.Range(0, 5),
                Slot = (EquipmentSlot)UnityEngine.Random.Range(0, 8)
            };
        }
    }
}
