using System;
using System.Collections.Generic;
using EtherDomes.Data;

namespace EtherDomes.UI
{
    /// <summary>
    /// Interface for the character selection UI.
    /// Requirements: 17.1, 17.2
    /// </summary>
    public interface ICharacterSelectUI
    {
        /// <summary>
        /// Shows the character selection UI.
        /// </summary>
        void Show();
        
        /// <summary>
        /// Hides the character selection UI.
        /// </summary>
        void Hide();
        
        /// <summary>
        /// Refreshes the list of available characters.
        /// </summary>
        void RefreshCharacterList();
        
        /// <summary>
        /// Shows the character creation panel.
        /// </summary>
        void ShowCreationPanel();
        
        /// <summary>
        /// Hides the character creation panel.
        /// </summary>
        void HideCreationPanel();
        
        /// <summary>
        /// Gets whether the UI is currently visible.
        /// </summary>
        bool IsVisible { get; }
        
        /// <summary>
        /// Gets the currently selected character data, or null if none selected.
        /// </summary>
        CharacterData SelectedCharacter { get; }
        
        /// <summary>
        /// Event fired when a character is selected from the list.
        /// </summary>
        event Action<CharacterData> OnCharacterSelected;
        
        /// <summary>
        /// Event fired when the Play button is clicked.
        /// </summary>
        event Action<CharacterData> OnPlayClicked;
        
        /// <summary>
        /// Event fired when the Delete button is clicked.
        /// </summary>
        event Action<CharacterData> OnDeleteClicked;
        
        /// <summary>
        /// Event fired when a new character is created.
        /// </summary>
        event Action<string, CharacterClass> OnCharacterCreated;
    }
}
