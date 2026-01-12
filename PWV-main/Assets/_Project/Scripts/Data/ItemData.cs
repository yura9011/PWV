using System;
using System.Collections.Generic;

namespace EtherDomes.Data
{
    /// <summary>
    /// Stats for an item.
    /// </summary>
    [Serializable]
    public class ItemStats
    {
        public int Strength;
        public int Agility;
        public int Intellect;
        public int Stamina;
        public int AttackPower;
        public int SpellPower;
        public int Armor;
        public int CriticalStrike;
        public int Haste;
        
        public ItemStats()
        {
            Strength = 0;
            Agility = 0;
            Intellect = 0;
            Stamina = 0;
            AttackPower = 0;
            SpellPower = 0;
            Armor = 0;
            CriticalStrike = 0;
            Haste = 0;
        }

        /// <summary>
        /// Suma los stats de otro ItemStats a este.
        /// </summary>
        public void Add(ItemStats other)
        {
            if (other == null) return;
            Strength += other.Strength;
            Agility += other.Agility;
            Intellect += other.Intellect;
            Stamina += other.Stamina;
            AttackPower += other.AttackPower;
            SpellPower += other.SpellPower;
            Armor += other.Armor;
            CriticalStrike += other.CriticalStrike;
            Haste += other.Haste;
        }
    }

    /// <summary>
    /// Data for an equipment item.
    /// </summary>
    [Serializable]
    public class ItemData
    {
        public string ItemId;
        public string ItemName;
        public string Description;
        public int ItemLevel;
        public ItemRarity Rarity;
        public ItemType Type;
        public EquipmentSlot Slot;
        public int RequiredLevel;
        
        /// <summary>
        /// Clases que pueden usar este item. Si está vacío, todas pueden.
        /// </summary>
        public CharacterClass[] AllowedClasses;
        
        /// <summary>
        /// Tipo de armadura (solo para slots de armadura).
        /// </summary>
        public ArmorType ArmorType;
        
        /// <summary>
        /// Tipo de arma (solo para MainHand/OffHand).
        /// </summary>
        public WeaponType WeaponType;
        
        /// <summary>
        /// Si es arma de 2 manos, ocupa MainHand y bloquea OffHand.
        /// </summary>
        public bool IsTwoHanded;
        
        public int StackCount = 1;
        
        /// <summary>
        /// Item stats.
        /// </summary>
        public ItemStats Stats;

        /// <summary>
        /// Maximum durability of the item.
        /// </summary>
        public int MaxDurability;

        /// <summary>
        /// Current durability of the item.
        /// </summary>
        public int CurrentDurability;

        /// <summary>
        /// Returns true if the item is broken (durability = 0).
        /// </summary>
        public bool IsBroken => CurrentDurability <= 0 && MaxDurability > 0;

        /// <summary>
        /// Returns the durability percentage (0-1).
        /// </summary>
        public float DurabilityPercent => MaxDurability > 0 ? (float)CurrentDurability / MaxDurability : 1f;

        public ItemData()
        {
            ItemId = Guid.NewGuid().ToString();
            Stats = new ItemStats();
            AllowedClasses = new CharacterClass[0];
            MaxDurability = 100;
            CurrentDurability = 100;
            StackCount = 1;
            Type = ItemType.Equipment;
            Description = "";
            IsTwoHanded = false;
        }

