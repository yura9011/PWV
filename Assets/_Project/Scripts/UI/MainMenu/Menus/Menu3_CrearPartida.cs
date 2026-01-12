using System.Collections.Generic;
using UnityEngine;
using EtherDomes.Data;

namespace EtherDomes.UI.Menus
{
    /// <summary>
    /// Menu 3: Crear Partida
    /// - Lista de mundos locales (seleccionable)
    /// - Botón Nuevo Mundo (popup para nombre)
    /// - Botón Borrar Mundo (popup confirmación)
    /// - Campo de texto Contraseña (opcional)
    /// - Botón Atrás -> Menu 2
    /// - Botón Iniciar Servidor -> Validar mundo -> Menu 4
    /// - Botón Iniciar Servidor Dedicado -> Validar mundo -> Iniciar sin personaje
    /// </summary>
    public class Menu3_CrearPartida : MonoBehaviour
    {
        private List<WorldSaveData> _worlds = new List<WorldSaveData>();
        private int _selectedWorldIndex = -1;
        private Vector2 _scrollPosition;
        private string _password = "";
        
        // Popups
        private bool _showNewWorldPopup = false;
        private bool _showDeleteConfirmPopup = false;
        private string _newWorldName = "";
        private string _newWorldError = "";
        
        private Rect _popupRect;
        
        private void OnEnable()
        {
            RefreshWorldList();
            _popupRect = new Rect(Screen.width / 2 - 175, Screen.height / 2 - 100, 350, 200);
        }
        
        private void RefreshWorldList()
        {
            var worldList = LocalDataManager.GetWorlds();
            _worlds = worldList.Worlds;
            _selectedWorldIndex = -1;
        }
        
        private void OnGUI()
        {
            if (MenuNavigator.Instance == null || MenuNavigator.Instance.CurrentMenu != MenuType.CrearPartida)
                return;
            
            float centerX = Screen.width / 2;
            float centerY = Screen.height / 2;
            
            // Título
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 36,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            titleStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(centerX - 300, 40, 600, 50), "Crear Partida", titleStyle);
            
            // Panel izquierdo - Lista de mundos
            float listX = centerX - 350;
            float listY = 110;
            float listWidth = 350;
            float listHeight = 300;
            
