using System.Threading.Tasks;
using EtherDomes.Data;

namespace EtherDomes.Persistence
{
    /// <summary>
    /// Interface for character data persistence with encryption.
    /// Supports Cross-World functionality (characters portable between servers).
    /// </summary>
    public interface ICharacterPersistenceService
    {
        /// <summary>
        /// Save character data to local storage with encryption.
        /// </summary>
        /// <param name="data">Character data to save</param>
        /// <returns>True if save was successful</returns>
        Task<bool> SaveCharacterAsync(CharacterData data);

        /// <summary>
        /// Load character data from local storage with decryption.
        /// </summary>
        /// <param name="characterId">ID of the character to load</param>
        /// <returns>Character data, or null if not found or corrupted</returns>
        Task<CharacterData> LoadCharacterAsync(string characterId);

        /// <summary>
        /// Validate character data integrity.
        /// </summary>
        /// <param name="data">Character data to validate</param>
        /// <returns>True if data is valid and not tampered</returns>
        bool ValidateCharacterIntegrity(CharacterData data);

        /// <summary>
        /// Export character data for network transfer (encrypted).
        /// Used when connecting to a server.
        /// </summary>
        /// <param name="data">Character data to export</param>
        /// <returns>Encrypted byte array for network transfer</returns>
        byte[] ExportCharacterForNetwork(CharacterData data);

        /// <summary>
        /// Import character data from network transfer (decrypted).
        /// Used by server to validate connecting player.
        /// </summary>
        /// <param name="payload">Encrypted byte array from network</param>
        /// <returns>Character data, or null if invalid</returns>
        CharacterData ImportCharacterFromNetwork(byte[] payload);

        /// <summary>
        /// Get list of all saved character IDs.
        /// </summary>
        /// <returns>Array of character IDs</returns>
        string[] GetSavedCharacterIds();

        /// <summary>
        /// Delete a saved character.
        /// </summary>
        /// <param name="characterId">ID of character to delete</param>
        /// <returns>True if deletion was successful</returns>
        bool DeleteCharacter(string characterId);
    }
}
