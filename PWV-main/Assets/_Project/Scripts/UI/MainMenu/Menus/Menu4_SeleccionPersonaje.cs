using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using EtherDomes.Data;

namespace EtherDomes.UI.Menus
{
    /// <summary>
    /// Menu 4: Selección de Personaje
    /// - Lista de personajes (nombre, nivel, clase) - máx 12
    /// - Botón Crear Personaje (popup con validación)
    /// - Vista central con modelo cápsula
    /// - Botón Eliminar Personaje (popup confirmación)
    /// - Botón Atrás -> Menu 3
    /// - Botón Iniciar Partida -> Entrar al juego
    /// </summary>
    public class Menu4_SeleccionPersonaje : MonoBehaviour
    {
        private List<CharacterSaveData> _characters = new List<CharacterSaveData>();
        private int _selectedCharacterIndex = -1;
        private Vector2 _scrollPosition;
        
        // Popups
        private bool _showCreatePopup = false;
        private bool _showDeletePopup = false;
        private string _newCharacterName = "";
        private string _createError = "";
        
        private Rect _popupRect;
        
        // Preview del personaje (rotación)
        private float _previewRotation = 0f;
        
        private void OnEnable()
        {
            RefreshCharacterList();
            _popupRect = new Rect(Screen.width / 2 - 175, Screen.height / 2 - 120, 350, 240);
        }
        
        private void Update()
        {
            // Rotar preview lentamente
            _previewRotation += Time.deltaTime * 20f;
            if (_previewRotation > 360f) _previewRotation -= 360f;
        }
        
        private void RefreshCharacterList()
        {
            var charList = LocalDataManager.GetCharacters();
            _characters = charList.Characters;
            _selectedCharacterIndex = -1;
        }
        
        private void OnGUI()
        {
            if (MenuNavigator.Instance == null || MenuNavigator.Instance.CurrentMenu != MenuType.SeleccionPersonaje)
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
            
            string worldName = MenuNavigator.Instance.SelectedWorldName ?? "Mundo";
            GUI.Label(new Rect(centerX - 300, 30, 600, 50), $"Selección de Personaje", titleStyle);
            
            GUIStyle subtitleStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleCenter };
            subtitleStyle.normal.textColor = Color.cyan;
            GUI.Label(new Rect(centerX - 200, 75, 400, 25), $"Mundo: {worldName}", subtitleStyle);
            
            // Panel izquierdo - Lista de personajes
            float listX = 50;
            float listY = 120;
            float listWidth = 320;
            float listHeight = 350;
            
