using System;
using System.Collections.Generic;
using EtherDomes.Combat;
using EtherDomes.Data;
using UnityEngine;
using UnityEngine.UI;

namespace EtherDomes.UI
{
    /// <summary>
    /// UI component for displaying buffs and debuffs on an entity.
    /// Buffs are displayed in a grid above the health bar.
    /// Debuffs are displayed in a grid below the health bar.
    /// Each icon shows remaining duration as a countdown.
    /// Requirements: 1.9, 1.10
    /// </summary>
    public class BuffUI : MonoBehaviour, IBuffUI
    {
        #region Serialized Fields

        [Header("Buff Display (Above Health Bar)")]
        [SerializeField] private Transform _buffContainer;
        [SerializeField] private GameObject _buffIconPrefab;
        [SerializeField] private int _maxBuffIcons = 20;

        [Header("Debuff Display (Below Health Bar)")]
        [SerializeField] private Transform _debuffContainer;
        [SerializeField] private GameObject _debuffIconPrefab;
        [SerializeField] private int _maxDebuffIcons = 20;

        [Header("Icon Settings")]
        [SerializeField] private float _iconSize = 32f;
        [SerializeField] private float _iconSpacing = 4f;
        [SerializeField] private int _iconsPerRow = 10;

        [Header("Colors")]
        [SerializeField] private Color _buffBorderColor = new Color(0.2f, 0.8f, 0.2f, 1f);
        [SerializeField] private Color _debuffBorderColor = new Color(0.8f, 0.2f, 0.2f, 1f);
        [SerializeField] private Color _playerDebuffHighlight = new Color(1f, 0.8f, 0.2f, 1f);

        [Header("Default Icons")]
        [SerializeField] private Sprite _defaultBuffIcon;
        [SerializeField] private Sprite _defaultDebuffIcon;

        #endregion

        #region Private Fields

        private ulong _entityId;
        private ulong _localPlayerId;
        private IBuffSystem _buffSystem;

        private readonly List<BuffIconInstance> _buffIcons = new List<BuffIconInstance>();
        private readonly List<BuffIconInstance> _debuffIcons = new List<BuffIconInstance>();

        private readonly Dictionary<string, BuffIconInstance> _activeBuffIcons = new Dictionary<string, BuffIconInstance>();
        private readonly Dictionary<string, BuffIconInstance> _activeDebuffIcons = new Dictionary<string, BuffIconInstance>();

        #endregion

        #region Events

        public event Action<string> OnBuffClicked;
        public event Action<string> OnDebuffClicked;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeIconPools();
        }

