using System.Collections.Generic;
using EtherDomes.Data;

namespace EtherDomes.Classes.Abilities
{
    /// <summary>
    /// Static definitions for all class abilities.
    /// In production, these would be loaded from ScriptableObjects.
    /// </summary>
    public static class ClassAbilityDefinitions
    {
        #region Warrior Abilities

        public static AbilityData[] GetWarriorProtectionAbilities() => new[]
        {
            // Tank abilities
            new AbilityData
            {
                AbilityId = "warrior_taunt",
                AbilityName = "Taunt",
                Description = "Forces the target to attack you for 3 seconds.",
                CastTime = 0f,
                Cooldown = 8f,
                ManaCost = 0f,
                Range = 30f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Taunt,
                RequiredClass = CharacterClass.Warrior,
                RequiredSpec = Specialization.Protection,
                UnlockLevel = 1,
                ThreatMultiplier = 1.1f
            },
            new AbilityData
            {
                AbilityId = "warrior_shield_block",
                AbilityName = "Shield Block",
                Description = "Raises your shield, blocking 30% of incoming damage for 6 seconds.",
                CastTime = 0f,
                Cooldown = 12f,
                ManaCost = 0f,
                Range = 0f,
                RequiresTarget = false,
                AffectedByGCD = false,
                Type = AbilityType.Buff,
                RequiredClass = CharacterClass.Warrior,
                RequiredSpec = Specialization.Protection,
                UnlockLevel = 10
            },
            new AbilityData
            {
                AbilityId = "warrior_shield_slam",
                AbilityName = "Shield Slam",
                Description = "Slams the target with your shield, dealing damage and generating high threat.",
                CastTime = 0f,
                Cooldown = 6f,
                ManaCost = 0f,
                Range = 5f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Physical,
                RequiredClass = CharacterClass.Warrior,
                RequiredSpec = Specialization.Protection,
                UnlockLevel = 1,
                BaseDamage = 50f,
                ThreatMultiplier = 1.5f
            },
            new AbilityData
            {
                AbilityId = "warrior_devastate",
                AbilityName = "Devastate",
                Description = "Sunder the target's armor, dealing damage and reducing armor.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 0f,
                Range = 5f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Physical,
                RequiredClass = CharacterClass.Warrior,
                RequiredSpec = Specialization.Protection,
                UnlockLevel = 20,
                BaseDamage = 35f,
                ThreatMultiplier = 1.3f
            }
        };

        public static AbilityData[] GetWarriorArmsAbilities() => new[]
        {
            // DPS abilities
            new AbilityData
            {
                AbilityId = "warrior_mortal_strike",
                AbilityName = "Mortal Strike",
                Description = "A powerful strike that deals heavy damage and reduces healing received.",
                CastTime = 0f,
                Cooldown = 6f,
                ManaCost = 0f,
                Range = 5f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Physical,
                RequiredClass = CharacterClass.Warrior,
                RequiredSpec = Specialization.Arms,
                UnlockLevel = 1,
                BaseDamage = 80f
            },
            new AbilityData
            {
                AbilityId = "warrior_overpower",
                AbilityName = "Overpower",
                Description = "Instantly overpower the enemy, dealing damage. Cannot be blocked or dodged.",
                CastTime = 0f,
                Cooldown = 5f,
                ManaCost = 0f,
                Range = 5f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Physical,
                RequiredClass = CharacterClass.Warrior,
                RequiredSpec = Specialization.Arms,
                UnlockLevel = 10,
                BaseDamage = 60f
            },
            new AbilityData
            {
                AbilityId = "warrior_execute",
                AbilityName = "Execute",
                Description = "Attempt to finish off a wounded foe. Only usable on targets below 20% health.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 0f,
                Range = 5f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Physical,
                RequiredClass = CharacterClass.Warrior,
                RequiredSpec = Specialization.Arms,
                UnlockLevel = 20,
                BaseDamage = 150f
            },
            new AbilityData
            {
                AbilityId = "warrior_bladestorm",
                AbilityName = "Bladestorm",
                Description = "Become a whirlwind of steel, dealing damage to all nearby enemies.",
                CastTime = 0f,
                Cooldown = 60f,
                ManaCost = 0f,
                Range = 8f,
                RequiresTarget = false,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Physical,
                RequiredClass = CharacterClass.Warrior,
                RequiredSpec = Specialization.Arms,
                UnlockLevel = 40,
                BaseDamage = 100f
            }
        };

        // Shared Warrior abilities (both specs)
        public static AbilityData[] GetWarriorSharedAbilities() => new[]
        {
            new AbilityData
            {
                AbilityId = "warrior_charge",
                AbilityName = "Charge",
                Description = "Charge to an enemy, stunning them briefly.",
                CastTime = 0f,
                Cooldown = 15f,
                ManaCost = 0f,
                Range = 25f,
                RequiresTarget = true,
                AffectedByGCD = false,
                Type = AbilityType.Utility,
                RequiredClass = CharacterClass.Warrior,
                UnlockLevel = 1
            },
            new AbilityData
            {
                AbilityId = "warrior_heroic_strike",
                AbilityName = "Heroic Strike",
                Description = "A strong attack that deals bonus damage.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 0f,
                Range = 5f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Physical,
                RequiredClass = CharacterClass.Warrior,
                UnlockLevel = 1,
                BaseDamage = 40f
            }
        };

        #endregion


        #region Mage Abilities

        public static AbilityData[] GetMageFireAbilities() => new[]
        {
            new AbilityData
            {
                AbilityId = "mage_fireball",
                AbilityName = "Fireball",
                Description = "Hurls a fiery ball that causes Fire damage.",
                CastTime = 2.5f,
                Cooldown = 0f,
                ManaCost = 30f,
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Fire,
                RequiredClass = CharacterClass.Mage,
                RequiredSpec = Specialization.Fire,
                UnlockLevel = 1,
                BaseDamage = 100f
            },
            new AbilityData
            {
                AbilityId = "mage_pyroblast",
                AbilityName = "Pyroblast",
                Description = "Hurls an immense fiery boulder that causes massive Fire damage.",
                CastTime = 4f,
                Cooldown = 0f,
                ManaCost = 50f,
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Fire,
                RequiredClass = CharacterClass.Mage,
                RequiredSpec = Specialization.Fire,
                UnlockLevel = 20,
                BaseDamage = 200f
            },
            new AbilityData
            {
                AbilityId = "mage_fire_blast",
                AbilityName = "Fire Blast",
                Description = "Blasts the enemy for Fire damage. Instant cast.",
                CastTime = 0f,
                Cooldown = 8f,
                ManaCost = 20f,
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Fire,
                RequiredClass = CharacterClass.Mage,
                RequiredSpec = Specialization.Fire,
                UnlockLevel = 10,
                BaseDamage = 70f
            },
            new AbilityData
            {
                AbilityId = "mage_combustion",
                AbilityName = "Combustion",
                Description = "Increases your critical strike chance by 50% for 10 seconds.",
                CastTime = 0f,
                Cooldown = 120f,
                ManaCost = 0f,
                Range = 0f,
                RequiresTarget = false,
                AffectedByGCD = false,
                Type = AbilityType.Buff,
                RequiredClass = CharacterClass.Mage,
                RequiredSpec = Specialization.Fire,
                UnlockLevel = 40
            }
        };

