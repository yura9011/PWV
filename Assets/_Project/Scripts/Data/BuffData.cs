using System;
using UnityEngine;

namespace EtherDomes.Data
{
    /// <summary>
    /// Data definition for a buff or debuff effect.
    /// Contains all configuration for the effect including duration, tick behavior, and stat modifications.
    /// </summary>
    [Serializable]
    public class BuffData
    {
        /// <summary>
        /// Unique identifier for this buff/debuff.
        /// </summary>
        public string BuffId;

        /// <summary>
        /// Display name shown in UI.
        /// </summary>
        public string DisplayName;

        /// <summary>
        /// Icon to display in buff/debuff bar.
        /// </summary>
        public Sprite Icon;

        /// <summary>
        /// Duration of the effect in seconds.
        /// Will be clamped between MinDuration (1) and MaxDuration (300).
        /// </summary>
        public float Duration;

        /// <summary>
        /// Type of effect (Buff, Debuff, DoT, HoT, etc.).
        /// </summary>
        public EffectType EffectType;

        /// <summary>
        /// Whether this is a periodic effect (DoT/HoT).
        /// </summary>
        public bool IsPeriodicEffect;

        /// <summary>
        /// Interval between ticks for periodic effects in seconds.
        /// </summary>
        public float TickInterval;

        /// <summary>
        /// Damage per tick for DoT effects.
        /// </summary>
        public float TickDamage;

        /// <summary>
        /// Healing per tick for HoT effects.
        /// </summary>
        public float TickHealing;

        /// <summary>
        /// Type of damage for DoT effects.
        /// </summary>
        public DamageType DamageType;

        /// <summary>
        /// Stat modifications applied by this buff/debuff.
        /// </summary>
        public CharacterStats StatModifiers;

        /// <summary>
        /// Damage multiplier (1.0 = no change, 1.2 = 20% more damage).
        /// </summary>
        public float DamageMultiplier = 1f;

        /// <summary>
        /// Healing multiplier (1.0 = no change, 1.2 = 20% more healing).
        /// </summary>
        public float HealingMultiplier = 1f;

        /// <summary>
        /// Speed multiplier (1.0 = no change, 0.7 = 30% slower).
        /// </summary>
        public float SpeedMultiplier = 1f;

        /// <summary>
        /// Whether this buff can be dispelled.
        /// </summary>
        public bool IsDispellable = true;

        /// <summary>
        /// Whether this buff stacks with itself.
        /// </summary>
        public bool IsStackable = false;

        /// <summary>
        /// Maximum stacks if stackable.
        /// </summary>
        public int MaxStacks = 1;

        /// <summary>
        /// Type of crowd control effect (Slow, Stun, Fear, Root).
        /// Requirements: 11.1, 11.2, 11.3
        /// </summary>
        public CCType CCType = CCType.None;

        /// <summary>
        /// Slow percentage for Slow effects (0.0 to 1.0, where 0.5 = 50% slow).
        /// Requirements: 11.1
        /// </summary>
        public float SlowPercent = 0f;

        /// <summary>
        /// Calculate the expected number of ticks for this effect.
        /// </summary>
        public int ExpectedTicks => IsPeriodicEffect && TickInterval > 0 
            ? Mathf.CeilToInt(Duration / TickInterval) 
            : 0;

        /// <summary>
        /// Create a default buff data instance.
        /// </summary>
        public BuffData()
        {
            BuffId = string.Empty;
            DisplayName = string.Empty;
            Duration = 10f;
            EffectType = EffectType.Buff;
            TickInterval = 1f;
            DamageMultiplier = 1f;
            HealingMultiplier = 1f;
            SpeedMultiplier = 1f;
        }

