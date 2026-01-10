using System;
using EtherDomes.Data;

namespace EtherDomes.Classes
{
    /// <summary>
    /// Interface for the class and specialization system.
    /// Manages character classes, specs, and their abilities.
    /// </summary>
    public interface IClassSystem
    {
        /// <summary>
        /// Get the class of a player.
        /// </summary>
        CharacterClass GetClass(ulong playerId);

        /// <summary>
        /// Get the current specialization of a player.
        /// </summary>
        Specialization GetSpecialization(ulong playerId);

        /// <summary>
        /// Set the specialization for a player. Only works out of combat.
        /// </summary>
        void SetSpecialization(ulong playerId, Specialization spec);

        /// <summary>
        /// Get all abilities available for a class/spec combination.
        /// </summary>
        AbilityData[] GetClassAbilities(CharacterClass charClass, Specialization spec);

        /// <summary>
        /// Check if a player can switch specialization (must be out of combat).
        /// </summary>
        bool CanSwitchSpec(ulong playerId);

        /// <summary>
        /// Get the effectiveness multiplier for a player's current spec.
        /// Returns 1.0 for main spec, 0.7 for off-spec (hybrid classes).
        /// </summary>
        float GetSpecEffectiveness(ulong playerId);

        /// <summary>
        /// Check if a specialization is valid for a given class.
        /// </summary>
        bool IsValidSpecForClass(CharacterClass charClass, Specialization spec);

        /// <summary>
        /// Get all valid specializations for a class.
        /// </summary>
        Specialization[] GetSpecializationsForClass(CharacterClass charClass);

        /// <summary>
        /// Get the primary resource type for a class.
        /// Requirements: 10.1, 10.2, 10.3
        /// </summary>
        PrimaryResourceType GetPrimaryResourceType(CharacterClass charClass);

        /// <summary>
        /// Get the secondary resource type for a class.
        /// Requirements: 10.1, 10.2, 10.3
        /// </summary>
        SecondaryResourceType GetSecondaryResourceType(CharacterClass charClass);

        /// <summary>
        /// Check if a class uses combo points.
        /// Requirements: 10.3, 5.3
        /// </summary>
        bool UsesComboPoints(CharacterClass charClass);

        /// <summary>
        /// Get the stat growth per level for a class.
        /// Requirements: 12.6, 12.7
        /// </summary>
        CharacterStats GetStatGrowthPerLevel(CharacterClass charClass);

        /// <summary>
        /// Get the base stats for a class at level 1.
        /// Requirements: 12.2, 12.3, 12.4, 12.5
        /// </summary>
        CharacterStats GetBaseStatsForClass(CharacterClass charClass);

        /// <summary>
        /// Event fired when a player changes specialization.
        /// </summary>
        event Action<ulong, Specialization> OnSpecializationChanged;
    }

    /// <summary>
    /// Primary resource types for classes.
    /// </summary>
    public enum PrimaryResourceType
    {
        None,       // No primary resource
        Mana,       // Mage, Priest, Warlock, Death Knight, Paladin
        Energy,     // Rogue
        Focus       // Hunter
    }
}
