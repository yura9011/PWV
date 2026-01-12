using System;
using EtherDomes.Data;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Interface for the core combat system handling damage, healing, death, and resurrection.
    /// </summary>
    public interface ICombatSystem
    {
        /// <summary>
        /// Apply damage to an entity.
        /// </summary>
        /// <param name="targetId">Entity receiving damage</param>
        /// <param name="damage">Amount of damage</param>
        /// <param name="type">Type of damage</param>
        /// <param name="sourceId">Entity dealing damage (for threat)</param>
        void ApplyDamage(ulong targetId, float damage, DamageType type, ulong sourceId = 0);

        /// <summary>
        /// Apply healing to an entity.
        /// </summary>
        /// <param name="targetId">Entity receiving healing</param>
        /// <param name="healing">Amount of healing</param>
        /// <param name="sourceId">Entity doing the healing (for threat)</param>
        void ApplyHealing(ulong targetId, float healing, ulong sourceId = 0);

        /// <summary>
        /// Kill an entity immediately.
        /// </summary>
        void Kill(ulong targetId);

        /// <summary>
        /// Resurrect a dead player.
        /// </summary>
        /// <param name="targetId">Player to resurrect</param>
        /// <param name="healthPercent">Health percentage to resurrect with (0-1)</param>
        void Resurrect(ulong targetId, float healthPercent);

        /// <summary>
        /// Release spirit (respawn at graveyard).
        /// </summary>
        void ReleaseSpirit(ulong playerId);

        /// <summary>
        /// Check if an entity is in combat.
        /// </summary>
        bool IsInCombat(ulong entityId);

        /// <summary>
        /// Check if an entity is dead.
        /// </summary>
        bool IsDead(ulong entityId);

        /// <summary>
        /// Get current health of an entity.
        /// </summary>
        float GetHealth(ulong entityId);

        /// <summary>
        /// Get max health of an entity.
        /// </summary>
        float GetMaxHealth(ulong entityId);

        /// <summary>
        /// Get health percentage (0-1).
        /// </summary>
        float GetHealthPercent(ulong entityId);

        /// <summary>
        /// Set an entity's health values.
        /// </summary>
        void SetHealth(ulong entityId, float current, float max);

        /// <summary>
        /// Enter combat for an entity.
        /// </summary>
        void EnterCombat(ulong entityId);

        /// <summary>
        /// Leave combat for an entity.
        /// </summary>
        void LeaveCombat(ulong entityId);

        /// <summary>
        /// Fired when an entity dies.
        /// </summary>
        event Action<ulong> OnEntityDied;

        /// <summary>
        /// Fired when an entity is resurrected.
        /// </summary>
        event Action<ulong> OnEntityResurrected;

        /// <summary>
        /// Fired when all players in a dungeon die (wipe).
        /// </summary>
        event Action OnWipe;

        /// <summary>
        /// Fired when damage is dealt.
        /// </summary>
        event Action<ulong, ulong, float, DamageType> OnDamageDealt; // target, source, amount, type

        /// <summary>
        /// Fired when healing is done.
        /// </summary>
        event Action<ulong, ulong, float> OnHealingDone; // target, source, amount

        /// <summary>
        /// Time window for resurrection in seconds.
        /// </summary>
        float ResurrectionWindowSeconds { get; }

        /// <summary>
        /// Health percentage when respawning at graveyard.
        /// </summary>
        float RespawnHealthPercent { get; }

        /// <summary>
        /// Gets the total damage taken by an entity in the last 5 seconds.
        /// Requirements 8.8: Used for Death Strike healing calculation
        /// </summary>
        float GetRecentDamageTaken(ulong entityId);

        /// <summary>
        /// Calculates Death Strike healing amount.
        /// Requirements 8.8: Heal = 25% of damage taken in last 5 seconds (min 10% max HP)
        /// </summary>
        float CalculateDeathStrikeHealing(ulong entityId);
    }
}
