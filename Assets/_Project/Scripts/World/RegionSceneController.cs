using EtherDomes.Data;
using UnityEngine;

namespace EtherDomes.World
{
    /// <summary>
    /// Controller for region scenes. Handles enemy spawning and dungeon entrances.
    /// </summary>
    public class RegionSceneController : MonoBehaviour
    {
        [Header("Region Settings")]
        [SerializeField] private RegionId _regionId = RegionId.Roca;
        [SerializeField] private int _minEnemyLevel = 1;
        [SerializeField] private int _maxEnemyLevel = 15;

        [Header("Spawn Points")]
        [SerializeField] private Transform[] _playerSpawnPoints;
        [SerializeField] private Transform[] _enemySpawnPoints;

        [Header("Dungeon Entrances")]
        [SerializeField] private DungeonEntrance[] _dungeonEntrances;

        private IWorldManager _worldManager;

        public RegionId RegionId => _regionId;
        public int MinEnemyLevel => _minEnemyLevel;
        public int MaxEnemyLevel => _maxEnemyLevel;

        private void Start()
        {
            _worldManager = FindFirstObjectByType<WorldManager>();
            InitializeRegion();
        }

        private void InitializeRegion()
        {
            Debug.Log($"[RegionSceneController] Initializing region: {_regionId} (Levels {_minEnemyLevel}-{_maxEnemyLevel})");
            
            // Spawn initial enemies
            SpawnEnemies();
        }

        private void SpawnEnemies()
        {
            if (_enemySpawnPoints == null || _enemySpawnPoints.Length == 0)
            {
                Debug.LogWarning("[RegionSceneController] No enemy spawn points configured");
                return;
            }

            foreach (var spawnPoint in _enemySpawnPoints)
            {
                if (spawnPoint == null) continue;

                // In production, this would spawn actual enemy prefabs
                int enemyLevel = Random.Range(_minEnemyLevel, _maxEnemyLevel + 1);
                Debug.Log($"[RegionSceneController] Would spawn level {enemyLevel} enemy at {spawnPoint.position}");
            }
        }

        /// <summary>
        /// Get a random player spawn point.
        /// </summary>
        public Vector3 GetPlayerSpawnPoint()
        {
            if (_playerSpawnPoints == null || _playerSpawnPoints.Length == 0)
                return Vector3.zero;

            var spawnPoint = _playerSpawnPoints[Random.Range(0, _playerSpawnPoints.Length)];
            return spawnPoint != null ? spawnPoint.position : Vector3.zero;
        }

        /// <summary>
        /// Get enemy level for this region (clamped to region bounds).
        /// </summary>
        public int GetEnemyLevel()
        {
            return Random.Range(_minEnemyLevel, _maxEnemyLevel + 1);
        }
    }

    /// <summary>
    /// Dungeon entrance trigger.
    /// </summary>
    [System.Serializable]
    public class DungeonEntrance
    {
        public string DungeonId;
        public Transform EntrancePoint;
        public int RequiredLevel;
    }
}
