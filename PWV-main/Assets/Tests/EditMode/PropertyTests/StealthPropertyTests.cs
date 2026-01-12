using System;
using NUnit.Framework;
using UnityEngine;
using EtherDomes.Combat;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for the Stealth System.
    /// Feature: new-classes-combat
    /// </summary>
    [TestFixture]
    public class StealthPropertyTests
    {
        private StealthSystem _stealthSystem;
        private const ulong TEST_PLAYER_ID = 1;

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("StealthSystem");
            _stealthSystem = go.AddComponent<StealthSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_stealthSystem != null)
            {
                UnityEngine.Object.DestroyImmediate(_stealthSystem.gameObject);
            }
        }

        #region Property 8: Stealth Break Conditions

        /// <summary>
        /// Feature: new-classes-combat, Property 8: Stealth Break Conditions
        /// For any player in stealth, receiving damage SHALL immediately break stealth.
        /// Validates: Requirements 4.2, 4.3
        /// </summary>
        [Test]
        [Repeat(100)]
        public void StealthBreak_DamageReceived_BreaksStealth()
        {
            // Arrange
            ulong playerId = (ulong)UnityEngine.Random.Range(1, 10000);
            _stealthSystem.ClearAllStealthStates();
            _stealthSystem.TryEnterStealth(playerId);
            
            Assert.That(_stealthSystem.IsInStealth(playerId), Is.True,
                "Player should be in stealth before damage");

            // Act
            _stealthSystem.OnDamageReceived(playerId);

            // Assert
            Assert.That(_stealthSystem.IsInStealth(playerId), Is.False,
                "Receiving damage SHALL break stealth");
        }

        /// <summary>
        /// Feature: new-classes-combat, Property 8: Stealth Break Conditions
        /// For any player in stealth, performing an attack SHALL immediately break stealth.
        /// Validates: Requirements 4.2, 4.3
        /// </summary>
        [Test]
        [Repeat(100)]
        public void StealthBreak_AttackPerformed_BreaksStealth()
        {
            // Arrange
            ulong playerId = (ulong)UnityEngine.Random.Range(1, 10000);
            _stealthSystem.ClearAllStealthStates();
            _stealthSystem.TryEnterStealth(playerId);
            
            Assert.That(_stealthSystem.IsInStealth(playerId), Is.True,
                "Player should be in stealth before attack");

            // Act
            _stealthSystem.OnAttackPerformed(playerId);

            // Assert
            Assert.That(_stealthSystem.IsInStealth(playerId), Is.False,
                "Performing an attack SHALL break stealth");
        }

        /// <summary>
        /// Property 8: Stealth break fires correct event with reason
        /// </summary>
        [Test]
        public void StealthBreak_DamageReceived_FiresEventWithCorrectReason()
        {
            // Arrange
            bool eventFired = false;
            StealthBreakReason capturedReason = StealthBreakReason.Manual;
            
            _stealthSystem.OnStealthBroken += (id, reason) =>
            {
                if (id == TEST_PLAYER_ID)
                {
                    eventFired = true;
                    capturedReason = reason;
                }
            };
            
            _stealthSystem.TryEnterStealth(TEST_PLAYER_ID);

            // Act
            _stealthSystem.OnDamageReceived(TEST_PLAYER_ID);

            // Assert
            Assert.That(eventFired, Is.True, "OnStealthBroken event should fire");
            Assert.That(capturedReason, Is.EqualTo(StealthBreakReason.DamageReceived),
                "Reason should be DamageReceived");
        }

        /// <summary>
        /// Property 8: Stealth break fires correct event with reason for attack
        /// </summary>
        [Test]
        public void StealthBreak_AttackPerformed_FiresEventWithCorrectReason()
        {
            // Arrange
            bool eventFired = false;
            StealthBreakReason capturedReason = StealthBreakReason.Manual;
            
            _stealthSystem.OnStealthBroken += (id, reason) =>
            {
                if (id == TEST_PLAYER_ID)
                {
                    eventFired = true;
                    capturedReason = reason;
                }
            };
            
            _stealthSystem.TryEnterStealth(TEST_PLAYER_ID);

            // Act
            _stealthSystem.OnAttackPerformed(TEST_PLAYER_ID);

            // Assert
            Assert.That(eventFired, Is.True, "OnStealthBroken event should fire");
            Assert.That(capturedReason, Is.EqualTo(StealthBreakReason.Attack),
                "Reason should be Attack");
        }

        /// <summary>
        /// Property 8: Damage on non-stealthed player does nothing
        /// </summary>
        [Test]
        [Repeat(100)]
        public void StealthBreak_DamageOnNonStealthed_DoesNothing()
        {
            // Arrange
            ulong playerId = (ulong)UnityEngine.Random.Range(1, 10000);
            _stealthSystem.ClearAllStealthStates();
            
            bool eventFired = false;
            _stealthSystem.OnStealthBroken += (id, reason) => eventFired = true;

            // Act
            _stealthSystem.OnDamageReceived(playerId);

            // Assert
            Assert.That(eventFired, Is.False,
                "No event should fire when damaging non-stealthed player");
        }

        #endregion

        #region Property 9: Stealth Movement Speed

        /// <summary>
        /// Feature: new-classes-combat, Property 9: Stealth Movement Speed
        /// For any player in stealth, movement speed SHALL be 70% of base speed.
        /// Validates: Requirements 4.4
        /// </summary>
        [Test]
        [Repeat(100)]
        public void StealthMovementSpeed_InStealth_Is70Percent()
        {
            // Arrange
            ulong playerId = (ulong)UnityEngine.Random.Range(1, 10000);
            _stealthSystem.ClearAllStealthStates();
            _stealthSystem.TryEnterStealth(playerId);

            // Act
            float multiplier = _stealthSystem.GetMovementSpeedMultiplier(playerId);

            // Assert
            Assert.That(multiplier, Is.EqualTo(StealthSystem.DEFAULT_MOVEMENT_SPEED_MULTIPLIER).Within(0.001f),
                $"Movement speed in stealth SHALL be {StealthSystem.DEFAULT_MOVEMENT_SPEED_MULTIPLIER * 100}% of base");
        }

        /// <summary>
        /// Feature: new-classes-combat, Property 9: Stealth Movement Speed
        /// For any player NOT in stealth, movement speed SHALL be 100% of base speed.
        /// Validates: Requirements 4.4
        /// </summary>
        [Test]
        [Repeat(100)]
        public void StealthMovementSpeed_NotInStealth_Is100Percent()
        {
            // Arrange
            ulong playerId = (ulong)UnityEngine.Random.Range(1, 10000);
            _stealthSystem.ClearAllStealthStates();

            // Act
            float multiplier = _stealthSystem.GetMovementSpeedMultiplier(playerId);

            // Assert
            Assert.That(multiplier, Is.EqualTo(1f).Within(0.001f),
                "Movement speed outside stealth SHALL be 100% of base");
        }

        /// <summary>
        /// Property 9: Movement speed returns to normal after stealth breaks
        /// </summary>
        [Test]
        [Repeat(100)]
        public void StealthMovementSpeed_AfterBreak_ReturnsToNormal()
        {
            // Arrange
            ulong playerId = (ulong)UnityEngine.Random.Range(1, 10000);
            _stealthSystem.ClearAllStealthStates();
            _stealthSystem.TryEnterStealth(playerId);
            
            // Verify in stealth
            Assert.That(_stealthSystem.GetMovementSpeedMultiplier(playerId), 
                Is.EqualTo(StealthSystem.DEFAULT_MOVEMENT_SPEED_MULTIPLIER).Within(0.001f));

            // Act
            _stealthSystem.OnDamageReceived(playerId);

            // Assert
            float multiplier = _stealthSystem.GetMovementSpeedMultiplier(playerId);
            Assert.That(multiplier, Is.EqualTo(1f).Within(0.001f),
                "Movement speed SHALL return to 100% after stealth breaks");
        }

        /// <summary>
        /// Property 9: Verify the constant value is exactly 0.7
        /// </summary>
        [Test]
        public void StealthMovementSpeed_ConstantValue_IsExactly70Percent()
        {
            Assert.That(StealthSystem.DEFAULT_MOVEMENT_SPEED_MULTIPLIER, Is.EqualTo(0.7f),
                "Stealth movement speed multiplier constant SHALL be 0.7 (70%)");
        }

        #endregion

        #region Property 10: Stealth Cooldown Enforcement

        /// <summary>
        /// Feature: new-classes-combat, Property 10: Stealth Cooldown Enforcement
        /// After stealth breaks, player SHALL NOT be able to re-enter stealth for 2 seconds.
        /// Validates: Requirements 4.6
        /// </summary>
        [Test]
        [Repeat(100)]
        public void StealthCooldown_AfterBreak_CannotReenterImmediately()
        {
            // Arrange
            ulong playerId = (ulong)UnityEngine.Random.Range(1, 10000);
            _stealthSystem.ClearAllStealthStates();
            _stealthSystem.TryEnterStealth(playerId);
            _stealthSystem.OnDamageReceived(playerId); // Break stealth

            // Act
            bool canReenter = _stealthSystem.TryEnterStealth(playerId);

            // Assert
            Assert.That(canReenter, Is.False,
                "Player SHALL NOT be able to re-enter stealth immediately after break");
            Assert.That(_stealthSystem.IsInStealth(playerId), Is.False,
                "Player should not be in stealth");
        }

        /// <summary>
        /// Feature: new-classes-combat, Property 10: Stealth Cooldown Enforcement
        /// The cooldown duration SHALL be exactly 2 seconds.
        /// Validates: Requirements 4.6
        /// </summary>
        [Test]
        public void StealthCooldown_Duration_IsExactly2Seconds()
        {
            Assert.That(StealthSystem.DEFAULT_STEALTH_COOLDOWN, Is.EqualTo(2f),
                "Stealth cooldown SHALL be exactly 2 seconds");
        }

        /// <summary>
        /// Property 10: Cooldown remaining is positive after break
        /// </summary>
        [Test]
        [Repeat(100)]
        public void StealthCooldown_AfterBreak_HasPositiveRemaining()
        {
            // Arrange
            ulong playerId = (ulong)UnityEngine.Random.Range(1, 10000);
            _stealthSystem.ClearAllStealthStates();
            _stealthSystem.TryEnterStealth(playerId);
            _stealthSystem.OnDamageReceived(playerId); // Break stealth

            // Act
            float remaining = _stealthSystem.GetStealthCooldownRemaining(playerId);

            // Assert
            Assert.That(remaining, Is.GreaterThan(0f),
                "Cooldown remaining should be positive immediately after break");
            Assert.That(remaining, Is.LessThanOrEqualTo(StealthSystem.DEFAULT_STEALTH_COOLDOWN),
                $"Cooldown remaining should not exceed {StealthSystem.DEFAULT_STEALTH_COOLDOWN}s");
        }

        /// <summary>
        /// Property 10: CanEnterStealth returns false during cooldown
        /// </summary>
        [Test]
        [Repeat(100)]
        public void StealthCooldown_DuringCooldown_CanEnterStealthReturnsFalse()
        {
            // Arrange
            ulong playerId = (ulong)UnityEngine.Random.Range(1, 10000);
            _stealthSystem.ClearAllStealthStates();
            _stealthSystem.TryEnterStealth(playerId);
            _stealthSystem.OnDamageReceived(playerId); // Break stealth

            // Act
            bool canEnter = _stealthSystem.CanEnterStealth(playerId);

            // Assert
            Assert.That(canEnter, Is.False,
                "CanEnterStealth SHALL return false during cooldown");
        }

        /// <summary>
        /// Property 10: No cooldown for players who haven't broken stealth
        /// </summary>
        [Test]
        [Repeat(100)]
        public void StealthCooldown_NeverBroken_NoCooldown()
        {
            // Arrange
            ulong playerId = (ulong)UnityEngine.Random.Range(1, 10000);
            _stealthSystem.ClearAllStealthStates();

            // Act
            float remaining = _stealthSystem.GetStealthCooldownRemaining(playerId);
            bool canEnter = _stealthSystem.CanEnterStealth(playerId);

            // Assert
            Assert.That(remaining, Is.EqualTo(0f),
                "Players who never broke stealth should have no cooldown");
            Assert.That(canEnter, Is.True,
                "Players who never broke stealth should be able to enter");
        }

        /// <summary>
        /// Property 10: Manual exit also triggers cooldown
        /// </summary>
        [Test]
        public void StealthCooldown_ManualExit_TriggersCooldown()
        {
            // Arrange
            _stealthSystem.TryEnterStealth(TEST_PLAYER_ID);

            // Act
            _stealthSystem.ExitStealth(TEST_PLAYER_ID);

            // Assert
            Assert.That(_stealthSystem.CanEnterStealth(TEST_PLAYER_ID), Is.False,
                "Manual exit should also trigger cooldown");
            Assert.That(_stealthSystem.GetStealthCooldownRemaining(TEST_PLAYER_ID), Is.GreaterThan(0f),
                "Cooldown should be active after manual exit");
        }

        #endregion

        #region Additional Tests

        /// <summary>
        /// Test: Entering stealth fires event
        /// </summary>
        [Test]
        public void EnterStealth_FiresOnStealthEnteredEvent()
        {
            // Arrange
            bool eventFired = false;
            ulong capturedPlayerId = 0;
            
            _stealthSystem.OnStealthEntered += (playerId) =>
            {
                eventFired = true;
                capturedPlayerId = playerId;
            };

            // Act
            _stealthSystem.TryEnterStealth(TEST_PLAYER_ID);

            // Assert
            Assert.That(eventFired, Is.True, "OnStealthEntered should fire");
            Assert.That(capturedPlayerId, Is.EqualTo(TEST_PLAYER_ID));
        }

        /// <summary>
        /// Test: Already stealthed player returns true but doesn't fire event again
        /// </summary>
        [Test]
        public void EnterStealth_AlreadyStealthed_ReturnsTrueNoEvent()
        {
            // Arrange
            _stealthSystem.TryEnterStealth(TEST_PLAYER_ID);
            
            int eventCount = 0;
            _stealthSystem.OnStealthEntered += (playerId) => eventCount++;

            // Act
            bool result = _stealthSystem.TryEnterStealth(TEST_PLAYER_ID);

            // Assert
            Assert.That(result, Is.True, "Should return true when already stealthed");
            Assert.That(eventCount, Is.EqualTo(0), "Should not fire event again");
        }

        /// <summary>
        /// Test: Multiple players can be stealthed independently
        /// </summary>
        [Test]
        public void Stealth_MultiplePlayers_IndependentStates()
        {
            // Arrange
            ulong player1 = 1;
            ulong player2 = 2;
            ulong player3 = 3;

            // Act
            _stealthSystem.TryEnterStealth(player1);
            _stealthSystem.TryEnterStealth(player2);
            _stealthSystem.OnDamageReceived(player1); // Break player1's stealth

            // Assert
            Assert.That(_stealthSystem.IsInStealth(player1), Is.False, "Player1 should not be stealthed");
            Assert.That(_stealthSystem.IsInStealth(player2), Is.True, "Player2 should still be stealthed");
            Assert.That(_stealthSystem.IsInStealth(player3), Is.False, "Player3 was never stealthed");
        }

        /// <summary>
        /// Test: ClearAllStealthStates resets everything
        /// </summary>
        [Test]
        public void ClearAllStealthStates_ResetsEverything()
        {
            // Arrange
            _stealthSystem.TryEnterStealth(1);
            _stealthSystem.TryEnterStealth(2);
            _stealthSystem.OnDamageReceived(1); // Create cooldown for player 1

            // Act
            _stealthSystem.ClearAllStealthStates();

            // Assert
            Assert.That(_stealthSystem.IsInStealth(1), Is.False);
            Assert.That(_stealthSystem.IsInStealth(2), Is.False);
            Assert.That(_stealthSystem.CanEnterStealth(1), Is.True, "Cooldowns should be cleared");
            Assert.That(_stealthSystem.GetStealthedPlayerCount(), Is.EqualTo(0));
        }

        /// <summary>
        /// Test: CanUseStealthAbility requires being in stealth
        /// </summary>
        [Test]
        public void CanUseStealthAbility_RequiresStealth()
        {
            // Arrange - Not in stealth
            Assert.That(_stealthSystem.CanUseStealthAbility(TEST_PLAYER_ID, "ambush"), Is.False,
                "Should not be able to use stealth ability when not stealthed");

            // Act - Enter stealth
            _stealthSystem.TryEnterStealth(TEST_PLAYER_ID);

            // Assert
            Assert.That(_stealthSystem.CanUseStealthAbility(TEST_PLAYER_ID, "ambush"), Is.True,
                "Should be able to use stealth ability when stealthed");
        }

        #endregion
    }
}
