using System.Collections.Generic;
using EtherDomes.Combat;
using EtherDomes.Data;
using UnityEngine;
using UnityEngine.UI;

namespace EtherDomes.UI
{
    /// <summary>
    /// UI component for displaying target information.
    /// Includes buff/debuff display for the selected target.
    /// Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 9.6
    /// </summary>
    public class TargetFrame : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject _frameRoot;
        [SerializeField] private TMPro.TextMeshProUGUI _nameText;
        [SerializeField] private Slider _healthBar;
        [SerializeField] private TMPro.TextMeshProUGUI _healthText;
        [SerializeField] private TMPro.TextMeshProUGUI _rangeText;
        [SerializeField] private Image _targetTypeIcon;

        [Header("Buff/Debuff Display")]
        [SerializeField] private Transform _buffContainer;
        [SerializeField] private Transform _debuffContainer;
        [SerializeField] private GameObject _buffIconPrefab;
        [SerializeField] private int _maxBuffIcons = 8;
        [SerializeField] private int _maxDebuffIcons = 8;

        [Header("Buff/Debuff Colors")]
        [SerializeField] private Color _buffBorderColor = new Color(0.2f, 0.8f, 0.2f, 1f);
        [SerializeField] private Color _debuffBorderColor = new Color(0.8f, 0.2f, 0.2f, 1f);
        [SerializeField] private Color _playerDebuffHighlight = new Color(1f, 0.8f, 0.2f, 1f);

        [Header("Default Icons")]
        [SerializeField] private Sprite _defaultBuffIcon;
        [SerializeField] private Sprite _defaultDebuffIcon;

        [Header("Colors")]
        [SerializeField] private Color _enemyColor = Color.red;
        [SerializeField] private Color _friendlyColor = Color.green;
        [SerializeField] private Color _neutralColor = Color.yellow;

        private ITargetSystem _targetSystem;
        private ICombatSystem _combatSystem;
        private IBuffSystem _buffSystem;
        private ulong _localPlayerId;
        private ulong _currentTargetId;

        // Buff/Debuff icon pools
        private readonly List<TargetBuffIcon> _buffIcons = new List<TargetBuffIcon>();
        private readonly List<TargetBuffIcon> _debuffIcons = new List<TargetBuffIcon>();

        private void Awake()
        {
            InitializeIconPools();
        }

