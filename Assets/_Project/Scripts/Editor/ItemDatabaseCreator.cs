using UnityEngine;
using UnityEditor;
using EtherDomes.Data;
using System.IO;
using System.Collections.Generic;

namespace EtherDomes.Editor
{
    /// <summary>
    /// Editor tool para crear los items de prueba nivel 1.
    /// </summary>
    public static class ItemDatabaseCreator
    {
        private const string ITEMS_PATH = "Assets/_Project/ScriptableObjects/Items";

        [MenuItem("EtherDomes/Data/Crear Items de Prueba (Nivel 1)")]
        public static void CreateAllTestItems()
        {
            // Crear carpeta si no existe
            if (!Directory.Exists(ITEMS_PATH))
            {
                Directory.CreateDirectory(ITEMS_PATH);
                AssetDatabase.Refresh();
            }

            var allItems = new List<ItemData>();

            // Armaduras por tipo (3 tipos x 6 slots = 18 items)
            allItems.AddRange(CreateArmorSet(ArmorType.ArmaduraPesada, "ArmaduraPesada"));
            allItems.AddRange(CreateArmorSet(ArmorType.ArmaduraLigera, "ArmaduraLigera"));
            allItems.AddRange(CreateArmorSet(ArmorType.ArmaduraEncantada, "ArmaduraEncantada"));

            // Items universales (3 items)
            allItems.Add(CreateCapa());
            allItems.Add(CreateAnillo());
            allItems.Add(CreateAbalorio());

            // Armas (10 items)
            allItems.AddRange(CreateAllWeapons());

            // Crear/Actualizar ItemDatabase
            CreateOrUpdateItemDatabase(allItems);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"[ItemDatabaseCreator] {allItems.Count} items creados en {ITEMS_PATH}");
        }

        private static List<ItemData> CreateArmorSet(ArmorType armorType, string typeName)
        {
            var items = new List<ItemData>();
            int baseArmor = armorType switch
            {
                ArmorType.ArmaduraPesada => 15,
                ArmorType.ArmaduraLigera => 8,
                ArmorType.ArmaduraEncantada => 5,
                _ => 10
            };

            // Casco
            items.Add(new ItemData
            {
                ItemId = $"casco_{typeName.ToLower()}_test",
                ItemName = $"Casco{typeName}Test",
                Description = $"Casco de {typeName} de prueba nivel 1",
                ItemLevel = 1,
                Rarity = ItemRarity.Common,
                Type = ItemType.Equipment,
                Slot = EquipmentSlot.Head,
                RequiredLevel = 1,
                ArmorType = armorType,
                Stats = new ItemStats { Armor = baseArmor, Stamina = 2 },
                MaxDurability = 100,
                CurrentDurability = 100
            });

            // Hombreras
            items.Add(new ItemData
            {
                ItemId = $"hombreras_{typeName.ToLower()}_test",
                ItemName = $"Hombreras{typeName}Test",
                Description = $"Hombreras de {typeName} de prueba nivel 1",
                ItemLevel = 1,
                Rarity = ItemRarity.Common,
                Type = ItemType.Equipment,
                Slot = EquipmentSlot.Shoulders,
                RequiredLevel = 1,
                ArmorType = armorType,
                Stats = new ItemStats { Armor = baseArmor - 2, Stamina = 1 },
                MaxDurability = 100,
                CurrentDurability = 100
            });

            // Pechera
            items.Add(new ItemData
            {
                ItemId = $"pechera_{typeName.ToLower()}_test",
                ItemName = $"Pechera{typeName}Test",
                Description = $"Pechera de {typeName} de prueba nivel 1",
                ItemLevel = 1,
                Rarity = ItemRarity.Common,
                Type = ItemType.Equipment,
                Slot = EquipmentSlot.Chest,
                RequiredLevel = 1,
                ArmorType = armorType,
                Stats = new ItemStats { Armor = baseArmor + 5, Stamina = 3 },
                MaxDurability = 100,
                CurrentDurability = 100
            });

            // Guantes
            items.Add(new ItemData
            {
                ItemId = $"guantes_{typeName.ToLower()}_test",
                ItemName = $"Guantes{typeName}Test",
                Description = $"Guantes de {typeName} de prueba nivel 1",
                ItemLevel = 1,
                Rarity = ItemRarity.Common,
                Type = ItemType.Equipment,
                Slot = EquipmentSlot.Hands,
                RequiredLevel = 1,
                ArmorType = armorType,
                Stats = new ItemStats { Armor = baseArmor - 3, Stamina = 1 },
                MaxDurability = 100,
                CurrentDurability = 100
            });

            // Pantalones
            items.Add(new ItemData
            {
                ItemId = $"pantalones_{typeName.ToLower()}_test",
                ItemName = $"Pantalones{typeName}Test",
                Description = $"Pantalones de {typeName} de prueba nivel 1",
                ItemLevel = 1,
                Rarity = ItemRarity.Common,
                Type = ItemType.Equipment,
                Slot = EquipmentSlot.Legs,
                RequiredLevel = 1,
                ArmorType = armorType,
                Stats = new ItemStats { Armor = baseArmor + 2, Stamina = 2 },
                MaxDurability = 100,
                CurrentDurability = 100
            });

            // Botas
            items.Add(new ItemData
            {
                ItemId = $"botas_{typeName.ToLower()}_test",
                ItemName = $"Botas{typeName}Test",
                Description = $"Botas de {typeName} de prueba nivel 1",
                ItemLevel = 1,
                Rarity = ItemRarity.Common,
                Type = ItemType.Equipment,
                Slot = EquipmentSlot.Feet,
                RequiredLevel = 1,
                ArmorType = armorType,
                Stats = new ItemStats { Armor = baseArmor - 2, Stamina = 1, Agility = 1 },
                MaxDurability = 100,
                CurrentDurability = 100
            });

            return items;
        }

