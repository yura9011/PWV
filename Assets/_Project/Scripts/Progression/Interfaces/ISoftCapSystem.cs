using System;

namespace EtherDomes.Progression
{
    /// <summary>
    /// Information about soft cap thresholds and penalties.
    /// </summary>
    public struct SoftCapInfo
    {
        public float FirstThreshold;      // 30%
        public float FirstPenalty;        // 50% DR
        public float SecondThreshold;     // 50%
        public float SecondPenalty;       // 75% DR
        public float HardCap;             // 75% maximum
    }

    /// <summary>
    /// Interface for the soft cap system that applies diminishing returns
    /// to secondary stats (CritChance, Haste, Mastery).
    /// Requirements: 8.2, 8.3
    /// </summary>
    public interface ISoftCapSystem
    {
        /// <summary>
        /// Applies diminishing returns to a raw stat value.
        /// </summary>
        /// <param name="rawValue">The raw percentage value (0-100)</param>
        /// <returns>The effective value after diminishing returns</returns>
        float ApplyDiminishingReturns(float rawValue);

        /// <summary>
        /// Gets the effective value for a stat after soft caps.
        /// Alias for ApplyDiminishingReturns for clarity.
        /// </summary>
        float GetEffectiveValue(float rawValue);

        /// <summary>
        /// Gets information about the soft cap thresholds and penalties.
        /// </summary>
        SoftCapInfo GetSoftCapInfo();
    }
}
