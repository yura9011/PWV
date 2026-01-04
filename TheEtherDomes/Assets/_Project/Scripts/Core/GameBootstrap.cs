using EtherDomes.Player;
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
            // Configure physics layers
            PlayerPhysicsConfig.ConfigurePlayerCollisions();

            Debug.Log("[GameBootstrap] Game initialized");
        }
    }
}