        private static ItemData CreateCapa()
        {
            return new ItemData
            {
                ItemId = "capa_test",
                ItemName = "CapaTest",
                Description = "Capa universal de prueba nivel 1",
                ItemLevel = 1,
                Rarity = ItemRarity.Common,
                Type = ItemType.Equipment,
                Slot = EquipmentSlot.Back,
                RequiredLevel = 1,
                Stats = new ItemStats { Stamina = 2, Armor = 3 },
                MaxDurability = 100,
                CurrentDurability = 100
            };
        }

        private static ItemData CreateAnillo()
        {
            return new ItemData
            {
                ItemId = "anillo_test",
                ItemName = "AnilloTest",
                Description = "Anillo universal de prueba nivel 1",
                ItemLevel = 1,
                Rarity = ItemRarity.Common,
                Type = ItemType.Equipment,
                Slot = EquipmentSlot.Finger,
                RequiredLevel = 1,
                Stats = new ItemStats { Stamina = 1, CriticalStrike = 1 },
                MaxDurability = 100,
                CurrentDurability = 100
            };
        }

        private static ItemData CreateAbalorio()
        {
            return new ItemData
            {
                ItemId = "abalorio_test",
                ItemName = "AbalorioTest",
                Description = "Abalorio universal de prueba nivel 1",
                ItemLevel = 1,
                Rarity = ItemRarity.Common,
                Type = ItemType.Equipment,
                Slot = EquipmentSlot.Trinket,
                RequiredLevel = 1,
                Stats = new ItemStats { Stamina = 1, Haste = 1 },
                MaxDurability = 100,
                CurrentDurability = 100
            };
        }

