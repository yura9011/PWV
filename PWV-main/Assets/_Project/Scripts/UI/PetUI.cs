using System;
using EtherDomes.Combat;
using EtherDomes.Data;
using UnityEngine;
using UnityEngine.UI;

namespace EtherDomes.UI
{
    /// <summary>
    /// UI component for displaying pet information.
    /// Shows pet health bar near the player frame.
    /// Requirements: 3.8
    /// </summary>
    public class PetUI : MonoBehaviour, IPetUI
    {
        #region Serialized Fields

        [Header("UI Elements")]
        [SerializeField] private GameObject _frameRoot;
        [SerializeField] private TMPro.TextMeshProUGUI _petNameText;
        [SerializeField] private Slider _healthBar;
        [SerializeField] private TMPro.TextMeshProUGUI _healthText;
        [SerializeField] private Image _healthBarFill;
        [SerializeField] private TMPro.TextMeshProUGUI _stateText;

        [Header("Command Buttons")]
        [SerializeField] private Button _attackButton;
        [SerializeField] private Button _followButton;
        [SerializeField] private Button _frameButton;

        [Header("Colors")]
        [SerializeField] private Color _healthyColor = new Color(0.2f, 0.8f, 0.2f, 1f);
        [SerializeField] private Color _damagedColor = new Color(0.8f, 0.8f, 0.2f, 1f);
        [SerializeField] private Color _criticalColor = new Color(0.8f, 0.2f, 0.2f, 1f);
        [SerializeField] private float _damagedThreshold = 0.5f;
        [SerializeField] private float _criticalThreshold = 0.25f;

        [Header("State Colors")]
        [SerializeField] private Color _followingColor = Color.white;
        [SerializeField] private Color _attackingColor = new Color(1f, 0.5f, 0.5f, 1f);
        [SerializeField] private Color _deadColor = Color.gray;

        #endregion

        #region Private Fields

        private ulong _ownerId;
        private IPetSystem _petSystem;
        private float _currentHealth;
        private float _maxHealth;
        private PetState _currentState;

        #endregion

        #region Events

        public event Action OnPetFrameClicked;
        public event Action OnAttackCommandClicked;
        public event Action OnFollowCommandClicked;

        #endregion

        #region Properties

        public bool IsVisible => _frameRoot != null && _frameRoot.activeSelf;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            SetupButtonListeners();
            Hide();
        }

