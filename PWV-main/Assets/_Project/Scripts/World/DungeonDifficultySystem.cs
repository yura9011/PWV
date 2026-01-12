using System;
using System.Collections.Generic;
using UnityEngine;

namespace EtherDomes.World
{
    /// <summary>
    /// Manages dungeon difficulty settings and modifiers.
    /// 
    /// Difficulty Levels:
    /// - Normal: 1.0x HP/DMG, 0 extra mechanics
    /// - Heroic: 1.5x HP, 1.3x DMG, 1 extra mechanic
    /// - Mythic: 2.0x HP, 1.6x DMG, 2 extra mechanics, exclusive loot
    /// 
    /// Requirements: 5.1, 5.2, 5.3
    /// </summary>
    public class DungeonDifficultySystem : IDungeonDifficultySystem
    {
        // Difficulty modifiers
        private static readonly Dictionary<DungeonDifficulty, DifficultyModifiers> DifficultySettings = new()
        {
            {
                DungeonDifficulty.Normal,
                new DifficultyModifiers
                {
                    HealthMultiplier = 1.0f,
                    DamageMultiplier = 1.0f,
                    AdditionalMechanics = 0,
                    HasExclusiveLoot = false
                }
            },
            {
                DungeonDifficulty.Heroic,
                new DifficultyModifiers
                {
                    HealthMultiplier = 1.5f,
                    DamageMultiplier = 1.3f,
                    AdditionalMechanics = 1,
                    HasExclusiveLoot = false
                }
            },
            {
                DungeonDifficulty.Mythic,
                new DifficultyModifiers
                {
                    HealthMultiplier = 2.0f,
                    DamageMultiplier = 1.6f,
                    AdditionalMechanics = 2,
                    HasExclusiveLoot = true
                }
            }
        };

        // Additional mechanics by difficulty
        private static readonly Dictionary<DungeonDifficulty, List<string>> AdditionalMechanicsMap = new()
        {
            { DungeonDifficulty.Normal, new List<string>() },
            { DungeonDifficulty.Heroic, new List<string> { "Enrage Timer" } },
            { DungeonDifficulty.Mythic, new List<string> { "Enrage Timer", "Mythic Phase" } }
        };

        // Exclusive loot for Mythic
        private static readonly List<string> MythicExclusiveLoot = new()
        {
            "Mythic Mount",
            "Mythic Title",
            "Mythic Transmog Set"
        };

        private DungeonDifficulty _currentDifficulty = DungeonDifficulty.Normal;

        public DungeonDifficulty CurrentDifficulty => _currentDifficulty;

        public event Action<DungeonDifficulty> OnDifficultyChanged;

        public void SetDifficulty(DungeonDifficulty difficulty)
        {
            if (_currentDifficulty == difficulty)
                return;

            var previousDifficulty = _currentDifficulty;
            _currentDifficulty = difficulty;

            Debug.Log($"[DungeonDifficulty] Changed from {previousDifficulty} to {difficulty}");
            OnDifficultyChanged?.Invoke(difficulty);
        }

        public DifficultyModifiers GetModifiers()
        {
            return GetModifiers(_currentDifficulty);
        }

        public DifficultyModifiers GetModifiers(DungeonDifficulty difficulty)
        {
            if (DifficultySettings.TryGetValue(difficulty, out var modifiers))
            {
                return modifiers;
            }

            // Default to Normal if not found
            return DifficultySettings[DungeonDifficulty.Normal];
        }

        public List<string> GetExclusiveLoot()
        {
            var modifiers = GetModifiers();
            if (modifiers.HasExclusiveLoot)
            {
                return new List<string>(MythicExclusiveLoot);
            }
            return null;
        }

        public List<string> GetAdditionalMechanics()
        {
            if (AdditionalMechanicsMap.TryGetValue(_currentDifficulty, out var mechanics))
            {
                return new List<string>(mechanics);
            }
            return new List<string>();
        }

        /// <summary>
        /// Applies difficulty modifiers to enemy stats.
        /// </summary>
        public void ApplyModifiersToEnemy(ref float health, ref float damage)
        {
            var modifiers = GetModifiers();
            health *= modifiers.HealthMultiplier;
            damage *= modifiers.DamageMultiplier;
        }

        /// <summary>
        /// Gets the display name for a difficulty.
        /// </summary>
        public static string GetDifficultyName(DungeonDifficulty difficulty)
        {
            return difficulty switch
            {
                DungeonDifficulty.Normal => "Normal",
                DungeonDifficulty.Heroic => "Heroic",
                DungeonDifficulty.Mythic => "Mythic",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Gets the color for a difficulty (for UI).
        /// </summary>
        public static Color GetDifficultyColor(DungeonDifficulty difficulty)
        {
            return difficulty switch
            {
                DungeonDifficulty.Normal => Color.white,
                DungeonDifficulty.Heroic => Color.blue,
                DungeonDifficulty.Mythic => new Color(0.6f, 0.2f, 0.8f), // Purple
                _ => Color.white
            };
        }
    }
}
