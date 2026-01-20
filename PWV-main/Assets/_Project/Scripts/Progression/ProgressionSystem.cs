using System;
using System.Collections.Generic;
using EtherDomes.Data;
using UnityEngine;

namespace EtherDomes.Progression
{
    /// <summary>
    /// Manages character progression including experience, levels, and stats.
    /// </summary>
    public class ProgressionSystem : MonoBehaviour, IProgressionSystem
    {
        public const int MAX_LEVEL = 60;
        public const int BASE_XP_REQUIREMENT = 100;
        public const float XP_SCALING_FACTOR = 1.15f;
        public const float LOW_LEVEL_ENEMY_XP_PENALTY = 0.1f;
        public const int LEVEL_DIFFERENCE_THRESHOLD = 10;

        private static readonly int[] _abilityUnlockLevels = { 10, 20, 30, 40, 50 };

        // Player progression data
        private readonly Dictionary<ulong, int> _playerLevels = new();
        private readonly Dictionary<ulong, int> _playerExperience = new();
        private readonly Dictionary<ulong, CharacterClass> _playerClasses = new();

        public event Action<ulong, int> OnLevelUp;
        public event Action<ulong, AbilityData> OnAbilityUnlocked;

        public int MaxLevel => MAX_LEVEL;
        public int[] AbilityUnlockLevels => _abilityUnlockLevels;

        /// <summary>
        /// Register a player with initial level and class.
        /// </summary>
        public void RegisterPlayer(ulong playerId, int level, CharacterClass charClass)
        {
            _playerLevels[playerId] = Mathf.Clamp(level, 1, MAX_LEVEL);
            _playerExperience[playerId] = 0;
            _playerClasses[playerId] = charClass;

            Debug.Log($"[ProgressionSystem] Registered player {playerId}: Level {level} {charClass}");
        }

        /// <summary>
        /// Unregister a player.
        /// </summary>
        public void UnregisterPlayer(ulong playerId)
        {
            _playerLevels.Remove(playerId);
            _playerExperience.Remove(playerId);
            _playerClasses.Remove(playerId);
        }


        public void AddExperience(ulong playerId, int amount)
        {
            if (amount <= 0) return;
            if (!_playerLevels.TryGetValue(playerId, out int currentLevel)) return;
            if (currentLevel >= MAX_LEVEL) return;

            int currentXP = GetExperience(playerId);
            int newXP = currentXP + amount;
            _playerExperience[playerId] = newXP;

            Debug.Log($"[ProgressionSystem] Player {playerId} gained {amount} XP (Total: {newXP})");

            // Check for level up
            CheckLevelUp(playerId);
        }

        private void CheckLevelUp(ulong playerId)
        {
            int currentLevel = GetLevel(playerId);
            int currentXP = GetExperience(playerId);
            int xpRequired = GetExperienceRequiredForLevel(currentLevel + 1);

            while (currentXP >= xpRequired && currentLevel < MAX_LEVEL)
            {
                // Level up!
                currentLevel++;
                currentXP -= xpRequired;
                _playerLevels[playerId] = currentLevel;
                _playerExperience[playerId] = currentXP;

                Debug.Log($"[ProgressionSystem] Player {playerId} leveled up to {currentLevel}!");
                OnLevelUp?.Invoke(playerId, currentLevel);

                // Check for ability unlock
                CheckAbilityUnlock(playerId, currentLevel);

                // Get next level requirement
                xpRequired = GetExperienceRequiredForLevel(currentLevel + 1);
            }
        }

        private void CheckAbilityUnlock(ulong playerId, int level)
        {
            if (Array.IndexOf(_abilityUnlockLevels, level) >= 0)
            {
                // In a real implementation, this would get the actual ability from ClassSystem
                var unlockedAbility = new AbilityData
                {
                    AbilityId = $"ability_unlock_{level}",
                    AbilityName = $"Level {level} Ability",
                    UnlockLevel = level
                };

                Debug.Log($"[ProgressionSystem] Player {playerId} unlocked new ability at level {level}");
                OnAbilityUnlocked?.Invoke(playerId, unlockedAbility);
            }
        }

        public int GetLevel(ulong playerId)
        {
            return _playerLevels.TryGetValue(playerId, out int level) ? level : 1;
        }

        public int GetExperience(ulong playerId)
        {
            return _playerExperience.TryGetValue(playerId, out int xp) ? xp : 0;
        }

        public int GetExperienceToNextLevel(ulong playerId)
        {
            int currentLevel = GetLevel(playerId);
            if (currentLevel >= MAX_LEVEL) return 0;

            int xpRequired = GetExperienceRequiredForLevel(currentLevel + 1);
            int currentXP = GetExperience(playerId);
            return Mathf.Max(0, xpRequired - currentXP);
        }

        /// <summary>
        /// Get total XP required to reach a specific level.
        /// </summary>
        public int GetExperienceRequiredForLevel(int level)
        {
            if (level <= 1) return 0;
            if (level > MAX_LEVEL) return int.MaxValue;

            // XP formula: BaseXP * (ScalingFactor ^ (level - 1))
            return Mathf.RoundToInt(BASE_XP_REQUIREMENT * Mathf.Pow(XP_SCALING_FACTOR, level - 1));
        }

