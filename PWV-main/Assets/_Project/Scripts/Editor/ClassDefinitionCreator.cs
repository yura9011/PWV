using UnityEngine;
using UnityEditor;
using EtherDomes.Data;
using System.IO;

namespace EtherDomes.Editor
{
    /// <summary>
    /// Editor tool para crear los 8 ScriptableObjects de clases.
    /// </summary>
    public static class ClassDefinitionCreator
    {
        private const string CLASSES_PATH = "Assets/_Project/ScriptableObjects/Classes";

        [MenuItem("EtherDomes/Data/Crear Definiciones de Clases")]
        public static void CreateAllClassDefinitions()
        {
            // Crear carpeta si no existe
            if (!Directory.Exists(CLASSES_PATH))
            {
                Directory.CreateDirectory(CLASSES_PATH);
                AssetDatabase.Refresh();
            }

            // Crear las 8 clases
            CreateCruzado();
            CreateProtector();
            CreateBerserker();
            CreateArquero();
            CreateMaestroElemental();
            CreateCaballeroRunico();
            CreateClerigo();
            CreateMedicoBrujo();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("[ClassDefinitionCreator] 8 clases creadas en " + CLASSES_PATH);
        }

        private static void CreateCruzado()
        {
            var def = ScriptableObject.CreateInstance<ClassDefinition>();
            def.ClassType = CharacterClass.Cruzado;
            def.ClassName = "Cruzado";
            def.Description = "Tank sagrado que combina fe y acero. Usa escudo mágico y maza.";
            def.Role = ClassRole.Tank;
            
            // Stats base (Tank - Alto HP/Stamina, Medio Str/Int)
            def.BaseHealth = 150;
            def.BaseMana = 80;
            def.BaseStrength = 12;
            def.BaseAgility = 8;
            def.BaseIntellect = 10;
            def.BaseStamina = 15;
            def.BaseArmor = 50;
            
            // Crecimiento
            def.HealthPerLevel = 15f;
            def.ManaPerLevel = 6f;
            def.StrengthPerLevel = 2f;
            def.AgilityPerLevel = 1f;
            def.IntellectPerLevel = 1.5f;
            def.StaminaPerLevel = 3f;
            
            // Equipo
            def.AllowedArmorType = ArmorType.ArmaduraPesada;
            def.AllowedWeaponTypes = new WeaponType[] { WeaponType.Maza1Mano, WeaponType.EscudoMagico };
            def.UsesTwoHandedWeapon = false;
            def.UsesShield = true;

            SaveAsset(def, "ClassDef_Cruzado");
        }

        private static void CreateProtector()
        {
            var def = ScriptableObject.CreateInstance<ClassDefinition>();
            def.ClassType = CharacterClass.Protector;
            def.ClassName = "Protector";
            def.Description = "Tank defensivo maestro del escudo y la espada.";
            def.Role = ClassRole.Tank;
            
            def.BaseHealth = 160;
            def.BaseMana = 50;
            def.BaseStrength = 14;
            def.BaseAgility = 8;
            def.BaseIntellect = 6;
            def.BaseStamina = 16;
            def.BaseArmor = 60;
            
            def.HealthPerLevel = 16f;
            def.ManaPerLevel = 4f;
            def.StrengthPerLevel = 2.5f;
            def.AgilityPerLevel = 1f;
            def.IntellectPerLevel = 0.5f;
            def.StaminaPerLevel = 3.5f;
            
            def.AllowedArmorType = ArmorType.ArmaduraPesada;
            def.AllowedWeaponTypes = new WeaponType[] { WeaponType.Espada1Mano, WeaponType.Escudo };
            def.UsesTwoHandedWeapon = false;
            def.UsesShield = true;

            SaveAsset(def, "ClassDef_Protector");
        }