        private static List<ItemData> CreateAllWeapons()
        {
            var weapons = new List<ItemData>();

            // Espada 2 manos (Berserker)
            weapons.Add(new ItemData
            {
                ItemId = "espada_2manos_test",
                ItemName = "Espada2ManosTest",
                Description = "Espada de dos manos de prueba nivel 1",
                ItemLevel = 1,
                Rarity = ItemRarity.Common,
                Type = ItemType.Equipment,
                Slot = EquipmentSlot.MainHand,
                RequiredLevel = 1,
                WeaponType = WeaponType.Espada2Manos,
                IsTwoHanded = true,
                AllowedClasses = new[] { CharacterClass.Berserker },
                Stats = new ItemStats { Strength = 5, AttackPower = 10 },
                MaxDurability = 100,
                CurrentDurability = 100
            });

            // Espada 1 mano (Protector)
            weapons.Add(new ItemData
            {
                ItemId = "espada_1mano_test",
                ItemName = "Espada1ManoTest",
                Description = "Espada de una mano de prueba nivel 1",
                ItemLevel = 1,
                Rarity = ItemRarity.Common,
                Type = ItemType.Equipment,
                Slot = EquipmentSlot.MainHand,
                RequiredLevel = 1,
                WeaponType = WeaponType.Espada1Mano,
                IsTwoHanded = false,
                AllowedClasses = new[] { CharacterClass.Protector },
                Stats = new ItemStats { Strength = 3, AttackPower = 6 },
                MaxDurability = 100,
                CurrentDurability = 100
            });

            // Escudo (Protector)
            weapons.Add(new ItemData
            {
                ItemId = "escudo_test",
                ItemName = "EscudoTest",
                Description = "Escudo de prueba nivel 1",
                ItemLevel = 1,
                Rarity = ItemRarity.Common,
                Type = ItemType.Equipment,
                Slot = EquipmentSlot.OffHand,
                RequiredLevel = 1,
                WeaponType = WeaponType.Escudo,
                IsTwoHanded = false,
                AllowedClasses = new[] { CharacterClass.Protector },
                Stats = new ItemStats { Armor = 20, Stamina = 3 },
                MaxDurability = 100,
                CurrentDurability = 100
            });

            // Escudo Mágico (Cruzado)
            weapons.Add(new ItemData
            {
                ItemId = "escudo_magico_test",
                ItemName = "EscudoMagicoTest",
                Description = "Escudo mágico de prueba nivel 1",
                ItemLevel = 1,
                Rarity = ItemRarity.Common,
                Type = ItemType.Equipment,
                Slot = EquipmentSlot.OffHand,
                RequiredLevel = 1,
                WeaponType = WeaponType.EscudoMagico,
                IsTwoHanded = false,
                AllowedClasses = new[] { CharacterClass.Cruzado },
                Stats = new ItemStats { Armor = 15, Stamina = 2, Intellect = 2 },
                MaxDurability = 100,
                CurrentDurability = 100
            });

            // Maza 1 mano (Cruzado)
            weapons.Add(new ItemData
            {
                ItemId = "maza_1mano_test",
                ItemName = "Maza1ManoTest",
                Description = "Maza de una mano de prueba nivel 1",
                ItemLevel = 1,
                Rarity = ItemRarity.Common,
                Type = ItemType.Equipment,
                Slot = EquipmentSlot.MainHand,
                RequiredLevel = 1,
                WeaponType = WeaponType.Maza1Mano,
                IsTwoHanded = false,
                AllowedClasses = new[] { CharacterClass.Cruzado },
                Stats = new ItemStats { Strength = 2, Intellect = 2, AttackPower = 4 },
                MaxDurability = 100,
                CurrentDurability = 100
            });

            // Arco 2 manos (Arquero)
            weapons.Add(new ItemData
            {
                ItemId = "arco_2manos_test",
                ItemName = "Arco2ManosTest",
                Description = "Arco de dos manos de prueba nivel 1",
                ItemLevel = 1,
                Rarity = ItemRarity.Common,
                Type = ItemType.Equipment,
                Slot = EquipmentSlot.MainHand,
                RequiredLevel = 1,
                WeaponType = WeaponType.Arco2Manos,
                IsTwoHanded = true,
                AllowedClasses = new[] { CharacterClass.Arquero },
                Stats = new ItemStats { Agility = 5, AttackPower = 8, CriticalStrike = 2 },
                MaxDurability = 100,
                CurrentDurability = 100
            });

            // Espada Mágica 2 manos (Caballero Rúnico)
            weapons.Add(new ItemData
            {
                ItemId = "espada_magica_2manos_test",
                ItemName = "EspadaMagica2ManosTest",
                Description = "Espada mágica de dos manos de prueba nivel 1",
                ItemLevel = 1,
                Rarity = ItemRarity.Common,
                Type = ItemType.Equipment,
                Slot = EquipmentSlot.MainHand,
                RequiredLevel = 1,
                WeaponType = WeaponType.EspadaMagica2Manos,
                IsTwoHanded = true,
                AllowedClasses = new[] { CharacterClass.CaballeroRunico },
                Stats = new ItemStats { Strength = 3, Intellect = 3, AttackPower = 5, SpellPower = 5 },
                MaxDurability = 100,
                CurrentDurability = 100
            });

            // Orbe 2 manos (Maestro Elemental)
            weapons.Add(new ItemData
            {
                ItemId = "orbe_2manos_test",
                ItemName = "Orbe2ManosTest",
                Description = "Orbe de dos manos de prueba nivel 1",
                ItemLevel = 1,
                Rarity = ItemRarity.Common,
                Type = ItemType.Equipment,
                Slot = EquipmentSlot.MainHand,
                RequiredLevel = 1,
                WeaponType = WeaponType.Orbe2Manos,
                IsTwoHanded = true,
                AllowedClasses = new[] { CharacterClass.MaestroElemental },
                Stats = new ItemStats { Intellect = 6, SpellPower = 12, CriticalStrike = 2 },
                MaxDurability = 100,
                CurrentDurability = 100
            });

            // Báculo 2 manos (Médico Brujo)
            weapons.Add(new ItemData
            {
                ItemId = "baculo_2manos_test",
                ItemName = "Baculo2ManosTest",
                Description = "Báculo de dos manos de prueba nivel 1",
                ItemLevel = 1,
                Rarity = ItemRarity.Common,
                Type = ItemType.Equipment,
                Slot = EquipmentSlot.MainHand,
                RequiredLevel = 1,
                WeaponType = WeaponType.Baculo2Manos,
                IsTwoHanded = true,
                AllowedClasses = new[] { CharacterClass.MedicoBrujo },
                Stats = new ItemStats { Intellect = 5, SpellPower = 10, Haste = 2 },
                MaxDurability = 100,
                CurrentDurability = 100
            });

            // Libro Sagrado 2 manos (Clérigo)
            weapons.Add(new ItemData
            {
                ItemId = "libro_sagrado_2manos_test",
                ItemName = "LibroSagrado2ManosTest",
                Description = "Libro sagrado de dos manos de prueba nivel 1",
                ItemLevel = 1,
                Rarity = ItemRarity.Common,
                Type = ItemType.Equipment,
                Slot = EquipmentSlot.MainHand,
                RequiredLevel = 1,
                WeaponType = WeaponType.LibroSagrado2Manos,
                IsTwoHanded = true,
                AllowedClasses = new[] { CharacterClass.Clerigo },
                Stats = new ItemStats { Intellect = 5, SpellPower = 10, Stamina = 2 },
                MaxDurability = 100,
                CurrentDurability = 100
            });

            return weapons;
        }

        private static void CreateOrUpdateItemDatabase(List<ItemData> items)
        {
            string dbPath = $"{ITEMS_PATH}/ItemDatabase.asset";
            
            var db = AssetDatabase.LoadAssetAtPath<ItemDatabase>(dbPath);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<ItemDatabase>();
                AssetDatabase.CreateAsset(db, dbPath);
            }

            db.AllItems = items;
            EditorUtility.SetDirty(db);
            
            Debug.Log($"[ItemDatabaseCreator] ItemDatabase actualizado con {items.Count} items");
        }
    }
}
