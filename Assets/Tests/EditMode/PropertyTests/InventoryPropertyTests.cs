using NUnit.Framework;
using EtherDomes.UI;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for the Inventory UI.
    /// </summary>
    [TestFixture]
    public class InventoryPropertyTests
    {
        /// <summary>
        /// Feature: mvp-10-features, Property 40: Inventory Slot Count
        /// The inventory SHALL have exactly 30 slots.
        /// Validates: Requirements 16.1
        /// </summary>
        [Test]
        public void InventorySlotCount_IsExactly30()
        {
            // Assert
            Assert.That(InventoryUI.INVENTORY_SLOT_COUNT, Is.EqualTo(30),
                "Inventory should have exactly 30 slots");
        }
        
        /// <summary>
        /// Property: Slot count constant matches interface expectation.
        /// </summary>
        [Test]
        public void InventorySlotCount_MatchesExpectedValue()
        {
            // The requirement specifies 30 slots
            const int expectedSlots = 30;
            
            Assert.That(InventoryUI.INVENTORY_SLOT_COUNT, Is.EqualTo(expectedSlots),
                $"Inventory slot count should be {expectedSlots}");
        }
        
        /// <summary>
        /// Property: Slot count is positive.
        /// </summary>
        [Test]
        public void InventorySlotCount_IsPositive()
        {
            Assert.That(InventoryUI.INVENTORY_SLOT_COUNT, Is.GreaterThan(0),
                "Inventory slot count should be positive");
        }
        
        /// <summary>
        /// Property: Slot count is reasonable (not too large).
        /// </summary>
        [Test]
        public void InventorySlotCount_IsReasonable()
        {
            // Inventory shouldn't be unreasonably large
            Assert.That(InventoryUI.INVENTORY_SLOT_COUNT, Is.LessThanOrEqualTo(100),
                "Inventory slot count should be reasonable (<=100)");
        }
    }
}