            GUI.Box(new Rect(listX - 10, listY - 10, listWidth + 20, listHeight + 60), "");
            
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };
            labelStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(listX, listY - 35, listWidth, 30), $"Personajes ({_characters.Count}/12):", labelStyle);
            
            // Lista scrolleable
            _scrollPosition = GUI.BeginScrollView(
                new Rect(listX, listY, listWidth, listHeight),
                _scrollPosition,
                new Rect(0, 0, listWidth - 20, Mathf.Max(listHeight, _characters.Count * 65))
            );
            
            for (int i = 0; i < _characters.Count; i++)
            {
                var character = _characters[i];
                Rect itemRect = new Rect(5, i * 65, listWidth - 30, 60);
                
                // Fondo seleccionado
                if (_selectedCharacterIndex == i)
                {
                    GUI.backgroundColor = new Color(0.3f, 0.5f, 0.7f, 0.8f);
                    GUI.Box(itemRect, "");
                    GUI.backgroundColor = Color.white;
                }
                else
                {
                    GUI.Box(itemRect, "");
                }
                
                // Botón de selección
                if (GUI.Button(itemRect, ""))
                {
                    _selectedCharacterIndex = i;
                }
                
                // Info del personaje
                GUIStyle nameStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
                nameStyle.normal.textColor = _selectedCharacterIndex == i ? Color.yellow : Color.white;
                GUI.Label(new Rect(itemRect.x + 10, itemRect.y + 5, itemRect.width - 20, 24), character.CharacterName, nameStyle);
                
                GUIStyle infoStyle = new GUIStyle(GUI.skin.label) { fontSize = 14 };
                infoStyle.normal.textColor = Color.gray;
                GUI.Label(new Rect(itemRect.x + 10, itemRect.y + 30, itemRect.width - 20, 22), 
                    $"Nivel {character.Level} - {character.ClassDisplayName}", infoStyle);
            }
            
            if (_characters.Count == 0)
            {
                GUIStyle emptyStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, alignment = TextAnchor.MiddleCenter };
                emptyStyle.normal.textColor = Color.gray;
                GUI.Label(new Rect(0, listHeight / 2 - 20, listWidth - 20, 40), "No hay personajes\nCrea uno nuevo", emptyStyle);
            }
            
            GUI.EndScrollView();
            
            // Botones de personaje
            float btnY = listY + listHeight + 15;
            float btnWidth = 150;
            float btnHeight = 35;
            
            bool canCreate = _characters.Count < CharacterSaveDataList.MAX_CHARACTERS;
            
            GUI.backgroundColor = canCreate ? new Color(0.2f, 0.6f, 0.2f) : new Color(0.4f, 0.4f, 0.4f);
            GUI.enabled = canCreate;
            if (GUI.Button(new Rect(listX, btnY, btnWidth, btnHeight), "Crear Personaje"))
            {
                _showCreatePopup = true;
                _newCharacterName = "";
                _createError = "";
            }
            GUI.enabled = true;
            
            GUI.backgroundColor = new Color(0.7f, 0.2f, 0.2f);
            GUI.enabled = _selectedCharacterIndex >= 0;
            if (GUI.Button(new Rect(listX + btnWidth + 20, btnY, btnWidth, btnHeight), "Eliminar"))
            {
                _showDeletePopup = true;
            }
            GUI.enabled = true;
            GUI.backgroundColor = Color.white;
            
            // Panel central - Preview del personaje
            float previewX = centerX - 100;
            float previewY = 150;
            float previewSize = 250;
            
            GUI.Box(new Rect(previewX - 20, previewY - 20, previewSize + 40, previewSize + 80), "");
            
            // Dibujar representación simple del personaje
            DrawCharacterPreview(new Rect(previewX, previewY, previewSize, previewSize));
            
            // Info del personaje seleccionado
            if (_selectedCharacterIndex >= 0 && _selectedCharacterIndex < _characters.Count)
            {
                var selected = _characters[_selectedCharacterIndex];
                GUIStyle selectedNameStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
                selectedNameStyle.normal.textColor = Color.white;
                GUI.Label(new Rect(previewX - 20, previewY + previewSize + 10, previewSize + 40, 30), selected.CharacterName, selectedNameStyle);
                
                GUIStyle selectedInfoStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, alignment = TextAnchor.MiddleCenter };
                selectedInfoStyle.normal.textColor = Color.cyan;
                GUI.Label(new Rect(previewX - 20, previewY + previewSize + 35, previewSize + 40, 25), 
                    $"Nivel {selected.Level} | {selected.ClassDisplayName}", selectedInfoStyle);
            }
            
            // Panel derecho - Stats (placeholder)
            float statsX = Screen.width - 300;
            float statsY = 150;
            float statsWidth = 250;
            
            GUI.Box(new Rect(statsX - 10, statsY - 10, statsWidth + 20, 200), "");
            
            GUIStyle statsTitle = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };
            statsTitle.normal.textColor = Color.white;
            GUI.Label(new Rect(statsX, statsY, statsWidth, 25), "Estadísticas:", statsTitle);
            
            if (_selectedCharacterIndex >= 0 && _selectedCharacterIndex < _characters.Count)
            {
                var selected = _characters[_selectedCharacterIndex];
                GUIStyle statStyle = new GUIStyle(GUI.skin.label) { fontSize = 14 };
                statStyle.normal.textColor = Color.white;
                
                GUI.Label(new Rect(statsX, statsY + 35, statsWidth, 22), $"Vida: {selected.CurrentHealth}/{selected.MaxHealth}", statStyle);
                GUI.Label(new Rect(statsX, statsY + 57, statsWidth, 22), $"Maná: {selected.CurrentMana}/{selected.MaxMana}", statStyle);
                GUI.Label(new Rect(statsX, statsY + 79, statsWidth, 22), $"Clase: {selected.ClassDisplayName}", statStyle);
            }
            
            // Botones principales
            float mainBtnY = Screen.height - 100;
            float mainBtnWidth = 200;
            float mainBtnHeight = 45;
            
            GUIStyle mainBtnStyle = new GUIStyle(GUI.skin.button) { fontSize = 18 };
            
            // Botón Atrás
            GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f);
            if (GUI.Button(new Rect(centerX - mainBtnWidth - 30, mainBtnY, mainBtnWidth, mainBtnHeight), "Atrás", mainBtnStyle))
            {
                MenuNavigator.Instance.NavigateTo(MenuType.CrearPartida);
            }
            
            // Botón Iniciar Partida
            GUI.backgroundColor = new Color(0.2f, 0.7f, 0.2f);
            GUI.enabled = _selectedCharacterIndex >= 0;
            if (GUI.Button(new Rect(centerX + 30, mainBtnY, mainBtnWidth, mainBtnHeight), "Iniciar Partida", mainBtnStyle))
            {
                StartGame();
            }
            GUI.enabled = true;
            GUI.backgroundColor = Color.white;
            
            // Popups
            if (_showCreatePopup)
            {
                _popupRect = GUI.Window(300, _popupRect, DrawCreatePopup, "Crear Personaje");
            }
            
            if (_showDeletePopup)
            {
                _popupRect = GUI.Window(301, _popupRect, DrawDeletePopup, "Confirmar");
            }
        }
        
        private void DrawCharacterPreview(Rect area)
        {
            // Dibujar una representación simple (círculo para cabeza, rectángulo para cuerpo)
            Color charColor = Color.gray;
            
            if (_selectedCharacterIndex >= 0 && _selectedCharacterIndex < _characters.Count)
            {
                var selected = _characters[_selectedCharacterIndex];
                switch (selected.Class)
                {
                    case CharacterClass.Warrior:
                        charColor = new Color(0.8f, 0.2f, 0.2f);
                        break;
                    case CharacterClass.Mage:
                        charColor = new Color(0.2f, 0.4f, 0.8f);
                        break;
                    case CharacterClass.Priest:
                        charColor = new Color(0.9f, 0.9f, 0.6f);
                        break;
                    case CharacterClass.Paladin:
                        charColor = new Color(0.8f, 0.6f, 0.2f);
                        break;
                    case CharacterClass.Rogue:
                        charColor = new Color(0.6f, 0.4f, 0.8f);
                        break;
                    case CharacterClass.Hunter:
                        charColor = new Color(0.2f, 0.7f, 0.3f);
                        break;
                    case CharacterClass.Warlock:
                        charColor = new Color(0.5f, 0.2f, 0.5f);
                        break;
                    case CharacterClass.DeathKnight:
                        charColor = new Color(0.3f, 0.5f, 0.7f);
                        break;
                    default:
                        charColor = new Color(0.5f, 0.5f, 0.5f);
                        break;
                }
            }
            
            // Cuerpo (cápsula simplificada)
            float bodyWidth = 60;
            float bodyHeight = 120;
            float bodyX = area.x + area.width / 2 - bodyWidth / 2;
            float bodyY = area.y + area.height / 2 - bodyHeight / 2 + 20;
            
            GUI.backgroundColor = charColor;
            GUI.Box(new Rect(bodyX, bodyY, bodyWidth, bodyHeight), "");
            
            // Cabeza
            float headSize = 50;
            float headX = area.x + area.width / 2 - headSize / 2;
            float headY = bodyY - headSize + 10;
            GUI.Box(new Rect(headX, headY, headSize, headSize), "");
            
            GUI.backgroundColor = Color.white;
        }
        
        private void DrawCreatePopup(int windowId)
        {
            GUILayout.Space(15);
            GUILayout.Label("Nombre del personaje:");
            _newCharacterName = GUILayout.TextField(_newCharacterName, 20);
            
            GUILayout.Space(10);
            GUILayout.Label("Requisitos:", new GUIStyle(GUI.skin.label) { fontSize = 11 });
            GUIStyle reqStyle = new GUIStyle(GUI.skin.label) { fontSize = 10 };
            reqStyle.normal.textColor = Color.gray;
            GUILayout.Label("• Mínimo 3 caracteres", reqStyle);
            GUILayout.Label("• Sin caracteres especiales", reqStyle);
            GUILayout.Label("• No repetir 3+ letras seguidas", reqStyle);
            
            if (!string.IsNullOrEmpty(_createError))
            {
                GUIStyle errorStyle = new GUIStyle(GUI.skin.label) { fontSize = 12 };
                errorStyle.normal.textColor = Color.red;
                GUILayout.Label(_createError, errorStyle);
            }
            
            GUILayout.FlexibleSpace();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancelar", GUILayout.Height(35)))
            {
                _showCreatePopup = false;
            }
            if (GUILayout.Button("Crear", GUILayout.Height(35)))
            {
                if (ValidateCharacterName(_newCharacterName, out string error))
                {
                    LocalDataManager.CreateCharacter(_newCharacterName);
                    RefreshCharacterList();
                    _showCreatePopup = false;
                }
                else
                {
                    _createError = error;
                }
            }
            GUILayout.EndHorizontal();
            
            GUI.DragWindow();
        }
        
        private void DrawDeletePopup(int windowId)
        {
            GUILayout.Space(20);
            
            string charName = _selectedCharacterIndex >= 0 && _selectedCharacterIndex < _characters.Count 
                ? _characters[_selectedCharacterIndex].CharacterName 
                : "???";
            
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
            GUILayout.Label($"¿Está seguro que desea eliminar el personaje \"{charName}\"?", labelStyle);
            GUILayout.Space(10);
            GUILayout.Label("Esta acción no se puede deshacer.", new GUIStyle(GUI.skin.label) 
            { 
                alignment = TextAnchor.MiddleCenter, 
                fontSize = 11,
                normal = { textColor = Color.yellow }
            });
            
            GUILayout.FlexibleSpace();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("No", GUILayout.Height(35)))
            {
                _showDeletePopup = false;
            }
            GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f);
            if (GUILayout.Button("Sí, Eliminar", GUILayout.Height(35)))
            {
                if (_selectedCharacterIndex >= 0 && _selectedCharacterIndex < _characters.Count)
                {
                    LocalDataManager.DeleteCharacter(_characters[_selectedCharacterIndex].CharacterId);
                    RefreshCharacterList();
                }
                _showDeletePopup = false;
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
            
            GUI.DragWindow();
        }
        
        private bool ValidateCharacterName(string name, out string error)
        {
            error = "";
            
            if (string.IsNullOrWhiteSpace(name))
            {
                error = "El nombre no puede estar vacío";
                return false;
            }
            
            name = name.Trim();
            
            if (name.Length < 3)
            {
                error = "Mínimo 3 caracteres";
                return false;
            }
            
            if (name.Length > 20)
            {
                error = "Máximo 20 caracteres";
                return false;
            }
            
            // Solo letras y números
            if (!Regex.IsMatch(name, @"^[a-zA-Z0-9]+$"))
            {
                error = "Solo letras y números permitidos";
                return false;
            }
            
            // No 3+ letras iguales seguidas
            if (Regex.IsMatch(name, @"(.)\1{2,}"))
            {
                error = "No puede tener 3+ letras iguales seguidas";
                return false;
            }
            
            // Verificar si ya existe
            foreach (var character in _characters)
            {
                if (character.CharacterName.ToLower() == name.ToLower())
                {
                    error = "Ya existe un personaje con ese nombre";
                    return false;
                }
            }
            
            return true;
        }
        
        private void StartGame()
        {
            if (_selectedCharacterIndex < 0 || _selectedCharacterIndex >= _characters.Count)
            {
                MenuNavigator.Instance.ShowError("Selecciona un personaje primero");
                return;
            }
            
            var character = _characters[_selectedCharacterIndex];
            MenuNavigator.Instance.SelectedCharacterId = character.CharacterId;
            
            // Actualizar última vez jugado
            LocalDataManager.UpdateCharacter(character);
            
            // Verificar si ya estamos conectados como cliente
            if (Unity.Netcode.NetworkManager.Singleton != null && 
                Unity.Netcode.NetworkManager.Singleton.IsConnectedClient &&
                !Unity.Netcode.NetworkManager.Singleton.IsHost)
            {
                // Ya estamos conectados como cliente, solo ocultar menús y aplicar datos
                UnityEngine.Debug.Log("[Menu4] Cliente ya conectado, aplicando datos de personaje");
                
                if (GameStarter.Instance != null)
                {
                    GameStarter.Instance.OnClientCharacterSelected();
                }
            }
            else
            {
                // Iniciar servidor como Host
                if (GameStarter.Instance != null)
                {
                    GameStarter.Instance.StartAsHost(
                        MenuNavigator.Instance.SelectedWorldId,
                        MenuNavigator.Instance.SelectedWorldName,
                        MenuNavigator.Instance.ServerPassword
                    );
                }
                else
                {
                    UnityEngine.Debug.LogError("[Menu4] GameStarter no encontrado!");
                    MenuNavigator.Instance.ShowError("Error: GameStarter no encontrado");
                }
            }
        }
    }
}
