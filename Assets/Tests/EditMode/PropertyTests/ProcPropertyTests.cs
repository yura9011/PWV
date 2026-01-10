using NUnit.Framework;
using EtherDomes.Combat;
using UnityEngine;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for the Proc system.
    /// </summary>
    [TestFixture]
    public class ProcPropertyTests
    {
        /// <summary>
        /// Feature: mvp-10-features, Property 25: Proc Probability
        /// For any proc with probability P, the trigger rate SHALL approximate P over many trials.
        /// Validates: Requirements 9.1
        /// </summary>
        [Test]
        [Repeat(10)]
        public void ProcProbability_ApproximatesConfiguredRate()
        {
            // Arrange
            var procGO = new GameObject("ProcSystem");
            var procSystem = procGO.AddComponent<ProcSystem>();
            
            float targetProbability = Random.Range(0.1f, 0.9f);
            int trials = 1000;
            int triggers = 0;

            var proc = new ProcDefinition
            {
                ProcId = "test_proc",
                ProcName = "Test Proc",
                Probability = targetProbability,
                InternalCooldown = 0, // No ICD for this test
                TriggerType = ProcTrigger.OnDamageDealt,
                Effect = ProcEffect.InstantDamage,
                EffectValue = 100,
                OwnerId = 1
            };

            // Count triggers manually using TryTriggerProc
            for (int i = 0; i < trials; i++)
            {
                if (procSystem.TryTriggerProc(proc))
                {
                    triggers++;
                }
            }

            // Assert
            float actualRate = (float)triggers / trials;
            float tolerance = 0.1f; // 10% tolerance for randomness
            
            Assert.That(actualRate, Is.InRange(targetProbability - tolerance, targetProbability + tolerance),
                $"Proc rate {actualRate:P1} should approximate {targetProbability:P1} (±{tolerance:P0})");

            // Cleanup
            Object.DestroyImmediate(procGO);
        }

        /// <summary>
        /// Property: Proc with 0% probability never triggers
        /// </summary>
        [Test]
        [Repeat(100)]
        public void ProcProbability_ZeroPercent_NeverTriggers()
        {
            // Arrange
            var procGO = new GameObject("ProcSystem");
            var procSystem = procGO.AddComponent<ProcSystem>();

            var proc = new ProcDefinition
            {
                ProcId = "zero_proc",
                ProcName = "Zero Proc",
                Probability = 0f,
                TriggerType = ProcTrigger.OnDamageDealt,
                Effect = ProcEffect.InstantDamage,
                OwnerId = 1
            };

            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                Assert.That(procSystem.TryTriggerProc(proc), Is.False,
                    "0% probability proc should never trigger");
            }

            // Cleanup
            Object.DestroyImmediate(procGO);
        }

        /// <summary>
        /// Property: Proc with 100% probability always triggers
        /// </summary>
        [Test]
        [Repeat(100)]
        public void ProcProbability_HundredPercent_AlwaysTriggers()
        {
            // Arrange
            var procGO = new GameObject("ProcSystem");
            var procSystem = procGO.AddComponent<ProcSystem>();

            var proc = new ProcDefinition
            {
                ProcId = "guaranteed_proc",
                ProcName = "Guaranteed Proc",
                Probability = 1f,
                TriggerType = ProcTrigger.OnDamageDealt,
                Effect = ProcEffect.InstantDamage,
                OwnerId = 1
            };

            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                Assert.That(procSystem.TryTriggerProc(proc), Is.True,
                    "100% probability proc should always trigger");
            }

            // Cleanup
            Object.DestroyImmediate(procGO);
        }

        /// <summary>
        /// Property: Registered proc can be retrieved
        /// </summary>
        [Test]
        public void RegisterProc_CanBeRetrieved()
        {
            // Arrange
            var procGO = new GameObject("ProcSystem");
            var procSystem = procGO.AddComponent<ProcSystem>();

            ulong playerId = (ulong)Random.Range(1, 1000);
            var proc = new ProcDefinition
            {
                ProcId = "test_proc",
                ProcName = "Test Proc",
                Probability = 0.5f,
                TriggerType = ProcTrigger.OnCriticalHit,
                Effect = ProcEffect.InstantDamage,
                OwnerId = playerId
            };

            // Act
            procSystem.RegisterProc(proc);
            var procs = procSystem.GetPlayerProcs(playerId);

            // Assert
            Assert.That(procs.Length, Is.EqualTo(1), "Should have 1 registered proc");
            Assert.That(procs[0].ProcId, Is.EqualTo("test_proc"), "Proc ID should match");
            Assert.That(procs[0].Probability, Is.EqualTo(0.5f), "Probability should match");

            // Cleanup
            Object.DestroyImmediate(procGO);
        }

        /// <summary>
        /// Property: Unregistered proc is removed
        /// </summary>
        [Test]
        public void UnregisterProc_RemovesProc()
        {
            // Arrange
            var procGO = new GameObject("ProcSystem");
            var procSystem = procGO.AddComponent<ProcSystem>();

            ulong playerId = 1;
            var proc = new ProcDefinition
            {
                ProcId = "removable_proc",
                ProcName = "Removable Proc",
                Probability = 0.5f,
                TriggerType = ProcTrigger.OnDamageDealt,
                Effect = ProcEffect.InstantDamage,
                OwnerId = playerId
            };

            procSystem.RegisterProc(proc);
            Assert.That(procSystem.GetProcCount(playerId), Is.EqualTo(1));

            // Act
            procSystem.UnregisterProc("removable_proc");

            // Assert
            Assert.That(procSystem.GetProcCount(playerId), Is.EqualTo(0),
                "Proc should be removed after unregister");

            // Cleanup
            Object.DestroyImmediate(procGO);
        }

        /// <summary>
        /// Property: CheckProcs fires event when proc triggers
        /// </summary>
        [Test]
        public void CheckProcs_FiresEventOnTrigger()
        {
            // Arrange
            var procGO = new GameObject("ProcSystem");
            var procSystem = procGO.AddComponent<ProcSystem>();

            ulong playerId = 1;
            bool eventFired = false;
            ProcDefinition triggeredProc = null;

            procSystem.OnProcTriggered += (id, proc) =>
            {
                eventFired = true;
                triggeredProc = proc;
            };

            var proc = new ProcDefinition
            {
                ProcId = "guaranteed_proc",
                ProcName = "Guaranteed Proc",
                Probability = 1f, // 100% to guarantee trigger
                InternalCooldown = 0,
                TriggerType = ProcTrigger.OnDamageDealt,
                Effect = ProcEffect.InstantDamage,
                OwnerId = playerId
            };

            procSystem.RegisterProc(proc);

            // Act
            procSystem.CheckProcs(playerId, ProcTrigger.OnDamageDealt);

            // Assert
            Assert.That(eventFired, Is.True, "OnProcTriggered should fire");
            Assert.That(triggeredProc, Is.Not.Null, "Triggered proc should not be null");
            Assert.That(triggeredProc.ProcId, Is.EqualTo("guaranteed_proc"));

            // Cleanup
            Object.DestroyImmediate(procGO);
        }

        /// <summary>
        /// Property: Wrong trigger type does not fire proc
        /// </summary>
        [Test]
        public void CheckProcs_WrongTrigger_DoesNotFire()
        {
            // Arrange
            var procGO = new GameObject("ProcSystem");
            var procSystem = procGO.AddComponent<ProcSystem>();

            ulong playerId = 1;
            bool eventFired = false;

            procSystem.OnProcTriggered += (id, proc) => eventFired = true;

            var proc = new ProcDefinition
            {
                ProcId = "damage_proc",
                ProcName = "Damage Proc",
                Probability = 1f,
                TriggerType = ProcTrigger.OnDamageDealt, // Only triggers on damage
                Effect = ProcEffect.InstantDamage,
                OwnerId = playerId
            };

            procSystem.RegisterProc(proc);

            // Act - use wrong trigger type
            procSystem.CheckProcs(playerId, ProcTrigger.OnHealingDone);

            // Assert
            Assert.That(eventFired, Is.False,
                "Proc should not trigger on wrong trigger type");

            // Cleanup
            Object.DestroyImmediate(procGO);
        }

        /// <summary>
        /// Property: Duplicate proc ID replaces existing
        /// </summary>
        [Test]
        public void RegisterProc_DuplicateId_ReplacesExisting()
        {
            // Arrange
            var procGO = new GameObject("ProcSystem");
            var procSystem = procGO.AddComponent<ProcSystem>();

            ulong playerId = 1;
            var proc1 = new ProcDefinition
            {
                ProcId = "same_id",
                ProcName = "First Proc",
                Probability = 0.3f,
                TriggerType = ProcTrigger.OnDamageDealt,
                Effect = ProcEffect.InstantDamage,
                OwnerId = playerId
            };

            var proc2 = new ProcDefinition
            {
                ProcId = "same_id",
                ProcName = "Second Proc",
                Probability = 0.7f,
                TriggerType = ProcTrigger.OnDamageDealt,
                Effect = ProcEffect.InstantHealing,
                OwnerId = playerId
            };

            // Act
            procSystem.RegisterProc(proc1);
            procSystem.RegisterProc(proc2);

            // Assert
            var procs = procSystem.GetPlayerProcs(playerId);
            Assert.That(procs.Length, Is.EqualTo(1), "Should only have 1 proc");
            Assert.That(procs[0].ProcName, Is.EqualTo("Second Proc"), "Should be the second proc");
            Assert.That(procs[0].Probability, Is.EqualTo(0.7f), "Should have second proc's probability");

            // Cleanup
            Object.DestroyImmediate(procGO);
        }

        /// <summary>
        /// Property: ClearAll removes all procs
        /// </summary>
        [Test]
        public void ClearAll_RemovesAllProcs()
        {
            // Arrange
            var procGO = new GameObject("ProcSystem");
            var procSystem = procGO.AddComponent<ProcSystem>();

            for (ulong i = 1; i <= 5; i++)
            {
                procSystem.RegisterProc(new ProcDefinition
                {
                    ProcId = $"proc_{i}",
                    ProcName = $"Proc {i}",
                    Probability = 0.5f,
                    TriggerType = ProcTrigger.OnDamageDealt,
                    Effect = ProcEffect.InstantDamage,
                    OwnerId = i
                });
            }

            // Act
            procSystem.ClearAll();

            // Assert
            for (ulong i = 1; i <= 5; i++)
            {
                Assert.That(procSystem.GetProcCount(i), Is.EqualTo(0),
                    $"Player {i} should have no procs after ClearAll");
            }

            // Cleanup
            Object.DestroyImmediate(procGO);
        }
    }
}
