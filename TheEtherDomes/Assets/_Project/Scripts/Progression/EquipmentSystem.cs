using System;
using System.Collections.Generic;
using EtherDomes.Data;
using UnityEngine;

namespace EtherDomes.Progression
{
    /// <summary>
    /// Manages player equipment and stat calculations.
    /// </summary>
    public class EquipmentSystem : MonoBehaviour, IEquipmentSystem
    {
        // Player equipment storage
        private readonly Dictionary<ulong, Dictionary<EquipmentSlot, ItemData>> _playerEquipment = new();
        private readonly Dictionary<ulong, int> _playerLevels = new();
        private readonly Dictionary<ulong, CharacterClass> _playerClasses = new();

        public event Action<ulong, ItemData, EquipmentSlot> OnItemEquipped;
        public event Action<ulong, ItemData, EquipmentSlot> OnItemUnequipped;

        /// <summary>
        /// Register a player for equipment tracking.
        /// </summary>
        public void RegisterPlayer(ulong playerId, int level, CharacterClass charClass)
        {
            _playerEquipment[playerId] = new Dictionary<EquipmentSlot, ItemData>();
            _playerLevels[playerId] = level;
            _playerClasses[playerId] = charClass;
        }

        /// <summary>
        /// Unregister a player.
        /// </summary>
        public void UnregisterPlayer(ulong playerId)
        {
            _playerEquipment.Remove(playerId);
            _playerLevels.Remove(playerId);
            _playerClasses.Remove(playerId);
        }

        /// <summary>
        /// Update player level (for equipment requirement checks).
        /// </summary>
        public void UpdatePlayerLevel(ulong playerId, int level)
        {
            _playerLevels[playerId] = level;
        }

        public bool CanEquip(ulong playerId, ItemData item)
        {
            if (item == null) return false;

            // Check level requirement
            if (_playerLevels.TryGetValue(playerId, out int playerLevel))
            {
                if (playerLevel < item.RequiredLevel)
                {
                    Debug.Log($"[EquipmentSystem] Cannot equip: Level {item.RequiredLevel} required, player is {playerLevel}");
                    return false;
                }
            }

            // Check class restriction
            if (item.AllowedClasses != null && item.AllowedClasses.Length > 0)
            {
                if (_playerClasses.TryGetValue(playerId, out CharacterClass playerClass))
                {
                    bool classAllowed = false;
                    foreach (var allowedClass in item.AllowedClasses)
                    {
                        if (allowedClass == playerClass)
                        {
                            classAllowed = true;
                            break;
                        }
                    }

                    if (!classAllowed)
                    {
                        Debug.Log($"[EquipmentSystem] Cannot equip: Class {playerClass} not allowed");
                        return false;
                    }
                }
            }

            return true;
        }

        public void Equip(ulong playerId, ItemData item, EquipmentSlot slot)
        {
            if (!CanEquip(playerId, item))
            {
                Debug.LogWarning($"[EquipmentSystem] Player {playerId} cannot equip {item.ItemName}");
                return;
            }

            // Ensure player has equipment dictionary
            if (!_playerEquipment.ContainsKey(playerId))
            {
                _playerEquipment[playerId] = new Dictionary<EquipmentSlot, ItemData>();
            }

            // Unequip existing item if any
            if (_playerEquipment[playerId].TryGetValue(slot, out ItemData existingItem))
            {
                Unequip(playerId, slot);
            }

            // Equip new item
            _playerEquipment[playerId][slot] = item;
            Debug.Log($"[EquipmentSystem] Player {playerId} equipped {item.ItemName} to {slot}");
            OnItemEquipped?.Invoke(playerId, item, slot);
        }

        public void Unequip(ulong playerId, EquipmentSlot slot)
        {
            if (!_playerEquipment.TryGetValue(playerId, out var equipment)) return;
            if (!equipment.TryGetValue(slot, out ItemData item)) return;

            equipment.Remove(slot);
            Debug.Log($"[EquipmentSystem] Player {playerId} unequipped {item.ItemName} from {slot}");
            OnItemUnequipped?.Invoke(playerId, item, slot);
        }

        public ItemData GetEquippedItem(ulong playerId, EquipmentSlot slot)
        {
            if (!_playerEquipment.TryGetValue(playerId, out var equipment)) return null;
            return equipment.TryGetValue(slot, out ItemData item) ? item : null;
        }

        public CharacterStats GetEquipmentStats(ulong playerId)
        {
            var totalStats = new CharacterStats();

            if (!_playerEquipment.TryGetValue(playerId, out var equipment))
                return totalStats;

            foreach (var item in equipment.Values)
            {
                if (item?.Stats == null) continue;

                foreach (var stat in item.Stats)
                {
                    AddStatToCharacterStats(ref totalStats, stat.Key, stat.Value);
                }
            }

            return totalStats;
        }

        private void AddStatToCharacterStats(ref CharacterStats stats, string statName, int value)
        {
            switch (statName.ToLower())
            {
                case "health":
                case "maxhealth":
                    stats.MaxHealth += value;
                    stats.Health += value;
                    break;
                case "mana":
                case "maxmana":
                    stats.MaxMana += value;
                    stats.Mana += value;
                    break;
                case "strength":
                    stats.Strength += value;
                    break;
                case "intellect":
                    stats.Intellect += value;
                    break;
                case "stamina":
                    stats.Stamina += value;
                    // Stamina also adds health (10 HP per stamina)
                    stats.MaxHealth += value * 10;
                    stats.Health += value * 10;
                    break;
                case "attackpower":
                    stats.AttackPower += value;
                    break;
                case "spellpower":
                    stats.SpellPower += value;
                    break;
                case "armor":
                    stats.Armor += value;
                    break;
            }
        }

        /// <summary>
        /// Get total item level of equipped items.
        /// </summary>
        public int GetTotalItemLevel(ulong playerId)
        {
            if (!_playerEquipment.TryGetValue(playerId, out var equipment))
                return 0;

            int total = 0;
            foreach (var item in equipment.Values)
            {
                if (item != null)
                    total += item.ItemLevel;
            }
            return total;
        }

        /// <summary>
        /// Get average item level of equipped items.
        /// </summary>
        public float GetAverageItemLevel(ulong playerId)
        {
            if (!_playerEquipment.TryGetValue(playerId, out var equipment))
                return 0;

            if (equipment.Count == 0) return 0;

            int total = 0;
            foreach (var item in equipment.Values)
            {
                if (item != null)
                    total += item.ItemLevel;
            }
            return (float)total / equipment.Count;
        }

        /// <summary>
        /// Get all equipped items for a player.
        /// </summary>
        public Dictionary<EquipmentSlot, ItemData> GetAllEquipment(ulong playerId)
        {
            if (_playerEquipment.TryGetValue(playerId, out var equipment))
            {
                return new Dictionary<EquipmentSlot, ItemData>(equipment);
            }
            return new Dictionary<EquipmentSlot, ItemData>();
        }

        /// <summary>
        /// Load equipment from saved data.
        /// </summary>
        public void LoadEquipment(ulong playerId, Dictionary<EquipmentSlot, ItemData> equipment)
        {
            if (!_playerEquipment.ContainsKey(playerId))
            {
                _playerEquipment[playerId] = new Dictionary<EquipmentSlot, ItemData>();
            }

            foreach (var kvp in equipment)
            {
                _playerEquipment[playerId][kvp.Key] = kvp.Value;
            }
        }
    }
}
