using UnityEngine;
using EtherDomes.Persistence;
using EtherDomes.Data;
using System.IO;

namespace EtherDomes.Testing
{
    /// <summary>
    /// Tester específico para verificar la funcionalidad Cross-World:
    /// - Crear personaje
    /// - Guardar stats encriptados
    /// - Cargar en otra sesión
    /// - Persistir posición entre mundos
    /// </summary>
    public class CrossWorldTester : MonoBehaviour
    {
        [Header("Cross-World Test Configuration")]
        [SerializeField] private bool _runTestOnStart = false;
        [SerializeField] private string _testCharacterName = "CrossWorldHero";
        [SerializeField] private CharacterClass _testClass = CharacterClass.Cruzado;
        
        private void Start()
        {
            if (_runTestOnStart)
            {
                RunCrossWorldTest();
            }
        }
        
        [ContextMenu("Run Cross-World Test")]
        public void RunCrossWorldTest()
        {
            Debug.Log("=== CROSS-WORLD FUNCTIONALITY TEST ===");
            
            TestCharacterCreationAndPersistence();
            TestPositionPersistence();
            TestEncryptedFileSecurity();
            TestSessionReload();
            
            Debug.Log("=== CROSS-WORLD TEST COMPLETED ===");
        }
        
        /// <summary>
        /// Prueba 1: Crear personaje y verificar que se guarda encriptado
        /// </summary>
        [ContextMenu("Test Character Creation & Persistence")]
        public void TestCharacterCreationAndPersistence()
        {
            Debug.Log("--- Testing Character Creation & Persistence ---");
            
            var saveManager = SaveManager.Instance;
            if (saveManager == null)
            {
                Debug.LogError("❌ SaveManager not found!");
                return;
            }
            
            // Limpiar personaje de prueba anterior si existe
            var existingChar = saveManager.CurrentSave.Characters.Find(c => c.Name == _testCharacterName);
            if (existingChar != null)
            {
                saveManager.DeleteCharacter(existingChar.CharacterId);
                Debug.Log("🧹 Cleaned up existing test character");
            }
            
            // Crear nuevo personaje
            bool createResult = saveManager.CreateCharacter(_testCharacterName, _testClass);
            if (!createResult)
            {
                Debug.LogError("❌ Failed to create test character!");
                return;
            }
            
            // Obtener el personaje creado
            var character = saveManager.CurrentSave.Characters.Find(c => c.Name == _testCharacterName);
            if (character == null)
            {
                Debug.LogError("❌ Created character not found!");
                return;
            }
            
            // Modificar stats para simular progreso
            character.Level = 15;
            character.CurrentXP = 5000;
            character.CurrentHP = 250;
            character.MaxHP = 300;
            character.CurrentMana = 150;
            character.MaxMana = 200;
            character.TotalStrength = 25;
            character.TotalAgility = 20;
            character.TotalIntellect = 18;
            character.LastWorldId = "TestWorld_1";
            character.LastPosition = new Vector3(10f, 2f, -5f);
            
            // Guardar cambios
            bool updateResult = saveManager.UpdateCharacter(character);
            if (updateResult)
            {
                Debug.Log($"✅ Character '{_testCharacterName}' created and updated successfully");
                Debug.Log($"   - Level: {character.Level}, XP: {character.CurrentXP}");
                Debug.Log($"   - HP: {character.CurrentHP}/{character.MaxHP}");
                Debug.Log($"   - Mana: {character.CurrentMana}/{character.MaxMana}");
                Debug.Log($"   - Stats: STR:{character.TotalStrength} AGI:{character.TotalAgility} INT:{character.TotalIntellect}");
                Debug.Log($"   - Last World: {character.LastWorldId}");
                Debug.Log($"   - Last Position: {character.LastPosition}");
            }
            else
            {
                Debug.LogError("❌ Failed to update character!");
            }
        }
        
        /// <summary>
        /// Prueba 2: Verificar persistencia de posición entre sesiones
        /// </summary>
        [ContextMenu("Test Position Persistence")]
        public void TestPositionPersistence()
        {
            Debug.Log("--- Testing Position Persistence ---");
            
            // Simular posición en mundo actual
            Vector3 testPosition = new Vector3(25f, 5f, -10f);
            PositionPersistenceManager.SavePosition(testPosition);
            
            // Verificar que se guardó
            if (PositionPersistenceManager.HasSavedPosition())
            {
                Vector3 loadedPosition = PositionPersistenceManager.LoadPosition(Vector3.zero);
                
                if (Vector3.Distance(testPosition, loadedPosition) < 0.1f)
                {
                    Debug.Log($"✅ Position persistence working - Saved: {testPosition}, Loaded: {loadedPosition}");
                }
                else
                {
                    Debug.LogError($"❌ Position mismatch - Saved: {testPosition}, Loaded: {loadedPosition}");
                }
            }
            else
            {
                Debug.LogError("❌ Position was not saved!");
            }
        }
        
