using System;
using System.Collections.Generic;
using EtherDomes.Combat;
using EtherDomes.Data;
using UnityEngine;

namespace EtherDomes.Classes
{
    /// <summary>
    /// Manages character classes, specializations, and class abilities.
    /// Implements the Trinity system with hybrid class flexibility.
    /// </summary>
    public class ClassSystem : MonoBehaviour, IClassSystem
    {
        public const float MAIN_SPEC_EFFECTIVENESS = 1.0f;
        public const float OFF_SPEC_EFFECTIVENESS = 0.7f;

        private ICombatSystem _combatSystem;

        // Player class/spec tracking
        private readonly Dictionary<ulong, CharacterClass> _playerClasses = new();
        private readonly Dictionary<ulong, Specialization> _playerSpecs = new();
        private readonly Dictionary<ulong, Specialization> _playerMainSpecs = new();

        // Class ability definitions (loaded from ScriptableObjects in production)
        private readonly Dictionary<(CharacterClass, Specialization), List<AbilityData>> _classAbilities = new();

        public event Action<ulong, Specialization> OnSpecializationChanged;

        public void Initialize(ICombatSystem combatSystem)
        {
            _combatSystem = combatSystem;
            InitializeDefaultAbilities();
        }

        private void InitializeDefaultAbilities()
        {
            // Initialize empty ability lists for all class/spec combinations
            // In production, these would be loaded from ScriptableObjects
            foreach (CharacterClass charClass in Enum.GetValues(typeof(CharacterClass)))
            {
                var specs = GetSpecializationsForClass(charClass);
                foreach (var spec in specs)
                {
                    _classAbilities[(charClass, spec)] = new List<AbilityData>();
                }
            }
        }


        /// <summary>
        /// Register a player with their class and initial specialization.
        /// </summary>
        public void RegisterPlayer(ulong playerId, CharacterClass charClass, Specialization spec)
        {
            if (!IsValidSpecForClass(charClass, spec))
            {
                Debug.LogError($"[ClassSystem] Invalid spec {spec} for class {charClass}");
                return;
            }

            _playerClasses[playerId] = charClass;
            _playerSpecs[playerId] = spec;
            _playerMainSpecs[playerId] = spec;

            Debug.Log($"[ClassSystem] Registered player {playerId}: {charClass} - {spec}");
        }

        /// <summary>
        /// Unregister a player.
        /// </summary>
        public void UnregisterPlayer(ulong playerId)
        {
            _playerClasses.Remove(playerId);
            _playerSpecs.Remove(playerId);
            _playerMainSpecs.Remove(playerId);
        }

        public CharacterClass GetClass(ulong playerId)
        {
            return _playerClasses.TryGetValue(playerId, out var charClass) 
                ? charClass 
                : CharacterClass.Warrior;
        }

        public Specialization GetSpecialization(ulong playerId)
        {
            return _playerSpecs.TryGetValue(playerId, out var spec) 
                ? spec 
                : Specialization.Arms;
        }

        public void SetSpecialization(ulong playerId, Specialization spec)
        {
            if (!CanSwitchSpec(playerId))
            {
                Debug.LogWarning($"[ClassSystem] Cannot switch spec while in combat: {playerId}");
                return;
            }

            var charClass = GetClass(playerId);
            if (!IsValidSpecForClass(charClass, spec))
            {
                Debug.LogError($"[ClassSystem] Invalid spec {spec} for class {charClass}");
                return;
            }

            var oldSpec = GetSpecialization(playerId);
            _playerSpecs[playerId] = spec;

            Debug.Log($"[ClassSystem] Player {playerId} changed spec: {oldSpec} -> {spec}");
            OnSpecializationChanged?.Invoke(playerId, spec);
        }

        public AbilityData[] GetClassAbilities(CharacterClass charClass, Specialization spec)
        {
            if (_classAbilities.TryGetValue((charClass, spec), out var abilities))
            {
                return abilities.ToArray();
            }
            return Array.Empty<AbilityData>();
        }

