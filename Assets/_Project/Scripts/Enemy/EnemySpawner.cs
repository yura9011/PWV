using Unity.Netcode;
using UnityEngine;

namespace EtherDomes.Enemy
{
    /// <summary>
    /// Spawns enemies as NetworkObjects when the server starts.
    /// Uses Unity Netcode for GameObjects (NGO).
    /// </summary>
    public class EnemySpawner : NetworkBehaviour
    {
        [SerializeField] private bool _spawnOnStart = true;
        
        public override void OnNetworkSpawn()
        {
            // In-scene placed NetworkObjects with NetworkObject components are automatically
            // spawned by NGO when StartHost() is called. We should NOT manually spawn them again.
            // The previous logic was causing SpawnStateException because enemies were already spawned.
            Debug.Log("[EnemySpawner] OnNetworkSpawn. In-scene enemies auto-spawned by NGO.");
            
            // If you need to spawn enemies at runtime (not pre-placed), instantiate them here instead.
            // if (IsServer && _spawnOnStart)
            // {
            //     SpawnAllEnemies();
            // }
        }
        
        /// <summary>
        /// Spawn all child enemies as NetworkObjects.
        /// </summary>
        public void SpawnAllEnemies()
        {
            if (!IsServer) return;

            int spawnedCount = 0;
            
            var enemies = GetComponentsInChildren<Enemy>(true);
            foreach (var enemy in enemies)
            {
                var networkObject = enemy.GetComponent<NetworkObject>();
                if (networkObject != null && !networkObject.IsSpawned)
                {
                    try
                    {
                        networkObject.Spawn();
                        spawnedCount++;
                    }
                    catch (SpawnStateException)
                    {
                        // Object was already spawned by NGO (in-scene placed NetworkObject)
                        // This is fine, just skip it
                        Debug.LogWarning($"[EnemySpawner] {enemy.name} already spawned, skipping.");
                    }
                }
            }
            
            Debug.Log($"[EnemySpawner] Spawned {spawnedCount} enemies on network");
        }
    }
}
