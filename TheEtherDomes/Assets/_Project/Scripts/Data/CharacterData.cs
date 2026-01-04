using System;

namespace EtherDomes.Data
{
    /// <summary>
    /// Complete character data for persistence and network transfer.
    /// </summary>
    [Serializable]
    public class CharacterData
    {
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