        public static AbilityData[] GetMageFrostAbilities() => new[]
        {
            new AbilityData
            {
                AbilityId = "mage_frostbolt",
                AbilityName = "Frostbolt",
                Description = "Launches a bolt of frost at the enemy, causing Frost damage and slowing movement.",
                CastTime = 2f,
                Cooldown = 0f,
                ManaCost = 25f,
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Frost,
                RequiredClass = CharacterClass.Mage,
                RequiredSpec = Specialization.Frost,
                UnlockLevel = 1,
                BaseDamage = 85f
            },
            new AbilityData
            {
                AbilityId = "mage_ice_lance",
                AbilityName = "Ice Lance",
                Description = "Quickly flings a shard of ice at the target.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 15f,
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Frost,
                RequiredClass = CharacterClass.Mage,
                RequiredSpec = Specialization.Frost,
                UnlockLevel = 10,
                BaseDamage = 45f
            },
            new AbilityData
            {
                AbilityId = "mage_blizzard",
                AbilityName = "Blizzard",
                Description = "Ice shards pelt the target area, dealing Frost damage over 8 seconds.",
                CastTime = 0f,
                Cooldown = 8f,
                ManaCost = 40f,
                Range = 35f,
                RequiresTarget = false,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Frost,
                RequiredClass = CharacterClass.Mage,
                RequiredSpec = Specialization.Frost,
                UnlockLevel = 20,
                BaseDamage = 120f
            },
            new AbilityData
            {
                AbilityId = "mage_icy_veins",
                AbilityName = "Icy Veins",
                Description = "Hastens your spellcasting, increasing spell haste by 30% for 20 seconds.",
                CastTime = 0f,
                Cooldown = 180f,
                ManaCost = 0f,
                Range = 0f,
                RequiresTarget = false,
                AffectedByGCD = false,
                Type = AbilityType.Buff,
                RequiredClass = CharacterClass.Mage,
                RequiredSpec = Specialization.Frost,
                UnlockLevel = 40
            }
        };

        // Shared Mage abilities
        public static AbilityData[] GetMageSharedAbilities() => new[]
        {
            new AbilityData
            {
                AbilityId = "mage_blink",
                AbilityName = "Blink",
                Description = "Teleports you forward a short distance.",
                CastTime = 0f,
                Cooldown = 15f,
                ManaCost = 10f,
                Range = 0f,
                RequiresTarget = false,
                AffectedByGCD = false,
                Type = AbilityType.Utility,
                RequiredClass = CharacterClass.Mage,
                UnlockLevel = 1
            },
            new AbilityData
            {
                AbilityId = "mage_arcane_intellect",
                AbilityName = "Arcane Intellect",
                Description = "Infuses the target with brilliance, increasing Intellect by 10%.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 20f,
                Range = 30f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Buff,
                RequiredClass = CharacterClass.Mage,
                UnlockLevel = 1
            }
        };

        #endregion

        #region Priest Abilities

        public static AbilityData[] GetPriestHolyAbilities() => new[]
        {
            new AbilityData
            {
                AbilityId = "priest_heal",
                AbilityName = "Heal",
                Description = "A slow but efficient heal that restores a large amount of health.",
                CastTime = 2.5f,
                Cooldown = 0f,
                ManaCost = 35f,
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Healing,
                RequiredClass = CharacterClass.Priest,
                RequiredSpec = Specialization.Holy,
                UnlockLevel = 1,
                BaseHealing = 150f
            },
            new AbilityData
            {
                AbilityId = "priest_flash_heal",
                AbilityName = "Flash Heal",
                Description = "A fast but expensive heal.",
                CastTime = 1.5f,
                Cooldown = 0f,
                ManaCost = 50f,
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Healing,
                RequiredClass = CharacterClass.Priest,
                RequiredSpec = Specialization.Holy,
                UnlockLevel = 10,
                BaseHealing = 100f
            },
            new AbilityData
            {
                AbilityId = "priest_prayer_of_healing",
                AbilityName = "Prayer of Healing",
                Description = "A powerful prayer that heals party members within 30 yards.",
                CastTime = 3f,
                Cooldown = 0f,
                ManaCost = 80f,
                Range = 30f,
                RequiresTarget = false,
                AffectedByGCD = true,
                Type = AbilityType.Healing,
                RequiredClass = CharacterClass.Priest,
                RequiredSpec = Specialization.Holy,
                UnlockLevel = 30,
                BaseHealing = 80f
            },
            new AbilityData
            {
                AbilityId = "priest_resurrect",
                AbilityName = "Resurrection",
                Description = "Brings a dead player back to life with 35% health and mana.",
                CastTime = 10f,
                Cooldown = 0f,
                ManaCost = 60f,
                Range = 30f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Utility,
                RequiredClass = CharacterClass.Priest,
                RequiredSpec = Specialization.Holy,
                UnlockLevel = 20
            }
        };

        public static AbilityData[] GetPriestShadowAbilities() => new[]
        {
            new AbilityData
            {
                AbilityId = "priest_shadow_word_pain",
                AbilityName = "Shadow Word: Pain",
                Description = "A word of darkness that causes Shadow damage over 18 seconds.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 25f,
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Shadow,
                RequiredClass = CharacterClass.Priest,
                RequiredSpec = Specialization.Shadow,
                UnlockLevel = 1,
                BaseDamage = 120f
            },
            new AbilityData
            {
                AbilityId = "priest_mind_blast",
                AbilityName = "Mind Blast",
                Description = "Blasts the target's mind for Shadow damage.",
                CastTime = 1.5f,
                Cooldown = 8f,
                ManaCost = 30f,
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Shadow,
                RequiredClass = CharacterClass.Priest,
                RequiredSpec = Specialization.Shadow,
                UnlockLevel = 10,
                BaseDamage = 90f
            },
            new AbilityData
            {
                AbilityId = "priest_mind_flay",
                AbilityName = "Mind Flay",
                Description = "Assaults the target's mind with Shadow energy, causing damage and slowing.",
                CastTime = 3f,
                Cooldown = 0f,
                ManaCost = 20f,
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Shadow,
                RequiredClass = CharacterClass.Priest,
                RequiredSpec = Specialization.Shadow,
                UnlockLevel = 20,
                BaseDamage = 100f
            },
            new AbilityData
            {
                AbilityId = "priest_shadowform",
                AbilityName = "Shadowform",
                Description = "Assume a Shadowform, increasing Shadow damage by 25%.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 0f,
                Range = 0f,
                RequiresTarget = false,
                AffectedByGCD = true,
                Type = AbilityType.Buff,
                RequiredClass = CharacterClass.Priest,
                RequiredSpec = Specialization.Shadow,
                UnlockLevel = 40
            }
        };

        // Shared Priest abilities
        public static AbilityData[] GetPriestSharedAbilities() => new[]
        {
            new AbilityData
            {
                AbilityId = "priest_power_word_shield",
                AbilityName = "Power Word: Shield",
                Description = "Draws on the soul of the friendly target to shield them, absorbing damage.",
                CastTime = 0f,
                Cooldown = 4f,
                ManaCost = 40f,
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Buff,
                RequiredClass = CharacterClass.Priest,
                UnlockLevel = 1
            },
            new AbilityData
            {
                AbilityId = "priest_smite",
                AbilityName = "Smite",
                Description = "Smites an enemy for Holy damage.",
                CastTime = 2f,
                Cooldown = 0f,
                ManaCost = 20f,
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Holy,
                RequiredClass = CharacterClass.Priest,
                UnlockLevel = 1,
                BaseDamage = 60f
            }
        };

        #endregion


        #region Paladin Abilities

