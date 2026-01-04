using System;
using Mirror;

namespace EtherDomes.Network
{
    /// <summary>
    /// Interface for managing network sessions in The Ether Domes.
    /// Provides a simplified API over Mirror's NetworkManager.
    /// </summary>
    public interface INetworkSessionManager
    {
        // Session Management
        void StartAsHost(ushort port = 7777);
        void StartAsClient(string ipAddress, ushort port = 7777);
        void StartAsDedicatedServer(ushort port = 7777);
        void Disconnect();

        // Events
        event Action<NetworkConnectionToClient> OnPlayerConnected;
        event Action<NetworkConnectionToClient> OnPlayerDisconnected;
        event Action<string> OnConnectionFailed;

        // State
        bool IsHost { get; }
        bool IsClient { get; }
        bool IsServer { get; }
        int ConnectedPlayerCount { get; }
        int MaxPlayers { get; }
    }
}
