using UnityEngine;

namespace EtherDomes.Data
{
    /// <summary>
    /// ScriptableObject que define una clase de personaje.
    /// Contiene stats base, crecimiento por nivel, y restricciones de equipo.
    /// </summary>
    [CreateAssetMenu(fileName = "NewClassDefinition", menuName = "EtherDomes/Data/Class Definition")]
    public class ClassDefinition : ScriptableObject
    {
        [Header("Identificación")]
        public CharacterClass ClassType;
        public string ClassName;
        [TextArea] public string Description;

        [Header("Rol")]
        public ClassRole Role;
        
        [Header("Base Stats (Level 1)")]
        public int BaseHealth = 100;
        public int BaseMana = 50;
        public int BaseStrength = 10;
        public int BaseAgility = 10;
        public int BaseIntellect = 10;
        public int BaseStamina = 10;
        public int BaseArmor = 0;

        [Header("Growth Per Level")]
        public float HealthPerLevel = 10f;
        public float ManaPerLevel = 5f;
        public float StrengthPerLevel = 2f;
        public float AgilityPerLevel = 1f;
        public float IntellectPerLevel = 1f;
        public float StaminaPerLevel = 2f;

        [Header("Restricciones de Equipo")]
        public ArmorType AllowedArmorType;
        public WeaponType[] AllowedWeaponTypes;
        public bool UsesTwoHandedWeapon = true;
        public bool UsesShield = false;

        /// <summary>
        /// Calcula los stats esperados para un nivel dado.
        /// Usado por el Host para validar datos del cliente.
        /// </summary>
        public CalculatedStats GetStatsForLevel(int level)
        {
            return new CalculatedStats
            {
                MaxHealth = Mathf.RoundToInt(BaseHealth + (HealthPerLevel * (level - 1))),
                MaxMana = Mathf.RoundToInt(BaseMana + (ManaPerLevel * (level - 1))),
                Strength = Mathf.RoundToInt(BaseStrength + (StrengthPerLevel * (level - 1))),
                Agility = Mathf.RoundToInt(BaseAgility + (AgilityPerLevel * (level - 1))),
                Intellect = Mathf.RoundToInt(BaseIntellect + (IntellectPerLevel * (level - 1))),
                Stamina = Mathf.RoundToInt(BaseStamina + (StaminaPerLevel * (level - 1))),
                BaseArmor = BaseArmor
            };
        }

        /// <summary>
        /// Verifica si un tipo de arma es válido para esta clase.
        /// </summary>
        public bool CanUseWeapon(WeaponType weaponType)
        {
            if (AllowedWeaponTypes == null || AllowedWeaponTypes.Length == 0)
                return false;

            foreach (var allowed in AllowedWeaponTypes)
            {
                if (allowed == weaponType)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Verifica si un tipo de armadura es válido para esta clase.
        /// </summary>
        public bool CanUseArmor(ArmorType armorType)
        {
            return AllowedArmorType == armorType;
        }
    }

    /// <summary>
    /// Rol de la clase en el grupo.
    /// </summary>
    public enum ClassRole
    {
        Tank,
        DPSFisico,
        DPSMagico,
        Healer
    }

    /// <summary>
    /// Stats calculados para un nivel específico.
    /// </summary>
    [System.Serializable]
    public struct CalculatedStats
    {
        public int MaxHealth;
        public int MaxMana;
        public int Strength;
        public int Agility;
        public int Intellect;
        public int Stamina;
        public int BaseArmor;
    }
}