        public static AbilityData[] GetPaladinProtectionAbilities() => new[]
        {
            new AbilityData
            {
                AbilityId = "paladin_righteous_defense",
                AbilityName = "Righteous Defense",
                Description = "Come to the defense of a friendly target, taunting up to 3 enemies attacking them.",
                CastTime = 0f,
                Cooldown = 8f,
                ManaCost = 0f,
                Range = 30f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Taunt,
                RequiredClass = CharacterClass.Paladin,
                RequiredSpec = Specialization.ProtectionPaladin,
                UnlockLevel = 1,
                ThreatMultiplier = 1.1f
            },
            new AbilityData
            {
                AbilityId = "paladin_avengers_shield",
                AbilityName = "Avenger's Shield",
                Description = "Hurls a holy shield at the enemy, dealing Holy damage and silencing.",
                CastTime = 0f,
                Cooldown = 15f,
                ManaCost = 20f,
                Range = 30f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Holy,
                RequiredClass = CharacterClass.Paladin,
                RequiredSpec = Specialization.ProtectionPaladin,
                UnlockLevel = 20,
                BaseDamage = 70f,
                ThreatMultiplier = 1.5f
            },
            new AbilityData
            {
                AbilityId = "paladin_consecration",
                AbilityName = "Consecration",
                Description = "Consecrates the land beneath you, dealing Holy damage to enemies.",
                CastTime = 0f,
                Cooldown = 8f,
                ManaCost = 30f,
                Range = 0f,
                RequiresTarget = false,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Holy,
                RequiredClass = CharacterClass.Paladin,
                RequiredSpec = Specialization.ProtectionPaladin,
                UnlockLevel = 10,
                BaseDamage = 50f,
                ThreatMultiplier = 1.3f
            },
            new AbilityData
            {
                AbilityId = "paladin_ardent_defender",
                AbilityName = "Ardent Defender",
                Description = "Reduces damage taken by 20% for 8 seconds. If you would die, heal for 20% instead.",
                CastTime = 0f,
                Cooldown = 120f,
                ManaCost = 0f,
                Range = 0f,
                RequiresTarget = false,
                AffectedByGCD = false,
                Type = AbilityType.Buff,
                RequiredClass = CharacterClass.Paladin,
                RequiredSpec = Specialization.ProtectionPaladin,
                UnlockLevel = 40
            }
        };

        public static AbilityData[] GetPaladinHolyAbilities() => new[]
        {
            new AbilityData
            {
                AbilityId = "paladin_holy_light",
                AbilityName = "Holy Light",
                Description = "A slow but efficient heal that restores health.",
                CastTime = 2.5f,
                Cooldown = 0f,
                ManaCost = 35f,
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Healing,
                RequiredClass = CharacterClass.Paladin,
                RequiredSpec = Specialization.HolyPaladin,
                UnlockLevel = 1,
                BaseHealing = 140f
            },
            new AbilityData
            {
                AbilityId = "paladin_flash_of_light",
                AbilityName = "Flash of Light",
                Description = "A quick heal that restores a small amount of health.",
                CastTime = 1.5f,
                Cooldown = 0f,
                ManaCost = 45f,
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Healing,
                RequiredClass = CharacterClass.Paladin,
                RequiredSpec = Specialization.HolyPaladin,
                UnlockLevel = 10,
                BaseHealing = 90f
            },
            new AbilityData
            {
                AbilityId = "paladin_beacon_of_light",
                AbilityName = "Beacon of Light",
                Description = "Place a beacon on a friendly target. Heals you cast on others also heal the beacon target.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 30f,
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Buff,
                RequiredClass = CharacterClass.Paladin,
                RequiredSpec = Specialization.HolyPaladin,
                UnlockLevel = 30
            },
            new AbilityData
            {
                AbilityId = "paladin_aura_mastery",
                AbilityName = "Aura Mastery",
                Description = "Empowers your aura, providing immunity to silence and interrupt effects.",
                CastTime = 0f,
                Cooldown = 180f,
                ManaCost = 0f,
                Range = 0f,
                RequiresTarget = false,
                AffectedByGCD = false,
                Type = AbilityType.Buff,
                RequiredClass = CharacterClass.Paladin,
                RequiredSpec = Specialization.HolyPaladin,
                UnlockLevel = 40
            }
        };

        public static AbilityData[] GetPaladinRetributionAbilities() => new[]
        {
            new AbilityData
            {
                AbilityId = "paladin_crusader_strike",
                AbilityName = "Crusader Strike",
                Description = "An instant strike that causes Physical damage.",
                CastTime = 0f,
                Cooldown = 4.5f,
                ManaCost = 0f,
                Range = 5f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Physical,
                RequiredClass = CharacterClass.Paladin,
                RequiredSpec = Specialization.Retribution,
                UnlockLevel = 1,
                BaseDamage = 55f
            },
            new AbilityData
            {
                AbilityId = "paladin_templar_verdict",
                AbilityName = "Templar's Verdict",
                Description = "A powerful weapon strike that deals Holy damage.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 0f,
                Range = 5f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Holy,
                RequiredClass = CharacterClass.Paladin,
                RequiredSpec = Specialization.Retribution,
                UnlockLevel = 10,
                BaseDamage = 100f
            },
            new AbilityData
            {
                AbilityId = "paladin_divine_storm",
                AbilityName = "Divine Storm",
                Description = "Whirls divine energy around you, dealing Holy damage to all nearby enemies.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 0f,
                Range = 8f,
                RequiresTarget = false,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Holy,
                RequiredClass = CharacterClass.Paladin,
                RequiredSpec = Specialization.Retribution,
                UnlockLevel = 20,
                BaseDamage = 70f
            },
            new AbilityData
            {
                AbilityId = "paladin_avenging_wrath",
                AbilityName = "Avenging Wrath",
                Description = "Increases damage and healing done by 20% for 20 seconds.",
                CastTime = 0f,
                Cooldown = 120f,
                ManaCost = 0f,
                Range = 0f,
                RequiresTarget = false,
                AffectedByGCD = false,
                Type = AbilityType.Buff,
                RequiredClass = CharacterClass.Paladin,
                RequiredSpec = Specialization.Retribution,
                UnlockLevel = 40
            }
        };

        // Shared Paladin abilities
        public static AbilityData[] GetPaladinSharedAbilities() => new[]
        {
            new AbilityData
            {
                AbilityId = "paladin_lay_on_hands",
                AbilityName = "Lay on Hands",
                Description = "Heals a friendly target for an amount equal to your maximum health.",
                CastTime = 0f,
                Cooldown = 600f,
                ManaCost = 0f,
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = false,
                Type = AbilityType.Healing,
                RequiredClass = CharacterClass.Paladin,
                UnlockLevel = 1,
                BaseHealing = 1000f
            },
            new AbilityData
            {
                AbilityId = "paladin_divine_shield",
                AbilityName = "Divine Shield",
                Description = "Protects you from all damage and spells for 8 seconds.",
                CastTime = 0f,
                Cooldown = 300f,
                ManaCost = 0f,
                Range = 0f,
                RequiresTarget = false,
                AffectedByGCD = false,
                Type = AbilityType.Buff,
                RequiredClass = CharacterClass.Paladin,
                UnlockLevel = 1
            },
            new AbilityData
            {
                AbilityId = "paladin_blessing_of_protection",
                AbilityName = "Blessing of Protection",
                Description = "Places a blessing on a party member, protecting them from physical attacks.",
                CastTime = 0f,
                Cooldown = 300f,
                ManaCost = 20f,
                Range = 30f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Buff,
                RequiredClass = CharacterClass.Paladin,
                UnlockLevel = 20
            },
            new AbilityData
            {
                AbilityId = "paladin_judgment",
                AbilityName = "Judgment",
                Description = "Releases a bolt of divine energy at the target, dealing Holy damage.",
                CastTime = 0f,
                Cooldown = 6f,
                ManaCost = 10f,
                Range = 30f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Holy,
                RequiredClass = CharacterClass.Paladin,
                UnlockLevel = 1,
                BaseDamage = 50f
            }
        };

