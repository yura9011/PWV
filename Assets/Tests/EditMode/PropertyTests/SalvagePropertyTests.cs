using NUnit.Framework;
using EtherDomes.Progression;
using EtherDomes.Data;
using System.Linq;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for the Salvage System.
    /// </summary>
    [TestFixture]
    public class SalvagePropertyTests
    {
        private SalvageSystem _salvageSystem;

        [SetUp]
        public void SetUp()
        {
            _salvageSystem = new SalvageSystem();
        }

        /// <summary>
        /// Feature: mvp-10-features, Property 14: Salvage Produces Materials
        /// For any item of Rare+ rarity, salvaging SHALL produce at least 2 materials.
        /// Validates: Requirements 4.7, 4.8
        /// </summary>
        [Test]
        [Repeat(100)]
        public void SalvageProducesMaterials_RarePlus_AtLeast2Materials()
        {
            // Arrange
            var item = CreateRandomItem();
            // Ensure Rare or higher
            item.Rarity = (ItemRarity)UnityEngine.Random.Range((int)ItemRarity.Rare, 5);

            // Act
            var result = _salvageSystem.Salvage(item);

            // Assert
            int totalMaterials = result.Materials.Values.Sum();
            Assert.That(totalMaterials, Is.GreaterThanOrEqualTo(2),
                $"Salvaging {item.Rarity} item should produce at least 2 materials, got {totalMaterials}");
        }

        /// <summary>
        /// Property 14: Common items produce at least 1 material
        /// </summary>
        [Test]
        [Repeat(100)]
        public void SalvageProducesMaterials_Common_AtLeast1Material()
        {
            // Arrange
            var item = CreateRandomItem();
            item.Rarity = ItemRarity.Common;

            // Act
            var result = _salvageSystem.Salvage(item);

            // Assert
            int totalMaterials = result.Materials.Values.Sum();
            Assert.That(totalMaterials, Is.GreaterThanOrEqualTo(1),
                "Salvaging Common item should produce at least 1 material");
        }

        /// <summary>
        /// Property: Higher rarity produces more materials
        /// </summary>
        [Test]
        public void SalvageYield_HigherRarity_MoreMaterials()
        {
            // Get min yields for each rarity
            int commonMin = SalvageSystem.GetMinYield(ItemRarity.Common);
            int rareMin = SalvageSystem.GetMinYield(ItemRarity.Rare);
            int epicMin = SalvageSystem.GetMinYield(ItemRarity.Epic);
            int legendaryMin = SalvageSystem.GetMinYield(ItemRarity.Legendary);

            // Assert
            Assert.That(rareMin, Is.GreaterThan(commonMin),
                "Rare should have higher min yield than Common");
            Assert.That(epicMin, Is.GreaterThan(rareMin),
                "Epic should have higher min yield than Rare");
            Assert.That(legendaryMin, Is.GreaterThan(epicMin),
                "Legendary should have higher min yield than Epic");
        }

        /// <summary>
        /// Property: Salvage result is successful for valid items
        /// </summary>
        [Test]
        [Repeat(100)]
        public void Salvage_ValidItem_Success()
        {
            // Arrange
            var item = CreateRandomItem();

            // Act
            var result = _salvageSystem.Salvage(item);

            // Assert
            Assert.That(result.Success, Is.True,
                "Salvaging valid item should succeed");
            Assert.That(result.SalvagedItem, Is.EqualTo(item),
                "Result should reference salvaged item");
        }

        /// <summary>
        /// Property: Salvage null item fails
        /// </summary>
        [Test]
        public void Salvage_NullItem_Fails()
        {
            // Act
            var result = _salvageSystem.Salvage(null);

            // Assert
            Assert.That(result.Success, Is.False,
                "Salvaging null item should fail");
        }

        /// <summary>
        /// Property: PreviewSalvage returns same material types as Salvage
        /// </summary>
        [Test]
        [Repeat(100)]
        public void PreviewSalvage_SameMaterialTypes()
        {
            // Arrange
            var item = CreateRandomItem();

            // Act
            var preview = _salvageSystem.PreviewSalvage(item);
            var result = _salvageSystem.Salvage(item);

            // Assert - Same material types (quantities may vary due to randomness)
            Assert.That(result.Materials.Keys, Is.EquivalentTo(preview.Keys),
                "Preview and salvage should produce same material types");
        }

        /// <summary>
        /// Property: CanSalvage returns true for all valid items
        /// </summary>
        [Test]
        [Repeat(100)]
        public void CanSalvage_ValidItem_ReturnsTrue()
        {
            // Arrange
            var item = CreateRandomItem();

            // Act
            bool canSalvage = _salvageSystem.CanSalvage(item);

            // Assert
            Assert.That(canSalvage, Is.True,
                "All valid items should be salvageable");
        }

        /// <summary>
        /// Property: CanSalvage returns false for null
        /// </summary>
        [Test]
        public void CanSalvage_Null_ReturnsFalse()
        {
            // Act
            bool canSalvage = _salvageSystem.CanSalvage(null);

            // Assert
            Assert.That(canSalvage, Is.False,
                "Null items should not be salvageable");
        }

        /// <summary>
        /// Property: OnItemSalvaged fires on successful salvage
        /// </summary>
        [Test]
        public void OnItemSalvaged_FiresOnSuccess()
        {
            // Arrange
            var item = CreateRandomItem();
            bool eventFired = false;
            SalvageResult reportedResult = default;
            
            _salvageSystem.OnItemSalvaged += (result) =>
            {
                eventFired = true;
                reportedResult = result;
            };

            // Act
            _salvageSystem.Salvage(item);

            // Assert
            Assert.That(eventFired, Is.True, "OnItemSalvaged should fire");
            Assert.That(reportedResult.Success, Is.True, "Reported result should be successful");
        }

        /// <summary>
        /// Property: Material yields are within expected ranges
        /// </summary>
        [Test]
        [Repeat(100)]
        public void MaterialYields_WithinExpectedRanges()
        {
            // Arrange
            var item = CreateRandomItem();
            item.ItemLevel = 1; // Low level to avoid bonus materials

            // Act
            var result = _salvageSystem.Salvage(item);

            // Assert
            int minYield = SalvageSystem.GetMinYield(item.Rarity);
            int maxYield = SalvageSystem.GetMaxYield(item.Rarity);

            // Get primary material yield (first material type)
            int primaryYield = result.Materials.Values.First();

            // Note: Rare+ items get bonus lower-tier materials, so we check primary only
            Assert.That(primaryYield, Is.InRange(minYield, maxYield + 1), // +1 for potential item level bonus
                $"Primary material yield for {item.Rarity} should be in range [{minYield}, {maxYield}]");
        }

        /// <summary>
        /// Property: Rare+ items produce lower tier materials too
        /// </summary>
        [Test]
        [Repeat(100)]
        public void RarePlusItems_ProduceLowerTierMaterials()
        {
            // Arrange
            var item = CreateRandomItem();
            item.Rarity = (ItemRarity)UnityEngine.Random.Range((int)ItemRarity.Rare, 5);

            // Act
            var result = _salvageSystem.Salvage(item);

            // Assert
            Assert.That(result.Materials.Count, Is.GreaterThanOrEqualTo(2),
                "Rare+ items should produce at least 2 types of materials");
        }

        private ItemData CreateRandomItem()
        {
            return new ItemData
            {
                ItemId = System.Guid.NewGuid().ToString(),
                ItemName = "Test Item",
                ItemLevel = UnityEngine.Random.Range(1, 60),
                Rarity = (ItemRarity)UnityEngine.Random.Range(0, 5),
                Slot = (EquipmentSlot)UnityEngine.Random.Range(0, 8)
            };
        }
    }
}
