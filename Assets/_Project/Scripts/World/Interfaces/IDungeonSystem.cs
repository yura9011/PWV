using System;
using EtherDomes.Data;

namespace EtherDomes.World
{
    /// <summary>
    /// Interface for the dungeon instance system.
    /// </summary>
    public interface IDungeonSystem
    {
        /// <summary>
        /// Create a new dungeon instance for a group.
        /// </summary>
        string CreateInstance(string dungeonId, ulong[] groupMembers);

        /// <summary>
        /// Enter a dungeon instance.
        /// </summary>
        void EnterInstance(string instanceId, ulong playerId);

        /// <summary>
        /// Leave a dungeon instance.
        /// </summary>
        void LeaveInstance(ulong playerId);

        /// <summary>
        /// Destroy a dungeon instance.
        /// </summary>
        void DestroyInstance(string instanceId);

        /// <summary>
        /// Get data for a dungeon instance.
        /// </summary>
        DungeonInstanceData GetInstanceData(string instanceId);

        /// <summary>
        /// Check if a boss has been defeated.
        /// </summary>
        bool IsBossDefeated(string instanceId, int bossIndex);

        /// <summary>
        /// Mark a boss as defeated.
        /// </summary>
        void MarkBossDefeated(string instanceId, int bossIndex);

        /// <summary>
        /// Check if dungeon is completed (all bosses defeated).
        /// </summary>
        bool IsCompleted(string instanceId);

        /// <summary>
        /// Event fired when a boss is defeated.
        /// </summary>
        event Action<string, int> OnBossDefeated;

        /// <summary>
        /// Event fired when a dungeon is completed.
        /// </summary>
        event Action<string> OnDungeonCompleted;

        /// <summary>
        /// Event fired when all players die (wipe).
        /// </summary>
        event Action<string> OnWipe;

        /// <summary>
        /// Delay before destroying empty instance (minutes).
        /// </summary>
        float InstanceDestroyDelayMinutes { get; }

        /// <summary>
        /// Get the difficulty multiplier for an instance.
        /// </summary>
        float GetDifficultyMultiplier(string instanceId);
    }

    /// <summary>
    /// Data for a dungeon instance.
    /// </summary>
    [Serializable]
    public class DungeonInstanceData
    {
        public string InstanceId;
        public string DungeonId;
        public DungeonSize Size;
        public int BossCount;
        public bool[] BossesDefeated;
        public ulong[] GroupMembers;
        public int GroupSize;
        public float DifficultyMultiplier;
        public float CreationTime;
    }
}