        #endregion

        #region Rogue Abilities

        /// <summary>
        /// Rogue Assassination spec abilities.
        /// Requirements 5.6: Mutilate, Envenom, Rupture, Vendetta
        /// </summary>
        public static AbilityData[] GetRogueAssassinationAbilities() => new[]
        {
            // Mutilate - generator, 50 energy
            new AbilityData
            {
                AbilityId = "rogue_mutilate",
                AbilityName = "Mutilate",
                Description = "Instantly attacks with both weapons for Physical damage. Generates 1 combo point.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 50f, // Energy cost
                Range = 5f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Physical,
                RequiredClass = CharacterClass.Rogue,
                RequiredSpec = Specialization.Assassination,
                UnlockLevel = 1,
                BaseDamage = 75f,
                ResourceType = SecondaryResourceType.Energy,
                GeneratesComboPoint = true,
                BreaksStealth = true
            },
            // Envenom - finisher, 35 energy
            new AbilityData
            {
                AbilityId = "rogue_envenom",
                AbilityName = "Envenom",
                Description = "Finishing move that deals instant Nature damage. Damage increases per combo point.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 35f, // Energy cost
                Range = 5f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Nature,
                RequiredClass = CharacterClass.Rogue,
                RequiredSpec = Specialization.Assassination,
                UnlockLevel = 10,
                BaseDamage = 60f,
                ResourceType = SecondaryResourceType.Energy,
                ConsumesComboPoints = true,
                ComboPointDamageMultiplier = 0.25f, // 25% per combo point
                BreaksStealth = true
            },
            // Rupture - finisher DoT, 25 energy
            new AbilityData
            {
                AbilityId = "rogue_rupture",
                AbilityName = "Rupture",
                Description = "Finishing move that causes damage over time. Duration and damage increase per combo point.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 25f, // Energy cost
                Range = 5f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Debuff,
                DamageType = DamageType.Physical,
                RequiredClass = CharacterClass.Rogue,
                RequiredSpec = Specialization.Assassination,
                UnlockLevel = 20,
                BaseDamage = 120f, // Total DoT damage
                ResourceType = SecondaryResourceType.Energy,
                ConsumesComboPoints = true,
                ComboPointDamageMultiplier = 0.20f, // 20% per combo point
                BreaksStealth = true
            },
            // Vendetta - buff, 0 energy, 2min CD
            new AbilityData
            {
                AbilityId = "rogue_vendetta",
                AbilityName = "Vendetta",
                Description = "Marks an enemy for death, increasing all damage you deal to them by 30% for 20 seconds.",
                CastTime = 0f,
                Cooldown = 120f, // 2 minutes
                ManaCost = 0f,
                Range = 30f,
                RequiresTarget = true,
                AffectedByGCD = false,
                Type = AbilityType.Debuff,
                RequiredClass = CharacterClass.Rogue,
                RequiredSpec = Specialization.Assassination,
                UnlockLevel = 40,
                ResourceType = SecondaryResourceType.Energy,
                BreaksStealth = true
            }
        };

        /// <summary>
        /// Rogue Combat spec abilities.
        /// Requirements 5.7: Sinister Strike, Eviscerate, Blade Flurry, Adrenaline Rush
        /// </summary>
        public static AbilityData[] GetRogueCombatAbilities() => new[]
        {
            // Sinister Strike - generator, 45 energy
            new AbilityData
            {
                AbilityId = "rogue_sinister_strike",
                AbilityName = "Sinister Strike",
                Description = "A quick strike that deals Physical damage. Generates 1 combo point.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 45f, // Energy cost
                Range = 5f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Physical,
                RequiredClass = CharacterClass.Rogue,
                RequiredSpec = Specialization.Combat,
                UnlockLevel = 1,
                BaseDamage = 65f,
                ResourceType = SecondaryResourceType.Energy,
                GeneratesComboPoint = true,
                BreaksStealth = true
            },
            // Eviscerate - finisher, 35 energy
            new AbilityData
            {
                AbilityId = "rogue_eviscerate",
                AbilityName = "Eviscerate",
                Description = "Finishing move that deals Physical damage. Damage increases per combo point.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 35f, // Energy cost
                Range = 5f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Physical,
                RequiredClass = CharacterClass.Rogue,
                RequiredSpec = Specialization.Combat,
                UnlockLevel = 10,
                BaseDamage = 70f,
                ResourceType = SecondaryResourceType.Energy,
                ConsumesComboPoints = true,
                ComboPointDamageMultiplier = 0.25f, // 25% per combo point
                BreaksStealth = true
            },
            // Blade Flurry - buff, 0 energy, 2min CD
            new AbilityData
            {
                AbilityId = "rogue_blade_flurry",
                AbilityName = "Blade Flurry",
                Description = "Strikes up to 4 nearby targets for 40% of normal damage for 15 seconds.",
                CastTime = 0f,
                Cooldown = 120f, // 2 minutes
                ManaCost = 0f,
                Range = 0f,
                RequiresTarget = false,
                AffectedByGCD = false,
                Type = AbilityType.Buff,
                RequiredClass = CharacterClass.Rogue,
                RequiredSpec = Specialization.Combat,
                UnlockLevel = 30,
                ResourceType = SecondaryResourceType.Energy,
                BreaksStealth = true
            },
            // Adrenaline Rush - buff, 0 energy, 3min CD
            new AbilityData
            {
                AbilityId = "rogue_adrenaline_rush",
                AbilityName = "Adrenaline Rush",
                Description = "Increases Energy regeneration by 100% for 15 seconds.",
                CastTime = 0f,
                Cooldown = 180f, // 3 minutes
                ManaCost = 0f,
                Range = 0f,
                RequiresTarget = false,
                AffectedByGCD = false,
                Type = AbilityType.Buff,
                RequiredClass = CharacterClass.Rogue,
                RequiredSpec = Specialization.Combat,
                UnlockLevel = 40,
                ResourceType = SecondaryResourceType.Energy,
                BreaksStealth = true
            }
        };

        /// <summary>
        /// Shared Rogue abilities (both specs).
        /// Requirements 5.8: Stealth, Cheap Shot, Ambush, Kick
        /// </summary>
        public static AbilityData[] GetRogueSharedAbilities() => new[]
        {
            // Stealth - toggle, 0 energy
            new AbilityData
            {
                AbilityId = "rogue_stealth",
                AbilityName = "Stealth",
                Description = "Conceals you in the shadows, allowing you to stalk enemies without being seen.",
                CastTime = 0f,
                Cooldown = 2f, // Cooldown after breaking stealth
                ManaCost = 0f,
                Range = 0f,
                RequiresTarget = false,
                AffectedByGCD = false,
                Type = AbilityType.Buff,
                RequiredClass = CharacterClass.Rogue,
                UnlockLevel = 1,
                ResourceType = SecondaryResourceType.Energy,
                BreaksStealth = false // This ability doesn't break stealth, it enables it
            },
            // Cheap Shot - stun from stealth, 40 energy
            new AbilityData
            {
                AbilityId = "rogue_cheap_shot",
                AbilityName = "Cheap Shot",
                Description = "Stuns the target for 4 seconds. Must be stealthed. Generates 1 combo point.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 40f, // Energy cost
                Range = 5f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Debuff,
                RequiredClass = CharacterClass.Rogue,
                UnlockLevel = 5,
                ResourceType = SecondaryResourceType.Energy,
                RequiresStealth = true,
                GeneratesComboPoint = true,
                BreaksStealth = true,
                CCType = CCType.Stun // Requirements 11.2, 11.6
            },
            // Ambush - damage from stealth, 60 energy
            new AbilityData
            {
                AbilityId = "rogue_ambush",
                AbilityName = "Ambush",
                Description = "Ambush the target, dealing massive Physical damage. Must be stealthed. Generates 2 combo points.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 60f, // Energy cost
                Range = 5f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Physical,
                RequiredClass = CharacterClass.Rogue,
                UnlockLevel = 10,
                BaseDamage = 150f,
                ResourceType = SecondaryResourceType.Energy,
                RequiresStealth = true,
                GeneratesComboPoint = true,
                ComboPointsGenerated = 2, // Ambush generates 2 combo points
                BreaksStealth = true
            },
            // Kick - interrupt, 15 energy
            new AbilityData
            {
                AbilityId = "rogue_kick",
                AbilityName = "Kick",
                Description = "A quick kick that interrupts spellcasting and prevents any spell in that school from being cast for 5 seconds.",
                CastTime = 0f,
                Cooldown = 15f,
                ManaCost = 15f, // Energy cost
                Range = 5f,
                RequiresTarget = true,
                AffectedByGCD = false,
                Type = AbilityType.Utility,
                RequiredClass = CharacterClass.Rogue,
                UnlockLevel = 15,
                ResourceType = SecondaryResourceType.Energy,
                BreaksStealth = true
            }
        };

