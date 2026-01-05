using NUnit.Framework;
using EtherDomes.Progression;
using EtherDomes.Data;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for the Durability System.
    /// </summary>
    [TestFixture]
    public class DurabilityPropertyTests
    {
        private DurabilitySystem _durabilitySystem;

        [SetUp]
        public void SetUp()
        {
            _durabilitySystem = new DurabilitySystem();
        }

        /// <summary>
        /// Feature: mvp-10-features, Property 24: Broken Item Stat Penalty
        /// For any item with durability = 0, GetStatPenalty SHALL return 0.5 (50% stats).
        /// Validates: Requirements 8.6
        /// </summary>
        [Test]
        [Repeat(100)]
        public void BrokenItemStatPenalty_DurabilityZero_Returns50Percent()
        {
            // Arrange
            var item = CreateRandomItem();
            item.MaxDurability = UnityEngine.Random.Range(1, 200);
            item.CurrentDurability = 0; // Broken

            // Act
            float penalty = _durabilitySystem.GetStatPenalty(item);

            // Assert
            Assert.That(penalty, Is.EqualTo(0.5f),
                "Broken item (durability=0) should have 50% stat penalty");
        }

        /// <summary>
        /// Property 24: Items with durability > 0 have no penalty
        /// </summary>
        [Test]
        [Repeat(100)]
        public void StatPenalty_DurabilityAboveZero_NoPenalty()
        {
            // Arrange
            var item = CreateRandomItem();
            item.MaxDurability = UnityEngine.Random.Range(10, 200);
            item.CurrentDurability = UnityEngine.Random.Range(1, item.MaxDurability + 1);

            // Act
            float penalty = _durabilitySystem.GetStatPenalty(item);

            // Assert
            Assert.That(penalty, Is.EqualTo(1f),
                $"Item with durability {item.CurrentDurability}/{item.MaxDurability} should have no penalty");
        }

        /// <summary>
        /// Property: Degrading durability reduces CurrentDurability
        /// </summary>
        [Test]
        [Repeat(100)]
        public void DegradeDurability_ReducesCurrentDurability()
        {
            // Arrange
            var item = CreateRandomItem();
            item.MaxDurability = 100;
            item.CurrentDurability = UnityEngine.Random.Range(10, 100);
            int initialDurability = item.CurrentDurability;
            int degradeAmount = UnityEngine.Random.Range(1, 10);

            // Act
            _durabilitySystem.DegradeDurability(item, degradeAmount);

            // Assert
            int expectedDurability = System.Math.Max(0, initialDurability - degradeAmount);
            Assert.That(item.CurrentDurability, Is.EqualTo(expectedDurability),
                $"Durability should decrease from {initialDurability} by {degradeAmount}");
        }

        /// <summary>
        /// Property: Durability never goes below 0
        /// </summary>
        [Test]
        [Repeat(100)]
        public void DegradeDurability_NeverBelowZero()
        {
            // Arrange
            var item = CreateRandomItem();
            item.MaxDurability = 100;
            item.CurrentDurability = UnityEngine.Random.Range(1, 10);
            int degradeAmount = UnityEngine.Random.Range(50, 200); // More than current

            // Act
            _durabilitySystem.DegradeDurability(item, degradeAmount);

            // Assert
            Assert.That(item.CurrentDurability, Is.GreaterThanOrEqualTo(0),
                "Durability should never go below 0");
        }

        /// <summary>
        /// Property: Repair restores to MaxDurability
        /// </summary>
        [Test]
        [Repeat(100)]
        public void RepairItem_RestoresToMaxDurability()
        {
            // Arrange
            var item = CreateRandomItem();
            item.MaxDurability = UnityEngine.Random.Range(50, 200);
            item.CurrentDurability = UnityEngine.Random.Range(0, item.MaxDurability);

            // Act
            _durabilitySystem.RepairItem(item);

            // Assert
            Assert.That(item.CurrentDurability, Is.EqualTo(item.MaxDurability),
                "Repair should restore durability to max");
        }

        /// <summary>
        /// Property: Repair cost is positive for damaged items
        /// </summary>
        [Test]
        [Repeat(100)]
        public void GetRepairCost_DamagedItem_PositiveCost()
        {
            // Arrange
            var item = CreateRandomItem();
            item.MaxDurability = UnityEngine.Random.Range(50, 200);
            item.CurrentDurability = UnityEngine.Random.Range(0, item.MaxDurability - 1);
            item.ItemLevel = UnityEngine.Random.Range(1, 60);

            // Act
            int cost = _durabilitySystem.GetRepairCost(item);

            // Assert
            Assert.That(cost, Is.GreaterThan(0),
                "Damaged item should have positive repair cost");
        }

        /// <summary>
        /// Property: Full durability items have zero repair cost
        /// </summary>
        [Test]
        [Repeat(100)]
        public void GetRepairCost_FullDurability_ZeroCost()
        {
            // Arrange
            var item = CreateRandomItem();
            item.MaxDurability = UnityEngine.Random.Range(50, 200);
            item.CurrentDurability = item.MaxDurability;

            // Act
            int cost = _durabilitySystem.GetRepairCost(item);

            // Assert
            Assert.That(cost, Is.EqualTo(0),
                "Full durability item should have zero repair cost");
        }

        /// <summary>
        /// Property: NeedsRepair returns true for damaged items
        /// </summary>
        [Test]
        [Repeat(100)]
        public void NeedsRepair_DamagedItem_ReturnsTrue()
        {
            // Arrange
            var item = CreateRandomItem();
            item.MaxDurability = UnityEngine.Random.Range(50, 200);
            item.CurrentDurability = UnityEngine.Random.Range(0, item.MaxDurability - 1);

            // Act
            bool needsRepair = _durabilitySystem.NeedsRepair(item);

            // Assert
            Assert.That(needsRepair, Is.True,
                "Damaged item should need repair");
        }

        /// <summary>
        /// Property: NeedsRepair returns false for full durability items
        /// </summary>
        [Test]
        [Repeat(100)]
        public void NeedsRepair_FullDurability_ReturnsFalse()
        {
            // Arrange
            var item = CreateRandomItem();
            item.MaxDurability = UnityEngine.Random.Range(50, 200);
            item.CurrentDurability = item.MaxDurability;

            // Act
            bool needsRepair = _durabilitySystem.NeedsRepair(item);

            // Assert
            Assert.That(needsRepair, Is.False,
                "Full durability item should not need repair");
        }

        /// <summary>
        /// Property: OnItemBroken fires when durability reaches 0
        /// </summary>
        [Test]
        public void OnItemBroken_FiresWhenDurabilityReachesZero()
        {
            // Arrange
            var item = CreateRandomItem();
            item.MaxDurability = 100;
            item.CurrentDurability = 5;
            
            bool eventFired = false;
            ItemData brokenItem = null;
            _durabilitySystem.OnItemBroken += (i) =>
            {
                eventFired = true;
                brokenItem = i;
            };

            // Act
            _durabilitySystem.DegradeDurability(item, 10);

            // Assert
            Assert.That(eventFired, Is.True, "OnItemBroken should fire");
            Assert.That(brokenItem, Is.EqualTo(item), "Event should pass the broken item");
        }

        /// <summary>
        /// Property: OnItemRepaired fires when item is repaired
        /// </summary>
        [Test]
        public void OnItemRepaired_FiresWhenRepaired()
        {
            // Arrange
            var item = CreateRandomItem();
            item.MaxDurability = 100;
            item.CurrentDurability = 50;
            
            bool eventFired = false;
            _durabilitySystem.OnItemRepaired += (i) => eventFired = true;

            // Act
            _durabilitySystem.RepairItem(item);

            // Assert
            Assert.That(eventFired, Is.True, "OnItemRepaired should fire");
        }

        /// <summary>
        /// Property: Higher rarity items cost more to repair
        /// </summary>
        [Test]
        public void RepairCost_HigherRarity_HigherCost()
        {
            // Arrange
            var commonItem = CreateRandomItem();
            commonItem.MaxDurability = 100;
            commonItem.CurrentDurability = 0;
            commonItem.ItemLevel = 50;
            commonItem.Rarity = ItemRarity.Common;

            var epicItem = CreateRandomItem();
            epicItem.MaxDurability = 100;
            epicItem.CurrentDurability = 0;
            epicItem.ItemLevel = 50;
            epicItem.Rarity = ItemRarity.Epic;

            // Act
            int commonCost = _durabilitySystem.GetRepairCost(commonItem);
            int epicCost = _durabilitySystem.GetRepairCost(epicItem);

            // Assert
            Assert.That(epicCost, Is.GreaterThan(commonCost),
                "Epic items should cost more to repair than common items");
        }

        /// <summary>
        /// Property: IsBroken property works correctly
        /// </summary>
        [Test]
        [Repeat(100)]
        public void ItemData_IsBroken_CorrectValue()
        {
            // Arrange
            var item = CreateRandomItem();
            item.MaxDurability = UnityEngine.Random.Range(1, 200);
            item.CurrentDurability = UnityEngine.Random.Range(0, item.MaxDurability + 1);

            // Act & Assert
            bool expectedBroken = item.CurrentDurability <= 0;
            Assert.That(item.IsBroken, Is.EqualTo(expectedBroken),
                $"IsBroken should be {expectedBroken} for durability {item.CurrentDurability}");
        }

        private ItemData CreateRandomItem()
        {
            return new ItemData
            {
                ItemId = System.Guid.NewGuid().ToString(),
                ItemName = "Test Item",
                ItemLevel = UnityEngine.Random.Range(1, 60),
                Rarity = (ItemRarity)UnityEngine.Random.Range(0, 5),
                Slot = (EquipmentSlot)UnityEngine.Random.Range(0, 8),
                MaxDurability = 100,
                CurrentDurability = 100
            };
        }
    }
}
