using UnityEngine;
using System.Collections;

namespace EtherDomes.Persistence
{
    /// <summary>
    /// Manages saving and loading player position using PlayerPrefs.
    /// </summary>
    public class PositionPersistenceManager : MonoBehaviour
    {
        private const string POSITION_X_KEY = "LastPosition_X";
        private const string POSITION_Y_KEY = "LastPosition_Y";
        private const string POSITION_Z_KEY = "LastPosition_Z";
        private const string HAS_POSITION_KEY = "HasSavedPosition";
        
        [SerializeField] private float _autoSaveInterval = 30f;
        [SerializeField] private Transform _targetTransform;
        
        private Coroutine _autoSaveCoroutine;
        private static PositionPersistenceManager _instance;
        
        public static PositionPersistenceManager Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        /// <summary>
        /// Sets the transform to track for auto-save.
        /// </summary>
        public void SetTarget(Transform target)
        {
            _targetTransform = target;
            
            if (_targetTransform != null && _autoSaveCoroutine == null)
            {
                _autoSaveCoroutine = StartCoroutine(AutoSaveCoroutine());
            }
        }

        /// <summary>
        /// Saves a position to PlayerPrefs.
        /// </summary>
        public static void SavePosition(Vector3 position)
        {
            PlayerPrefs.SetFloat(POSITION_X_KEY, position.x);
            PlayerPrefs.SetFloat(POSITION_Y_KEY, position.y);
            PlayerPrefs.SetFloat(POSITION_Z_KEY, position.z);
            PlayerPrefs.SetInt(HAS_POSITION_KEY, 1);
            PlayerPrefs.Save();
            
            Debug.Log($"[PositionPersistence] Saved position: {position}");
        }

        /// <summary>
        /// Loads the saved position or returns default.
        /// </summary>
        public static Vector3 LoadPosition(Vector3 defaultPosition)
        {
            if (!HasSavedPosition())
            {
                Debug.Log($"[PositionPersistence] No saved position, using default: {defaultPosition}");
                return defaultPosition;
            }
            
            Vector3 position = new Vector3(
                PlayerPrefs.GetFloat(POSITION_X_KEY),
                PlayerPrefs.GetFloat(POSITION_Y_KEY),
                PlayerPrefs.GetFloat(POSITION_Z_KEY)
            );
            
            Debug.Log($"[PositionPersistence] Loaded position: {position}");
            return position;
        }

        /// <summary>
        /// Checks if there's a saved position.
        /// </summary>
        public static bool HasSavedPosition()
        {
            return PlayerPrefs.GetInt(HAS_POSITION_KEY, 0) == 1;
        }

        /// <summary>
        /// Clears the saved position.
        /// </summary>
        public static void ClearSavedPosition()
        {
            PlayerPrefs.DeleteKey(POSITION_X_KEY);
            PlayerPrefs.DeleteKey(POSITION_Y_KEY);
            PlayerPrefs.DeleteKey(POSITION_Z_KEY);
            PlayerPrefs.DeleteKey(HAS_POSITION_KEY);
            PlayerPrefs.Save();
            
            Debug.Log("[PositionPersistence] Cleared saved position");
        }

        private IEnumerator AutoSaveCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(_autoSaveInterval);
                
                if (_targetTransform != null)
                {
                    SavePosition(_targetTransform.position);
                }
            }
        }

        /// <summary>
        /// Saves current position immediately (call on disconnect).
        /// </summary>
        public void SaveCurrentPosition()
        {
            if (_targetTransform != null)
            {
                SavePosition(_targetTransform.position);
            }
        }

        private void OnDestroy()
        {
            // Save position when destroyed (disconnect)
            SaveCurrentPosition();
            
            if (_autoSaveCoroutine != null)
            {
                StopCoroutine(_autoSaveCoroutine);
            }
        }
    }
}
