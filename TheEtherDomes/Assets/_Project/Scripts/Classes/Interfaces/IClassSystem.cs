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
        /// Event fired when a player changes specialization.
        /// </summary>
        event Action<ulong, Specialization> OnSpecializationChanged;
    }
}
