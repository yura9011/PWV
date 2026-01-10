using NUnit.Framework;
using EtherDomes.Progression;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for the Loot Window UI.
    /// </summary>
    [TestFixture]
    public class LootWindowPropertyTests
    {
        /// <summary>
        /// Feature: mvp-10-features, Property 39: Loot Window Timeout
        /// The loot window timeout SHALL be 30 seconds.
        /// Validates: Requirements 15.4
        /// </summary>
        [Test]
        public void LootWindowTimeout_Is30Seconds()
        {
            // Arrange
            var lootSystem = new LootDistributionSystem();

            // Assert
            Assert.That(lootSystem.RollTimeout, Is.EqualTo(30f),
                "Loot window timeout should be 30 seconds");
        }

        /// <summary>
        /// Property 39: Auto-pass after timeout
        /// When timeout expires, players who haven't rolled SHALL be auto-passed.
        /// </summary>
        [Test]
        public void AutoPass_AfterTimeout_PlayersAreAutoPassedOnFinalize()
        {
            // Arrange
            var lootSystem = new LootDistributionSystem();
            var item = new EtherDomes.Data.ItemData
            {
                ItemId = "test",
                ItemName = "Test Item",
                Rarity = EtherDomes.Data.ItemRarity.Rare
            };
            var players = new System.Collections.Generic.List<ulong> { 1, 2, 3 };

            string sessionId = lootSystem.StartNeedGreedRoll(item, players);

            // Only player 1 rolls
            lootSystem.SubmitRoll(sessionId, 1, LootRollType.Need);

            // Act - Finalize (simulating timeout)
            lootSystem.FinalizeRolls(sessionId);

            // Assert
            var session = lootSystem.GetSession(sessionId);
            Assert.That(session.IsFinalized, Is.True, "Session should be finalized");
            Assert.That(session.Rolls.ContainsKey(2), Is.True, "Player 2 should have auto-pass roll");
            Assert.That(session.Rolls.ContainsKey(3), Is.True, "Player 3 should have auto-pass roll");
            Assert.That(session.Rolls[2].RollType, Is.EqualTo(LootRollType.Pass), "Player 2 should be auto-passed");
            Assert.That(session.Rolls[3].RollType, Is.EqualTo(LootRollType.Pass), "Player 3 should be auto-passed");
        }

        /// <summary>
        /// Property: Winner is determined correctly after timeout
        /// </summary>
        [Test]
        public void AutoPass_AfterTimeout_WinnerDeterminedCorrectly()
        {
            // Arrange
            var lootSystem = new LootDistributionSystem();
            var item = new EtherDomes.Data.ItemData
            {
                ItemId = "test",
                ItemName = "Test Item",
                Rarity = EtherDomes.Data.ItemRarity.Rare
            };
            var players = new System.Collections.Generic.List<ulong> { 1, 2, 3 };

            string sessionId = lootSystem.StartNeedGreedRoll(item, players);

            // Only player 2 rolls Greed
            lootSystem.SubmitRoll(sessionId, 2, LootRollType.Greed);

            // Act - Finalize (simulating timeout)
            var winnerId = lootSystem.FinalizeRolls(sessionId);

            // Assert
            Assert.That(winnerId, Is.EqualTo(2UL),
                "Player 2 should win since others auto-passed");
        }

        /// <summary>
        /// Property: All auto-pass results in no winner
        /// </summary>
        [Test]
        public void AutoPass_AllPlayersTimeout_NoWinner()
        {
            // Arrange
            var lootSystem = new LootDistributionSystem();
            var item = new EtherDomes.Data.ItemData
            {
                ItemId = "test",
                ItemName = "Test Item",
                Rarity = EtherDomes.Data.ItemRarity.Rare
            };
            var players = new System.Collections.Generic.List<ulong> { 1, 2, 3 };

            string sessionId = lootSystem.StartNeedGreedRoll(item, players);

            // No one rolls - simulate timeout

            // Act - Finalize (simulating timeout)
            var winnerId = lootSystem.FinalizeRolls(sessionId);

            // Assert
            Assert.That(winnerId, Is.Null,
                "No winner when all players auto-pass");
        }

        /// <summary>
        /// Property: Session timeout value is correct
        /// </summary>
        [Test]
        public void SessionTimeout_HasCorrectValue()
        {
            // Arrange
            var lootSystem = new LootDistributionSystem();
            var item = new EtherDomes.Data.ItemData
            {
                ItemId = "test",
                ItemName = "Test Item",
                Rarity = EtherDomes.Data.ItemRarity.Rare
            };
            var players = new System.Collections.Generic.List<ulong> { 1 };

            // Act
            string sessionId = lootSystem.StartNeedGreedRoll(item, players);
            var session = lootSystem.GetSession(sessionId);

            // Assert
            Assert.That(session.TimeoutSeconds, Is.EqualTo(30f),
                "Session timeout should be 30 seconds");
        }
    }
}
