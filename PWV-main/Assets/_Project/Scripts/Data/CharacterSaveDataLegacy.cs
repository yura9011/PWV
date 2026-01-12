using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace EtherDomes.Data
{
    [Serializable]
    public class CharacterSaveDataList
    {
        public const int MAX_CHARACTERS = 12;
        public List<CharacterSaveData> Characters = new List<CharacterSaveData>();
        public bool CanCreateMore => Characters.Count < MAX_CHARACTERS;
        public int Count => Characters.Count;
    }

    [Serializable]
    public class CharacterSaveData
    {
        public string CharacterId;
        public string CharacterName;
        public int Level;
        public CharacterClass Class;
        public DateTime CreatedAt;
        public DateTime LastPlayedAt;
        public int CurrentHealth;
        public int MaxHealth;
        public int CurrentMana;
        public int MaxMana;
        public float LastPosX, LastPosY, LastPosZ;
        
        public CharacterSaveData()
        {
            CharacterId = Guid.NewGuid().ToString();
            Level = 1;
            Class = CharacterClass.Cruzado;
            CreatedAt = DateTime.Now;
            LastPlayedAt = DateTime.Now;
            MaxHealth = 100;
            CurrentHealth = 100;
            MaxMana = 50;
            CurrentMana = 50;
        }
        
        public CharacterSaveData(string name) : this() { CharacterName = name; }
        
        [JsonIgnore]
        public Vector3 LastPosition
        {
            get => new Vector3(LastPosX, LastPosY, LastPosZ);
            set { LastPosX = value.x; LastPosY = value.y; LastPosZ = value.z; }
        }

        [JsonIgnore]
        public string ClassDisplayName => Class.ToString();

        public CharacterData ToCharacterData()
        {
            return new CharacterData
            {
                CharacterId = this.CharacterId,
                Name = this.CharacterName,
                ClassID = (int)this.Class,
                Level = this.Level,
                CurrentXP = 0,
                CreatedAt = this.CreatedAt,
                LastPlayedAt = this.LastPlayedAt,
                CurrentHP = this.CurrentHealth,
                MaxHP = this.MaxHealth,
                CurrentMana = this.CurrentMana,
                MaxMana = this.MaxMana,
                LastPosX = this.LastPosX,
                LastPosY = this.LastPosY,
                LastPosZ = this.LastPosZ
            };
        }
    }
}
