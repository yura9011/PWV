using UnityEngine;
using UnityEngine.UI;
using EtherDomes.Data;

namespace EtherDomes.Combat.Visuals
{
    /// <summary>
    /// Floating combat text that displays damage/healing numbers.
    /// Animates upward and fades out.
    /// </summary>
    public class FloatingCombatText : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float _duration = 1.5f;
        [SerializeField] private float _riseHeight = 1f;
        [SerializeField] private float _randomOffsetRange = 0.5f;
        [SerializeField] private AnimationCurve _scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.5f);
        [SerializeField] private AnimationCurve _alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [Header("References")]
        [SerializeField] private Text _legacyText;
        [SerializeField] private Canvas _canvas;

        [Header("Colors")]
        [SerializeField] private Color _physicalDamageColor = Color.white;
        [SerializeField] private Color _fireDamageColor = new Color(1f, 0.5f, 0f);
        [SerializeField] private Color _frostDamageColor = new Color(0.5f, 0.8f, 1f);
        [SerializeField] private Color _holyDamageColor = new Color(1f, 1f, 0.5f);
        [SerializeField] private Color _shadowDamageColor = new Color(0.5f, 0f, 0.5f);
        [SerializeField] private Color _natureDamageColor = new Color(0.5f, 1f, 0.5f);
        [SerializeField] private Color _arcaneDamageColor = new Color(0.8f, 0.5f, 1f);
        [SerializeField] private Color _healingColor = Color.green;
        [SerializeField] private Color _critColor = Color.yellow;

        private Vector3 _startPosition;
        private float _elapsed;
        private Color _baseColor;

        private static GameObject _prefab;
        private static CombatConfigSO _config;

        #region Static Spawn Methods

        /// <summary>
        /// Spawns damage floating text at position.
        /// </summary>
        public static void SpawnDamage(Vector3 position, float amount, DamageType type, bool isCrit = false)
        {
            var instance = SpawnInstance(position);
            if (instance == null) return;

            instance.SetupDamage(amount, type, isCrit);
        }

        /// <summary>
        /// Spawns healing floating text at position.
        /// </summary>
        public static void SpawnHeal(Vector3 position, float amount, bool isCrit = false)
        {
            var instance = SpawnInstance(position);
            if (instance == null) return;

            instance.SetupHealing(amount, isCrit);
        }

        private static FloatingCombatText SpawnInstance(Vector3 position)
        {
            if (_prefab == null)
            {
                _prefab = Resources.Load<GameObject>("FloatingCombatText");
                if (_prefab == null)
                {
                    Debug.LogWarning("[FloatingCombatText] Prefab not found in Resources");
                    return null;
                }
            }

            // Add random offset
            Vector3 offset = new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(0f, 0.3f),
                Random.Range(-0.5f, 0.5f)
            );

            var go = Instantiate(_prefab, position + offset, Quaternion.identity);
            return go.GetComponent<FloatingCombatText>();
        }

        #endregion

        #region Setup Methods

        private void SetupDamage(float amount, DamageType type, bool isCrit)
        {
            _baseColor = GetDamageColor(type);
            
            string text = Mathf.RoundToInt(amount).ToString();
            if (isCrit)
            {
                text = $"*{text}*";
                _baseColor = _critColor;
                transform.localScale *= 1.5f;
            }

            if (_legacyText != null)
            {
                _legacyText.text = text;
                _legacyText.color = _baseColor;
            }
        }

        private void SetupHealing(float amount, bool isCrit)
        {
            _baseColor = _healingColor;
            
            string text = $"+{Mathf.RoundToInt(amount)}";
            if (isCrit)
            {
                text = $"*{text}*";
                _baseColor = _critColor;
                transform.localScale *= 1.5f;
            }

            if (_legacyText != null)
            {
                _legacyText.text = text;
                _legacyText.color = _baseColor;
            }
        }

        private Color GetDamageColor(DamageType type)
        {
            return type switch
            {
                DamageType.Physical => _physicalDamageColor,
                DamageType.Fire => _fireDamageColor,
                DamageType.Frost => _frostDamageColor,
                DamageType.Holy => _holyDamageColor,
                DamageType.Shadow => _shadowDamageColor,
                DamageType.Nature => _natureDamageColor,
                DamageType.Arcane => _arcaneDamageColor,
                _ => _physicalDamageColor
            };
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_legacyText == null)
                _legacyText = GetComponentInChildren<Text>();

            if (_config == null)
                _config = CombatConfigSO.Instance;

            if (_config != null)
            {
                _duration = _config.FloatingTextDuration;
                _riseHeight = _config.FloatingTextRiseHeight;
            }
        }

        private void Start()
        {
            _startPosition = transform.position;
            _elapsed = 0f;

            // Face camera
            if (Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float t = _elapsed / _duration;

            if (t >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            // Move upward
            float yOffset = _riseHeight * t;
            transform.position = _startPosition + Vector3.up * yOffset;

            // Scale animation
            float scale = _scaleCurve.Evaluate(t);
            transform.localScale = Vector3.one * scale;

            // Alpha fade
            if (_legacyText != null)
            {
                float alpha = _alphaCurve.Evaluate(t);
                Color c = _baseColor;
                c.a = alpha;
                _legacyText.color = c;
            }

            // Billboard - face camera
            if (Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }
        }

        #endregion
    }
}
