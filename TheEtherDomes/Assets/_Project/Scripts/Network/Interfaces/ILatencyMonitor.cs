using System;

namespace EtherDomes.Network
{
    /// <summary>
    /// Interface for monitoring network latency and pausing actions during high latency.
    /// </summary>
    public interface ILatencyMonitor
    {
        /// <summary>
        /// Current latency in milliseconds.
        /// </summary>
        float CurrentLatency { get; }

        /// <summary>
        /// Average latency over recent samples in milliseconds.
        /// </summary>
        float AverageLatency { get; }

        /// <summary>
        /// Current latency state (Normal, Warning, Paused).
        /// </summary>
        LatencyState CurrentState { get; }

        /// <summary>
        /// True if latency is above warning threshold (200ms).
        /// </summary>
        bool IsHighLatency { get; }

        /// <summary>
        /// True if actions are paused due to high latency (>500ms).
        /// </summary>
        bool IsPaused { get; }

        /// <summary>
        /// Event fired when latency state changes.
        /// </summary>
        event Action<LatencyState> OnLatencyStateChanged;

        /// <summary>
        /// Event fired when actions are paused due to high latency.
        /// </summary>
        event Action OnActionsPaused;

        /// <summary>
        /// Event fired when actions resume after latency improves.
        /// </summary>
        event Action OnActionsResumed;

        /// <summary>
        /// Warning threshold in milliseconds (200ms).
        /// </summary>
        float WarningThreshold { get; }

        /// <summary>
        /// Pause threshold in milliseconds (500ms).
        /// </summary>
        float PauseThreshold { get; }

        /// <summary>
        /// Resume threshold in milliseconds (400ms).
        /// Latency must drop below this to resume from paused state.
        /// </summary>
        float ResumeThreshold { get; }
    }

    /// <summary>
    /// Latency state levels.
    /// </summary>
    public enum LatencyState
    {
        /// <summary>
        /// Latency below 200ms - normal operation.
        /// </summary>
        Normal,

        /// <summary>
        /// Latency between 200-500ms - warning state.
        /// </summary>
        Warning,

        /// <summary>
        /// Latency above 500ms - actions paused.
        /// </summary>
        Paused
    }
}
