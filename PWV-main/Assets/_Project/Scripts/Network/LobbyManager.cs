#if UNITY_SERVICES_LOBBY
using System;
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

        public const string KEY_RELAY_CODE = "RelayJoinCode";
        public const string KEY_PASSWORD_HASH = "PasswordHash";
        public const string KEY_WORLD_NAME = "WorldName";
        
        private Lobby _hostLobby;
        private Lobby _joinedLobby;
        private float _heartbeatTimer;
        private float _pollTimer;
        
        // Contraseña del servidor actual (solo el host la conoce)
        public string CurrentServerPasswordHash { get; private set; } = "";

        public Lobby CurrentLobby => _joinedLobby;
        public bool IsHostingLobby => _hostLobby != null;

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
            HandleLobbyPollForUpdates();
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

        private async void HandleLobbyPollForUpdates()
        {
            if (_joinedLobby != null)
            {
                _pollTimer -= Time.deltaTime;
                if (_pollTimer <= 0f)
                {
                    float pollInterval = 1.1f;
                    _pollTimer = pollInterval;

                    try
                    {
                        _joinedLobby = await LobbyService.Instance.GetLobbyAsync(_joinedLobby.Id);
                        
                        if (_hostLobby != null && _hostLobby.Id == _joinedLobby.Id)
                        {
                            _hostLobby = _joinedLobby; 
                        }
                    }
                    catch (LobbyServiceException e)
                    {
                        Debug.LogWarning($"[LobbyManager] Poll failed: {e.Message}");
                        if (e.Reason == LobbyExceptionReason.LobbyNotFound)
                        {
                            _joinedLobby = null;
                            _hostLobby = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Crea un lobby público o privado
        /// </summary>
        public async Task<Lobby> CreateLobby(string lobbyName, int maxPlayers, string relayJoinCode, bool isPrivate = false, string passwordHash = "")
        {
            try
            {
                // Guardar contraseña localmente
                CurrentServerPasswordHash = passwordHash;
                
                var lobbyData = new Dictionary<string, DataObject>
                {
                    { KEY_RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) },
                    { KEY_WORLD_NAME, new DataObject(DataObject.VisibilityOptions.Public, lobbyName) }
                };
                
                // Solo agregar password si existe (para que los clientes sepan que requiere contraseña)
                if (!string.IsNullOrEmpty(passwordHash))
                {
                    // Guardamos "true" para indicar que tiene password, no el hash real
                    lobbyData.Add(KEY_PASSWORD_HASH, new DataObject(DataObject.VisibilityOptions.Public, "true"));
                }

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
                    Data = lobbyData
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

        /// <summary>
        /// Actualiza el código Relay del lobby actual (cuando el host reinicia)
        /// </summary>
        public async Task<bool> UpdateRelayCode(string newRelayCode)
        {
            if (_hostLobby == null) return false;
            
            try
            {
                UpdateLobbyOptions options = new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { KEY_RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Member, newRelayCode) }
                    }
                };
                
                _hostLobby = await LobbyService.Instance.UpdateLobbyAsync(_hostLobby.Id, options);
                _joinedLobby = _hostLobby;
                
                Debug.Log($"[LobbyManager] Updated Relay Code to: {newRelayCode}");
                return true;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"[LobbyManager] Failed to update relay code: {e.Message}");
                return false;
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
                        new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                    },
                    Order = new List<QueryOrder>
                    {
                        new QueryOrder(false, QueryOrder.FieldOptions.Created) // Más recientes primero
                    }
                };

                QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(options);
                Debug.Log($"[LobbyManager] Found {queryResponse.Results.Count} public lobbies");
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
                    CurrentServerPasswordHash = "";
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"[LobbyManager] Failed to leave lobby: {e.Message}");
            }
        }
        
        public async Task DeleteLobby()
        {
            try
            {
                if (_hostLobby != null)
                {
                    await LobbyService.Instance.DeleteLobbyAsync(_hostLobby.Id);
                    Debug.Log($"[LobbyManager] Deleted lobby: {_hostLobby.Name}");
                    _hostLobby = null;
                    _joinedLobby = null;
                    CurrentServerPasswordHash = "";
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"[LobbyManager] Failed to delete lobby: {e.Message}");
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
        
        /// <summary>
        /// Verifica si un lobby requiere contraseña
        /// </summary>
        public static bool LobbyRequiresPassword(Lobby lobby)
        {
            if (lobby?.Data == null) return false;
            return lobby.Data.ContainsKey(KEY_PASSWORD_HASH);
        }
        
        /// <summary>
        /// Obtiene el nombre del mundo desde el lobby
        /// </summary>
        public static string GetLobbyWorldName(Lobby lobby)
        {
            if (lobby?.Data == null) return lobby?.Name ?? "Unknown";
            if (lobby.Data.TryGetValue(KEY_WORLD_NAME, out DataObject dataObject))
            {
                return dataObject.Value;
            }
            return lobby.Name;
        }

        private void OnDestroy()
        {
            // Fire and forget
            _ = DeleteLobby();
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
        public string CurrentServerPasswordHash { get; set; } = "";
        public bool IsHostingLobby => false;
        
        private void Awake() { Instance = this; }
        public Task<object> CreateLobby(string name, int max, string code, bool priv = false, string passwordHash = "") { return Task.FromResult<object>(null); }
        public Task<bool> UpdateRelayCode(string code) { return Task.FromResult(false); }
        public Task<List<object>> ListLobbies() { return Task.FromResult(new List<object>()); }
        public Task<bool> JoinLobbyById(string id) { return Task.FromResult(false); }
        public Task<bool> QuickJoinLobby() { return Task.FromResult(false); }
        public Task LeaveLobby() { return Task.CompletedTask; }
        public Task DeleteLobby() { return Task.CompletedTask; }
        public string GetRelayJoinCode() { return null; }
    }
}
#endif
