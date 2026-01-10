using System;

namespace EtherDomes.Data
{
    /// <summary>
    /// Character statistics that affect combat and gameplay.
    /// </summary>
    [Serializable]
    public class CharacterStats
    {
        public int Health;
        public int MaxHealth;
        public int Mana;
        public int MaxMana;
        public int Strength;
        public int Intellect;
        public int Stamina;
        public int AttackPower;
        public int SpellPower;
        public int Armor;

        // Secondary stats affected by soft caps (Requirements: 8.2)
        // Note: Use ISoftCapSystem.GetEffectiveValue() to get capped values
        public float CritChance;
        public float Haste;
        public float Mastery;

        // Delegate for soft cap calculation (injected by Progression system)
        public static Func<float, float> SoftCapCalculator { get; set; } = value => value;

        public CharacterStats()
        {
            Health = 100;
            MaxHealth = 100;
            Mana = 100;
            MaxMana = 100;
            Strength = 10;
            Intellect = 10;
            Stamina = 10;
            AttackPower = 10;
            SpellPower = 10;
            Armor = 0;
            CritChance = 5f;
            Haste = 0f;
            Mastery = 0f;
        }

        /// <summary>
        /// Gets the effective crit chance after soft caps.
        /// Requirements: 8.2
        /// </summary>
        public float GetEffectiveCritChance() => SoftCapCalculator(CritChance);

        /// <summary>
        /// Gets the effective haste after soft caps.
        /// Requirements: 8.2
        /// </summary>
        public float GetEffectiveHaste() => SoftCapCalculator(Haste);

        /// <summary>
        /// Gets the effective mastery after soft caps.
        /// Requirements: 8.2
        /// </summary>
        public float GetEffectiveMastery() => SoftCapCalculator(Mastery);

        public CharacterStats Clone()
        {
            return new CharacterStats
            {
                Health = Health,
                MaxHealth = MaxHealth,
                Mana = Mana,
                MaxMana = MaxMana,
                Strength = Strength,
                Intellect = Intellect,
                Stamina = Stamina,
                AttackPower = AttackPower,
                SpellPower = SpellPower,
                Armor = Armor,
                CritChance = CritChance,
                Haste = Haste,
                Mastery = Mastery
            };
        }

        public void Add(CharacterStats other)
        {
            if (other == null) return;
            
            MaxHealth += other.MaxHealth;
            MaxMana += other.MaxMana;
            Strength += other.Strength;
            Intellect += other.Intellect;
            Stamina += other.Stamina;
            AttackPower += other.AttackPower;
            SpellPower += other.SpellPower;
            Armor += other.Armor;
            CritChance += other.CritChance;
            Haste += other.Haste;
            Mastery += other.Mastery;
        }
    }
}
