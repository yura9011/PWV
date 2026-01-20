using System;
using EtherDomes.Combat;
using EtherDomes.Data;
using UnityEngine;
using UnityEngine.UI;

namespace EtherDomes.UI
{
    /// <summary>
    /// UI component for displaying class-specific secondary resources.
    /// Supports Energy (Rogue), Focus (Hunter), and Combo Points (Rogue).
    /// Requirements: 10.6, 10.7
    /// </summary>
    public class ResourceUI : MonoBehaviour, IResourceUI
    {
        #region Serialized Fields

        [Header("Resource Bar")]
        [SerializeField] private GameObject _resourceBarRoot;
        [SerializeField] private Slider _resourceBar;
        [SerializeField] private Image _resourceBarFill;
        [SerializeField] private TMPro.TextMeshProUGUI _resourceText;
        [SerializeField] private TMPro.TextMeshProUGUI _resourceTypeLabel;

        [Header("Combo Points Display")]
        [SerializeField] private GameObject _comboPointsRoot;
        [SerializeField] private Image[] _comboPointPips;
        [SerializeField] private int _maxComboPointPips = 5;

        [Header("Resource Colors")]
        [SerializeField] private Color _energyColor = new Color(1f, 0.9f, 0.2f, 1f);
        [SerializeField] private Color _focusColor = new Color(0.8f, 0.5f, 0.2f, 1f);
        [SerializeField] private Color _rageColor = new Color(0.8f, 0.2f, 0.2f, 1f);
        [SerializeField] private Color _holyPowerColor = new Color(1f, 0.9f, 0.5f, 1f);
        [SerializeField] private Color _defaultColor = new Color(0.5f, 0.5f, 0.8f, 1f);

        [Header("Combo Point Colors")]
        [SerializeField] private Color _comboPointActiveColor = new Color(1f, 0.2f, 0.2f, 1f);
        [SerializeField] private Color _comboPointInactiveColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        [SerializeField] private Color _comboPointFullColor = new Color(1f, 0.5f, 0.2f, 1f);

        #endregion

        #region Private Fields

        private ulong _playerId;
        private SecondaryResourceType _resourceType;
        private ISecondaryResourceSystem _resourceSystem;
        private float _currentResource;
        private float _maxResource;
        private int _currentComboPoints;
        private int _maxComboPoints = 5;

        #endregion

        #region Events

        public event Action<float, float> OnResourceChanged;

        #endregion

        #region Properties

        public bool IsVisible => gameObject.activeSelf;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Hide by default until initialized
            SetVisible(false);
        }

