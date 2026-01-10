#if UNITY_SERVICES_LOBBY
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace EtherDomes.Network
{
    /// <summary>
    /// Manages Unity Lobby service.
    /// Handles creating, listing, and joining lobbies.
    /// Keeps lobbies alive via Heartbeat.
    /// </summary>
    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager Instance { get; private set; }

        private const string KEY_RELAY_CODE = "RelayJoinCode";
        private Lobby _hostLobby;
        private Lobby _joinedLobby;
        private float _heartbeatTimer;
        private float _pollTimer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            HandleLobbyHeartbeat();
            HandleLobbyPollForUpdates(); // Polling for updates like player count changes or host leaving
        }

        private async void HandleLobbyHeartbeat()
        {
            if (_hostLobby != null)
            {
                _heartbeatTimer -= Time.deltaTime;
                if (_heartbeatTimer <= 0f)
                {
                    float heartbeatInterval = 15f;
                    _heartbeatTimer = heartbeatInterval;

                    try
                    {
                        await LobbyService.Instance.SendHeartbeatPingAsync(_hostLobby.Id);
                    }
                    catch (LobbyServiceException e)
                    {
                        Debug.LogWarning($"[LobbyManager] Heartbeat failed: {e.Message}");
                    }
                }
            }
        }

        // Basic polling implementation to keep local lobby object updated
        private async void HandleLobbyPollForUpdates()
        {
            if (_joinedLobby != null)
            {
                _pollTimer -= Time.deltaTime;
                if (_pollTimer <= 0f)
                {
                    float pollInterval = 1.1f; // Rate limit is 1 request per second usually
                    _pollTimer = pollInterval;

                    try
                    {
                        _joinedLobby = await LobbyService.Instance.GetLobbyAsync(_joinedLobby.Id);
                        
                        // If we are host, update local ref too
                        if (_hostLobby != null && _hostLobby.Id == _joinedLobby.Id)
                        {
                            _hostLobby = _joinedLobby; 
                        }
                    }
                    catch (LobbyServiceException e)
                    {
                        Debug.LogWarning($"[LobbyManager] Poll failed: {e.Message}");
                        // Handle disconnect or lobby close
                        if (e.Reason == LobbyExceptionReason.LobbyNotFound)
                        {
                            _joinedLobby = null;
                            _hostLobby = null;
                            // Trigger event: Left Lobby
                        }
                    }
                }
            }
        }

        public async Task<Lobby> CreateLobby(string lobbyName, int maxPlayers, string relayJoinCode, bool isPrivate = false)
        {
            try
            {
                CreateLobbyOptions options = new CreateLobbyOptions
                {
                    IsPrivate = isPrivate,
                    Player = new Player
                    {
                        Data = new Dictionary<string, PlayerDataObject>
                        {
                            { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "HostPlayer") }
                        }
                    },
                    Data = new Dictionary<string, DataObject>
                    {
                        { KEY_RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                    }
                };

                Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
                
                _hostLobby = lobby;
                _joinedLobby = lobby;
                
                Debug.Log($"[LobbyManager] Created Lobby: {lobby.Name} Code: {lobby.LobbyCode} Id: {lobby.Id}");
                return lobby;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"[LobbyManager] Failed to create lobby: {e.Message}");
                return null;
            }
        }

        public async Task<List<Lobby>> ListLobbies()
        {
            try
            {
                QueryLobbiesOptions options = new QueryLobbiesOptions
                {
                    Count = 25,
                    Filters = new List<QueryFilter>
                    {
                        new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT) // Slots > 0
                    },
                    Order = new List<QueryOrder>
                    {
                        new QueryOrder(true, QueryOrder.FieldOptions.Created)
                    }
                };

                QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(options);
                return queryResponse.Results;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"[LobbyManager] Failed to list lobbies: {e.Message}");
                return new List<Lobby>();
            }
        }

        public async Task<bool> JoinLobbyById(string lobbyId)
        {
            try
            {
                JoinLobbyByIdOptions options = new JoinLobbyByIdOptions
                {
                    Player = new Player
                    {
                        Data = new Dictionary<string, PlayerDataObject>
                        {
                            { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "ClientPlayer") }
                        }
                    }
                };

                Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);
                _joinedLobby = lobby;

                Debug.Log($"[LobbyManager] Joined Lobby: {lobby.Name}");
                return true;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"[LobbyManager] Failed to join lobby: {e.Message}");
                return false;
            }
        }

        public async Task<bool> QuickJoinLobby()
        {
             try
            {
                _joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
                Debug.Log($"[LobbyManager] Quick Joined Lobby: {_joinedLobby.Name}");
                return true;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log($"[LobbyManager] Quick Join Failed: {e.Message}");
                return false;
            }
        }

        public async Task LeaveLobby()
        {
            try
            {
                if (_joinedLobby != null)
                {
                    string playerId = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;
                    await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, playerId);
                    _joinedLobby = null;
                    _hostLobby = null;
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"[LobbyManager] Failed to leave lobby: {e.Message}");
            }
        }

        public string GetRelayJoinCode()
        {
            if (_joinedLobby != null && _joinedLobby.Data != null)
            {
                if (_joinedLobby.Data.TryGetValue(KEY_RELAY_CODE, out DataObject dataObject))
                {
                    return dataObject.Value;
                }
            }
            return null;
        }

        private void OnDestroy()
        {
            // Try to leave lobby on destroy if we are in one
            // Note: Async in OnDestroy is tricky, fire and forget
            LeaveLobby();
        }
    }
}
#else
// Stub implementation when Lobby package is not available
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace EtherDomes.Network
{
    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager Instance { get; private set; }
        private void Awake() { Instance = this; }
        public Task<object> CreateLobby(string name, int max, string code, bool priv = false) { /* Debug.LogWarning("[LobbyManager] Lobby package not available."); */ return Task.FromResult<object>(null); }
        public Task<List<object>> ListLobbies() { return Task.FromResult(new List<object>()); }
        public Task<bool> JoinLobbyById(string id) { return Task.FromResult(false); }
        public Task<bool> QuickJoinLobby() { return Task.FromResult(false); }
        public Task LeaveLobby() { return Task.CompletedTask; }
        public string GetRelayJoinCode() { return null; }
    }
}
#endif
