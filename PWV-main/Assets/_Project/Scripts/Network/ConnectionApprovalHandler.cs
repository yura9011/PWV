using System;
using EtherDomes.Data;
using EtherDomes.Persistence;
using UnityEngine;

namespace EtherDomes.Network
{
    /// <summary>
    /// Handles connection approval by validating character data integrity.
    /// Implements basic anti-cheat by checking stat ranges.
    /// NOTE: Consider using ConnectionApprovalManager instead which has more complete validation.
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

                Debug.Log($"[ConnectionApproval] Approved: {character.Name} (Level {character.Level})");
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

            if (string.IsNullOrEmpty(character.Name))
            {
                error = "Missing character name";
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
            if (character.CurrentXP < 0)
            {
                error = "Invalid experience: negative value";
                return false;
            }

            // Validate total stats
            if (!IsStatInRange((int)character.MaxHP, "MaxHealth", out error)) return false;
            if (!IsStatInRange((int)character.MaxMana, "MaxMana", out error)) return false;
            if (!IsStatInRange(character.TotalStrength, "Strength", out error)) return false;
            if (!IsStatInRange(character.TotalIntellect, "Intellect", out error)) return false;
            if (!IsStatInRange(character.TotalStamina, "Stamina", out error)) return false;
            if (!IsStatInRange(character.TotalAttackPower, "AttackPower", out error)) return false;
            if (!IsStatInRange(character.TotalSpellPower, "SpellPower", out error)) return false;
            if (!IsStatInRange(character.TotalArmor, "Armor", out error)) return false;

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

            if (character.EquippedItemIDs == null)
                return true; // Empty equipment is valid

            // Basic validation - just check that equipped items list is reasonable
            if (character.EquippedItemIDs.Count > 20)
            {
                error = "Too many equipped items";
                return false;
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
