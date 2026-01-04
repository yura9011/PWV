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
        }

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
                Armor = Armor
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
        }
    }
}