        private void Update()
        {
            if (_petSystem != null && _ownerId != 0)
            {
                UpdateFromPetSystem();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the pet UI with the owner's entity ID.
        /// </summary>
        public void Initialize(ulong ownerId)
        {
            _ownerId = ownerId;
        }

        /// <summary>
        /// Initialize with system references.
        /// </summary>
        public void Initialize(IPetSystem petSystem, ulong ownerId)
        {
            _petSystem = petSystem;
            _ownerId = ownerId;

            SubscribeToEvents();
            
            // Check if pet already exists
            if (_petSystem.HasPet(ownerId))
            {
                var pet = _petSystem.GetPet(ownerId);
                if (pet != null)
                {
                    UpdatePetName(pet.Data?.DisplayName ?? "Pet");
                    UpdateHealth(pet.CurrentHealth, pet.MaxHealth);
                    UpdatePetState(pet.State);
                    Show();
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Update the pet health display.
        /// </summary>
        public void UpdateHealth(float currentHealth, float maxHealth)
        {
            _currentHealth = currentHealth;
            _maxHealth = maxHealth;

            if (_healthBar != null)
            {
                _healthBar.value = maxHealth > 0 ? currentHealth / maxHealth : 0;
            }

            if (_healthText != null)
            {
                _healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
            }

            UpdateHealthBarColor();
        }

        /// <summary>
        /// Update the pet name display.
        /// </summary>
        public void UpdatePetName(string petName)
        {
            if (_petNameText != null)
            {
                _petNameText.text = petName ?? "Pet";
            }
        }

        /// <summary>
        /// Update the pet state display.
        /// </summary>
        public void UpdatePetState(PetState state)
        {
            _currentState = state;

            if (_stateText != null)
            {
                _stateText.text = GetStateDisplayText(state);
                _stateText.color = GetStateColor(state);
            }

            // Update button states
            UpdateCommandButtons(state);
        }

        /// <summary>
        /// Show the pet UI.
        /// </summary>
        public void Show()
        {
            if (_frameRoot != null)
            {
                _frameRoot.SetActive(true);
            }
        }

        /// <summary>
        /// Hide the pet UI.
        /// </summary>
        public void Hide()
        {
            if (_frameRoot != null)
            {
                _frameRoot.SetActive(false);
            }
        }

        #endregion

        #region Private Methods

        private void SetupButtonListeners()
        {
            if (_attackButton != null)
            {
                _attackButton.onClick.AddListener(HandleAttackButtonClicked);
            }

            if (_followButton != null)
            {
                _followButton.onClick.AddListener(HandleFollowButtonClicked);
            }

            if (_frameButton != null)
            {
                _frameButton.onClick.AddListener(HandleFrameClicked);
            }
        }

        private void UpdateFromPetSystem()
        {
            if (!_petSystem.HasPet(_ownerId))
            {
                if (IsVisible)
                {
                    Hide();
                }
                return;
            }

            var pet = _petSystem.GetPet(_ownerId);
            if (pet == null) return;

            UpdateHealth(pet.CurrentHealth, pet.MaxHealth);
            
            if (pet.State != _currentState)
            {
                UpdatePetState(pet.State);
            }
        }

        private void UpdateHealthBarColor()
        {
            if (_healthBarFill == null) return;

            float healthPercent = _maxHealth > 0 ? _currentHealth / _maxHealth : 0;

            Color targetColor;
            if (healthPercent <= _criticalThreshold)
            {
                targetColor = _criticalColor;
            }
            else if (healthPercent <= _damagedThreshold)
            {
                targetColor = _damagedColor;
            }
            else
            {
                targetColor = _healthyColor;
            }

            _healthBarFill.color = targetColor;
        }

        private void UpdateCommandButtons(PetState state)
        {
            bool canCommand = state == PetState.Following || state == PetState.Attacking;

            if (_attackButton != null)
            {
                _attackButton.interactable = canCommand;
            }

            if (_followButton != null)
            {
                _followButton.interactable = canCommand && state == PetState.Attacking;
            }
        }

        private string GetStateDisplayText(PetState state)
        {
            return state switch
            {
                PetState.Following => "Following",
                PetState.Attacking => "Attacking",
                PetState.Dead => "Dead",
                PetState.Dismissed => "Dismissed",
                _ => ""
            };
        }

        private Color GetStateColor(PetState state)
        {
            return state switch
            {
                PetState.Following => _followingColor,
                PetState.Attacking => _attackingColor,
                PetState.Dead => _deadColor,
                PetState.Dismissed => _deadColor,
                _ => Color.white
            };
        }

        private void HandleAttackButtonClicked()
        {
            OnAttackCommandClicked?.Invoke();
        }

        private void HandleFollowButtonClicked()
        {
            OnFollowCommandClicked?.Invoke();
        }

        private void HandleFrameClicked()
        {
            OnPetFrameClicked?.Invoke();
        }

        #endregion

        #region Event Subscriptions

        private void SubscribeToEvents()
        {
            if (_petSystem == null) return;

            _petSystem.OnPetSummoned += HandlePetSummoned;
            _petSystem.OnPetDismissed += HandlePetDismissed;
            _petSystem.OnPetDied += HandlePetDied;
            _petSystem.OnPetStateChanged += HandlePetStateChanged;
        }

        private void UnsubscribeFromEvents()
        {
            if (_petSystem == null) return;

            _petSystem.OnPetSummoned -= HandlePetSummoned;
            _petSystem.OnPetDismissed -= HandlePetDismissed;
            _petSystem.OnPetDied -= HandlePetDied;
            _petSystem.OnPetStateChanged -= HandlePetStateChanged;
        }

        private void HandlePetSummoned(ulong ownerId, PetInstance pet)
        {
            if (ownerId != _ownerId) return;

            UpdatePetName(pet.Data?.DisplayName ?? "Pet");
            UpdateHealth(pet.CurrentHealth, pet.MaxHealth);
            UpdatePetState(pet.State);
            Show();
        }

        private void HandlePetDismissed(ulong ownerId)
        {
            if (ownerId != _ownerId) return;
            Hide();
        }

        private void HandlePetDied(ulong ownerId)
        {
            if (ownerId != _ownerId) return;
            UpdatePetState(PetState.Dead);
            // Keep showing for a moment so player sees the death
        }

        private void HandlePetStateChanged(ulong ownerId, PetState newState)
        {
            if (ownerId != _ownerId) return;
            UpdatePetState(newState);
        }

        #endregion
    }
}
