using System;
using UnityEngine;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Static event bus for combat events that need to be displayed in UI.
    /// Decouples Enemy/Combat from UI to avoid circular dependencies.
    /// </summary>
    public static class CombatEvents
    {
        /// <summary>
        /// Fired when damage is dealt. Parameters: position, damage, isCritical
        /// </summary>
        public static event Action<Vector3, float, bool> OnDamageDealt;
        
        /// <summary>
        /// Fired when healing is applied. Parameters: position, amount, isCritical
        /// </summary>
        public static event Action<Vector3, float, bool> OnHealingApplied;
        
        /// <summary>
        /// Fired when an attack misses. Parameters: position
        /// </summary>
        public static event Action<Vector3> OnMiss;
        
        /// <summary>
        /// Fired when an attack is dodged. Parameters: position
        /// </summary>
        public static event Action<Vector3> OnDodge;
        
        public static void RaiseDamageDealt(Vector3 position, float damage, bool isCritical = false)
        {
            OnDamageDealt?.Invoke(position, damage, isCritical);
        }
        
        public static void RaiseHealingApplied(Vector3 position, float amount, bool isCritical = false)
        {
            OnHealingApplied?.Invoke(position, amount, isCritical);
        }
        
        public static void RaiseMiss(Vector3 position)
        {
            OnMiss?.Invoke(position);
        }
        
        public static void RaiseDodge(Vector3 position)
        {
            OnDodge?.Invoke(position);
        }
    }
}
