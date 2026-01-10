using System.Collections.Generic;

namespace EtherDomes.Data
{
    /// <summary>
    /// Static definitions for all pet types.
    /// In production, these would be loaded from ScriptableObjects.
    /// </summary>
    public static class PetDefinitions
    {
        #region Warlock Pets

        /// <summary>
        /// Voidwalker - Warlock tank pet with high HP and taunt ability.
        /// Requirements 7.8: Voidwalker (tank): high HP, taunt ability
        /// </summary>
        public static PetData GetVoidwalker() => new PetData
        {
            PetId = "warlock_voidwalker",
            DisplayName = "Voidwalker",
            PetType = PetType.Voidwalker,
            BaseHealth = 200f,  // High HP for tank role
            BaseDamage = 15f,   // Lower damage, tank focused
            AttackSpeed = 2.5f,
            AttackRange = 2f,
            MoveSpeed = 5f
        };

        /// <summary>
        /// Imp - Warlock damage pet with low HP and fireball ability.
        /// Requirements 7.8: Imp (damage): low HP, fireball ability
        /// </summary>
        public static PetData GetImp() => new PetData
        {
            PetId = "warlock_imp",
            DisplayName = "Imp",
            PetType = PetType.Imp,
            BaseHealth = 80f,   // Low HP for damage role
            BaseDamage = 35f,   // Higher damage
            AttackSpeed = 1.5f, // Faster attacks
            AttackRange = 30f,  // Ranged attacks (fireball)
            MoveSpeed = 6f
        };

        #endregion

        #region Hunter Pets

        /// <summary>
        /// Generic beast pet for Hunters.
        /// </summary>
        public static PetData GetHunterBeast() => new PetData
        {
            PetId = "hunter_beast",
            DisplayName = "Beast",
            PetType = PetType.Beast,
            BaseHealth = 120f,
            BaseDamage = 25f,
            AttackSpeed = 2f,
            AttackRange = 2f,
            MoveSpeed = 7f
        };

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get the default pet for a class and specialization.
        /// </summary>
        public static PetData GetDefaultPetForClass(CharacterClass charClass, Specialization spec)
        {
            return charClass switch
            {
                CharacterClass.Hunter => GetHunterBeast(),
                CharacterClass.Warlock => spec == Specialization.Affliction 
                    ? GetImp()        // Affliction gets Imp (damage)
                    : GetVoidwalker(), // Destruction gets Voidwalker (tank)
                _ => null
            };
        }

        /// <summary>
        /// Get all available pets for a class.
        /// </summary>
        public static List<PetData> GetAvailablePetsForClass(CharacterClass charClass)
        {
            var pets = new List<PetData>();

            switch (charClass)
            {
                case CharacterClass.Hunter:
                    pets.Add(GetHunterBeast());
                    break;
                case CharacterClass.Warlock:
                    pets.Add(GetVoidwalker());
                    pets.Add(GetImp());
                    break;
            }

            return pets;
        }

        /// <summary>
        /// Check if a class can have pets.
        /// </summary>
        public static bool ClassHasPets(CharacterClass charClass)
        {
            return charClass is CharacterClass.Hunter or CharacterClass.Warlock;
        }

        #endregion
    }
}
