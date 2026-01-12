#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace EtherDomes.Editor
{
    /// <summary>
    /// Editor utility to configure physics layers for player non-collision.
    /// </summary>
    public static class PhysicsLayerSetup
    {
        private const int PLAYER_LAYER = 9;

        [MenuItem("EtherDomes/Configure Player Physics Layer")]
        public static void ConfigurePlayerPhysicsLayer()
        {
            Physics.IgnoreLayerCollision(PLAYER_LAYER, PLAYER_LAYER, true);
            
            UnityEngine.Debug.Log("[PhysicsLayerSetup] Configured Player layer (9) to ignore Player-Player collisions");
            
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
            
            UnityEngine.Debug.Log($"[PhysicsLayerSetup] {message}");
            
            EditorUtility.DisplayDialog("Physics Layer Status", message, "OK");
        }

        [MenuItem("EtherDomes/Setup Player Layer")]
        public static void SetupPlayerLayer()
        {
            EditorUtility.DisplayDialog(
                "Setup Player Layer",
                "To setup the Player layer:\n\n" +
                "1. Go to Edit > Project Settings > Tags and Layers\n" +
                "2. Add 'Player' to Layer 9\n" +
                "3. Go to Edit > Project Settings > Physics\n" +
                "4. In the Layer Collision Matrix, uncheck Player-Player collision\n\n" +
                "Or use: EtherDomes > Configure Player Physics Layer",
                "OK"
            );
        }
    }
}
#endif
