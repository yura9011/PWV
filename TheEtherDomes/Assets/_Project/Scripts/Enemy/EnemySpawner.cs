using Unity.Netcode;
using UnityEngine;

namespace EtherDomes.Enemy
{
    /// <summary>
    /// Spawns enemies as NetworkObjects when the server starts.
    /// Place this on a GameObject with child Enemy objects to spawn them on the network.
    /// </summary>
    public class EnemySpawner : NetworkBehaviour
    {
        [SerializeField] private bool _spawnOnStart = true;
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if (IsServer && _spawnOnStart)
            {
                SpawnAllEnemies();
            }
        }
        
        /// <summary>
        /// Spawn all child enemies as NetworkObjects.
        /// </summary>
        public void SpawnAllEnemies()
        {
            if (!IsServer)
            {
                Debug.LogWarning("[EnemySpawner] Only server can spawn enemies");
                return;
            }
            
            int spawnedCount = 0;
            
            // Find all Enemy components in children
            var enemies = GetComponentsInChildren<Enemy>(true);
            foreach (var enemy in enemies)
            {
                var networkObject = enemy.GetComponent<NetworkObject>();
                if (networkObject != null && !networkObject.IsSpawned)
                {
                    networkObject.Spawn();
                    spawnedCount++;
                }
            }
            
            Debug.Log($"[EnemySpawner] Spawned {spawnedCount} enemies on network");
        }
    }
}
