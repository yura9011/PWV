using System;
using Mirror;
using kcp2k;
using UnityEngine;
using EtherDomes.Persistence;

namespace EtherDomes.Network
{
    /// <summary>
    /// Manages network sessions for The Ether Domes.
    /// Wraps Mirror's NetworkManager with a simplified API for Host/Client/DedicatedServer modes.
    /// </summary>
    public class NetworkSessionManager : NetworkManager, INetworkSessionManager
    {
        public const int DEFAULT_MAX_PLAYERS = 10;

        [Header("Authentication")]
        [SerializeField] private ConnectionApprovalAuthenticator _authenticator;

        // Events
        public event Action<NetworkConnectionToClient> OnPlayerConnected;
        public event Action<NetworkConnectionToClient> OnPlayerDisconnected;
        public event Action<string> OnConnectionFailed;

        // State Properties
        public bool IsHost => NetworkServer.active && NetworkClient.active;
        public bool IsClient => NetworkClient.active && !NetworkServer.active;
        public bool IsServer => NetworkServer.active && !NetworkClient.active;
        public int ConnectedPlayerCount => NetworkServer.connections.Count;
        public int MaxPlayers => maxConnections;

        /// <summary>
        /// Gets the authenticator for sending auth requests.
        /// </summary>
        public ConnectionApprovalAuthenticator Authenticator => _authenticator;

        public override void Awake()
        {
            // Ensure we have a transport before base.Awake()
            EnsureTransport();
            
            base.Awake();
            maxConnections = DEFAULT_MAX_PLAYERS;

            // Setup authenticator if not assigned
            if (_authenticator == null)
            {
                _authenticator = GetComponent<ConnectionApprovalAuthenticator>();
            }

            if (_authenticator != null)
            {
                authenticator = _authenticator;
            }
        }

        private void EnsureTransport()
        {
            // Check if transport is already assigned
            if (transport != null) return;

            // Try to find existing transport on this GameObject
            transport = GetComponent<Transport>();
            if (transport != null)
            {
                Transport.active = transport;
                return;
            }

            // Add KcpTransport (Mirror's recommended transport)
            var kcpTransport = gameObject.AddComponent<kcp2k.KcpTransport>();
            transport = kcpTransport;
            Transport.active = kcpTransport;
            
            Debug.Log("[NetworkSessionManager] Added KcpTransport automatically");
        }

        #region Session Management

        public void StartAsHost(ushort port = 7777)
        {
            ConfigureTransport(port);
            StartHost();
            Debug.Log($"[NetworkSessionManager] Started as Host on port {port}");
        }

        public void StartAsClient(string ipAddress, ushort port = 7777)
        {
            StartAsClient(ipAddress, port, null);
        }

        /// <summary>
        /// Starts as client with character data for authentication.
        /// </summary>
        public void StartAsClient(string ipAddress, ushort port, CharacterData characterData)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                OnConnectionFailed?.Invoke("Invalid IP address");
                return;
            }

            networkAddress = ipAddress;
            ConfigureTransport(port);
            StartClient();
            
            // Send auth request after connection if character data provided
            if (characterData != null && _authenticator != null)
            {
                StartCoroutine(SendAuthAfterConnect(characterData));
            }
            
            Debug.Log($"[NetworkSessionManager] Connecting as Client to {ipAddress}:{port}");
        }

        private System.Collections.IEnumerator SendAuthAfterConnect(CharacterData characterData)
        {
            // Wait for connection to establish
            yield return new WaitUntil(() => NetworkClient.isConnected);
            yield return null; // Wait one frame
            
            _authenticator?.SendAuthRequest(characterData);
        }

        public void StartAsDedicatedServer(ushort port = 7777)
        {
            ConfigureTransport(port);
            StartServer();
            Debug.Log($"[NetworkSessionManager] Started as Dedicated Server on port {port}");
        }

        public void Disconnect()
        {
            if (IsHost)
            {
                StopHost();
                Debug.Log("[NetworkSessionManager] Host stopped");
            }
            else if (IsClient)
            {
                StopClient();
                Debug.Log("[NetworkSessionManager] Client disconnected");
            }
            else if (IsServer)
            {
                StopServer();
                Debug.Log("[NetworkSessionManager] Server stopped");
            }
        }

        private void ConfigureTransport(ushort port)
        {
            if (transport is PortTransport portTransport)
            {
                portTransport.Port = port;
            }
        }

        #endregion

        #region Mirror Callbacks

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);
            
            // Check max players
            if (ConnectedPlayerCount > maxConnections)
            {
                conn.Disconnect();
                Debug.LogWarning($"[NetworkSessionManager] Connection rejected: Max players ({maxConnections}) reached");
                return;
            }

            OnPlayerConnected?.Invoke(conn);
            Debug.Log($"[NetworkSessionManager] Player connected. Total: {ConnectedPlayerCount}");
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            OnPlayerDisconnected?.Invoke(conn);
            Debug.Log($"[NetworkSessionManager] Player disconnected. Remaining: {ConnectedPlayerCount - 1}");
            
            base.OnServerDisconnect(conn);
        }

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            Debug.Log("[NetworkSessionManager] Successfully connected to server");
        }

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            Debug.Log("[NetworkSessionManager] Disconnected from server");
        }

        public override void OnClientError(TransportError error, string reason)
        {
            base.OnClientError(error, reason);
            OnConnectionFailed?.Invoke($"Connection error: {error} - {reason}");
            Debug.LogError($"[NetworkSessionManager] Client error: {error} - {reason}");
        }

        #endregion
    }
}
