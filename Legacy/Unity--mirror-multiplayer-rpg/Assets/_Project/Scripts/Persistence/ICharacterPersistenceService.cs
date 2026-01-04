using System.Threading.Tasks;

namespace EtherDomes.Persistence
{
    /// <summary>
    /// Interface for character persistence with encryption support.
    /// Handles local save/load and Cross-World network transfer.
    /// </summary>
    public interface ICharacterPersistenceService
    {
        /// <summary>
        /// Saves character data to local encrypted storage.
        /// </summary>
        /// <param name="data">Character data to save</param>
        /// <returns>True if save was successful</returns>
        Task<bool> SaveCharacterAsync(CharacterData data);

        /// <summary>
        /// Loads character data from local encrypted storage.
        /// </summary>
        /// <param name="characterId">Unique character identifier</param>
        /// <returns>Character data or null if not found/corrupted</returns>
        Task<CharacterData> LoadCharacterAsync(string characterId);

        /// <summary>
        /// Validates character data integrity using stored hash.
        /// </summary>
        /// <param name="data">Character data to validate</param>
        /// <returns>True if data integrity is valid</returns>
        bool ValidateCharacterIntegrity(CharacterData data);

        /// <summary>
        /// Exports character data for network transfer (encrypted).
        /// Used for Cross-World functionality.
        /// </summary>
        /// <param name="data">Character data to export</param>
        /// <returns>Encrypted byte array for network transfer</returns>
        byte[] ExportCharacterForNetwork(CharacterData data);

        /// <summary>
        /// Imports character data from network transfer (decrypted).
        /// Used for Cross-World functionality.
        /// </summary>
        /// <param name="payload">Encrypted byte array from network</param>
        /// <returns>Decrypted character data or null if invalid</returns>
        CharacterData ImportCharacterFromNetwork(byte[] payload);

        /// <summary>
        /// Deletes a character from local storage.
        /// </summary>
        /// <param name="characterId">Unique character identifier</param>
        /// <returns>True if deletion was successful</returns>
        Task<bool> DeleteCharacterAsync(string characterId);

        /// <summary>
        /// Gets all saved character IDs.
        /// </summary>
        /// <returns>Array of character IDs</returns>
        Task<string[]> GetAllCharacterIdsAsync();
    }
}
