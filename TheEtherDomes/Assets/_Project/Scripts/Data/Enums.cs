namespace EtherDomes.Data
{
    /// <summary>
    /// Character class types available in the game.
    /// </summary>
    public enum CharacterClass
    {
        Warrior,  // Tank, DPS
        Mage,     // DPS
        Priest,   // Healer
        Paladin   // Tank, Healer, DPS
    }

    /// <summary>
    /// Specializations for each class.
    /// </summary>
    public enum Specialization
    {
        // Warrior
        Protection = 0,    // Tank
        Arms = 1,          // DPS
        
        // Mage
        Fire = 10,         // DPS
        Frost = 11,        // DPS
        
        // Priest
        Holy = 20,         // Healer
        Shadow = 21,       // DPS
        
        // Paladin
        ProtectionPaladin = 30, // Tank
        HolyPaladin = 31,       // Healer
        Retribution = 32        // DPS
    }

    /// <summary>
    /// Item rarity levels with associated drop rates.
    /// </summary>
    public enum ItemRarity
    {
        Common,   // 70%
        Rare,     // 25%
        Epic      // 5%
    }

    /// <summary>
    /// Equipment slots on a character.
    /// </summary>
    public enum EquipmentSlot
    {
        Head,
        Shoulders,
        Chest,
        Hands,
        Legs,
        Feet,
        MainHand,
        OffHand,
        Trinket1,
        Trinket2
    }


    /// <summary>
    /// Types of abilities in the game.
    /// </summary>
    public enum AbilityType
    {
        Damage,
        Healing,
        Buff,
        Debuff,
        Taunt,
        Utility
    }

    /// <summary>
    /// Types of damage that can be dealt.
    /// </summary>
    public enum DamageType
    {
        Physical,
        Fire,
        Frost,
        Holy,
        Shadow,
        Nature
    }

    /// <summary>
    /// Target types for the targeting system.
    /// </summary>
    public enum TargetType
    {
        Enemy,
        Friendly,
        Neutral
    }

    /// <summary>
    /// World regions with level ranges.
    /// </summary>
    public enum RegionId
    {
        Roca,      // Levels 1-15
        Bosque,    // Levels 15-30
        Nieve,     // Levels 30-40
        Pantano,   // Levels 40-50
        Ciudadela  // Levels 50-60
    }

    /// <summary>
    /// Dungeon sizes.
    /// </summary>
    public enum DungeonSize
    {
        Small, // 3 bosses
        Large  // 5 bosses
    }

    /// <summary>
    /// Boss encounter phases.
    /// </summary>
    public enum BossPhase
    {
        Phase1,  // 100% - 75%
        Phase2,  // 75% - 50%
        Phase3,  // 50% - 25%
        Phase4   // 25% - 0%
    }

    /// <summary>
    /// Loot distribution modes.
    /// </summary>
    public enum LootDistributionMode
    {
        RoundRobin,
        NeedGreed,
        MasterLooter
    }

    /// <summary>
    /// Connection approval error codes.
    /// </summary>
    public enum ApprovalErrorCode
    {
        None = 0,
        InvalidDataFormat = 1,
        CorruptedData = 2,
        StatsOutOfRange = 3,
        ValidationTimeout = 4,
        EncryptionFailure = 5,
        ServerFull = 6
    }
}
