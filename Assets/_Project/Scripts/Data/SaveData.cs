using System;
using System.Collections.Generic;

namespace EtherDomes.Data
{
    /// <summary>
    /// Root object for the save file.
    /// This is what gets serialized to JSON and encrypted.
    /// </summary>
    [Serializable]
    public class SaveFile
    {
        public string AccountID;       // Unique ID for the player account (e.g. SteamID)
        public string LastPlayedCharacterId; // For auto-loading last char
        public List<CharacterData> Characters;

        public SaveFile()
        {
            Characters = new List<CharacterData>();
            AccountID = Guid.NewGuid().ToString();
        }
    }
}