        #endregion

        #region Hunter Abilities

        /// <summary>
        /// Hunter Beast Mastery spec abilities.
        /// Requirements 6.4: Kill Command, Bestial Wrath, Cobra Shot, Multi-Shot
        /// </summary>
        public static AbilityData[] GetHunterBeastMasteryAbilities() => new[]
        {
            // Kill Command - pet attack, 30 focus
            new AbilityData
            {
                AbilityId = "hunter_kill_command",
                AbilityName = "Kill Command",
                Description = "Give the command to kill, causing your pet to instantly attack for heavy damage.",
                CastTime = 0f,
                Cooldown = 6f,
                ManaCost = 30f, // Focus cost
                Range = 50f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Physical,
                RequiredClass = CharacterClass.Hunter,
                RequiredSpec = Specialization.BeastMastery,
                UnlockLevel = 1,
                BaseDamage = 100f,
                ResourceType = SecondaryResourceType.Focus,
                MinRange = 0f, // Kill Command has no minimum range (pet attack)
                BreaksStealth = true
            },
            // Bestial Wrath - buff, 0 focus, 2min CD
            new AbilityData
            {
                AbilityId = "hunter_bestial_wrath",
                AbilityName = "Bestial Wrath",
                Description = "Send your pet into a rage, increasing all damage dealt by 20% for 15 seconds.",
                CastTime = 0f,
                Cooldown = 120f, // 2 minutes
                ManaCost = 0f,
                Range = 0f,
                RequiresTarget = false,
                AffectedByGCD = false,
                Type = AbilityType.Buff,
                RequiredClass = CharacterClass.Hunter,
                RequiredSpec = Specialization.BeastMastery,
                UnlockLevel = 40,
                ResourceType = SecondaryResourceType.Focus,
                BreaksStealth = true
            },
            // Cobra Shot - damage, 35 focus
            new AbilityData
            {
                AbilityId = "hunter_cobra_shot",
                AbilityName = "Cobra Shot",
                Description = "A quick shot that deals Nature damage and extends the duration of your Serpent Sting.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 35f, // Focus cost
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Nature,
                RequiredClass = CharacterClass.Hunter,
                RequiredSpec = Specialization.BeastMastery,
                UnlockLevel = 10,
                BaseDamage = 55f,
                ResourceType = SecondaryResourceType.Focus,
                MinRange = 8f, // Hunter dead zone
                BreaksStealth = true
            },
            // Multi-Shot - AoE, 40 focus
            new AbilityData
            {
                AbilityId = "hunter_multi_shot",
                AbilityName = "Multi-Shot",
                Description = "Fires several missiles, hitting your target and all enemies within 8 yards.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 40f, // Focus cost
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Physical,
                RequiredClass = CharacterClass.Hunter,
                RequiredSpec = Specialization.BeastMastery,
                UnlockLevel = 20,
                BaseDamage = 45f,
                ResourceType = SecondaryResourceType.Focus,
                MinRange = 8f, // Hunter dead zone
                BreaksStealth = true
            }
        };

        /// <summary>
        /// Hunter Marksmanship spec abilities.
        /// Requirements 6.5: Aimed Shot, Rapid Fire, Arcane Shot, Volley
        /// </summary>
        public static AbilityData[] GetHunterMarksmanshipAbilities() => new[]
        {
            // Aimed Shot - cast time, 50 focus
            new AbilityData
            {
                AbilityId = "hunter_aimed_shot",
                AbilityName = "Aimed Shot",
                Description = "A powerful aimed shot that deals heavy Physical damage.",
                CastTime = 2.5f,
                Cooldown = 12f,
                ManaCost = 50f, // Focus cost
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Physical,
                RequiredClass = CharacterClass.Hunter,
                RequiredSpec = Specialization.Marksmanship,
                UnlockLevel = 1,
                BaseDamage = 150f,
                ResourceType = SecondaryResourceType.Focus,
                MinRange = 8f, // Hunter dead zone
                BreaksStealth = true
            },
            // Rapid Fire - channel, 0 focus, 2min CD
            new AbilityData
            {
                AbilityId = "hunter_rapid_fire",
                AbilityName = "Rapid Fire",
                Description = "Shoot a stream of 7 shots at your target over 3 seconds, dealing Physical damage with each shot.",
                CastTime = 0f,
                Cooldown = 120f, // 2 minutes
                ManaCost = 0f,
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Physical,
                RequiredClass = CharacterClass.Hunter,
                RequiredSpec = Specialization.Marksmanship,
                UnlockLevel = 40,
                BaseDamage = 40f, // Per tick
                ResourceType = SecondaryResourceType.Focus,
                MinRange = 8f, // Hunter dead zone
                IsChanneled = true,
                ChannelDuration = 3f,
                TickInterval = 0.43f, // ~7 ticks over 3 seconds
                BreaksStealth = true
            },
            // Arcane Shot - instant, 40 focus
            new AbilityData
            {
                AbilityId = "hunter_arcane_shot",
                AbilityName = "Arcane Shot",
                Description = "An instant shot that causes Arcane damage.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 40f, // Focus cost
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Nature, // Using Nature as closest to Arcane
                RequiredClass = CharacterClass.Hunter,
                RequiredSpec = Specialization.Marksmanship,
                UnlockLevel = 10,
                BaseDamage = 65f,
                ResourceType = SecondaryResourceType.Focus,
                MinRange = 8f, // Hunter dead zone
                BreaksStealth = true
            },
            // Volley - AoE channel, 60 focus
            new AbilityData
            {
                AbilityId = "hunter_volley",
                AbilityName = "Volley",
                Description = "Continuously fire a volley of arrows at the target area, dealing Physical damage to all enemies over 6 seconds.",
                CastTime = 0f,
                Cooldown = 45f,
                ManaCost = 60f, // Focus cost
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Physical,
                RequiredClass = CharacterClass.Hunter,
                RequiredSpec = Specialization.Marksmanship,
                UnlockLevel = 30,
                BaseDamage = 30f, // Per tick
                ResourceType = SecondaryResourceType.Focus,
                MinRange = 8f, // Hunter dead zone
                IsChanneled = true,
                ChannelDuration = 6f,
                TickInterval = 1f, // 6 ticks over 6 seconds
                BreaksStealth = true
            }
        };

