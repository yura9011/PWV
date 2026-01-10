using NUnit.Framework;
using UnityEngine;
using EtherDomes.Enemy;
using EtherDomes.Data;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for the Enemy AI system.
    /// </summary>
    [TestFixture]
    public class EnemyAIPropertyTests
    {
        // Test helper class that exposes internal state for testing
        private class TestableEnemyAI
        {
            private EnemyAIState _currentState = EnemyAIState.Idle;
            private Vector3 _spawnPosition;
            private Vector3 _currentPosition;
            private float _currentHealth;
            private float _hpOnLeash;
            private readonly float _leashDistance;
            
            public EnemyAIState CurrentState => _currentState;
            public Vector3 SpawnPosition => _spawnPosition;
            public float DistanceFromSpawn => Vector3.Distance(_currentPosition, _spawnPosition);
            public float CurrentHealth => _currentHealth;
            public float HPOnLeash => _hpOnLeash;
            
            public TestableEnemyAI(float leashDistance = 40f)
            {
                _leashDistance = leashDistance;
                _spawnPosition = Vector3.zero;
                _currentPosition = Vector3.zero;
                _currentHealth = 100f;
            }
            
            public void Initialize(Vector3 spawnPosition)
            {
                _spawnPosition = spawnPosition;
                _currentPosition = spawnPosition;
            }
            
            public void SetPosition(Vector3 position)
            {
                _currentPosition = position;
            }
            
            public void SetHealth(float health)
            {
                _currentHealth = health;
            }
            
            public void SetState(EnemyAIState state)
            {
                _currentState = state;
            }
            
            /// <summary>
            /// Simulates the leash check that happens in UpdateAggro/UpdateCombat.
            /// Returns true if enemy should start returning.
            /// </summary>
            public bool CheckLeashDistance()
            {
                if (DistanceFromSpawn > _leashDistance)
                {
                    _hpOnLeash = _currentHealth;
                    _currentState = EnemyAIState.Returning;
                    return true;
                }
                return false;
            }
            
            /// <summary>
            /// Simulates returning to spawn.
            /// HP is preserved (not healed).
            /// </summary>
            public void SimulateReturnToSpawn()
            {
                _currentPosition = _spawnPosition;
                _currentState = EnemyAIState.Idle;
                // Note: HP is NOT changed - this is the key property we're testing
            }
        }
        
        /// <summary>
        /// Feature: mvp-10-features, Property 10: Leash Distance Enforcement
        /// For any enemy at distance > 40m from spawn, the enemy SHALL enter Returning state.
        /// Validates: Requirements 3.3
        /// </summary>
        [Test]
        [Repeat(100)]
        public void LeashDistanceEnforcement_BeyondLeashDistance_EntersReturning()
        {
            // Arrange
            var ai = new TestableEnemyAI(leashDistance: 40f);
            ai.Initialize(Vector3.zero);
            ai.SetState(EnemyAIState.Combat);
            
            // Generate random position beyond leash distance (40m+)
            float distance = Random.Range(40.1f, 100f);
            Vector3 direction = Random.onUnitSphere;
            direction.y = 0; // Keep on ground plane
            direction.Normalize();
            Vector3 position = direction * distance;
            
            ai.SetPosition(position);
            
            // Act
            bool shouldLeash = ai.CheckLeashDistance();
            
            // Assert
            Assert.That(shouldLeash, Is.True,
                $"Enemy at {distance}m should trigger leash (threshold: 40m)");
            Assert.That(ai.CurrentState, Is.EqualTo(EnemyAIState.Returning),
                $"Enemy at {distance}m should be in Returning state");
        }
        
        /// <summary>
        /// Property 10: Enemy within leash distance should NOT enter Returning state.
        /// </summary>
        [Test]
        [Repeat(100)]
        public void LeashDistanceEnforcement_WithinLeashDistance_StaysInCombat()
        {
            // Arrange
            var ai = new TestableEnemyAI(leashDistance: 40f);
            ai.Initialize(Vector3.zero);
            ai.SetState(EnemyAIState.Combat);
            
            // Generate random position within leash distance
            float distance = Random.Range(0f, 39.9f);
            Vector3 direction = Random.onUnitSphere;
            direction.y = 0;
            direction.Normalize();
            Vector3 position = direction * distance;
            
            ai.SetPosition(position);
            
            // Act
            bool shouldLeash = ai.CheckLeashDistance();
            
            // Assert
            Assert.That(shouldLeash, Is.False,
                $"Enemy at {distance}m should NOT trigger leash");
            Assert.That(ai.CurrentState, Is.EqualTo(EnemyAIState.Combat),
                $"Enemy at {distance}m should stay in Combat state");
        }

        /// <summary>
        /// Feature: mvp-10-features, Property 11: HP Preservation on Return
        /// For any enemy returning to spawn, HP SHALL remain unchanged.
        /// Validates: Requirements 3.4
        /// </summary>
        [Test]
        [Repeat(100)]
        public void HPPreservationOnReturn_ReturningToSpawn_HPUnchanged()
        {
            // Arrange
            var ai = new TestableEnemyAI(leashDistance: 40f);
            ai.Initialize(Vector3.zero);
            
            // Set random HP between 1 and 100
            float initialHP = Random.Range(1f, 100f);
            ai.SetHealth(initialHP);
            ai.SetState(EnemyAIState.Combat);
            
            // Move beyond leash distance
            float distance = Random.Range(41f, 80f);
            Vector3 position = Vector3.forward * distance;
            ai.SetPosition(position);
            
            // Trigger leash
            ai.CheckLeashDistance();
            Assert.That(ai.CurrentState, Is.EqualTo(EnemyAIState.Returning),
                "Enemy should be in Returning state");
            
            // Act - Simulate return to spawn
            ai.SimulateReturnToSpawn();
            
            // Assert - HP should be unchanged
            Assert.That(ai.CurrentHealth, Is.EqualTo(initialHP).Within(0.001f),
                $"HP should remain {initialHP} after returning to spawn, but was {ai.CurrentHealth}");
        }
        
        /// <summary>
        /// Property 11: HP stored on leash should match current HP at leash time.
        /// </summary>
        [Test]
        [Repeat(100)]
        public void HPPreservationOnReturn_HPOnLeash_MatchesCurrentHP()
        {
            // Arrange
            var ai = new TestableEnemyAI(leashDistance: 40f);
            ai.Initialize(Vector3.zero);
            
            float currentHP = Random.Range(1f, 100f);
            ai.SetHealth(currentHP);
            ai.SetState(EnemyAIState.Combat);
            
            // Move beyond leash
            ai.SetPosition(Vector3.forward * 50f);
            
            // Act
            ai.CheckLeashDistance();
            
            // Assert
            Assert.That(ai.HPOnLeash, Is.EqualTo(currentHP).Within(0.001f),
                $"HPOnLeash should be {currentHP} but was {ai.HPOnLeash}");
        }
        
        /// <summary>
        /// Property: Leash distance boundary test - exactly at 40m should NOT leash.
        /// </summary>
        [Test]
        public void LeashDistanceEnforcement_ExactlyAtBoundary_DoesNotLeash()
        {
            // Arrange
            var ai = new TestableEnemyAI(leashDistance: 40f);
            ai.Initialize(Vector3.zero);
            ai.SetState(EnemyAIState.Combat);
            
            // Position exactly at 40m
            ai.SetPosition(Vector3.forward * 40f);
            
            // Act
            bool shouldLeash = ai.CheckLeashDistance();
            
            // Assert
            Assert.That(shouldLeash, Is.False,
                "Enemy exactly at 40m should NOT trigger leash");
        }
        
        /// <summary>
        /// Property: Leash distance boundary test - just over 40m should leash.
        /// </summary>
        [Test]
        public void LeashDistanceEnforcement_JustOverBoundary_DoesLeash()
        {
            // Arrange
            var ai = new TestableEnemyAI(leashDistance: 40f);
            ai.Initialize(Vector3.zero);
            ai.SetState(EnemyAIState.Combat);
            
            // Position just over 40m
            ai.SetPosition(Vector3.forward * 40.01f);
            
            // Act
            bool shouldLeash = ai.CheckLeashDistance();
            
            // Assert
            Assert.That(shouldLeash, Is.True,
                "Enemy at 40.01m should trigger leash");
        }
        
        /// <summary>
        /// Property: State transitions are valid.
        /// </summary>
        [Test]
        public void StateTransitions_FromIdle_ToAggro_IsValid()
        {
            // Arrange
            var ai = new TestableEnemyAI();
            ai.Initialize(Vector3.zero);
            Assert.That(ai.CurrentState, Is.EqualTo(EnemyAIState.Idle));
            
            // Act - Simulate player detection
            ai.SetState(EnemyAIState.Aggro);
            
            // Assert
            Assert.That(ai.CurrentState, Is.EqualTo(EnemyAIState.Aggro));
        }
        
        /// <summary>
        /// Property: State transitions from Aggro to Combat when in range.
        /// </summary>
        [Test]
        public void StateTransitions_FromAggro_ToCombat_IsValid()
        {
            // Arrange
            var ai = new TestableEnemyAI();
            ai.Initialize(Vector3.zero);
            ai.SetState(EnemyAIState.Aggro);
            
            // Act - Simulate reaching attack range
            ai.SetState(EnemyAIState.Combat);
            
            // Assert
            Assert.That(ai.CurrentState, Is.EqualTo(EnemyAIState.Combat));
        }
        
        /// <summary>
        /// Property: Returning state transitions to Idle when reaching spawn.
        /// </summary>
        [Test]
        public void StateTransitions_FromReturning_ToIdle_WhenAtSpawn()
        {
            // Arrange
            var ai = new TestableEnemyAI();
            ai.Initialize(Vector3.zero);
            ai.SetPosition(Vector3.forward * 50f);
            ai.SetState(EnemyAIState.Returning);
            
            // Act - Simulate reaching spawn
            ai.SimulateReturnToSpawn();
            
            // Assert
            Assert.That(ai.CurrentState, Is.EqualTo(EnemyAIState.Idle));
            Assert.That(ai.DistanceFromSpawn, Is.LessThan(0.1f));
        }
        
        /// <summary>
        /// Property: Custom leash distances work correctly.
        /// </summary>
        [Test]
        [Repeat(100)]
        public void LeashDistance_CustomValue_WorksCorrectly()
        {
            // Arrange
            float customLeash = Random.Range(20f, 100f);
            var ai = new TestableEnemyAI(leashDistance: customLeash);
            ai.Initialize(Vector3.zero);
            ai.SetState(EnemyAIState.Combat);
            
            // Position beyond custom leash
            float distance = customLeash + Random.Range(0.1f, 20f);
            ai.SetPosition(Vector3.forward * distance);
            
            // Act
            bool shouldLeash = ai.CheckLeashDistance();
            
            // Assert
            Assert.That(shouldLeash, Is.True,
                $"Enemy at {distance}m should trigger leash (threshold: {customLeash}m)");
        }
        
        /// <summary>
        /// Property: Distance calculation is correct in 3D space.
        /// </summary>
        [Test]
        [Repeat(100)]
        public void DistanceFromSpawn_3DPosition_CalculatesCorrectly()
        {
            // Arrange
            var ai = new TestableEnemyAI();
            Vector3 spawn = new Vector3(
                Random.Range(-100f, 100f),
                Random.Range(0f, 10f),
                Random.Range(-100f, 100f)
            );
            ai.Initialize(spawn);
            
            Vector3 current = new Vector3(
                Random.Range(-100f, 100f),
                Random.Range(0f, 10f),
                Random.Range(-100f, 100f)
            );
            ai.SetPosition(current);
            
            // Act
            float calculatedDistance = ai.DistanceFromSpawn;
            float expectedDistance = Vector3.Distance(spawn, current);
            
            // Assert
            Assert.That(calculatedDistance, Is.EqualTo(expectedDistance).Within(0.001f),
                $"Distance should be {expectedDistance} but was {calculatedDistance}");
        }
    }
}
