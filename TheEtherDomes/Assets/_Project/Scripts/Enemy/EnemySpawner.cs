using Mirror;
using UnityEngine;

namespace EtherDomes.Enemy
{
    /// <summary>
    /// Spawns enemies as NetworkObjects when the server starts.
    /// </summary>
    public class EnemySpawner : NetworkBehaviour
    {
        [SerializeField] private bool _spawnOnStart = true;
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            
            if (_spawnOnStart)
            {
                SpawnAllEnemies();
            }
        }
        
        /// <summary>
        /// Spawn all child enemies as NetworkObjects.
        /// </summary>
        [Server]
        public void SpawnAllEnemies()
        {
            int spawnedCount = 0;
            
            var enemies = GetComponentsInChildren<Enemy>(true);
            foreach (var enemy in enemies)
            {
                var networkIdentity = enemy.GetComponent<NetworkIdentity>();
                if (networkIdentity != null)
                {
                    NetworkServer.Spawn(enemy.gameObject);
                    spawnedCount++;
                }
            }
            
            Debug.Log($"[EnemySpawner] Spawned {spawnedCount} enemies on network");
        }
    }
}
