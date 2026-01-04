using System;
using UnityEngine;

namespace EtherDomes.World
{
    /// <summary>
    /// Interface for the guild base system.
    /// </summary>
    public interface IGuildBaseSystem
    {
        /// <summary>
        /// Enter the guild base.
        /// </summary>
        void Enter(ulong playerId);

        /// <summary>
        /// Leave the guild base.
        /// </summary>
        void Leave(ulong playerId);

        /// <summary>
        /// Place furniture in the guild base.
        /// </summary>
        void PlaceFurniture(FurnitureData furniture, Vector3 position, Quaternion rotation);

        /// <summary>
        /// Remove furniture from the guild base.
        /// </summary>
        void RemoveFurniture(string furnitureInstanceId);

        /// <summary>
        /// Unlock a trophy for defeating a boss.
        /// </summary>
        void UnlockTrophy(string bossId);

        /// <summary>
        /// Get the current state of the guild base.
        /// </summary>
        GuildBaseState GetState();

        /// <summary>
        /// Check if a trophy is unlocked.
        /// </summary>
        bool IsTrophyUnlocked(string bossId);

        /// <summary>
        /// Event fired when a trophy is unlocked.
        /// </summary>
        event Action<string> OnTrophyUnlocked;
    }

    /// <summary>
    /// Data for furniture placement.
    /// </summary>
    [Serializable]
    public class FurnitureData
    {
        public string FurnitureId;
        public string FurnitureName;
        public string PrefabPath;
    }

    /// <summary>
    /// Instance of placed furniture.
    /// </summary>
    [Serializable]
    public class FurnitureInstance
    {
        public string InstanceId;
        public FurnitureData Data;
        public Vector3 Position;
        public Quaternion Rotation;
    }

    /// <summary>
    /// State of the guild base.
    /// </summary>
    [Serializable]
    public class GuildBaseState
    {
        public FurnitureInstance[] PlacedFurniture;
        public string[] UnlockedTrophies;
    }
}
