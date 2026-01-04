using NUnit.Framework;
using UnityEngine;

namespace EtherDomes.Tests
{
    /// <summary>
    /// Tests for physics layer configuration.
    /// </summary>
    [TestFixture]
    public class PhysicsLayerTests
    {
        private const int PLAYER_LAYER = 9;

        #region Property 8: Player-Player Non-Collision

        /// <summary>
        /// Feature: network-player-foundation, Property 8: Player-Player Non-Collision
        /// For any two player colliders, Physics.GetIgnoreLayerCollision for the player layer
        /// SHALL return true, allowing both to occupy the same space.
        /// Validates: Requirements 5.1, 5.2
        /// </summary>
        [Test]
        public void Property8_PlayerPlayerNonCollision_LayerIgnoresItself()
        {
            // Verify that Player layer (9) ignores collision with itself
            bool ignoresCollision = Physics.GetIgnoreLayerCollision(PLAYER_LAYER, PLAYER_LAYER);
            
            Assert.IsTrue(ignoresCollision,
                "Player layer should ignore collision with itself");
        }

        /// <summary>
        /// Feature: network-player-foundation, Property 8: Player-Player Non-Collision
        /// Verifies that Player layer still collides with other layers.
        /// Validates: Requirements 5.3
        /// </summary>
        [Test]
        public void Property8_PlayerPlayerNonCollision_CollidesWithOtherLayers()
        {
            // Player should still collide with Default layer (0)
            bool collidesWithDefault = !Physics.GetIgnoreLayerCollision(PLAYER_LAYER, 0);
            Assert.IsTrue(collidesWithDefault,
                "Player layer should collide with Default layer");

            // Player should still collide with Terrain layer (8)
            bool collidesWithTerrain = !Physics.GetIgnoreLayerCollision(PLAYER_LAYER, 8);
            Assert.IsTrue(collidesWithTerrain,
                "Player layer should collide with Terrain layer");
        }

        /// <summary>
        /// Feature: network-player-foundation, Property 8: Player-Player Non-Collision
        /// Tests that the Player layer exists at the expected index.
        /// Validates: Requirements 5.1
        /// </summary>
        [Test]
        public void Property8_PlayerPlayerNonCollision_PlayerLayerExists()
        {
            string layerName = LayerMask.LayerToName(PLAYER_LAYER);
            Assert.AreEqual("Player", layerName,
                $"Layer {PLAYER_LAYER} should be named 'Player', but was '{layerName}'");
        }

        #endregion

        #region Unit Tests

        [Test]
        public void PlayerLayerMask_CanBeRetrieved()
        {
            int playerLayerMask = LayerMask.GetMask("Player");
            Assert.Greater(playerLayerMask, 0, "Player layer mask should be retrievable");
        }

        [Test]
        public void PlayerLayerIndex_IsCorrect()
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            Assert.AreEqual(PLAYER_LAYER, playerLayer,
                "Player layer should be at index 9");
        }

        #endregion
    }
}
