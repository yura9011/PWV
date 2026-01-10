using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using EtherDomes.Classes;
using EtherDomes.Classes.Abilities;
using EtherDomes.Data;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for the Class System.
    /// </summary>
    [TestFixture]
    public class ClassSystemPropertyTests
    {
        /// <summary>
        /// Feature: mvp-10-features, Property 6: Spec Change Replaces Abilities
        /// For any class with multiple specs, spec-specific abilities SHALL NOT be shared between specs.
        /// Validates: Requirements 2.1
        /// </summary>
        [Test]
        public void SpecChangeReplacesAbilities_WarriorSpecs_NoSharedSpecAbilities()
        {
            // Arrange
            var protectionAbilities = ClassAbilityDefinitions.GetWarriorProtectionAbilities();
            var armsAbilities = ClassAbilityDefinitions.GetWarriorArmsAbilities();
            
            // Act - Get ability IDs
            var protectionIds = protectionAbilities.Select(a => a.AbilityId).ToHashSet();
            var armsIds = armsAbilities.Select(a => a.AbilityId).ToHashSet();
            
            // Assert - No intersection
            var sharedAbilities = protectionIds.Intersect(armsIds).ToList();
            Assert.That(sharedAbilities, Is.Empty,
                $"Warrior Protection and Arms should not share spec-specific abilities. Shared: {string.Join(", ", sharedAbilities)}");
        }
        
        /// <summary>
        /// Property 6: Mage specs should not share spec-specific abilities.
        /// </summary>
        [Test]
        public void SpecChangeReplacesAbilities_MageSpecs_NoSharedSpecAbilities()
        {
            // Arrange
            var fireAbilities = ClassAbilityDefinitions.GetMageFireAbilities();
            var frostAbilities = ClassAbilityDefinitions.GetMageFrostAbilities();
            
            // Act
            var fireIds = fireAbilities.Select(a => a.AbilityId).ToHashSet();
            var frostIds = frostAbilities.Select(a => a.AbilityId).ToHashSet();
            
            // Assert
            var sharedAbilities = fireIds.Intersect(frostIds).ToList();
            Assert.That(sharedAbilities, Is.Empty,
                $"Mage Fire and Frost should not share spec-specific abilities. Shared: {string.Join(", ", sharedAbilities)}");
        }
        
        /// <summary>
        /// Property 6: Priest specs should not share spec-specific abilities.
        /// </summary>
        [Test]
        public void SpecChangeReplacesAbilities_PriestSpecs_NoSharedSpecAbilities()
        {
            // Arrange
            var holyAbilities = ClassAbilityDefinitions.GetPriestHolyAbilities();
            var shadowAbilities = ClassAbilityDefinitions.GetPriestShadowAbilities();
            
            // Act
            var holyIds = holyAbilities.Select(a => a.AbilityId).ToHashSet();
            var shadowIds = shadowAbilities.Select(a => a.AbilityId).ToHashSet();
            
            // Assert
            var sharedAbilities = holyIds.Intersect(shadowIds).ToList();
            Assert.That(sharedAbilities, Is.Empty,
                $"Priest Holy and Shadow should not share spec-specific abilities. Shared: {string.Join(", ", sharedAbilities)}");
        }
        
        /// <summary>
        /// Property 6: Paladin specs should not share spec-specific abilities.
        /// </summary>
        [Test]
        public void SpecChangeReplacesAbilities_PaladinSpecs_NoSharedSpecAbilities()
        {
            // Arrange
            var protectionAbilities = ClassAbilityDefinitions.GetPaladinProtectionAbilities();
            var holyAbilities = ClassAbilityDefinitions.GetPaladinHolyAbilities();
            var retributionAbilities = ClassAbilityDefinitions.GetPaladinRetributionAbilities();
            
            // Act
            var protectionIds = protectionAbilities.Select(a => a.AbilityId).ToHashSet();
            var holyIds = holyAbilities.Select(a => a.AbilityId).ToHashSet();
            var retributionIds = retributionAbilities.Select(a => a.AbilityId).ToHashSet();
            
            // Assert - No pairwise intersections
            var protHolyShared = protectionIds.Intersect(holyIds).ToList();
            var protRetShared = protectionIds.Intersect(retributionIds).ToList();
            var holyRetShared = holyIds.Intersect(retributionIds).ToList();
            
            Assert.That(protHolyShared, Is.Empty,
                $"Paladin Protection and Holy should not share abilities. Shared: {string.Join(", ", protHolyShared)}");
            Assert.That(protRetShared, Is.Empty,
                $"Paladin Protection and Retribution should not share abilities. Shared: {string.Join(", ", protRetShared)}");
            Assert.That(holyRetShared, Is.Empty,
                $"Paladin Holy and Retribution should not share abilities. Shared: {string.Join(", ", holyRetShared)}");
        }
        
        /// <summary>
        /// Property 6: Using helper method to verify no shared spec abilities.
        /// </summary>
        [Test]
        public void SpecChangeReplacesAbilities_AllClasses_NoSharedSpecAbilities()
        {
            // Test all class/spec combinations
            Assert.That(ClassSystem.SpecsShareAbilities(CharacterClass.Warrior, Specialization.Protection, Specialization.Arms), 
                Is.False, "Warrior specs should not share spec-specific abilities");
            
            Assert.That(ClassSystem.SpecsShareAbilities(CharacterClass.Mage, Specialization.Fire, Specialization.Frost), 
                Is.False, "Mage specs should not share spec-specific abilities");
            
            Assert.That(ClassSystem.SpecsShareAbilities(CharacterClass.Priest, Specialization.Holy, Specialization.Shadow), 
                Is.False, "Priest specs should not share spec-specific abilities");
            
            Assert.That(ClassSystem.SpecsShareAbilities(CharacterClass.Paladin, Specialization.ProtectionPaladin, Specialization.HolyPaladin), 
                Is.False, "Paladin Protection and Holy should not share spec-specific abilities");
            
            Assert.That(ClassSystem.SpecsShareAbilities(CharacterClass.Paladin, Specialization.ProtectionPaladin, Specialization.Retribution), 
                Is.False, "Paladin Protection and Retribution should not share spec-specific abilities");
            
            Assert.That(ClassSystem.SpecsShareAbilities(CharacterClass.Paladin, Specialization.HolyPaladin, Specialization.Retribution), 
                Is.False, "Paladin Holy and Retribution should not share spec-specific abilities");
        }
        
        /// <summary>
        /// Property: Each spec should have at least 4 unique abilities.
        /// </summary>
        [Test]
        public void EachSpec_HasAtLeast4UniqueAbilities()
        {
            // Warrior
            Assert.That(ClassAbilityDefinitions.GetWarriorProtectionAbilities().Length, Is.GreaterThanOrEqualTo(4),
                "Warrior Protection should have at least 4 abilities");
            Assert.That(ClassAbilityDefinitions.GetWarriorArmsAbilities().Length, Is.GreaterThanOrEqualTo(4),
                "Warrior Arms should have at least 4 abilities");
            
            // Mage
            Assert.That(ClassAbilityDefinitions.GetMageFireAbilities().Length, Is.GreaterThanOrEqualTo(4),
                "Mage Fire should have at least 4 abilities");
            Assert.That(ClassAbilityDefinitions.GetMageFrostAbilities().Length, Is.GreaterThanOrEqualTo(4),
                "Mage Frost should have at least 4 abilities");
            
            // Priest
            Assert.That(ClassAbilityDefinitions.GetPriestHolyAbilities().Length, Is.GreaterThanOrEqualTo(4),
                "Priest Holy should have at least 4 abilities");
            Assert.That(ClassAbilityDefinitions.GetPriestShadowAbilities().Length, Is.GreaterThanOrEqualTo(4),
                "Priest Shadow should have at least 4 abilities");
            
            // Paladin
            Assert.That(ClassAbilityDefinitions.GetPaladinProtectionAbilities().Length, Is.GreaterThanOrEqualTo(4),
                "Paladin Protection should have at least 4 abilities");
            Assert.That(ClassAbilityDefinitions.GetPaladinHolyAbilities().Length, Is.GreaterThanOrEqualTo(4),
                "Paladin Holy should have at least 4 abilities");
            Assert.That(ClassAbilityDefinitions.GetPaladinRetributionAbilities().Length, Is.GreaterThanOrEqualTo(4),
                "Paladin Retribution should have at least 4 abilities");
        }
        
        /// <summary>
        /// Property: GetAllAbilitiesForSpec returns correct abilities.
        /// </summary>
        [Test]
        [Repeat(100)]
        public void GetAllAbilitiesForSpec_ReturnsCorrectAbilities()
        {
            // Arrange - Pick random class
            var classes = new[] { CharacterClass.Warrior, CharacterClass.Mage, CharacterClass.Priest, CharacterClass.Paladin };
            var charClass = classes[UnityEngine.Random.Range(0, classes.Length)];
            
            // Get valid specs for this class
            Specialization[] specs = charClass switch
            {
                CharacterClass.Warrior => new[] { Specialization.Protection, Specialization.Arms },
                CharacterClass.Mage => new[] { Specialization.Fire, Specialization.Frost },
                CharacterClass.Priest => new[] { Specialization.Holy, Specialization.Shadow },
                CharacterClass.Paladin => new[] { Specialization.ProtectionPaladin, Specialization.HolyPaladin, Specialization.Retribution },
                _ => new[] { Specialization.Arms }
            };
            
            var spec = specs[UnityEngine.Random.Range(0, specs.Length)];
            
            // Act
            var abilities = ClassAbilityDefinitions.GetAllAbilitiesForSpec(charClass, spec);
            
            // Assert - Should have abilities
            Assert.That(abilities.Count, Is.GreaterThan(0),
                $"{charClass}/{spec} should have abilities");
            
            // All abilities should be valid for this class
            foreach (var ability in abilities)
            {
                Assert.That(ability.RequiredClass, Is.EqualTo(charClass),
                    $"Ability {ability.AbilityId} should be for class {charClass}");
            }
        }
        
        /// <summary>
        /// Property: Shared abilities are included in all specs of a class.
        /// </summary>
        [Test]
        public void SharedAbilities_IncludedInAllSpecs()
        {
            // Warrior shared abilities
            var warriorShared = ClassAbilityDefinitions.GetWarriorSharedAbilities();
            var warriorProtAll = ClassAbilityDefinitions.GetAllAbilitiesForSpec(CharacterClass.Warrior, Specialization.Protection);
            var warriorArmsAll = ClassAbilityDefinitions.GetAllAbilitiesForSpec(CharacterClass.Warrior, Specialization.Arms);
            
            foreach (var shared in warriorShared)
            {
                Assert.That(warriorProtAll.Any(a => a.AbilityId == shared.AbilityId), Is.True,
                    $"Warrior Protection should include shared ability {shared.AbilityId}");
                Assert.That(warriorArmsAll.Any(a => a.AbilityId == shared.AbilityId), Is.True,
                    $"Warrior Arms should include shared ability {shared.AbilityId}");
            }
            
            // Mage shared abilities
            var mageShared = ClassAbilityDefinitions.GetMageSharedAbilities();
            var mageFireAll = ClassAbilityDefinitions.GetAllAbilitiesForSpec(CharacterClass.Mage, Specialization.Fire);
            var mageFrostAll = ClassAbilityDefinitions.GetAllAbilitiesForSpec(CharacterClass.Mage, Specialization.Frost);
            
            foreach (var shared in mageShared)
            {
                Assert.That(mageFireAll.Any(a => a.AbilityId == shared.AbilityId), Is.True,
                    $"Mage Fire should include shared ability {shared.AbilityId}");
                Assert.That(mageFrostAll.Any(a => a.AbilityId == shared.AbilityId), Is.True,
                    $"Mage Frost should include shared ability {shared.AbilityId}");
            }
        }
    }
}