        /// <summary>
        /// Verifica si una clase puede equipar este item.
        /// </summary>
        public bool CanBeEquippedBy(CharacterClass charClass, int playerLevel)
        {
            if (playerLevel < RequiredLevel)
                return false;

            // Si no hay restricción de clases, todas pueden
            if (AllowedClasses == null || AllowedClasses.Length == 0)
                return true;

            foreach (var allowedClass in AllowedClasses)
            {
                if (allowedClass == charClass)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Crea una copia del item (para instancias en inventario).
        /// </summary>
        public ItemData Clone()
        {
            return new ItemData
            {
                ItemId = Guid.NewGuid().ToString(), // Nuevo ID para la instancia
                ItemName = this.ItemName,
                Description = this.Description,
                ItemLevel = this.ItemLevel,
                Rarity = this.Rarity,
                Type = this.Type,
                Slot = this.Slot,
                RequiredLevel = this.RequiredLevel,
                AllowedClasses = this.AllowedClasses,
                ArmorType = this.ArmorType,
                WeaponType = this.WeaponType,
                IsTwoHanded = this.IsTwoHanded,
                StackCount = this.StackCount,
                Stats = new ItemStats
                {
                    Strength = this.Stats.Strength,
                    Agility = this.Stats.Agility,
                    Intellect = this.Stats.Intellect,
                    Stamina = this.Stats.Stamina,
                    AttackPower = this.Stats.AttackPower,
                    SpellPower = this.Stats.SpellPower,
                    Armor = this.Stats.Armor,
                    CriticalStrike = this.Stats.CriticalStrike,
                    Haste = this.Stats.Haste
                },
                MaxDurability = this.MaxDurability,
                CurrentDurability = this.MaxDurability
            };
        }
    }

    /// <summary>
    /// Slot de inventario que contiene un item y cantidad.
    /// </summary>
    [Serializable]
    public class InventorySlot
    {
        public string ItemId;       // Referencia al ItemDatabase
        public string InstanceId;   // ID único de esta instancia
        public int Quantity;
        public int CurrentDurability;

        public InventorySlot()
        {
            ItemId = "";
            InstanceId = Guid.NewGuid().ToString();
            Quantity = 1;
            CurrentDurability = 100;
        }

        public InventorySlot(string itemId, int quantity = 1)
        {
            ItemId = itemId;
            InstanceId = Guid.NewGuid().ToString();
            Quantity = quantity;
            CurrentDurability = 100;
        }
    }

    /// <summary>
    /// Equipment data for a character.
    /// </summary>
    [Serializable]
    public class EquipmentData
    {
        public Dictionary<EquipmentSlot, ItemData> EquippedItems;
        public int TotalItemLevel;

        public EquipmentData()
        {
            EquippedItems = new Dictionary<EquipmentSlot, ItemData>();
            TotalItemLevel = 0;
        }

        public void Equip(ItemData item)
        {
            if (item == null) return;
            
            EquippedItems[item.Slot] = item;
            
            // Si es arma de 2 manos, limpiar OffHand
            if (item.Slot == EquipmentSlot.MainHand && item.IsTwoHanded)
            {
                EquippedItems.Remove(EquipmentSlot.OffHand);
            }
            
            RecalculateTotalItemLevel();
        }

        public ItemData Unequip(EquipmentSlot slot)
        {
            if (EquippedItems.TryGetValue(slot, out var item))
            {
                EquippedItems.Remove(slot);
                RecalculateTotalItemLevel();
                return item;
            }
            return null;
        }

        /// <summary>
        /// Obtiene los stats totales de todo el equipo.
        /// </summary>
        public ItemStats GetTotalStats()
        {
            var total = new ItemStats();
            foreach (var item in EquippedItems.Values)
            {
                if (item?.Stats != null)
                {
                    total.Add(item.Stats);
                }
            }
            return total;
        }

        /// <summary>
        /// Obtiene los stats totales como CharacterStats (para compatibilidad).
        /// </summary>
        public CharacterStats GetTotalStatsAsCharacterStats()
        {
            var itemStats = GetTotalStats();
            return new CharacterStats
            {
                Strength = itemStats.Strength,
                Intellect = itemStats.Intellect,
                Stamina = itemStats.Stamina,
                AttackPower = itemStats.AttackPower,
                SpellPower = itemStats.SpellPower,
                Armor = itemStats.Armor,
                MaxHealth = itemStats.Stamina * 10,
                MaxMana = itemStats.Intellect * 5
            };
        }

        /// <summary>
        /// Obtiene la armadura total del equipo.
        /// </summary>
        public int GetTotalArmor()
        {
            int armor = 0;
            foreach (var item in EquippedItems.Values)
            {
                if (item?.Stats != null)
                {
                    armor += item.Stats.Armor;
                }
            }
            return armor;
        }

        private void RecalculateTotalItemLevel()
        {
            TotalItemLevel = 0;
            foreach (var item in EquippedItems.Values)
            {
                if (item != null)
                    TotalItemLevel += item.ItemLevel;
            }
        }
    }
}
