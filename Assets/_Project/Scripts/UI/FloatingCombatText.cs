using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using EtherDomes.Combat;

namespace EtherDomes.UI
{
    /// <summary>
    /// Manages floating combat text display.
    /// 
    /// Colors:
    /// - White: Normal damage
    /// - Yellow: Critical damage
    /// - Green: Healing
    /// - Red: Damage received
    /// - Gray: Miss/Dodge/Parry
    /// 
    /// Animation: Float up, fade out
    /// Scale: Larger for crits
    /// 
    /// Requirements: 6.3, 6.4
    /// </summary>
    public class FloatingCombatText : MonoBehaviour, IFloatingCombatText
    {
        [Header("Prefab")]
        [SerializeField] private GameObject _fctPrefab;

        [Header("Colors")]
        [SerializeField] private Color _normalDamageColor = Color.white;
        [SerializeField] private Color _criticalDamageColor = Color.yellow;
        [SerializeField] private Color _healingColor = Color.green;
        [SerializeField] private Color _criticalHealingColor = new Color(0.5f, 1f, 0.5f);
        [SerializeField] private Color _missColor = Color.gray;
        [SerializeField] private Color _statusColor = new Color(1f, 0.5f, 0f);

        [Header("Animation")]
        [SerializeField] private float _floatSpeed = 1f;
        [SerializeField] private float _fadeSpeed = 1f;
        [SerializeField] private float _lifetime = 1.5f;
        [SerializeField] private float _criticalScale = 1.5f;
        [SerializeField] private float _normalScale = 1f;
        [SerializeField] private Vector3 _randomOffset = new Vector3(0.5f, 0f, 0.5f);

        private readonly List<FCTInstance> _activeInstances = new();
        private Camera _mainCamera;

        public event Action<FCTData> OnFCTCreated;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            CombatEvents.OnDamageDealt += HandleDamageDealt;
            CombatEvents.OnHealingApplied += HandleHealingApplied;
            CombatEvents.OnMiss += HandleMiss;
            CombatEvents.OnDodge += HandleDodge;
        }

        private void OnDisable()
        {
            CombatEvents.OnDamageDealt -= HandleDamageDealt;
            CombatEvents.OnHealingApplied -= HandleHealingApplied;
            CombatEvents.OnMiss -= HandleMiss;
            CombatEvents.OnDodge -= HandleDodge;
        }

        private void HandleDamageDealt(Vector3 position, float damage, bool isCritical)
        {
            ShowDamage(position, damage, isCritical);
        }

        private void HandleHealingApplied(Vector3 position, float amount, bool isCritical)
        {
            ShowHealing(position, amount, isCritical);
        }

        private void HandleMiss(Vector3 position)
        {
            ShowMiss(position);
        }

        private void HandleDodge(Vector3 position)
        {
            ShowDodge(position);
        }

        private void Update()
        {
            UpdateInstances();
        }

        public void ShowDamage(Vector3 position, float damage, bool isCritical = false)
        {
            var data = new FCTData
            {
                Type = isCritical ? FCTType.CriticalDamage : FCTType.Damage,
                Value = damage,
                Text = Mathf.RoundToInt(damage).ToString(),
                WorldPosition = position,
                IsCritical = isCritical
            };

            CreateFCT(data, isCritical ? _criticalDamageColor : _normalDamageColor, isCritical);
            OnFCTCreated?.Invoke(data);
        }

        public void ShowHealing(Vector3 position, float healing, bool isCritical = false)
        {
            var data = new FCTData
            {
                Type = isCritical ? FCTType.CriticalHealing : FCTType.Healing,
                Value = healing,
                Text = "+" + Mathf.RoundToInt(healing).ToString(),
                WorldPosition = position,
                IsCritical = isCritical
            };

            CreateFCT(data, isCritical ? _criticalHealingColor : _healingColor, isCritical);
            OnFCTCreated?.Invoke(data);
        }

        public void ShowMiss(Vector3 position)
        {
            var data = new FCTData
            {
                Type = FCTType.Miss,
                Value = 0,
                Text = "Miss",
                WorldPosition = position,
                IsCritical = false
            };

            CreateFCT(data, _missColor, false);
            OnFCTCreated?.Invoke(data);
        }