        private void Update()
        {
            if (_targetSystem == null)
            {
                HideFrame();
                return;
            }

            if (_targetSystem.HasTarget)
            {
                UpdateFrame();
            }
            else
            {
                HideFrame();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromBuffEvents();
        }

        private void UpdateFrame()
        {
            if (_frameRoot != null)
                _frameRoot.SetActive(true);

            var target = _targetSystem.CurrentTarget;
            if (target == null) return;

            // Check if target changed
            ulong newTargetId = target.NetworkId;
            if (newTargetId != _currentTargetId)
            {
                _currentTargetId = newTargetId;
                RefreshBuffDebuffDisplay();
            }

            // Update name
            if (_nameText != null)
                _nameText.text = target.DisplayName;

            // Update health
            if (_combatSystem != null && _healthBar != null)
            {
                float health = _combatSystem.GetHealth(target.NetworkId);
                float maxHealth = _combatSystem.GetMaxHealth(target.NetworkId);
                _healthBar.value = maxHealth > 0 ? health / maxHealth : 0;

                if (_healthText != null)
                    _healthText.text = $"{health:F0} / {maxHealth:F0}";
            }

            // Update range indicator
            if (_rangeText != null)
            {
                if (_targetSystem.IsTargetInRange)
                {
                    _rangeText.text = $"{_targetSystem.TargetDistance:F0}m";
                    _rangeText.color = Color.white;
                }
                else
                {
                    _rangeText.text = "Out of Range";
                    _rangeText.color = Color.red;
                }
            }

            // Update target type color
            if (_targetTypeIcon != null)
            {
                _targetTypeIcon.color = target.Type switch
                {
                    Data.TargetType.Enemy => _enemyColor,
                    Data.TargetType.Friendly => _friendlyColor,
                    _ => _neutralColor
                };
            }

            // Update buff/debuff duration countdowns
            UpdateBuffDebuffDurations();
        }

        private void HideFrame()
        {
            if (_frameRoot != null)
                _frameRoot.SetActive(false);
            
            _currentTargetId = 0;
            ClearBuffDebuffDisplay();
        }

        /// <summary>
        /// Initialize with system references.
        /// </summary>
        public void Initialize(ITargetSystem targetSystem, ICombatSystem combatSystem)
        {
            _targetSystem = targetSystem;
            _combatSystem = combatSystem;
        }

        /// <summary>
        /// Initialize with system references including buff system.
        /// Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 9.6
        /// </summary>
        public void Initialize(ITargetSystem targetSystem, ICombatSystem combatSystem, IBuffSystem buffSystem, ulong localPlayerId)
        {
            _targetSystem = targetSystem;
            _combatSystem = combatSystem;
            _buffSystem = buffSystem;
            _localPlayerId = localPlayerId;

            SubscribeToBuffEvents();
        }

        /// <summary>
        /// Set the local player ID for highlighting player-applied debuffs.
        /// Requirements: 9.6
        /// </summary>
        public void SetLocalPlayerId(ulong localPlayerId)
        {
            _localPlayerId = localPlayerId;
            RefreshBuffDebuffDisplay();
        }

        #region Buff/Debuff Icon Management

        private void InitializeIconPools()
        {
            // Create buff icon pool
            if (_buffContainer != null && _buffIconPrefab != null)
            {
                for (int i = 0; i < _maxBuffIcons; i++)
                {
                    var iconObj = Instantiate(_buffIconPrefab, _buffContainer);
                    var iconInstance = CreateIconInstance(iconObj);
                    iconInstance.SetActive(false);
                    _buffIcons.Add(iconInstance);
                }
            }

            // Create debuff icon pool
            if (_debuffContainer != null && _buffIconPrefab != null)
            {
                for (int i = 0; i < _maxDebuffIcons; i++)
                {
                    var iconObj = Instantiate(_buffIconPrefab, _debuffContainer);
                    var iconInstance = CreateIconInstance(iconObj);
                    iconInstance.SetActive(false);
                    _debuffIcons.Add(iconInstance);
                }
            }
        }

        private TargetBuffIcon CreateIconInstance(GameObject iconObj)
        {
            return new TargetBuffIcon
            {
                GameObject = iconObj,
                IconImage = iconObj.GetComponentInChildren<Image>(),
                DurationText = iconObj.GetComponentInChildren<TMPro.TextMeshProUGUI>(),
                BorderImage = iconObj.transform.Find("Border")?.GetComponent<Image>(),
                StackText = iconObj.transform.Find("StackText")?.GetComponent<TMPro.TextMeshProUGUI>()
            };
        }

        /// <summary>
        /// Refresh the buff/debuff display for the current target.
        /// Requirements: 9.1, 9.2, 9.4, 9.5
        /// </summary>
        private void RefreshBuffDebuffDisplay()
        {
            ClearBuffDebuffDisplay();

            if (_buffSystem == null || _currentTargetId == 0) return;

            // Get and display buffs (Requirements 9.1)
            var buffs = _buffSystem.GetActiveBuffs(_currentTargetId);
            DisplayBuffs(buffs);

            // Get and display debuffs (Requirements 9.2)
            var debuffs = _buffSystem.GetActiveDebuffs(_currentTargetId);
            DisplayDebuffs(debuffs);
        }

        /// <summary>
        /// Display buffs on the target frame.
        /// Requirements: 9.1, 9.3
        /// </summary>
        private void DisplayBuffs(IReadOnlyList<BuffInstance> buffs)
        {
            if (buffs == null) return;

            int displayCount = Mathf.Min(buffs.Count, _maxBuffIcons);
            for (int i = 0; i < displayCount; i++)
            {
                if (i < _buffIcons.Count)
                {
                    SetupIcon(_buffIcons[i], buffs[i], true);
                }
            }
        }

        /// <summary>
        /// Display debuffs on the target frame.
        /// Requirements: 9.2, 9.3, 9.6
        /// </summary>
        private void DisplayDebuffs(IReadOnlyList<BuffInstance> debuffs)
        {
            if (debuffs == null) return;

            int displayCount = Mathf.Min(debuffs.Count, _maxDebuffIcons);
            for (int i = 0; i < displayCount; i++)
            {
                if (i < _debuffIcons.Count)
                {
                    SetupIcon(_debuffIcons[i], debuffs[i], false);
                }
            }
        }

        private void SetupIcon(TargetBuffIcon icon, BuffInstance buff, bool isBuff)
        {
            icon.BuffId = buff.BuffId;
            icon.RemainingDuration = buff.RemainingDuration;
            icon.SourceId = buff.SourceId;

            // Set icon sprite
            if (icon.IconImage != null)
            {
                icon.IconImage.sprite = buff.Data?.Icon ?? (isBuff ? _defaultBuffIcon : _defaultDebuffIcon);
            }

            // Set border color with player highlight for debuffs (Requirements 9.6)
            if (icon.BorderImage != null)
            {
                Color borderColor = isBuff ? _buffBorderColor : _debuffBorderColor;
                
                // Highlight debuffs applied by the local player
                if (!isBuff && buff.SourceId == _localPlayerId && _localPlayerId != 0)
                {
                    borderColor = _playerDebuffHighlight;
                }
                
                icon.BorderImage.color = borderColor;
            }

            // Set duration text (Requirements 9.3)
            UpdateIconDuration(icon);

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

        /// <summary>
        /// Update duration countdowns for all displayed buff/debuff icons.
        /// Requirements: 9.3
        /// </summary>
        private void UpdateBuffDebuffDurations()
        {
            float deltaTime = Time.deltaTime;

            foreach (var icon in _buffIcons)
            {
                if (icon.IsActive)
                {
                    icon.RemainingDuration -= deltaTime;
                    UpdateIconDuration(icon);
                }
            }

            foreach (var icon in _debuffIcons)
            {
                if (icon.IsActive)
                {
                    icon.RemainingDuration -= deltaTime;
                    UpdateIconDuration(icon);
                }
            }
        }

        private void UpdateIconDuration(TargetBuffIcon icon)
        {
            if (icon.DurationText == null) return;

            float duration = icon.RemainingDuration;
            
            if (duration <= 0)
            {
                icon.DurationText.text = "";
            }
            else if (duration < 60)
            {
                icon.DurationText.text = $"{Mathf.CeilToInt(duration)}";
            }
            else
            {
                int minutes = Mathf.FloorToInt(duration / 60);
                icon.DurationText.text = $"{minutes}m";
            }
        }

        private void ClearBuffDebuffDisplay()
        {
            foreach (var icon in _buffIcons)
            {
                icon.SetActive(false);
                icon.BuffId = null;
            }

            foreach (var icon in _debuffIcons)
            {
                icon.SetActive(false);
                icon.BuffId = null;
            }
        }

        #endregion

        #region Buff System Event Handlers

        private void SubscribeToBuffEvents()
        {
            if (_buffSystem == null) return;

            _buffSystem.OnBuffApplied += HandleBuffApplied;
            _buffSystem.OnBuffExpired += HandleBuffExpired;
            _buffSystem.OnDebuffApplied += HandleDebuffApplied;
            _buffSystem.OnDebuffExpired += HandleDebuffExpired;
        }

        private void UnsubscribeFromBuffEvents()
        {
            if (_buffSystem == null) return;

            _buffSystem.OnBuffApplied -= HandleBuffApplied;
            _buffSystem.OnBuffExpired -= HandleBuffExpired;
            _buffSystem.OnDebuffApplied -= HandleDebuffApplied;
            _buffSystem.OnDebuffExpired -= HandleDebuffExpired;
        }

        /// <summary>
        /// Handle buff applied event - update display immediately.
        /// Requirements: 9.4
        /// </summary>
        private void HandleBuffApplied(ulong targetId, BuffInstance buff)
        {
            if (targetId != _currentTargetId) return;
            RefreshBuffDebuffDisplay();
        }

        /// <summary>
        /// Handle buff expired event - remove icon immediately.
        /// Requirements: 9.5
        /// </summary>
        private void HandleBuffExpired(ulong targetId, BuffInstance buff)
        {
            if (targetId != _currentTargetId) return;
            RefreshBuffDebuffDisplay();
        }

        /// <summary>
        /// Handle debuff applied event - update display immediately.
        /// Requirements: 9.4
        /// </summary>
        private void HandleDebuffApplied(ulong targetId, BuffInstance debuff)
        {
            if (targetId != _currentTargetId) return;
            RefreshBuffDebuffDisplay();
        }

        /// <summary>
        /// Handle debuff expired event - remove icon immediately.
        /// Requirements: 9.5
        /// </summary>
        private void HandleDebuffExpired(ulong targetId, BuffInstance debuff)
        {
            if (targetId != _currentTargetId) return;
            RefreshBuffDebuffDisplay();
        }

        #endregion

        #region Nested Types

        /// <summary>
        /// Represents a buff/debuff icon in the target frame.
        /// </summary>
        private class TargetBuffIcon
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
