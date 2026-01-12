using UnityEngine;

namespace EtherDomes.UI.Menus
{
    /// <summary>
    /// Menu 1: Principal
    /// - Botón Empezar Partida -> Menu 2
    /// - Botón Ajustes -> Popup placeholder
    /// - Botón Salir -> Cierra aplicación
    /// </summary>
    public class Menu1_Principal : MonoBehaviour
    {
        private bool _showAjustesPopup = false;
        private Rect _windowRect;
        
        private void OnEnable()
        {
            _windowRect = new Rect(Screen.width / 2 - 150, Screen.height / 2 - 100, 300, 200);
        }
        
        private void OnGUI()
        {
            if (MenuNavigator.Instance == null || MenuNavigator.Instance.CurrentMenu != MenuType.Principal)
                return;
            
            float centerX = Screen.width / 2;
            float centerY = Screen.height / 2;
            float buttonWidth = 250;
            float buttonHeight = 50;
            float spacing = 20;
            
            // Título
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 48,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            titleStyle.normal.textColor = Color.white;
            
            GUI.Label(new Rect(centerX - 300, centerY - 200, 600, 80), "The Ether Domes", titleStyle);
            
            // Botones
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 22
            };
            
            float startY = centerY - 50;
            
            // Botón Empezar Partida
            if (GUI.Button(new Rect(centerX - buttonWidth / 2, startY, buttonWidth, buttonHeight), "Empezar Partida", buttonStyle))
            {
                MenuNavigator.Instance.NavigateTo(MenuType.FormaDeJuego);
            }
            
            // Botón Ajustes
            if (GUI.Button(new Rect(centerX - buttonWidth / 2, startY + buttonHeight + spacing, buttonWidth, buttonHeight), "Ajustes", buttonStyle))
            {
                _showAjustesPopup = true;
            }
            
            // Botón Salir
            GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f);
            if (GUI.Button(new Rect(centerX - buttonWidth / 2, startY + (buttonHeight + spacing) * 2, buttonWidth, buttonHeight), "Salir", buttonStyle))
            {
                MenuNavigator.Instance.QuitApplication();
            }
            GUI.backgroundColor = Color.white;
            
            // Popup de Ajustes
            if (_showAjustesPopup)
            {
                _windowRect = GUI.Window(100, _windowRect, DrawAjustesWindow, "Ajustes");
            }
        }
        
        private void DrawAjustesWindow(int windowId)
        {
            GUILayout.Space(20);
            GUILayout.Label("Próximamente...", new GUIStyle(GUI.skin.label) 
            { 
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18
            });
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Cerrar", GUILayout.Height(40)))
            {
                _showAjustesPopup = false;
            }
            
            GUI.DragWindow();
        }
    }
}
