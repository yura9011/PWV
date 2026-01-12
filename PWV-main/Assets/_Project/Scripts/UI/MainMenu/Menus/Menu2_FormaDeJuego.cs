using UnityEngine;

namespace EtherDomes.UI.Menus
{
    /// <summary>
    /// Menu 2: Forma de Juego
    /// - Botón Crear Partida -> Menu 3
    /// - Botón Unirse a Partida -> Menu 5
    /// - Botón Atrás -> Menu 1
    /// - Área inferior para mensajes de error
    /// </summary>
    public class Menu2_FormaDeJuego : MonoBehaviour
    {
        private string _errorMessage = "";
        private float _errorTimer = 0f;
        private const float ERROR_DISPLAY_TIME = 5f;
        
        private void OnEnable()
        {
            if (MenuNavigator.Instance != null)
            {
                MenuNavigator.Instance.OnErrorMessage += OnErrorReceived;
            }
        }
        
        private void OnDisable()
        {
            if (MenuNavigator.Instance != null)
            {
                MenuNavigator.Instance.OnErrorMessage -= OnErrorReceived;
            }
        }
        
        private void Update()
        {
            if (_errorTimer > 0)
            {
                _errorTimer -= Time.deltaTime;
                if (_errorTimer <= 0)
                {
                    _errorMessage = "";
                }
            }
        }
        
        private void OnErrorReceived(string message)
        {
            _errorMessage = message;
            _errorTimer = ERROR_DISPLAY_TIME;
        }
        
        private void OnGUI()
        {
            if (MenuNavigator.Instance == null || MenuNavigator.Instance.CurrentMenu != MenuType.FormaDeJuego)
                return;
            
            float centerX = Screen.width / 2;
            float centerY = Screen.height / 2;
            float buttonWidth = 280;
            float buttonHeight = 50;
            float spacing = 20;
            
            // Título
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 36,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            titleStyle.normal.textColor = Color.white;
            
            GUI.Label(new Rect(centerX - 300, centerY - 180, 600, 60), "Modo de Juego", titleStyle);
            
            // Botones
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 22
            };
            
            float startY = centerY - 60;
            
            // Botón Crear Partida
            GUI.backgroundColor = new Color(0.2f, 0.6f, 0.2f);
            if (GUI.Button(new Rect(centerX - buttonWidth / 2, startY, buttonWidth, buttonHeight), "Crear Partida", buttonStyle))
            {
                MenuNavigator.Instance.NavigateTo(MenuType.CrearPartida);
            }
            
            // Botón Unirse a Partida
            GUI.backgroundColor = new Color(0.3f, 0.5f, 0.7f);
            if (GUI.Button(new Rect(centerX - buttonWidth / 2, startY + buttonHeight + spacing, buttonWidth, buttonHeight), "Unirse a Partida", buttonStyle))
            {
                MenuNavigator.Instance.NavigateTo(MenuType.UnirsePartida);
            }
            
            // Botón Atrás
            GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f);
            if (GUI.Button(new Rect(centerX - buttonWidth / 2, startY + (buttonHeight + spacing) * 2, buttonWidth, buttonHeight), "Atrás", buttonStyle))
            {
                MenuNavigator.Instance.NavigateTo(MenuType.Principal);
            }
            GUI.backgroundColor = Color.white;
            
            // Mensaje de error en la parte inferior
            if (!string.IsNullOrEmpty(_errorMessage))
            {
                GUIStyle errorStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 16,
                    alignment = TextAnchor.MiddleCenter
                };
                errorStyle.normal.textColor = new Color(1f, 0.3f, 0.3f);
                
                GUI.Label(new Rect(centerX - 300, Screen.height - 80, 600, 40), _errorMessage, errorStyle);
            }
        }
    }
}
