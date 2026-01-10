using System;
using EtherDomes.Data;

namespace EtherDomes.Network
{
    /// <summary>
    /// Interface for handling connection approval and character validation.
    /// </summary>
    public interface IConnectionApprovalHandler
    {
        /// <summary>
        /// Validate a connection request with the provided payload.
        /// </summary>
        /// <param name="payload">Encrypted character data from the connecting client</param>
        /// <returns>Result indicating approval or rejection with reason</returns>
        ConnectionApprovalResult ValidateConnectionRequest(byte[] payload);

        /// <summary>
        /// Timeout for validation operations.
        /// </summary>
        TimeSpan ValidationTimeout { get; set; }

        /// <summary>
        /// Maximum allowed stat value for validation.
        /// </summary>
        int MaxStatValue { get; }

        /// <summary>
        /// Minimum allowed stat value for validation.
        /// </summary>
        int MinStatValue { get; }
    }
}
