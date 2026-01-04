using System;
using System.Collections;
using Mirror;
using UnityEngine;
using EtherDomes.Persistence;

namespace EtherDomes.Network
{
    /// <summary>
    /// Mirror Authenticator that validates character data integrity on connection.
    /// Implements anti-cheat validation for equipment stats.
    /// </summary>
    public class ConnectionApprovalAuthenticator : NetworkAuthenticator
    {
        [Header("Validation Settings")]
        [SerializeField] private float _validationTimeout = 10f;
        [SerializeField] private int _maxStatValue = 9999;
        [SerializeField] private int _maxLevel = 100;
        
        [Header("Debug")]
        [SerializeField] private bool _skipAuthenticationForTesting = true;

        private ICharacterPersistenceService _persistenceService;

        public TimeSpan ValidationTimeout
        {
            get => TimeSpan.FromSeconds(_validationTimeout);
            set => _validationTimeout = (float)value.TotalSeconds;
        }

        public int MaxStatValue
        {
            get => _maxStatValue;
            set => _maxStatValue = value;
        }

        private void Awake()
        {
            _persistenceService = new CharacterPersistenceService();
        }

        /// <summary>
        /// Sets a custom persistence service (for testing).
        /// </summary>
        public void SetPersistenceService(ICharacterPersistenceService service)
        {
            _persistenceService = service;
        }

        #region Server Authentication

        public override void OnStartServer()
        {
            NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage, false);
        }

        public override void OnServerAuthenticate(NetworkConnectionToClient conn)
        {
            // Skip authentication for testing
            if (_skipAuthenticationForTesting)
            {
                Debug.Log($"[ConnectionApproval] Skipping authentication for {conn.connectionId} (testing mode)");
                ServerAccept(conn);
                return;
            }
            
            // Start timeout coroutine
            StartCoroutine(AuthenticationTimeoutCoroutine(conn));
        }

        private void OnAuthRequestMessage(NetworkConnectionToClient conn, AuthRequestMessage msg)
        {
            var result = ValidateConnectionRequest(conn, msg);

            if (result.Approved)
            {
                Debug.Log($"[ConnectionApproval] Connection approved for {conn.connectionId}");
                ServerAccept(conn);
            }
            else
            {
                Debug.LogWarning($"[ConnectionApproval] Connection rejected: {result.RejectionReason}");
                
                // Send rejection message to client
                conn.Send(new AuthResponseMessage
                {
                    Success = false,
                    Message = result.RejectionReason,
                    ErrorCode = result.ErrorCode
                });

                StartCoroutine(DelayedReject(conn));
            }
        }

        private IEnumerator DelayedReject(NetworkConnectionToClient conn)
        {
            yield return new WaitForSeconds(0.5f);
            ServerReject(conn);
        }

        private IEnumerator AuthenticationTimeoutCoroutine(NetworkConnectionToClient conn)
        {
            yield return new WaitForSeconds(_validationTimeout);

            if (!conn.isAuthenticated)
            {
                Debug.LogWarning($"[ConnectionApproval] Authentication timeout for {conn.connectionId}");
                
                conn.Send(new AuthResponseMessage
                {
                    Success = false,
                    Message = "Authentication timeout",
                    ErrorCode = ApprovalErrorCode.ValidationTimeout
                });

                ServerReject(conn);
            }
        }

        /// <summary>
        /// Validates the connection request and character data.
        /// </summary>
        public ConnectionApprovalResult ValidateConnectionRequest(NetworkConnectionToClient conn, AuthRequestMessage msg)
        {
            // Check for empty payload
            if (msg.EncryptedCharacterData == null || msg.EncryptedCharacterData.Length == 0)
            {
                return new ConnectionApprovalResult
                {
                    Approved = false,
                    RejectionReason = "Empty character data",
                    ErrorCode = ApprovalErrorCode.InvalidDataFormat
                };
            }

            // Try to decrypt and deserialize
            CharacterData characterData;
            try
            {
                characterData = _persistenceService.ImportCharacterFromNetwork(msg.EncryptedCharacterData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ConnectionApproval] Decryption failed: {ex.Message}");
                return new ConnectionApprovalResult
                {
                    Approved = false,
                    RejectionReason = "Failed to decrypt character data",
                    ErrorCode = ApprovalErrorCode.EncryptionFailure
                };
            }

            if (characterData == null)
            {
                return new ConnectionApprovalResult
                {
                    Approved = false,
                    RejectionReason = "Invalid or corrupted character data",
                    ErrorCode = ApprovalErrorCode.CorruptedData
                };
            }

            // Validate integrity
            if (!_persistenceService.ValidateCharacterIntegrity(characterData))
            {
                return new ConnectionApprovalResult
                {
                    Approved = false,
                    RejectionReason = "Character data integrity check failed",
                    ErrorCode = ApprovalErrorCode.CorruptedData
                };
            }

            // Validate stats ranges
            var statsValidation = ValidateCharacterStats(characterData);
            if (!statsValidation.Approved)
            {
                return statsValidation;
            }

            return new ConnectionApprovalResult
            {
                Approved = true,
                RejectionReason = null,
                ErrorCode = ApprovalErrorCode.None
            };
        }

