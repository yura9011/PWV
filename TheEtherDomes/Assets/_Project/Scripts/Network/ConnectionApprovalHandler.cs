using System;
using EtherDomes.Data;
using EtherDomes.Persistence;
using UnityEngine;

namespace EtherDomes.Network
{
    /// <summary>
    /// Handles connection approval by validating character data integrity.
    /// Implements basic anti-cheat by checking stat ranges.
    /// </summary>
    public class ConnectionApprovalHandler : IConnectionApprovalHandler
    {
        public const int DEFAULT_MAX_STAT = 10000;
        public const int DEFAULT_MIN_STAT = 0;
        public const int DEFAULT_MAX_LEVEL = 60;
        public const int DEFAULT_MIN_LEVEL = 1;

        private readonly ICharacterPersistenceService _persistenceService;
        
        public TimeSpan ValidationTimeout { get; set; } = TimeSpan.FromSeconds(5);
        public int MaxStatValue { get; private set; } = DEFAULT_MAX_STAT;
        public int MinStatValue { get; private set; } = DEFAULT_MIN_STAT;

        public ConnectionApprovalHandler() : this(new CharacterPersistenceService())
        {
        }

        public ConnectionApprovalHandler(ICharacterPersistenceService persistenceService)
        {
            _persistenceService = persistenceService ?? throw new ArgumentNullException(nameof(persistenceService));
        }

        public ConnectionApprovalResult ValidateConnectionRequest(byte[] payload)
        {
            if (payload == null || payload.Length == 0)
            {
                return ConnectionApprovalResult.Failure(
                    ApprovalErrorCode.InvalidDataFormat,
                    "Empty payload received");
            }

            try
            {
                // Import and decrypt character data
                CharacterData character = _persistenceService.ImportCharacterFromNetwork(payload);
                
                if (character == null)
                {
                    return ConnectionApprovalResult.Failure(
                        ApprovalErrorCode.CorruptedData,
                        "Failed to decrypt character data");
                }

                // Validate character structure
                if (!ValidateCharacterStructure(character, out string structureError))
                {
                    return ConnectionApprovalResult.Failure(
                        ApprovalErrorCode.InvalidDataFormat,
                        structureError);
                }

                // Validate stats are within valid ranges
                if (!ValidateStats(character, out string statsError))
                {
                    return ConnectionApprovalResult.Failure(
                        ApprovalErrorCode.StatsOutOfRange,
                        statsError);
                }

                // Validate equipment
                if (!ValidateEquipment(character, out string equipError))
                {
                    return ConnectionApprovalResult.Failure(
                        ApprovalErrorCode.StatsOutOfRange,
                        equipError);
                }

                Debug.Log($"[ConnectionApproval] Approved: {character.CharacterName} (Level {character.Level})");
                return ConnectionApprovalResult.Success();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ConnectionApproval] Validation exception: {ex.Message}");
                return ConnectionApprovalResult.Failure(
                    ApprovalErrorCode.CorruptedData,
                    "Validation error occurred");
            }
        }


        private bool ValidateCharacterStructure(CharacterData character, out string error)
        {
            error = null;

            if (string.IsNullOrEmpty(character.CharacterId))
            {
                error = "Missing character ID";
                return false;
            }

            if (string.IsNullOrEmpty(character.CharacterName))
            {
                error = "Missing character name";
                return false;
            }

            if (character.BaseStats == null)
            {
                error = "Missing base stats";
                return false;
            }

            if (character.Equipment == null)
            {
                error = "Missing equipment data";
                return false;
            }

            return true;
        }

        private bool ValidateStats(CharacterData character, out string error)
        {
            error = null;

            // Validate level
            if (character.Level < DEFAULT_MIN_LEVEL || character.Level > DEFAULT_MAX_LEVEL)
            {
                error = $"Invalid level: {character.Level} (must be {DEFAULT_MIN_LEVEL}-{DEFAULT_MAX_LEVEL})";
                return false;
            }

            // Validate experience
            if (character.Experience < 0)
            {
                error = "Invalid experience: negative value";
                return false;
            }

            // Validate base stats
            var stats = character.BaseStats;
            
            if (!IsStatInRange(stats.MaxHealth, "MaxHealth", out error)) return false;
            if (!IsStatInRange(stats.MaxMana, "MaxMana", out error)) return false;
            if (!IsStatInRange(stats.Strength, "Strength", out error)) return false;
            if (!IsStatInRange(stats.Intellect, "Intellect", out error)) return false;
            if (!IsStatInRange(stats.Stamina, "Stamina", out error)) return false;
            if (!IsStatInRange(stats.AttackPower, "AttackPower", out error)) return false;
            if (!IsStatInRange(stats.SpellPower, "SpellPower", out error)) return false;
            if (!IsStatInRange(stats.Armor, "Armor", out error)) return false;

            return true;
        }

        private bool IsStatInRange(int value, string statName, out string error)
        {
            error = null;
            if (value < MinStatValue || value > MaxStatValue)
            {
                error = $"Invalid {statName}: {value} (must be {MinStatValue}-{MaxStatValue})";
                return false;
            }
            return true;
        }

        private bool ValidateEquipment(CharacterData character, out string error)
        {
            error = null;

            if (character.Equipment?.EquippedItems == null)
                return true; // Empty equipment is valid

            foreach (var kvp in character.Equipment.EquippedItems)
            {
                var item = kvp.Value;
                if (item == null) continue;

                // Validate item level
                if (item.ItemLevel < 0 || item.ItemLevel > MaxStatValue)
                {
                    error = $"Invalid item level on {item.ItemName}: {item.ItemLevel}";
                    return false;
                }

                // Validate required level doesn't exceed character level
                if (item.RequiredLevel > character.Level)
                {
                    error = $"Item {item.ItemName} requires level {item.RequiredLevel}, character is {character.Level}";
                    return false;
                }

                // Validate item stats
                if (item.Stats != null)
                {
                    foreach (var stat in item.Stats)
                    {
                        if (!IsStatInRange(stat.Value, $"Item {stat.Key}", out error)) return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Configure validation ranges.
        /// </summary>
        public void SetStatRanges(int minStat, int maxStat)
        {
            MinStatValue = minStat;
            MaxStatValue = maxStat;
        }
    }
}
