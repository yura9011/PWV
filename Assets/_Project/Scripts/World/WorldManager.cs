using System;
using System.Collections.Generic;
using EtherDomes.Data;
using UnityEngine;

namespace EtherDomes.World
{
    /// <summary>
    /// Manages world regions, loading, and player locations.
    /// </summary>
    public class WorldManager : MonoBehaviour, IWorldManager
    {
        // Region definitions
        private readonly Dictionary<RegionId, RegionData> _regionData = new();
        
        // Player tracking
        private readonly Dictionary<ulong, RegionId> _playerRegions = new();
        private readonly Dictionary<ulong, int> _playerLevels = new();
        
        // Loaded regions
        private readonly HashSet<RegionId> _loadedRegions = new();

        public event Action<RegionId> OnRegionLoading;
        public event Action<RegionId> OnRegionLoaded;
        public event Action<ulong, RegionId> OnPlayerChangedRegion;

        private void Awake()
        {
            InitializeRegionData();
        }

        private void InitializeRegionData()
        {
            // Define the 5 regions with their level ranges
            _regionData[RegionId.Roca] = new RegionData
            {
                Id = RegionId.Roca,
                DisplayName = "Roca",
                MinLevel = 1,
                MaxLevel = 15,
                SpawnPoints = new[] { Vector3.zero },
                DungeonIds = new[] { "dungeon_roca_small" },
                SceneName = "Region_Roca"
            };

            _regionData[RegionId.Bosque] = new RegionData
            {
                Id = RegionId.Bosque,
                DisplayName = "Bosque",
                MinLevel = 15,
                MaxLevel = 30,
                SpawnPoints = new[] { new Vector3(100, 0, 0) },
                DungeonIds = new[] { "dungeon_bosque_small", "dungeon_bosque_large" },
                SceneName = "Region_Bosque"
            };

            _regionData[RegionId.Nieve] = new RegionData
            {
                Id = RegionId.Nieve,
                DisplayName = "Nieve",
                MinLevel = 30,
                MaxLevel = 40,
                SpawnPoints = new[] { new Vector3(200, 50, 0) },
                DungeonIds = new[] { "dungeon_nieve_large" },
                SceneName = "Region_Nieve"
            };

            _regionData[RegionId.Pantano] = new RegionData
            {
                Id = RegionId.Pantano,
                DisplayName = "Pantano",
                MinLevel = 40,
                MaxLevel = 50,
                SpawnPoints = new[] { new Vector3(300, 0, 100) },
                DungeonIds = new[] { "dungeon_pantano_small", "dungeon_pantano_large" },
                SceneName = "Region_Pantano"
            };

            _regionData[RegionId.Ciudadela] = new RegionData
            {
                Id = RegionId.Ciudadela,
                DisplayName = "Ciudadela",
                MinLevel = 50,
                MaxLevel = 60,
                SpawnPoints = new[] { new Vector3(400, 100, 0) },
                DungeonIds = new[] { "dungeon_ciudadela_large" },
                SceneName = "Region_Ciudadela"
            };
        }


        /// <summary>
        /// Register a player with their level.
        /// </summary>
        public void RegisterPlayer(ulong playerId, int level, RegionId startRegion = RegionId.Roca)
        {
            _playerLevels[playerId] = level;
            _playerRegions[playerId] = startRegion;
        }

        /// <summary>
        /// Unregister a player.
        /// </summary>
        public void UnregisterPlayer(ulong playerId)
        {
            _playerLevels.Remove(playerId);
            _playerRegions.Remove(playerId);
        }

        /// <summary>
        /// Update player level.
        /// </summary>
        public void UpdatePlayerLevel(ulong playerId, int level)
        {
            _playerLevels[playerId] = level;
        }

        public void LoadRegion(RegionId region)
        {
            if (_loadedRegions.Contains(region))
            {
                Debug.Log($"[WorldManager] Region {region} already loaded");
                return;
            }

            Debug.Log($"[WorldManager] Loading region: {region}");
            OnRegionLoading?.Invoke(region);

            // In production, this would use SceneManager.LoadSceneAsync
            // For now, just mark as loaded
            _loadedRegions.Add(region);

            Debug.Log($"[WorldManager] Region {region} loaded");
            OnRegionLoaded?.Invoke(region);
        }

        public void UnloadRegion(RegionId region)
        {
            if (!_loadedRegions.Contains(region))
            {
                Debug.Log($"[WorldManager] Region {region} not loaded");
                return;
            }

            // Check if any players are in this region
            foreach (var kvp in _playerRegions)
            {
                if (kvp.Value == region)
                {
                    Debug.LogWarning($"[WorldManager] Cannot unload region {region} - players present");
                    return;
                }
            }

            Debug.Log($"[WorldManager] Unloading region: {region}");
            _loadedRegions.Remove(region);
        }

        public RegionId GetCurrentRegion(ulong playerId)
        {
            return _playerRegions.TryGetValue(playerId, out RegionId region) 
                ? region 
                : RegionId.Roca;
        }

        public RegionData GetRegionData(RegionId region)
        {
            return _regionData.TryGetValue(region, out RegionData data) ? data : null;
        }

        public bool CanEnterRegion(ulong playerId, RegionId region)
        {
            if (!_playerLevels.TryGetValue(playerId, out int playerLevel))
                return false;

            if (!_regionData.TryGetValue(region, out RegionData data))
                return false;

            // Player must be at least the minimum level for the region
            // Allow some flexibility (can enter 5 levels early)
            return playerLevel >= (data.MinLevel - 5);
        }

        /// <summary>
        /// Move a player to a new region.
        /// </summary>
        public bool TryEnterRegion(ulong playerId, RegionId region)
        {
            if (!CanEnterRegion(playerId, region))
            {
                Debug.LogWarning($"[WorldManager] Player {playerId} cannot enter region {region}");
                return false;
            }

            // Ensure region is loaded
            if (!_loadedRegions.Contains(region))
            {
                LoadRegion(region);
            }

            var oldRegion = GetCurrentRegion(playerId);
            _playerRegions[playerId] = region;

            Debug.Log($"[WorldManager] Player {playerId} moved from {oldRegion} to {region}");
            OnPlayerChangedRegion?.Invoke(playerId, region);

            return true;
        }

        /// <summary>
        /// Get spawn point for a region.
        /// </summary>
        public Vector3 GetSpawnPoint(RegionId region)
        {
            if (_regionData.TryGetValue(region, out RegionData data) && 
                data.SpawnPoints != null && data.SpawnPoints.Length > 0)
            {
                return data.SpawnPoints[UnityEngine.Random.Range(0, data.SpawnPoints.Length)];
            }
            return Vector3.zero;
        }

        /// <summary>
        /// Get all dungeons in a region.
        /// </summary>
        public string[] GetRegionDungeons(RegionId region)
        {
            if (_regionData.TryGetValue(region, out RegionData data))
            {
                return data.DungeonIds ?? Array.Empty<string>();
            }
            return Array.Empty<string>();
        }

        /// <summary>
        /// Check if a region is loaded.
        /// </summary>
        public bool IsRegionLoaded(RegionId region)
        {
            return _loadedRegions.Contains(region);
        }

        /// <summary>
        /// Get recommended region for a player level.
        /// </summary>
        public RegionId GetRecommendedRegion(int playerLevel)
        {
            foreach (var kvp in _regionData)
            {
                if (playerLevel >= kvp.Value.MinLevel && playerLevel <= kvp.Value.MaxLevel)
                {
                    return kvp.Key;
                }
            }
            return playerLevel < 15 ? RegionId.Roca : RegionId.Ciudadela;
        }
    }
}
