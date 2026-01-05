using System;
using System.Collections.Generic;
using System.IO;
using EtherDomes.Data;
using UnityEngine;

namespace EtherDomes.Persistence
{
    /// <summary>
    /// Handles character data migrations between versions.
    /// Maintains a chain of migrators and executes them sequentially.
    /// Creates backups before migration to prevent data loss.
    /// </summary>
    public class DataMigrationService : IDataMigrationService
    {
        private const string BACKUP_FOLDER = "Backups";
        private const int CURRENT_DATA_VERSION = 1;

        private readonly string _backupPath;
        private readonly Dictionary<int, Func<CharacterData, CharacterData>> _migrators;

        public event Action<string, int, int> OnMigrationStarted;
        public event Action<string, int> OnMigrationCompleted;
        public event Action<string, int, string> OnMigrationFailed;

        public int CurrentVersion => CURRENT_DATA_VERSION;

        public DataMigrationService()
        {
            _backupPath = Path.Combine(Application.persistentDataPath, BACKUP_FOLDER);
            _migrators = new Dictionary<int, Func<CharacterData, CharacterData>>();
            
            EnsureBackupDirectoryExists();
            RegisterDefaultMigrations();
        }

        private void EnsureBackupDirectoryExists()
        {
            if (!Directory.Exists(_backupPath))
            {
                Directory.CreateDirectory(_backupPath);
                Debug.Log($"[DataMigrationService] Created backup directory: {_backupPath}");
            }
        }

        private void RegisterDefaultMigrations()
        {
            // Register migration from version 0 (legacy) to version 1
            RegisterMigration(0, 1, MigrateV0ToV1);
        }


        /// <summary>
        /// Migration from version 0 (legacy data without version) to version 1.
        /// Initializes all new fields with sensible defaults.
        /// </summary>
        private CharacterData MigrateV0ToV1(CharacterData data)
        {
            Debug.Log($"[DataMigrationService] Migrating {data.CharacterId} from v0 to v1");
            
            // Initialize new fields that didn't exist in v0
            data.DataVersion = 1;
            data.CurrentMana = data.MaxMana > 0 ? data.MaxMana : 100f;
            data.MaxMana = data.MaxMana > 0 ? data.MaxMana : 100f;
            data.SecondaryResource = 0f;
            data.ResourceType = SecondaryResourceType.None;
            
            if (data.LootAttempts == null)
                data.LootAttempts = new Dictionary<ItemRarity, int>();
            
            if (data.LockedBossIds == null)
                data.LockedBossIds = new List<string>();
            
            if (data.Inventory == null)
                data.Inventory = new List<ItemData>(30);
            
            // Set lockout reset time if not set
            if (data.LockoutResetTime == default)
                data.LockoutResetTime = GetNextWeeklyReset();
            
            return data;
        }

        private static DateTime GetNextWeeklyReset()
        {
            var now = DateTime.UtcNow;
            int daysUntilMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
            if (daysUntilMonday == 0 && now.TimeOfDay > TimeSpan.Zero)
                daysUntilMonday = 7;
            return now.Date.AddDays(daysUntilMonday);
        }

        public void RegisterMigration(int fromVersion, int toVersion, Func<CharacterData, CharacterData> migrator)
        {
            if (migrator == null)
                throw new ArgumentNullException(nameof(migrator));
            
            _migrators[fromVersion] = migrator;
            Debug.Log($"[DataMigrationService] Registered migration: v{fromVersion} -> v{toVersion}");
        }

        public bool NeedsMigration(CharacterData data)
        {
            if (data == null)
                return false;
            
            return data.DataVersion < CurrentVersion;
        }

        public CharacterData Migrate(CharacterData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            // If already at current version, return as-is (idempotent)
            if (data.DataVersion >= CurrentVersion)
            {
                Debug.Log($"[DataMigrationService] No migration needed for {data.CharacterId} (v{data.DataVersion})");
                return data;
            }

            int startVersion = data.DataVersion;
            OnMigrationStarted?.Invoke(data.CharacterId, startVersion, CurrentVersion);

            try
            {
                // Create backup before migration
                string backupPath = CreateBackup(data);
                if (string.IsNullOrEmpty(backupPath))
                {
                    Debug.LogWarning($"[DataMigrationService] Backup failed, proceeding with migration anyway");
                }

                // Execute migration chain
                while (data.DataVersion < CurrentVersion)
                {
                    int currentVersion = data.DataVersion;
                    
                    if (!_migrators.TryGetValue(currentVersion, out var migrator))
                    {
                        throw new InvalidOperationException(
                            $"No migrator registered for version {currentVersion}");
                    }

                    data = migrator(data);
                    
                    // Ensure version was incremented
                    if (data.DataVersion <= currentVersion)
                    {
                        data.DataVersion = currentVersion + 1;
                    }
                    
                    Debug.Log($"[DataMigrationService] Migrated {data.CharacterId}: v{currentVersion} -> v{data.DataVersion}");
                }

                OnMigrationCompleted?.Invoke(data.CharacterId, data.DataVersion);
                Debug.Log($"[DataMigrationService] Migration complete for {data.CharacterId}: v{startVersion} -> v{data.DataVersion}");
                
                return data;
            }
            catch (Exception ex)
            {
                OnMigrationFailed?.Invoke(data.CharacterId, data.DataVersion, ex.Message);
                Debug.LogError($"[DataMigrationService] Migration failed for {data.CharacterId}: {ex.Message}");
                throw;
            }
        }

        public string CreateBackup(CharacterData data)
        {
            if (data == null || string.IsNullOrEmpty(data.CharacterId))
                return null;

            try
            {
                string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                string safeId = SanitizeFileName(data.CharacterId);
                string backupFileName = $"{safeId}_v{data.DataVersion}_{timestamp}.backup";
                string backupFilePath = Path.Combine(_backupPath, backupFileName);

                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(backupFilePath, json);

                Debug.Log($"[DataMigrationService] Created backup: {backupFilePath}");
                return backupFilePath;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DataMigrationService] Backup failed: {ex.Message}");
                return null;
            }
        }

        private string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
