using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EtherDomes.World;
using UnityEngine;

namespace EtherDomes.Persistence
{
    /// <summary>
    /// Handles world state persistence including dungeon progress and guild base.
    /// </summary>
    public class WorldPersistenceService : MonoBehaviour, IWorldPersistenceService
    {
        private const string WORLD_SAVE_FILENAME = "world_state.json";
        private const float DEFAULT_AUTOSAVE_INTERVAL = 300f; // 5 minutes

        [SerializeField] private string _worldId = "default_world";

        private IEncryptionService _encryptionService;
        private string _savePath;
        private bool _autoSaveEnabled;
        private float _autoSaveInterval;
        private float _lastAutoSaveTime;

        // References to systems for state gathering
        private IDungeonSystem _dungeonSystem;
        private IGuildBaseSystem _guildBaseSystem;

        public event Action OnWorldSaved;
        public event Action OnWorldLoaded;

        private void Awake()
        {
            _savePath = Path.Combine(Application.persistentDataPath, "Worlds", _worldId);
            Directory.CreateDirectory(_savePath);
        }

        public void Initialize(IEncryptionService encryptionService, IDungeonSystem dungeonSystem, IGuildBaseSystem guildBaseSystem)
        {
            _encryptionService = encryptionService;
            _dungeonSystem = dungeonSystem;
            _guildBaseSystem = guildBaseSystem;
        }

        private void Update()
        {
            if (_autoSaveEnabled && Time.time - _lastAutoSaveTime >= _autoSaveInterval)
            {
                _lastAutoSaveTime = Time.time;
                _ = AutoSaveAsync();
            }
        }

        private async Task AutoSaveAsync()
        {
            var state = GatherWorldState();
            await SaveWorldStateAsync(state);
            Debug.Log("[WorldPersistenceService] Auto-save completed");
        }

        public async Task SaveWorldStateAsync(WorldState state)
        {
            if (state == null)
            {
                Debug.LogError("[WorldPersistenceService] Cannot save null state");
                return;
            }

            try
            {
                state.LastSaveTime = DateTime.UtcNow;
                state.WorldId = _worldId;

                string json = JsonUtility.ToJson(new WorldStateWrapper(state), true);
                
                // Encrypt if service available
                byte[] data = System.Text.Encoding.UTF8.GetBytes(json);
                if (_encryptionService != null)
                {
                    data = _encryptionService.Encrypt(data);
                }

                string filePath = Path.Combine(_savePath, WORLD_SAVE_FILENAME);
                await Task.Run(() => File.WriteAllBytes(filePath, data));

                Debug.Log($"[WorldPersistenceService] World saved to {filePath}");
                OnWorldSaved?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WorldPersistenceService] Save failed: {ex.Message}");
            }
        }

        public async Task<WorldState> LoadWorldStateAsync()
        {
            string filePath = Path.Combine(_savePath, WORLD_SAVE_FILENAME);

            if (!File.Exists(filePath))
            {
                Debug.Log("[WorldPersistenceService] No save file found, creating new world state");
                return CreateNewWorldState();
            }

            try
            {
                byte[] data = await Task.Run(() => File.ReadAllBytes(filePath));

                // Decrypt if service available
                if (_encryptionService != null)
                {
                    data = _encryptionService.Decrypt(data);
                }

                string json = System.Text.Encoding.UTF8.GetString(data);
                var wrapper = JsonUtility.FromJson<WorldStateWrapper>(json);
                var state = wrapper.ToWorldState();

                Debug.Log($"[WorldPersistenceService] World loaded from {filePath}");
                OnWorldLoaded?.Invoke();

                return state;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WorldPersistenceService] Load failed: {ex.Message}");
                return CreateNewWorldState();
            }
        }

        public void EnableAutoSave(TimeSpan interval)
        {
            _autoSaveEnabled = true;
            _autoSaveInterval = (float)interval.TotalSeconds;
            _lastAutoSaveTime = Time.time;
            Debug.Log($"[WorldPersistenceService] Auto-save enabled: {interval.TotalMinutes:F1} minutes");
        }

        public void DisableAutoSave()
        {
            _autoSaveEnabled = false;
            Debug.Log("[WorldPersistenceService] Auto-save disabled");
        }

        private WorldState CreateNewWorldState()
        {
            return new WorldState
            {
                DungeonProgress = new Dictionary<string, bool[]>(),
                GuildBase = new GuildBaseState
                {
                    PlacedFurniture = Array.Empty<FurnitureInstance>(),
                    UnlockedTrophies = Array.Empty<string>()
                },
                LastSaveTime = DateTime.UtcNow,
                WorldId = _worldId
            };
        }

        private WorldState GatherWorldState()
        {
            var state = new WorldState
            {
                DungeonProgress = new Dictionary<string, bool[]>(),
                GuildBase = _guildBaseSystem?.GetState() ?? new GuildBaseState(),
                LastSaveTime = DateTime.UtcNow,
                WorldId = _worldId
            };

            return state;
        }

        /// <summary>
        /// Set the world ID (for multi-world support).
        /// </summary>
        public void SetWorldId(string worldId)
        {
            _worldId = worldId;
            _savePath = Path.Combine(Application.persistentDataPath, "Worlds", _worldId);
            Directory.CreateDirectory(_savePath);
        }
    }

    /// <summary>
    /// Wrapper for JSON serialization (Unity's JsonUtility doesn't support Dictionary).
    /// </summary>
    [Serializable]
    internal class WorldStateWrapper
    {
        public string WorldId;
        public string LastSaveTime;
        public GuildBaseState GuildBase;
        public string[] DungeonIds;
        public string[] DungeonProgressJson;

        public WorldStateWrapper() { }

        public WorldStateWrapper(WorldState state)
        {
            WorldId = state.WorldId;
            LastSaveTime = state.LastSaveTime.ToString("O");
            GuildBase = state.GuildBase;

            if (state.DungeonProgress != null)
            {
                DungeonIds = new string[state.DungeonProgress.Count];
                DungeonProgressJson = new string[state.DungeonProgress.Count];
                int i = 0;
                foreach (var kvp in state.DungeonProgress)
                {
                    DungeonIds[i] = kvp.Key;
                    DungeonProgressJson[i] = string.Join(",", kvp.Value);
                    i++;
                }
            }
        }

        public WorldState ToWorldState()
        {
            var state = new WorldState
            {
                WorldId = WorldId,
                LastSaveTime = DateTime.TryParse(LastSaveTime, out var dt) ? dt : DateTime.UtcNow,
                GuildBase = GuildBase,
                DungeonProgress = new Dictionary<string, bool[]>()
            };

            if (DungeonIds != null && DungeonProgressJson != null)
            {
                for (int i = 0; i < DungeonIds.Length && i < DungeonProgressJson.Length; i++)
                {
                    var parts = DungeonProgressJson[i].Split(',');
                    var progress = new bool[parts.Length];
                    for (int j = 0; j < parts.Length; j++)
                    {
                        progress[j] = parts[j].Trim().ToLower() == "true";
                    }
                    state.DungeonProgress[DungeonIds[i]] = progress;
                }
            }

            return state;
        }
    }
}
