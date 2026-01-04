using UnityEngine;

namespace EtherDomes.Player
{
    /// <summary>
    /// Interface for player movement controller with network ownership verification.
    /// </summary>
    public interface IPlayerController
    {
        /// <summary>
        /// Processes movement input. Only processes if IsLocalPlayer is true.
        /// </summary>
        /// <param name="input">2D movement input (x = horizontal, y = vertical)</param>
        void ProcessMovementInput(Vector2 input);

        /// <summary>
        /// Sets the movement speed.
        /// </summary>
        /// <param name="speed">Movement speed in units per second</param>
        void SetMovementSpeed(float speed);

        /// <summary>
        /// Whether this is the local player (has authority over input).
        /// </summary>
        bool IsLocalPlayer { get; }

        /// <summary>
        /// Whether the local client owns this object.
        /// </summary>
        bool IsOwned { get; }

        /// <summary>
        /// Current velocity of the player.
        /// </summary>
        Vector3 CurrentVelocity { get; }
    }
}
