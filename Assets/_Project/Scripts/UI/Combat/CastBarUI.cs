using UnityEngine;
using UnityEngine.UI;
using EtherDomes.Combat;
using EtherDomes.Combat.Abilities;
using EtherDomes.Data;

namespace EtherDomes.UI.Combat
{
    /// <summary>
    /// UI component displaying cast progress bar.
    /// Shows during Casting state with ability name and progress.
    /// </summary>
    public class CastBarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CombatStateMachine _stateMachine;

        [Header("UI Elements")]
        [SerializeField] private GameObject _castBarContainer;
        [SerializeField] private Image _progressFill;
        [SerializeField] private Text _abilityNameText;
        [SerializeField] private Text _castTimeText;
        [SerializeField] private Image _abilityIcon;

        [Header("Animation")]
        [SerializeField] private float _fadeSpeed = 5f;
        [SerializeField] private CanvasGroup _canvasGroup;

        private bool _isShowing;
        private float _targetAlpha;

        private void OnEnable()
        {
            if (_stateMachine != null)
            {
                _stateMachine.OnStateChanged += HandleStateChanged;
                _stateMachine.OnCastProgress += HandleCastProgress;
                _stateMachine.OnCastInterrupted += HandleCastInterrupted;
            }

            Hide();
        }

        private void OnDisable()
        {
            if (_stateMachine != null)
            {
                _stateMachine.OnStateChanged -= HandleStateChanged;
                _stateMachine.OnCastProgress -= HandleCastProgress;
                _stateMachine.OnCastInterrupted -= HandleCastInterrupted;
            }
        }

        private void Update()
        {
            // Smooth fade animation
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, _targetAlpha, Time.deltaTime * _fadeSpeed);
                
                if (_canvasGroup.alpha < 0.01f && !_isShowing)
                {
                    if (_castBarContainer != null)
                        _castBarContainer.SetActive(false);
                }
            }
        }

        private void HandleStateChanged(CombatState previousState, CombatState newState)
        {
            if (newState == CombatState.Casting)
            {
                Show();
            }
            else if (previousState == CombatState.Casting)
            {
                Hide();
            }
        }

        private void HandleCastProgress(float progress, float totalDuration)
        {
            if (_progressFill != null)
                _progressFill.fillAmount = progress;

            if (_castTimeText != null)
            {
                float remaining = totalDuration * (1f - progress);
                _castTimeText.text = $"{remaining:F1}s";
            }
        }

        private void HandleCastInterrupted(ScriptableObject abilityObj)
        {
            // Flash red on interrupt
            if (_progressFill != null)
                _progressFill.color = Color.red;

            Hide();
        }

        private void Show()
        {
            _isShowing = true;
            _targetAlpha = 1f;

            if (_castBarContainer != null)
                _castBarContainer.SetActive(true);

            // Reset fill color
            if (_progressFill != null)
                _progressFill.color = Color.yellow;

            // Update ability info - cast to AbilityDefinitionSO
            var abilityObj = _stateMachine?.CurrentCastingAbility;
            var ability = abilityObj as AbilityDefinitionSO;
            if (ability != null)
            {
                if (_abilityNameText != null)
                    _abilityNameText.text = ability.AbilityName;

                if (_abilityIcon != null && ability.Icon != null)
                    _abilityIcon.sprite = ability.Icon;
            }
        }

        private void Hide()
        {
            _isShowing = false;
            _targetAlpha = 0f;
        }

        /// <summary>
        /// Sets the state machine reference.
        /// </summary>
        public void SetStateMachine(CombatStateMachine stateMachine)
        {
            if (_stateMachine != null)
            {
                _stateMachine.OnStateChanged -= HandleStateChanged;
                _stateMachine.OnCastProgress -= HandleCastProgress;
                _stateMachine.OnCastInterrupted -= HandleCastInterrupted;
            }

            _stateMachine = stateMachine;

            if (_stateMachine != null)
            {
                _stateMachine.OnStateChanged += HandleStateChanged;
                _stateMachine.OnCastProgress += HandleCastProgress;
                _stateMachine.OnCastInterrupted += HandleCastInterrupted;
            }
        }
    }
}
