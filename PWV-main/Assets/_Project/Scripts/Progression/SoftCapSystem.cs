using System;
using EtherDomes.Data;

namespace EtherDomes.Progression
{
    /// <summary>
    /// Implements diminishing returns for secondary stats.
    /// 
    /// Soft Cap Formula:
    /// - 0-30%: No penalty (1:1 ratio)
    /// - 30-50%: 50% diminishing returns (2:1 ratio)
    /// - 50%+: 75% diminishing returns (4:1 ratio)
    /// - Hard cap: 75% maximum
    /// 
    /// Requirements: 8.2, 8.3
    /// </summary>
    public class SoftCapSystem : ISoftCapSystem
    {
        private static SoftCapSystem _instance;
        
        public SoftCapSystem()
        {
            // Register this system's calculator with CharacterStats
            _instance = this;
            CharacterStats.SoftCapCalculator = GetEffectiveValue;
        }
        
        /// <summary>
        /// Ensures the soft cap system is initialized and registered.
        /// Call this at game startup.
        /// </summary>
        public static void Initialize()
        {
            if (_instance == null)
            {
                _instance = new SoftCapSystem();
            }
        }
        public const float FirstThreshold = 30f;
        public const float FirstPenalty = 0.5f;    // 50% DR
        public const float SecondThreshold = 50f;
        public const float SecondPenalty = 0.75f;  // 75% DR
        public const float HardCap = 75f;

        // Pre-calculated effective values at thresholds
        private const float EffectiveAtFirstThreshold = 30f;
        // From 30 to 50 raw = 20 points * 0.5 = 10 effective points
        private const float EffectiveAtSecondThreshold = 40f; // 30 + 10

        public float ApplyDiminishingReturns(float rawValue)
        {
            if (rawValue <= 0f)
                return 0f;

            if (rawValue <= FirstThreshold)
            {
                // No penalty below first threshold
                return rawValue;
            }

            if (rawValue <= SecondThreshold)
            {
                // 50% DR between first and second threshold
                float baseValue = FirstThreshold;
                float excessValue = rawValue - FirstThreshold;
                float effectiveExcess = excessValue * (1f - FirstPenalty);
                return baseValue + effectiveExcess;
            }

            // 75% DR above second threshold
            float effectiveAtSecond = EffectiveAtSecondThreshold;
            float excessAboveSecond = rawValue - SecondThreshold;
            float effectiveExcessAboveSecond = excessAboveSecond * (1f - SecondPenalty);
            float result = effectiveAtSecond + effectiveExcessAboveSecond;

            // Apply hard cap
            return Math.Min(result, HardCap);
        }

        public float GetEffectiveValue(float rawValue)
        {
            return ApplyDiminishingReturns(rawValue);
        }

        public SoftCapInfo GetSoftCapInfo()
        {
            return new SoftCapInfo
            {
                FirstThreshold = FirstThreshold,
                FirstPenalty = FirstPenalty,
                SecondThreshold = SecondThreshold,
                SecondPenalty = SecondPenalty,
                HardCap = HardCap
            };
        }
    }
}
