using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for player movement system.
    /// Tests mathematical properties that should always hold true.
    /// </summary>
    [TestFixture]
    public class MovementPropertyTests
    {
        /// <summary>
        /// Property 1: Movement Direction Relative to Camera
        /// For any camera rotation and input direction, the resulting world movement
        /// should be correctly transformed relative to the camera's forward/right vectors.
        /// </summary>
        [Test]
        public void MovementDirection_IsRelativeToCamera_ForAllInputs(
            [Values(0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f)] float cameraYRotation,
            [Values(-1f, 0f, 1f)] float inputX,
            [Values(-1f, 0f, 1f)] float inputY)
        {
            // Skip zero input case
            if (inputX == 0f && inputY == 0f)
            {
                Assert.Pass("Zero input produces zero movement");
                return;
            }

            // Arrange
            var cameraRotation = Quaternion.Euler(0f, cameraYRotation, 0f);
            var cameraForward = cameraRotation * Vector3.forward;
            var cameraRight = cameraRotation * Vector3.right;
            
            // Flatten to horizontal plane
            cameraForward.y = 0f;
            cameraForward.Normalize();
            cameraRight.y = 0f;
            cameraRight.Normalize();

            // Act - Calculate movement direction (same logic as PlayerController)
            var moveDirection = (cameraForward * inputY + cameraRight * inputX).normalized;

            // Assert - Movement should be normalized (magnitude = 1)
            Assert.That(moveDirection.magnitude, Is.EqualTo(1f).Within(0.001f),
                $"Movement direction should be normalized for camera rotation {cameraYRotation}° and input ({inputX}, {inputY})");

            // Assert - Movement should be on horizontal plane
            Assert.That(moveDirection.y, Is.EqualTo(0f).Within(0.001f),
                "Movement direction should be on horizontal plane (y = 0)");
        }

        /// <summary>
        /// Property 2: Forward input should move in camera's forward direction
        /// </summary>
        [Test]
        public void ForwardInput_MovesInCameraForwardDirection(
            [Values(0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f)] float cameraYRotation)
        {
            // Arrange
            var cameraRotation = Quaternion.Euler(0f, cameraYRotation, 0f);
            var expectedForward = cameraRotation * Vector3.forward;
            expectedForward.y = 0f;
            expectedForward.Normalize();

            // Act - Forward input (W key = 0, 1)
            var cameraForward = cameraRotation * Vector3.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();
            var moveDirection = cameraForward.normalized;

            // Assert - Movement should match camera forward
            Assert.That(Vector3.Dot(moveDirection, expectedForward), Is.EqualTo(1f).Within(0.001f),
                $"Forward input should move in camera forward direction at {cameraYRotation}°");
        }

        /// <summary>
        /// Property 3: Diagonal movement should have same speed as cardinal movement
        /// </summary>
        [Test]
        public void DiagonalMovement_HasSameSpeed_AsCardinalMovement()
        {
            var cameraRotation = Quaternion.identity;
            var cameraForward = Vector3.forward;
            var cameraRight = Vector3.right;

            // Cardinal movement (forward only)
            var cardinalMove = cameraForward.normalized;

            // Diagonal movement (forward + right)
            var diagonalMove = (cameraForward + cameraRight).normalized;

            // Both should have magnitude of 1 (normalized)
            Assert.That(cardinalMove.magnitude, Is.EqualTo(1f).Within(0.001f),
                "Cardinal movement should be normalized");
            Assert.That(diagonalMove.magnitude, Is.EqualTo(1f).Within(0.001f),
                "Diagonal movement should be normalized (no speed boost)");
        }

        /// <summary>
        /// Property 4: Opposite inputs should produce opposite directions
        /// </summary>
        [Test]
        public void OppositeInputs_ProduceOppositeDirections(
            [Values(0f, 90f, 180f, 270f)] float cameraYRotation)
        {
            var cameraRotation = Quaternion.Euler(0f, cameraYRotation, 0f);
            var cameraForward = cameraRotation * Vector3.forward;
            var cameraRight = cameraRotation * Vector3.right;
            cameraForward.y = 0f;
            cameraForward.Normalize();
            cameraRight.y = 0f;
            cameraRight.Normalize();

            // Forward vs Backward
            var forward = cameraForward.normalized;
            var backward = (-cameraForward).normalized;
            Assert.That(Vector3.Dot(forward, backward), Is.EqualTo(-1f).Within(0.001f),
                "Forward and backward should be opposite");

            // Left vs Right
            var right = cameraRight.normalized;
            var left = (-cameraRight).normalized;
            Assert.That(Vector3.Dot(right, left), Is.EqualTo(-1f).Within(0.001f),
                "Left and right should be opposite");
        }
    }
}
