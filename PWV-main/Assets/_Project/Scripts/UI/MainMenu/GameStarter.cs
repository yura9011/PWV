using System.Collections;
using UnityEngine;
using EtherDomes.Network;
using EtherDomes.Data;
using Unity.Netcode;

namespace EtherDomes.UI
{
    /// <summary>
    /// Conecta el sistema de menús con NetworkSessionManager para iniciar partidas
    /// </summary>
    public class GameStarter : MonoBehaviour
    {
        public static GameStarter Instance { get; private set; }
        
        private NetworkSessionManager _networkManager;
        private RelayManager _relayManager;
        
        private bool _isStarting = false;
        
        // Datos pendientes para cliente que se une
        private bool _isJoiningAsClient = false;
        private string _pendingRelayCode = "";
        private string _pendingServerName = "";
        private string _pendingServerPassword = "";
        
        // Contraseña del servidor actual (solo el host la conoce)
        private string _currentServerPasswordHash = "";
        
        // Mensaje de desconexión
        public string DisconnectMessage { get; private set; } = "";
        public bool HasDisconnectMessage => !string.IsNullOrEmpty(DisconnectMessage);
        
        // Propiedad para que ConnectionApprovalManager pueda validar
        public string CurrentServerPasswordHash => _currentServerPasswordHash;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Start()
        {
            _networkManager = FindFirstObjectByType<NetworkSessionManager>();
            _relayManager = FindFirstObjectByType<RelayManager>();
            
            if (_networkManager != null)
            {
                _networkManager.OnConnectionFailed += OnConnectionFailed;
                _networkManager.OnPlayerConnected += OnPlayerConnected;
                _networkManager.OnPlayerDisconnected += OnPlayerDisconnected;
            }
            
            // Suscribirse al evento de shutdown del NetworkManager
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientStopped += OnClientStopped;
            }
        }
        
