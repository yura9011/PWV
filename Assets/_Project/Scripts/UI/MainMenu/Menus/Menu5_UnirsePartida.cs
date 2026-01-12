using System.Collections.Generic;
using UnityEngine;
using EtherDomes.Data;

namespace EtherDomes.UI.Menus
{
    /// <summary>
    /// Menu 5: Unirse a Partida
    /// Pestañas: Servidores Recientes, Servidores de Amigos, Servidores Públicos
    /// </summary>
    public class Menu5_UnirsePartida : MonoBehaviour
    {
        private enum ServerTab { Recientes, Amigos, Publicos }
        private ServerTab _currentTab = ServerTab.Recientes;
        
        // Listas de servidores
        private List<RecentServerData> _recentServers = new List<RecentServerData>();
        private List<FriendServerData> _friendServers = new List<FriendServerData>();
        private List<PublicServerData> _publicServers = new List<PublicServerData>();
        
        private int _selectedIndex = -1;
        private Vector2 _scrollPosition;
        
        // Popups
        private bool _showPasswordPopup = false;
        private bool _showRelayPopup = false;
        private bool _showDisconnectPopup = false;
        private bool _showDeleteConfirmPopup = false;
        private string _passwordInput = "";
        private string _relayCodeInput = "";
        private string _popupError = "";
        private bool _isConnecting = false;
        private float _connectionTimeout = 0f;
        
        private object _pendingServer = null;
        private RecentServerData _serverToDelete = null;
        
        // Para reconexión con contraseña
        private string _pendingRelayCode = "";
        private string _pendingServerName = "";
        
        private Rect _popupRect;
        
        private void OnEnable()
        {
            _popupRect = new Rect(Screen.width / 2 - 175, Screen.height / 2 - 100, 350, 200);
            ClearSelection();
            RefreshAllLists();
            
            // Mostrar popup de desconexión si hay mensaje
            if (GameStarter.Instance != null && GameStarter.Instance.HasDisconnectMessage)
            {
                _showDisconnectPopup = true;
            }
        }
        
        private void Update()
        {
            if (_isConnecting && _connectionTimeout > 0)
            {
                _connectionTimeout -= Time.deltaTime;
                if (_connectionTimeout <= 0)
                {
                    _isConnecting = false;
                    _popupError = "Error: Timeout - Servidor no disponible";
                    
                    if (_showRelayPopup)
                    {
                        _showRelayPopup = false;
                        MenuNavigator.Instance.ShowError("Error Time Out - Servidor no disponible");
                    }
                }
            }
        }
        
        private void RefreshAllLists()
        {
            RefreshRecentServers();
            RefreshFriendServers();
            RefreshPublicServers();
        }
        
        private void RefreshRecentServers()
        {
            var recentData = LocalDataManager.GetRecentServers();
            _recentServers = recentData.Servers;
        }
        
        private void RefreshFriendServers()
        {
            // TODO: Implementar sistema de amigos
            // Por ahora lista vacía con placeholder
            _friendServers.Clear();
        }
        
        private void RefreshPublicServers()
        {
            // TODO: Implementar lista de servidores públicos
            // Por ahora lista vacía con placeholder
            _publicServers.Clear();
        }
        
        private void ClearSelection()
        {
            _selectedIndex = -1;
        }
        
        private void OnGUI()
        {
            if (MenuNavigator.Instance == null || MenuNavigator.Instance.CurrentMenu != MenuType.UnirsePartida)
                return;
            
            float centerX = Screen.width / 2;
            
            // Título
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 36,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            titleStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(centerX - 300, 30, 600, 50), "Unirse a Partida", titleStyle);
            
            // Pestañas
            float tabY = 90;
            float tabWidth = 180;
            float tabHeight = 35;
            float tabStartX = centerX - (tabWidth * 3 + 20) / 2;
            
            DrawTabs(tabStartX, tabY, tabWidth, tabHeight);
            
            // Lista de servidores según pestaña
            float listWidth = Screen.width - 200;
            float listHeight = 320;
            float listX = 100;
            float listY = tabY + tabHeight + 20;
            
