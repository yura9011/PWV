using EtherDomes.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace EtherDomes.UI
{
    /// <summary>
    /// UI component for displaying cast and channel progress.
    /// Channels display as depleting bars (1 to 0) while casts display as filling bars (0 to 1).
    /// Requirements: 2.6
    /// </summary>
    public class CastBar : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject _barRoot;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TMPro.TextMeshProUGUI _abilityNameText;
        [SerializeField] private TMPro.TextMeshProUGUI _castTimeText;
        [SerializeField] private Image _abilityIcon;
        
        [Header("Channel Display")]
        [SerializeField] private Color _castBarColor = new Color(1f, 0.8f, 0f, 1f);    // Yellow for casts
        [SerializeField] private Color _channelBarColor = new Color(0.5f, 0.8f, 1f, 1f); // Blue for channels
        [SerializeField] private Image _fillImage;

        private IAbilitySystem _abilitySystem;
        private bool _isShowingChannel;

        private void Update()
        {
            if (_abilitySystem == null)
            {
                HideBar();
                return;
            }

            if (_abilitySystem.IsChanneling)
            {
                UpdateChannelBar();
            }
            else if (_abilitySystem.IsCasting)
            {
                UpdateCastBar();
            }
            else
            {
                HideBar();
            }
        }

        /// <summary>
        /// Updates the bar for regular casts (fills from 0 to 1).
        /// </summary>
        private void UpdateCastBar()
        {
            ShowBar(false);

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

            // Update progress (filling)
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

        /// <summary>
        /// Updates the bar for channeled abilities (depletes from 1 to 0).
        /// Requirements: 2.6
        /// </summary>
        private void UpdateChannelBar()
        {
            ShowBar(true);

            var ability = _abilitySystem.CurrentChannelAbility;
            if (ability == null) return;

            // Update ability info
            if (_abilityNameText != null)
                _abilityNameText.text = ability.AbilityName;

            if (_abilityIcon != null && ability.Icon != null)
            {
                _abilityIcon.sprite = ability.Icon;
                _abilityIcon.enabled = true;
            }

            // Update progress (depleting - starts at 1, goes to 0)
            float progress = _abilitySystem.ChannelProgress;
            if (_progressBar != null)
                _progressBar.value = progress;

            // Update time remaining
            if (_castTimeText != null)
            {
                float remaining = ability.ChannelDuration * progress;
                int ticksRemaining = ability.TotalTicks - _abilitySystem.ChannelTicksCompleted;
                _castTimeText.text = $"{remaining:F1}s ({ticksRemaining} ticks)";
            }
        }

        /// <summary>
        /// Shows the bar with appropriate color for cast or channel.
        /// </summary>
        private void ShowBar(bool isChannel)
        {
            if (_barRoot != null)
                _barRoot.SetActive(true);

            // Update color if changed
            if (_isShowingChannel != isChannel && _fillImage != null)
            {
                _fillImage.color = isChannel ? _channelBarColor : _castBarColor;
                _isShowingChannel = isChannel;
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
