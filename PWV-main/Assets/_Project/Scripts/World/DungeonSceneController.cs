using EtherDomes.Data;
using UnityEngine;

namespace EtherDomes.World
{
    /// <summary>
    /// Controller for dungeon scenes. Handles boss encounters and instance management.
    /// </summary>
    public class DungeonSceneController : MonoBehaviour
    {
        [Header("Dungeon Settings")]
        [SerializeField] private string _dungeonId = "dungeon_roca_small";
        [SerializeField] private DungeonSize _size = DungeonSize.Small;

        [Header("Boss Encounters")]
        [SerializeField] private BossEncounterTrigger[] _bossEncounters;

        [Header("Spawn Points")]
        [SerializeField] private Transform _entranceSpawnPoint;
        [SerializeField] private Transform[] _checkpointSpawnPoints;

        private IDungeonSystem _dungeonSystem;
        private IBossSystem _bossSystem;
        private string _currentInstanceId;

        public string DungeonId => _dungeonId;
        public DungeonSize Size => _size;
        public int BossCount => _size == DungeonSize.Large ? 5 : 3;

        private void Start()
        {
            _dungeonSystem = FindFirstObjectByType<DungeonSystem>();
            _bossSystem = FindFirstObjectByType<BossSystem>();

            InitializeDungeon();
        }

        private void InitializeDungeon()
        {
            Debug.Log($"[DungeonSceneController] Initializing dungeon: {_dungeonId} ({_size}, {BossCount} bosses)");

            // Subscribe to events
            if (_dungeonSystem != null)
            {
                _dungeonSystem.OnBossDefeated += OnBossDefeated;
                _dungeonSystem.OnDungeonCompleted += OnDungeonCompleted;
                _dungeonSystem.OnWipe += OnWipe;
            }
        }

        private void OnDestroy()
        {
            if (_dungeonSystem != null)
            {
                _dungeonSystem.OnBossDefeated -= OnBossDefeated;
                _dungeonSystem.OnDungeonCompleted -= OnDungeonCompleted;
                _dungeonSystem.OnWipe -= OnWipe;
            }
        }

        /// <summary>
        /// Set the instance ID for this dungeon scene.
        /// </summary>
        public void SetInstanceId(string instanceId)
        {
            _currentInstanceId = instanceId;
            Debug.Log($"[DungeonSceneController] Instance ID set: {instanceId}");
        }

        /// <summary>
        /// Start a boss encounter.
        /// </summary>
        public void StartBossEncounter(int bossIndex)
        {
            if (string.IsNullOrEmpty(_currentInstanceId))
            {
                Debug.LogError("[DungeonSceneController] No instance ID set");
                return;
            }

            _bossSystem?.StartEncounter(_currentInstanceId, bossIndex);
        }

        /// <summary>
        /// Get spawn point for entering the dungeon.
        /// </summary>
        public Vector3 GetEntranceSpawnPoint()
        {
            return _entranceSpawnPoint != null ? _entranceSpawnPoint.position : Vector3.zero;
        }

        /// <summary>
        /// Get checkpoint spawn point after a boss.
        /// </summary>
        public Vector3 GetCheckpointSpawnPoint(int checkpointIndex)
        {
            if (_checkpointSpawnPoints == null || checkpointIndex >= _checkpointSpawnPoints.Length)
                return GetEntranceSpawnPoint();

            var checkpoint = _checkpointSpawnPoints[checkpointIndex];
            return checkpoint != null ? checkpoint.position : GetEntranceSpawnPoint();
        }

        private void OnBossDefeated(string instanceId, int bossIndex)
        {
            if (instanceId != _currentInstanceId) return;

            Debug.Log($"[DungeonSceneController] Boss {bossIndex} defeated!");
            
            // Unlock checkpoint
            // Show loot UI
        }

        private void OnDungeonCompleted(string instanceId)
        {
            if (instanceId != _currentInstanceId) return;

            Debug.Log("[DungeonSceneController] Dungeon completed!");
            
            // Show completion UI
            // Award bonus rewards
        }

        private void OnWipe(string instanceId)
        {
            if (instanceId != _currentInstanceId) return;

            Debug.Log("[DungeonSceneController] WIPE! Resetting to checkpoint...");
            
            // Teleport players to last checkpoint
            // Reset current boss encounter
        }
    }

    /// <summary>
    /// Trigger for starting boss encounters.
    /// </summary>
    [System.Serializable]
    public class BossEncounterTrigger
    {
        public int BossIndex;
        public Transform TriggerPoint;
        public Transform BossSpawnPoint;
        public string BossName;
    }
}
