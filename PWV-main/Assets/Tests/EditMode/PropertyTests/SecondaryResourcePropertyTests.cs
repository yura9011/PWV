using NUnit.Framework;
using EtherDomes.Combat;
using EtherDomes.Data;
using UnityEngine;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for the Secondary Resource system.
    /// 
    /// Phase 2 - 8 Classes Resource Assignment:
    /// - Mana: Cruzado, MaestroElemental, Clerigo, MedicoBrujo
    /// - Colera: Protector, Berserker
    /// - Energia: Arquero
    /// - EnergiaRunica + Runas: CaballeroRunico
    /// </summary>
    [TestFixture]
    public class SecondaryResourcePropertyTests
    {
        #region Class Resource Assignment Tests

        [Test]
        public void ClassResourceAssignment_CruzadoHasMana()
        {
            var resourceType = SecondaryResourceSystem.GetResourceTypeForClass(CharacterClass.Cruzado);
            Assert.That(resourceType, Is.EqualTo(SecondaryResourceType.Mana),
                "Cruzado should have Mana as secondary resource");
        }

        [Test]
        public void ClassResourceAssignment_ProtectorHasColera()
        {
            var resourceType = SecondaryResourceSystem.GetResourceTypeForClass(CharacterClass.Protector);
            Assert.That(resourceType, Is.EqualTo(SecondaryResourceType.Colera),
                "Protector should have Colera as secondary resource");
        }

        [Test]
        public void ClassResourceAssignment_BerserkerHasColera()
        {
            var resourceType = SecondaryResourceSystem.GetResourceTypeForClass(CharacterClass.Berserker);
            Assert.That(resourceType, Is.EqualTo(SecondaryResourceType.Colera),
                "Berserker should have Colera as secondary resource");
        }

        [Test]
        public void ClassResourceAssignment_ArqueroHasEnergia()
        {
            var resourceType = SecondaryResourceSystem.GetResourceTypeForClass(CharacterClass.Arquero);
            Assert.That(resourceType, Is.EqualTo(SecondaryResourceType.Energia),
                "Arquero should have Energia as secondary resource");
        }

        [Test]
        public void ClassResourceAssignment_MaestroElementalHasMana()
        {
            var resourceType = SecondaryResourceSystem.GetResourceTypeForClass(CharacterClass.MaestroElemental);
            Assert.That(resourceType, Is.EqualTo(SecondaryResourceType.Mana),
                "MaestroElemental should have Mana as secondary resource");
        }

        [Test]
        public void ClassResourceAssignment_CaballeroRunicoHasEnergiaRunica()
        {
            var resourceType = SecondaryResourceSystem.GetResourceTypeForClass(CharacterClass.CaballeroRunico);
            Assert.That(resourceType, Is.EqualTo(SecondaryResourceType.EnergiaRunica),
                "CaballeroRunico should have EnergiaRunica as secondary resource");
        }

        [Test]
        public void ClassResourceAssignment_ClerigoHasMana()
        {
            var resourceType = SecondaryResourceSystem.GetResourceTypeForClass(CharacterClass.Clerigo);
            Assert.That(resourceType, Is.EqualTo(SecondaryResourceType.Mana),
                "Clerigo should have Mana as secondary resource");
        }

        [Test]
        public void ClassResourceAssignment_MedicoBrujoHasMana()
        {
            var resourceType = SecondaryResourceSystem.GetResourceTypeForClass(CharacterClass.MedicoBrujo);
            Assert.That(resourceType, Is.EqualTo(SecondaryResourceType.Mana),
                "MedicoBrujo should have Mana as secondary resource");
        }

        [Test]
        public void ClassResourceAssignment_CaballeroRunicoUsesRunes()
        {
            Assert.That(SecondaryResourceSystem.ClassUsesRunes(CharacterClass.CaballeroRunico), Is.True,
                "CaballeroRunico should use Runes");
            Assert.That(SecondaryResourceSystem.ClassUsesRunes(CharacterClass.Cruzado), Is.False,
                "Cruzado should not use Runes");
        }

        #endregion

        #region Colera (Rage) Tests

        [Test]
        public void ColeraDecay_OutOfCombat_Decays()
        {
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterResource(playerId, SecondaryResourceType.Colera, 100f);
            resourceSystem.AddResource(playerId, 50f);

            float initialColera = resourceSystem.GetResource(playerId);

            resourceSystem.ApplyDecay(playerId, 1f, inCombat: false);

            float currentColera = resourceSystem.GetResource(playerId);
            Assert.That(currentColera, Is.LessThan(initialColera),
                "Colera should decay out of combat");
            Assert.That(currentColera, Is.EqualTo(initialColera - SecondaryResourceSystem.COLERA_DECAY_RATE).Within(0.01f),
                "Colera should decay at correct rate");

            Object.DestroyImmediate(resourceGO);
        }

        [Test]
        public void ColeraDecay_InCombat_DoesNotDecay()
        {
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterResource(playerId, SecondaryResourceType.Colera, 100f);
            resourceSystem.AddResource(playerId, 50f);

            float initialColera = resourceSystem.GetResource(playerId);

            resourceSystem.ApplyDecay(playerId, 1f, inCombat: true);

            float currentColera = resourceSystem.GetResource(playerId);
            Assert.That(currentColera, Is.EqualTo(initialColera),
                "Colera should not decay in combat");

            Object.DestroyImmediate(resourceGO);
        }

        [Test]
        public void ColeraGeneration_FromDamageDealt()
        {
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterResource(playerId, SecondaryResourceType.Colera, 100f);

            resourceSystem.GenerateColeraFromDamageDealt(playerId, 100f);

            Assert.That(resourceSystem.GetResource(playerId), 
                Is.EqualTo(SecondaryResourceSystem.COLERA_PER_DAMAGE_DEALT),
                "Should add correct colera for damage dealt");

            Object.DestroyImmediate(resourceGO);
        }

        [Test]
        public void ColeraGeneration_FromDamageTaken()
        {
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterResource(playerId, SecondaryResourceType.Colera, 100f);

            resourceSystem.GenerateColeraFromDamageTaken(playerId, 50f);

            Assert.That(resourceSystem.GetResource(playerId), 
                Is.EqualTo(SecondaryResourceSystem.COLERA_PER_DAMAGE_TAKEN),
                "Should add correct colera for damage taken");

            Object.DestroyImmediate(resourceGO);
        }

        #endregion

        #region Mana Tests

        [Test]
        public void ManaRegeneration_RegeneratesOverTime()
        {
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterResource(playerId, SecondaryResourceType.Mana, SecondaryResourceSystem.MANA_MAX);
            
            // Spend some mana first (starts full)
            resourceSystem.TrySpendResource(playerId, 50f);
            float initialMana = resourceSystem.GetResource(playerId);

            resourceSystem.ApplyDecay(playerId, 1f, inCombat: false);

            float currentMana = resourceSystem.GetResource(playerId);
            Assert.That(currentMana, Is.GreaterThan(initialMana),
                "Mana should regenerate over time");

            Object.DestroyImmediate(resourceGO);
        }

        [Test]
        public void ManaRegeneration_SlowerInCombat()
        {
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId1 = 1;
            ulong playerId2 = 2;
            
            resourceSystem.RegisterResource(playerId1, SecondaryResourceType.Mana, SecondaryResourceSystem.MANA_MAX);
            resourceSystem.RegisterResource(playerId2, SecondaryResourceType.Mana, SecondaryResourceSystem.MANA_MAX);
            
            resourceSystem.TrySpendResource(playerId1, 50f);
            resourceSystem.TrySpendResource(playerId2, 50f);

            resourceSystem.ApplyDecay(playerId1, 1f, inCombat: false);
            resourceSystem.ApplyDecay(playerId2, 1f, inCombat: true);

            float outOfCombatMana = resourceSystem.GetResource(playerId1);
            float inCombatMana = resourceSystem.GetResource(playerId2);
            
            Assert.That(outOfCombatMana, Is.GreaterThan(inCombatMana),
                "Mana should regenerate faster out of combat");

            Object.DestroyImmediate(resourceGO);
        }

        [Test]
        public void Mana_StartsAtFull()
        {
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterResource(playerId, SecondaryResourceType.Mana, SecondaryResourceSystem.MANA_MAX);

            Assert.That(resourceSystem.GetResource(playerId), Is.EqualTo(SecondaryResourceSystem.MANA_MAX),
                "Mana should start at full");

            Object.DestroyImmediate(resourceGO);
        }

        #endregion

        #region Energia Tests (Arquero)

        [Test]
        public void EnergiaRegeneration_RegeneratesAt10PerSecond()
        {
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterResource(playerId, SecondaryResourceType.Energia, SecondaryResourceSystem.ENERGIA_MAX);
            
            resourceSystem.TrySpendResource(playerId, 50f);
            float initialEnergia = resourceSystem.GetResource(playerId);

            resourceSystem.ApplyDecay(playerId, 1f, inCombat: false);

            float currentEnergia = resourceSystem.GetResource(playerId);
            float expectedEnergia = initialEnergia + SecondaryResourceSystem.ENERGIA_REGEN_RATE;
            
            Assert.That(currentEnergia, Is.EqualTo(expectedEnergia).Within(0.01f),
                $"Energia should regenerate at {SecondaryResourceSystem.ENERGIA_REGEN_RATE}/s");

            Object.DestroyImmediate(resourceGO);
        }

        [Test]
        public void Energia_StartsAtFull()
        {
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterResource(playerId, SecondaryResourceType.Energia, SecondaryResourceSystem.ENERGIA_MAX);

            Assert.That(resourceSystem.GetResource(playerId), Is.EqualTo(SecondaryResourceSystem.ENERGIA_MAX),
                "Energia should start at full");

            Object.DestroyImmediate(resourceGO);
        }

        [Test]
        public void Energia_CapsAt100()
        {
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterResource(playerId, SecondaryResourceType.Energia, SecondaryResourceSystem.ENERGIA_MAX);

            // Try to add more
            resourceSystem.AddResource(playerId, 50f);

            Assert.That(resourceSystem.GetResource(playerId), Is.EqualTo(SecondaryResourceSystem.ENERGIA_MAX),
                "Energia should cap at 100");

            Object.DestroyImmediate(resourceGO);
        }

        #endregion

        #region Energia Runica + Runas Tests (CaballeroRunico)

        [Test]
        public void EnergiaRunicaDecay_OutOfCombat_Decays()
        {
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterResource(playerId, SecondaryResourceType.EnergiaRunica, 100f);
            resourceSystem.AddResource(playerId, 50f);

            float initial = resourceSystem.GetResource(playerId);

            resourceSystem.ApplyDecay(playerId, 1f, inCombat: false);

            float current = resourceSystem.GetResource(playerId);
            Assert.That(current, Is.LessThan(initial),
                "EnergiaRunica should decay out of combat");

            Object.DestroyImmediate(resourceGO);
        }

        [Test]
        public void Runas_StartAt6()
        {
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterRunes(playerId);

            Assert.That(resourceSystem.GetAvailableRunes(playerId), Is.EqualTo(SecondaryResourceSystem.RUNAS_MAX),
                "Should start with 6 runes");

            Object.DestroyImmediate(resourceGO);
        }

        [Test]
        public void Runas_CanBeSpent()
        {
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterRunes(playerId);

            bool success = resourceSystem.TrySpendRune(playerId, 2);

            Assert.That(success, Is.True, "Should be able to spend runes");
            Assert.That(resourceSystem.GetAvailableRunes(playerId), Is.EqualTo(4),
                "Should have 4 runes after spending 2");

            Object.DestroyImmediate(resourceGO);
        }

        [Test]
        public void Runas_CannotSpendMoreThanAvailable()
        {
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterRunes(playerId);
            resourceSystem.TrySpendRune(playerId, 5); // Spend 5, have 1 left

            bool success = resourceSystem.TrySpendRune(playerId, 2);

            Assert.That(success, Is.False, "Should not be able to spend more runes than available");
            Assert.That(resourceSystem.GetAvailableRunes(playerId), Is.EqualTo(1),
                "Runes should remain unchanged after failed spend");

            Object.DestroyImmediate(resourceGO);
        }

        #endregion

        #region General Resource Tests

        [Test]
        [Repeat(100)]
        public void AddResource_CannotExceedMax()
        {
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            float maxValue = Random.Range(50f, 200f);
            resourceSystem.RegisterResource(playerId, SecondaryResourceType.Colera, maxValue);

            float addAmount = maxValue * 2;
            resourceSystem.AddResource(playerId, addAmount);

            float current = resourceSystem.GetResource(playerId);
            Assert.That(current, Is.EqualTo(maxValue),
                "Resource should be capped at max value");

            Object.DestroyImmediate(resourceGO);
        }

        [Test]
        [Repeat(100)]
        public void SpendResource_CannotGoBelowZero()
        {
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterResource(playerId, SecondaryResourceType.Colera, 100f);
            
            float initialAmount = Random.Range(10f, 50f);
            resourceSystem.AddResource(playerId, initialAmount);

            float spendAmount = initialAmount + 10f;
            bool success = resourceSystem.TrySpendResource(playerId, spendAmount);

            Assert.That(success, Is.False, "Should fail to spend more than available");
            Assert.That(resourceSystem.GetResource(playerId), Is.EqualTo(initialAmount),
                "Resource should remain unchanged after failed spend");

            Object.DestroyImmediate(resourceGO);
        }

        [Test]
        [Repeat(100)]
        public void TrySpendResource_SufficientResource_ReturnsTrue()
        {
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterResource(playerId, SecondaryResourceType.Colera, 100f);
            
            float initialAmount = Random.Range(50f, 100f);
            resourceSystem.AddResource(playerId, initialAmount);

            float spendAmount = Random.Range(1f, initialAmount);

            bool success = resourceSystem.TrySpendResource(playerId, spendAmount);

            Assert.That(success, Is.True, "Should succeed when sufficient resource");
            Assert.That(resourceSystem.GetResource(playerId), 
                Is.EqualTo(initialAmount - spendAmount).Within(0.01f),
                "Resource should be reduced by spend amount");

            Object.DestroyImmediate(resourceGO);
        }

        [Test]
        public void DefaultMaxValues_AreCorrect()
        {
            Assert.That(SecondaryResourceSystem.GetDefaultMax(SecondaryResourceType.Mana), 
                Is.EqualTo(100f), "Mana max should be 100");
            Assert.That(SecondaryResourceSystem.GetDefaultMax(SecondaryResourceType.Colera), 
                Is.EqualTo(100f), "Colera max should be 100");
            Assert.That(SecondaryResourceSystem.GetDefaultMax(SecondaryResourceType.Energia), 
                Is.EqualTo(100f), "Energia max should be 100");
            Assert.That(SecondaryResourceSystem.GetDefaultMax(SecondaryResourceType.EnergiaRunica), 
                Is.EqualTo(100f), "EnergiaRunica max should be 100");
            Assert.That(SecondaryResourceSystem.GetDefaultMax(SecondaryResourceType.None), 
                Is.EqualTo(0f), "None should have 0 max");
        }

        [Test]
        public void DoesResourceDecay_ReturnsCorrectValues()
        {
            Assert.That(SecondaryResourceSystem.DoesResourceDecay(SecondaryResourceType.Colera), 
                Is.True, "Colera should decay");
            Assert.That(SecondaryResourceSystem.DoesResourceDecay(SecondaryResourceType.EnergiaRunica), 
                Is.True, "EnergiaRunica should decay");
            Assert.That(SecondaryResourceSystem.DoesResourceDecay(SecondaryResourceType.Mana), 
                Is.False, "Mana should not decay");
            Assert.That(SecondaryResourceSystem.DoesResourceDecay(SecondaryResourceType.Energia), 
                Is.False, "Energia should not decay");
        }

        [Test]
        public void DoesResourceRegenerate_ReturnsCorrectValues()
        {
            Assert.That(SecondaryResourceSystem.DoesResourceRegenerate(SecondaryResourceType.Mana), 
                Is.True, "Mana should regenerate");
            Assert.That(SecondaryResourceSystem.DoesResourceRegenerate(SecondaryResourceType.Energia), 
                Is.True, "Energia should regenerate");
            Assert.That(SecondaryResourceSystem.DoesResourceRegenerate(SecondaryResourceType.Colera), 
                Is.False, "Colera should not regenerate");
            Assert.That(SecondaryResourceSystem.DoesResourceRegenerate(SecondaryResourceType.EnergiaRunica), 
                Is.False, "EnergiaRunica should not regenerate");
        }

        [Test]
        public void OnResourceChanged_FiresOnChange()
        {
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            bool eventFired = false;
            float receivedCurrent = 0;
            float receivedMax = 0;

            resourceSystem.OnResourceChanged += (id, current, max) =>
            {
                eventFired = true;
                receivedCurrent = current;
                receivedMax = max;
            };

            resourceSystem.RegisterResource(playerId, SecondaryResourceType.Colera, 100f);
            resourceSystem.AddResource(playerId, 25f);

            Assert.That(eventFired, Is.True, "OnResourceChanged should fire");
            Assert.That(receivedCurrent, Is.EqualTo(25f), "Current should be 25");
            Assert.That(receivedMax, Is.EqualTo(100f), "Max should be 100");

            Object.DestroyImmediate(resourceGO);
        }

        #endregion

        #region Legacy Combo Points Tests (may be removed)

        [Test]
        public void ComboPoints_GenerationAndConsumption()
        {
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterComboPoints(playerId);

            resourceSystem.AddComboPoint(playerId);
            resourceSystem.AddComboPoint(playerId);
            resourceSystem.AddComboPoint(playerId);

            Assert.That(resourceSystem.GetComboPoints(playerId), Is.EqualTo(3));

            int consumed = resourceSystem.ConsumeAllComboPoints(playerId);
            Assert.That(consumed, Is.EqualTo(3));
            Assert.That(resourceSystem.GetComboPoints(playerId), Is.EqualTo(0));

            Object.DestroyImmediate(resourceGO);
        }

        [Test]
        public void ComboPoints_CapsAt5()
        {
            var resourceGO = new GameObject("SecondaryResource");
            var resourceSystem = resourceGO.AddComponent<SecondaryResourceSystem>();

            ulong playerId = 1;
            resourceSystem.RegisterComboPoints(playerId);

            for (int i = 0; i < 10; i++)
                resourceSystem.AddComboPoint(playerId);

            Assert.That(resourceSystem.GetComboPoints(playerId), Is.EqualTo(5),
                "Combo points should cap at 5");

            Object.DestroyImmediate(resourceGO);
        }

        #endregion
    }
}
