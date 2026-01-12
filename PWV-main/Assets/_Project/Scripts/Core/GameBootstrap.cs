using UnityEngine;

namespace EtherDomes.Core
{
    /// <summary>
    /// Bootstrap script that initializes game systems on startup.
    /// Attach to a GameObject in the first scene that loads.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            // Physics layer configuration is now handled by PlayerPhysicsConfig in EtherDomes.Player
            Debug.Log("[GameBootstrap] Game initialized");
        }
    }
}
