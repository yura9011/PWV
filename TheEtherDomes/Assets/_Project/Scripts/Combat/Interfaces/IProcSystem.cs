using System;
using EtherDomes.Data;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Interface for the proc (random effect trigger) system.
    /// Requirements: 9.1
    /// </summary>
    public interface IProcSystem
    {
        /// <summary>
        /// Register a proc effect that can trigger.
        /// </summary>
        void RegisterProc(ProcDefinition proc);

        /// <summary>
        /// Unregister a proc effect.
        /// </summary>
        void UnregisterProc(string procId);

        /// <summary>
        /// Check and potentially trigger procs based on an event.
        /// </summary>
        void CheckProcs(ulong playerId, ProcTrigger trigger, AbilityData ability = null);

        /// <summary>
        /// Get all registered procs for a player.
        /// </summary>
        ProcDefinition[] GetPlayerProcs(ulong playerId);

        /// <summary>
        /// Check if a proc is on internal cooldown.
        /// </summary>
        bool IsOnInternalCooldown(ulong playerId, string procId);

        /// <summary>
        /// Event fired when a proc triggers.
        /// </summary>
        event Action<ulong, ProcDefinition> OnProcTriggered;
    }

    /// <summary>
    /// Definition of a proc effect.
    /// </summary>
    public class ProcDefinition
    {
        public string ProcId;
        public string ProcName;
        public float Probability;           // 0-1 chance to trigger
        public float InternalCooldown;      // Seconds before can trigger again
        public ProcTrigger TriggerType;     // What triggers this proc
        public ProcEffect Effect;           // What happens when triggered
        public float EffectValue;           // Damage/healing/buff amount
        public float EffectDuration;        // Duration for buffs/debuffs
        public ulong OwnerId;               // Player who owns this proc
    }

    /// <summary>
    /// What can trigger a proc.
    /// </summary>
    public enum ProcTrigger
    {
        OnDamageDealt,
        OnDamageTaken,
        OnHealingDone,
        OnHealingReceived,
        OnCriticalHit,
        OnAbilityUse,
        OnSpellCast,
        OnMeleeHit,
        OnDodge,
        OnParry,
        OnBlock
    }

    /// <summary>
    /// What effect a proc has.
    /// </summary>
    public enum ProcEffect
    {
        InstantDamage,
        InstantHealing,
        DamageOverTime,
        HealingOverTime,
        BuffStat,
        DebuffStat,
        ResetCooldown,
        RestoreMana,
        RestoreResource
    }
}
