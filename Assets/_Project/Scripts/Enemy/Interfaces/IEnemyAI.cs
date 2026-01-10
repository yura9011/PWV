using System;
using UnityEngine;

namespace EtherDomes.Enemy
{
    /// <summary>
    /// Interface for enemy AI behavior with state machine.
    /// </summary>
    public interface IEnemyAI
    {
        /// <summary>
        /// Initializes the AI with spawn position.
        /// </summary>
        void Initialize(ulong enemyId, Vector3 spawnPosition);

        /// <summary>
        /// Forces a state transition.
        /// </summary>
        void SetState(EnemyAIState state);

        /// <summary>
        /// Current AI state.
        /// </summary>
        EnemyAIState CurrentState { get; }

        /// <summary>
        /// Network ID of current target (0 if none).
        /// </summary>
        ulong CurrentTargetId { get; }

        /// <summary>
        /// Original spawn position for leashing.
        /// </summary>
        Vector3 SpawnPosition { get; }

        /// <summary>
        /// Current distance from spawn position.
        /// </summary>
        float DistanceFromSpawn { get; }

        /// <summary>
        /// Event fired when AI state changes.
        /// </summary>
        event Action<EnemyAIState> OnStateChanged;

        /// <summary>
        /// Event fired when target changes.
        /// </summary>
        event Action<ulong> OnTargetChanged;
    }

    /// <summary>
    /// Enemy AI states.
    /// </summary>
    public enum EnemyAIState
    {
        /// <summary>
        /// Standing still, waiting for player detection.
        /// </summary>
        Idle,

        /// <summary>
        /// Moving along patrol path (optional).
        /// </summary>
        Patrol,

        /// <summary>
        /// Player detected, moving to engage.
        /// </summary>
        Aggro,

        /// <summary>
        /// In combat, attacking target.
        /// </summary>
        Combat,

        /// <summary>
        /// Returning to spawn position after leash.
        /// HP is preserved during return.
        /// </summary>
        Returning,

        /// <summary>
        /// Enemy is dead.
        /// </summary>
        Dead
    }
}