        /// <summary>
        /// Shared Hunter abilities (both specs).
        /// Requirements 6.6: Concussive Shot, Freezing Trap, Disengage, Counter Shot
        /// </summary>
        public static AbilityData[] GetHunterSharedAbilities() => new[]
        {
            // Concussive Shot - slow, 20 focus
            new AbilityData
            {
                AbilityId = "hunter_concussive_shot",
                AbilityName = "Concussive Shot",
                Description = "Dazes the target, slowing movement speed by 50% for 6 seconds.",
                CastTime = 0f,
                Cooldown = 5f,
                ManaCost = 20f, // Focus cost
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Debuff,
                RequiredClass = CharacterClass.Hunter,
                UnlockLevel = 5,
                ResourceType = SecondaryResourceType.Focus,
                MinRange = 8f, // Hunter dead zone
                BreaksStealth = true,
                CCType = CCType.Slow // Requirements 11.1, 11.6
            },
            // Freezing Trap - CC, 0 focus, 30s CD
            new AbilityData
            {
                AbilityId = "hunter_freezing_trap",
                AbilityName = "Freezing Trap",
                Description = "Place a trap that freezes the first enemy that approaches, incapacitating them for 1 minute. Damage will break the effect.",
                CastTime = 0f,
                Cooldown = 30f,
                ManaCost = 0f,
                Range = 0f, // Placed at feet
                RequiresTarget = false,
                AffectedByGCD = true,
                Type = AbilityType.Debuff,
                RequiredClass = CharacterClass.Hunter,
                UnlockLevel = 15,
                ResourceType = SecondaryResourceType.Focus,
                MinRange = 0f, // No minimum range for traps
                BreaksStealth = false, // Traps don't break stealth
                CCType = CCType.Stun // Requirements 11.2, 11.6
            },
            // Disengage - knockback self, 0 focus, 20s CD
            new AbilityData
            {
                AbilityId = "hunter_disengage",
                AbilityName = "Disengage",
                Description = "Leap backwards, away from your target.",
                CastTime = 0f,
                Cooldown = 20f,
                ManaCost = 0f,
                Range = 0f,
                RequiresTarget = false,
                AffectedByGCD = false,
                Type = AbilityType.Utility,
                RequiredClass = CharacterClass.Hunter,
                UnlockLevel = 10,
                ResourceType = SecondaryResourceType.Focus,
                MinRange = 0f, // No minimum range
                BreaksStealth = true,
                IsKnockbackSelf = true, // Requirements 11.5
                KnockbackDistance = 15f
            },
            // Counter Shot - interrupt, 40 focus
            new AbilityData
            {
                AbilityId = "hunter_counter_shot",
                AbilityName = "Counter Shot",
                Description = "Interrupts spellcasting and prevents any spell in that school from being cast for 3 seconds.",
                CastTime = 0f,
                Cooldown = 24f,
                ManaCost = 40f, // Focus cost
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = false,
                Type = AbilityType.Utility,
                RequiredClass = CharacterClass.Hunter,
                UnlockLevel = 20,
                ResourceType = SecondaryResourceType.Focus,
                MinRange = 8f, // Hunter dead zone
                BreaksStealth = true
            }
        };

        #endregion

        #region Warlock Abilities

        /// <summary>
        /// Warlock Affliction spec abilities.
        /// Requirements 7.4: Corruption (DoT instant, 20 mana), Agony (DoT instant, 25 mana), 
        /// Drain Life (channel heal, 30 mana), Haunt (damage + debuff, 40 mana)
        /// </summary>
        public static AbilityData[] GetWarlockAfflictionAbilities() => new[]
        {
            // Corruption - DoT instant, 20 mana
            new AbilityData
            {
                AbilityId = "warlock_corruption",
                AbilityName = "Corruption",
                Description = "Corrupts the target, causing Shadow damage over 14 seconds.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 20f,
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Debuff,
                DamageType = DamageType.Shadow,
                RequiredClass = CharacterClass.Warlock,
                RequiredSpec = Specialization.Affliction,
                UnlockLevel = 1,
                BaseDamage = 140f, // Total DoT damage over duration
                BreaksStealth = true
            },
            // Agony - DoT instant, 25 mana
            new AbilityData
            {
                AbilityId = "warlock_agony",
                AbilityName = "Agony",
                Description = "Inflicts increasing agony on the target, causing Shadow damage over 18 seconds. Damage starts low and increases over time.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 25f,
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Debuff,
                DamageType = DamageType.Shadow,
                RequiredClass = CharacterClass.Warlock,
                RequiredSpec = Specialization.Affliction,
                UnlockLevel = 10,
                BaseDamage = 180f, // Total DoT damage over duration
                BreaksStealth = true
            },
            // Drain Life - channel heal, 30 mana
            new AbilityData
            {
                AbilityId = "warlock_drain_life",
                AbilityName = "Drain Life",
                Description = "Drains life from the target, causing Shadow damage and healing you for 50% of the damage dealt.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 30f,
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Shadow,
                RequiredClass = CharacterClass.Warlock,
                RequiredSpec = Specialization.Affliction,
                UnlockLevel = 20,
                BaseDamage = 30f, // Per tick
                IsChanneled = true,
                ChannelDuration = 5f,
                TickInterval = 1f, // 5 ticks over 5 seconds
                BreaksStealth = true,
                HealsOnDamage = true,
                HealOnDamagePercent = 0.5f // 50% of damage dealt
            },
            // Haunt - damage + debuff, 40 mana
            new AbilityData
            {
                AbilityId = "warlock_haunt",
                AbilityName = "Haunt",
                Description = "A ghostly soul haunts the target, dealing Shadow damage and increasing all damage you deal to the target by 10% for 18 seconds.",
                CastTime = 1.5f,
                Cooldown = 15f,
                ManaCost = 40f,
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Shadow,
                RequiredClass = CharacterClass.Warlock,
                RequiredSpec = Specialization.Affliction,
                UnlockLevel = 40,
                BaseDamage = 100f,
                BreaksStealth = true
            }
        };

        /// <summary>
        /// Warlock Destruction spec abilities.
        /// Requirements 7.5: Chaos Bolt (cast time, 50 mana), Incinerate (cast time, 35 mana),
        /// Immolate (DoT, 25 mana), Rain of Fire (AoE channel, 60 mana)
        /// </summary>
        public static AbilityData[] GetWarlockDestructionAbilities() => new[]
        {
            // Chaos Bolt - cast time, 50 mana
            new AbilityData
            {
                AbilityId = "warlock_chaos_bolt",
                AbilityName = "Chaos Bolt",
                Description = "Unleashes a devastating bolt of chaos energy at the target, dealing massive Fire damage. Always critically strikes.",
                CastTime = 3f,
                Cooldown = 0f,
                ManaCost = 50f,
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Fire,
                RequiredClass = CharacterClass.Warlock,
                RequiredSpec = Specialization.Destruction,
                UnlockLevel = 1,
                BaseDamage = 200f,
                BreaksStealth = true
            },
            // Incinerate - cast time, 35 mana
            new AbilityData
            {
                AbilityId = "warlock_incinerate",
                AbilityName = "Incinerate",
                Description = "Burns the enemy with fel fire, dealing Fire damage.",
                CastTime = 2f,
                Cooldown = 0f,
                ManaCost = 35f,
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Fire,
                RequiredClass = CharacterClass.Warlock,
                RequiredSpec = Specialization.Destruction,
                UnlockLevel = 10,
                BaseDamage = 90f,
                BreaksStealth = true
            },
            // Immolate - DoT, 25 mana
            new AbilityData
            {
                AbilityId = "warlock_immolate",
                AbilityName = "Immolate",
                Description = "Burns the enemy with fel flames, causing Fire damage and additional Fire damage over 15 seconds.",
                CastTime = 1.5f,
                Cooldown = 0f,
                ManaCost = 25f,
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Debuff,
                DamageType = DamageType.Fire,
                RequiredClass = CharacterClass.Warlock,
                RequiredSpec = Specialization.Destruction,
                UnlockLevel = 20,
                BaseDamage = 150f, // Total DoT damage over duration
                BreaksStealth = true
            },
            // Rain of Fire - AoE channel, 60 mana
            new AbilityData
            {
                AbilityId = "warlock_rain_of_fire",
                AbilityName = "Rain of Fire",
                Description = "Calls down a rain of hellfire, dealing Fire damage to all enemies in the area over 8 seconds.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 60f,
                Range = 35f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Fire,
                RequiredClass = CharacterClass.Warlock,
                RequiredSpec = Specialization.Destruction,
                UnlockLevel = 40,
                BaseDamage = 25f, // Per tick
                IsChanneled = true,
                ChannelDuration = 8f,
                TickInterval = 1f, // 8 ticks over 8 seconds
                BreaksStealth = true
            }
        };

