using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;

namespace EtherDomes.Editor
{
    /// <summary>
    /// Editor utility to create the main menu UI with class selection.
    /// </summary>
    public static class MainMenuUICreator
    {
        [MenuItem("EtherDomes/Create Main Menu UI")]
        public static void CreateMainMenuUI()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();
            }
            
            EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject eventSystemGO = new GameObject("EventSystem");
                eventSystem = eventSystemGO.AddComponent<EventSystem>();
                eventSystemGO.AddComponent<StandaloneInputModule>();
            }

            GameObject mainPanel = CreatePanel(canvas.transform, "MainMenuPanel");
            
            CreateText(mainPanel.transform, "TitleText", "THE ETHER DOMES", 
                new Vector2(0, 200), 36, TextAnchor.MiddleCenter, Color.white);

            CreateText(mainPanel.transform, "ClassLabel", "Selecciona tu Clase:", 
                new Vector2(0, 100), 24, TextAnchor.MiddleCenter, Color.white);

            CreateButton(mainPanel.transform, "GuerreroButton", "GUERRERO", 
                new Vector2(-120, 40), new Vector2(200, 60), new Color(0.8f, 0.2f, 0.2f));
            
            CreateButton(mainPanel.transform, "MagoButton", "MAGO", 
                new Vector2(120, 40), new Vector2(200, 60), new Color(0.2f, 0.4f, 0.8f));

            CreateText(mainPanel.transform, "SelectionText", "Clase: Guerrero (Rojo)", 
                new Vector2(0, -20), 18, TextAnchor.MiddleCenter, Color.yellow);

            CreateText(mainPanel.transform, "ConnectionLabel", "Conexión:", 
                new Vector2(0, -70), 20, TextAnchor.MiddleCenter, Color.white);

            CreateInputField(mainPanel.transform, "IPInput", "127.0.0.1",
                new Vector2(0, -110), new Vector2(250, 40));

            CreateButton(mainPanel.transform, "HostButton", "HOST", 
                new Vector2(-100, -170), new Vector2(180, 50), new Color(0.2f, 0.6f, 0.2f));
            
            CreateButton(mainPanel.transform, "JoinButton", "JOIN", 
                new Vector2(100, -170), new Vector2(180, 50), new Color(0.3f, 0.5f, 0.7f));

            CreateText(mainPanel.transform, "StatusText", "Selecciona tu clase y conecta", 
                new Vector2(0, -230), 16, TextAnchor.MiddleCenter, Color.gray);

            GameObject controllerGO = new GameObject("MainMenuController");
            controllerGO.transform.SetParent(canvas.transform);
            controllerGO.AddComponent<UI.MirrorMainMenuController>();

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            UnityEngine.Debug.Log("[MainMenuUICreator] Main Menu UI created!");
            EditorUtility.DisplayDialog("Success", 
                "Main Menu UI created!\n\n" +
                "Includes:\n- EventSystem\n- Class selection (Guerrero/Mago)\n- IP input\n- Host/Join buttons\n- Status text\n\n" +
                "MirrorMainMenuController will auto-find all buttons.", 
                "OK");
        }

        private static GameObject CreatePanel(Transform parent, string name)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

            return panel;
        }

        private static GameObject CreateButton(Transform parent, string name, string text, 
            Vector2 position, Vector2 size, Color color)
        {
            GameObject buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent, false);

            RectTransform rect = buttonGO.AddComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Image image = buttonGO.AddComponent<Image>();
            image.color = color;

            Button button = buttonGO.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = color * 1.2f;
            colors.pressedColor = color * 0.8f;
            button.colors = colors;

            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);

            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text textComp = textGO.AddComponent<Text>();
            textComp.text = text;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComp.fontSize = 20;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.color = Color.white;

            return buttonGO;
        }

        private static GameObject CreateText(Transform parent, string name, string content,
            Vector2 position, int fontSize, TextAnchor alignment, Color color)
        {
            GameObject textGO = new GameObject(name);
            textGO.transform.SetParent(parent, false);

            RectTransform rect = textGO.AddComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(400, 50);

            Text text = textGO.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;

            return textGO;
        }

        private static GameObject CreateInputField(Transform parent, string name, string placeholder,
            Vector2 position, Vector2 size)
        {
            GameObject inputGO = new GameObject(name);
            inputGO.transform.SetParent(parent, false);

            RectTransform rect = inputGO.AddComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Image image = inputGO.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.25f);

            InputField input = inputGO.AddComponent<InputField>();

            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(inputGO.transform, false);

            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, -5);

            Text textComp = textGO.AddComponent<Text>();
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComp.fontSize = 18;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.color = Color.white;
            textComp.supportRichText = false;

            GameObject placeholderGO = new GameObject("Placeholder");
            placeholderGO.transform.SetParent(inputGO.transform, false);

            RectTransform phRect = placeholderGO.AddComponent<RectTransform>();
            phRect.anchorMin = Vector2.zero;
            phRect.anchorMax = Vector2.one;
            phRect.offsetMin = new Vector2(10, 5);
            phRect.offsetMax = new Vector2(-10, -5);

            Text phText = placeholderGO.AddComponent<Text>();
            phText.text = placeholder;
            phText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            phText.fontSize = 18;
            phText.fontStyle = FontStyle.Italic;
            phText.alignment = TextAnchor.MiddleCenter;
            phText.color = new Color(0.5f, 0.5f, 0.5f);

            input.textComponent = textComp;
            input.placeholder = phText;
            input.text = placeholder;

            return inputGO;
        }
    }
}
