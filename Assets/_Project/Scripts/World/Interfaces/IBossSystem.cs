using System;
using EtherDomes.Data;

namespace EtherDomes.World
{
    /// <summary>
    /// Interface for the boss encounter system.
    /// </summary>
    public interface IBossSystem
    {
        /// <summary>
        /// Start a boss encounter.
        /// </summary>
        void StartEncounter(string instanceId, int bossIndex);

        /// <summary>
        /// End a boss encounter.
        /// </summary>
        void EndEncounter(string instanceId, int bossIndex, bool victory);

        /// <summary>
        /// Get the current phase of a boss.
        /// </summary>
        BossPhase GetCurrentPhase(string instanceId, int bossIndex);

        /// <summary>
        /// Get the health percentage of a boss.
        /// </summary>
        float GetHealthPercent(string instanceId, int bossIndex);

        /// <summary>
        /// Event fired when an encounter starts.
        /// </summary>
        event Action<string, int> OnEncounterStarted;

        /// <summary>
        /// Event fired when a boss changes phase.
        /// </summary>
        event Action<string, int, BossPhase> OnPhaseTransition;

        /// <summary>
        /// Event fired when an encounter ends.
        /// </summary>
        event Action<string, int, bool> OnEncounterEnded;
    }
}