        private void OnDestroy()
        {
            if (_networkManager != null)
            {
                _networkManager.OnConnectionFailed -= OnConnectionFailed;
                _networkManager.OnPlayerConnected -= OnPlayerConnected;
                _networkManager.OnPlayerDisconnected -= OnPlayerDisconnected;
            }
            
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientStopped -= OnClientStopped;
            }
        }
        
        /// <summary>
        /// Inicia el servidor como Host (jugador + servidor)
        /// </summary>
        public async void StartAsHost(string worldId, string worldName, string password)
        {
            if (_isStarting || _networkManager == null) return;
            _isStarting = true;
            
            MenuNavigator.Instance?.ShowStatus("Iniciando servidor...");
            
            bool success = false;
            string joinCode = "";
            
            // Guardar contraseña del servidor (local y en holder para ConnectionApprovalManager)
            _currentServerPasswordHash = !string.IsNullOrEmpty(password) ? ComputePasswordHash(password) : "";
            ServerPasswordHolder.CurrentPasswordHash = _currentServerPasswordHash;
            ServerPasswordHolder.IsDedicatedServer = false;
            
            if (_relayManager != null)
            {
                joinCode = await _networkManager.StartHostWithRelay();
                success = !string.IsNullOrEmpty(joinCode);
            }
            
            if (!success)
            {
                MenuNavigator.Instance?.ShowStatus("Relay no disponible, iniciando directo...");
                success = _networkManager.StartAsHost(7777);
            }
            
            if (success)
            {
                LocalDataManager.UpdateWorldLastPlayed(worldId);
                
                // El Host aplica su personaje inmediatamente
                HideAllMenus();
                ApplyCharacterData();
                
                MenuNavigator.Instance?.ShowStatus($"Servidor iniciado! Código: {joinCode}");
                UnityEngine.Debug.Log($"[GameStarter] Host iniciado - Mundo: {worldName}, Relay: {joinCode}");
            }
            else
            {
                _currentServerPasswordHash = "";
                ServerPasswordHolder.Clear();
                MenuNavigator.Instance?.ShowError("Error al iniciar servidor");
            }
            
            _isStarting = false;
        }
        
        public static string ComputePasswordHash(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input);
                byte[] hash = sha256.ComputeHash(bytes);
                return System.Convert.ToBase64String(hash);
            }
        }
        
        /// <summary>
        /// Inicia servidor dedicado (sin jugador local)
        /// </summary>
        public async void StartAsDedicatedServer(string worldId, string worldName, string password)
        {
            if (_isStarting || _networkManager == null) return;
            _isStarting = true;
            
            MenuNavigator.Instance?.ShowStatus("Iniciando servidor dedicado...");
            
            bool success = false;
            string joinCode = "";
            
            // Guardar contraseña del servidor y marcar como dedicado
            _currentServerPasswordHash = !string.IsNullOrEmpty(password) ? ComputePasswordHash(password) : "";
            ServerPasswordHolder.CurrentPasswordHash = _currentServerPasswordHash;
            ServerPasswordHolder.IsDedicatedServer = true;
            
            if (_relayManager != null)
            {
                joinCode = await _networkManager.StartHostWithRelay();
                success = !string.IsNullOrEmpty(joinCode);
            }
            
            if (!success)
            {
                if (NetworkManager.Singleton != null)
                {
                    success = NetworkManager.Singleton.StartServer();
                }
            }
            
            if (success)
            {
                LocalDataManager.UpdateWorldLastPlayed(worldId);
                
                // Servidor dedicado NO aplica personaje (no hay jugador local)
                HideAllMenus();
                
                MenuNavigator.Instance?.ShowStatus($"Servidor dedicado iniciado! Código: {joinCode}");
                UnityEngine.Debug.Log($"[GameStarter] Servidor dedicado iniciado - Mundo: {worldName}, Relay: {joinCode}");
            }
            else
            {
                _currentServerPasswordHash = "";
                ServerPasswordHolder.Clear();
                MenuNavigator.Instance?.ShowError("Error al iniciar servidor dedicado");
            }
            
            _isStarting = false;
        }
        
        // Callback pendiente para conexión
        private System.Action<bool, bool> _pendingConnectionCallback = null;
        
        /// <summary>
        /// Conecta como cliente usando código Relay con contraseña
        /// Callback: (success, requiresPassword)
        /// </summary>
        public async void JoinWithRelay(string relayCode, string serverName, string passwordHash, System.Action<bool, bool> onComplete)
        {
            if (_isStarting || _networkManager == null)
            {
                onComplete?.Invoke(false, false);
                return;
            }
            _isStarting = true;
            _isJoiningAsClient = true;
            _pendingRelayCode = relayCode;
            _pendingServerName = serverName;
            _pendingServerPassword = passwordHash;
            _pendingConnectionCallback = onComplete;
            _lastConnectionFailReason = "";
            
            MenuNavigator.Instance?.ShowStatus($"Conectando con código: {relayCode}...");
            
            // Usar el método con contraseña
            bool startSuccess = await _networkManager.StartClientWithRelay(relayCode, passwordHash);
            
            if (!startSuccess)
            {
                _isStarting = false;
                _isJoiningAsClient = false;
                _pendingConnectionCallback = null;
                
                // Obtener error específico del RelayManager
                string relayError = _relayManager?.LastError ?? "";
                UnityEngine.Debug.LogWarning($"[GameStarter] StartClientWithRelay failed. RelayError: {relayError}");
                
                // Verificar si fue por contraseña inválida
                bool requiresPassword = _lastConnectionFailReason == "InvalidPassword";
                
                // Mostrar error específico
                if (!string.IsNullOrEmpty(relayError))
                {
                    MenuNavigator.Instance?.ShowError($"Error: {relayError}");
                }
                
                onComplete?.Invoke(false, requiresPassword);
                return;
            }
            
            UnityEngine.Debug.Log("[GameStarter] StartClientWithRelay succeeded, waiting for connection...");
            
            // El callback se llamará desde OnPlayerConnected o OnConnectionFailed
            // Iniciar timeout de conexión
            StartCoroutine(ConnectionTimeoutCoroutine(15f));
        }
        
        private IEnumerator ConnectionTimeoutCoroutine(float timeout)
        {
            float elapsed = 0f;
            
            while (elapsed < timeout && _pendingConnectionCallback != null)
            {
                // Si ya estamos conectados, el callback ya se llamó
                if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
                {
                    yield break;
                }
                
                yield return new WaitForSeconds(0.5f);
                elapsed += 0.5f;
            }
            
            // Timeout - si todavía hay callback pendiente, falló
            if (_pendingConnectionCallback != null)
            {
                var callback = _pendingConnectionCallback;
                _pendingConnectionCallback = null;
                _isStarting = false;
                _isJoiningAsClient = false;
                
                // Desconectar
                if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
                {
                    NetworkManager.Singleton.Shutdown();
                }
                
                UnityEngine.Debug.LogWarning("[GameStarter] Connection timeout");
                callback?.Invoke(false, false);
            }
        }
        
        // Almacena la razón del último fallo de conexión
        private string _lastConnectionFailReason = "";
        
        /// <summary>
        /// Conecta como cliente usando IP directa
        /// </summary>
        public void JoinWithIP(string ip, ushort port, System.Action<bool> onComplete)
        {
            if (_isStarting || _networkManager == null)
            {
                onComplete?.Invoke(false);
                return;
            }
            _isStarting = true;
            
            MenuNavigator.Instance?.ShowStatus($"Conectando a {ip}:{port}...");
            
            _networkManager.StartAsClient(ip, port);
            
            StartCoroutine(WaitForConnection(10f, success =>
            {
                if (success)
                {
                    LocalDataManager.AddRecentServer($"Servidor {ip}", $"{ip}:{port}", false, 1, 50);
                }
                _isStarting = false;
                onComplete?.Invoke(success);
            }));
        }
        
        private IEnumerator WaitForConnection(float timeout, System.Action<bool> onComplete)
        {
            float elapsed = 0f;
            
            while (elapsed < timeout)
            {
                if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
                {
                    onComplete?.Invoke(true);
                    yield break;
                }
                
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }
            
            _networkManager?.Disconnect();
            onComplete?.Invoke(false);
        }
        
        private void OnPlayerConnected(ulong clientId)
        {
            if (NetworkManager.Singleton == null) return;
            
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                UnityEngine.Debug.Log("[GameStarter] Jugador local conectado");
                
                _isStarting = false;
                
                // Llamar callback pendiente si existe
                if (_pendingConnectionCallback != null)
                {
                    var callback = _pendingConnectionCallback;
                    _pendingConnectionCallback = null;
                    callback?.Invoke(true, false);
                }
                
                if (_isJoiningAsClient)
                {
                    _isJoiningAsClient = false;
                    
                    // Agregar a servidores recientes
                    LocalDataManager.AddRecentServer(_pendingServerName, _pendingRelayCode, true, 1, 50, _pendingServerPassword);
                    UnityEngine.Debug.Log($"[GameStarter] Servidor agregado a recientes: {_pendingServerName}");
                    
                    // Ir a selección de personaje
                    MenuNavigator.Instance.SelectedWorldName = _pendingServerName;
                    MenuNavigator.Instance.ServerPassword = _pendingServerPassword;
                    MenuNavigator.Instance.NavigateTo(MenuType.SeleccionPersonaje);
                }
                // Si es Host, el personaje ya se aplicó en StartAsHost
            }
        }
        
        private void ApplyCharacterData()
        {
            string characterId = MenuNavigator.Instance?.SelectedCharacterId;
            if (string.IsNullOrEmpty(characterId)) return;
            
            var characters = LocalDataManager.GetCharacters();
            var character = characters.Characters.Find(c => c.CharacterId == characterId);
            
            if (character == null)
            {
                UnityEngine.Debug.LogWarning("[GameStarter] Personaje no encontrado");
                return;
            }
            
            UnityEngine.Debug.Log($"[GameStarter] Aplicando datos de personaje: {character.CharacterName} (Nivel {character.Level})");
            StartCoroutine(ApplyCharacterDataCoroutine(character));
        }
        
        private IEnumerator ApplyCharacterDataCoroutine(CharacterSaveData character)
        {
            // Esperar a que el Player se spawne (máximo 5 segundos)
            float timeout = 5f;
            float elapsed = 0f;
            GameObject playerObj = null;
            
            while (elapsed < timeout && playerObj == null)
            {
                yield return new WaitForSeconds(0.2f);
                elapsed += 0.2f;
                
                var players = FindObjectsByType<NetworkBehaviour>(FindObjectsSortMode.None);
                foreach (var player in players)
                {
                    if (player.IsOwner && player.gameObject.name.Contains("Player"))
                    {
                        playerObj = player.gameObject;
                        break;
                    }
                }
            }
            
            if (playerObj == null)
            {
                UnityEngine.Debug.LogWarning("[GameStarter] No se encontró el Player después de esperar");
                yield break;
            }
            
            // Aplicar datos del personaje
            var renderer = playerObj.GetComponentInChildren<MeshRenderer>();
            if (renderer != null)
            {
                Color classColor = GetClassColor(character.Class);
                renderer.material.color = classColor;
                UnityEngine.Debug.Log($"[GameStarter] Color aplicado: {character.Class}");
            }
            
            if (character.LastPosition != Vector3.zero)
            {
                playerObj.transform.position = character.LastPosition;
            }
            
            UnityEngine.Debug.Log($"[GameStarter] Datos de personaje aplicados: {character.CharacterName}");
        }
        
        private Color GetClassColor(CharacterClass charClass)
        {
            switch (charClass)
            {
                case CharacterClass.Warrior: return new Color(0.8f, 0.2f, 0.2f);
                case CharacterClass.Mage: return new Color(0.2f, 0.4f, 0.8f);
                case CharacterClass.Priest: return new Color(0.9f, 0.9f, 0.6f);
                case CharacterClass.Paladin: return new Color(0.8f, 0.6f, 0.2f);
                case CharacterClass.Rogue: return new Color(0.6f, 0.4f, 0.8f);
                case CharacterClass.Hunter: return new Color(0.2f, 0.7f, 0.3f);
                case CharacterClass.Warlock: return new Color(0.5f, 0.2f, 0.5f);
                case CharacterClass.DeathKnight: return new Color(0.3f, 0.5f, 0.7f);
                default: return new Color(0.5f, 0.5f, 0.5f);
            }
        }
        
        private void OnConnectionFailed(string reason)
        {
            _isStarting = false;
            _isJoiningAsClient = false;
            _lastConnectionFailReason = reason;
            
            // Llamar callback pendiente si existe
            if (_pendingConnectionCallback != null)
            {
                var callback = _pendingConnectionCallback;
                _pendingConnectionCallback = null;
                bool requiresPassword = reason == "InvalidPassword";
                callback?.Invoke(false, requiresPassword);
            }
            
            string message = reason;
            if (reason == "InvalidPassword")
            {
                message = "Contraseña incorrecta";
            }
            
            MenuNavigator.Instance?.ShowError($"Conexión fallida: {message}");
        }
        
        private void OnPlayerDisconnected(ulong clientId)
        {
            if (NetworkManager.Singleton == null) return;
            
            // Si somos cliente y nos desconectamos
            if (!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsServer)
            {
                if (clientId == NetworkManager.Singleton.LocalClientId || !NetworkManager.Singleton.IsConnectedClient)
                {
                    UnityEngine.Debug.Log("[GameStarter] Cliente desconectado del servidor");
                    DisconnectMessage = "Conexión perdida con el servidor";
                    ReturnToJoinMenu();
                }
            }
        }
        
        /// <summary>
        /// Llamado cuando el cliente se detiene (incluye cuando el host cierra)
        /// </summary>
        private void OnClientStopped(bool wasHost)
        {
            if (!wasHost && _isJoiningAsClient == false)
            {
                // Era un cliente y se desconectó
                UnityEngine.Debug.Log("[GameStarter] Cliente detenido");
                
                // Solo mostrar mensaje si estábamos en juego (no en menús)
                if (MenuNavigator.Instance != null && MenuNavigator.Instance.CurrentMenu == MenuType.None)
                {
                    DisconnectMessage = "Conexión perdida con el servidor";
                    ReturnToJoinMenu();
                }
            }
        }
        
        /// <summary>
        /// Vuelve al menú de unirse (Menu 5) con mensaje de desconexión
        /// </summary>
        public void ReturnToJoinMenu()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }
            
            _isStarting = false;
            _isJoiningAsClient = false;
            _currentServerPasswordHash = "";
            ServerPasswordHolder.Clear();
            
            if (MenuNavigator.Instance != null)
            {
                MenuNavigator.Instance.ClearSelections();
                MenuNavigator.Instance.NavigateTo(MenuType.UnirsePartida);
            }
        }
        
        /// <summary>
        /// Vuelve al menú principal
        /// </summary>
        public void ReturnToMainMenu()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }
            
            _isStarting = false;
            _isJoiningAsClient = false;
            _currentServerPasswordHash = "";
            ServerPasswordHolder.Clear();
            DisconnectMessage = "";
            
            if (MenuNavigator.Instance != null)
            {
                MenuNavigator.Instance.ClearSelections();
                MenuNavigator.Instance.NavigateTo(MenuType.Principal);
            }
        }
        
        /// <summary>
        /// Limpia el mensaje de desconexión
        /// </summary>
        public void ClearDisconnectMessage()
        {
            DisconnectMessage = "";
        }
        
        /// <summary>
        /// Llamado cuando el cliente ya conectado selecciona un personaje
        /// </summary>
        public void OnClientCharacterSelected()
        {
            HideAllMenus();
            ApplyCharacterData();
        }
        
        private void HideAllMenus()
        {
            var menuSystem = FindFirstObjectByType<MenuNavigator>();
            if (menuSystem != null)
            {
                menuSystem.NavigateTo(MenuType.None);
            }
        }
    }
}
