using EtherDomes.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace EtherDomes.UI
{
    /// <summary>
    /// UI component for displaying target information.
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

        [Header("Colors")]
        [SerializeField] private Color _enemyColor = Color.red;
        [SerializeField] private Color _friendlyColor = Color.green;
        [SerializeField] private Color _neutralColor = Color.yellow;

        private ITargetSystem _targetSystem;
        private ICombatSystem _combatSystem;

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

        private void UpdateFrame()
        {
            if (_frameRoot != null)
                _frameRoot.SetActive(true);

            var target = _targetSystem.CurrentTarget;
            if (target == null) return;

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
        }

        private void HideFrame()
        {
            if (_frameRoot != null)
                _frameRoot.SetActive(false);
        }

        /// <summary>
        /// Initialize with system references.
        /// </summary>
        public void Initialize(ITargetSystem targetSystem, ICombatSystem combatSystem)
        {
            _targetSystem = targetSystem;
            _combatSystem = combatSystem;
        }
    }
}
