using System.Collections.Generic;

namespace EtherDomes.Data
{
    /// <summary>
    /// Static definitions for all pet types.
    /// In production, these would be loaded from ScriptableObjects.
    /// </summary>
    public static class PetDefinitions
    {
        #region Combat Pets

        /// <summary>
        /// Generic combat pet.
        /// </summary>
        public static PetData GetCombatPet() => new PetData
        {
            PetId = "combat_pet_basic",
            DisplayName = "Compañero de Combate",
            PetType = PetType.Combat,
            BaseHealth = 150f,
            BaseDamage = 20f,
            AttackSpeed = 2f,
            AttackRange = 2f,
            MoveSpeed = 6f
        };

        /// <summary>
        /// Companion pet (cosmetic).
        /// </summary>
        public static PetData GetCompanionPet() => new PetData
        {
            PetId = "companion_basic",
            DisplayName = "Compañero",
            PetType = PetType.Companion,
            BaseHealth = 50f,
            BaseDamage = 0f,
            AttackSpeed = 0f,
            AttackRange = 0f,
            MoveSpeed = 5f
        };

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get the default pet for a class.
        /// </summary>
        public static PetData GetDefaultPetForClass(CharacterClass charClass)
        {
            // Por ahora, todas las clases pueden tener un compañero cosmético
            return GetCompanionPet();
        }

        /// <summary>
        /// Get all available pets for a class.
        /// </summary>
        public static List<PetData> GetAvailablePetsForClass(CharacterClass charClass)
        {
            var pets = new List<PetData>();
            pets.Add(GetCompanionPet());
            
            // Algunas clases pueden tener mascotas de combate
            if (charClass == CharacterClass.MedicoBrujo)
            {
                pets.Add(GetCombatPet());
            }
            
            return pets;
        }

        /// <summary>
        /// Check if a class can have combat pets.
        /// </summary>
        public static bool ClassHasCombatPets(CharacterClass charClass)
        {
            return charClass == CharacterClass.MedicoBrujo;
        }

        #endregion
    }
}
