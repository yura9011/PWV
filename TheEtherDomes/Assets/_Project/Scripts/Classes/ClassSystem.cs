using System;
using System.Collections.Generic;
using System.Linq;
using EtherDomes.Combat;
using EtherDomes.Data;
using EtherDomes.Classes.Abilities;
using UnityEngine;

namespace EtherDomes.Classes
{
    /// <summary>
    /// Manages character classes, specializations, and class abilities.
    /// Implements the Trinity system with hybrid class flexibility.
    /// Requirement 2.1: Spec change completely replaces abilities.
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

        // Player abilities - loaded per spec (Requirement 2.1)
        private readonly Dictionary<ulong, List<AbilityData>> _playerAbilities = new();

        // Class ability definitions (loaded from ScriptableObjects in production)
        private readonly Dictionary<(CharacterClass, Specialization), List<AbilityData>> _classAbilities = new();

        public event Action<ulong, Specialization> OnSpecializationChanged;
        
        /// <summary>
        /// Event fired when a player's abilities are replaced due to spec change.
        /// </summary>
        public event Action<ulong, AbilityData[]> OnAbilitiesChanged;

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
            
            // Load abilities for the initial spec (Requirement 2.1)
            LoadAbilitiesForPlayer(playerId, charClass, spec);

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
            _playerAbilities.Remove(playerId);
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
            if (oldSpec == spec)
            {
                Debug.Log($"[ClassSystem] Player {playerId} already has spec {spec}");
                return;
            }
            
            _playerSpecs[playerId] = spec;
            
            // Requirement 2.1: Completely replace abilities when changing spec
            // Clear old abilities and load new ones
            LoadAbilitiesForPlayer(playerId, charClass, spec);

            Debug.Log($"[ClassSystem] Player {playerId} changed spec: {oldSpec} -> {spec}");
            OnSpecializationChanged?.Invoke(playerId, spec);
        }
        
        /// <summary>
        /// Loads abilities for a player based on their class and spec.
        /// Requirement 2.1: Spec change completely replaces abilities.
        /// </summary>
        private void LoadAbilitiesForPlayer(ulong playerId, CharacterClass charClass, Specialization spec)
        {
            // Clear existing abilities
            if (_playerAbilities.ContainsKey(playerId))
            {
                _playerAbilities[playerId].Clear();
            }
            else
            {
                _playerAbilities[playerId] = new List<AbilityData>();
            }
            
            // Load new abilities from definitions
            var newAbilities = ClassAbilityDefinitions.GetAllAbilitiesForSpec(charClass, spec);
            _playerAbilities[playerId].AddRange(newAbilities);
            
            Debug.Log($"[ClassSystem] Loaded {newAbilities.Count} abilities for player {playerId} ({charClass}/{spec})");
            OnAbilitiesChanged?.Invoke(playerId, newAbilities.ToArray());
        }
        
        /// <summary>
        /// Gets the current abilities for a player.
        /// </summary>
        public AbilityData[] GetPlayerAbilities(ulong playerId)
        {
            if (_playerAbilities.TryGetValue(playerId, out var abilities))
            {
                return abilities.ToArray();
            }
            return Array.Empty<AbilityData>();
        }
        
        /// <summary>
        /// Checks if two specs share any spec-specific abilities.
        /// Used for Property 6: Spec Change Replaces Abilities.
        /// </summary>
        public static bool SpecsShareAbilities(CharacterClass charClass, Specialization spec1, Specialization spec2)
        {
            if (spec1 == spec2) return true;
            
            var abilities1 = ClassAbilityDefinitions.GetAllAbilitiesForSpec(charClass, spec1);
            var abilities2 = ClassAbilityDefinitions.GetAllAbilitiesForSpec(charClass, spec2);
            
            // Get spec-specific abilities (exclude shared abilities)
            var specAbilities1 = abilities1.Where(a => a.RequiredSpec == spec1).Select(a => a.AbilityId).ToHashSet();
            var specAbilities2 = abilities2.Where(a => a.RequiredSpec == spec2).Select(a => a.AbilityId).ToHashSet();
            
            // Check for intersection of spec-specific abilities
            return specAbilities1.Intersect(specAbilities2).Any();
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
