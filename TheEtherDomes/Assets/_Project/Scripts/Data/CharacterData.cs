using System;
using System.Collections.Generic;

namespace EtherDomes.Data
{
    /// <summary>
    /// Complete character data for persistence and network transfer.
    /// </summary>
    [Serializable]
    public class CharacterData
    {
        // Core identity
        public string CharacterId;
        public string CharacterName;
        public CharacterClass Class;
        public Specialization CurrentSpec;
        public int Level;
        public int Experience;
        public CharacterStats BaseStats;
        public EquipmentData Equipment;
        public string[] UnlockedAbilityIds;
        public DateTime LastSaveTime;
        public byte[] IntegrityHash;

        // NEW: Version for data migration (Requirement 11.1)
        public int DataVersion;

        // NEW: Mana system (Requirement 11.6)
        public float CurrentMana;
        public float MaxMana;

        // NEW: Secondary resource for class-specific mechanics (Requirement 11.2)
        public float SecondaryResource;
        public SecondaryResourceType ResourceType;

        // NEW: Bad Luck Protection tracking (Requirement 11.3)
        public Dictionary<ItemRarity, int> LootAttempts;

        // NEW: Weekly lockout tracking (Requirement 11.4)
        public List<string> LockedBossIds;
        public DateTime LockoutResetTime;

        // NEW: Inventory separate from Equipment (Requirement 11.5)
        public List<ItemData> Inventory;

        public CharacterData()
        {
            CharacterId = Guid.NewGuid().ToString();
            CharacterName = "New Character";
            Class = CharacterClass.Warrior;
            CurrentSpec = Specialization.Arms;
            Level = 1;
            Experience = 0;
            BaseStats = new CharacterStats();
            Equipment = new EquipmentData();
            UnlockedAbilityIds = new string[0];
            LastSaveTime = DateTime.UtcNow;

            // Initialize new fields with sensible defaults (Requirement 11.7)
            DataVersion = 1;
            CurrentMana = 100f;
            MaxMana = 100f;
            SecondaryResource = 0f;
            ResourceType = SecondaryResourceType.None;
            LootAttempts = new Dictionary<ItemRarity, int>();
            LockedBossIds = new List<string>();
            LockoutResetTime = GetNextWeeklyReset();
            Inventory = new List<ItemData>(30); // 30 slot capacity
        }

        /// <summary>
        /// Gets the next Monday 00:00 UTC for weekly reset.
        /// </summary>
        private static DateTime GetNextWeeklyReset()
        {
            var now = DateTime.UtcNow;
            int daysUntilMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
            if (daysUntilMonday == 0 && now.TimeOfDay > TimeSpan.Zero)
                daysUntilMonday = 7;
            return now.Date.AddDays(daysUntilMonday);
        }

        public CharacterStats GetTotalStats()
        {
            var total = BaseStats.Clone();
            if (Equipment != null)
            {
                total.Add(Equipment.GetTotalStats());
            }
            return total;
        }

        public bool IsValid()
        {
            if (string.IsNullOrEmpty(CharacterId)) return false;
            if (string.IsNullOrEmpty(CharacterName)) return false;
            if (Level < 1 || Level > 60) return false;
            if (Experience < 0) return false;
            if (BaseStats == null) return false;
            return true;
        }

        /// <summary>
        /// Checks if weekly lockout has reset and clears locked bosses if needed.
        /// </summary>
        public void CheckWeeklyReset()
        {
            if (DateTime.UtcNow >= LockoutResetTime)
            {
                LockedBossIds.Clear();
                LockoutResetTime = GetNextWeeklyReset();
            }
        }

        /// <summary>
        /// Records a loot attempt for Bad Luck Protection.
        /// </summary>
        public void RecordLootAttempt(ItemRarity rarity, bool success)
        {
            if (success)
            {
                LootAttempts[rarity] = 0;
            }
            else
            {
                if (!LootAttempts.ContainsKey(rarity))
                    LootAttempts[rarity] = 0;
                LootAttempts[rarity]++;
            }
        }

        /// <summary>
        /// Gets the Bad Luck Protection bonus for a rarity (5% per attempt after 10).
        /// </summary>
        public float GetBadLuckProtectionBonus(ItemRarity rarity)
        {
            if (!LootAttempts.TryGetValue(rarity, out int attempts))
                return 0f;
            
            if (attempts <= 10)
                return 0f;
            
            return (attempts - 10) * 0.05f;
        }
    }

    /// <summary>
    /// Payload sent during connection for character validation.
    /// </summary>
    [Serializable]
    public class ConnectionPayload
    {
        public byte[] EncryptedCharacterData;
        public string ClientVersion;
        public byte[] ClientSignature;
    }

    /// <summary>
    /// Result of connection approval validation.
    /// </summary>
    public struct ConnectionApprovalResult
    {
        public bool Approved;
        public string RejectionReason;
        public ApprovalErrorCode ErrorCode;

        public static ConnectionApprovalResult Success()
        {
            return new ConnectionApprovalResult
            {
                Approved = true,
                RejectionReason = null,
                ErrorCode = ApprovalErrorCode.None
            };
        }

        public static ConnectionApprovalResult Failure(ApprovalErrorCode code, string reason)
        {
            return new ConnectionApprovalResult
            {
                Approved = false,
                RejectionReason = reason,
                ErrorCode = code
            };
        }
    }
}
