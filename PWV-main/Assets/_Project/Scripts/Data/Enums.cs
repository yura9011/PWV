namespace EtherDomes.Data
{
    /// <summary>
    /// Character class types available in the game.
    /// 8 clases: 2 Tanks, 2 DPS Físico, 2 DPS Mágico, 2 Healers
    /// </summary>
    public enum CharacterClass
    {
        // Tanks
        Cruzado = 0,        // Tank - Escudo mágico + Maza 1 mano - Armadura Pesada
        Protector = 1,      // Tank - Escudo + Espada 1 mano - Armadura Pesada
        
        // DPS Físico
        Berserker = 2,      // DPS - Espada 2 manos - Armadura Pesada
        Arquero = 3,        // DPS - Arco 2 manos - Armadura Ligera
        
        // DPS Mágico
        MaestroElemental = 4,   // DPS - Orbe 2 manos - Armadura Encantada
        CaballeroRunico = 5,    // DPS - Espada mágica 2 manos - Armadura Pesada
        
        // Healers
        Clerigo = 6,        // Healer - Libro Sagrado 2 manos - Armadura Encantada
        MedicoBrujo = 7,    // Healer - Báculo 2 manos - Armadura Ligera

        // Legacy aliases (para compatibilidad con código existente)
        Warrior = Cruzado,
        Paladin = Protector,
        Mage = MaestroElemental,
        Priest = Clerigo,
        Rogue = Berserker,
        Hunter = Arquero,
        Warlock = MedicoBrujo,
        DeathKnight = CaballeroRunico
    }

    /// <summary>
    /// Tipos de armadura
    /// </summary>
    public enum ArmorType
    {
        None = -1,              // Sin armadura (accesorios)
        Universal = -2,         // Para todas las clases (capa, anillo, abalorio)
        ArmaduraPesada = 0,     // Cruzado, Protector, Berserker, CaballeroRunico
        ArmaduraLigera = 1,     // Arquero, MedicoBrujo
        ArmaduraEncantada = 2   // Clerigo, MaestroElemental
    }

    /// <summary>
    /// Tipos de armas
    /// </summary>
    public enum WeaponType
    {
        None = -1,
        
        // 1 Mano
        Espada1Mano = 0,
        Maza1Mano = 1,
        
        // Escudos (Off-hand)
        Escudo = 10,
        EscudoMagico = 11,
        
        // 2 Manos
        Espada2Manos = 20,
        EspadaMagica2Manos = 21,
        Arco2Manos = 22,
        Orbe2Manos = 23,
        Baculo2Manos = 24,
        LibroSagrado2Manos = 25
    }

    /// <summary>
    /// Specializations for each class (futuro uso).
    /// </summary>
    public enum Specialization
    {
        // Cruzado
        CruzadoTank = 0,
        
        // Protector
        ProtectorTank = 10,
        
        // Berserker
        BerserkerDPS = 20,
        
        // Arquero
        ArqueroDPS = 30,
        
        // Maestro Elemental
        MaestroElementalDPS = 40,
        
        // Caballero Runico
        CaballeroRunicoDPS = 50,
        
        // Clerigo
        ClerigoHealer = 60,
        
        // Medico Brujo
        MedicoBrujoHealer = 70,

        // Legacy aliases (para compatibilidad con código existente)
        Protection = CruzadoTank,
        ProtectionPaladin = ProtectorTank,
        HolyPaladin = ClerigoHealer,
        Retribution = BerserkerDPS,
        Arms = BerserkerDPS,
        Fire = MaestroElementalDPS,
        Frost = MaestroElementalDPS,
        FrostDK = CaballeroRunicoDPS,
        Holy = ClerigoHealer,
        Shadow = MedicoBrujoHealer,
        Affliction = MedicoBrujoHealer,
        Destruction = MaestroElementalDPS,
        BeastMastery = ArqueroDPS,
        Marksmanship = ArqueroDPS,
        Assassination = BerserkerDPS,
        Combat = BerserkerDPS,
        Blood = CaballeroRunicoDPS,
        Unholy = CaballeroRunicoDPS
    }

    /// <summary>
    /// Item rarity levels with associated drop rates.
    /// </summary>
    public enum ItemRarity
    {
        Common,     // 70%
        Uncommon,   // 20%
        Rare,       // 7%
        Epic,       // 2.5%
        Legendary   // 0.5%
    }

    /// <summary>
    /// Types of items.
    /// </summary>
    public enum ItemType
    {
        Equipment,
        Consumable,
        Material,
        Quest,
        Currency
    }

    /// <summary>
    /// Equipment slots on a character.
    /// 10 slots: Cabeza, Hombros, Pecho, Manos, Piernas, Pies, Espalda, Dedo, Abalorio, Arma
    /// </summary>
    public enum EquipmentSlot
    {
        Head = 0,           // Casco
        Shoulders = 1,      // Hombreras
        Chest = 2,          // Pechera
        Hands = 3,          // Guantes
        Legs = 4,           // Pantalones
        Feet = 5,           // Botas
        Back = 6,           // Capa/Alas
        Finger = 7,         // Anillo
        Trinket = 8,        // Abalorio
        MainHand = 9,       // Arma principal (1 o 2 manos)
        OffHand = 10        // Escudo o segunda arma (solo para clases con 1 mano)
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
        Nature,
        Arcane
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
        ServerFull = 6,
        InvalidStats = 7,
        SanitizationRequired = 8
    }

    /// <summary>
    /// Secondary resource types for class-specific mechanics.
    /// - Mana: Cruzado, MaestroElemental, Clerigo, MedicoBrujo
    /// - Colera (Rage): Protector, Berserker
    /// - Energia: Arquero
    /// - EnergiaRunica + Runas: CaballeroRunico
    /// </summary>
    public enum SecondaryResourceType
    {
        None,
        Mana,           // Cruzado, MaestroElemental, Clerigo, MedicoBrujo
        Colera,         // Protector, Berserker (antes Rage)
        Energia,        // Arquero
        EnergiaRunica,  // CaballeroRunico (recurso principal)
        Runas,          // CaballeroRunico (recurso secundario - 6 runas)
        
        // Legacy aliases
        Rage = Colera,
        Energy = Energia,
        RunicPower = EnergiaRunica,
        Focus = Energia,
        HolyPower = Mana
    }

    /// <summary>
    /// Dungeon difficulty tiers.
    /// </summary>
    public enum DungeonDifficulty
    {
        Normal,
        Heroic,
        Mythic
    }

    /// <summary>
    /// Types of buff/debuff effects.
    /// </summary>
    public enum EffectType
    {
        Buff,
        Debuff,
        DoT,
        HoT,
        Slow,
        Stun,
        Fear,
        Root
    }

    /// <summary>
    /// Types of crowd control effects.
    /// </summary>
    public enum CCType
    {
        None,
        Slow,
        Stun,
        Fear,
        Root
    }

    /// <summary>
    /// Combat state machine states for ability casting.
    /// </summary>
    public enum CombatState
    {
        /// <summary>Ready to use abilities.</summary>
        Idle,
        /// <summary>Currently casting an ability with cast time.</summary>
        Casting,
        /// <summary>In Global Cooldown period after using an ability.</summary>
        GlobalCooldown,
        /// <summary>Stunned, silenced, or otherwise unable to act.</summary>
        Locked
    }

    /// <summary>
    /// Tipos de mascotas/compañeros.
    /// </summary>
    public enum PetType
    {
        None = 0,
        Combat,     // Mascota de combate
        Mount,      // Montura
        Companion   // Compañero cosmético
    }

    /// <summary>
    /// Estados de una mascota.
    /// </summary>
    public enum PetState
    {
        Idle,
        Following,
        Attacking,
        Defending,
        Dead,
        Dismissed
    }
}
