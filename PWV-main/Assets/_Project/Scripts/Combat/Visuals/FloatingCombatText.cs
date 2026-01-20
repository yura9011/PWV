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
        [SerializeField] private Text _text;
        [SerializeField] private Canvas _canvas;

        // Public setters for dynamic prefab creation
        public void SetReferences(Text text, Canvas canvas)
        {
            _text = text;
            _canvas = canvas;
            Debug.Log("[FloatingCombatText] References set via SetReferences method");
        }

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
            Debug.Log($"[FloatingCombatText] SpawnDamage called: position={position}, amount={amount}, type={type}");
            var instance = SpawnInstance(position);
            if (instance == null) 
            {
                Debug.LogError("[FloatingCombatText] SpawnInstance returned null!");
                return;
            }

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
            Debug.Log($"[FloatingCombatText] SpawnInstance called at position: {position}");
            
            // ALWAYS use dynamic prefab for now (Resources prefab is broken)
            if (_prefab == null)
            {
                Debug.Log("[FloatingCombatText] Creating dynamic FCT prefab (skipping Resources)");
                _prefab = CreateDynamicFCTPrefab();
            }

            // Add random offset
            Vector3 offset = new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(0f, 0.3f),
                Random.Range(-0.5f, 0.5f)
            );

            Debug.Log($"[FloatingCombatText] Instantiating prefab at {position + offset}");
            var go = Instantiate(_prefab, position + offset, Quaternion.identity);
            var fct = go.GetComponent<FloatingCombatText>();
            
            if (fct == null)
            {
                Debug.LogError("[FloatingCombatText] No FloatingCombatText component found on instantiated prefab!");
            }
            else
            {
                Debug.Log("[FloatingCombatText] FloatingCombatText component found successfully");
            }
            
            return fct;
        }

        /// <summary>
        /// Creates a dynamic FCT prefab when none is found in Resources.
        /// </summary>
        private static GameObject CreateDynamicFCTPrefab()
        {
            Debug.Log("[FloatingCombatText] Creating SCREEN SPACE FCT prefab");
            
            // Create root GameObject with Canvas
            var root = new GameObject("FCTText");
            
            // Add Canvas component - SCREEN SPACE OVERLAY (always works!)
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // Very high sorting order to appear on top
            
            // Add CanvasScaler
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // Add GraphicRaycaster
            root.AddComponent<GraphicRaycaster>();
            
            // Create child GameObject for Text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(root.transform, false);
            
            // Add Text component with MAXIMUM visibility settings
            var text = textObj.AddComponent<Text>();
            text.text = "TEST";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 48; // Good size for screen space
            text.color = Color.yellow; // Bright yellow for maximum contrast
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = FontStyle.Bold; // Make it bold
            
            // Add Outline for better visibility
            var outline = textObj.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2, 2);
            
            // Configure text RectTransform for screen space
            var textRect = text.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(200, 100);
            textRect.anchoredPosition = Vector2.zero; // Will be updated in runtime
            
            // Add FloatingCombatText component
            var fct = root.AddComponent<FloatingCombatText>();
            fct.SetReferences(text, canvas);
            
            Debug.Log("[FloatingCombatText] SCREEN SPACE FCT prefab created - This WILL be visible!");
            return root;
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

            if (_text != null)
            {
                _text.text = text;
                _text.color = _baseColor;
                Debug.Log($"[FloatingCombatText] SetupDamage: '{text}' (amount: {amount}, type: {type}) at position {transform.position}");
                Debug.Log($"[FloatingCombatText] Text fontSize: {_text.fontSize}");
            }
            else
            {
                Debug.LogError("[FloatingCombatText] _text is null!");
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

            if (_text != null)
            {
                _text.text = text;
                _text.color = _baseColor;
                Debug.Log($"[FloatingCombatText] SetupHealing: '{text}' (amount: {amount})");
            }
            else
            {
                Debug.LogError("[FloatingCombatText] _text is null for healing!");
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
            if (_text == null)
                _text = GetComponentInChildren<Text>();

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

            if (_config == null)
                _config = CombatConfigSO.Instance;

            if (_config != null)
            {
                _duration = _config.FloatingTextDuration;
                _riseHeight = _config.FloatingTextRiseHeight;
            }

            // FORCE position to center of screen for visibility testing
            if (_text != null)
            {
                var rectTransform = _text.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(0, 0); // Center of screen
                Debug.Log($"[FloatingCombatText] FORCED to screen center (0,0)");
            }
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float t = _elapsed / _duration;

            if (t >= 1f)
            {
                Debug.Log($"[FloatingCombatText] Destroying after {_elapsed:F1}s");
                Destroy(gameObject);
                return;
            }

            // KEEP IT IN CENTER - don't move it around
            if (_text != null)
            {
                var rectTransform = _text.GetComponent<RectTransform>();
                // Move it up slightly over time, but keep it visible
                rectTransform.anchoredPosition = new Vector2(0, t * 100); // Move up 100 pixels over duration
            }

            // NO SCALING - keep it at normal size
            transform.localScale = Vector3.one;

            // NO ALPHA FADE - keep it fully visible
            if (_text != null)
            {
                Color c = Color.yellow; // Force bright yellow always
                c.a = 1f; // Full alpha always
                _text.color = c;
                
                Debug.Log($"[FloatingCombatText] Update: CENTERED, text='{_text.text}', t={t:F2}");
            }
        }

        #endregion
    }
}
