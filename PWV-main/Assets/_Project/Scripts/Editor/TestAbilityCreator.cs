#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using EtherDomes.Combat.Abilities;
using EtherDomes.Data;

namespace EtherDomes.Editor
{
    /// <summary>
    /// Editor utility to create test ability assets for Phase 3 combat system.
    /// </summary>
    public static class TestAbilityCreator
    {
        private const string ABILITY_PATH = "Assets/_Project/Data/Abilities/";

        [MenuItem("EtherDomes/Combat/Create Test Abilities")]
        public static void CreateTestAbilities()
        {
            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Data"))
                AssetDatabase.CreateFolder("Assets/_Project", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Data/Abilities"))
                AssetDatabase.CreateFolder("Assets/_Project/Data", "Abilities");

            // Create Bola de Fuego (Fireball)
            CreateBolaFuego();
            
            // Create Disparo Rápido (Quick Shot)
            CreateDisparoRapido();
            
            // Create Escudo de Hielo (Ice Shield)
            CreateEscudoHielo();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("[TestAbilityCreator] Created 3 test abilities in " + ABILITY_PATH);
        }

        private static void CreateBolaFuego()
        {
            var ability = ScriptableObject.CreateInstance<AbilityDefinitionSO>();
            
            ability.AbilityID = 1001;
            ability.AbilityName = "Bola de Fuego";
            ability.Description = "Lanza una bola de fuego que inflige daño de fuego alto al objetivo.";
            
            ability.CastTime = 2f;
            ability.Cooldown = 3f;
            ability.Range = 35f;
            ability.ResourceCost = 25;
            ability.ResourceType = SecondaryResourceType.Mana;
            
            ability.RequiresStationary = true;
            ability.TriggersGCD = true;
            ability.IsOffensive = true;
            ability.IsSelfCast = false;
            
            ability.BaseDamage = 150f;
            ability.DamageType = DamageType.Fire;
            ability.StatMultiplier = 1.2f;
            
            ability.ProjectileSpeed = 25f;
            ability.CastAnimationTrigger = "CastFire";
            ability.AttackAnimationTrigger = "Attack";

            SaveAbility(ability, "Ability_BolaFuego");
        }

        private static void CreateDisparoRapido()
        {
            var ability = ScriptableObject.CreateInstance<AbilityDefinitionSO>();
            
            ability.AbilityID = 1002;
            ability.AbilityName = "Disparo Rápido";
            ability.Description = "Dispara una flecha rápida que inflige daño físico medio. Puede usarse en movimiento.";
            
            ability.CastTime = 0f; // Instant
            ability.Cooldown = 1.5f;
            ability.Range = 40f;
            ability.ResourceCost = 15;
            ability.ResourceType = SecondaryResourceType.Energia;
            
            ability.RequiresStationary = false; // Can move while using
            ability.TriggersGCD = true;
            ability.IsOffensive = true;
            ability.IsSelfCast = false;
            
            ability.BaseDamage = 80f;
            ability.DamageType = DamageType.Physical;
            ability.StatMultiplier = 1f;
            
            ability.ProjectileSpeed = 50f;
            ability.CastAnimationTrigger = "Shoot";
            ability.AttackAnimationTrigger = "Attack";

            SaveAbility(ability, "Ability_DisparoRapido");
        }

        private static void CreateEscudoHielo()
        {
            var ability = ScriptableObject.CreateInstance<AbilityDefinitionSO>();
            
            ability.AbilityID = 1003;
            ability.AbilityName = "Escudo de Hielo";
            ability.Description = "Te envuelves en un escudo de hielo que absorbe daño. Puede usarse durante el GCD.";
            
            ability.CastTime = 0f; // Instant
            ability.Cooldown = 30f;
            ability.Range = 0f; // Self-cast
            ability.ResourceCost = 0; // Free
            ability.ResourceType = SecondaryResourceType.None;
            
            ability.RequiresStationary = false;
            ability.TriggersGCD = false; // Off-GCD
            ability.IsOffensive = false;
            ability.IsSelfCast = true;
            
            ability.BaseDamage = 0f;
            ability.BaseHealing = 0f; // It's a shield, not healing
            ability.DamageType = DamageType.Frost;
            
            ability.CastAnimationTrigger = "Shield";

            SaveAbility(ability, "Ability_EscudoHielo");
        }

        private static void SaveAbility(AbilityDefinitionSO ability, string fileName)
        {
            string path = ABILITY_PATH + fileName + ".asset";
            
            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<AbilityDefinitionSO>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(ability, existing);
                EditorUtility.SetDirty(existing);
                Debug.Log($"[TestAbilityCreator] Updated: {fileName}");
            }
            else
            {
                AssetDatabase.CreateAsset(ability, path);
                Debug.Log($"[TestAbilityCreator] Created: {fileName}");
            }
        }
    }
}
#endif