        /// <summary>
        /// Shared Warlock abilities (both specs).
        /// Requirements 7.6: Fear (CC, 30 mana), Shadowfury (AoE stun, 40 mana, 1min CD), 
        /// Summon Demon (summon pet, 100 mana)
        /// </summary>
        public static AbilityData[] GetWarlockSharedAbilities() => new[]
        {
            // Fear - CC, 30 mana
            new AbilityData
            {
                AbilityId = "warlock_fear",
                AbilityName = "Fear",
                Description = "Strikes fear in the enemy, causing it to run in terror for up to 20 seconds. Damage may interrupt the effect.",
                CastTime = 1.7f,
                Cooldown = 0f,
                ManaCost = 30f,
                Range = 30f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Debuff,
                DamageType = DamageType.Shadow,
                RequiredClass = CharacterClass.Warlock,
                UnlockLevel = 5,
                BreaksStealth = true,
                CCType = CCType.Fear // Requirements 11.3, 11.6
            },
            // Shadowfury - AoE stun, 40 mana, 1min CD
            new AbilityData
            {
                AbilityId = "warlock_shadowfury",
                AbilityName = "Shadowfury",
                Description = "Stuns all enemies within 8 yards for 3 seconds.",
                CastTime = 0f,
                Cooldown = 60f, // 1 minute
                ManaCost = 40f,
                Range = 30f,
                RequiresTarget = false,
                AffectedByGCD = true,
                Type = AbilityType.Debuff,
                DamageType = DamageType.Shadow,
                RequiredClass = CharacterClass.Warlock,
                UnlockLevel = 30,
                BaseDamage = 50f,
                BreaksStealth = true,
                CCType = CCType.Stun // Requirements 11.2, 11.6
            },
            // Summon Demon - summon pet, 100 mana
            new AbilityData
            {
                AbilityId = "warlock_summon_demon",
                AbilityName = "Summon Demon",
                Description = "Summons a demon to serve you. Affliction summons an Imp, Destruction summons a Voidwalker.",
                CastTime = 5f,
                Cooldown = 0f,
                ManaCost = 100f,
                Range = 0f,
                RequiresTarget = false,
                AffectedByGCD = true,
                Type = AbilityType.Utility,
                RequiredClass = CharacterClass.Warlock,
                UnlockLevel = 1,
                BreaksStealth = true
            },
            // Shadow Bolt - basic filler spell
            new AbilityData
            {
                AbilityId = "warlock_shadow_bolt",
                AbilityName = "Shadow Bolt",
                Description = "Sends a shadowy bolt at the enemy, causing Shadow damage.",
                CastTime = 2.5f,
                Cooldown = 0f,
                ManaCost = 25f,
                Range = 40f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Shadow,
                RequiredClass = CharacterClass.Warlock,
                UnlockLevel = 1,
                BaseDamage = 85f,
                BreaksStealth = true
            }
        };

        #endregion

        #region Death Knight Abilities

        /// <summary>
        /// Death Knight Blood spec abilities (Tank).
        /// Requirements 8.5: Death Strike (heal, 45 mana), Heart Strike (damage, 30 mana),
        /// Blood Boil (AoE, 35 mana), Vampiric Blood (buff, 0 mana, 2min CD)
        /// </summary>
        public static AbilityData[] GetDeathKnightBloodAbilities() => new[]
        {
            // Death Strike - heal, 45 mana
            // Requirements 8.8: Heal = 25% of damage taken in last 5 seconds (min 10% max HP)
            new AbilityData
            {
                AbilityId = "dk_death_strike",
                AbilityName = "Death Strike",
                Description = "A deadly attack that deals Physical damage and heals you for 25% of damage taken in the last 5 seconds (minimum 10% of max health).",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 45f,
                Range = 5f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Physical,
                RequiredClass = CharacterClass.DeathKnight,
                RequiredSpec = Specialization.Blood,
                UnlockLevel = 1,
                BaseDamage = 80f,
                ThreatMultiplier = 1.3f,
                HealsOnDamage = true, // Special handling for Death Strike healing
                HealOnDamagePercent = 0.25f, // 25% of recent damage taken
                BreaksStealth = true
            },
            // Heart Strike - damage, 30 mana
            new AbilityData
            {
                AbilityId = "dk_heart_strike",
                AbilityName = "Heart Strike",
                Description = "Instantly strike the target and up to 2 nearby enemies, dealing Physical damage.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 30f,
                Range = 5f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Physical,
                RequiredClass = CharacterClass.DeathKnight,
                RequiredSpec = Specialization.Blood,
                UnlockLevel = 10,
                BaseDamage = 65f,
                ThreatMultiplier = 1.4f,
                BreaksStealth = true
            },
            // Blood Boil - AoE, 35 mana
            new AbilityData
            {
                AbilityId = "dk_blood_boil",
                AbilityName = "Blood Boil",
                Description = "Boils the blood of all enemies within 10 yards, dealing Shadow damage.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 35f,
                Range = 10f,
                RequiresTarget = false,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Shadow,
                RequiredClass = CharacterClass.DeathKnight,
                RequiredSpec = Specialization.Blood,
                UnlockLevel = 20,
                BaseDamage = 50f,
                ThreatMultiplier = 1.5f,
                BreaksStealth = true
            },
            // Vampiric Blood - buff, 0 mana, 2min CD
            new AbilityData
            {
                AbilityId = "dk_vampiric_blood",
                AbilityName = "Vampiric Blood",
                Description = "Embrace your undead nature, increasing maximum health by 30% and healing received by 40% for 10 seconds.",
                CastTime = 0f,
                Cooldown = 120f, // 2 minutes
                ManaCost = 0f,
                Range = 0f,
                RequiresTarget = false,
                AffectedByGCD = false,
                Type = AbilityType.Buff,
                RequiredClass = CharacterClass.DeathKnight,
                RequiredSpec = Specialization.Blood,
                UnlockLevel = 40,
                BreaksStealth = true
            }
        };