        private void Update()
        {
            UpdateDurationCountdowns();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the buff UI with the entity ID to track.
        /// </summary>
        public void Initialize(ulong entityId)
        {
            _entityId = entityId;
            RefreshDisplay();
        }

        /// <summary>
        /// Initialize with system references.
        /// </summary>
        public void Initialize(IBuffSystem buffSystem, ulong entityId, ulong localPlayerId = 0)
        {
            _buffSystem = buffSystem;
            _entityId = entityId;
            _localPlayerId = localPlayerId;

            SubscribeToEvents();
            RefreshDisplay();
        }

        /// <summary>
        /// Set the entity ID to track buffs/debuffs for.
        /// </summary>
        public void SetEntity(ulong entityId)
        {
            _entityId = entityId;
            RefreshDisplay();
        }

        /// <summary>
        /// Set the local player ID for highlighting player-applied debuffs.
        /// </summary>
        public void SetLocalPlayerId(ulong localPlayerId)
        {
            _localPlayerId = localPlayerId;
            RefreshDebuffHighlights();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Update the buff display with current active buffs.
        /// </summary>
        public void UpdateBuffs(IReadOnlyList<BuffInstance> buffs)
        {
            // Hide all current buff icons
            foreach (var icon in _activeBuffIcons.Values)
            {
                ReturnIconToPool(icon, true);
            }
            _activeBuffIcons.Clear();

            if (buffs == null) return;

            // Display new buffs
            int displayCount = Mathf.Min(buffs.Count, _maxBuffIcons);
            for (int i = 0; i < displayCount; i++)
            {
                var buff = buffs[i];
                var icon = GetIconFromPool(true);
                if (icon != null)
                {
                    SetupBuffIcon(icon, buff, true);
                    _activeBuffIcons[buff.BuffId] = icon;
                }
            }
        }

        /// <summary>
        /// Update the debuff display with current active debuffs.
        /// </summary>
        public void UpdateDebuffs(IReadOnlyList<BuffInstance> debuffs)
        {
            // Hide all current debuff icons
            foreach (var icon in _activeDebuffIcons.Values)
            {
                ReturnIconToPool(icon, false);
            }
            _activeDebuffIcons.Clear();

            if (debuffs == null) return;

            // Display new debuffs
            int displayCount = Mathf.Min(debuffs.Count, _maxDebuffIcons);
            for (int i = 0; i < displayCount; i++)
            {
                var debuff = debuffs[i];
                var icon = GetIconFromPool(false);
                if (icon != null)
                {
                    SetupBuffIcon(icon, debuff, false);
                    _activeDebuffIcons[debuff.BuffId] = icon;
                }
            }
        }

        /// <summary>
        /// Clear all buff/debuff displays.
        /// </summary>
        public void ClearAll()
        {
            foreach (var icon in _activeBuffIcons.Values)
            {
                ReturnIconToPool(icon, true);
            }
            _activeBuffIcons.Clear();

            foreach (var icon in _activeDebuffIcons.Values)
            {
                ReturnIconToPool(icon, false);
            }
            _activeDebuffIcons.Clear();
        }

        /// <summary>
        /// Show or hide the buff UI.
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        #endregion

        #region Private Methods

        private void InitializeIconPools()
        {
            // Create buff icon pool
            if (_buffContainer != null && _buffIconPrefab != null)
            {
                for (int i = 0; i < _maxBuffIcons; i++)
                {
                    var iconObj = Instantiate(_buffIconPrefab, _buffContainer);
                    var iconInstance = CreateIconInstance(iconObj, true);
                    iconInstance.SetActive(false);
                    _buffIcons.Add(iconInstance);
                }
            }

            // Create debuff icon pool
            if (_debuffContainer != null && _debuffIconPrefab != null)
            {
                for (int i = 0; i < _maxDebuffIcons; i++)
                {
                    var iconObj = Instantiate(_debuffIconPrefab, _debuffContainer);
                    var iconInstance = CreateIconInstance(iconObj, false);
                    iconInstance.SetActive(false);
                    _debuffIcons.Add(iconInstance);
                }
            }
        }

        private BuffIconInstance CreateIconInstance(GameObject iconObj, bool isBuff)
        {
            var instance = new BuffIconInstance
            {
                GameObject = iconObj,
                IconImage = iconObj.GetComponentInChildren<Image>(),
                DurationText = iconObj.GetComponentInChildren<TMPro.TextMeshProUGUI>(),
                BorderImage = iconObj.transform.Find("Border")?.GetComponent<Image>(),
                StackText = iconObj.transform.Find("StackText")?.GetComponent<TMPro.TextMeshProUGUI>()
            };

            // Setup click handler
            var button = iconObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnIconClicked(instance, isBuff));
            }

            return instance;
        }

        private BuffIconInstance GetIconFromPool(bool isBuff)
        {
            var pool = isBuff ? _buffIcons : _debuffIcons;
            foreach (var icon in pool)
            {
                if (!icon.IsActive)
                {
                    return icon;
                }
            }
            return null;
        }

        private void ReturnIconToPool(BuffIconInstance icon, bool isBuff)
        {
            icon.SetActive(false);
            icon.BuffId = null;
            icon.RemainingDuration = 0;
        }

        private void SetupBuffIcon(BuffIconInstance icon, BuffInstance buff, bool isBuff)
        {
            icon.BuffId = buff.BuffId;
            icon.RemainingDuration = buff.RemainingDuration;
            icon.SourceId = buff.SourceId;

            // Set icon sprite
            if (icon.IconImage != null)
            {
                icon.IconImage.sprite = buff.Data?.Icon ?? (isBuff ? _defaultBuffIcon : _defaultDebuffIcon);
            }

            // Set border color
            if (icon.BorderImage != null)
            {
                Color borderColor = isBuff ? _buffBorderColor : _debuffBorderColor;
                
                // Highlight player-applied debuffs (Requirements 9.6)
                if (!isBuff && buff.SourceId == _localPlayerId && _localPlayerId != 0)
                {
                    borderColor = _playerDebuffHighlight;
                }
                
                icon.BorderImage.color = borderColor;
            }

            // Set duration text
            UpdateDurationText(icon);

            // Set stack count
            if (icon.StackText != null)
            {
                if (buff.CurrentStacks > 1)
                {
                    icon.StackText.text = buff.CurrentStacks.ToString();
                    icon.StackText.gameObject.SetActive(true);
                }
                else
                {
                    icon.StackText.gameObject.SetActive(false);
                }
            }

            icon.SetActive(true);
        }