        /// <summary>
        /// Prueba 3: Verificar que el archivo está encriptado (anti-trampas)
        /// </summary>
        [ContextMenu("Test Encrypted File Security")]
        public void TestEncryptedFileSecurity()
        {
            Debug.Log("--- Testing Encrypted File Security ---");
            
            string savePath = Path.Combine(Application.persistentDataPath, "etherdomes_save.ted");
            
            if (!File.Exists(savePath))
            {
                Debug.LogError("❌ Save file not found!");
                return;
            }
            
            // Leer contenido del archivo
            string fileContent = File.ReadAllText(savePath);
            
            // Verificar que está encriptado (no contiene datos legibles)
            if (fileContent.Contains("EncryptedData") && fileContent.Contains("IntegrityHash"))
            {
                Debug.Log("✅ Save file is properly encrypted");
                
                // Verificar que los datos del personaje NO están en texto plano
                if (!fileContent.Contains(_testCharacterName) && !fileContent.Contains("TestWorld_1"))
                {
                    Debug.Log("✅ Character data is encrypted (not readable in plain text)");
                }
                else
                {
                    Debug.LogWarning("⚠️ Character data might be visible in plain text!");
                }
                
                // Mostrar estructura del archivo (sin datos sensibles)
                Debug.Log($"File structure preview: {fileContent.Substring(0, Mathf.Min(200, fileContent.Length))}...");
            }
            else
            {
                Debug.LogWarning("⚠️ Save file appears to be in legacy format (unencrypted)");
            }
        }
        
        /// <summary>
        /// Prueba 4: Simular recarga de sesión (Cross-World)
        /// </summary>
        [ContextMenu("Test Session Reload")]
        public void TestSessionReload()
        {
            Debug.Log("--- Testing Session Reload (Cross-World) ---");
            
            var saveManager = SaveManager.Instance;
            if (saveManager == null)
            {
                Debug.LogError("❌ SaveManager not found!");
                return;
            }
            
            // Obtener datos del personaje antes de la "recarga"
            var originalChar = saveManager.CurrentSave.Characters.Find(c => c.Name == _testCharacterName);
            if (originalChar == null)
            {
                Debug.LogError("❌ Test character not found for reload test!");
                return;
            }
            
            // Guardar datos originales
            int originalLevel = originalChar.Level;
            float originalXP = originalChar.CurrentXP;
            Vector3 originalPosition = originalChar.LastPosition;
            string originalWorldId = originalChar.LastWorldId;
            
            // Forzar guardado
            saveManager.ForceSave();
            
            // Simular recarga forzando una nueva carga
            bool reloadResult = saveManager.Load();
            
            if (!reloadResult)
            {
                Debug.LogError("❌ Failed to reload save file!");
                return;
            }
            
            // Verificar que los datos se mantuvieron
            var reloadedChar = saveManager.CurrentSave.Characters.Find(c => c.Name == _testCharacterName);
            if (reloadedChar == null)
            {
                Debug.LogError("❌ Character lost after reload!");
                return;
            }
            
            // Comparar datos
            bool dataIntact = (reloadedChar.Level == originalLevel &&
                              reloadedChar.CurrentXP == originalXP &&
                              Vector3.Distance(reloadedChar.LastPosition, originalPosition) < 0.1f &&
                              reloadedChar.LastWorldId == originalWorldId);
            
            if (dataIntact)
            {
                Debug.Log("✅ Session reload successful - all character data preserved");
                Debug.Log($"   - Level: {reloadedChar.Level} (preserved)");
                Debug.Log($"   - XP: {reloadedChar.CurrentXP} (preserved)");
                Debug.Log($"   - Position: {reloadedChar.LastPosition} (preserved)");
                Debug.Log($"   - World: {reloadedChar.LastWorldId} (preserved)");
            }
            else
            {
                Debug.LogError("❌ Data corruption detected after reload!");
                Debug.LogError($"   Level: {originalLevel} -> {reloadedChar.Level}");
                Debug.LogError($"   XP: {originalXP} -> {reloadedChar.CurrentXP}");
                Debug.LogError($"   Position: {originalPosition} -> {reloadedChar.LastPosition}");
                Debug.LogError($"   World: {originalWorldId} -> {reloadedChar.LastWorldId}");
            }
        }
        
        [ContextMenu("Show Cross-World Status")]
        public void ShowCrossWorldStatus()
        {
            Debug.Log("=== CROSS-WORLD STATUS ===");
            
            var saveManager = SaveManager.Instance;
            if (saveManager?.CurrentSave != null)
            {
                Debug.Log($"Characters: {saveManager.CurrentSave.Characters.Count}/{SaveFile.MAX_CHARACTERS}");
                
                foreach (var character in saveManager.CurrentSave.Characters)
                {
                    Debug.Log($"  - {character.Name} (Lv.{character.Level}) in {character.LastWorldId} at {character.LastPosition}");
                }
            }
            
            if (PositionPersistenceManager.HasSavedPosition())
            {
                Vector3 pos = PositionPersistenceManager.LoadPosition(Vector3.zero);
                Debug.Log($"Current session position: {pos}");
            }
            else
            {
                Debug.Log("No session position saved");
            }
            
            string savePath = Path.Combine(Application.persistentDataPath, "etherdomes_save.ted");
            if (File.Exists(savePath))
            {
                FileInfo fileInfo = new FileInfo(savePath);
                Debug.Log($"Save file: {fileInfo.Length} bytes, modified {fileInfo.LastWriteTime}");
            }
        }
        
        [ContextMenu("Clean Up Test Data")]
        public void CleanUpTestData()
        {
            var saveManager = SaveManager.Instance;
            if (saveManager != null)
            {
                var testChar = saveManager.CurrentSave.Characters.Find(c => c.Name == _testCharacterName);
                if (testChar != null)
                {
                    saveManager.DeleteCharacter(testChar.CharacterId);
                    Debug.Log($"🧹 Cleaned up test character: {_testCharacterName}");
                }
            }
            
            PositionPersistenceManager.ClearSavedPosition();
            Debug.Log("🧹 Cleaned up position data");
        }
    }
}