        private static void CreateBerserker()
        {
            var def = ScriptableObject.CreateInstance<ClassDefinition>();
            def.ClassType = CharacterClass.Berserker;
            def.ClassName = "Berserker";
            def.Description = "DPS físico furioso que empuña una gran espada de dos manos.";
            def.Role = ClassRole.DPSFisico;
            
            def.BaseHealth = 120;
            def.BaseMana = 30;
            def.BaseStrength = 16;
            def.BaseAgility = 10;
            def.BaseIntellect = 5;
            def.BaseStamina = 12;
            def.BaseArmor = 40;
            
            def.HealthPerLevel = 12f;
            def.ManaPerLevel = 2f;
            def.StrengthPerLevel = 3f;
            def.AgilityPerLevel = 1.5f;
            def.IntellectPerLevel = 0.5f;
            def.StaminaPerLevel = 2f;
            
            def.AllowedArmorType = ArmorType.ArmaduraPesada;
            def.AllowedWeaponTypes = new WeaponType[] { WeaponType.Espada2Manos };
            def.UsesTwoHandedWeapon = true;
            def.UsesShield = false;

            SaveAsset(def, "ClassDef_Berserker");
        }

        private static void CreateArquero()
        {
            var def = ScriptableObject.CreateInstance<ClassDefinition>();
            def.ClassType = CharacterClass.Arquero;
            def.ClassName = "Arquero";
            def.Description = "DPS físico a distancia, maestro del arco.";
            def.Role = ClassRole.DPSFisico;
            
            def.BaseHealth = 100;
            def.BaseMana = 40;
            def.BaseStrength = 10;
            def.BaseAgility = 16;
            def.BaseIntellect = 6;
            def.BaseStamina = 10;
            def.BaseArmor = 20;
            
            def.HealthPerLevel = 10f;
            def.ManaPerLevel = 3f;
            def.StrengthPerLevel = 1.5f;
            def.AgilityPerLevel = 3f;
            def.IntellectPerLevel = 0.5f;
            def.StaminaPerLevel = 1.5f;
            
            def.AllowedArmorType = ArmorType.ArmaduraLigera;
            def.AllowedWeaponTypes = new WeaponType[] { WeaponType.Arco2Manos };
            def.UsesTwoHandedWeapon = true;
            def.UsesShield = false;

            SaveAsset(def, "ClassDef_Arquero");
        }

        private static void CreateMaestroElemental()
        {
            var def = ScriptableObject.CreateInstance<ClassDefinition>();
            def.ClassType = CharacterClass.MaestroElemental;
            def.ClassName = "Maestro Elemental";
            def.Description = "DPS mágico que domina los elementos con su orbe.";
            def.Role = ClassRole.DPSMagico;
            
            def.BaseHealth = 90;
            def.BaseMana = 120;
            def.BaseStrength = 5;
            def.BaseAgility = 8;
            def.BaseIntellect = 18;
            def.BaseStamina = 8;
            def.BaseArmor = 10;
            
            def.HealthPerLevel = 8f;
            def.ManaPerLevel = 10f;
            def.StrengthPerLevel = 0.5f;
            def.AgilityPerLevel = 1f;
            def.IntellectPerLevel = 3.5f;
            def.StaminaPerLevel = 1f;
            
            def.AllowedArmorType = ArmorType.ArmaduraEncantada;
            def.AllowedWeaponTypes = new WeaponType[] { WeaponType.Orbe2Manos };
            def.UsesTwoHandedWeapon = true;
            def.UsesShield = false;

            SaveAsset(def, "ClassDef_MaestroElemental");
        }