        /// <summary>
        /// Death Knight Frost spec abilities (DPS).
        /// Requirements 8.6: Obliterate (damage, 45 mana), Frost Strike (damage, 25 mana),
        /// Howling Blast (AoE, 40 mana), Pillar of Frost (buff, 0 mana, 1min CD)
        /// </summary>
        public static AbilityData[] GetDeathKnightFrostAbilities() => new[]
        {
            // Obliterate - damage, 45 mana
            new AbilityData
            {
                AbilityId = "dk_obliterate",
                AbilityName = "Obliterate",
                Description = "A brutal attack that deals massive Physical damage.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 45f,
                Range = 5f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Physical,
                RequiredClass = CharacterClass.DeathKnight,
                RequiredSpec = Specialization.FrostDK,
                UnlockLevel = 1,
                BaseDamage = 120f,
                BreaksStealth = true
            },
            // Frost Strike - damage, 25 mana
            new AbilityData
            {
                AbilityId = "dk_frost_strike",
                AbilityName = "Frost Strike",
                Description = "Chill your weapon with icy power and strike the enemy, dealing Frost damage.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 25f,
                Range = 5f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Frost,
                RequiredClass = CharacterClass.DeathKnight,
                RequiredSpec = Specialization.FrostDK,
                UnlockLevel = 10,
                BaseDamage = 75f,
                BreaksStealth = true
            },
            // Howling Blast - AoE, 40 mana
            new AbilityData
            {
                AbilityId = "dk_howling_blast",
                AbilityName = "Howling Blast",
                Description = "Blast the target with a frigid wind, dealing Frost damage to the target and all enemies within 10 yards.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 40f,
                Range = 30f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Damage,
                DamageType = DamageType.Frost,
                RequiredClass = CharacterClass.DeathKnight,
                RequiredSpec = Specialization.FrostDK,
                UnlockLevel = 20,
                BaseDamage = 55f,
                BreaksStealth = true
            },
            // Pillar of Frost - buff, 0 mana, 1min CD
            new AbilityData
            {
                AbilityId = "dk_pillar_of_frost",
                AbilityName = "Pillar of Frost",
                Description = "Calls upon the power of Frost to increase your Strength by 25% for 12 seconds.",
                CastTime = 0f,
                Cooldown = 60f, // 1 minute
                ManaCost = 0f,
                Range = 0f,
                RequiresTarget = false,
                AffectedByGCD = false,
                Type = AbilityType.Buff,
                RequiredClass = CharacterClass.DeathKnight,
                RequiredSpec = Specialization.FrostDK,
                UnlockLevel = 40,
                BreaksStealth = true
            }
        };

        /// <summary>
        /// Shared Death Knight abilities (both specs).
        /// Requirements 8.7: Death Grip (pull, 30 mana), Chains of Ice (slow, 25 mana),
        /// Mind Freeze (interrupt, 20 mana)
        /// </summary>
        public static AbilityData[] GetDeathKnightSharedAbilities() => new[]
        {
            // Death Grip - pull, 30 mana
            // Requirements 11.4: Move target to caster position
            new AbilityData
            {
                AbilityId = "dk_death_grip",
                AbilityName = "Death Grip",
                Description = "Harness the unholy energy that surrounds and binds all matter, drawing the target toward you.",
                CastTime = 0f,
                Cooldown = 25f,
                ManaCost = 30f,
                Range = 30f,
                RequiresTarget = true,
                AffectedByGCD = false,
                Type = AbilityType.Utility,
                RequiredClass = CharacterClass.DeathKnight,
                UnlockLevel = 1,
                ThreatMultiplier = 1.2f,
                BreaksStealth = true,
                IsPullEffect = true // Requirements 11.4
            },
            // Chains of Ice - slow, 25 mana
            new AbilityData
            {
                AbilityId = "dk_chains_of_ice",
                AbilityName = "Chains of Ice",
                Description = "Shackles the target with frozen chains, reducing movement speed by 70% for 8 seconds.",
                CastTime = 0f,
                Cooldown = 0f,
                ManaCost = 25f,
                Range = 30f,
                RequiresTarget = true,
                AffectedByGCD = true,
                Type = AbilityType.Debuff,
                DamageType = DamageType.Frost,
                RequiredClass = CharacterClass.DeathKnight,
                UnlockLevel = 5,
                BaseDamage = 20f, // Minor damage
                BreaksStealth = true,
                CCType = CCType.Slow // Requirements 11.1, 11.6
            },
            // Mind Freeze - interrupt, 20 mana
            new AbilityData
            {
                AbilityId = "dk_mind_freeze",
                AbilityName = "Mind Freeze",
                Description = "Smash the target's mind with cold, interrupting spellcasting and preventing any spell in that school from being cast for 3 seconds.",
                CastTime = 0f,
                Cooldown = 15f,
                ManaCost = 20f,
                Range = 15f,
                RequiresTarget = true,
                AffectedByGCD = false,
                Type = AbilityType.Utility,
                RequiredClass = CharacterClass.DeathKnight,
                UnlockLevel = 15,
                BreaksStealth = true
            },
            // Raise Dead - basic filler ability (summon ghoul)
            new AbilityData
            {
                AbilityId = "dk_raise_dead",
                AbilityName = "Raise Dead",
                Description = "Raises a ghoul to fight by your side for 1 minute.",
                CastTime = 0f,
                Cooldown = 30f,
                ManaCost = 40f,
                Range = 0f,
                RequiresTarget = false,
                AffectedByGCD = true,
                Type = AbilityType.Utility,
                RequiredClass = CharacterClass.DeathKnight,
                UnlockLevel = 10,
                BreaksStealth = true
            }
        };

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get all abilities for a specific class and specialization.
        /// </summary>
        public static List<AbilityData> GetAllAbilitiesForSpec(CharacterClass charClass, Specialization spec)
        {
            var abilities = new List<AbilityData>();

            switch (charClass)
            {
                case CharacterClass.Warrior:
                    abilities.AddRange(GetWarriorSharedAbilities());
                    abilities.AddRange(spec == Specialization.Protection 
                        ? GetWarriorProtectionAbilities() 
                        : GetWarriorArmsAbilities());
                    break;

                case CharacterClass.Mage:
                    abilities.AddRange(GetMageSharedAbilities());
                    abilities.AddRange(spec == Specialization.Fire 
                        ? GetMageFireAbilities() 
                        : GetMageFrostAbilities());
                    break;

                case CharacterClass.Priest:
                    abilities.AddRange(GetPriestSharedAbilities());
                    abilities.AddRange(spec == Specialization.Holy 
                        ? GetPriestHolyAbilities() 
                        : GetPriestShadowAbilities());
                    break;

                case CharacterClass.Paladin:
                    abilities.AddRange(GetPaladinSharedAbilities());
                    abilities.AddRange(spec switch
                    {
                        Specialization.ProtectionPaladin => GetPaladinProtectionAbilities(),
                        Specialization.HolyPaladin => GetPaladinHolyAbilities(),
                        Specialization.Retribution => GetPaladinRetributionAbilities(),
                        _ => GetPaladinRetributionAbilities()
                    });
                    break;

                case CharacterClass.Rogue:
                    abilities.AddRange(GetRogueSharedAbilities());
                    abilities.AddRange(spec == Specialization.Assassination 
                        ? GetRogueAssassinationAbilities() 
                        : GetRogueCombatAbilities());
                    break;

                case CharacterClass.Hunter:
                    abilities.AddRange(GetHunterSharedAbilities());
                    abilities.AddRange(spec == Specialization.BeastMastery 
                        ? GetHunterBeastMasteryAbilities() 
                        : GetHunterMarksmanshipAbilities());
                    break;

                case CharacterClass.Warlock:
                    abilities.AddRange(GetWarlockSharedAbilities());
                    abilities.AddRange(spec == Specialization.Affliction 
                        ? GetWarlockAfflictionAbilities() 
                        : GetWarlockDestructionAbilities());
                    break;

                case CharacterClass.DeathKnight:
                    abilities.AddRange(GetDeathKnightSharedAbilities());
                    abilities.AddRange(spec == Specialization.Blood 
                        ? GetDeathKnightBloodAbilities() 
                        : GetDeathKnightFrostAbilities());
                    break;
            }

            return abilities;
        }

        #endregion
    }
}
