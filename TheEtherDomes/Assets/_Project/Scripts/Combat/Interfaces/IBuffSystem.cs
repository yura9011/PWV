using System;
using System.Collections.Generic;
using EtherDomes.Data;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Interface for the buff/debuff system handling temporary effects, DoTs, and HoTs.
    /// </summary>
    public interface IBuffSystem
    {
        #region Constants
        
        /// <summary>
        /// Maximum number of buffs per entity.
        /// </summary>
        int MaxBuffsPerEntity { get; } // 20
        
        /// <summary>
        /// Maximum number of debuffs per entity.
        /// </summary>
        int MaxDebuffsPerEntity { get; } // 20
        
        /// <summary>
        /// Minimum duration for any buff/debuff in seconds.
        /// </summary>
        float MinDuration { get; } // 1 second
        
        /// <summary>
        /// Maximum duration for any buff/debuff in seconds.
        /// </summary>
        float MaxDuration { get; } // 300 seconds
        
        #endregion

        #region Core Operations
        
        /// <summary>
        /// Apply a buff to an entity.
        /// </summary>
        /// <param name="targetId">Entity receiving the buff</param>
        /// <param name="buff">Buff data to apply</param>
        /// <param name="sourceId">Entity applying the buff</param>
        void ApplyBuff(ulong targetId, BuffData buff, ulong sourceId = 0);
        
        /// <summary>
        /// Apply a debuff to an entity.
        /// </summary>
        /// <param name="targetId">Entity receiving the debuff</param>
        /// <param name="debuff">Debuff data to apply</param>
        /// <param name="sourceId">Entity applying the debuff</param>
        void ApplyDebuff(ulong targetId, BuffData debuff, ulong sourceId = 0);
        
        /// <summary>
        /// Remove a specific buff from an entity.
        /// </summary>
        /// <param name="targetId">Entity to remove buff from</param>
        /// <param name="buffId">ID of the buff to remove</param>
        void RemoveBuff(ulong targetId, string buffId);
        
        /// <summary>
        /// Remove a specific debuff from an entity.
        /// </summary>
        /// <param name="targetId">Entity to remove debuff from</param>
        /// <param name="debuffId">ID of the debuff to remove</param>
        void RemoveDebuff(ulong targetId, string debuffId);
        
        /// <summary>
        /// Remove all buffs from an entity.
        /// </summary>
        /// <param name="targetId">Entity to clear buffs from</param>
        void RemoveAllBuffs(ulong targetId);
        
        /// <summary>
        /// Remove all debuffs from an entity.
        /// </summary>
        /// <param name="targetId">Entity to clear debuffs from</param>
        void RemoveAllDebuffs(ulong targetId);
        
        #endregion

        #region Queries
        
        /// <summary>
        /// Get all active buffs on an entity.
        /// </summary>
        IReadOnlyList<BuffInstance> GetActiveBuffs(ulong targetId);
        
        /// <summary>
        /// Get all active debuffs on an entity.
        /// </summary>
        IReadOnlyList<BuffInstance> GetActiveDebuffs(ulong targetId);
        
        /// <summary>
        /// Check if an entity has a specific buff.
        /// </summary>
        bool HasBuff(ulong targetId, string buffId);
        
        /// <summary>
        /// Check if an entity has a specific debuff.
        /// </summary>
        bool HasDebuff(ulong targetId, string debuffId);
        
        /// <summary>
        /// Get remaining duration of a buff/debuff.
        /// </summary>
        /// <param name="targetId">Entity to check</param>
        /// <param name="effectId">ID of the buff/debuff</param>
        /// <returns>Remaining duration in seconds, or 0 if not found</returns>
        float GetRemainingDuration(ulong targetId, string effectId);
        
        /// <summary>
        /// Get the number of active buffs on an entity.
        /// </summary>
        int GetBuffCount(ulong targetId);
        
        /// <summary>
        /// Get the number of active debuffs on an entity.
        /// </summary>
        int GetDebuffCount(ulong targetId);

        /// <summary>
        /// Check if an entity is affected by a specific CC type.
        /// Requirements: 11.1, 11.2, 11.3
        /// </summary>
        bool IsAffectedByCC(ulong targetId, CCType ccType);

        /// <summary>
        /// Get the active CC effect of a specific type on an entity.
        /// Returns null if no such CC is active.
        /// </summary>
        BuffInstance GetActiveCCEffect(ulong targetId, CCType ccType);

        /// <summary>
        /// Check if an entity is stunned (cannot perform any actions).
        /// Requirements: 11.2
        /// </summary>
        bool IsStunned(ulong targetId);

        /// <summary>
        /// Check if an entity is feared (random movement).
        /// Requirements: 11.3
        /// </summary>
        bool IsFeared(ulong targetId);

        /// <summary>
        /// Check if an entity is rooted (cannot move).
        /// </summary>
        bool IsRooted(ulong targetId);

        /// <summary>
        /// Get the total slow percentage on an entity (0.0 to 1.0).
        /// Requirements: 11.1
        /// </summary>
        float GetSlowPercent(ulong targetId);
        
        #endregion

        #region Events
        
        /// <summary>
        /// Fired when a buff is applied to an entity.
        /// Parameters: targetId, buffInstance
        /// </summary>
        event Action<ulong, BuffInstance> OnBuffApplied;
        
        /// <summary>
        /// Fired when a buff expires on an entity.
        /// Parameters: targetId, buffInstance
        /// </summary>
        event Action<ulong, BuffInstance> OnBuffExpired;
        
        /// <summary>
        /// Fired when a debuff is applied to an entity.
        /// Parameters: targetId, buffInstance
        /// </summary>
        event Action<ulong, BuffInstance> OnDebuffApplied;
        
        /// <summary>
        /// Fired when a debuff expires on an entity.
        /// Parameters: targetId, buffInstance
        /// </summary>
        event Action<ulong, BuffInstance> OnDebuffExpired;
        
        /// <summary>
        /// Fired when a DoT (Damage over Time) effect ticks.
        /// Parameters: targetId, damage, damageType, sourceId
        /// </summary>
        event Action<ulong, float, DamageType, ulong> OnDoTTick;
        
        /// <summary>
        /// Fired when a HoT (Healing over Time) effect ticks.
        /// Parameters: targetId, healing, sourceId
        /// </summary>
        event Action<ulong, float, ulong> OnHoTTick;

        /// <summary>
        /// Fired when a CC effect is applied to an entity.
        /// Parameters: targetId, ccType, duration, sourceId
        /// Requirements: 11.1, 11.2, 11.3
        /// </summary>
        event Action<ulong, CCType, float, ulong> OnCCApplied;

        /// <summary>
        /// Fired when a CC effect expires on an entity.
        /// Parameters: targetId, ccType
        /// </summary>
        event Action<ulong, CCType> OnCCExpired;
        
        #endregion
    }
}
