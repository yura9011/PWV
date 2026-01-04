using EtherDomes.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace EtherDomes.UI
{
    /// <summary>
    /// UI component for displaying cast progress.
    /// </summary>
    public class CastBar : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject _barRoot;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TMPro.TextMeshProUGUI _abilityNameText;
        [SerializeField] private TMPro.TextMeshProUGUI _castTimeText;
        [SerializeField] private Image _abilityIcon;

        private IAbilitySystem _abilitySystem;

        private void Update()
        {
            if (_abilitySystem == null)
            {
                HideBar();
                return;
            }

            if (_abilitySystem.IsCasting)
            {
                UpdateBar();
            }
            else
            {
                HideBar();
            }
        }

        private void UpdateBar()
        {
            if (_barRoot != null)
                _barRoot.SetActive(true);

            var ability = _abilitySystem.CurrentCastAbility;
            if (ability == null) return;

            // Update ability info
            if (_abilityNameText != null)
                _abilityNameText.text = ability.AbilityName;

            if (_abilityIcon != null && ability.Icon != null)
            {
                _abilityIcon.sprite = ability.Icon;
                _abilityIcon.enabled = true;
            }

            // Update progress
            float progress = _abilitySystem.CastProgress;
            if (_progressBar != null)
                _progressBar.value = progress;

            // Update time remaining
            if (_castTimeText != null)
            {
                float remaining = ability.CastTime * (1f - progress);
                _castTimeText.text = $"{remaining:F1}s";
            }
        }

        private void HideBar()
        {
            if (_barRoot != null)
                _barRoot.SetActive(false);
        }

        /// <summary>
        /// Initialize with ability system reference.
        /// </summary>
        public void Initialize(IAbilitySystem abilitySystem)
        {
            _abilitySystem = abilitySystem;
        }
    }
}
