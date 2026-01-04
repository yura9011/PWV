using System;
using EtherDomes.Classes;
using EtherDomes.Combat;
using EtherDomes.Data;
using EtherDomes.Network;
using EtherDomes.Persistence;
using EtherDomes.Player;
using EtherDomes.Progression;
using EtherDomes.World;
using Unity.Netcode;
using UnityEngine;

namespace EtherDomes.Core
{
    /// <summary>
    /// Central game manager that initializes and coordinates all game systems.
    /// Singleton pattern for easy access across the game.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Network")]
        [SerializeField] private NetworkSessionManager _networkSession;
        [SerializeField] private ConnectionApprovalHandler _connectionApproval;

        [Header("Combat")]
        [SerializeField] private TargetSystem _targetSystem;
        [SerializeField] private AbilitySystem _abilitySystem;
        [SerializeField] private AggroSystem _aggroSystem;
        [SerializeField] private CombatSystem _combatSystem;

        [Header("Classes")]
        [SerializeField] private ClassSystem _classSystem;

        [Header("Progression")]
        [SerializeField] private ProgressionSystem _progressionSystem;
        [SerializeField] private LootSystem _lootSystem;
        [SerializeField] private EquipmentSystem _equipmentSystem;

        [Header("World")]
        [SerializeField] private WorldManager _worldManager;
        [SerializeField] private DungeonSystem _dungeonSystem;
        [SerializeField] private BossSystem _bossSystem;
        [SerializeField] private GuildBaseSystem _guildBaseSystem;

        // Services
        private CharacterPersistenceService _characterPersistence;
        private WorldPersistenceService _worldPersistence;

        // Public accessors
        public NetworkSessionManager Network => _networkSession;
        public TargetSystem Targeting => _targetSystem;
        public AbilitySystem Abilities => _abilitySystem;
        public AggroSystem Aggro => _aggroSystem;
        public CombatSystem Combat => _combatSystem;
        public ClassSystem Classes => _classSystem;
        public ProgressionSystem Progression => _progressionSystem;
        public LootSystem Loot => _lootSystem;
        public EquipmentSystem Equipment => _equipmentSystem;
        public WorldManager World => _worldManager;
        public DungeonSystem Dungeons => _dungeonSystem;
        public BossSystem Bosses => _bossSystem;
        public GuildBaseSystem GuildBase => _guildBaseSystem;
        public CharacterPersistenceService CharacterPersistence => _characterPersistence;
        public WorldPersistenceService WorldPersistence => _worldPersistence;

        public bool IsInitialized { get; private set; }

        public event Action OnGameInitialized;
        public event Action<ulong> OnPlayerJoined;
        public event Action<ulong> OnPlayerLeft;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeSystems();
        }

        private void InitializeSystems()
        {
            Debug.Log("[GameManager] Initializing game systems...");

            // Create services (these are plain C# classes, not MonoBehaviours)
            _characterPersistence = new CharacterPersistenceService();
            
            // WorldPersistenceService is a MonoBehaviour - add as component or find existing
            _worldPersistence = GetComponent<WorldPersistenceService>();
            if (_worldPersistence == null)
            {
                _worldPersistence = gameObject.AddComponent<WorldPersistenceService>();
            }

            // Initialize combat systems
            _combatSystem?.Initialize(_aggroSystem);
            _classSystem?.Initialize(_combatSystem);
            _abilitySystem?.Initialize(_targetSystem);
            _bossSystem?.Initialize(_dungeonSystem, _lootSystem);

            // Setup network events
            if (_networkSession != null)
            {
                _networkSession.SetApprovalHandler(_connectionApproval);
                _networkSession.OnPlayerConnected += HandlePlayerConnected;
                _networkSession.OnPlayerDisconnected += HandlePlayerDisconnected;
            }

            // Setup boss/dungeon integration
            if (_bossSystem != null && _dungeonSystem != null)
            {
                _bossSystem.OnEncounterEnded += (instanceId, bossIndex, victory) =>
                {
                    if (victory)
                    {
                        _dungeonSystem.MarkBossDefeated(instanceId, bossIndex);
                    }
                };
            }

            IsInitialized = true;
            Debug.Log("[GameManager] All systems initialized");
            OnGameInitialized?.Invoke();
        }

        private void OnDestroy()
        {
            if (_networkSession != null)
            {
                _networkSession.OnPlayerConnected -= HandlePlayerConnected;
                _networkSession.OnPlayerDisconnected -= HandlePlayerDisconnected;
            }
        }

        private void HandlePlayerConnected(ulong clientId)
        {
            Debug.Log($"[GameManager] Player connected: {clientId}");
            OnPlayerJoined?.Invoke(clientId);
        }

        private void HandlePlayerDisconnected(ulong clientId)
        {
            Debug.Log($"[GameManager] Player disconnected: {clientId}");
            
            // Cleanup player from all systems
            _combatSystem?.UnregisterPlayer(clientId);
            _classSystem?.UnregisterPlayer(clientId);
            _worldManager?.UnregisterPlayer(clientId);
            _dungeonSystem?.LeaveInstance(clientId);

            OnPlayerLeft?.Invoke(clientId);
        }

        /// <summary>
        /// Register a new player with all systems.
        /// </summary>
        public void RegisterPlayer(ulong clientId, CharacterData data)
        {
            if (data == null) return;

            // Register with combat
            _combatSystem?.RegisterPlayer(clientId, data.BaseStats?.MaxHealth ?? 100f);

            // Register with class system
            _classSystem?.RegisterPlayer(clientId, data.Class, data.CurrentSpec);

            // Register with world
            _worldManager?.RegisterPlayer(clientId, data.Level);

            Debug.Log($"[GameManager] Registered player {clientId}: {data.CharacterName} (Lv.{data.Level} {data.Class})");
        }

        /// <summary>
        /// Start a dungeon run for a group.
        /// </summary>
        public string StartDungeonRun(string dungeonId, ulong[] groupMembers)
        {
            string instanceId = _dungeonSystem?.CreateInstance(dungeonId, groupMembers);
            
            if (!string.IsNullOrEmpty(instanceId))
            {
                foreach (var playerId in groupMembers)
                {
                    _dungeonSystem?.EnterInstance(instanceId, playerId);
                }
                _lootSystem?.ResetRoundRobin();
            }

            return instanceId;
        }

        /// <summary>
        /// Handle boss defeat - generate and distribute loot.
        /// </summary>
        public void HandleBossDefeat(string instanceId, int bossIndex, string bossId)
        {
            var instance = _dungeonSystem?.GetInstanceData(instanceId);
            if (instance == null) return;

            // Mark boss defeated
            _dungeonSystem?.MarkBossDefeated(instanceId, bossIndex);

            // Generate loot
            var loot = _lootSystem?.GenerateLoot(bossId, instance.GroupSize);
            if (loot != null && loot.Length > 0)
            {
                _lootSystem?.DistributeLoot(loot, instance.GroupMembers, LootDistributionMode.RoundRobin);
            }
        }

        /// <summary>
        /// Save all game state (for dedicated server).
        /// </summary>
        public async void SaveAllState()
        {
            await _worldPersistence?.SaveWorldStateAsync(new WorldState());
            Debug.Log("[GameManager] World state saved");
        }
    }
}