        private void Update()
        {
            if (_resourceSystem != null && _playerId != 0)
            {
                UpdateFromResourceSystem();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the resource UI for a player.
        /// </summary>
        public void Initialize(ulong playerId, SecondaryResourceType resourceType)
        {
            _playerId = playerId;
            SetResourceType(resourceType);
        }

        /// <summary>
        /// Initialize with system references.
        /// </summary>
        public void Initialize(ISecondaryResourceSystem resourceSystem, ulong playerId, SecondaryResourceType resourceType)
        {
            _resourceSystem = resourceSystem;
            _playerId = playerId;
            
            SetResourceType(resourceType);
            SubscribeToEvents();
            
            // Initial update
            UpdateFromResourceSystem();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Update the resource bar display.
        /// Requirements: 10.6
        /// </summary>
        public void UpdateResource(float current, float max)
        {
            _currentResource = current;
            _maxResource = max;

            if (_resourceBar != null)
            {
                _resourceBar.value = max > 0 ? current / max : 0;
            }

            if (_resourceText != null)
            {
                _resourceText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
            }

            OnResourceChanged?.Invoke(current, max);
        }

        /// <summary>
        /// Update the combo points display.
        /// Requirements: 10.7
        /// </summary>
        public void UpdateComboPoints(int current, int max)
        {
            _currentComboPoints = current;
            _maxComboPoints = max;

            if (_comboPointPips == null) return;

            bool isAtMax = current >= max;

            for (int i = 0; i < _comboPointPips.Length; i++)
            {
                if (_comboPointPips[i] == null) continue;

                if (i < current)
                {
                    // Active pip
                    _comboPointPips[i].color = isAtMax ? _comboPointFullColor : _comboPointActiveColor;
                    _comboPointPips[i].gameObject.SetActive(true);
                }
                else if (i < max)
                {
                    // Inactive pip (within max)
                    _comboPointPips[i].color = _comboPointInactiveColor;
                    _comboPointPips[i].gameObject.SetActive(true);
                }
                else
                {
                    // Beyond max, hide
                    _comboPointPips[i].gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Set the resource type to display.
        /// </summary>
        public void SetResourceType(SecondaryResourceType resourceType)
        {
            _resourceType = resourceType;

            // Update visibility based on resource type
            bool showResourceBar = resourceType != SecondaryResourceType.None;
            bool showComboPoints = resourceType == SecondaryResourceType.Energy; // Rogues have combo points with Energy

            if (_resourceBarRoot != null)
            {
                _resourceBarRoot.SetActive(showResourceBar);
            }

            if (_comboPointsRoot != null)
            {
                _comboPointsRoot.SetActive(showComboPoints);
            }

            // Update resource bar color
            UpdateResourceBarColor();

            // Update label
            UpdateResourceTypeLabel();

            // Show UI if we have a resource type
            SetVisible(resourceType != SecondaryResourceType.None);
        }

        /// <summary>
        /// Show or hide the resource UI.
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        #endregion

        #region Private Methods

        private void UpdateFromResourceSystem()
        {
            if (_resourceSystem == null) return;

            // Update main resource
            float current = _resourceSystem.GetResource(_playerId);
            float max = _resourceSystem.GetMaxResource(_playerId);
            
            if (current != _currentResource || max != _maxResource)
            {
                UpdateResource(current, max);
            }

            // Update combo points if applicable (Energy users like Rogues)
            if (_resourceType == SecondaryResourceType.Energy)
            {
                int comboPoints = _resourceSystem.GetComboPoints(_playerId);
                int maxComboPoints = _resourceSystem.GetMaxComboPoints();
                
                if (comboPoints != _currentComboPoints || maxComboPoints != _maxComboPoints)
                {
                    UpdateComboPoints(comboPoints, maxComboPoints);
                }
            }
        }

        private void UpdateResourceBarColor()
        {
            if (_resourceBarFill == null) return;

            Color resourceColor = _resourceType switch
            {
                SecondaryResourceType.Mana => _defaultColor,
                SecondaryResourceType.Colera => _rageColor,
                SecondaryResourceType.Energia => _energyColor,
                SecondaryResourceType.EnergiaRunica => _holyPowerColor,
                _ => _defaultColor
            };

            _resourceBarFill.color = resourceColor;
        }

        private void UpdateResourceTypeLabel()
        {
            if (_resourceTypeLabel == null) return;

            string label = _resourceType switch
            {
                SecondaryResourceType.Mana => "Maná",
                SecondaryResourceType.Colera => "Cólera",
                SecondaryResourceType.Energia => "Energía",
                SecondaryResourceType.EnergiaRunica => "Energía Rúnica",
                _ => ""
            };

            _resourceTypeLabel.text = label;
        }

        #endregion

        #region Event Subscriptions

        private void SubscribeToEvents()
        {
            if (_resourceSystem == null) return;

            _resourceSystem.OnResourceChanged += HandleResourceChanged;
            _resourceSystem.OnComboPointsChanged += HandleComboPointsChanged;
        }

        private void UnsubscribeFromEvents()
        {
            if (_resourceSystem == null) return;

            _resourceSystem.OnResourceChanged -= HandleResourceChanged;
            _resourceSystem.OnComboPointsChanged -= HandleComboPointsChanged;
        }

        private void HandleResourceChanged(ulong playerId, float current, float max)
        {
            if (playerId != _playerId) return;
            UpdateResource(current, max);
        }

        private void HandleComboPointsChanged(ulong playerId, int current, int max)
        {
            if (playerId != _playerId) return;
            UpdateComboPoints(current, max);
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        /// <summary>
        /// Preview combo points in editor.
        /// </summary>
        [ContextMenu("Preview 3 Combo Points")]
        private void PreviewComboPoints()
        {
            UpdateComboPoints(3, 5);
        }

        /// <summary>
        /// Preview full combo points in editor.
        /// </summary>
        [ContextMenu("Preview Full Combo Points")]
        private void PreviewFullComboPoints()
        {
            UpdateComboPoints(5, 5);
        }

        /// <summary>
        /// Preview energy bar in editor.
        /// </summary>
        [ContextMenu("Preview Energy Bar")]
        private void PreviewEnergyBar()
        {
            SetResourceType(SecondaryResourceType.Energy);
            UpdateResource(75, 100);
        }

        /// <summary>
        /// Preview focus bar in editor.
        /// </summary>
        [ContextMenu("Preview Focus Bar")]
        private void PreviewFocusBar()
        {
            SetResourceType(SecondaryResourceType.Focus);
            UpdateResource(50, 100);
        }
#endif

        #endregion
    }
}