        public bool CanSwitchSpec(ulong playerId)
        {
            // Can only switch spec when out of combat
            return _combatSystem == null || !_combatSystem.IsInCombat(playerId);
        }

        public float GetSpecEffectiveness(ulong playerId)
        {
            if (!_playerSpecs.TryGetValue(playerId, out var currentSpec) ||
                !_playerMainSpecs.TryGetValue(playerId, out var mainSpec))
            {
                return MAIN_SPEC_EFFECTIVENESS;
            }

            // Main spec = 100% effectiveness, off-spec = 70%
            return currentSpec == mainSpec ? MAIN_SPEC_EFFECTIVENESS : OFF_SPEC_EFFECTIVENESS;
        }

        public bool IsValidSpecForClass(CharacterClass charClass, Specialization spec)
        {
            return charClass switch
            {
                CharacterClass.Warrior => spec is Specialization.Protection or Specialization.Arms,
                CharacterClass.Mage => spec is Specialization.Fire or Specialization.Frost,
                CharacterClass.Priest => spec is Specialization.Holy or Specialization.Shadow,
                CharacterClass.Paladin => spec is Specialization.ProtectionPaladin 
                    or Specialization.HolyPaladin or Specialization.Retribution,
                _ => false
            };
        }

        public Specialization[] GetSpecializationsForClass(CharacterClass charClass)
        {
            return charClass switch
            {
                CharacterClass.Warrior => new[] { Specialization.Protection, Specialization.Arms },
                CharacterClass.Mage => new[] { Specialization.Fire, Specialization.Frost },
                CharacterClass.Priest => new[] { Specialization.Holy, Specialization.Shadow },
                CharacterClass.Paladin => new[] { Specialization.ProtectionPaladin, 
                    Specialization.HolyPaladin, Specialization.Retribution },
                _ => Array.Empty<Specialization>()
            };
        }

        /// <summary>
        /// Set the main spec for a player (used for effectiveness calculation).
        /// </summary>
        public void SetMainSpec(ulong playerId, Specialization mainSpec)
        {
            var charClass = GetClass(playerId);
            if (IsValidSpecForClass(charClass, mainSpec))
            {
                _playerMainSpecs[playerId] = mainSpec;
            }
        }

        /// <summary>
        /// Get the role of a specialization.
        /// </summary>
        public static SpecRole GetSpecRole(Specialization spec)
        {
            return spec switch
            {
                Specialization.Protection => SpecRole.Tank,
                Specialization.Arms => SpecRole.DPS,
                Specialization.Fire => SpecRole.DPS,
                Specialization.Frost => SpecRole.DPS,
                Specialization.Holy => SpecRole.Healer,
                Specialization.Shadow => SpecRole.DPS,
                Specialization.ProtectionPaladin => SpecRole.Tank,
                Specialization.HolyPaladin => SpecRole.Healer,
                Specialization.Retribution => SpecRole.DPS,
                _ => SpecRole.DPS
            };
        }

        /// <summary>
        /// Check if a class is a hybrid (can fulfill multiple roles).
        /// </summary>
        public static bool IsHybridClass(CharacterClass charClass)
        {
            return charClass is CharacterClass.Warrior or CharacterClass.Paladin;
        }

        /// <summary>
        /// Register abilities for a class/spec combination.
        /// </summary>
        public void RegisterAbilities(CharacterClass charClass, Specialization spec, AbilityData[] abilities)
        {
            if (!_classAbilities.ContainsKey((charClass, spec)))
            {
                _classAbilities[(charClass, spec)] = new List<AbilityData>();
            }
            _classAbilities[(charClass, spec)].AddRange(abilities);
        }
    }

    /// <summary>
    /// Role types for specializations.
    /// </summary>
    public enum SpecRole
    {
        Tank,
        Healer,
        DPS
    }
}
