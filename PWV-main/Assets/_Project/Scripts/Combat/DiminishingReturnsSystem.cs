using System;
using System.Collections.Generic;
using System.Linq;
using EtherDomes.Data;
using UnityEngine;

namespace EtherDomes.Combat
{
    /// <summary>
    /// System for tracking and applying Diminishing Returns on CC effects.
    /// After 3 applications of the same CC type within 15 seconds, the target becomes immune.
    /// Requirements: 11.6, 11.7
    /// </summary>
    public class DiminishingReturnsSystem : MonoBehaviour, IDiminishingReturnsSystem
    {
        #region Constants
        
        public const float DEFAULT_DR_RESET_TIME = 15f;
        public const float DEFAULT_IMMUNITY_DURATION = 15f;
        
        // DR multipliers: 100% -> 50% -> 25% -> immune
        private static readonly float[] DR_MULTIPLIERS = { 1.0f, 0.5f, 0.25f, 0f };
        
        public float DRResetTime => DEFAULT_DR_RESET_TIME;
        public float ImmunityDuration => DEFAULT_IMMUNITY_DURATION;
        
        #endregion

        #region Private Types
        
        /// <summary>
        /// Tracks DR state for a specific CC type on a specific target.
        /// </summary>
        private class DRState
        {
            public CCType CCType;
            public int ApplicationCount;
            public float TimeSinceLastApplication;
            public bool IsImmune;
            public float ImmunityRemaining;
            
            public DRState(CCType ccType)
            {
                CCType = ccType;
                ApplicationCount = 0;
                TimeSinceLastApplication = 0f;
                IsImmune = false;
                ImmunityRemaining = 0f;
            }
        }
        
        #endregion

        #region Private Fields
        
        // DR tracking: targetId -> (ccType -> DRState)
        private readonly Dictionary<ulong, Dictionary<CCType, DRState>> _drTracking = 
            new Dictionary<ulong, Dictionary<CCType, DRState>>();
        
        #endregion

        #region Events
        
        public event Action<ulong, CCType, int, float> OnDRApplied;
        public event Action<ulong, CCType> OnImmunityStarted;
        public event Action<ulong, CCType> OnImmunityExpired;
        public event Action<ulong, CCType> OnDRReset;
        
        #endregion

        #region Unity Lifecycle
        
        private void Update()
        {
            Update(Time.deltaTime);
        }
        
        #endregion

        #region Core Operations
        
        /// <summary>
        /// Calculate the effective duration of a CC effect after applying DR.
        /// This also records the application for future DR calculations.
        /// Requirements: 11.6, 11.7
        /// </summary>
        public float ApplyDiminishingReturns(ulong targetId, CCType ccType, float baseDuration)
        {
            if (ccType == CCType.None || baseDuration <= 0)
            {
                return baseDuration;
            }
            
            var state = GetOrCreateDRState(targetId, ccType);
            
            // Check if immune
            if (state.IsImmune)
            {
                Debug.Log($"[DR] Target {targetId} is immune to {ccType}");
                OnDRApplied?.Invoke(targetId, ccType, 3, 0f);
                return 0f;
            }
            
            // Calculate effective duration based on application count
            int drLevel = Mathf.Min(state.ApplicationCount, DR_MULTIPLIERS.Length - 1);
            float multiplier = DR_MULTIPLIERS[drLevel];
            float effectiveDuration = baseDuration * multiplier;
            
            // Record this application
            state.ApplicationCount++;
            state.TimeSinceLastApplication = 0f;
            
            Debug.Log($"[DR] Applied {ccType} to {targetId}: DR level {drLevel}, " +
                     $"multiplier {multiplier:P0}, duration {baseDuration}s -> {effectiveDuration}s");
            
            OnDRApplied?.Invoke(targetId, ccType, drLevel, effectiveDuration);
            
            // Check if this triggers immunity (after 3 applications)
            if (state.ApplicationCount >= 3)
            {
                state.IsImmune = true;
                state.ImmunityRemaining = ImmunityDuration;
                Debug.Log($"[DR] Target {targetId} is now immune to {ccType} for {ImmunityDuration}s");
                OnImmunityStarted?.Invoke(targetId, ccType);
            }
            
            return effectiveDuration;
        }
        
        /// <summary>
        /// Check if a target is immune to a specific CC type.
        /// </summary>
        public bool IsImmune(ulong targetId, CCType ccType)
        {
            if (ccType == CCType.None) return false;
            
            var state = GetDRState(targetId, ccType);
            return state?.IsImmune ?? false;
        }
        
