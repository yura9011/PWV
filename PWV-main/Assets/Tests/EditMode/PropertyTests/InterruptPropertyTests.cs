using NUnit.Framework;
using EtherDomes.Combat;
using UnityEngine;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for the Interrupt system.
    /// </summary>
    [TestFixture]
    public class InterruptPropertyTests
    {
        /// <summary>
        /// Feature: mvp-10-features, Property 29: Interrupt Lockout Duration
        /// After interrupt, target SHALL be locked out for 4 seconds.
        /// Validates: Requirements 9.7
        /// </summary>
        [Test]
        public void InterruptLockout_DefaultDuration_IsFourSeconds()
        {
            // Assert
            Assert.That(InterruptSystem.DEFAULT_LOCKOUT_DURATION, Is.EqualTo(4f),
                "Default lockout duration should be 4 seconds");
        }

        /// <summary>
        /// Property: TryInterrupt applies lockout
        /// </summary>
        [Test]
        public void TryInterrupt_AppliesLockout()
        {
            // Arrange
            var interruptGO = new GameObject("InterruptSystem");
            var interruptSystem = interruptGO.AddComponent<InterruptSystem>();

            ulong interrupterId = 1;
            ulong targetId = 2;

            // Act
            bool result = interruptSystem.TryInterrupt(interrupterId, targetId);

            // Assert
            Assert.That(result, Is.True, "TryInterrupt should return true");
            Assert.That(interruptSystem.IsLockedOut(targetId), Is.True,
                "Target should be locked out after interrupt");

            // Cleanup
            Object.DestroyImmediate(interruptGO);
        }

        /// <summary>
        /// Property: IsLockedOut returns false when not locked
        /// </summary>
        [Test]
        public void IsLockedOut_NotLocked_ReturnsFalse()
        {
            // Arrange
            var interruptGO = new GameObject("InterruptSystem");
            var interruptSystem = interruptGO.AddComponent<InterruptSystem>();

            ulong targetId = 1;

            // Act & Assert
            Assert.That(interruptSystem.IsLockedOut(targetId), Is.False,
                "Target should not be locked out initially");

            // Cleanup
            Object.DestroyImmediate(interruptGO);
        }

        /// <summary>
        /// Property: ApplyLockout makes target locked out
        /// </summary>
        [Test]
        public void ApplyLockout_MakesTargetLockedOut()
        {
            // Arrange
            var interruptGO = new GameObject("InterruptSystem");
            var interruptSystem = interruptGO.AddComponent<InterruptSystem>();

            ulong targetId = 1;
            float duration = 5f;

            // Act
            interruptSystem.ApplyLockout(targetId, duration);

            // Assert
            Assert.That(interruptSystem.IsLockedOut(targetId), Is.True,
                "Target should be locked out after ApplyLockout");

            // Cleanup
            Object.DestroyImmediate(interruptGO);
        }

        /// <summary>
        /// Property: GetLockoutRemaining returns correct value
        /// </summary>
        [Test]
        public void GetLockoutRemaining_ReturnsCorrectValue()
        {
            // Arrange
            var interruptGO = new GameObject("InterruptSystem");
            var interruptSystem = interruptGO.AddComponent<InterruptSystem>();

            ulong targetId = 1;
            float duration = 10f;

            // Act
            interruptSystem.ApplyLockout(targetId, duration);
            float remaining = interruptSystem.GetLockoutRemaining(targetId);

            // Assert - remaining should be close to duration (within frame time)
            Assert.That(remaining, Is.InRange(duration - 0.1f, duration),
                "Lockout remaining should be close to applied duration");

            // Cleanup
            Object.DestroyImmediate(interruptGO);
        }

        /// <summary>
        /// Property: GetLockoutRemaining returns 0 when not locked
        /// </summary>
        [Test]
        public void GetLockoutRemaining_NotLocked_ReturnsZero()
        {
            // Arrange
            var interruptGO = new GameObject("InterruptSystem");
            var interruptSystem = interruptGO.AddComponent<InterruptSystem>();

            ulong targetId = 1;

            // Act
            float remaining = interruptSystem.GetLockoutRemaining(targetId);

            // Assert
            Assert.That(remaining, Is.EqualTo(0f),
                "Lockout remaining should be 0 when not locked");

            // Cleanup
            Object.DestroyImmediate(interruptGO);
        }

        /// <summary>
        /// Property: ClearLockout removes lockout
        /// </summary>
        [Test]
        public void ClearLockout_RemovesLockout()
        {
            // Arrange
            var interruptGO = new GameObject("InterruptSystem");
            var interruptSystem = interruptGO.AddComponent<InterruptSystem>();

            ulong targetId = 1;
            interruptSystem.ApplyLockout(targetId, 10f);
            Assert.That(interruptSystem.IsLockedOut(targetId), Is.True);

            // Act
            interruptSystem.ClearLockout(targetId);

            // Assert
            Assert.That(interruptSystem.IsLockedOut(targetId), Is.False,
                "Target should not be locked out after ClearLockout");

            // Cleanup
            Object.DestroyImmediate(interruptGO);
        }

        /// <summary>
        /// Property: OnCastInterrupted fires on interrupt
        /// </summary>
        [Test]
        public void OnCastInterrupted_FiresOnInterrupt()
        {
            // Arrange
            var interruptGO = new GameObject("InterruptSystem");
            var interruptSystem = interruptGO.AddComponent<InterruptSystem>();

            bool eventFired = false;
            ulong receivedInterrupterId = 0;
            ulong receivedTargetId = 0;

            interruptSystem.OnCastInterrupted += (interrupterId, targetId) =>
            {
                eventFired = true;
                receivedInterrupterId = interrupterId;
                receivedTargetId = targetId;
            };

            // Act
            interruptSystem.TryInterrupt(1, 2);

            // Assert
            Assert.That(eventFired, Is.True, "OnCastInterrupted should fire");
            Assert.That(receivedInterrupterId, Is.EqualTo(1UL), "Interrupter ID should match");
            Assert.That(receivedTargetId, Is.EqualTo(2UL), "Target ID should match");

            // Cleanup
            Object.DestroyImmediate(interruptGO);
        }

        /// <summary>
        /// Property: OnLockoutApplied fires on lockout
        /// </summary>
        [Test]
        public void OnLockoutApplied_FiresOnLockout()
        {
            // Arrange
            var interruptGO = new GameObject("InterruptSystem");
            var interruptSystem = interruptGO.AddComponent<InterruptSystem>();

            bool eventFired = false;
            ulong receivedTargetId = 0;
            float receivedDuration = 0;

            interruptSystem.OnLockoutApplied += (targetId, duration) =>
            {
                eventFired = true;
                receivedTargetId = targetId;
                receivedDuration = duration;
            };

            // Act
            interruptSystem.ApplyLockout(1, 5f);

            // Assert
            Assert.That(eventFired, Is.True, "OnLockoutApplied should fire");
            Assert.That(receivedTargetId, Is.EqualTo(1UL), "Target ID should match");
            Assert.That(receivedDuration, Is.EqualTo(5f), "Duration should match");

            // Cleanup
            Object.DestroyImmediate(interruptGO);
        }

        /// <summary>
        /// Property: CanCast returns false when locked out
        /// </summary>
        [Test]
        public void CanCast_WhenLockedOut_ReturnsFalse()
        {
            // Arrange
            var interruptGO = new GameObject("InterruptSystem");
            var interruptSystem = interruptGO.AddComponent<InterruptSystem>();

            ulong targetId = 1;
            interruptSystem.ApplyLockout(targetId, 10f);

            // Act & Assert
            Assert.That(interruptSystem.CanCast(targetId), Is.False,
                "CanCast should return false when locked out");

            // Cleanup
            Object.DestroyImmediate(interruptGO);
        }

        /// <summary>
        /// Property: CanCast returns true when not locked out
        /// </summary>
        [Test]
        public void CanCast_WhenNotLockedOut_ReturnsTrue()
        {
            // Arrange
            var interruptGO = new GameObject("InterruptSystem");
            var interruptSystem = interruptGO.AddComponent<InterruptSystem>();

            ulong targetId = 1;

            // Act & Assert
            Assert.That(interruptSystem.CanCast(targetId), Is.True,
                "CanCast should return true when not locked out");

            // Cleanup
            Object.DestroyImmediate(interruptGO);
        }

        /// <summary>
        /// Property: ClearAll removes all lockouts
        /// </summary>
        [Test]
        public void ClearAll_RemovesAllLockouts()
        {
            // Arrange
            var interruptGO = new GameObject("InterruptSystem");
            var interruptSystem = interruptGO.AddComponent<InterruptSystem>();

            for (ulong i = 1; i <= 5; i++)
            {
                interruptSystem.ApplyLockout(i, 10f);
            }

            Assert.That(interruptSystem.GetActiveLockoutCount(), Is.EqualTo(5));

            // Act
            interruptSystem.ClearAll();

            // Assert
            Assert.That(interruptSystem.GetActiveLockoutCount(), Is.EqualTo(0),
                "All lockouts should be cleared");

            for (ulong i = 1; i <= 5; i++)
            {
                Assert.That(interruptSystem.IsLockedOut(i), Is.False,
                    $"Target {i} should not be locked out after ClearAll");
            }

            // Cleanup
            Object.DestroyImmediate(interruptGO);
        }

        /// <summary>
        /// Property: Zero duration lockout is not applied
        /// </summary>
        [Test]
        public void ApplyLockout_ZeroDuration_NotApplied()
        {
            // Arrange
            var interruptGO = new GameObject("InterruptSystem");
            var interruptSystem = interruptGO.AddComponent<InterruptSystem>();

            ulong targetId = 1;

            // Act
            interruptSystem.ApplyLockout(targetId, 0f);

            // Assert
            Assert.That(interruptSystem.IsLockedOut(targetId), Is.False,
                "Zero duration lockout should not be applied");

            // Cleanup
            Object.DestroyImmediate(interruptGO);
        }

        /// <summary>
        /// Property: Negative duration lockout is not applied
        /// </summary>
        [Test]
        public void ApplyLockout_NegativeDuration_NotApplied()
        {
            // Arrange
            var interruptGO = new GameObject("InterruptSystem");
            var interruptSystem = interruptGO.AddComponent<InterruptSystem>();

            ulong targetId = 1;

            // Act
            interruptSystem.ApplyLockout(targetId, -5f);

            // Assert
            Assert.That(interruptSystem.IsLockedOut(targetId), Is.False,
                "Negative duration lockout should not be applied");

            // Cleanup
            Object.DestroyImmediate(interruptGO);
        }
    }
}
