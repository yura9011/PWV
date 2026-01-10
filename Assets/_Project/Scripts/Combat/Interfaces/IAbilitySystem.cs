using System;
using EtherDomes.Data;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Interface for the ability execution system with GCD and cooldowns.
    /// </summary>
    public interface IAbilitySystem
    {
        /// <summary>
        /// Attempt to execute an ability in the given slot (1-9).
        /// </summary>
        /// <param name="slotIndex">Slot index (0-8 for keys 1-9)</param>
        /// <returns>True if ability execution started</returns>
        bool TryExecuteAbility(int slotIndex);

        /// <summary>
        /// Interrupt the current cast.
        /// </summary>
        void InterruptCast();

        /// <summary>
        /// Interrupt the current channel.
        /// </summary>
        void InterruptChannel();

        /// <summary>
        /// Whether the Global Cooldown is active.
        /// </summary>
        bool IsOnGCD { get; }

        /// <summary>
        /// Time remaining on the GCD in seconds.
        /// </summary>
        float GCDRemaining { get; }

        /// <summary>
        /// Whether currently casting an ability.
        /// </summary>
        bool IsCasting { get; }

        /// <summary>
        /// Whether currently channeling an ability.
        /// </summary>
        bool IsChanneling { get; }

        /// <summary>
        /// Cast progress (0-1).
        /// </summary>
        float CastProgress { get; }

        /// <summary>
        /// Channel progress (0-1, depletes from 1 to 0).
        /// </summary>
        float ChannelProgress { get; }

        /// <summary>
        /// Number of ticks completed in current channel.
        /// </summary>
        int ChannelTicksCompleted { get; }

        /// <summary>
        /// The ability currently being cast, or null.
        /// </summary>
        AbilityData CurrentCastAbility { get; }

        /// <summary>
        /// The ability currently being channeled, or null.
        /// </summary>
        AbilityData CurrentChannelAbility { get; }

        /// <summary>
        /// Get all abilities in the action bar.
        /// </summary>
        AbilityState[] GetAbilities();

        /// <summary>
        /// Get the ability in a specific slot.
        /// </summary>
        AbilityState GetAbility(int slotIndex);

        /// <summary>
        /// Set an ability in a specific slot.
        /// </summary>
        void SetAbility(int slotIndex, AbilityData ability);

        /// <summary>
        /// Get cooldown remaining for an ability slot.
        /// </summary>
        float GetCooldownRemaining(int slotIndex);

        /// <summary>
        /// Fired when GCD starts.
        /// </summary>
        event Action OnGCDStarted;

        /// <summary>
        /// Fired when GCD ends.
        /// </summary>
        event Action OnGCDEnded;

        /// <summary>
        /// Fired when a cast starts.
        /// </summary>
        event Action<AbilityData> OnCastStarted;

        /// <summary>
        /// Fired when a cast completes successfully.
        /// </summary>
        event Action<AbilityData> OnCastCompleted;

        /// <summary>
        /// Fired when a cast is interrupted.
        /// </summary>
        event Action<AbilityData> OnCastInterrupted;

        /// <summary>
        /// Fired when a channel starts.
        /// </summary>
        event Action<AbilityData> OnChannelStarted;

        /// <summary>
        /// Fired when a channel tick occurs.
        /// </summary>
        event Action<AbilityData, int> OnChannelTick;

        /// <summary>
        /// Fired when a channel completes all ticks.
        /// </summary>
        event Action<AbilityData> OnChannelCompleted;

        /// <summary>
        /// Fired when a channel is interrupted.
        /// </summary>
        event Action<AbilityData> OnChannelInterrupted;

        /// <summary>
        /// Fired when an ability cannot be used (error message).
        /// </summary>
        event Action<string> OnAbilityError;

        /// <summary>
        /// Fired when an ability is executed (for combat system integration).
        /// </summary>
        event Action<AbilityData, ITargetable> OnAbilityExecuted;

        /// <summary>
        /// Fired when Drain Life or similar ability heals the caster.
        /// Parameters: casterId, healAmount, ability
        /// Requirements: 7.7
        /// </summary>
        event Action<ulong, float, AbilityData> OnDrainLifeHealing;

        /// <summary>
        /// Fired when a pull effect (Death Grip) is executed.
        /// Parameters: casterId, targetId, casterPosition
        /// Requirements: 11.4
        /// </summary>
        event Action<ulong, ulong, UnityEngine.Vector3> OnPullEffect;

        /// <summary>
        /// Fired when a knockback self effect (Disengage) is executed.
        /// Parameters: casterId, direction, distance
        /// Requirements: 11.5
        /// </summary>
        event Action<ulong, UnityEngine.Vector3, float> OnKnockbackSelf;

        /// <summary>
        /// Duration of the Global Cooldown in seconds.
        /// </summary>
        float GlobalCooldownDuration { get; }

        /// <summary>
        /// Number of ability slots (typically 9).
        /// </summary>
        int SlotCount { get; }
    }
}
