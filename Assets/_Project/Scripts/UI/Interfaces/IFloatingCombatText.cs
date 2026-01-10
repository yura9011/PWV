using System;
using UnityEngine;

namespace EtherDomes.UI
{
    /// <summary>
    /// Type of floating combat text.
    /// </summary>
    public enum FCTType
    {
        Damage,
        CriticalDamage,
        Healing,
        CriticalHealing,
        Miss,
        Dodge,
        Parry,
        Block,
        Status
    }

    /// <summary>
    /// Data for a floating combat text instance.
    /// </summary>
    public struct FCTData
    {
        public FCTType Type;
        public float Value;
        public string Text;
        public Vector3 WorldPosition;
        public bool IsCritical;
    }

    /// <summary>
    /// Interface for the floating combat text system.
    /// Requirements: 6.3
    /// </summary>
    public interface IFloatingCombatText
    {
        /// <summary>
        /// Shows damage text at a position.
        /// </summary>
        /// <param name="position">World position</param>
        /// <param name="damage">Damage amount</param>
        /// <param name="isCritical">Whether it was a critical hit</param>
        void ShowDamage(Vector3 position, float damage, bool isCritical = false);

        /// <summary>
        /// Shows healing text at a position.
        /// </summary>
        /// <param name="position">World position</param>
        /// <param name="healing">Healing amount</param>
        /// <param name="isCritical">Whether it was a critical heal</param>
        void ShowHealing(Vector3 position, float healing, bool isCritical = false);

        /// <summary>
        /// Shows miss text at a position.
        /// </summary>
        /// <param name="position">World position</param>
        void ShowMiss(Vector3 position);

        /// <summary>
        /// Shows status text at a position.
        /// </summary>
        /// <param name="position">World position</param>
        /// <param name="status">Status text (e.g., "Stunned", "Silenced")</param>
        void ShowStatus(Vector3 position, string status);

        /// <summary>
        /// Shows dodge text at a position.
        /// </summary>
        void ShowDodge(Vector3 position);

        /// <summary>
        /// Shows parry text at a position.
        /// </summary>
        void ShowParry(Vector3 position);

        /// <summary>
        /// Shows block text at a position.
        /// </summary>
        /// <param name="position">World position</param>
        /// <param name="blockedAmount">Amount blocked (optional)</param>
        void ShowBlock(Vector3 position, float blockedAmount = 0);

        /// <summary>
        /// Event fired when FCT is created.
        /// </summary>
        event Action<FCTData> OnFCTCreated;
    }
}
