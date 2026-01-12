using System;

namespace EtherDomes.Network
{
    /// <summary>
    /// Interface for managing network sessions (Host, Client, Dedicated Server).
    /// Wraps Unity Netcode for GameObjects NetworkManager.
    /// </summary>
    public interface INetworkSessionManager
    {
        /// <summary>
        /// Start a session as Host (server + client simultaneously).
        /// </summary>
        /// <param name="port">Port to listen on (default: 7777)</param>
        bool StartAsHost(ushort port = 7777);

        /// <summary>
        /// Start a session as Client, connecting to a Host.
        /// </summary>
        /// <param name="ipAddress">IP address of the Host</param>
        /// <param name="port">Port to connect to (default: 7777)</param>
        void StartAsClient(string ipAddress, ushort port = 7777);

        /// <summary>
        /// Start a session as Dedicated Server (headless, no local client).
        /// </summary>
        /// <param name="port">Port to listen on (default: 7777)</param>
        void StartAsDedicatedServer(ushort port = 7777);

        /// <summary>
        /// Disconnect from the current session.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Fired when a player connects to the session.
        /// Parameter is the client ID of the connected player.
        /// </summary>
        event Action<ulong> OnPlayerConnected;

        /// <summary>
        /// Fired when a player disconnects from the session.
        /// Parameter is the client ID of the disconnected player.
        /// </summary>
        event Action<ulong> OnPlayerDisconnected;

        /// <summary>
        /// Fired when a connection attempt fails.
        /// Parameter is the error message.
        /// </summary>
        event Action<string> OnConnectionFailed;

        /// <summary>
        /// True if this instance is the Host (server + client).
        /// </summary>
        bool IsHost { get; }

        /// <summary>
        /// True if this instance is a Client (connected to a Host or Server).
        /// </summary>
        bool IsClient { get; }

        /// <summary>
        /// True if this instance is running as a Server (Host or Dedicated).
        /// </summary>
        bool IsServer { get; }

        /// <summary>
        /// Number of currently connected players.
        /// </summary>
        int ConnectedPlayerCount { get; }

        /// <summary>
        /// Maximum number of players allowed (always 10).
        /// </summary>
        int MaxPlayers { get; }

        /// <summary>
        /// Local client ID (only valid when IsClient is true).
        /// </summary>
        ulong LocalClientId { get; }
    }
}
