using System;

namespace EtherDomes.World
{
    /// <summary>
    /// Interface for tracking wipes in dungeon instances.
    /// Requirements: 5.4, 5.5
    /// </summary>
    public interface IWipeTracker
    {
        /// <summary>
        /// Maximum wipes before group expulsion.
        /// </summary>
        int MaxWipesBeforeExpulsion { get; }

        /// <summary>
        /// Gets the current wipe count for an instance.
        /// </summary>
        /// <param name="instanceId">The dungeon instance ID</param>
        /// <returns>Number of wipes</returns>
        int GetWipeCount(string instanceId);

        /// <summary>
        /// Records a wipe for an instance.
        /// </summary>
        /// <param name="instanceId">The dungeon instance ID</param>
        void RecordWipe(string instanceId);

        /// <summary>
        /// Checks if the group should be expelled from the instance.
        /// </summary>
        /// <param name="instanceId">The dungeon instance ID</param>
        /// <returns>True if wipe count >= MaxWipesBeforeExpulsion</returns>
        bool ShouldExpelGroup(string instanceId);

        /// <summary>
        /// Resets the wipe count for an instance.
        /// </summary>
        /// <param name="instanceId">The dungeon instance ID</param>
        void ResetWipeCount(string instanceId);

        /// <summary>
        /// Event fired when a wipe is recorded.
        /// </summary>
        event Action<string, int> OnWipeRecorded;

        /// <summary>
        /// Event fired when group should be expelled.
        /// </summary>
        event Action<string> OnGroupExpelled;
    }
}
