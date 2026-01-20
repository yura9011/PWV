using UnityEngine;
using EtherDomes.Persistence;

namespace EtherDomes.Testing
{
    public class PersistenceTester : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("--- Testing Persistence ---");
            if (SaveManager.Instance == null)
            {
                Debug.LogError("SaveManager Instance is NULL!");
                return;
            }

            Debug.Log("1. Current Save Characters: " + SaveManager.Instance.CurrentSave.Characters.Count);
            
            Debug.Log("2. Creating Test Character...");
            SaveManager.Instance.CreateCharacter("TestHero_" + Random.Range(0, 100), 0);
            
            Debug.Log("3. Characters after create: " + SaveManager.Instance.CurrentSave.Characters.Count);

            Debug.Log("4. Force Saving...");
            SaveManager.Instance.Save();

            Debug.Log("5. Reloading...");
            SaveManager.Instance.Load();
            
            Debug.Log("6. Characters after reload: " + SaveManager.Instance.CurrentSave.Characters.Count);
            Debug.Log("--- Test Complete ---");
        }
    }
}
