using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace EtherDomes.Data
{
    /// <summary>
    /// Archivo de guardado principal que contiene todos los personajes.
    /// Maximo 12 personajes por cuenta.
    /// </summary>
    [Serializable]
    public class SaveFile
    {
        public const int MAX_CHARACTERS = 12;
        public const int SAVE_VERSION = 2;
        
        public string AccountID;
        public int Version = SAVE_VERSION;
        public DateTime LastSaved;
        public List<CharacterData> Characters = new List<CharacterData>();
        
        public bool CanCreateMore => Characters.Count < MAX_CHARACTERS;
        public int Count => Characters.Count;

        public SaveFile()
        {
            AccountID = Guid.NewGuid().ToString();
            LastSaved = DateTime.Now;
        }
    }

    /// <summary>
    /// Datos completos de un personaje.
    /// </summary>
    [Serializable]
    public class CharacterData
    {
        public string CharacterId;
        public string Name;
        public int ClassID;
        public int Level;
        public float CurrentXP;
        public DateTime CreatedAt;
        public DateTime LastPlayedAt;

        public float CurrentHP;
        public float CurrentMana;
        public float MaxHP;
        public float MaxMana;
        public List<ActiveStatusEffect> StatusEffects = new List<ActiveStatusEffect>();

        public int TotalStrength;
        public int TotalAgility;
        public int TotalIntellect;
        public int TotalStamina;
        public int TotalArmor;
        public int TotalAttackPower;
        public int TotalSpellPower;

        public List<InventorySlot> Inventory = new List<InventorySlot>();
        public List<string> EquippedItemIDs = new List<string>();

        public string LastWorldId;
        public float LastPosX;
        public float LastPosY;
        public float LastPosZ;

        public CharacterData()
        {
            CharacterId = Guid.NewGuid().ToString();
            Level = 1;
            CurrentXP = 0;
            ClassID = 0;
            CreatedAt = DateTime.Now;
            LastPlayedAt = DateTime.Now;
            CurrentHP = 100;
            MaxHP = 100;
            CurrentMana = 50;
            MaxMana = 50;
        }

        public CharacterData(string name, CharacterClass characterClass) : this()
        {
            Name = name;
            ClassID = (int)characterClass;
        }

        public CharacterClass GetCharacterClass() => (CharacterClass)ClassID;

        [JsonIgnore]
        public Vector3 LastPosition
        {
            get => new Vector3(LastPosX, LastPosY, LastPosZ);
            set { LastPosX = value.x; LastPosY = value.y; LastPosZ = value.z; }
        }

        [JsonIgnore]
        public string ClassDisplayName => ((CharacterClass)ClassID).ToString();
        [JsonIgnore]
        public float HPPercent => MaxHP > 0 ? CurrentHP / MaxHP : 1f;
        [JsonIgnore]
        public float ManaPercent => MaxMana > 0 ? CurrentMana / MaxMana : 1f;

        public void RecalculateTotalStats(ClassDefinition classDef, ItemDatabase itemDb)
        {
            if (classDef == null) return;
            var baseStats = classDef.GetStatsForLevel(Level);
            TotalStrength = baseStats.Strength;
            TotalAgility = baseStats.Agility;
            TotalIntellect = baseStats.Intellect;
            TotalStamina = baseStats.Stamina;
            TotalArmor = baseStats.BaseArmor;
            TotalAttackPower = 0;
            TotalSpellPower = 0;

            if (itemDb != null && EquippedItemIDs != null)
            {
                foreach (var itemId in EquippedItemIDs)
                {
                    var item = itemDb.GetItem(itemId);
                    if (item != null && item.Stats != null)
                    {
                        TotalStrength += item.Stats.Strength;
                        TotalAgility += item.Stats.Agility;
                        TotalIntellect += item.Stats.Intellect;
                        TotalStamina += item.Stats.Stamina;
                        TotalArmor += item.Stats.Armor;
                        TotalAttackPower += item.Stats.AttackPower;
                        TotalSpellPower += item.Stats.SpellPower;
                    }
                }
            }
            MaxHP = baseStats.MaxHealth + (TotalStamina * 10);
            MaxMana = baseStats.MaxMana + (TotalIntellect * 5);
        }
    }

    [Serializable]
    public class ActiveStatusEffect
    {
        public string EffectId;
        public string EffectName;
        public EffectType Type;
        public float RemainingDuration;
        public int Stacks;
        public float Value;

        public ActiveStatusEffect() { EffectId = ""; EffectName = ""; Type = EffectType.Buff; }
        public bool IsExpired => RemainingDuration <= 0;
    }
}
