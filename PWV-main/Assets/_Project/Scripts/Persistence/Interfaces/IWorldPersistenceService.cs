using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EtherDomes.World;

namespace EtherDomes.Persistence
{
    /// <summary>
    /// Interface for world state persistence.
    /// </summary>
    public interface IWorldPersistenceService
    {
        /// <summary>
        /// Save the current world state.
        /// </summary>
        Task SaveWorldStateAsync(WorldState state);

        /// <summary>
        /// Load the world state.
        /// </summary>
        Task<WorldState> LoadWorldStateAsync();

        /// <summary>
        /// Enable auto-save for dedicated servers.
        /// </summary>
        void EnableAutoSave(TimeSpan interval);

        /// <summary>
        /// Disable auto-save.
        /// </summary>
        void DisableAutoSave();
    }

    /// <summary>
    /// Complete world state for persistence.
    /// </summary>
    [Serializable]
    public class WorldState
    {
        public Dictionary<string, bool[]> DungeonProgress;
        public GuildBaseState GuildBase;
        public DateTime LastSaveTime;
        public string WorldId;
    }
}