        public int CalculateExperienceReward(int playerLevel, int enemyLevel)
        {
            if (playerLevel <= 0 || enemyLevel <= 0) return 0;

            // Base XP based on enemy level
            int baseXP = enemyLevel * 10;

            // Level difference modifier
            int levelDiff = enemyLevel - playerLevel;

            if (levelDiff < -LEVEL_DIFFERENCE_THRESHOLD)
            {
                // Enemy is much lower level - reduced XP
                return Mathf.RoundToInt(baseXP * LOW_LEVEL_ENEMY_XP_PENALTY);
            }

            // Scale XP based on level difference
            float modifier = 1f + (levelDiff * 0.05f);
            modifier = Mathf.Clamp(modifier, 0.5f, 2f);

            return Mathf.RoundToInt(baseXP * modifier);
        }

        public CharacterStats GetBaseStats(int level, CharacterClass charClass)
        {
            // Base stats at level 1
            var baseStats = GetLevel1Stats(charClass);

            // Apply class-specific stat growth per level (Requirements 12.6, 12.7)
            var growth = GetStatGrowthPerLevel(charClass);
            int levelsGained = level - 1;

            return new CharacterStats
            {
                Health = baseStats.Health + (growth.Stamina * levelsGained * 10), // Stamina affects health
                MaxHealth = baseStats.MaxHealth + (growth.Stamina * levelsGained * 10),
                Mana = baseStats.Mana + (growth.Intellect * levelsGained * 5), // Intellect affects mana
                MaxMana = baseStats.MaxMana + (growth.Intellect * levelsGained * 5),
                Strength = baseStats.Strength + (growth.Strength * levelsGained),
                Intellect = baseStats.Intellect + (growth.Intellect * levelsGained),
                Stamina = baseStats.Stamina + (growth.Stamina * levelsGained),
                AttackPower = baseStats.AttackPower + (growth.AttackPower * levelsGained),
                SpellPower = baseStats.SpellPower + (growth.SpellPower * levelsGained),
                Armor = baseStats.Armor + (growth.Armor * levelsGained),
                CritChance = baseStats.CritChance + (growth.CritChance * levelsGained),
                Haste = baseStats.Haste + (growth.Haste * levelsGained),
                Mastery = baseStats.Mastery + (growth.Mastery * levelsGained)
            };
        }

        /// <summary>
        /// Get stat growth per level for a class.
        /// Requirements: 12.6, 12.7
        /// </summary>
        public CharacterStats GetStatGrowthPerLevel(CharacterClass charClass)
        {
            return charClass switch
            {
                // Tanks
                CharacterClass.Cruzado => new CharacterStats
                {
                    Strength = 2, Stamina = 3, Intellect = 1,
                    AttackPower = 2, SpellPower = 1, Armor = 6,
                    CritChance = 0.1f, Haste = 0f, Mastery = 0f
                },
                CharacterClass.Protector => new CharacterStats
                {
                    Strength = 2, Stamina = 3, Intellect = 0,
                    AttackPower = 2, SpellPower = 0, Armor = 7,
                    CritChance = 0.1f, Haste = 0f, Mastery = 0f
                },
                // DPS Fisico
                CharacterClass.Berserker => new CharacterStats
                {
                    Strength = 3, Stamina = 2, Intellect = 0,
                    AttackPower = 4, SpellPower = 0, Armor = 4,
                    CritChance = 0.15f, Haste = 0.1f, Mastery = 0f
                },
                CharacterClass.Arquero => new CharacterStats
                {
                    Strength = 0, Stamina = 1, Intellect = 0,
                    AttackPower = 4, SpellPower = 0, Armor = 2,
                    CritChance = 0.15f, Haste = 0.15f, Mastery = 0f
                },
                // DPS Magico
                CharacterClass.MaestroElemental => new CharacterStats
                {
                    Strength = 0, Stamina = 1, Intellect = 3,
                    AttackPower = 0, SpellPower = 5, Armor = 1,
                    CritChance = 0.15f, Haste = 0.1f, Mastery = 0f
                },
                CharacterClass.CaballeroRunico => new CharacterStats
                {
                    Strength = 2, Stamina = 2, Intellect = 2,
                    AttackPower = 2, SpellPower = 3, Armor = 4,
                    CritChance = 0.1f, Haste = 0.1f, Mastery = 0f
                },
                // Healers
                CharacterClass.Clerigo => new CharacterStats
                {
                    Strength = 0, Stamina = 1, Intellect = 3,
                    AttackPower = 0, SpellPower = 4, Armor = 1,
                    CritChance = 0.1f, Haste = 0.15f, Mastery = 0f
                },
                CharacterClass.MedicoBrujo => new CharacterStats
                {
                    Strength = 0, Stamina = 1, Intellect = 3,
                    AttackPower = 0, SpellPower = 4, Armor = 2,
                    CritChance = 0.1f, Haste = 0.1f, Mastery = 0f
                },
                _ => new CharacterStats
                {
                    Strength = 1, Stamina = 1, Intellect = 1,
                    AttackPower = 2, SpellPower = 2, Armor = 2,
                    CritChance = 0.1f, Haste = 0f, Mastery = 0f
                }
            };
        }

