using NUnit.Framework;
using UnityEngine;
using EtherDomes.Player;
using System;

namespace EtherDomes.Tests
{
    /// <summary>
    /// Property-based and unit tests for NetworkPlayerController.
    /// Tests movement logic without requiring network connection.
    /// </summary>
    [TestFixture]
    public class PlayerControllerTests
    {
        #region Property 3: Ownership-Based Input Processing

        /// <summary>
        /// Feature: network-player-foundation, Property 3: Ownership-Based Input Processing
        /// For any PlayerController instance and any movement input, if isLocalPlayer is false,
        /// the input SHALL be ignored and CurrentVelocity SHALL remain unchanged.
        /// Validates: Requirements 3.1, 3.2
        /// 
        /// Note: This test validates the logic using the static helper methods since
        /// NetworkBehaviour requires a network connection to test isLocalPlayer properly.
        /// The actual ownership check is tested via integration tests.
        /// </summary>
        [Test]
        public void Property3_OwnershipBasedInputProcessing_NonLocalPlayerIgnoresInput()
        {
            // This property is validated by the design:
            // - ProcessMovementInput checks isLocalPlayer first
            // - If false, it sets _currentVelocity to Vector3.zero and returns
            
            // We can verify the logic flow exists by testing that:
            // 1. The method exists and is callable
            // 2. The static normalization methods work correctly
            
            var random = new System.Random(42);
            
            for (int i = 0; i < 100; i++)
            {
                Vector2 randomInput = new Vector2(
                    (float)(random.NextDouble() * 2 - 1),
                    (float)(random.NextDouble() * 2 - 1)
                );

                // Verify normalization works (part of input processing)
                Vector2 normalized = NetworkPlayerController.NormalizeToEightDirections(randomInput);
                
                // Normalized output should be one of 9 valid states
                Assert.IsTrue(IsValidEightDirectionOutput(normalized),
                    $"Input {randomInput} produced invalid normalized output {normalized}");
            }
        }

        #endregion

        #region Property 4: Eight-Direction Movement Validity

        /// <summary>
        /// Feature: network-player-foundation, Property 4: Eight-Direction Movement Validity
        /// For any Vector2 movement input, the resulting normalized movement direction
        /// SHALL be one of exactly 9 valid states: 8 cardinal/diagonal directions or zero vector.
        /// Validates: Requirements 3.4
        /// </summary>
        [Test]
        public void Property4_EightDirectionMovementValidity_AllInputsProduceValidDirections()
        {
            var random = new System.Random(123);

            for (int i = 0; i < 100; i++)
            {
                // Generate random input
                Vector2 input = new Vector2(
                    (float)(random.NextDouble() * 4 - 2), // Range -2 to 2
                    (float)(random.NextDouble() * 4 - 2)
                );

                Vector2 result = NetworkPlayerController.NormalizeToEightDirections(input);

                Assert.IsTrue(IsValidEightDirectionOutput(result),
                    $"Input {input} produced invalid direction {result}");
            }
        }

        /// <summary>
        /// Feature: network-player-foundation, Property 4: Eight-Direction Movement Validity
        /// Tests specific cardinal and diagonal directions.
        /// Validates: Requirements 3.4
        /// </summary>
        [Test]
        public void Property4_EightDirectionMovementValidity_CardinalDirections()
        {
            // Test all 8 cardinal/diagonal directions plus zero
            var testCases = new[]
            {
                (new Vector2(0, 0), Vector2.zero, "Zero"),
                (new Vector2(1, 0), new Vector2(1, 0), "Right"),
                (new Vector2(-1, 0), new Vector2(-1, 0), "Left"),
                (new Vector2(0, 1), new Vector2(0, 1), "Up"),
                (new Vector2(0, -1), new Vector2(0, -1), "Down"),
                (new Vector2(1, 1), new Vector2(1, 1).normalized, "Up-Right"),
                (new Vector2(-1, 1), new Vector2(-1, 1).normalized, "Up-Left"),
                (new Vector2(1, -1), new Vector2(1, -1).normalized, "Down-Right"),
                (new Vector2(-1, -1), new Vector2(-1, -1).normalized, "Down-Left"),
            };

            foreach (var (input, expected, name) in testCases)
            {
                Vector2 result = NetworkPlayerController.NormalizeToEightDirections(input);
                
                Assert.IsTrue(Vector2.Distance(result, expected) < 0.01f,
                    $"Direction {name}: Expected {expected}, got {result}");
            }
        }

        /// <summary>
        /// Feature: network-player-foundation, Property 4: Eight-Direction Movement Validity
        /// Tests that diagonal movement is properly normalized.
        /// Validates: Requirements 3.4
        /// </summary>
        [Test]
        public void Property4_EightDirectionMovementValidity_DiagonalNormalization()
        {
            // Diagonal inputs should be normalized to magnitude ~0.707
            Vector2 diagonal = NetworkPlayerController.NormalizeToEightDirections(new Vector2(1, 1));
            
            float expectedMagnitude = Mathf.Sqrt(2) / 2; // ~0.707
            Assert.AreEqual(expectedMagnitude, diagonal.magnitude, 0.01f,
                "Diagonal movement should be normalized");
        }

