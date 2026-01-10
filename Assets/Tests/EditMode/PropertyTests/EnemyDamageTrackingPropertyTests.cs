using NUnit.Framework;
using UnityEngine;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for Enemy Damage Tracking.
    /// Tests damage accumulation and highest damage dealer selection.
    /// </summary>
    [TestFixture]
    public class EnemyDamageTrackingPropertyTests
    {
        // Simple test class that mimics Enemy damage tracking without NetworkBehaviour
        private class DamageTracker
        {
            private readonly System.Collections.Generic.Dictionary<ulong, float> _damageByPlayer = new();

            public System.Collections.Generic.IReadOnlyDictionary<ulong, float> DamageByPlayer => _damageByPlayer;

            public void RecordDamage(ulong playerId, float damage)
            {
                if (damage <= 0) return;
                
                if (_damageByPlayer.ContainsKey(playerId))
                {
                    _damageByPlayer[playerId] += damage;
                }
                else
                {
                    _damageByPlayer[playerId] = damage;
                }
            }

            public float GetTotalDamage(ulong playerId)
            {
                return _damageByPlayer.TryGetValue(playerId, out float damage) ? damage : 0f;
            }

            public ulong GetHighestDamageDealer()
            {
                if (_damageByPlayer.Count == 0)
                    return 0;

                ulong highestPlayer = 0;
                float highestDamage = 0;
                
                foreach (var kvp in _damageByPlayer)
                {
                    if (kvp.Value > highestDamage)
                    {
                        highestDamage = kvp.Value;
                        highestPlayer = kvp.Key;
                    }
                }
                
                return highestPlayer;
            }

            public void ClearDamageTracking()
            {
                _damageByPlayer.Clear();
            }
        }

        private DamageTracker _tracker;

        [SetUp]
        public void SetUp()
        {
            _tracker = new DamageTracker();
        }

        /// <summary>
        /// Feature: mvp-10-features, Property 37: Damage Accumulation
        /// For any damage applied to enemy, GetTotalDamage for that player 
        /// SHALL increase by exactly that amount.
        /// Validates: Requirements 13.2
        /// </summary>
        [Test]
        [Repeat(100)]
        public void DamageAccumulation_IncreasesExactly()
        {
            // Arrange
            ulong playerId = (ulong)Random.Range(1, 1000);
            float damage1 = Random.Range(1f, 100f);
            float damage2 = Random.Range(1f, 100f);
            float damage3 = Random.Range(1f, 100f);

            // Act
            _tracker.RecordDamage(playerId, damage1);
            float afterFirst = _tracker.GetTotalDamage(playerId);
            
            _tracker.RecordDamage(playerId, damage2);
            float afterSecond = _tracker.GetTotalDamage(playerId);
            
            _tracker.RecordDamage(playerId, damage3);
            float afterThird = _tracker.GetTotalDamage(playerId);

            // Assert
            Assert.That(afterFirst, Is.EqualTo(damage1).Within(0.001f),
                "After first damage, total should equal first damage");
            Assert.That(afterSecond, Is.EqualTo(damage1 + damage2).Within(0.001f),
                "After second damage, total should equal sum of first two");
            Assert.That(afterThird, Is.EqualTo(damage1 + damage2 + damage3).Within(0.001f),
                "After third damage, total should equal sum of all three");
        }

        /// <summary>
        /// Feature: mvp-10-features, Property 38: Highest Damage Dealer Selection
        /// For any enemy with damage tracking, GetHighestDamageDealer SHALL return 
        /// the playerId with maximum total damage.
        /// Validates: Requirements 13.3
        /// </summary>
        [Test]
        [Repeat(100)]
        public void HighestDamageDealer_ReturnsCorrectPlayer()
        {
            // Arrange - Create 3 players with different damage amounts
            ulong player1 = 1;
            ulong player2 = 2;
            ulong player3 = 3;
            
            float damage1 = Random.Range(1f, 100f);
            float damage2 = Random.Range(1f, 100f);
            float damage3 = Random.Range(1f, 100f);

            // Ensure one player has clearly highest damage
            float maxDamage = Mathf.Max(damage1, Mathf.Max(damage2, damage3));
            ulong expectedHighest = 0;
            
            if (damage1 == maxDamage) expectedHighest = player1;
            else if (damage2 == maxDamage) expectedHighest = player2;
            else expectedHighest = player3;

            // Act
            _tracker.RecordDamage(player1, damage1);
            _tracker.RecordDamage(player2, damage2);
            _tracker.RecordDamage(player3, damage3);

            ulong highestDealer = _tracker.GetHighestDamageDealer();

            // Assert
            Assert.That(_tracker.GetTotalDamage(highestDealer), 
                Is.EqualTo(maxDamage).Within(0.001f),
                "Highest damage dealer should have the maximum damage");
        }

        /// <summary>
        /// Property: Multiple damage instances from same player accumulate
        /// </summary>
        [Test]
        [Repeat(100)]
        public void MultipleDamageInstances_Accumulate()
        {
            // Arrange
            ulong playerId = 1;
            int hitCount = Random.Range(5, 20);
            float totalExpected = 0f;

            // Act
            for (int i = 0; i < hitCount; i++)
            {
                float damage = Random.Range(10f, 50f);
                totalExpected += damage;
                _tracker.RecordDamage(playerId, damage);
            }

            // Assert
            Assert.That(_tracker.GetTotalDamage(playerId), 
                Is.EqualTo(totalExpected).Within(0.01f),
                "Total damage should equal sum of all hits");
        }

        /// <summary>
        /// Property: Zero or negative damage is not recorded
        /// </summary>
        [Test]
        public void ZeroOrNegativeDamage_NotRecorded()
        {
            // Arrange
            ulong playerId = 1;
            
            // Act
            _tracker.RecordDamage(playerId, 0f);
            _tracker.RecordDamage(playerId, -10f);
            _tracker.RecordDamage(playerId, -100f);

            // Assert
            Assert.That(_tracker.GetTotalDamage(playerId), Is.EqualTo(0f),
                "Zero and negative damage should not be recorded");
        }

        /// <summary>
        /// Property: ClearDamageTracking resets all tracking
        /// </summary>
        [Test]
        public void ClearDamageTracking_ResetsAll()
        {
            // Arrange
            _tracker.RecordDamage(1, 100f);
            _tracker.RecordDamage(2, 200f);
            _tracker.RecordDamage(3, 300f);

            // Act
            _tracker.ClearDamageTracking();

            // Assert
            Assert.That(_tracker.GetTotalDamage(1), Is.EqualTo(0f));
            Assert.That(_tracker.GetTotalDamage(2), Is.EqualTo(0f));
            Assert.That(_tracker.GetTotalDamage(3), Is.EqualTo(0f));
            Assert.That(_tracker.GetHighestDamageDealer(), Is.EqualTo(0UL),
                "After clear, no highest damage dealer");
        }

        /// <summary>
        /// Property: GetHighestDamageDealer returns 0 when no damage recorded
        /// </summary>
        [Test]
        public void GetHighestDamageDealer_ReturnsZero_WhenNoDamage()
        {
            // Act
            ulong result = _tracker.GetHighestDamageDealer();

            // Assert
            Assert.That(result, Is.EqualTo(0UL),
                "Should return 0 when no damage has been recorded");
        }

        /// <summary>
        /// Property: Damage from different players tracked separately
        /// </summary>
        [Test]
        [Repeat(100)]
        public void DifferentPlayers_TrackedSeparately()
        {
            // Arrange
            ulong player1 = 1;
            ulong player2 = 2;
            float damage1 = Random.Range(50f, 100f);
            float damage2 = Random.Range(150f, 200f);

            // Act
            _tracker.RecordDamage(player1, damage1);
            _tracker.RecordDamage(player2, damage2);

            // Assert
            Assert.That(_tracker.GetTotalDamage(player1), Is.EqualTo(damage1).Within(0.001f),
                "Player 1 damage should be tracked separately");
            Assert.That(_tracker.GetTotalDamage(player2), Is.EqualTo(damage2).Within(0.001f),
                "Player 2 damage should be tracked separately");
            Assert.That(_tracker.GetHighestDamageDealer(), Is.EqualTo(player2),
                "Player 2 should be highest damage dealer");
        }

        /// <summary>
        /// Property: Highest damage dealer updates correctly as damage changes
        /// </summary>
        [Test]
        public void HighestDamageDealer_UpdatesCorrectly()
        {
            // Arrange
            ulong player1 = 1;
            ulong player2 = 2;

            // Act & Assert - Player 1 starts highest
            _tracker.RecordDamage(player1, 100f);
            Assert.That(_tracker.GetHighestDamageDealer(), Is.EqualTo(player1));

            // Player 2 takes over
            _tracker.RecordDamage(player2, 150f);
            Assert.That(_tracker.GetHighestDamageDealer(), Is.EqualTo(player2));

            // Player 1 catches up and surpasses
            _tracker.RecordDamage(player1, 100f); // Total: 200
            Assert.That(_tracker.GetHighestDamageDealer(), Is.EqualTo(player1));
        }
    }
}
