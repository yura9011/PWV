using System;
using System.Collections.Generic;
using UnityEngine;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Manages friendly fire for AoE abilities.
    /// 
    /// Features:
    /// - AoE damage can affect allies if configured
    /// - AoE healing can affect enemies if configured
    /// - Configurable per-ability
    /// 
    /// Requirements: 9.4, 9.5
    /// </summary>
    public class FriendlyFireSystem : MonoBehaviour, IFriendlyFireSystem
    {
        [SerializeField] private bool _friendlyFireEnabled = true;
        [SerializeField] private LayerMask _targetableLayers;

        private readonly Collider[] _overlapResults = new Collider[50];

        public bool IsFriendlyFireEnabled => _friendlyFireEnabled;

        public event Action<ulong, ITargetable, float, bool> OnAoEApplied;

        public AoEResult ApplyAoEDamage(Vector3 center, float radius, float damage,
            ulong casterId, bool affectAllies, bool affectEnemies)
        {
            var result = new AoEResult();
            var targets = GetAffectedTargets(center, radius, casterId, affectAllies, affectEnemies);

            foreach (var target in targets)
            {
                bool isAlly = IsAlly(casterId, target);
                
                // Apply damage via IDamageable if available
                var damageable = target.Transform?.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damage);
                }
                
                result.TotalDamageDealt += damage;
                result.AffectedTargets.Add(target);

                if (isAlly)
                    result.AlliesAffected++;
                else
                    result.EnemiesAffected++;

                OnAoEApplied?.Invoke(casterId, target, damage, true);
                
                Debug.Log($"[FriendlyFire] AoE damage: {damage} to {target.DisplayName} " +
                          $"(Ally: {isAlly})");
            }

            return result;
        }

        public AoEResult ApplyAoEHealing(Vector3 center, float radius, float healing,
            ulong casterId, bool affectAllies, bool affectEnemies)
        {
            var result = new AoEResult();
            var targets = GetAffectedTargets(center, radius, casterId, affectAllies, affectEnemies);

            foreach (var target in targets)
            {
                bool isAlly = IsAlly(casterId, target);
                
                // Apply healing via IHealable if available
                var healable = target.Transform?.GetComponent<IHealable>();
                if (healable != null)
                {
                    healable.Heal(healing);
                }
                
                result.TotalHealingDone += healing;
                result.AffectedTargets.Add(target);

                if (isAlly)
                    result.AlliesAffected++;
                else
                    result.EnemiesAffected++;

                OnAoEApplied?.Invoke(casterId, target, healing, false);
                
                Debug.Log($"[FriendlyFire] AoE healing: {healing} to {target.DisplayName} " +
                          $"(Ally: {isAlly})");
            }

            return result;
        }

        public List<ITargetable> GetAffectedTargets(Vector3 center, float radius,
            ulong casterId, bool includeAllies, bool includeEnemies)
        {
            var targets = new List<ITargetable>();

            int count = Physics.OverlapSphereNonAlloc(center, radius, _overlapResults, _targetableLayers);

            for (int i = 0; i < count; i++)
            {
                var targetable = _overlapResults[i].GetComponent<ITargetable>();
                if (targetable == null)
                    continue;

                // Skip dead targets (use IsAlive from ITargetable)
                if (!targetable.IsAlive)
                    continue;

                bool isAlly = IsAlly(casterId, targetable);

                // Check if we should include this target
                if (isAlly && !includeAllies)
                    continue;
                if (!isAlly && !includeEnemies)
                    continue;

                // For friendly fire, check if it's enabled
                if (isAlly && !_friendlyFireEnabled)
                    continue;

                targets.Add(targetable);
            }

            return targets;
        }

        /// <summary>
        /// Check if a target is an ally of the caster.
        /// </summary>
        public bool IsAlly(ulong casterId, ITargetable target)
        {
            // Players are allies of other players
            // Enemies are allies of other enemies
            // This is a simplified check - in a real implementation,
            // you'd check party membership, faction, etc.
            
            var targetType = target.Type;
            
            // Assume caster is a player for now
            // In a real implementation, you'd look up the caster's type
            return targetType == Data.TargetType.Friendly;
        }

        /// <summary>
        /// Set friendly fire enabled state.
        /// </summary>
        public void SetFriendlyFireEnabled(bool enabled)
        {
            _friendlyFireEnabled = enabled;
            Debug.Log($"[FriendlyFire] Friendly fire {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// Calculate targets for a cone AoE.
        /// </summary>
        public List<ITargetable> GetConeTargets(Vector3 origin, Vector3 direction, 
            float range, float angle, ulong casterId, bool includeAllies, bool includeEnemies)
        {
            var targets = new List<ITargetable>();
            var allTargets = GetAffectedTargets(origin, range, casterId, includeAllies, includeEnemies);

            float halfAngle = angle / 2f;

            foreach (var target in allTargets)
            {
                Vector3 toTarget = (target.Position - origin).normalized;
                float targetAngle = Vector3.Angle(direction, toTarget);

                if (targetAngle <= halfAngle)
                {
                    targets.Add(target);
                }
            }

            return targets;
        }

        /// <summary>
        /// Calculate targets for a line AoE.
        /// </summary>
        public List<ITargetable> GetLineTargets(Vector3 origin, Vector3 direction,
            float range, float width, ulong casterId, bool includeAllies, bool includeEnemies)
        {
            var targets = new List<ITargetable>();
            var allTargets = GetAffectedTargets(origin, range, casterId, includeAllies, includeEnemies);

            float halfWidth = width / 2f;

            foreach (var target in allTargets)
            {
                Vector3 toTarget = target.Position - origin;
                float distance = Vector3.Dot(toTarget, direction);
                
                if (distance < 0 || distance > range)
                    continue;

                Vector3 closestPoint = origin + direction * distance;
                float perpDistance = Vector3.Distance(target.Position, closestPoint);

                if (perpDistance <= halfWidth)
                {
                    targets.Add(target);
                }
            }

            return targets;
        }
    }
}
