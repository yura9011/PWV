using UnityEngine;
using UnityEngine.UI;
using EtherDomes.Core;

namespace EtherDomes.UI
{
    /// <summary>
    /// UI controller for class selection (Guerrero/Mago).
    /// </summary>
    public class ClassSelectorUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _guerreroButton;
        [SerializeField] private Button _magoButton;
        
        [Header("Visual Feedback")]
        [SerializeField] private Image _guerreroHighlight;
        [SerializeField] private Image _magoHighlight;
        [SerializeField] private Color _selectedColor = new Color(1f, 1f, 1f, 1f);
        [SerializeField] private Color _unselectedColor = new Color(1f, 1f, 1f, 0.5f);
        
        [Header("Labels")]
        [SerializeField] private Text _selectionLabel;

        private void Start()
        {
            // Auto-find buttons if not assigned
            if (_guerreroButton == null)
                _guerreroButton = FindButtonByName("GuerreroButton");
            if (_magoButton == null)
                _magoButton = FindButtonByName("MagoButton");
            
            // Setup listeners
            if (_guerreroButton != null)
                _guerreroButton.onClick.AddListener(SelectGuerrero);
            if (_magoButton != null)
                _magoButton.onClick.AddListener(SelectMago);
            
            // Apply initial selection
            UpdateVisualFeedback();
        }

        public void SelectGuerrero()
        {
            ClassSelectionData.SelectedClass = PlayerClass.Guerrero;
            UpdateVisualFeedback();
            UnityEngine.Debug.Log("[ClassSelector] Selected: Guerrero (Red)");
        }

        public void SelectMago()
        {
            ClassSelectionData.SelectedClass = PlayerClass.Mago;
            UpdateVisualFeedback();
            UnityEngine.Debug.Log("[ClassSelector] Selected: Mago (Blue)");
        }

        private void UpdateVisualFeedback()
        {
            bool isGuerrero = ClassSelectionData.SelectedClass == PlayerClass.Guerrero;
            
            // Update button colors
            if (_guerreroButton != null)
            {
                var colors = _guerreroButton.colors;
                colors.normalColor = isGuerrero ? _selectedColor : _unselectedColor;
                _guerreroButton.colors = colors;
            }
            
            if (_magoButton != null)
            {
                var colors = _magoButton.colors;
                colors.normalColor = !isGuerrero ? _selectedColor : _unselectedColor;
                _magoButton.colors = colors;
            }
            
            // Update highlights
            if (_guerreroHighlight != null)
                _guerreroHighlight.enabled = isGuerrero;
            if (_magoHighlight != null)
                _magoHighlight.enabled = !isGuerrero;
            
            // Update label
            if (_selectionLabel != null)
            {
                _selectionLabel.text = isGuerrero ? "Guerrero (Rojo)" : "Mago (Azul)";
                _selectionLabel.color = ClassSelectionData.GetClassColor();
            }
        }

        private Button FindButtonByName(string name)
        {
            var buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            foreach (var button in buttons)
            {
                if (button.gameObject.name == name)
                    return button;
            }
            return null;
        }
    }
}
