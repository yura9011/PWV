using UnityEngine;
using UnityEngine.UI;
using EtherDomes.Combat.Targeting;
using EtherDomes.Combat;

namespace EtherDomes.UI.Combat
{
    /// <summary>
    /// UI component displaying current target information.
    /// Shows target name and health bar.
    /// </summary>
    public class TargetFrameUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TargetingSystem _targetingSystem;

        [Header("UI Elements")]
        [SerializeField] private GameObject _frameContainer;
        [SerializeField] private Text _nameText;
        [SerializeField] private Image _healthBarFill;
        [SerializeField] private Text _healthText;

        [Header("Colors")]
        [SerializeField] private Color _enemyColor = Color.red;
        [SerializeField] private Color _friendlyColor = Color.green;
        [SerializeField] private Color _neutralColor = Color.yellow;

        private ITargetable _currentTarget;

        private void OnEnable()
        {
            if (_targetingSystem != null)
            {
                _targetingSystem.OnTargetChanged += HandleTargetChanged;
            }
            
            UpdateUI();
        }

        private void OnDisable()
        {
            if (_targetingSystem != null)
            {
                _targetingSystem.OnTargetChanged -= HandleTargetChanged;
            }

            UnsubscribeFromTarget();
        }

        private void Update()
        {
            // Update health bar smoothly
            if (_currentTarget != null && _healthBarFill != null)
            {
                float targetFill = _currentTarget.MaxHealth > 0 
                    ? _currentTarget.CurrentHealth / _currentTarget.MaxHealth 
                    : 0f;
                _healthBarFill.fillAmount = Mathf.Lerp(_healthBarFill.fillAmount, targetFill, Time.deltaTime * 10f);
                
                if (_healthText != null)
                {
                    _healthText.text = $"{Mathf.CeilToInt(_currentTarget.CurrentHealth)} / {Mathf.CeilToInt(_currentTarget.MaxHealth)}";
                }
            }
        }

        private void HandleTargetChanged(ITargetable newTarget)
        {
            UnsubscribeFromTarget();
            _currentTarget = newTarget;
            SubscribeToTarget();
            UpdateUI();
        }

        private void SubscribeToTarget()
        {
            if (_currentTarget != null)
            {
                _currentTarget.OnDeath += HandleTargetDeath;
            }
        }

        private void UnsubscribeFromTarget()
        {
            if (_currentTarget != null)
            {
                _currentTarget.OnDeath -= HandleTargetDeath;
            }
        }

        private void HandleTargetDeath(ITargetable target)
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            bool hasTarget = _currentTarget != null && _currentTarget.IsAlive;

            if (_frameContainer != null)
                _frameContainer.SetActive(hasTarget);

            if (!hasTarget) return;

            if (_nameText != null)
                _nameText.text = _currentTarget.DisplayName;

            if (_healthBarFill != null)
            {
                float healthPercent = _currentTarget.MaxHealth > 0 
                    ? _currentTarget.CurrentHealth / _currentTarget.MaxHealth 
                    : 0f;
                _healthBarFill.fillAmount = healthPercent;
                _healthBarFill.color = _enemyColor; // TODO: Determine target type
            }
        }

        /// <summary>
        /// Sets the targeting system reference.
        /// </summary>
        public void SetTargetingSystem(TargetingSystem system)
        {
            if (_targetingSystem != null)
                _targetingSystem.OnTargetChanged -= HandleTargetChanged;

            _targetingSystem = system;

            if (_targetingSystem != null)
                _targetingSystem.OnTargetChanged += HandleTargetChanged;

            UpdateUI();
        }
    }
}
