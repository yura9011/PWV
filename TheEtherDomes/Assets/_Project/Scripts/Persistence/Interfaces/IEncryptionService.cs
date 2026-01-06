namespace EtherDomes.Persistence
{
    /// <summary>
    /// Interface for encryption and hashing services.
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Encrypt data using AES encryption.
        /// </summary>
        /// <param name="data">Plain data to encrypt</param>
        /// <returns>Encrypted data</returns>
        byte[] Encrypt(byte[] data);

        /// <summary>
        /// Decrypt data using AES encryption.
        /// </summary>
        /// <param name="encryptedData">Encrypted data to decrypt</param>
        /// <returns>Decrypted data, or null if decryption fails</returns>
        byte[] Decrypt(byte[] encryptedData);

        /// <summary>
        /// Compute SHA256 hash of data.
        /// </summary>
        /// <param name="data">Data to hash</param>
        /// <returns>SHA256 hash</returns>
        byte[] ComputeHash(byte[] data);

        /// <summary>
        /// Verify that data matches a hash.
        /// </summary>
        /// <param name="data">Data to verify</param>
        /// <param name="hash">Expected hash</param>
        /// <returns>True if hash matches</returns>
        bool VerifyHash(byte[] data, byte[] hash);
    }
}
