using System;
using System.Collections.Generic;
using EtherDomes.Data;
using UnityEngine;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Manages proc (random effect trigger) system.
    /// 
    /// Features:
    /// - Register procs with probability
    /// - Check procs on various triggers
    /// - Internal cooldown to prevent spam
    /// 
    /// Requirements: 9.1, 9.2
    /// </summary>
    public class ProcSystem : MonoBehaviour, IProcSystem
    {
        // playerId -> list of procs
        private readonly Dictionary<ulong, List<ProcDefinition>> _playerProcs = new();
        
        // playerId -> (procId -> last trigger time)
        private readonly Dictionary<ulong, Dictionary<string, float>> _internalCooldowns = new();

        public event Action<ulong, ProcDefinition> OnProcTriggered;

        public void RegisterProc(ProcDefinition proc)
        {
            if (proc == null || string.IsNullOrEmpty(proc.ProcId))
            {
                Debug.LogWarning("[ProcSystem] Cannot register null or invalid proc");
                return;
            }

            if (!_playerProcs.ContainsKey(proc.OwnerId))
            {
                _playerProcs[proc.OwnerId] = new List<ProcDefinition>();
            }

            // Remove existing proc with same ID
            _playerProcs[proc.OwnerId].RemoveAll(p => p.ProcId == proc.ProcId);
            _playerProcs[proc.OwnerId].Add(proc);

            Debug.Log($"[ProcSystem] Registered proc '{proc.ProcName}' for player {proc.OwnerId} " +
                      $"(Trigger: {proc.TriggerType}, Chance: {proc.Probability * 100}%)");
        }

        public void UnregisterProc(string procId)
        {
            foreach (var playerProcs in _playerProcs.Values)
            {
                playerProcs.RemoveAll(p => p.ProcId == procId);
            }
        }

        public void CheckProcs(ulong playerId, ProcTrigger trigger, AbilityData ability = null)
        {
            if (!_playerProcs.TryGetValue(playerId, out var procs))
                return;

            foreach (var proc in procs)
            {
                if (proc.TriggerType != trigger)
                    continue;

                if (IsOnInternalCooldown(playerId, proc.ProcId))
                    continue;

                // Roll for proc
                if (TryTriggerProc(proc))
                {
                    ApplyProcEffect(playerId, proc);
                    SetInternalCooldown(playerId, proc.ProcId, proc.InternalCooldown);
                    
                    Debug.Log($"[ProcSystem] Proc triggered: {proc.ProcName} for player {playerId}");
                    OnProcTriggered?.Invoke(playerId, proc);
                }
            }
        }

        public ProcDefinition[] GetPlayerProcs(ulong playerId)
        {
            if (!_playerProcs.TryGetValue(playerId, out var procs))
                return Array.Empty<ProcDefinition>();

            return procs.ToArray();
        }

        public bool IsOnInternalCooldown(ulong playerId, string procId)
        {
            if (!_internalCooldowns.TryGetValue(playerId, out var cooldowns))
                return false;

            if (!cooldowns.TryGetValue(procId, out float lastTrigger))
                return false;

            return Time.time < lastTrigger;
        }

        /// <summary>
        /// Roll for proc based on probability.
        /// </summary>
        public bool TryTriggerProc(ProcDefinition proc)
        {
            if (proc.Probability <= 0)
                return false;

            if (proc.Probability >= 1)
                return true;

            float roll = UnityEngine.Random.value;
            return roll < proc.Probability;
        }

        private void ApplyProcEffect(ulong playerId, ProcDefinition proc)
        {
            // In a real implementation, this would apply the actual effect
            // For now, we just log and fire the event
            switch (proc.Effect)
            {
                case ProcEffect.InstantDamage:
                    Debug.Log($"[ProcSystem] Applying {proc.EffectValue} instant damage from {proc.ProcName}");
                    break;
                    
                case ProcEffect.InstantHealing:
                    Debug.Log($"[ProcSystem] Applying {proc.EffectValue} instant healing from {proc.ProcName}");
                    break;
                    
                case ProcEffect.BuffStat:
                    Debug.Log($"[ProcSystem] Applying stat buff from {proc.ProcName} for {proc.EffectDuration}s");
                    break;
                    
                case ProcEffect.RestoreMana:
                    Debug.Log($"[ProcSystem] Restoring {proc.EffectValue} mana from {proc.ProcName}");
                    break;
                    
                default:
                    Debug.Log($"[ProcSystem] Applying effect {proc.Effect} from {proc.ProcName}");
                    break;
            }
        }

        private void SetInternalCooldown(ulong playerId, string procId, float duration)
        {
            if (duration <= 0)
                return;

            if (!_internalCooldowns.ContainsKey(playerId))
            {
                _internalCooldowns[playerId] = new Dictionary<string, float>();
            }

            _internalCooldowns[playerId][procId] = Time.time + duration;
        }

        /// <summary>
        /// Clear all procs for a player.
        /// </summary>
        public void ClearPlayerProcs(ulong playerId)
        {
            _playerProcs.Remove(playerId);
            _internalCooldowns.Remove(playerId);
        }

        /// <summary>
        /// Clear all procs.
        /// </summary>
        public void ClearAll()
        {
            _playerProcs.Clear();
            _internalCooldowns.Clear();
        }

        /// <summary>
        /// Get proc count for testing.
        /// </summary>
        public int GetProcCount(ulong playerId)
        {
            return _playerProcs.TryGetValue(playerId, out var procs) ? procs.Count : 0;
        }
    }
}