        private void UpdateDurationCountdowns()
        {
            float deltaTime = Time.deltaTime;

            // Update buff durations
            foreach (var kvp in _activeBuffIcons)
            {
                var icon = kvp.Value;
                icon.RemainingDuration -= deltaTime;
                UpdateDurationText(icon);
            }

            // Update debuff durations
            foreach (var kvp in _activeDebuffIcons)
            {
                var icon = kvp.Value;
                icon.RemainingDuration -= deltaTime;
                UpdateDurationText(icon);
            }
        }

        private void UpdateDurationText(BuffIconInstance icon)
        {
            if (icon.DurationText == null) return;

            float duration = icon.RemainingDuration;
            
            if (duration <= 0)
            {
                icon.DurationText.text = "";
            }
            else if (duration < 60)
            {
                // Show seconds
                icon.DurationText.text = $"{Mathf.CeilToInt(duration)}";
            }
            else
            {
                // Show minutes
                int minutes = Mathf.FloorToInt(duration / 60);
                icon.DurationText.text = $"{minutes}m";
            }
        }

        private void RefreshDisplay()
        {
            if (_buffSystem == null) return;

            var buffs = _buffSystem.GetActiveBuffs(_entityId);
            var debuffs = _buffSystem.GetActiveDebuffs(_entityId);

            UpdateBuffs(buffs);
            UpdateDebuffs(debuffs);
        }

        private void RefreshDebuffHighlights()
        {
            foreach (var kvp in _activeDebuffIcons)
            {
                var icon = kvp.Value;
                if (icon.BorderImage != null)
                {
                    Color borderColor = _debuffBorderColor;
                    if (icon.SourceId == _localPlayerId && _localPlayerId != 0)
                    {
                        borderColor = _playerDebuffHighlight;
                    }
                    icon.BorderImage.color = borderColor;
                }
            }
        }

        private void OnIconClicked(BuffIconInstance icon, bool isBuff)
        {
            if (string.IsNullOrEmpty(icon.BuffId)) return;

            if (isBuff)
            {
                OnBuffClicked?.Invoke(icon.BuffId);
            }
            else
            {
                OnDebuffClicked?.Invoke(icon.BuffId);
            }
        }

        #endregion

        #region Event Subscriptions

        private void SubscribeToEvents()
        {
            if (_buffSystem == null) return;

            _buffSystem.OnBuffApplied += HandleBuffApplied;
            _buffSystem.OnBuffExpired += HandleBuffExpired;
            _buffSystem.OnDebuffApplied += HandleDebuffApplied;
            _buffSystem.OnDebuffExpired += HandleDebuffExpired;
        }

        private void UnsubscribeFromEvents()
        {
            if (_buffSystem == null) return;

            _buffSystem.OnBuffApplied -= HandleBuffApplied;
            _buffSystem.OnBuffExpired -= HandleBuffExpired;
            _buffSystem.OnDebuffApplied -= HandleDebuffApplied;
            _buffSystem.OnDebuffExpired -= HandleDebuffExpired;
        }

        private void HandleBuffApplied(ulong targetId, BuffInstance buff)
        {
            if (targetId != _entityId) return;
            RefreshDisplay();
        }

        private void HandleBuffExpired(ulong targetId, BuffInstance buff)
        {
            if (targetId != _entityId) return;
            RefreshDisplay();
        }

        private void HandleDebuffApplied(ulong targetId, BuffInstance debuff)
        {
            if (targetId != _entityId) return;
            RefreshDisplay();
        }

        private void HandleDebuffExpired(ulong targetId, BuffInstance debuff)
        {
            if (targetId != _entityId) return;
            RefreshDisplay();
        }

        #endregion

        #region Nested Types

        /// <summary>
        /// Represents a single buff/debuff icon in the UI.
        /// </summary>
        private class BuffIconInstance
        {
            public GameObject GameObject;
            public Image IconImage;
            public TMPro.TextMeshProUGUI DurationText;
            public Image BorderImage;
            public TMPro.TextMeshProUGUI StackText;
            
            public string BuffId;
            public float RemainingDuration;
            public ulong SourceId;
            public bool IsActive => GameObject != null && GameObject.activeSelf;

            public void SetActive(bool active)
            {
                if (GameObject != null)
                {
                    GameObject.SetActive(active);
                }
            }
        }

        #endregion
    }
}