        private static void CreateCaballeroRunico()
        {
            var def = ScriptableObject.CreateInstance<ClassDefinition>();
            def.ClassType = CharacterClass.CaballeroRunico;
            def.ClassName = "Caballero Rúnico";
            def.Description = "DPS mágico-físico híbrido con espada mágica de dos manos.";
            def.Role = ClassRole.DPSMagico;
            
            def.BaseHealth = 110;
            def.BaseMana = 80;
            def.BaseStrength = 12;
            def.BaseAgility = 8;
            def.BaseIntellect = 14;
            def.BaseStamina = 11;
            def.BaseArmor = 35;
            
            def.HealthPerLevel = 11f;
            def.ManaPerLevel = 7f;
            def.StrengthPerLevel = 2f;
            def.AgilityPerLevel = 1f;
            def.IntellectPerLevel = 2.5f;
            def.StaminaPerLevel = 2f;
            
            def.AllowedArmorType = ArmorType.ArmaduraPesada;
            def.AllowedWeaponTypes = new WeaponType[] { WeaponType.EspadaMagica2Manos };
            def.UsesTwoHandedWeapon = true;
            def.UsesShield = false;

            SaveAsset(def, "ClassDef_CaballeroRunico");
        }

        private static void CreateClerigo()
        {
            var def = ScriptableObject.CreateInstance<ClassDefinition>();
            def.ClassType = CharacterClass.Clerigo;
            def.ClassName = "Clérigo";
            def.Description = "Healer sagrado que canaliza luz divina con su libro sagrado.";
            def.Role = ClassRole.Healer;
            
            def.BaseHealth = 95;
            def.BaseMana = 130;
            def.BaseStrength = 5;
            def.BaseAgility = 6;
            def.BaseIntellect = 16;
            def.BaseStamina = 9;
            def.BaseArmor = 15;
            
            def.HealthPerLevel = 9f;
            def.ManaPerLevel = 12f;
            def.StrengthPerLevel = 0.5f;
            def.AgilityPerLevel = 0.5f;
            def.IntellectPerLevel = 3f;
            def.StaminaPerLevel = 1.5f;
            
            def.AllowedArmorType = ArmorType.ArmaduraEncantada;
            def.AllowedWeaponTypes = new WeaponType[] { WeaponType.LibroSagrado2Manos };
            def.UsesTwoHandedWeapon = true;
            def.UsesShield = false;

            SaveAsset(def, "ClassDef_Clerigo");
        }

        private static void CreateMedicoBrujo()
        {
            var def = ScriptableObject.CreateInstance<ClassDefinition>();
            def.ClassType = CharacterClass.MedicoBrujo;
            def.ClassName = "Médico Brujo";
            def.Description = "Healer tribal que usa magia natural con su báculo.";
            def.Role = ClassRole.Healer;
            
            def.BaseHealth = 100;
            def.BaseMana = 120;
            def.BaseStrength = 6;
            def.BaseAgility = 10;
            def.BaseIntellect = 15;
            def.BaseStamina = 10;
            def.BaseArmor = 18;
            
            def.HealthPerLevel = 10f;
            def.ManaPerLevel = 11f;
            def.StrengthPerLevel = 0.5f;
            def.AgilityPerLevel = 1.5f;
            def.IntellectPerLevel = 2.8f;
            def.StaminaPerLevel = 1.5f;
            
            def.AllowedArmorType = ArmorType.ArmaduraLigera;
            def.AllowedWeaponTypes = new WeaponType[] { WeaponType.Baculo2Manos };
            def.UsesTwoHandedWeapon = true;
            def.UsesShield = false;

            SaveAsset(def, "ClassDef_MedicoBrujo");
        }

        private static void SaveAsset(ClassDefinition def, string fileName)
        {
            string path = $"{CLASSES_PATH}/{fileName}.asset";
            
            // Si ya existe, actualizar
            var existing = AssetDatabase.LoadAssetAtPath<ClassDefinition>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(def, existing);
                EditorUtility.SetDirty(existing);
                Debug.Log($"[ClassDefinitionCreator] Actualizado: {fileName}");
            }
            else
            {
                AssetDatabase.CreateAsset(def, path);
                Debug.Log($"[ClassDefinitionCreator] Creado: {fileName}");
            }
        }
    }
}
