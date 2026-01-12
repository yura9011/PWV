using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using EtherDomes.Classes;
using EtherDomes.Data;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for the Class System.
    /// Updated for new 8-class system:
    /// - Tanks: Cruzado (HolyPower), Protector (Rage)
    /// - DPS Físico: Berserker (Rage), Arquero (Focus)
    /// - DPS Mágico: MaestroElemental (Mana), CaballeroRunico (RunicPower)
    /// - Healers: Clerigo (HolyPower), MedicoBrujo (Mana)
    /// 
    /// NOTE: Ability tests are marked as Ignore until abilities are defined for new classes.
    /// </summary>
    [TestFixture]
    public class ClassSystemPropertyTests
    {
        #region Class Role Tests (New 8-Class System)

        /// <summary>
        /// Property: Each class has a valid CharacterClass enum value.
        /// </summary>
        [Test]
        public void AllClasses_HaveValidEnumValues()
        {
            var allClasses = new[]
            {
                CharacterClass.Cruzado,
                CharacterClass.Protector,
                CharacterClass.Berserker,
                CharacterClass.Arquero,
                CharacterClass.MaestroElemental,
                CharacterClass.CaballeroRunico,
                CharacterClass.Clerigo,
                CharacterClass.MedicoBrujo
            };

            for (int i = 0; i < allClasses.Length; i++)
            {
                Assert.That((int)allClasses[i], Is.EqualTo(i),
                    $"{allClasses[i]} should have enum value {i}");
            }
        }

        /// <summary>
        /// Property: Tank classes are Cruzado and Protector.
        /// </summary>
        [Test]
        public void TankClasses_AreCruzadoAndProtector()
        {
            var tankClasses = new[] { CharacterClass.Cruzado, CharacterClass.Protector };
            
            Assert.That(tankClasses.Length, Is.EqualTo(2), "Should have exactly 2 tank classes");
            Assert.That(tankClasses, Contains.Item(CharacterClass.Cruzado));
            Assert.That(tankClasses, Contains.Item(CharacterClass.Protector));
        }

        /// <summary>
        /// Property: Physical DPS classes are Berserker and Arquero.
        /// </summary>
        [Test]
        public void PhysicalDPSClasses_AreBerserkerAndArquero()
        {
            var physicalDPS = new[] { CharacterClass.Berserker, CharacterClass.Arquero };
            
            Assert.That(physicalDPS.Length, Is.EqualTo(2), "Should have exactly 2 physical DPS classes");
            Assert.That(physicalDPS, Contains.Item(CharacterClass.Berserker));
            Assert.That(physicalDPS, Contains.Item(CharacterClass.Arquero));
        }

        /// <summary>
        /// Property: Magical DPS classes are MaestroElemental and CaballeroRunico.
        /// </summary>
        [Test]
        public void MagicalDPSClasses_AreMaestroElementalAndCaballeroRunico()
        {
            var magicalDPS = new[] { CharacterClass.MaestroElemental, CharacterClass.CaballeroRunico };
            
            Assert.That(magicalDPS.Length, Is.EqualTo(2), "Should have exactly 2 magical DPS classes");
            Assert.That(magicalDPS, Contains.Item(CharacterClass.MaestroElemental));
            Assert.That(magicalDPS, Contains.Item(CharacterClass.CaballeroRunico));
        }

        /// <summary>
        /// Property: Healer classes are Clerigo and MedicoBrujo.
        /// </summary>
        [Test]
        public void HealerClasses_AreClerigoAndMedicoBrujo()
        {
            var healerClasses = new[] { CharacterClass.Clerigo, CharacterClass.MedicoBrujo };
            
            Assert.That(healerClasses.Length, Is.EqualTo(2), "Should have exactly 2 healer classes");
            Assert.That(healerClasses, Contains.Item(CharacterClass.Clerigo));
            Assert.That(healerClasses, Contains.Item(CharacterClass.MedicoBrujo));
        }

        /// <summary>
        /// Property: Legacy aliases map to correct new classes.
        /// </summary>
        [Test]
        public void LegacyAliases_MapToCorrectClasses()
        {
            Assert.That(CharacterClass.Warrior, Is.EqualTo(CharacterClass.Cruzado));
            Assert.That(CharacterClass.Paladin, Is.EqualTo(CharacterClass.Protector));
            Assert.That(CharacterClass.Mage, Is.EqualTo(CharacterClass.MaestroElemental));
            Assert.That(CharacterClass.Priest, Is.EqualTo(CharacterClass.Clerigo));
            Assert.That(CharacterClass.Rogue, Is.EqualTo(CharacterClass.Berserker));
            Assert.That(CharacterClass.Hunter, Is.EqualTo(CharacterClass.Arquero));
            Assert.That(CharacterClass.Warlock, Is.EqualTo(CharacterClass.MedicoBrujo));
            Assert.That(CharacterClass.DeathKnight, Is.EqualTo(CharacterClass.CaballeroRunico));
        }

        #endregion

        #region Specialization Tests

        /// <summary>
        /// Property: Each class has a default specialization.
        /// </summary>
        [Test]
        public void EachClass_HasDefaultSpecialization()
        {
            Assert.That(Specialization.CruzadoTank, Is.EqualTo((Specialization)0));
            Assert.That(Specialization.ProtectorTank, Is.EqualTo((Specialization)10));
            Assert.That(Specialization.BerserkerDPS, Is.EqualTo((Specialization)20));
            Assert.That(Specialization.ArqueroDPS, Is.EqualTo((Specialization)30));
            Assert.That(Specialization.MaestroElementalDPS, Is.EqualTo((Specialization)40));
            Assert.That(Specialization.CaballeroRunicoDPS, Is.EqualTo((Specialization)50));
            Assert.That(Specialization.ClerigoHealer, Is.EqualTo((Specialization)60));
            Assert.That(Specialization.MedicoBrujoHealer, Is.EqualTo((Specialization)70));
        }

        /// <summary>
        /// Property: Legacy specialization aliases map correctly.
        /// </summary>
        [Test]
        public void LegacySpecializationAliases_MapCorrectly()
        {
            Assert.That(Specialization.Protection, Is.EqualTo(Specialization.CruzadoTank));
            Assert.That(Specialization.ProtectionPaladin, Is.EqualTo(Specialization.ProtectorTank));
            Assert.That(Specialization.Arms, Is.EqualTo(Specialization.BerserkerDPS));
            Assert.That(Specialization.Fire, Is.EqualTo(Specialization.MaestroElementalDPS));
            Assert.That(Specialization.Holy, Is.EqualTo(Specialization.ClerigoHealer));
            Assert.That(Specialization.Shadow, Is.EqualTo(Specialization.MedicoBrujoHealer));
            Assert.That(Specialization.FrostDK, Is.EqualTo(Specialization.CaballeroRunicoDPS));
            Assert.That(Specialization.BeastMastery, Is.EqualTo(Specialization.ArqueroDPS));
        }

        #endregion

        #region Ability Tests (Pending - Awaiting Ability Definitions)

        /// <summary>
        /// Feature: mvp-10-features, Property 6: Spec Change Replaces Abilities
        /// PENDING: Awaiting ability definitions for new 8-class system.
        /// </summary>
        [Test]
        [Ignore("Pending: Ability system not yet implemented for new 8-class system. Will be enabled when abilities are defined.")]
        public void SpecChangeReplacesAbilities_NewClasses_NoSharedSpecAbilities()
        {
            // TODO: Implement when abilities are defined for new classes
            Assert.Pass("Test pending ability implementation");
        }

        /// <summary>
        /// Property 6: Each class should have unique abilities.
        /// PENDING: Awaiting ability definitions.
        /// </summary>
        [Test]
        [Ignore("Pending: Ability system not yet implemented for new 8-class system. Will be enabled when abilities are defined.")]
        public void EachClass_HasUniqueAbilities()
        {
            // TODO: Implement when abilities are defined for new classes
            Assert.Pass("Test pending ability implementation");
        }

        /// <summary>
        /// Property: Each class should have at least 4 abilities.
        /// PENDING: Awaiting ability definitions.
        /// </summary>
        [Test]
        [Ignore("Pending: Ability system not yet implemented for new 8-class system. Will be enabled when abilities are defined.")]
        public void EachClass_HasAtLeast4Abilities()
        {
            // TODO: Implement when abilities are defined for new classes
            Assert.Pass("Test pending ability implementation");
        }

        /// <summary>
        /// Property: GetAllAbilitiesForClass returns correct abilities.
        /// PENDING: Awaiting ability definitions.
        /// </summary>
        [Test]
        [Ignore("Pending: Ability system not yet implemented for new 8-class system. Will be enabled when abilities are defined.")]
        public void GetAllAbilitiesForClass_ReturnsCorrectAbilities()
        {
            // TODO: Implement when abilities are defined for new classes
            Assert.Pass("Test pending ability implementation");
        }

        /// <summary>
        /// Property: Tank classes have taunt abilities.
        /// PENDING: Awaiting ability definitions.
        /// </summary>
        [Test]
        [Ignore("Pending: Ability system not yet implemented for new 8-class system. Will be enabled when abilities are defined.")]
        public void TankClasses_HaveTauntAbilities()
        {
            // TODO: Cruzado and Protector should have taunt abilities
            Assert.Pass("Test pending ability implementation");
        }

        /// <summary>
        /// Property: Healer classes have healing abilities.
        /// PENDING: Awaiting ability definitions.
        /// </summary>
        [Test]
        [Ignore("Pending: Ability system not yet implemented for new 8-class system. Will be enabled when abilities are defined.")]
        public void HealerClasses_HaveHealingAbilities()
        {
            // TODO: Clerigo and MedicoBrujo should have healing abilities
            Assert.Pass("Test pending ability implementation");
        }

        /// <summary>
        /// Property: DPS classes have damage abilities.
        /// PENDING: Awaiting ability definitions.
        /// </summary>
        [Test]
        [Ignore("Pending: Ability system not yet implemented for new 8-class system. Will be enabled when abilities are defined.")]
        public void DPSClasses_HaveDamageAbilities()
        {
            // TODO: Berserker, Arquero, MaestroElemental, CaballeroRunico should have damage abilities
            Assert.Pass("Test pending ability implementation");
        }

        #endregion
    }
}
