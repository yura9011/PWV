using System;
using UnityEngine;

namespace EtherDomes.UI
{
    /// <summary>
    /// Tipos de menú disponibles
    /// </summary>
    public enum MenuType
    {
        None,
        Principal,      // Menu 1
        FormaDeJuego,   // Menu 2
        CrearPartida,   // Menu 3
        SeleccionPersonaje, // Menu 4
        UnirsePartida   // Menu 5
    }
    
    /// <summary>
    /// Controlador central de navegación entre menús
    /// </summary>
    public class MenuNavigator : MonoBehaviour
    {
        public static MenuNavigator Instance { get; private set; }
        
        public event Action<MenuType> OnMenuChanged;
        public event Action<string> OnStatusMessage;
        public event Action<string> OnErrorMessage;
        
        private MenuType _currentMenu = MenuType.None;
        public MenuType CurrentMenu => _currentMenu;
        
        // Datos compartidos entre menús
        public string SelectedWorldId { get; set; }
        public string SelectedWorldName { get; set; }
        public string ServerPassword { get; set; }
        public string SelectedCharacterId { get; set; }
        public bool IsDedicatedServer { get; set; }
        
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
            // Iniciar en menú principal
            NavigateTo(MenuType.Principal);
        }
        
        /// <summary>
        /// Navega a un menú específico
        /// </summary>
        public void NavigateTo(MenuType menu)
        {
            var previousMenu = _currentMenu;
            _currentMenu = menu;
            
            UnityEngine.Debug.Log($"[MenuNavigator] {previousMenu} -> {menu}");
            OnMenuChanged?.Invoke(menu);
        }
        
        /// <summary>
        /// Muestra un mensaje de estado
        /// </summary>
        public void ShowStatus(string message)
        {
            UnityEngine.Debug.Log($"[MenuNavigator] Status: {message}");
            OnStatusMessage?.Invoke(message);
        }
        
        /// <summary>
        /// Muestra un mensaje de error
        /// </summary>
        public void ShowError(string message)
        {
            UnityEngine.Debug.LogWarning($"[MenuNavigator] Error: {message}");
            OnErrorMessage?.Invoke(message);
        }
        
        /// <summary>
        /// Limpia los datos de selección
        /// </summary>
        public void ClearSelections()
        {
            SelectedWorldId = null;
            SelectedWorldName = null;
            ServerPassword = null;
            SelectedCharacterId = null;
            IsDedicatedServer = false;
        }
        
        /// <summary>
        /// Sale de la aplicación
        /// </summary>
        public void QuitApplication()
        {
            UnityEngine.Debug.Log("[MenuNavigator] Saliendo del juego...");
            Application.Quit();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }
}
