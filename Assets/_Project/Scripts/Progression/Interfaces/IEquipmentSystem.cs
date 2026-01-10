using System;
using EtherDomes.Data;

namespace EtherDomes.Progression
{
    /// <summary>
    /// Interface for the equipment management system.
    /// </summary>
    public interface IEquipmentSystem
    {
        /// <summary>
        /// Check if a player can equip an item.
        /// </summary>
        bool CanEquip(ulong playerId, ItemData item);

        /// <summary>
        /// Equip an item to a slot.
        /// </summary>
        void Equip(ulong playerId, ItemData item, EquipmentSlot slot);

        /// <summary>
        /// Unequip an item from a slot.
        /// </summary>
        void Unequip(ulong playerId, EquipmentSlot slot);

        /// <summary>
        /// Get the item equipped in a slot.
        /// </summary>
        ItemData GetEquippedItem(ulong playerId, EquipmentSlot slot);

        /// <summary>
        /// Get total stats from all equipped items.
        /// </summary>
        CharacterStats GetEquipmentStats(ulong playerId);

        /// <summary>
        /// Event fired when an item is equipped.
        /// </summary>
        event Action<ulong, ItemData, EquipmentSlot> OnItemEquipped;

        /// <summary>
        /// Event fired when an item is unequipped.
        /// </summary>
        event Action<ulong, ItemData, EquipmentSlot> OnItemUnequipped;
    }
}
