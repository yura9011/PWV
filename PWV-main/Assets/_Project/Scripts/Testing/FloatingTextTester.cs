using UnityEngine;
using EtherDomes.Combat.Visuals;
using EtherDomes.Data;

namespace EtherDomes.Testing
{
    /// <summary>
    /// Simple tester for FloatingCombatText to verify size and functionality
    /// </summary>
    public class FloatingTextTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private KeyCode _testKey = KeyCode.T;
        [SerializeField] private float _testDamage = 125f;
        [SerializeField] private float _testHealing = 50f;

        private void Update()
        {
            if (Input.GetKeyDown(_testKey))
            {
                TestFloatingText();
            }
        }

        private void TestFloatingText()
        {
            Vector3 testPos = transform.position + Vector3.up * 2f;
            
            Debug.Log($"[FloatingTextTester] Testing floating text at position: {testPos}");
            
            // Test damage text
            FloatingCombatText.SpawnDamage(testPos, _testDamage, DamageType.Physical);
            
            // Test healing text (offset to the right)
            FloatingCombatText.SpawnHeal(testPos + Vector3.right * 2f, _testHealing);
            
            Debug.Log($"[FloatingTextTester] Spawned test texts - Damage: {_testDamage}, Heal: {_testHealing}");
        }

        private void OnDrawGizmos()
        {
            // Draw a sphere to show where the test will spawn
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f);
        }
    }
}