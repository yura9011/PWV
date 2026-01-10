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
                CharacterClass.Rogue => spec is Specialization.Assassination or Specialization.Combat,
                CharacterClass.Hunter => spec is Specialization.BeastMastery or Specialization.Marksmanship,
                CharacterClass.Warlock => spec is Specialization.Affliction or Specialization.Destruction,
                CharacterClass.DeathKnight => spec is Specialization.Blood or Specialization.FrostDK,
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
                CharacterClass.Rogue => new[] { Specialization.Assassination, Specialization.Combat },
                CharacterClass.Hunter => new[] { Specialization.BeastMastery, Specialization.Marksmanship },
                CharacterClass.Warlock => new[] { Specialization.Affliction, Specialization.Destruction },
                CharacterClass.DeathKnight => new[] { Specialization.Blood, Specialization.FrostDK },
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
                Specialization.Assassination => SpecRole.DPS,
                Specialization.Combat => SpecRole.DPS,
                Specialization.BeastMastery => SpecRole.DPS,
                Specialization.Marksmanship => SpecRole.DPS,
                Specialization.Affliction => SpecRole.DPS,
                Specialization.Destruction => SpecRole.DPS,
                Specialization.Blood => SpecRole.Tank,
                Specialization.FrostDK => SpecRole.DPS,
                _ => SpecRole.DPS
            };
        }

        /// <summary>
        /// Check if a class is a hybrid (can fulfill multiple roles).
        /// </summary>
        public static bool IsHybridClass(CharacterClass charClass)
        {
            return charClass is CharacterClass.Warrior or CharacterClass.Paladin 
                or CharacterClass.Priest or CharacterClass.DeathKnight;
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

        /// <summary>
        /// Get the primary resource type for a class.
        /// Requirements: 10.1, 10.2, 10.3
        /// </summary>
        public PrimaryResourceType GetPrimaryResourceType(CharacterClass charClass)
        {
            return charClass switch
            {
                CharacterClass.Warrior => PrimaryResourceType.None,        // Warrior uses Rage (secondary)
                CharacterClass.Mage => PrimaryResourceType.Mana,
                CharacterClass.Priest => PrimaryResourceType.Mana,
                CharacterClass.Paladin => PrimaryResourceType.Mana,
                CharacterClass.Rogue => PrimaryResourceType.Energy,        // Requirements 5.2
                CharacterClass.Hunter => PrimaryResourceType.Focus,        // Requirements 6.2
                CharacterClass.Warlock => PrimaryResourceType.Mana,        // Requirements 7.2
                CharacterClass.DeathKnight => PrimaryResourceType.Mana,    // Requirements 8.2 (simplified from runes)
                _ => PrimaryResourceType.None
            };
        }

        /// <summary>
        /// Get the secondary resource type for a class.
        /// Requirements: 10.1, 10.2, 10.3
        /// </summary>
        public SecondaryResourceType GetSecondaryResourceType(CharacterClass charClass)
        {
            return SecondaryResourceSystem.GetResourceTypeForClass(charClass);
        }

        /// <summary>
        /// Check if a class uses combo points.
        /// Requirements: 10.3, 5.3
        /// </summary>
        public bool UsesComboPoints(CharacterClass charClass)
        {
            return SecondaryResourceSystem.ClassUsesComboPoints(charClass);
        }

        /// <summary>
        /// Get the stat growth per level for a class.
        /// Requirements: 12.6, 12.7
        /// </summary>
        public CharacterStats GetStatGrowthPerLevel(CharacterClass charClass)
        {
            return charClass switch
            {
                CharacterClass.Warrior => new CharacterStats { Strength = 2, Stamina = 2 },
                CharacterClass.Mage => new CharacterStats { Intellect = 2, Stamina = 1 },
                CharacterClass.Priest => new CharacterStats { Intellect = 2, Stamina = 1 },
                CharacterClass.Paladin => new CharacterStats { Strength = 1, Intellect = 1, Stamina = 1 },
                CharacterClass.Rogue => new CharacterStats { Strength = 0, Stamina = 1 }, // Agility not in CharacterStats, use Strength placeholder
                CharacterClass.Hunter => new CharacterStats { Strength = 0, Stamina = 1 }, // Agility not in CharacterStats
                CharacterClass.Warlock => new CharacterStats { Intellect = 2, Stamina = 1 },
                CharacterClass.DeathKnight => new CharacterStats { Strength = 2, Stamina = 2 },
                _ => new CharacterStats()
            };
        }

        /// <summary>
        /// Get the base stats for a class at level 1.
        /// Requirements: 12.2, 12.3, 12.4, 12.5
        /// </summary>
        public CharacterStats GetBaseStatsForClass(CharacterClass charClass)
        {
            return charClass switch
            {
                CharacterClass.Warrior => new CharacterStats 
                { 
                    MaxHealth = 100, Strength = 12, Intellect = 5, Stamina = 12 
                },
                CharacterClass.Mage => new CharacterStats 
                { 
                    MaxHealth = 80, MaxMana = 150, Strength = 5, Intellect = 15, Stamina = 8 
                },
                CharacterClass.Priest => new CharacterStats 
                { 
                    MaxHealth = 80, MaxMana = 150, Strength = 5, Intellect = 14, Stamina = 9 
                },
                CharacterClass.Paladin => new CharacterStats 
                { 
                    MaxHealth = 95, MaxMana = 100, Strength = 10, Intellect = 10, Stamina = 11 
                },
                CharacterClass.Rogue => new CharacterStats 
                { 
                    MaxHealth = 90, Strength = 8, Intellect = 5, Stamina = 10 
                    // Note: Agility (15) not in CharacterStats
                },
                CharacterClass.Hunter => new CharacterStats 
                { 
                    MaxHealth = 95, Strength = 7, Intellect = 6, Stamina = 11 
                    // Note: Agility (14) not in CharacterStats
                },
                CharacterClass.Warlock => new CharacterStats 
                { 
                    MaxHealth = 85, MaxMana = 150, Strength = 5, Intellect = 15, Stamina = 9 
                },
                CharacterClass.DeathKnight => new CharacterStats 
                { 
                    MaxHealth = 110, MaxMana = 100, Strength = 14, Intellect = 8, Stamina = 14 
                },
                _ => new CharacterStats()
            };
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
