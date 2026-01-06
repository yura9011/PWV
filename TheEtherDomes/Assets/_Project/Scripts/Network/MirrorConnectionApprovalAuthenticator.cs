using System;
using System.Collections;
using Mirror;
using UnityEngine;
using EtherDomes.Data;

namespace EtherDomes.Network
{
    /// <summary>
    /// Mirror Authenticator that validates character data integrity on connection.
    /// Simplified version for basic authentication.
    /// </summary>
    public class MirrorConnectionApprovalAuthenticator : NetworkAuthenticator
    {
        [Header("Validation Settings")]
        [SerializeField] private float _validationTimeout = 10f;
        
        [Header("Debug")]
        [SerializeField] private bool _skipAuthenticationForTesting = true;

        public TimeSpan ValidationTimeout
        {
            get => TimeSpan.FromSeconds(_validationTimeout);
            set => _validationTimeout = (float)value.TotalSeconds;
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
            // Simple validation - accept all for now
            Debug.Log($"[ConnectionApproval] Connection approved for {conn.connectionId}");
            ServerAccept(conn);
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
