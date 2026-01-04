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
            }

            return abilities;
        }

        #endregion
    }
}