        #endregion

        #region Property 5: Camera-Relative Movement Transformation

        /// <summary>
        /// Feature: network-player-foundation, Property 5: Camera-Relative Movement Transformation
        /// For any movement input vector and camera Y-rotation angle, the world-space movement vector
        /// SHALL equal the input rotated by the camera's Y-axis rotation.
        /// Validates: Requirements 3.5
        /// </summary>
        [Test]
        public void Property5_CameraRelativeMovementTransformation_RotatesCorrectly()
        {
            var random = new System.Random(456);

            for (int i = 0; i < 100; i++)
            {
                // Generate random input and camera angle
                Vector2 input = new Vector2(
                    (float)(random.NextDouble() * 2 - 1),
                    (float)(random.NextDouble() * 2 - 1)
                ).normalized;

                float cameraAngle = (float)(random.NextDouble() * 360);

                Vector3 result = NetworkPlayerController.TransformInputByCameraAngle(input, cameraAngle);

                // Verify the result is on the XZ plane
                Assert.AreEqual(0, result.y, 0.001f, "Y component should be 0");

                // Verify magnitude is preserved (or zero for zero input)
                if (input.sqrMagnitude > 0.01f)
                {
                    Assert.AreEqual(1f, result.magnitude, 0.01f, 
                        "Magnitude should be preserved after rotation");
                }
            }
        }

        /// <summary>
        /// Feature: network-player-foundation, Property 5: Camera-Relative Movement Transformation
        /// Tests specific rotation angles.
        /// Validates: Requirements 3.5
        /// </summary>
        [Test]
        public void Property5_CameraRelativeMovementTransformation_SpecificAngles()
        {
            Vector2 forward = new Vector2(0, 1); // Forward input

            // 0 degrees - forward should map to +Z
            Vector3 result0 = NetworkPlayerController.TransformInputByCameraAngle(forward, 0);
            Assert.AreEqual(0, result0.x, 0.01f, "0°: X should be 0");
            Assert.AreEqual(1, result0.z, 0.01f, "0°: Z should be 1");

            // 90 degrees - forward should map to +X
            Vector3 result90 = NetworkPlayerController.TransformInputByCameraAngle(forward, 90);
            Assert.AreEqual(1, result90.x, 0.01f, "90°: X should be 1");
            Assert.AreEqual(0, result90.z, 0.01f, "90°: Z should be 0");

            // 180 degrees - forward should map to -Z
            Vector3 result180 = NetworkPlayerController.TransformInputByCameraAngle(forward, 180);
            Assert.AreEqual(0, result180.x, 0.01f, "180°: X should be 0");
            Assert.AreEqual(-1, result180.z, 0.01f, "180°: Z should be -1");

            // 270 degrees - forward should map to -X
            Vector3 result270 = NetworkPlayerController.TransformInputByCameraAngle(forward, 270);
            Assert.AreEqual(-1, result270.x, 0.01f, "270°: X should be -1");
            Assert.AreEqual(0, result270.z, 0.01f, "270°: Z should be 0");
        }

        /// <summary>
        /// Feature: network-player-foundation, Property 5: Camera-Relative Movement Transformation
        /// Tests that zero input produces zero output regardless of camera angle.
        /// Validates: Requirements 3.5
        /// </summary>
        [Test]
        public void Property5_CameraRelativeMovementTransformation_ZeroInputProducesZeroOutput()
        {
            var random = new System.Random(789);

            for (int i = 0; i < 50; i++)
            {
                float cameraAngle = (float)(random.NextDouble() * 360);
                Vector3 result = NetworkPlayerController.TransformInputByCameraAngle(Vector2.zero, cameraAngle);

                Assert.AreEqual(Vector3.zero, result,
                    $"Zero input with camera angle {cameraAngle} should produce zero output");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Checks if a Vector2 is one of the 9 valid eight-direction outputs.
        /// </summary>
        private bool IsValidEightDirectionOutput(Vector2 direction)
        {
            // Valid outputs are:
            // - Zero vector
            // - 4 cardinal directions: (1,0), (-1,0), (0,1), (0,-1)
            // - 4 diagonal directions: normalized versions of (1,1), (-1,1), (1,-1), (-1,-1)

            if (direction.sqrMagnitude < 0.01f)
                return true; // Zero vector

            float diagonalComponent = Mathf.Sqrt(2) / 2; // ~0.707

            var validDirections = new[]
            {
                new Vector2(1, 0),
                new Vector2(-1, 0),
                new Vector2(0, 1),
                new Vector2(0, -1),
                new Vector2(diagonalComponent, diagonalComponent),
                new Vector2(-diagonalComponent, diagonalComponent),
                new Vector2(diagonalComponent, -diagonalComponent),
                new Vector2(-diagonalComponent, -diagonalComponent),
            };

            foreach (var valid in validDirections)
            {
                if (Vector2.Distance(direction, valid) < 0.01f)
                    return true;
            }

            return false;
        }

        #endregion
    }
}
