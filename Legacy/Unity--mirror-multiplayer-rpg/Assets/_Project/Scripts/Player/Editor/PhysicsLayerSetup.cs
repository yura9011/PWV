#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace EtherDomes.Player.Editor
{
    /// <summary>
    /// Editor utility to configure physics layers for player non-collision.
    /// </summary>
    public static class PhysicsLayerSetup
    {
        private const int PLAYER_LAYER = 9; // Layer 9 = "Player"

        [MenuItem("EtherDomes/Configure Player Physics Layer")]
        public static void ConfigurePlayerPhysicsLayer()
        {
            // Disable Player-Player collision
            Physics.IgnoreLayerCollision(PLAYER_LAYER, PLAYER_LAYER, true);
            
            Debug.Log("[PhysicsLayerSetup] Configured Player layer (9) to ignore Player-Player collisions");
            
            EditorUtility.DisplayDialog(
                "Physics Layer Configured",
                "Player-Player collision has been disabled.\n\n" +
                "Layer 9 (Player) will no longer collide with itself.\n" +
                "Players can now pass through each other.",
                "OK"
            );
        }

        [MenuItem("EtherDomes/Verify Player Physics Layer")]
        public static void VerifyPlayerPhysicsLayer()
        {
            bool ignoresCollision = Physics.GetIgnoreLayerCollision(PLAYER_LAYER, PLAYER_LAYER);
            
            string message = ignoresCollision
                ? "✓ Player-Player collision is DISABLED (correct)"
                : "✗ Player-Player collision is ENABLED (needs configuration)";
            
            Debug.Log($"[PhysicsLayerSetup] {message}");
            
            EditorUtility.DisplayDialog(
                "Physics Layer Status",
                message,
                "OK"
            );
        }
    }
}
#endif
