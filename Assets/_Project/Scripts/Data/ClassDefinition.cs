using UnityEngine;

namespace EtherDomes.Data
{
    [CreateAssetMenu(fileName = "NewClassDefinition", menuName = "EtherDomes/Data/Class Definition")]
    public class ClassDefinition : ScriptableObject
    {
        public CharacterClass ClassType;
        public string ClassName;
        [TextArea] public string Description;

        [Header("Base Stats (Level 1)")]
        public int BaseHealth = 100;
        public int BaseMana = 50;
        public int BaseStrength = 10;
        public int BaseAgility = 10;
        public int BaseIntellect = 10;
        public int BaseStamina = 10;

        [Header("Growth Per Level")]
        public float HealthPerLevel = 10f;
        public float ManaPerLevel = 5f;
        public float StrengthPerLevel = 2f;
        public float AgilityPerLevel = 1f;
        public float IntellectPerLevel = 1f;
        public float StaminaPerLevel = 2f;
    }
}
