using System;
using EtherDomes.Data;

namespace EtherDomes.Persistence
{
    /// <summary>
    /// Interface for managing character data migrations between versions.
    /// Handles automatic migration of saved character data when game structure changes.
    /// </summary>
    public interface IDataMigrationService
    {
        /// <summary>
        /// The current data version that new characters are created with.
        /// </summary>
        int CurrentVersion { get; }

        /// <summary>
        /// Migrates character data from its current version to the latest version.
        /// </summary>
        /// <param name="data">The character data to migrate.</param>
        /// <returns>The migrated character data (may be the same instance if no migration needed).</returns>
        CharacterData Migrate(CharacterData data);

        /// <summary>
        /// Checks if the character data needs migration.
        /// </summary>
        /// <param name="data">The character data to check.</param>
        /// <returns>True if migration is needed, false otherwise.</returns>
        bool NeedsMigration(CharacterData data);

        /// <summary>
        /// Registers a migration function for a specific version transition.
        /// </summary>
        /// <param name="fromVersion">The source version.</param>
        /// <param name="toVersion">The target version.</param>
        /// <param name="migrator">The function that performs the migration.</param>
        void RegisterMigration(int fromVersion, int toVersion, Func<CharacterData, CharacterData> migrator);

        /// <summary>
        /// Creates a backup of the character data before migration.
        /// </summary>
        /// <param name="data">The character data to backup.</param>
        /// <returns>The backup file path, or null if backup failed.</returns>
        string CreateBackup(CharacterData data);

        /// <summary>
        /// Event fired when a migration starts.
        /// </summary>
        event Action<string, int, int> OnMigrationStarted; // characterId, fromVersion, toVersion

        /// <summary>
        /// Event fired when a migration completes successfully.
        /// </summary>
        event Action<string, int> OnMigrationCompleted; // characterId, newVersion

        /// <summary>
        /// Event fired when a migration fails.
        /// </summary>
        event Action<string, int, string> OnMigrationFailed; // characterId, version, errorMessage
    }
}
