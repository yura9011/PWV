using System;
using System.Collections.Generic;
using System.IO;
using EtherDomes.Data;
using UnityEngine;

namespace EtherDomes.Persistence
{
    /// <summary>
    /// Handles character data migrations between versions.
    /// NOTE: This service is being deprecated in favor of LocalDataManager which handles
    /// migration from legacy format automatically.
    /// </summary>
    [Obsolete("Use LocalDataManager for new character persistence and migration")]
    public class DataMigrationService : IDataMigrationService
    {
        private const string BACKUP_FOLDER = "Backups";
        private const int CURRENT_DATA_VERSION = 2; // Updated to match SaveFile.SAVE_VERSION

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
        }

        private void EnsureBackupDirectoryExists()
        {
            if (!Directory.Exists(_backupPath))
            {
                Directory.CreateDirectory(_backupPath);
                Debug.Log($"[DataMigrationService] Created backup directory: {_backupPath}");
            }
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
            // New CharacterData doesn't have DataVersion, migration is handled by LocalDataManager
            return false;
        }

        public CharacterData Migrate(CharacterData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            // Migration is now handled by LocalDataManager
            Debug.Log($"[DataMigrationService] Migration delegated to LocalDataManager for {data.CharacterId}");
            return data;
        }

        public string CreateBackup(CharacterData data)
        {
            if (data == null || string.IsNullOrEmpty(data.CharacterId))
                return null;

            try
            {
                string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                string safeId = SanitizeFileName(data.CharacterId);
                string backupFileName = $"{safeId}_{timestamp}.backup";
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
