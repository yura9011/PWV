using UnityEngine;

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
    }
}
