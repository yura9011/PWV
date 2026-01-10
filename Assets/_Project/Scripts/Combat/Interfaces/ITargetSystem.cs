using System;
using System.Collections.Generic;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Interface for the Tab-Target selection system.
    /// </summary>
    public interface ITargetSystem
    {
        /// <summary>
        /// Cycle to the next visible enemy within range (Tab key).
        /// </summary>
        void CycleTarget();

        /// <summary>
        /// Select a specific target (click).
        /// </summary>
        /// <param name="target">Target to select</param>
        void SelectTarget(ITargetable target);

        /// <summary>
        /// Clear the current target selection (Escape key).
        /// </summary>
        void ClearTarget();

        /// <summary>
        /// Currently selected target, or null if none.
        /// </summary>
        ITargetable CurrentTarget { get; }

        /// <summary>
        /// Whether a target is currently selected.
        /// </summary>
        bool HasTarget { get; }

        /// <summary>
        /// Whether the current target is within ability range.
        /// </summary>
        bool IsTargetInRange { get; }

        /// <summary>
        /// Distance to the current target in meters.
        /// </summary>
        float TargetDistance { get; }

        /// <summary>
        /// Fired when the target changes.
        /// </summary>
        event Action<ITargetable> OnTargetChanged;

        /// <summary>
        /// Fired when the target is lost (died, out of range, etc.).
        /// </summary>
        event Action OnTargetLost;

        /// <summary>
        /// Maximum targeting range in meters.
        /// </summary>
        float MaxTargetRange { get; }

        /// <summary>
        /// Get all targetable entities within range.
        /// </summary>
        IReadOnlyList<ITargetable> GetTargetsInRange();

        /// <summary>
        /// Register a targetable entity.
        /// </summary>
        void RegisterTarget(ITargetable target);

        /// <summary>
        /// Unregister a targetable entity.
        /// </summary>
        void UnregisterTarget(ITargetable target);
    }
}
