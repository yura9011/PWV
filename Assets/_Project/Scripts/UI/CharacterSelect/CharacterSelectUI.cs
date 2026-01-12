using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EtherDomes.Data;
using EtherDomes.Persistence;

namespace EtherDomes.UI
{
    /// <summary>
    /// Character selection UI implementation.
    /// Requirements: 17.1, 17.2, 17.3, 17.4, 17.5
    /// </summary>
    public class CharacterSelectUI : MonoBehaviour, ICharacterSelectUI
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private Transform _characterListContainer;
        [SerializeField] private GameObject _characterSlotPrefab;
        
        [Header("Buttons")]
        [SerializeField] private Button _newCharacterButton;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _deleteButton;
        
        [Header("Creation Panel")]
        [SerializeField] private GameObject _creationPanel;
        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private TMP_Dropdown _classDropdown;
        [SerializeField] private Button _createButton;
        [SerializeField] private Button _cancelButton;
        
        [Header("Selected Character Display")]
        [SerializeField] private TextMeshProUGUI _selectedNameText;
        [SerializeField] private TextMeshProUGUI _selectedClassText;
        [SerializeField] private TextMeshProUGUI _selectedLevelText;
        
        // State
        private List<CharacterData> _characters = new();
        private CharacterData _selectedCharacter;
        private List<GameObject> _characterSlots = new();
        
        // Events
        public event Action<CharacterData> OnCharacterSelected;
        public event Action<CharacterData> OnPlayClicked;
        public event Action<CharacterData> OnDeleteClicked;
        public event Action<string, CharacterClass> OnCharacterCreated;
        
        // Properties
        public bool IsVisible => _mainPanel != null && _mainPanel.activeSelf;
        public CharacterData SelectedCharacter => _selectedCharacter;
        
        private void Awake()
        {
            SetupButtons();
            SetupClassDropdown();
        }
        
        private void Start()
        {
            Hide();
            HideCreationPanel();
        }
        
        private void SetupButtons()
        {
            if (_newCharacterButton != null)
                _newCharacterButton.onClick.AddListener(ShowCreationPanel);
            
            if (_playButton != null)
                _playButton.onClick.AddListener(OnPlayButtonClicked);
            
            if (_deleteButton != null)
                _deleteButton.onClick.AddListener(OnDeleteButtonClicked);
            
            if (_createButton != null)
                _createButton.onClick.AddListener(OnCreateButtonClicked);
            
            if (_cancelButton != null)
                _cancelButton.onClick.AddListener(HideCreationPanel);
            
            UpdateButtonStates();
        }
        
        private void SetupClassDropdown()
        {
            if (_classDropdown == null) return;
            
            _classDropdown.ClearOptions();
            var options = new List<string>
            {
                "Warrior",
                "Mage",
                "Priest",
                "Paladin"
            };
            _classDropdown.AddOptions(options);
        }

        public void Show()
        {
            if (_mainPanel != null)
            {
                _mainPanel.SetActive(true);
            }
            RefreshCharacterList();
        }
        
        public void Hide()
        {
            if (_mainPanel != null)
            {
                _mainPanel.SetActive(false);
            }
            HideCreationPanel();
        }
        
        public void RefreshCharacterList()
        {
            ClearCharacterSlots();
            
            // In production, this would load from CharacterPersistenceService
            foreach (var character in _characters)
            {
                CreateCharacterSlot(character);
            }
            
            UpdateButtonStates();
        }
        
        public void ShowCreationPanel()
        {
            if (_creationPanel != null)
            {
                _creationPanel.SetActive(true);
            }
            
            if (_nameInput != null)
            {
                _nameInput.text = "";
                _nameInput.Select();
            }
            
            if (_classDropdown != null)
            {
                _classDropdown.value = 0;
            }
        }
        
        public void HideCreationPanel()
        {
            if (_creationPanel != null)
            {
                _creationPanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// Sets the list of characters to display.
        /// </summary>
        public void SetCharacters(List<CharacterData> characters)
        {
            _characters = characters ?? new List<CharacterData>();
            RefreshCharacterList();
        }
        
        private void ClearCharacterSlots()
        {
            foreach (var slot in _characterSlots)
            {
                if (slot != null)
                {
                    Destroy(slot);
                }
            }
            _characterSlots.Clear();
        }
        
        private void CreateCharacterSlot(CharacterData character)
        {
            if (_characterSlotPrefab == null || _characterListContainer == null)
            {
                UnityEngine.Debug.LogWarning("[CharacterSelectUI] Missing prefab or container");
                return;
            }
            
            var slot = Instantiate(_characterSlotPrefab, _characterListContainer);
            _characterSlots.Add(slot);
            
            // Setup slot display
            var nameText = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = $"{character.Name} - {character.ClassDisplayName} Lv.{character.Level}";
            }
            
            // Setup click handler
            var button = slot.GetComponent<Button>();
            if (button != null)
            {
                var capturedCharacter = character;
                button.onClick.AddListener(() => SelectCharacter(capturedCharacter));
            }
        }
        
        private void SelectCharacter(CharacterData character)
        {
            _selectedCharacter = character;
            UpdateSelectedDisplay();
            UpdateButtonStates();
            OnCharacterSelected?.Invoke(character);
        }
        
        private void UpdateSelectedDisplay()
        {
            if (_selectedCharacter == null)
            {
                if (_selectedNameText != null) _selectedNameText.text = "No character selected";
                if (_selectedClassText != null) _selectedClassText.text = "";
                if (_selectedLevelText != null) _selectedLevelText.text = "";
            }
            else
            {
                if (_selectedNameText != null) _selectedNameText.text = _selectedCharacter.Name;
                if (_selectedClassText != null) _selectedClassText.text = _selectedCharacter.ClassDisplayName;
                if (_selectedLevelText != null) _selectedLevelText.text = $"Level {_selectedCharacter.Level}";
            }
        }
        
        private void UpdateButtonStates()
        {
            bool hasSelection = _selectedCharacter != null;
            
            if (_playButton != null)
                _playButton.interactable = hasSelection;
            
            if (_deleteButton != null)
                _deleteButton.interactable = hasSelection;
        }
        
        private void OnPlayButtonClicked()
        {
            if (_selectedCharacter != null)
            {
                OnPlayClicked?.Invoke(_selectedCharacter);
            }
        }
        
        private void OnDeleteButtonClicked()
        {
            if (_selectedCharacter != null)
            {
                OnDeleteClicked?.Invoke(_selectedCharacter);
            }
        }
        
        private void OnCreateButtonClicked()
        {
            if (_nameInput == null || _classDropdown == null) return;
            
            string characterName = _nameInput.text.Trim();
            if (string.IsNullOrEmpty(characterName))
            {
                UnityEngine.Debug.LogWarning("[CharacterSelectUI] Character name cannot be empty");
                return;
            }
            
            CharacterClass selectedClass = (CharacterClass)_classDropdown.value;
            
            OnCharacterCreated?.Invoke(characterName, selectedClass);
            HideCreationPanel();
        }
        
        private void OnDestroy()
        {
            // Cleanup listeners
            if (_newCharacterButton != null)
                _newCharacterButton.onClick.RemoveAllListeners();
            
            if (_playButton != null)
                _playButton.onClick.RemoveAllListeners();
            
            if (_deleteButton != null)
                _deleteButton.onClick.RemoveAllListeners();
            
            if (_createButton != null)
                _createButton.onClick.RemoveAllListeners();
            
            if (_cancelButton != null)
                _cancelButton.onClick.RemoveAllListeners();
        }
    }
}