        private ConnectionApprovalResult ValidateCharacterStats(CharacterData data)
        {
            // Validate level
            if (data.Level < 1 || data.Level > _maxLevel)
            {
                return new ConnectionApprovalResult
                {
                    Approved = false,
                    RejectionReason = $"Invalid level: {data.Level}",
                    ErrorCode = ApprovalErrorCode.StatsOutOfRange
                };
            }

            // Validate base stats
            if (!IsStatInRange(data.Health, 0, _maxStatValue) ||
                !IsStatInRange(data.MaxHealth, 1, _maxStatValue) ||
                !IsStatInRange(data.Armor, 0, _maxStatValue) ||
                !IsStatInRange(data.Strength, 0, _maxStatValue) ||
                !IsStatInRange(data.Intelligence, 0, _maxStatValue) ||
                !IsStatInRange(data.Stamina, 0, _maxStatValue))
            {
                return new ConnectionApprovalResult
                {
                    Approved = false,
                    RejectionReason = "Character stats out of valid range",
                    ErrorCode = ApprovalErrorCode.StatsOutOfRange
                };
            }

            // Validate equipment stats
            if (data.Equipment != null)
            {
                if (!IsStatInRange(data.Equipment.TotalArmorValue, 0, _maxStatValue) ||
                    !IsStatInRange(data.Equipment.TotalDamageBonus, 0, _maxStatValue))
                {
                    return new ConnectionApprovalResult
                    {
                        Approved = false,
                        RejectionReason = "Equipment stats out of valid range",
                        ErrorCode = ApprovalErrorCode.StatsOutOfRange
                    };
                }

                // Validate individual item stats
                if (data.Equipment.EquippedItems != null)
                {
                    foreach (var item in data.Equipment.EquippedItems)
                    {
                        if (item.Stats != null)
                        {
                            foreach (var stat in item.Stats.Values)
                            {
                                if (!IsStatInRange(stat, 0, _maxStatValue))
                                {
                                    return new ConnectionApprovalResult
                                    {
                                        Approved = false,
                                        RejectionReason = $"Item '{item.ItemName}' has invalid stat value",
                                        ErrorCode = ApprovalErrorCode.StatsOutOfRange
                                    };
                                }
                            }
                        }
                    }
                }
            }

            return new ConnectionApprovalResult { Approved = true };
        }

        private bool IsStatInRange(int value, int min, int max)
        {
            return value >= min && value <= max;
        }

        private bool IsStatInRange(long value, long min, long max)
        {
            return value >= min && value <= max;
        }

        #endregion

        #region Client Authentication

        public override void OnStartClient()
        {
            NetworkClient.RegisterHandler<AuthResponseMessage>(OnAuthResponseMessage, false);
        }

        public override void OnClientAuthenticate()
        {
            // Skip authentication for testing
            if (_skipAuthenticationForTesting)
            {
                Debug.Log("[ConnectionApproval] Skipping client authentication (testing mode)");
                ClientAccept();
                return;
            }
            
            // Client should send auth request with character data
            // This will be called by the UI when connecting
        }

        /// <summary>
        /// Sends authentication request with character data.
        /// Call this from UI after StartClient().
        /// </summary>
        public void SendAuthRequest(CharacterData characterData)
        {
            if (characterData == null)
            {
                Debug.LogError("[ConnectionApproval] Cannot send auth request with null character data");
                return;
            }

            byte[] encryptedData = _persistenceService.ExportCharacterForNetwork(characterData);
            
            var msg = new AuthRequestMessage
            {
                EncryptedCharacterData = encryptedData,
                ClientVersion = Application.version
            };

            NetworkClient.Send(msg);
            Debug.Log("[ConnectionApproval] Auth request sent");
        }

        private void OnAuthResponseMessage(AuthResponseMessage msg)
        {
            if (msg.Success)
            {
                Debug.Log("[ConnectionApproval] Authentication successful");
                ClientAccept();
            }
            else
            {
                Debug.LogError($"[ConnectionApproval] Authentication failed: {msg.Message} (Code: {msg.ErrorCode})");
                ClientReject();
            }
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Result of connection approval validation.
    /// </summary>
    public struct ConnectionApprovalResult
    {
        public bool Approved;
        public string RejectionReason;
        public ApprovalErrorCode ErrorCode;
    }

    /// <summary>
    /// Error codes for connection approval failures.
    /// </summary>
    public enum ApprovalErrorCode
    {
        None = 0,
        InvalidDataFormat = 1,
        CorruptedData = 2,
        StatsOutOfRange = 3,
        ValidationTimeout = 4,
        EncryptionFailure = 5
    }

    /// <summary>
    /// Authentication request message sent from client to server.
    /// </summary>
    public struct AuthRequestMessage : NetworkMessage
    {
        public byte[] EncryptedCharacterData;
        public string ClientVersion;
    }

    /// <summary>
    /// Authentication response message sent from server to client.
    /// </summary>
    public struct AuthResponseMessage : NetworkMessage
    {
        public bool Success;
        public string Message;
        public ApprovalErrorCode ErrorCode;
    }

    #endregion
}