            GUI.Box(new Rect(listX - 10, listY - 10, listWidth + 20, listHeight + 20), "");
            
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };
            labelStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(listX, listY - 35, listWidth, 30), "Mundos Guardados:", labelStyle);
            
            // Lista scrolleable
            _scrollPosition = GUI.BeginScrollView(
                new Rect(listX, listY, listWidth, listHeight),
                _scrollPosition,
                new Rect(0, 0, listWidth - 20, Mathf.Max(listHeight, _worlds.Count * 55))
            );
            
            for (int i = 0; i < _worlds.Count; i++)
            {
                var world = _worlds[i];
                Rect itemRect = new Rect(5, i * 55, listWidth - 30, 50);
                
                // Fondo seleccionado
                if (_selectedWorldIndex == i)
                {
                    GUI.Box(itemRect, "");
                    GUI.backgroundColor = new Color(0.3f, 0.5f, 0.7f, 0.5f);
                    GUI.Box(itemRect, "");
                    GUI.backgroundColor = Color.white;
                }
                
                // Botón de selección
                if (GUI.Button(itemRect, ""))
                {
                    _selectedWorldIndex = i;
                }
                
                // Info del mundo
                GUIStyle nameStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };
                nameStyle.normal.textColor = _selectedWorldIndex == i ? Color.yellow : Color.white;
                GUI.Label(new Rect(itemRect.x + 10, itemRect.y + 5, itemRect.width - 20, 22), world.WorldName, nameStyle);
                
                GUIStyle infoStyle = new GUIStyle(GUI.skin.label) { fontSize = 12 };
                infoStyle.normal.textColor = Color.gray;
                string info = $"Creado: {world.CreatedAt:dd/MM/yyyy} | {(world.HasPassword ? "🔒" : "🔓")}";
                GUI.Label(new Rect(itemRect.x + 10, itemRect.y + 27, itemRect.width - 20, 20), info, infoStyle);
            }
            
            if (_worlds.Count == 0)
            {
                GUIStyle emptyStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, alignment = TextAnchor.MiddleCenter };
                emptyStyle.normal.textColor = Color.gray;
                GUI.Label(new Rect(0, listHeight / 2 - 20, listWidth - 20, 40), "No hay mundos creados\nCrea uno nuevo", emptyStyle);
            }
            
            GUI.EndScrollView();
            
            // Botones de mundo
            float btnY = listY + listHeight + 20;
            float btnWidth = 165;
            float btnHeight = 35;
            
            GUI.backgroundColor = new Color(0.2f, 0.6f, 0.2f);
            if (GUI.Button(new Rect(listX, btnY, btnWidth, btnHeight), "Nuevo Mundo"))
            {
                _showNewWorldPopup = true;
                _newWorldName = "";
                _newWorldError = "";
            }
            
            GUI.backgroundColor = new Color(0.7f, 0.2f, 0.2f);
            GUI.enabled = _selectedWorldIndex >= 0;
            if (GUI.Button(new Rect(listX + btnWidth + 20, btnY, btnWidth, btnHeight), "Borrar Mundo"))
            {
                _showDeleteConfirmPopup = true;
            }
            GUI.enabled = true;
            GUI.backgroundColor = Color.white;
            
            // Panel derecho - Configuración
            float configX = centerX + 50;
            float configY = 150;
            float configWidth = 300;
            
            GUIStyle configLabelStyle = new GUIStyle(GUI.skin.label) { fontSize = 16 };
            configLabelStyle.normal.textColor = Color.white;
            
            GUI.Label(new Rect(configX, configY, configWidth, 25), "Contraseña (opcional):", configLabelStyle);
            _password = GUI.PasswordField(new Rect(configX, configY + 30, configWidth, 35), _password, '*', 32);
            
            // Mundo seleccionado info
            if (_selectedWorldIndex >= 0 && _selectedWorldIndex < _worlds.Count)
            {
                var selected = _worlds[_selectedWorldIndex];
                GUIStyle selectedStyle = new GUIStyle(GUI.skin.label) { fontSize = 14 };
                selectedStyle.normal.textColor = Color.cyan;
                GUI.Label(new Rect(configX, configY + 90, configWidth, 25), $"Mundo: {selected.WorldName}", selectedStyle);
            }
            
            // Botones principales
            float mainBtnY = Screen.height - 150;
            float mainBtnWidth = 220;
            float mainBtnHeight = 45;
            
            GUIStyle mainBtnStyle = new GUIStyle(GUI.skin.button) { fontSize = 18 };
            
            // Iniciar Servidor
            GUI.backgroundColor = new Color(0.2f, 0.7f, 0.2f);
            GUI.enabled = _selectedWorldIndex >= 0;
            if (GUI.Button(new Rect(centerX - mainBtnWidth - 20, mainBtnY, mainBtnWidth, mainBtnHeight), "Iniciar Servidor", mainBtnStyle))
            {
                StartServer(false);
            }
            
            // Servidor Dedicado
            GUI.backgroundColor = new Color(0.4f, 0.4f, 0.6f);
            if (GUI.Button(new Rect(centerX + 20, mainBtnY, mainBtnWidth, mainBtnHeight), "Servidor Dedicado", mainBtnStyle))
            {
                StartServer(true);
            }
            GUI.enabled = true;
            
            // Botón Atrás
            GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f);
            if (GUI.Button(new Rect(centerX - 100, mainBtnY + mainBtnHeight + 15, 200, 40), "Atrás"))
            {
                MenuNavigator.Instance.NavigateTo(MenuType.FormaDeJuego);
            }
            GUI.backgroundColor = Color.white;
            
            // Popups
            if (_showNewWorldPopup)
            {
                _popupRect = GUI.Window(200, _popupRect, DrawNewWorldPopup, "Nuevo Mundo");
            }
            
            if (_showDeleteConfirmPopup)
            {
                _popupRect = GUI.Window(201, _popupRect, DrawDeleteConfirmPopup, "Confirmar");
            }
        }
        
        private void DrawNewWorldPopup(int windowId)
        {
            GUILayout.Space(15);
            GUILayout.Label("Nombre del mundo:");
            _newWorldName = GUILayout.TextField(_newWorldName, 30);
            
            if (!string.IsNullOrEmpty(_newWorldError))
            {
                GUIStyle errorStyle = new GUIStyle(GUI.skin.label);
                errorStyle.normal.textColor = Color.red;
                GUILayout.Label(_newWorldError, errorStyle);
            }
            
            GUILayout.FlexibleSpace();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancelar", GUILayout.Height(35)))
            {
                _showNewWorldPopup = false;
            }
            if (GUILayout.Button("Crear", GUILayout.Height(35)))
            {
                if (ValidateWorldName(_newWorldName, out string error))
                {
                    LocalDataManager.CreateWorld(_newWorldName);
                    RefreshWorldList();
                    _showNewWorldPopup = false;
                }
                else
                {
                    _newWorldError = error;
                }
            }
            GUILayout.EndHorizontal();
            
            GUI.DragWindow();
        }
        
        private void DrawDeleteConfirmPopup(int windowId)
        {
            GUILayout.Space(20);
            
            string worldName = _selectedWorldIndex >= 0 && _selectedWorldIndex < _worlds.Count 
                ? _worlds[_selectedWorldIndex].WorldName 
                : "???";
            
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
            GUILayout.Label($"¿Está seguro que desea borrar el mundo \"{worldName}\"?", labelStyle);
            
            GUILayout.FlexibleSpace();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("No", GUILayout.Height(35)))
            {
                _showDeleteConfirmPopup = false;
            }
            GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f);
            if (GUILayout.Button("Sí, Borrar", GUILayout.Height(35)))
            {
                if (_selectedWorldIndex >= 0 && _selectedWorldIndex < _worlds.Count)
                {
                    LocalDataManager.DeleteWorld(_worlds[_selectedWorldIndex].WorldId);
                    RefreshWorldList();
                }
                _showDeleteConfirmPopup = false;
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
            
            GUI.DragWindow();
        }
        
        private bool ValidateWorldName(string name, out string error)
        {
            error = "";
            
            if (string.IsNullOrWhiteSpace(name))
            {
                error = "El nombre no puede estar vacío";
                return false;
            }
            
            if (name.Length < 3)
            {
                error = "El nombre debe tener al menos 3 caracteres";
                return false;
            }
            
            if (name.Length > 30)
            {
                error = "El nombre no puede tener más de 30 caracteres";
                return false;
            }
            
            // Verificar si ya existe
            foreach (var world in _worlds)
            {
                if (world.WorldName.ToLower() == name.ToLower())
                {
                    error = "Ya existe un mundo con ese nombre";
                    return false;
                }
            }
            
            return true;
        }
        
        private void StartServer(bool dedicated)
        {
            if (_selectedWorldIndex < 0 || _selectedWorldIndex >= _worlds.Count)
            {
                MenuNavigator.Instance.ShowError("Selecciona un mundo primero");
                return;
            }
            
            var world = _worlds[_selectedWorldIndex];
            
            // Guardar datos en el navegador
            MenuNavigator.Instance.SelectedWorldId = world.WorldId;
            MenuNavigator.Instance.SelectedWorldName = world.WorldName;
            MenuNavigator.Instance.ServerPassword = _password;
            MenuNavigator.Instance.IsDedicatedServer = dedicated;
            
            // Actualizar última vez jugado
            LocalDataManager.UpdateWorldLastPlayed(world.WorldId);
            
            if (dedicated)
            {
                // Iniciar servidor dedicado directamente
                if (GameStarter.Instance != null)
                {
                    GameStarter.Instance.StartAsDedicatedServer(world.WorldId, world.WorldName, _password);
                }
                else
                {
                    MenuNavigator.Instance.ShowError("Error: GameStarter no encontrado");
                }
            }
            else
            {
                // Ir a selección de personaje
                MenuNavigator.Instance.NavigateTo(MenuType.SeleccionPersonaje);
            }
        }
    }
}
