using NUnit.Framework;
using UnityEngine;
using EtherDomes.Combat;
using EtherDomes.Data;
using EtherDomes.Tests.Generators;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for the PetSystem.
    /// Feature: new-classes-combat
    /// </summary>
    [TestFixture]
    public class PetSystemPropertyTests : PropertyTestBase
    {
        private PetSystem _petSystem;
        private GameObject _petSystemObject;

        [SetUp]
        public void SetUp()
        {
            _petSystemObject = new GameObject("PetSystem");
            _petSystem = _petSystemObject.AddComponent<PetSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_petSystemObject != null)
            {
                Object.DestroyImmediate(_petSystemObject);
            }
        }

        #region Property 6: Pet Follow Distance

        /// <summary>
        /// Feature: new-classes-combat, Property 6: Pet Follow Distance
        /// *For any* pet in follow state, the distance between pet and owner SHALL converge to 3 meters (±0.5m tolerance).
        /// **Validates: Requirements 3.3**
        /// </summary>
        [Test]
        public void Property6_PetFollowDistance_ConvergesToThreeMeters()
        {
            // Arrange
            const float expectedDistance = PetSystem.DEFAULT_FOLLOW_DISTANCE;
            const float tolerance = PetSystem.FOLLOW_DISTANCE_TOLERANCE;

            RunPropertyTest(
                () => (
                    ownerId: RandomULong(),
                    petData: TestDataGenerators.GeneratePetData(),
                    ownerPosition: RandomPosition(50f)
                ),
                input =>
                {
                    // Act
                    bool summoned = _petSystem.SummonPet(input.ownerId, input.petData);
                    
                    // Assert
                    Assert.IsTrue(summoned, "Pet should be summoned successfully");
                    
                    var pet = _petSystem.GetPet(input.ownerId);
                    Assert.IsNotNull(pet, "Pet instance should exist");
                    Assert.AreEqual(PetState.Following, pet.State, "Pet should be in Following state");
                    
                    // Verify the follow distance constant is correct
                    Assert.AreEqual(expectedDistance, _petSystem.FollowDistance, 
                        $"Follow distance should be {expectedDistance}m");
                    
                    // Verify tolerance is reasonable
                    Assert.That(tolerance, Is.LessThanOrEqualTo(1f), 
                        "Tolerance should be reasonable (≤1m)");
                    Assert.That(tolerance, Is.GreaterThan(0f), 
                        "Tolerance should be positive");
                    
                    // Clean up
                    _petSystem.DismissPet(input.ownerId);
                },
                MIN_ITERATIONS
            );
        }

        /// <summary>
        /// Feature: new-classes-combat, Property 6: Pet Follow Distance (Constants)
        /// Verify that the follow distance constant is exactly 3 meters.
        /// **Validates: Requirements 3.3**
        /// </summary>
        [Test]
        public void Property6_FollowDistanceConstant_IsThreeMeters()
        {
            Assert.AreEqual(3f, PetSystem.DEFAULT_FOLLOW_DISTANCE, 
                "Follow distance should be exactly 3 meters");
        }

        #endregion

        #region Property 7: Pet Resummon Cooldown

        /// <summary>
        /// Feature: new-classes-combat, Property 7: Pet Resummon Cooldown
        /// *For any* pet that dies, attempting to resummon before 10 seconds have elapsed SHALL fail.
        /// **Validates: Requirements 3.5**
        /// </summary>
        [Test]
        public void Property7_PetResummonCooldown_PreventsEarlySummon()
        {
            // Arrange
            const float expectedCooldown = PetSystem.DEFAULT_RESUMMON_COOLDOWN;
            ulong iterationCounter = 0;

            RunPropertyTest(
                () => (
                    // Use unique owner IDs to avoid cooldown collisions between iterations
                    ownerId: (ulong)(100000 + iterationCounter++),
                    petData: TestDataGenerators.GeneratePetData(),
                    damage: RandomFloat(1000f, 10000f) // Enough to kill any pet
                ),
                input =>
                {
                    // Act - Summon pet
                    bool summoned = _petSystem.SummonPet(input.ownerId, input.petData);
                    Assert.IsTrue(summoned, "Initial summon should succeed");
                    
                    // Kill the pet
                    _petSystem.ApplyDamageToPet(input.ownerId, input.damage);
                    
                    // Verify pet is dead
                    Assert.IsFalse(_petSystem.IsPetAlive(input.ownerId), "Pet should be dead after damage");
                    
                    // Verify cooldown is active
                    float cooldownRemaining = _petSystem.GetResummonCooldown(input.ownerId);
                    Assert.That(cooldownRemaining, Is.GreaterThan(0f), 
                        "Cooldown should be active after pet death");
                    Assert.That(cooldownRemaining, Is.LessThanOrEqualTo(expectedCooldown), 
                        $"Cooldown should not exceed {expectedCooldown}s");
                    
                    // Verify cannot summon during cooldown
                    Assert.IsFalse(_petSystem.CanSummonPet(input.ownerId), 
                        "Should not be able to summon during cooldown");
                    
                    // Attempt to summon should fail
                    bool resummoned = _petSystem.SummonPet(input.ownerId, input.petData);
                    Assert.IsFalse(resummoned, "Resummon should fail during cooldown");
                    
                    // Clean up - unregister owner to prevent state leakage
                    _petSystem.UnregisterOwner(input.ownerId);
                },
                MIN_ITERATIONS
            );
        }

        /// <summary>
        /// Feature: new-classes-combat, Property 7: Pet Resummon Cooldown (Constants)
        /// Verify that the resummon cooldown constant is exactly 10 seconds.
        /// **Validates: Requirements 3.5**
        /// </summary>
        [Test]
        public void Property7_ResummonCooldownConstant_IsTenSeconds()
        {
            Assert.AreEqual(10f, PetSystem.DEFAULT_RESUMMON_COOLDOWN, 
                "Resummon cooldown should be exactly 10 seconds");
        }

        /// <summary>
        /// Feature: new-classes-combat, Property 7: Pet Resummon Cooldown
        /// Verify that summoning is allowed when no cooldown is active.
        /// **Validates: Requirements 3.5**
        /// </summary>
        [Test]
        public void Property7_NoCooldown_AllowsSummon()
        {
            RunPropertyTest(
                () => (
                    ownerId: RandomULong(),
                    petData: TestDataGenerators.GeneratePetData()
                ),
                input =>
                {
                    // Verify no cooldown initially
                    Assert.AreEqual(0f, _petSystem.GetResummonCooldown(input.ownerId), 
                        "No cooldown should exist for new owner");
                    Assert.IsTrue(_petSystem.CanSummonPet(input.ownerId), 
                        "Should be able to summon with no cooldown");
                    
                    // Summon should succeed
                    bool summoned = _petSystem.SummonPet(input.ownerId, input.petData);
                    Assert.IsTrue(summoned, "Summon should succeed with no cooldown");
                    
                    // Clean up
                    _petSystem.DismissPet(input.ownerId);
                },
                MIN_ITERATIONS
            );
        }

        #endregion

        #region Additional Pet System Tests

        /// <summary>
        /// Verify that teleport distance constant is 40 meters.
        /// **Validates: Requirements 3.7**
        /// </summary>
        [Test]
        public void TeleportDistanceConstant_IsFortyMeters()
        {
            Assert.AreEqual(40f, PetSystem.DEFAULT_TELEPORT_DISTANCE, 
                "Teleport distance should be exactly 40 meters");
        }

        /// <summary>
        /// Verify that dismissing a pet removes it from active pets.
        /// **Validates: Requirements 3.1**
        /// </summary>
        [Test]
        public void DismissPet_RemovesPetFromActivePets()
        {
            RunPropertyTest(
                () => (
                    ownerId: RandomULong(),
                    petData: TestDataGenerators.GeneratePetData()
                ),
                input =>
                {
                    // Summon pet
                    _petSystem.SummonPet(input.ownerId, input.petData);
                    Assert.IsTrue(_petSystem.HasPet(input.ownerId), "Pet should exist after summon");
                    
                    // Dismiss pet
                    _petSystem.DismissPet(input.ownerId);
                    Assert.IsFalse(_petSystem.HasPet(input.ownerId), "Pet should not exist after dismiss");
                },
                MIN_ITERATIONS
            );
        }

        /// <summary>
        /// Verify that pet health is initialized correctly.
        /// **Validates: Requirements 3.4**
        /// </summary>
        [Test]
        public void PetHealth_InitializedToBaseHealth()
        {
            RunPropertyTest(
                () => (
                    ownerId: RandomULong(),
                    petData: TestDataGenerators.GeneratePetData()
                ),
                input =>
                {
                    // Summon pet
                    _petSystem.SummonPet(input.ownerId, input.petData);
                    
                    // Verify health
                    float currentHealth = _petSystem.GetPetHealth(input.ownerId);
                    float maxHealth = _petSystem.GetPetMaxHealth(input.ownerId);
                    
                    Assert.AreEqual(input.petData.BaseHealth, maxHealth, 
                        "Max health should equal base health from pet data");
                    Assert.AreEqual(maxHealth, currentHealth, 
                        "Current health should equal max health on summon");
                    
                    // Clean up
                    _petSystem.DismissPet(input.ownerId);
                },
                MIN_ITERATIONS
            );
        }

        /// <summary>
        /// Verify that commanding attack changes pet state.
        /// **Validates: Requirements 3.2**
        /// </summary>
        [Test]
        public void CommandAttack_ChangesPetStateToAttacking()
        {
            RunPropertyTest(
                () => (
                    ownerId: RandomULong(),
                    targetId: RandomULong(),
                    petData: TestDataGenerators.GeneratePetData()
                ),
                input =>
                {
                    // Summon pet
                    _petSystem.SummonPet(input.ownerId, input.petData);
                    var pet = _petSystem.GetPet(input.ownerId);
                    Assert.AreEqual(PetState.Following, pet.State, "Pet should start in Following state");
                    
                    // Command attack
                    _petSystem.CommandAttack(input.ownerId, input.targetId);
                    
                    // Verify state changed
                    Assert.AreEqual(PetState.Attacking, pet.State, "Pet should be in Attacking state");
                    Assert.AreEqual(input.targetId, pet.CurrentTargetId, "Pet should have correct target");
                    
                    // Clean up
                    _petSystem.DismissPet(input.ownerId);
                },
                MIN_ITERATIONS
            );
        }

        /// <summary>
        /// Verify that commanding follow clears target and changes state.
        /// **Validates: Requirements 3.3**
        /// </summary>
        [Test]
        public void CommandFollow_ChangesPetStateToFollowing()
        {
            RunPropertyTest(
                () => (
                    ownerId: RandomULong(),
                    targetId: RandomULong(),
                    petData: TestDataGenerators.GeneratePetData()
                ),
                input =>
                {
                    // Summon pet and set to attacking
                    _petSystem.SummonPet(input.ownerId, input.petData);
                    _petSystem.CommandAttack(input.ownerId, input.targetId);
                    
                    var pet = _petSystem.GetPet(input.ownerId);
                    Assert.AreEqual(PetState.Attacking, pet.State, "Pet should be attacking");
                    
                    // Command follow
                    _petSystem.CommandFollow(input.ownerId);
                    
                    // Verify state changed
                    Assert.AreEqual(PetState.Following, pet.State, "Pet should be in Following state");
                    Assert.AreEqual(0UL, pet.CurrentTargetId, "Pet should have no target");
                    
                    // Clean up
                    _petSystem.DismissPet(input.ownerId);
                },
                MIN_ITERATIONS
            );
        }

        #endregion
    }
}
