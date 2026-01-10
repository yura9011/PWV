using UnityEngine;
using EtherDomes.Data;

namespace EtherDomes.Core
{
    /// <summary>
    /// Static class to store player's class selection between scenes.
    /// Persists across scene transitions without DontDestroyOnLoad.
    /// </summary>
    public static class ClassSelectionData
    {
        private static PlayerClass _selectedClass = PlayerClass.Guerrero;
        
        /// <summary>
        /// Currently selected player class.
        /// </summary>
        public static PlayerClass SelectedClass
        {
            get => _selectedClass;
            set => _selectedClass = value;
        }
        
        /// <summary>
        /// Gets the color associated with the selected class.
        /// </summary>
        public static Color GetClassColor()
        {
            return _selectedClass == PlayerClass.Guerrero ? Color.red : Color.blue;
        }
        
        /// <summary>
        /// Gets the color for a specific class.
        /// </summary>
        public static Color GetColorForClass(PlayerClass playerClass)
        {
            return playerClass == PlayerClass.Guerrero ? Color.red : Color.blue;
        }
        
        /// <summary>
        /// Gets the class ID as int for network transmission.
        /// </summary>
        public static int GetClassID()
        {
            return (int)_selectedClass;
        }

        /// <summary>
        /// Converts the selected PlayerClass to the full CharacterClass enum.
        /// Used for ClassSystem integration.
        /// </summary>
        public static CharacterClass ToCharacterClass()
        {
            return _selectedClass switch
            {
                PlayerClass.Guerrero => CharacterClass.Warrior,
                PlayerClass.Mago => CharacterClass.Mage,
                _ => CharacterClass.Warrior
            };
        }

        /// <summary>
        /// Sets the selected class from a CharacterClass enum.
        /// Used when loading saved character data.
        /// </summary>
        public static void SetFromCharacterClass(CharacterClass charClass)
        {
            _selectedClass = charClass switch
            {
                CharacterClass.Warrior or CharacterClass.Paladin or 
                CharacterClass.Rogue or CharacterClass.DeathKnight => PlayerClass.Guerrero, // Melee = Red
                CharacterClass.Mage or CharacterClass.Priest or 
                CharacterClass.Hunter or CharacterClass.Warlock => PlayerClass.Mago, // Ranged/Casters = Blue
                _ => PlayerClass.Guerrero
            };
        }

        /// <summary>
        /// Gets the default specialization for the selected class.
        /// </summary>
        public static Specialization GetDefaultSpecialization()
        {
            return _selectedClass switch
            {
                PlayerClass.Guerrero => Specialization.Arms,
                PlayerClass.Mago => Specialization.Fire,
                _ => Specialization.Arms
            };
        }
    }
}
