using System;
using System.Collections.Generic;
using UnityEngine;

namespace EtherDomes.Network
{
    /// <summary>
    /// Monitors network latency and manages action pausing during high latency periods.
    /// Thresholds: Warning at 200ms, Pause at 500ms, Resume at 400ms.
    /// </summary>
    public class LatencyMonitor : ILatencyMonitor
    {
        private const int SAMPLE_COUNT = 10;
        private const float DEFAULT_WARNING_THRESHOLD = 200f;
        private const float DEFAULT_PAUSE_THRESHOLD = 500f;
        private const float DEFAULT_RESUME_THRESHOLD = 400f;

        private readonly Queue<float> _latencySamples = new();
        private float _currentLatency;
        private LatencyState _currentState = LatencyState.Normal;

        public float CurrentLatency => _currentLatency;
        public float AverageLatency => CalculateAverageLatency();
        public LatencyState CurrentState => _currentState;
        public bool IsHighLatency => _currentLatency >= WarningThreshold;
        public bool IsPaused => _currentState == LatencyState.Paused;

        public float WarningThreshold { get; }
        public float PauseThreshold { get; }
        public float ResumeThreshold { get; }

        public event Action<LatencyState> OnLatencyStateChanged;
        public event Action OnActionsPaused;
        public event Action OnActionsResumed;

        public LatencyMonitor() 
            : this(DEFAULT_WARNING_THRESHOLD, DEFAULT_PAUSE_THRESHOLD, DEFAULT_RESUME_THRESHOLD)
        {
        }

        public LatencyMonitor(float warningThreshold, float pauseThreshold, float resumeThreshold)
        {
            WarningThreshold = warningThreshold;
            PauseThreshold = pauseThreshold;
            ResumeThreshold = resumeThreshold;
        }

        /// <summary>
        /// Updates the latency monitor with a new RTT sample.
        /// Should be called regularly with network RTT measurements.
        /// </summary>
        /// <param name="latencyMs">Round-trip time in milliseconds.</param>
        public void UpdateLatency(float latencyMs)
        {
            _currentLatency = latencyMs;
            
            // Add to samples for averaging
            _latencySamples.Enqueue(latencyMs);
            while (_latencySamples.Count > SAMPLE_COUNT)
            {
                _latencySamples.Dequeue();
            }

            // Determine new state
            LatencyState newState = DetermineState(latencyMs);
            
            if (newState != _currentState)
            {
                LatencyState previousState = _currentState;
                _currentState = newState;
                
                Debug.Log($"[LatencyMonitor] State changed: {previousState} -> {newState} (latency: {latencyMs}ms)");
                OnLatencyStateChanged?.Invoke(newState);

                // Fire specific events
                if (newState == LatencyState.Paused && previousState != LatencyState.Paused)
                {
                    OnActionsPaused?.Invoke();
                }
                else if (newState != LatencyState.Paused && previousState == LatencyState.Paused)
                {
                    OnActionsResumed?.Invoke();
                }
            }
        }

        private LatencyState DetermineState(float latencyMs)
        {
            // If currently paused, need to drop below resume threshold to unpause
            if (_currentState == LatencyState.Paused)
            {
                if (latencyMs < ResumeThreshold)
                {
                    // Check if we should go to Warning or Normal
                    return latencyMs >= WarningThreshold ? LatencyState.Warning : LatencyState.Normal;
                }
                return LatencyState.Paused;
            }

            // Normal state transitions
            if (latencyMs >= PauseThreshold)
            {
                return LatencyState.Paused;
            }
            
            if (latencyMs >= WarningThreshold)
            {
                return LatencyState.Warning;
            }

            return LatencyState.Normal;
        }

        private float CalculateAverageLatency()
        {
            if (_latencySamples.Count == 0)
                return 0f;

            float sum = 0f;
            foreach (var sample in _latencySamples)
            {
                sum += sample;
            }
            return sum / _latencySamples.Count;
        }

        /// <summary>
        /// Resets the latency monitor to initial state.
        /// </summary>
        public void Reset()
        {
            _latencySamples.Clear();
            _currentLatency = 0f;
            
            if (_currentState != LatencyState.Normal)
            {
                bool wasPaused = _currentState == LatencyState.Paused;
                _currentState = LatencyState.Normal;
                OnLatencyStateChanged?.Invoke(LatencyState.Normal);
                
                if (wasPaused)
                {
                    OnActionsResumed?.Invoke();
                }
            }
        }

        /// <summary>
        /// Gets the number of latency samples currently stored.
        /// </summary>
        public int SampleCount => _latencySamples.Count;
    }
}
