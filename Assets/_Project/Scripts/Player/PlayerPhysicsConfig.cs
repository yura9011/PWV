using UnityEngine;

namespace EtherDomes.Player
{
    /// <summary>
    /// Configures physics layers to prevent player-to-player collisions.
    /// Attach to a GameObject in the scene or use the static Initialize method.
    /// </summary>
    public class PlayerPhysicsConfig : MonoBehaviour
    {
        public const string PLAYER_LAYER_NAME = "Player";
        public const int PLAYER_LAYER_INDEX = 8; // Default Unity layer 8

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            ConfigurePlayerCollisions();
        }

        /// <summary>
        /// Configure physics to ignore player-to-player collisions.
        /// </summary>
        public static void ConfigurePlayerCollisions()
        {
            int playerLayer = LayerMask.NameToLayer(PLAYER_LAYER_NAME);
            
            if (playerLayer == -1)
            {
                // Layer doesn't exist, use default layer 8
                playerLayer = PLAYER_LAYER_INDEX;
                Debug.LogWarning($"[PlayerPhysicsConfig] '{PLAYER_LAYER_NAME}' layer not found. Using layer {PLAYER_LAYER_INDEX}. Please create the layer in Project Settings > Tags and Layers.");
            }

            // Ignore collisions between players
            Physics.IgnoreLayerCollision(playerLayer, playerLayer, true);
            
            Debug.Log($"[PlayerPhysicsConfig] Player-to-player collisions disabled on layer {playerLayer}");
        }

        /// <summary>
        /// Set a GameObject to the Player layer.
        /// </summary>
        public static void SetPlayerLayer(GameObject obj)
        {
            if (obj == null) return;

            int playerLayer = LayerMask.NameToLayer(PLAYER_LAYER_NAME);
            if (playerLayer == -1)
            {
                playerLayer = PLAYER_LAYER_INDEX;
            }

            obj.layer = playerLayer;

            // Also set children
            foreach (Transform child in obj.transform)
            {
                SetPlayerLayer(child.gameObject);
            }
        }

        /// <summary>
        /// Check if a GameObject is on the Player layer.
        /// </summary>
        public static bool IsPlayerLayer(GameObject obj)
        {
            if (obj == null) return false;

            int playerLayer = LayerMask.NameToLayer(PLAYER_LAYER_NAME);
            if (playerLayer == -1)
            {
                playerLayer = PLAYER_LAYER_INDEX;
            }

            return obj.layer == playerLayer;
        }
    }
}
