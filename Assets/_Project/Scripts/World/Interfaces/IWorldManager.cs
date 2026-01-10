using System;
using EtherDomes.Data;
using UnityEngine;

namespace EtherDomes.World
{
    /// <summary>
    /// Interface for the world management system.
    /// Handles region loading and player location tracking.
    /// </summary>
    public interface IWorldManager
    {
        /// <summary>
        /// Load a region.
        /// </summary>
        void LoadRegion(RegionId region);

        /// <summary>
        /// Unload a region.
        /// </summary>
        void UnloadRegion(RegionId region);

        /// <summary>
        /// Get the current region of a player.
        /// </summary>
        RegionId GetCurrentRegion(ulong playerId);

        /// <summary>
        /// Get data for a region.
        /// </summary>
        RegionData GetRegionData(RegionId region);

        /// <summary>
        /// Check if a player can enter a region (level requirement).
        /// </summary>
        bool CanEnterRegion(ulong playerId, RegionId region);

        /// <summary>
        /// Event fired when a region starts loading.
        /// </summary>
        event Action<RegionId> OnRegionLoading;

        /// <summary>
        /// Event fired when a region finishes loading.
        /// </summary>
        event Action<RegionId> OnRegionLoaded;

        /// <summary>
        /// Event fired when a player changes region.
        /// </summary>
        event Action<ulong, RegionId> OnPlayerChangedRegion;
    }

    /// <summary>
    /// Data structure for region information.
    /// </summary>
    [Serializable]
    public class RegionData
    {
        public RegionId Id;
        public string DisplayName;
        public int MinLevel;
        public int MaxLevel;
        public Vector3[] SpawnPoints;
        public string[] DungeonIds;
        public string SceneName;
    }
}