        public void ShowStatus(Vector3 position, string status)
        {
            var data = new FCTData
            {
                Type = FCTType.Status,
                Value = 0,
                Text = status,
                WorldPosition = position,
                IsCritical = false
            };

            CreateFCT(data, _statusColor, false);
            OnFCTCreated?.Invoke(data);
        }

        public void ShowDodge(Vector3 position)
        {
            var data = new FCTData
            {
                Type = FCTType.Dodge,
                Value = 0,
                Text = "Dodge",
                WorldPosition = position,
                IsCritical = false
            };

            CreateFCT(data, _missColor, false);
            OnFCTCreated?.Invoke(data);
        }

        public void ShowParry(Vector3 position)
        {
            var data = new FCTData
            {
                Type = FCTType.Parry,
                Value = 0,
                Text = "Parry",
                WorldPosition = position,
                IsCritical = false
            };

            CreateFCT(data, _missColor, false);
            OnFCTCreated?.Invoke(data);
        }

        public void ShowBlock(Vector3 position, float blockedAmount = 0)
        {
            string text = blockedAmount > 0 ? $"Block ({Mathf.RoundToInt(blockedAmount)})" : "Block";
            
            var data = new FCTData
            {
                Type = FCTType.Block,
                Value = blockedAmount,
                Text = text,
                WorldPosition = position,
                IsCritical = false
            };

            CreateFCT(data, _missColor, false);
            OnFCTCreated?.Invoke(data);
        }

        private void CreateFCT(FCTData data, Color color, bool isCritical)
        {
            if (_fctPrefab == null)
            {
                UnityEngine.Debug.LogWarning("[FCT] No prefab assigned");
                return;
            }

            // Add random offset to prevent overlap
            Vector3 offset = new Vector3(
                UnityEngine.Random.Range(-_randomOffset.x, _randomOffset.x),
                UnityEngine.Random.Range(0, _randomOffset.y),
                UnityEngine.Random.Range(-_randomOffset.z, _randomOffset.z)
            );

            Vector3 spawnPosition = data.WorldPosition + offset + Vector3.up * 2f;

            var instance = Instantiate(_fctPrefab, spawnPosition, Quaternion.identity, transform);
            
            var text = instance.GetComponentInChildren<TMP_Text>();

            if (text != null)
            {
                text.text = data.Text;
                text.color = color;
            }

            float scale = isCritical ? _criticalScale : _normalScale;
            instance.transform.localScale = Vector3.one * scale;

            _activeInstances.Add(new FCTInstance
            {
                GameObject = instance,
                Text = text,
                StartPosition = spawnPosition,
                StartTime = Time.time,
                StartColor = color,
                IsCritical = isCritical
            });

            UnityEngine.Debug.Log($"[FCT] Created: {data.Text} at {data.WorldPosition}");
        }

        private void UpdateInstances()
        {
            for (int i = _activeInstances.Count - 1; i >= 0; i--)
            {
                var instance = _activeInstances[i];
                float elapsed = Time.time - instance.StartTime;
                float progress = elapsed / _lifetime;

                if (progress >= 1f)
                {
                    Destroy(instance.GameObject);
                    _activeInstances.RemoveAt(i);
                    continue;
                }

                // Float up
                if (instance.GameObject != null)
                {
                    Vector3 newPos = instance.StartPosition + Vector3.up * (_floatSpeed * elapsed);
                    instance.GameObject.transform.position = newPos;

                    // Face camera
                    if (_mainCamera != null)
                    {
                        instance.GameObject.transform.LookAt(
                            instance.GameObject.transform.position + _mainCamera.transform.forward
                        );
                    }
                }

                // Fade out
                if (instance.Text != null)
                {
                    float alpha = 1f - (progress * _fadeSpeed);
                    alpha = Mathf.Clamp01(alpha);
                    
                    Color newColor = instance.StartColor;
                    newColor.a = alpha;
                    instance.Text.color = newColor;
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var instance in _activeInstances)
            {
                if (instance.GameObject != null)
                    Destroy(instance.GameObject);
            }
            _activeInstances.Clear();
        }

        private class FCTInstance
        {
            public GameObject GameObject;
            public TMP_Text Text;
            public Vector3 StartPosition;
            public float StartTime;
            public Color StartColor;
            public bool IsCritical;
        }
    }
}