        private CharacterStats GetLevel1Stats(CharacterClass charClass)
        {
            return charClass switch
            {
                CharacterClass.Warrior => new CharacterStats
                {
                    Health = 150, MaxHealth = 150,
                    Mana = 0, MaxMana = 0,
                    Strength = 25, Intellect = 10, Stamina = 25,
                    AttackPower = 50, SpellPower = 0, Armor = 100
                },
                CharacterClass.Mage => new CharacterStats
                {
                    Health = 80, MaxHealth = 80,
                    Mana = 200, MaxMana = 200,
                    Strength = 10, Intellect = 30, Stamina = 15,
                    AttackPower = 10, SpellPower = 60, Armor = 20
                },
                CharacterClass.Priest => new CharacterStats
                {
                    Health = 90, MaxHealth = 90,
                    Mana = 180, MaxMana = 180,
                    Strength = 10, Intellect = 28, Stamina = 18,
                    AttackPower = 10, SpellPower = 55, Armor = 25
                },
                CharacterClass.Paladin => new CharacterStats
                {
                    Health = 120, MaxHealth = 120,
                    Mana = 100, MaxMana = 100,
                    Strength = 20, Intellect = 18, Stamina = 22,
                    AttackPower = 40, SpellPower = 35, Armor = 80
                },
                // Rogue: High Agility (represented as AttackPower), Medium Stamina, Low Strength/Intellect
                // Requirements 12.2: High Agility (15), Medium Stamina (10), Low Strength/Intellect
                // Growth: +2 Agility, +1 Stamina per level
                CharacterClass.Rogue => new CharacterStats
                {
                    Health = 90, MaxHealth = 90,
                    Mana = 0, MaxMana = 0, // Rogue uses Energy, not Mana
                    Strength = 8, Intellect = 5, Stamina = 10,
                    AttackPower = 45, SpellPower = 0, Armor = 40,
                    CritChance = 8f // Rogues have higher base crit
                },
                // Hunter: High Agility (14), Medium Stamina (11), Low Strength/Intellect
                // Requirements 12.3: High Agility (14), Medium Stamina (11), Low Strength/Intellect
                // Growth: +2 Agility, +1 Stamina per level
                CharacterClass.Hunter => new CharacterStats
                {
                    Health = 95, MaxHealth = 95,
                    Mana = 0, MaxMana = 0, // Hunter uses Focus, not Mana
                    Strength = 7, Intellect = 6, Stamina = 11,
                    AttackPower = 42, SpellPower = 0, Armor = 45,
                    CritChance = 6f // Hunters have moderate base crit
                },
                // Warlock: High Intellect (15), Medium Stamina (9), Low Strength/Agility
                // Requirements 12.4: High Intellect (15), Medium Stamina (9), Low Strength/Agility
                // Growth: +2 Intellect, +1 Stamina per level
                CharacterClass.Warlock => new CharacterStats
                {
                    Health = 85, MaxHealth = 85,
                    Mana = 200, MaxMana = 200, // Warlock uses Mana
                    Strength = 5, Intellect = 15, Stamina = 9,
                    AttackPower = 10, SpellPower = 55, Armor = 25,
                    CritChance = 5f // Warlocks have standard base crit
                },
                // Death Knight: High Strength (14), High Stamina (14), Medium Intellect (8)
                // Requirements 12.5: High Strength (14), High Stamina (14), Medium Intellect (8)
                // Growth: +2 Strength, +2 Stamina per level
                CharacterClass.DeathKnight => new CharacterStats
                {
                    Health = 110, MaxHealth = 110,
                    Mana = 100, MaxMana = 100, // Death Knight uses Mana (simplified from runes)
                    Strength = 14, Intellect = 8, Stamina = 14,
                    AttackPower = 48, SpellPower = 20, Armor = 90,
                    CritChance = 5f // Death Knights have standard base crit
                },
                _ => new CharacterStats
                {
                    Health = 100, MaxHealth = 100,
                    Mana = 100, MaxMana = 100,
                    Strength = 15, Intellect = 15, Stamina = 15,
                    AttackPower = 30, SpellPower = 30, Armor = 50
                }
            };
        }

        /// <summary>
        /// Get the progress percentage to next level (0-1).
        /// </summary>
        public float GetLevelProgress(ulong playerId)
        {
            int currentLevel = GetLevel(playerId);
            if (currentLevel >= MAX_LEVEL) return 1f;

            int currentXP = GetExperience(playerId);
            int xpRequired = GetExperienceRequiredForLevel(currentLevel + 1);

            if (xpRequired <= 0) return 1f;
            return Mathf.Clamp01((float)currentXP / xpRequired);
        }

        /// <summary>
        /// Set player level directly (for loading saved data).
        /// </summary>
        public void SetLevel(ulong playerId, int level, int experience)
        {
            _playerLevels[playerId] = Mathf.Clamp(level, 1, MAX_LEVEL);
            _playerExperience[playerId] = Mathf.Max(0, experience);
        }
    }
}