        /// <summary>
        /// Create a copy of this buff data.
        /// </summary>
        public BuffData Clone()
        {
            return new BuffData
            {
                BuffId = BuffId,
                DisplayName = DisplayName,
                Icon = Icon,
                Duration = Duration,
                EffectType = EffectType,
                IsPeriodicEffect = IsPeriodicEffect,
                TickInterval = TickInterval,
                TickDamage = TickDamage,
                TickHealing = TickHealing,
                DamageType = DamageType,
                StatModifiers = StatModifiers,
                DamageMultiplier = DamageMultiplier,
                HealingMultiplier = HealingMultiplier,
                SpeedMultiplier = SpeedMultiplier,
                IsDispellable = IsDispellable,
                IsStackable = IsStackable,
                MaxStacks = MaxStacks,
                CCType = CCType,
                SlowPercent = SlowPercent
            };
        }
    }


    /// <summary>
    /// Runtime instance of an active buff/debuff on an entity.
    /// Tracks remaining duration, tick timing, and source information.
    /// </summary>
    [Serializable]
    public class BuffInstance
    {
        /// <summary>
        /// The buff data definition.
        /// </summary>
        public BuffData Data;

        /// <summary>
        /// Remaining duration in seconds.
        /// </summary>
        public float RemainingDuration;

        /// <summary>
        /// Time until next tick for periodic effects.
        /// </summary>
        public float NextTickTime;

        /// <summary>
        /// Entity that applied this buff/debuff.
        /// </summary>
        public ulong SourceId;

        /// <summary>
        /// Time when this buff was applied (for sorting oldest).
        /// </summary>
        public float AppliedTime;

        /// <summary>
        /// Current stack count if stackable.
        /// </summary>
        public int CurrentStacks = 1;

        /// <summary>
        /// Number of ticks that have occurred.
        /// </summary>
        public int TicksOccurred;

        /// <summary>
        /// Whether this is a buff (true) or debuff (false).
        /// </summary>
        public bool IsBuff;

        /// <summary>
        /// Create a new buff instance from buff data.
        /// </summary>
        /// <param name="data">The buff data definition</param>
        /// <param name="sourceId">Entity applying the buff</param>
        /// <param name="isBuff">Whether this is a buff or debuff</param>
        /// <param name="currentTime">Current game time for tracking</param>
        public BuffInstance(BuffData data, ulong sourceId, bool isBuff, float currentTime)
        {
            Data = data;
            SourceId = sourceId;
            IsBuff = isBuff;
            AppliedTime = currentTime;
            RemainingDuration = data.Duration;
            NextTickTime = data.IsPeriodicEffect ? data.TickInterval : 0f;
            CurrentStacks = 1;
            TicksOccurred = 0;
        }

        /// <summary>
        /// Default constructor for serialization.
        /// </summary>
        public BuffInstance()
        {
        }

        /// <summary>
        /// Check if this buff has expired.
        /// </summary>
        public bool IsExpired => RemainingDuration <= 0;

        /// <summary>
        /// Check if it's time for the next tick.
        /// </summary>
        public bool IsTimeForTick => Data.IsPeriodicEffect && NextTickTime <= 0;

        /// <summary>
        /// Get the buff ID from the data.
        /// </summary>
        public string BuffId => Data?.BuffId ?? string.Empty;

        /// <summary>
        /// Get the display name from the data.
        /// </summary>
        public string DisplayName => Data?.DisplayName ?? string.Empty;

        /// <summary>
        /// Update the buff instance by delta time.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last update</param>
        public void Update(float deltaTime)
        {
            RemainingDuration -= deltaTime;
            
            if (Data.IsPeriodicEffect)
            {
                NextTickTime -= deltaTime;
            }
        }

        /// <summary>
        /// Reset the tick timer after a tick occurs.
        /// </summary>
        public void ResetTickTimer()
        {
            NextTickTime = Data.TickInterval;
            TicksOccurred++;
        }

        /// <summary>
        /// Add a stack to this buff (if stackable).
        /// </summary>
        /// <returns>True if stack was added, false if at max</returns>
        public bool AddStack()
        {
            if (!Data.IsStackable) return false;
            if (CurrentStacks >= Data.MaxStacks) return false;
            
            CurrentStacks++;
            return true;
        }

        /// <summary>
        /// Refresh the duration of this buff.
        /// </summary>
        public void RefreshDuration()
        {
            RemainingDuration = Data.Duration;
        }
    }
}