            switch (_currentTab)
            {
                case ServerTab.Recientes:
                    DrawRecentServerList(new Rect(listX, listY, listWidth, listHeight));
                    break;
                case ServerTab.Amigos:
                    DrawFriendServerList(new Rect(listX, listY, listWidth, listHeight));
                    break;
                case ServerTab.Publicos:
                    DrawPublicServerList(new Rect(listX, listY, listWidth, listHeight));
                    break;
            }
            
            // Botones principales
            float btnY = Screen.height - 120;
            float btnWidth = 200;
            float btnHeight = 45;
            
            GUIStyle btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 16 };
            
            // Botón Atrás
            GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f);
            if (GUI.Button(new Rect(50, btnY, btnWidth, btnHeight), "Atrás", btnStyle))
            {
                MenuNavigator.Instance.NavigateTo(MenuType.FormaDeJuego);
            }
            
            // Botón Código Relay
            GUI.backgroundColor = new Color(0.4f, 0.5f, 0.7f);
            if (GUI.Button(new Rect(centerX - btnWidth / 2, btnY, btnWidth, btnHeight), "Código Relay", btnStyle))
            {
                _showRelayPopup = true;
                _relayCodeInput = "";
                _popupError = "";
                _isConnecting = false;
            }
            
            // Botón Entrar al Mundo
            GUI.backgroundColor = new Color(0.2f, 0.7f, 0.2f);
            GUI.enabled = _selectedIndex >= 0 && HasSelectedServer();
            if (GUI.Button(new Rect(Screen.width - 50 - btnWidth, btnY, btnWidth, btnHeight), "Entrar al Mundo", btnStyle))
            {
                TryJoinSelectedServer();
            }
            GUI.enabled = true;
            GUI.backgroundColor = Color.white;
            
            // Popups
            if (_showPasswordPopup)
                _popupRect = GUI.Window(400, _popupRect, DrawPasswordPopup, "Contraseña Requerida");
            
            if (_showRelayPopup)
                _popupRect = GUI.Window(401, _popupRect, DrawRelayPopup, "Ingresar Código Relay");
            
            if (_showDisconnectPopup)
                _popupRect = GUI.Window(402, _popupRect, DrawDisconnectPopup, "Desconectado");
            
            if (_showDeleteConfirmPopup)
                _popupRect = GUI.Window(403, _popupRect, DrawDeleteConfirmPopup, "Confirmar Eliminación");
        }
        
        private void DrawTabs(float startX, float y, float width, float height)
        {
            GUIStyle tabStyle = new GUIStyle(GUI.skin.button) { fontSize = 14, fontStyle = FontStyle.Bold };
            GUIStyle activeTabStyle = new GUIStyle(tabStyle);
            activeTabStyle.normal.textColor = Color.yellow;
            
            // Tab Recientes
            GUI.backgroundColor = _currentTab == ServerTab.Recientes ? new Color(0.3f, 0.5f, 0.3f) : new Color(0.3f, 0.3f, 0.3f);
            if (GUI.Button(new Rect(startX, y, width, height), $"Recientes ({_recentServers.Count})", 
                _currentTab == ServerTab.Recientes ? activeTabStyle : tabStyle))
            {
                _currentTab = ServerTab.Recientes;
                ClearSelection();
            }
            
            // Tab Amigos
            GUI.backgroundColor = _currentTab == ServerTab.Amigos ? new Color(0.3f, 0.5f, 0.3f) : new Color(0.3f, 0.3f, 0.3f);
            if (GUI.Button(new Rect(startX + width + 10, y, width, height), $"Amigos ({_friendServers.Count})", 
                _currentTab == ServerTab.Amigos ? activeTabStyle : tabStyle))
            {
                _currentTab = ServerTab.Amigos;
                ClearSelection();
            }
            
            // Tab Públicos
            GUI.backgroundColor = _currentTab == ServerTab.Publicos ? new Color(0.3f, 0.5f, 0.3f) : new Color(0.3f, 0.3f, 0.3f);
            if (GUI.Button(new Rect(startX + (width + 10) * 2, y, width, height), $"Públicos ({_publicServers.Count})", 
                _currentTab == ServerTab.Publicos ? activeTabStyle : tabStyle))
            {
                _currentTab = ServerTab.Publicos;
                ClearSelection();
            }
            
            GUI.backgroundColor = Color.white;
        }
        
        private void DrawRecentServerList(Rect area)
        {
            GUI.Box(new Rect(area.x - 5, area.y - 5, area.width + 10, area.height + 10), "");
            
            _scrollPosition = GUI.BeginScrollView(
                area,
                _scrollPosition,
                new Rect(0, 0, area.width - 20, Mathf.Max(area.height, _recentServers.Count * 60))
            );
            
            if (_recentServers.Count == 0)
            {
                DrawEmptyMessage(area, "No hay servidores recientes\n\nConéctate a un servidor usando 'Código Relay'");
            }
            else
            {
                for (int i = 0; i < _recentServers.Count; i++)
                {
                    var server = _recentServers[i];
                    DrawServerItem(i, server.ServerName, server.ConnectionCode, server.HasPassword, 
                        GetTimeAgo(server.LastVisited), area.width, true);
                }
            }
            
            GUI.EndScrollView();
        }
        
        private void DrawFriendServerList(Rect area)
        {
            GUI.Box(new Rect(area.x - 5, area.y - 5, area.width + 10, area.height + 10), "");
            
            _scrollPosition = GUI.BeginScrollView(
                area,
                _scrollPosition,
                new Rect(0, 0, area.width - 20, Mathf.Max(area.height, _friendServers.Count * 60))
            );
            
            if (_friendServers.Count == 0)
            {
                DrawEmptyMessage(area, "No hay amigos en línea\n\nEl sistema de amigos estará disponible próximamente");
            }
            else
            {
                for (int i = 0; i < _friendServers.Count; i++)
                {
                    var server = _friendServers[i];
                    DrawServerItem(i, server.FriendName, server.ServerName, false, "En línea", area.width, false);
                }
            }
            
            GUI.EndScrollView();
        }
        
        private void DrawPublicServerList(Rect area)
        {
            GUI.Box(new Rect(area.x - 5, area.y - 5, area.width + 10, area.height + 10), "");
            
            // Botón Actualizar
            GUI.backgroundColor = new Color(0.3f, 0.5f, 0.7f);
            if (GUI.Button(new Rect(area.x + area.width - 110, area.y - 35, 100, 28), "Actualizar"))
            {
                RefreshPublicServers();
            }
            GUI.backgroundColor = Color.white;
            
            _scrollPosition = GUI.BeginScrollView(
                area,
                _scrollPosition,
                new Rect(0, 0, area.width - 20, Mathf.Max(area.height, _publicServers.Count * 60))
            );
            
            if (_publicServers.Count == 0)
            {
                DrawEmptyMessage(area, "No hay servidores públicos disponibles\n\nLos servidores dedicados aparecerán aquí");
            }
            else
            {
                for (int i = 0; i < _publicServers.Count; i++)
                {
                    var server = _publicServers[i];
                    string info = $"{server.PlayerCount}/{server.MaxPlayers} jugadores";
                    DrawServerItem(i, server.ServerName, server.RelayCode, server.HasPassword, info, area.width, false);
                }
            }
            
            GUI.EndScrollView();
        }
        
        private void DrawEmptyMessage(Rect area, string message)
        {
            GUIStyle emptyStyle = new GUIStyle(GUI.skin.label) 
            { 
                fontSize = 14, 
                alignment = TextAnchor.MiddleCenter, 
                wordWrap = true 
            };
            emptyStyle.normal.textColor = Color.gray;
            GUI.Label(new Rect(10, area.height / 2 - 40, area.width - 40, 80), message, emptyStyle);
        }
        
        private void DrawServerItem(int index, string name, string code, bool hasPassword, string info, float listWidth, bool showDeleteBtn)
        {
            Rect itemRect = new Rect(5, index * 60, listWidth - 30, 55);
            bool isSelected = _selectedIndex == index;
            
            if (isSelected)
            {
                GUI.backgroundColor = new Color(0.3f, 0.6f, 0.3f, 0.8f);
                GUI.Box(itemRect, "");
                GUI.backgroundColor = Color.white;
            }
            
            // Área clickeable para seleccionar
            float selectWidth = showDeleteBtn ? itemRect.width - 35 : itemRect.width;
            if (GUI.Button(new Rect(itemRect.x, itemRect.y, selectWidth, itemRect.height), ""))
            {
                _selectedIndex = index;
            }
            
            // Nombre con icono de candado si tiene password
            GUIStyle nameStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };
            nameStyle.normal.textColor = isSelected ? Color.yellow : Color.white;
            string displayName = hasPassword ? $"🔒 {name}" : name;
            GUI.Label(new Rect(itemRect.x + 10, itemRect.y + 5, itemRect.width - 200, 24), displayName, nameStyle);
            
            // Info
            GUIStyle infoStyle = new GUIStyle(GUI.skin.label) { fontSize = 12 };
            infoStyle.normal.textColor = Color.gray;
            GUI.Label(new Rect(itemRect.x + 10, itemRect.y + 30, itemRect.width - 100, 20), info, infoStyle);
            
            // Código (a la derecha)
            GUIStyle codeStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, alignment = TextAnchor.MiddleRight };
            codeStyle.normal.textColor = Color.cyan;
            GUI.Label(new Rect(itemRect.x + itemRect.width - 180, itemRect.y + 10, 140, 20), code, codeStyle);
            
            // Botón eliminar (X) solo para recientes
            if (showDeleteBtn)
            {
                GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f);
                if (GUI.Button(new Rect(itemRect.x + itemRect.width - 30, itemRect.y + 12, 25, 25), "X"))
                {
                    if (_currentTab == ServerTab.Recientes && index < _recentServers.Count)
                    {
                        _serverToDelete = _recentServers[index];
                        _showDeleteConfirmPopup = true;
                    }
                }
                GUI.backgroundColor = Color.white;
            }
        }
        
        private string GetTimeAgo(System.DateTime dateTime)
        {
            var span = System.DateTime.Now - dateTime;
            if (span.TotalMinutes < 1) return "Hace un momento";
            if (span.TotalMinutes < 60) return $"Hace {(int)span.TotalMinutes} min";
            if (span.TotalHours < 24) return $"Hace {(int)span.TotalHours} horas";
            if (span.TotalDays < 7) return $"Hace {(int)span.TotalDays} días";
            return dateTime.ToString("dd/MM/yyyy");
        }
        
        private bool HasSelectedServer()
        {
            switch (_currentTab)
            {
                case ServerTab.Recientes:
                    return _selectedIndex >= 0 && _selectedIndex < _recentServers.Count;
                case ServerTab.Amigos:
                    return _selectedIndex >= 0 && _selectedIndex < _friendServers.Count;
                case ServerTab.Publicos:
                    return _selectedIndex >= 0 && _selectedIndex < _publicServers.Count;
                default:
                    return false;
            }
        }
        
        private void TryJoinSelectedServer()
        {
            if (!HasSelectedServer())
            {
                MenuNavigator.Instance.ShowError("Selecciona un servidor primero");
                return;
            }
            
            string serverName = "";
            string connectionCode = "";
            bool hasPassword = false;
            
            switch (_currentTab)
            {
                case ServerTab.Recientes:
                    var recent = _recentServers[_selectedIndex];
                    serverName = recent.ServerName;
                    connectionCode = recent.ConnectionCode;
                    hasPassword = recent.HasPassword;
                    _pendingServer = recent;
                    break;
                case ServerTab.Amigos:
                    var friend = _friendServers[_selectedIndex];
                    serverName = friend.ServerName;
                    connectionCode = friend.RelayCode;
                    hasPassword = false;
                    _pendingServer = friend;
                    break;
                case ServerTab.Publicos:
                    var pub = _publicServers[_selectedIndex];
                    serverName = pub.ServerName;
                    connectionCode = pub.RelayCode;
                    hasPassword = pub.HasPassword;
                    _pendingServer = pub;
                    break;
            }
            
            if (hasPassword)
            {
                _passwordInput = "";
                _popupError = "";
                _showPasswordPopup = true;
            }
            else
            {
                ConnectToServer(serverName, connectionCode, "");
            }
        }
        
        private void ConnectToServer(string serverName, string connectionCode, string passwordHash)
        {
            UnityEngine.Debug.Log($"[Menu5] Conectando a: {serverName} ({connectionCode})");
            
            _isConnecting = true;
            _connectionTimeout = 15f;
            
            if (GameStarter.Instance != null)
            {
                GameStarter.Instance.JoinWithRelay(connectionCode, serverName, passwordHash, (success, requiresPassword) =>
                {
                    _isConnecting = false;
                    _showPasswordPopup = false;
                    
                    if (!success)
                    {
                        if (requiresPassword)
                        {
                            _popupError = "Contraseña incorrecta";
                            MenuNavigator.Instance.ShowError("Contraseña incorrecta");
                        }
                        else
                        {
                            _popupError = "No se pudo conectar al servidor";
                            MenuNavigator.Instance.ShowError("No se pudo conectar al servidor");
                        }
                    }
                });
            }
            else
            {
                _isConnecting = false;
                MenuNavigator.Instance.ShowError("Error: GameStarter no encontrado");
            }
        }
        
        private void DrawPasswordPopup(int windowId)
        {
            GUILayout.Space(15);
            GUILayout.Label("Este servidor requiere contraseña:");
            
            _passwordInput = GUILayout.PasswordField(_passwordInput, '*', 32);
            
            if (!string.IsNullOrEmpty(_popupError))
            {
                GUIStyle errorStyle = new GUIStyle(GUI.skin.label);
                errorStyle.normal.textColor = Color.red;
                GUILayout.Label(_popupError, errorStyle);
            }
            
            if (_isConnecting)
            {
                GUILayout.Label("Conectando...");
            }
            
            GUILayout.FlexibleSpace();
            
            bool enterPressed = Event.current.type == EventType.KeyDown && 
                               (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);
            
            GUILayout.BeginHorizontal();
            GUI.enabled = !_isConnecting;
            if (GUILayout.Button("Cancelar", GUILayout.Height(35)))
            {
                _showPasswordPopup = false;
                _pendingServer = null;
                _pendingRelayCode = "";
                _pendingServerName = "";
            }
            if ((GUILayout.Button("Entrar", GUILayout.Height(35)) || enterPressed) && !_isConnecting)
            {
                if (enterPressed) Event.current.Use();
                
                string hash = GameStarter.ComputePasswordHash(_passwordInput);
                
                // Verificar si es una conexión desde lista o desde código Relay
                if (_pendingServer != null)
                {
                    if (_pendingServer is RecentServerData recent)
                        ConnectToServer(recent.ServerName, recent.ConnectionCode, hash);
                    else if (_pendingServer is PublicServerData pub)
                        ConnectToServer(pub.ServerName, pub.RelayCode, hash);
                }
                else if (!string.IsNullOrEmpty(_pendingRelayCode))
                {
                    // Conexión desde código Relay manual
                    ConnectWithPassword(_pendingRelayCode, _pendingServerName, hash);
                }
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            
            GUI.DragWindow();
        }
        
        private void ConnectWithPassword(string relayCode, string serverName, string passwordHash)
        {
            _isConnecting = true;
            _connectionTimeout = 15f;
            
            UnityEngine.Debug.Log($"[Menu5] Reconectando con contraseña a: {serverName} ({relayCode})");
            
            if (GameStarter.Instance != null)
            {
                GameStarter.Instance.JoinWithRelay(relayCode, serverName, passwordHash, (success, requiresPassword) =>
                {
                    _isConnecting = false;
                    
                    if (success)
                    {
                        _showPasswordPopup = false;
                        _pendingRelayCode = "";
                        _pendingServerName = "";
                    }
                    else if (requiresPassword)
                    {
                        _popupError = "Contraseña incorrecta";
                    }
                    else
                    {
                        _popupError = "No se pudo conectar al servidor";
                    }
                });
            }
            else
            {
                _isConnecting = false;
                _popupError = "Error: GameStarter no encontrado";
            }
        }
        
        private void DrawRelayPopup(int windowId)
        {
            GUILayout.Space(15);
            
            if (_isConnecting)
            {
                GUILayout.Label($"Conectando... ({_connectionTimeout:F1}s)", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
                
                float progress = _connectionTimeout / 15f;
                GUI.Box(new Rect(20, 80, 310, 20), "");
                GUI.backgroundColor = new Color(0.3f, 0.7f, 0.3f);
                GUI.Box(new Rect(22, 82, 306 * progress, 16), "");
                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUILayout.Label("Ingresa el código Relay:");
                _relayCodeInput = GUILayout.TextField(_relayCodeInput.ToUpper(), 10);
                
                if (!string.IsNullOrEmpty(_popupError))
                {
                    GUIStyle errorStyle = new GUIStyle(GUI.skin.label);
                    errorStyle.normal.textColor = Color.red;
                    GUILayout.Label(_popupError, errorStyle);
                }
            }
            
            GUILayout.FlexibleSpace();
            
            bool enterPressed = Event.current.type == EventType.KeyDown && 
                               (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);
            
            GUILayout.BeginHorizontal();
            GUI.enabled = !_isConnecting;
            if (GUILayout.Button("Cancelar", GUILayout.Height(35)))
            {
                _showRelayPopup = false;
                _isConnecting = false;
            }
            
            if (!_isConnecting)
            {
                if (GUILayout.Button("Conectar", GUILayout.Height(35)) || enterPressed)
                {
                    if (enterPressed) Event.current.Use();
                    
                    if (string.IsNullOrWhiteSpace(_relayCodeInput) || _relayCodeInput.Length < 4)
                    {
                        _popupError = "Código inválido";
                    }
                    else
                    {
                        StartRelayConnection(_relayCodeInput.ToUpper());
                    }
                }
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            
            GUI.DragWindow();
        }
        
        private void DrawDisconnectPopup(int windowId)
        {
            GUILayout.Space(20);
            
            GUIStyle msgStyle = new GUIStyle(GUI.skin.label) 
            { 
                fontSize = 16, 
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            msgStyle.normal.textColor = Color.yellow;
            
            string message = GameStarter.Instance?.DisconnectMessage ?? "Conexión perdida con el servidor";
            GUILayout.Label(message, msgStyle);
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Aceptar", GUILayout.Height(40)))
            {
                _showDisconnectPopup = false;
                GameStarter.Instance?.ClearDisconnectMessage();
            }
            
            GUI.DragWindow();
        }
        
        private void DrawDeleteConfirmPopup(int windowId)
        {
            GUILayout.Space(20);
            
            GUIStyle msgStyle = new GUIStyle(GUI.skin.label) 
            { 
                fontSize = 14, 
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            
            string serverName = _serverToDelete?.ServerName ?? "servidor";
            GUILayout.Label($"¿Eliminar '{serverName}' de la lista de recientes?", msgStyle);
            
            GUILayout.FlexibleSpace();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancelar", GUILayout.Height(35)))
            {
                _showDeleteConfirmPopup = false;
                _serverToDelete = null;
            }
            
            GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f);
            if (GUILayout.Button("Eliminar", GUILayout.Height(35)))
            {
                if (_serverToDelete != null)
                {
                    LocalDataManager.DeleteRecentServer(_serverToDelete.ServerId);
                    RefreshRecentServers();
                    ClearSelection();
                }
                _showDeleteConfirmPopup = false;
                _serverToDelete = null;
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
            
            GUI.DragWindow();
        }
        
        private void StartRelayConnection(string code)
        {
            _isConnecting = true;
            _connectionTimeout = 15f;
            _popupError = "";
            _pendingRelayCode = code;
            _pendingServerName = $"Servidor {code}";
            
            UnityEngine.Debug.Log($"[Menu5] Intentando conectar con código Relay: {code}");
            
            if (GameStarter.Instance != null)
            {
                // Primero intentar sin contraseña
                GameStarter.Instance.JoinWithRelay(code, _pendingServerName, "", (success, requiresPassword) =>
                {
                    _isConnecting = false;
                    
                    if (success)
                    {
                        _showRelayPopup = false;
                    }
                    else if (requiresPassword)
                    {
                        // El servidor requiere contraseña, mostrar popup
                        _showRelayPopup = false;
                        _passwordInput = "";
                        _popupError = "";
                        _showPasswordPopup = true;
                        _pendingServer = null; // Usamos _pendingRelayCode en su lugar
                    }
                    else
                    {
                        _popupError = "No se pudo conectar al servidor";
                    }
                });
            }
            else
            {
                _isConnecting = false;
                _popupError = "Error: GameStarter no encontrado";
            }
        }
    }
    
    /// <summary>
    /// Datos de servidor de amigo
    /// </summary>
    [System.Serializable]
    public class FriendServerData
    {
        public string FriendId;
        public string FriendName;
        public string ServerName;
        public string RelayCode;
        public bool IsOnline;
    }
    
    /// <summary>
    /// Datos de servidor público
    /// </summary>
    [System.Serializable]
    public class PublicServerData
    {
        public string ServerId;
        public string ServerName;
        public string RelayCode;
        public int PlayerCount;
        public int MaxPlayers;
        public bool HasPassword;
    }
}