        /// <summary>
        /// Get the current DR level for a target and CC type.
        /// 0 = no DR (100%), 1 = first DR (50%), 2 = second DR (25%), 3+ = immune
        /// </summary>
        public int GetDRLevel(ulong targetId, CCType ccType)
        {
            if (ccType == CCType.None) return 0;
            
            var state = GetDRState(targetId, ccType);
            if (state == null) return 0;
            if (state.IsImmune) return 3;
            
            return Mathf.Min(state.ApplicationCount, DR_MULTIPLIERS.Length - 1);
        }
        
        /// <summary>
        /// Get the duration multiplier for the current DR level.
        /// </summary>
        public float GetDurationMultiplier(ulong targetId, CCType ccType)
        {
            int drLevel = GetDRLevel(targetId, ccType);
            if (drLevel >= DR_MULTIPLIERS.Length) return 0f;
            return DR_MULTIPLIERS[drLevel];
        }
        
        /// <summary>
        /// Get the remaining immunity time for a target and CC type.
        /// Returns 0 if not immune.
        /// </summary>
        public float GetImmunityRemaining(ulong targetId, CCType ccType)
        {
            if (ccType == CCType.None) return 0f;
            
            var state = GetDRState(targetId, ccType);
            if (state == null || !state.IsImmune) return 0f;
            
            return state.ImmunityRemaining;
        }
        
        /// <summary>
        /// Clear all DR tracking for an entity (e.g., on death or zone change).
        /// </summary>
        public void ClearDR(ulong targetId)
        {
            if (_drTracking.ContainsKey(targetId))
            {
                _drTracking.Remove(targetId);
                Debug.Log($"[DR] Cleared all DR tracking for {targetId}");
            }
        }
        
        /// <summary>
        /// Update the system, processing DR resets and immunity expiration.
        /// </summary>
        public void Update(float deltaTime)
        {
            var targetsToClean = new List<ulong>();
            
            foreach (var kvp in _drTracking)
            {
                ulong targetId = kvp.Key;
                var ccStates = kvp.Value;
                var ccTypesToReset = new List<CCType>();
                
                foreach (var ccKvp in ccStates)
                {
                    var state = ccKvp.Value;
                    
                    // Update immunity timer
                    if (state.IsImmune)
                    {
                        state.ImmunityRemaining -= deltaTime;
                        if (state.ImmunityRemaining <= 0)
                        {
                            state.IsImmune = false;
                            state.ImmunityRemaining = 0f;
                            state.ApplicationCount = 0;
                            state.TimeSinceLastApplication = 0f;
                            Debug.Log($"[DR] Immunity expired for {targetId} against {state.CCType}");
                            OnImmunityExpired?.Invoke(targetId, state.CCType);
                        }
                    }
                    else
                    {
                        // Update time since last application
                        state.TimeSinceLastApplication += deltaTime;
                        
                        // Check for DR reset (15s without that CC type)
                        if (state.ApplicationCount > 0 && state.TimeSinceLastApplication >= DRResetTime)
                        {
                            ccTypesToReset.Add(state.CCType);
                        }
                    }
                }
                
                // Reset DR for CC types that haven't been applied in 15s
                foreach (var ccType in ccTypesToReset)
                {
                    if (ccStates.TryGetValue(ccType, out var state))
                    {
                        state.ApplicationCount = 0;
                        state.TimeSinceLastApplication = 0f;
                        Debug.Log($"[DR] DR reset for {targetId} against {ccType}");
                        OnDRReset?.Invoke(targetId, ccType);
                    }
                }
                
                // Mark empty targets for cleanup
                if (ccStates.Count == 0 || ccStates.Values.All(s => s.ApplicationCount == 0 && !s.IsImmune))
                {
                    targetsToClean.Add(targetId);
                }
            }
            
            // Clean up empty entries
            foreach (var targetId in targetsToClean)
            {
                _drTracking.Remove(targetId);
            }
        }
        
        #endregion

        #region Private Methods
        
        private DRState GetDRState(ulong targetId, CCType ccType)
        {
            if (!_drTracking.TryGetValue(targetId, out var ccStates))
            {
                return null;
            }
            
            ccStates.TryGetValue(ccType, out var state);
            return state;
        }
        
        private DRState GetOrCreateDRState(ulong targetId, CCType ccType)
        {
            if (!_drTracking.TryGetValue(targetId, out var ccStates))
            {
                ccStates = new Dictionary<CCType, DRState>();
                _drTracking[targetId] = ccStates;
            }
            
            if (!ccStates.TryGetValue(ccType, out var state))
            {
                state = new DRState(ccType);
                ccStates[ccType] = state;
            }
            
            return state;
        }
        
        #endregion
    }
}
