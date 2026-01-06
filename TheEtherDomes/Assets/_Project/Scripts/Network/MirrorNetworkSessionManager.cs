using System;
using System.Collections.Generic;
using Mirror;
using kcp2k;
using UnityEngine;
using EtherDomes.Core;
using EtherDomes.Persistence;
using EtherDomes.Player;

namespace EtherDomes.Network
{
    /// <summary>
    /// Manages network sessions for The Ether Domes using Mirror.
    /// Wraps Mirror's NetworkManager with a simplified API for Host/Client/DedicatedServer modes.
    /// </summary>
    public class MirrorNetworkSessionManager : NetworkManager, INetworkSessionManager
    {
        public const int DEFAULT_MAX_PLAYERS = 10;

        [Header("Authentication")]
        [SerializeField] private MirrorConnectionApprovalAuthenticator _authenticator;

        // Events - using uint for Mirror compatibility (interface expects ulong)
        public event Action<ulong> OnPlayerConnected;
        public event Action<ulong> OnPlayerDisconnected;
        public event Action<string> OnConnectionFailed;

        // State Properties
        public bool IsHost => NetworkServer.active && NetworkClient.active;
        public bool IsClient => NetworkClient.active && !NetworkServer.active;
        public bool IsServer => NetworkServer.active && !NetworkClient.active;
        public int ConnectedPlayerCount => NetworkServer.connections.Count;
        public int MaxPlayers => maxConnections;
        public ulong LocalClientId => NetworkClient.active ? (ulong)NetworkClient.localPlayer?.netId : 0;

        /// <summary>
        /// Gets the authenticator for sending auth requests.
        /// </summary>
        public MirrorConnectionApprovalAuthenticator Authenticator => _authenticator;

        // Payload storage for spawning
        private Dictionary<int, ConnectionPayloadMessage> _pendingPayloads = new Dictionary<int, ConnectionPayloadMessage>();

        [Header("Spawn Settings")]
        [SerializeField] private Transform _defaultSpawnPoint;

        public override void Awake()
        {
            // Ensure we have a transport before base.Awake()
            EnsureTransport();
            
            base.Awake();
            maxConnections = DEFAULT_MAX_PLAYERS;

            // Setup authenticator if not assigned
            if (_authenticator == null)
            {
                _authenticator = GetComponent<MirrorConnectionApprovalAuthenticator>();
            }

            if (_authenticator != null)
            {
                authenticator = _authenticator;
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            NetworkServer.RegisterHandler<ConnectionPayloadMessage>(OnPayloadReceived);
            Debug.Log("[MirrorNetworkSessionManager] Server started, payload handler registered");
        }

        private void OnPayloadReceived(NetworkConnectionToClient conn, ConnectionPayloadMessage msg)
        {
            _pendingPayloads[conn.connectionId] = msg;
            Debug.Log($"[MirrorNetworkSessionManager] Received payload from {conn.connectionId}: Class={msg.ClassID}, Pos={msg.LastPosition}");
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
            
            Debug.Log("[MirrorNetworkSessionManager] Added KcpTransport automatically");
        }

        #region Session Management

        public void StartAsHost(ushort port = 7777)
        {
            ConfigureTransport(port);
            StartHost();
            Debug.Log($"[MirrorNetworkSessionManager] Started as Host on port {port}");
        }

        public void StartAsClient(string ipAddress, ushort port = 7777)
        {
            StartAsClientWithPayload(ipAddress, port);
        }

        /// <summary>
        /// Starts as client and sends connection payload with class and position.
        /// </summary>
        public void StartAsClientWithPayload(string ipAddress, ushort port = 7777)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                OnConnectionFailed?.Invoke("Invalid IP address");
                return;
            }

            networkAddress = ipAddress;
            ConfigureTransport(port);
            StartClient();
            
            // Send payload after connection
            StartCoroutine(SendPayloadAfterConnect());
            
            Debug.Log($"[MirrorNetworkSessionManager] Connecting as Client to {ipAddress}:{port}");
        }

        private System.Collections.IEnumerator SendPayloadAfterConnect()
        {
            yield return new WaitUntil(() => NetworkClient.isConnected);
            yield return null;
            
            // Build payload from current selection and saved position
            Vector3 defaultPos = _defaultSpawnPoint != null ? _defaultSpawnPoint.position : Vector3.zero;
            Vector3 lastPos = PositionPersistenceManager.LoadPosition(defaultPos);
            bool hasPos = PositionPersistenceManager.HasSavedPosition();
            
            var payload = new ConnectionPayloadMessage(
                ClassSelectionData.GetClassID(),
                lastPos,
                hasPos
            );
            
            NetworkClient.Send(payload);
            Debug.Log($"[MirrorNetworkSessionManager] Sent payload: Class={payload.ClassID}, Pos={payload.LastPosition}");
        }

        public void StartAsDedicatedServer(ushort port = 7777)
        {
            ConfigureTransport(port);
            StartServer();
            Debug.Log($"[MirrorNetworkSessionManager] Started as Dedicated Server on port {port}");
        }

        public void Disconnect()
        {
            if (IsHost)
            {
                StopHost();
                Debug.Log("[MirrorNetworkSessionManager] Host stopped");
            }
            else if (IsClient)
            {
                StopClient();
                Debug.Log("[MirrorNetworkSessionManager] Client disconnected");
            }
            else if (IsServer)
            {
                StopServer();
                Debug.Log("[MirrorNetworkSessionManager] Server stopped");
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

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            // Get spawn position from payload or default
            Vector3 spawnPos = _defaultSpawnPoint != null ? _defaultSpawnPoint.position : Vector3.zero;
            int classID = 0;
            
            if (_pendingPayloads.TryGetValue(conn.connectionId, out var payload))
            {
                if (payload.HasSavedPosition)
                {
                    spawnPos = payload.LastPosition;
                }
                classID = payload.ClassID;
                _pendingPayloads.Remove(conn.connectionId);
            }
            
            // Spawn player at position
            GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            
            // Set class visual if component exists
            var visualController = player.GetComponent<PlayerVisualController>();
            if (visualController != null)
            {
                visualController.ServerSetClass(classID);
            }
            
            NetworkServer.AddPlayerForConnection(conn, player);
            Debug.Log($"[MirrorNetworkSessionManager] Spawned player at {spawnPos} with class {classID}");
        }

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);
            
            // Check max players
            if (ConnectedPlayerCount > maxConnections)
            {
                conn.Disconnect();
                Debug.LogWarning($"[MirrorNetworkSessionManager] Connection rejected: Max players ({maxConnections}) reached");
                return;
            }

            OnPlayerConnected?.Invoke((ulong)conn.connectionId);
            Debug.Log($"[MirrorNetworkSessionManager] Player connected. Total: {ConnectedPlayerCount}");
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            OnPlayerDisconnected?.Invoke((ulong)conn.connectionId);
            Debug.Log($"[MirrorNetworkSessionManager] Player disconnected. Remaining: {ConnectedPlayerCount - 1}");
            
            base.OnServerDisconnect(conn);
        }

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            Debug.Log("[MirrorNetworkSessionManager] Successfully connected to server");
        }

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            Debug.Log("[MirrorNetworkSessionManager] Disconnected from server");
        }

        public override void OnClientError(TransportError error, string reason)
        {
            base.OnClientError(error, reason);
            OnConnectionFailed?.Invoke($"Connection error: {error} - {reason}");
            Debug.LogError($"[MirrorNetworkSessionManager] Client error: {error} - {reason}");
        }

        #endregion
    }
}
